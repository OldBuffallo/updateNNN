using IRM.Services;
using IRM.Application.Features.ImportExport;
using static IRM.Application.Features.ImportExport.ImportMapping;
using System.Text.Json;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Persistence.Stores.ImportExport;

/// <summary>
/// Service xử lý nghiệp vụ Import Excel vào CSDL
/// </summary>
public partial class SqlImportStore
{


    // ══════════════════════════════════════════
    // EXECUTE IMPORT
    // ══════════════════════════════════════════

    /// <summary>
    /// Thực hiện import — INSERT mới, UPDATE trùng (theo lựa chọn user), backup dữ liệu cũ
    /// </summary>
    public async Task<ImportResult> ExecuteImportAsync(
        List<ImportPreviewRow> previewRows,
        Dictionary<int, string> duplicateDecisions, // employeeId → "update" | "skip"
        int companyId,
        string fileName,
        string? username = null)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var accountId = _guard is null ? 1 : await _guard.GetRequiredAccountIdAsync();
        var sessionId = $"IMP-{DateTime.Now:yyyy}-{DateTime.Now:MMddHHmmss}";
        var result = new ImportResult { SessionId = sessionId };

        try
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    int added = 0, updated = 0, errors = 0, skipped = 0;
                    var errorDetails = new List<string>();
                    var activeCompanyIds = (await _db.Companies
                        .Where(company => company.Delete_flag == 0)
                        .Select(company => company.IDCompany)
                        .ToListAsync())
                        .ToHashSet();

                    foreach (var row in previewRows)

