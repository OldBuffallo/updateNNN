using IRM.Application.Features.ImportExport;
using static IRM.Application.Features.ImportExport.ImportMapping;
using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IRM.Infrastructure.Persistence.Stores.ImportExport;

public partial class SqlImportStore
{
    public async Task<ImportResult> ExecuteImportAsync(List<ImportPreviewRow> previewRows,
        Dictionary<int, string> duplicateDecisions, int companyId, string fileName, string? username = null)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var accountId = _guard is null ? 1 : await _guard.GetRequiredAccountIdAsync();
        var result = new ImportResult { SessionId = $"IMP-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid():N}", TotalRows = previewRows.Count };
        bool Writable(ImportPreviewRow r) => r.ParsedEmployee != null && (r.Status == "new" ||
            ((r.Status == "changed" || r.Status == "duplicate") && r.ExistingEmployeeId.HasValue &&
                duplicateDecisions.GetValueOrDefault(r.ExistingEmployeeId.Value, "skip") == "update"));
        try
        {
            await _db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    var nationalities = await _db.Nationality.AsNoTracking().Where(n => n.Delete_flag == 0).ToListAsync();
                    foreach (var row in previewRows.Where(Writable))
                    {
                        try { row.ParsedEmployee!.Nationality = ResolveNationalityCode(row.ParsedEmployee.Nationality, nationalities); }
                        catch (InvalidOperationException ex) { result.Errors.Add($"Dòng {row.RowNumber}: {ex.Message}"); }
                    }
                    if (result.Errors.Count > 0) { result.ErrorCount = result.Errors.Count; return; }
                    var affected = new HashSet<int>();
                    var identities = new Dictionary<ImportPreviewRow, (int Id, int Company)>();
                    foreach (var row in previewRows)
                    {
                        if (row.Status == "error" || row.ParsedEmployee == null)
                        { result.ErrorCount++; result.Errors.Add($"Dòng {row.RowNumber}: {row.ErrorMessage}"); continue; }
                        if (!Writable(row)) { result.SkippedCount++; continue; }
                        var targetId = row.ResolvedCompanyId is > 0 ? row.ResolvedCompanyId.Value : companyId;
                        if (row.ExtendedFields.TryGetValue("__AutoCreateCompanyName", out var companyName))
                        {
                            if (row.ExtendedFields.GetValueOrDefault("__AllowCreateCompany") != "true")
                                throw new InvalidOperationException($"Dòng {row.RowNumber}: Chưa cho phép tạo công ty.");
                            var company = await _db.Companies.FirstOrDefaultAsync(c => c.Delete_flag == 0 && c.CompanyName == companyName);
                            if (company == null)
                            {
                                var fieldId = await _db.Fields.Where(f => f.Delete_flag == 0).OrderBy(f => f.IDField).Select(f => f.IDField).FirstOrDefaultAsync();
                                if (fieldId == 0) throw new InvalidOperationException("Chưa có lĩnh vực hoạt động cho công ty mới.");
                                company = new Company { CompanyName = companyName, IDField = fieldId, TrackerID = accountId, UpdateDay = DateTime.Now };
                                _db.Companies.Add(company); await _db.SaveChangesAsync();
                                _db.ImportBackups.Add(new() { ImportSessionId = result.SessionId, ActionType = "INSERT", EntityType = "Company", EntityId = company.IDCompany });
                            }
                            targetId = company.IDCompany;
                        }
                        if (!await _db.Companies.AnyAsync(c => c.IDCompany == targetId && c.Delete_flag == 0))
                        { result.ErrorCount++; result.Errors.Add($"Dòng {row.RowNumber}: Công ty không tồn tại hoặc chưa được chọn."); continue; }
                        var incoming = row.ParsedEmployee;
                        var active = await _db.Employees.Where(e => e.IDCompany == targetId && e.Passport == incoming.Passport && e.Hidden_flag == 0).ToListAsync();
                        if (active.Any(e => LegacyImportRules.SameAs13Fields(e, incoming))) { result.SkippedCount++; continue; }
                        if (active.Count > 0 && !(row.ExistingEmployeeId.HasValue && active.Any(e => e.IDEmployee == row.ExistingEmployeeId) &&
                            duplicateDecisions.GetValueOrDefault(row.ExistingEmployeeId.Value, "skip") == "update"))
                            throw new InvalidOperationException($"Dòng {row.RowNumber}: Dữ liệu đã thay đổi sau xem trước. Hãy xem trước lại.");
                        foreach (var previous in active)
                        {
                            _db.ImportBackups.Add(new() { ImportSessionId = result.SessionId, EntityType = "Employee", EntityId = previous.IDEmployee,
                                ActionType = "VERSION_HIDE", OldData = JsonSerializer.Serialize(new { previous.Hidden_flag, previous.IDCompany }) });
                            previous.Hidden_flag = 1;
                        }
                        var employee = new Employee();
                        _db.Entry(employee).CurrentValues.SetValues(incoming);
                        employee.IDEmployee = 0; employee.IDCompany = targetId; employee.IDUser = accountId;
                        employee.DateCreated = DateTime.Now; employee.Hidden_flag = 0; employee.WorkingStatus = 0;
                        employee.DateOfJoin = null; employee.DateOfLeave = null;
                        if (string.IsNullOrEmpty(employee.Note)) employee.Note = " ";
                        _db.Employees.Add(employee);
                        await _db.SaveChangesAsync();
                        var changes = await UpsertV010ExtensionsAsync(employee, row.ExtendedFields, targetId);
                        _db.ImportBackups.Add(new() { ImportSessionId = result.SessionId, ActionType = "INSERT", EntityType = "Employee", EntityId = employee.IDEmployee,
                            OldData = JsonSerializer.Serialize(new { V010Changes = changes, Versioned = active.Count > 0 }) });
                        if (active.Count > 0) result.UpdatedCount++; else result.AddedCount++;
                        affected.Add(targetId);
                        identities[row] = (employee.IDEmployee, targetId);
                    }
                    if (result.AddedCount == 0 && result.UpdatedCount == 0 && result.ErrorCount > 0) throw new InvalidOperationException(result.Errors[0]);
                    foreach (var id in affected) await RecomputeCompanyCountersAsync(id);
                    var historyCompanyId = affected.Count == 1 ? affected.Single() : companyId;
                    _db.ImportHistories.Add(new() {
                        SessionId = result.SessionId, FileName = fileName, CompanyId = historyCompanyId,
                        CompanyName = affected.Count > 1 ? $"Nhiều công ty ({affected.Count})" : (await _db.Companies.FindAsync(historyCompanyId))?.CompanyName,
                        TotalRows = result.TotalRows, AddedRows = result.AddedCount, UpdatedRows = result.UpdatedCount,
                        ErrorRows = result.ErrorCount, Status = "committed", Username = username,
                        ErrorDetails = result.Errors.Count > 0 ? JsonSerializer.Serialize(result.Errors) : null
                    });
                    await _db.SaveChangesAsync();
                    await _audit.LogAsync("IMPORT", "Employee", null, $"Import {fileName}: +{result.AddedCount}, ~{result.UpdatedCount} (session: {result.SessionId})", username);
                    await transaction.CommitAsync(); result.Success = true;
                    foreach (var entry in identities)
                    {
                        entry.Key.ParsedEmployee!.IDEmployee = entry.Value.Id;
                        entry.Key.ResolvedCompanyId = entry.Value.Company;
                    }
                }
                catch { await transaction.RollbackAsync(); throw; }
            });
        }
        catch (Exception ex)
        {
            result.Success = false; result.AddedCount = 0; result.UpdatedCount = 0;
            result.Errors.Add($"Import thất bại: {GetDetailedErrorMessage(ex)}");
        }
        return result;
    }

    private static string GetDetailedErrorMessage(Exception exception)
    {
        while (exception.InnerException != null) exception = exception.InnerException;
        return exception.Message;
    }

    public async Task RecomputeCompanyCountersAsync(int companyId)
    {
        var today = DateTime.Today;
        var permits = await _db.Employees.Where(e => e.IDCompany == companyId && e.Hidden_flag == 0 &&
            (e.DateOfLeave == null || e.DateOfLeave > today) && e.TemporaryStay >= today).Select(e => e.WorkPermit).ToListAsync();
        var company = await _db.Companies.FindAsync(companyId);
        if (company == null) return;
        company.AmountOfExemption = permits.Count(p => (int)p is 0 or 3);
        company.QuantityAvailable = permits.Count(p => (int)p is 1 or 4);
        company.QuantityNotYet = permits.Count(p => (int)p is 2 or 5);
        company.NumberOfPersonalities = permits.Count(p => (int)p == 6);
        company.TotalAmount = company.AmountOfExemption + company.QuantityAvailable + company.QuantityNotYet + company.NumberOfPersonalities;
        await _db.SaveChangesAsync();
    }
}