                    {
                        if (row.Status == "error" || row.ParsedEmployee == null)
                        {
                            errors++;
                            errorDetails.Add($"Dòng {row.RowNumber}: {row.ErrorMessage}");
                            continue;
                        }

                        var resolvedCompanyId = row.ResolvedCompanyId ?? companyId;
                        if (!activeCompanyIds.Contains(resolvedCompanyId))
                        {
                            errors++;
                            errorDetails.Add($"Dòng {row.RowNumber}: Công ty không tồn tại hoặc chưa được chọn.");
                            continue;
                        }

                        row.ParsedEmployee.IDCompany = resolvedCompanyId;

                        if (row.Status == "new")
                        {
                            // INSERT mới
                            try
                            {
                                var emp = row.ParsedEmployee;
                                emp.DateCreated = DateTime.Now;
                                emp.IDUser = accountId;
                                emp.Hidden_flag = 0;
                                emp.WorkingStatus = 0;
                                _db.Employees.Add(emp);
                                await _db.SaveChangesAsync();
                                var v010Changes = await UpsertV010ExtensionsAsync(emp, row.ExtendedFields, row.ResolvedCompanyId ?? companyId);

                                // Lưu backup record (để rollback có thể xóa)
                                _db.ImportBackups.Add(new ImportBackup
                                {
                                    ImportSessionId = sessionId,
                                    ActionType = "INSERT",
                                    EntityType = "Employee",
                                    EntityId = emp.IDEmployee,
                                    OldData = JsonSerializer.Serialize(new { V010Changes = v010Changes }),
                                    CreatedAt = DateTime.Now
                                });

                                added++;
                            }
                            catch (Exception ex)
                            {
                                errors++;
                                errorDetails.Add($"Dòng {row.RowNumber}: Insert lỗi — {GetDetailedErrorMessage(ex)}");
                                result.TotalRows = previewRows.Count;
                                result.ErrorCount = errors;
                                result.Errors = errorDetails.ToList();
                                throw;
                            }
                        }
                        else if (row.Status == "duplicate" && row.ExistingEmployeeId.HasValue)
                        {
                            var decision = duplicateDecisions.GetValueOrDefault(row.ExistingEmployeeId.Value, "skip");

                            if (decision == "update")
                            {
                                try
                                {
                                    var existing = await _db.Employees.FindAsync(row.ExistingEmployeeId.Value);
                                    if (existing != null)
                                    {
                                        // Backup dữ liệu cũ
                                        var oldDataJson = JsonSerializer.Serialize(new
                                        {
                                            existing.StaffName,
                                            existing.Gender,
                                            existing.Birthday,
                                            existing.Nationality,
                                            existing.Passport,
                                            existing.Address,
                                            existing.IDCareer,
                                            existing.WorkPermit,
                                            existing.WorkPermitNumber,
                                            existing.VisaNumber,
                                            existing.VisaSymbol,
                                            existing.TemporaryStay,
                                            existing.Note,
                                            existing.FamilyVisit,
                                            existing.FamilyVisitRelativeName,
                                            existing.FamilyVisitRelationship,
                                            existing.FamilyVisitRelativeIdCard,
                                            existing.FamilyVisitStartDate,
                                            existing.FamilyVisitEndDate,
                                            existing.FamilyVisitNote
                                        });

                                        var backupRecord = new ImportBackup
                                        {
                                            ImportSessionId = sessionId,
                                            ActionType = "UPDATE",
                                            EntityType = "Employee",
                                            EntityId = existing.IDEmployee,
                                            OldData = oldDataJson,
                                            CreatedAt = DateTime.Now
                                        };
                                        _db.ImportBackups.Add(backupRecord);

                                        // Cập nhật dữ liệu mới
                                        var newData = row.ParsedEmployee;
                                        existing.StaffName = newData.StaffName;
                                        existing.Gender = newData.Gender;
                                        existing.Birthday = newData.Birthday;
                                        existing.Nationality = newData.Nationality;
                                        existing.Address = newData.Address;
                                        existing.IDCareer = newData.IDCareer;
                                        existing.WorkPermit = newData.WorkPermit;
                                        existing.WorkPermitNumber = newData.WorkPermitNumber;
                                        existing.VisaNumber = newData.VisaNumber;
                                        existing.VisaSymbol = newData.VisaSymbol;
                                        existing.TemporaryStay = newData.TemporaryStay;
                                        if (!string.IsNullOrEmpty(newData.Note))
                                            existing.Note = newData.Note;
                                        // Thăm thân
                                        existing.FamilyVisit = newData.FamilyVisit;
                                        existing.FamilyVisitRelativeName = newData.FamilyVisitRelativeName;
                                        existing.FamilyVisitRelationship = newData.FamilyVisitRelationship;
                                        existing.FamilyVisitRelativeIdCard = newData.FamilyVisitRelativeIdCard;
                                        existing.FamilyVisitStartDate = newData.FamilyVisitStartDate;
                                        existing.FamilyVisitEndDate = newData.FamilyVisitEndDate;
                                        if (!string.IsNullOrEmpty(newData.FamilyVisitNote))
                                            existing.FamilyVisitNote = newData.FamilyVisitNote;

                                        await _db.SaveChangesAsync();
                                        var v010Changes = await UpsertV010ExtensionsAsync(existing, row.ExtendedFields, row.ResolvedCompanyId ?? companyId);
                                        using (var oldDocument = JsonDocument.Parse(backupRecord.OldData!))
                                        {
                                            var rollbackData = oldDocument.RootElement.EnumerateObject()
                                                .ToDictionary(property => property.Name, property => (object?)property.Value.Clone());
                                            rollbackData["V010Changes"] = v010Changes;
                                            backupRecord.OldData = JsonSerializer.Serialize(rollbackData);
                                        }
                                        updated++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errors++;
                                    errorDetails.Add($"Dòng {row.RowNumber}: Update lỗi — {GetDetailedErrorMessage(ex)}");
                                    result.TotalRows = previewRows.Count;
                                    result.ErrorCount = errors;
                                    result.Errors = errorDetails.ToList();
                                    throw;
                                }
                            }
                            else
                            {
                                skipped++;
                            }
                        }
                    }

                    result.TotalRows = previewRows.Count;
                    result.AddedCount = added;
                    result.UpdatedCount = updated;
                    result.ErrorCount = errors;
                    result.SkippedCount = skipped;
                    result.Errors = errorDetails;

                    if (added == 0 && updated == 0 && errors > 0)
                        throw new InvalidOperationException(errorDetails[0]);

                    // Lưu lịch sử import
                    var distinctCompanyIds = previewRows
                        .Where(r => r.ResolvedCompanyId.HasValue && r.ResolvedCompanyId > 0)
                        .Select(r => r.ResolvedCompanyId!.Value)
                        .Distinct()
                        .ToList();
                    string? historyCompanyName;
                    int historyCompanyId;
                    if (distinctCompanyIds.Count > 1)
                    {
                        historyCompanyId = 0;
                        historyCompanyName = $"Nhiều công ty ({distinctCompanyIds.Count})";
                    }
                    else if (distinctCompanyIds.Count == 1)
                    {
                        historyCompanyId = distinctCompanyIds[0];
                        var comp = await _db.Companies.FindAsync(historyCompanyId);
                        historyCompanyName = comp?.CompanyName;
                    }
                    else
                    {
                        historyCompanyId = companyId;
                        var comp = companyId > 0 ? await _db.Companies.FindAsync(companyId) : null;
                        historyCompanyName = comp?.CompanyName;
                    }
                    var history = new ImportHistory
                    {
                        SessionId = sessionId,
                        FileName = fileName,
                        CompanyId = historyCompanyId,
                        CompanyName = historyCompanyName,
                        TotalRows = previewRows.Count,
                        AddedRows = added,
                        UpdatedRows = updated,
                        ErrorRows = errors,
                        Status = "committed",
                        Username = username,
                        ImportDate = DateTime.Now,
                        ErrorDetails = errorDetails.Any() ? JsonSerializer.Serialize(errorDetails) : null
                    };
                    _db.ImportHistories.Add(history);
                    await _db.SaveChangesAsync();

                    // Ghi audit log
                    await _audit.LogAsync("IMPORT", "Employee", null,
                        $"Import {fileName}: +{added} mới, ~{updated} cập nhật, {errors} lỗi (session: {sessionId})",
                        username);

                    await transaction.CommitAsync();

                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            var detail = GetDetailedErrorMessage(ex);
            if (!result.Errors.Any(error => error.Contains(detail, StringComparison.OrdinalIgnoreCase)))
                result.Errors.Add($"Import thất bại: {detail}");
        }

        return result;
    }

    private static string GetDetailedErrorMessage(Exception exception)
    {
        var current = exception;
        while (current.InnerException is not null)
            current = current.InnerException;
        return current.Message;
    }
}
