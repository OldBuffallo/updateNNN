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
    // ROLLBACK
    // ══════════════════════════════════════════

    /// <summary>
    /// Hoàn tác import — khôi phục dữ liệu cũ, xóa records mới
    /// </summary>
    public async Task<bool> RollbackImportAsync(string sessionId)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        bool rollbackAttemptedCommit = false;
        try
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    var history = await _db.ImportHistories.FirstOrDefaultAsync(h => h.SessionId == sessionId);
                    if (history != null) await _db.Entry(history).ReloadAsync();
                    if (history?.Status == "rolledback" && rollbackAttemptedCommit) return;
                    if (history == null || history.Status == "rolledback") throw new InvalidOperationException("Phiên đã hoàn tác hoặc không tồn tại.");
                    var backups = await _db.ImportBackups.Where(b => b.ImportSessionId == sessionId).OrderByDescending(b => b.Id).ToListAsync();
                    await ValidateRollbackStateAsync(backups);
                    var affected = new HashSet<int>();
                    foreach (var backup in backups)
                    {
                        await RollbackV010ChangesAsync(backup.OldData);
                        if (backup.ActionType == "INSERT")
                        {
                            if (backup.EntityType == "Company")
                            {
                                var comp = await _db.Companies.FindAsync(backup.EntityId);
                                if (comp != null)
                                {
                                    await _db.Entry(comp).ReloadAsync();
                                    comp.Delete_flag = 1;
                                    await _db.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                // Xóa record đã insert (soft delete)
                                var emp = await _db.Employees.FindAsync(backup.EntityId);
                                if (emp != null)
                                {
                                    await _db.Entry(emp).ReloadAsync();
                                    affected.Add(emp.IDCompany);
                                    var versioned = !string.IsNullOrEmpty(backup.OldData) &&
                                        JsonSerializer.Deserialize<JsonElement>(backup.OldData).TryGetProperty("Versioned", out var version) && version.GetBoolean();
                                    if (versioned) _db.Employees.Remove(emp);
                                    else emp.Hidden_flag = 1;
                                    await _db.SaveChangesAsync();
                                }
                            }
                        }
                        else if (backup.ActionType == "VERSION_HIDE")
                        {
                            var previous = await _db.Employees.FindAsync(backup.EntityId);
                            if (previous == null) throw new InvalidOperationException("Không tìm thấy bản ghi cũ để hoàn tác.");
                            await _db.Entry(previous).ReloadAsync();
                            var saved = JsonSerializer.Deserialize<JsonElement>(backup.OldData!);
                            previous.Hidden_flag = saved.GetProperty("Hidden_flag").GetInt32();
                            affected.Add(previous.IDCompany);
                            await _db.SaveChangesAsync();
                        }
                        else if (backup.ActionType == "UPDATE" && !string.IsNullOrEmpty(backup.OldData))
                        {
                            // Khôi phục dữ liệu cũ
                            var emp = await _db.Employees.FindAsync(backup.EntityId);
                            if (emp != null)
                            {
                                affected.Add(emp.IDCompany);
                                var oldData = JsonSerializer.Deserialize<JsonElement>(backup.OldData);
                                if (oldData.TryGetProperty("StaffName", out var name))
                                    emp.StaffName = name.GetString() ?? "";
                                if (oldData.TryGetProperty("Gender", out var gender))
                                    emp.Gender = (Gender)gender.GetInt32();
                                if (oldData.TryGetProperty("Birthday", out var bday) && bday.ValueKind != JsonValueKind.Null)
                                    emp.Birthday = bday.GetDateTime();
                                if (oldData.TryGetProperty("Nationality", out var nat))
                                    emp.Nationality = nat.GetString();
                                if (oldData.TryGetProperty("Passport", out var pass))
                                    emp.Passport = pass.GetString();
                                if (oldData.TryGetProperty("Address", out var addr))
                                    emp.Address = addr.GetString();
                                if (oldData.TryGetProperty("IDCareer", out var career) && career.ValueKind != JsonValueKind.Null)
                                    emp.IDCareer = career.GetInt32();
                                if (oldData.TryGetProperty("WorkPermit", out var wp))
                                    emp.WorkPermit = (WorkPermitType)wp.GetInt32();
                                if (oldData.TryGetProperty("WorkPermitNumber", out var wpn))
                                    emp.WorkPermitNumber = wpn.GetString();
                                if (oldData.TryGetProperty("VisaNumber", out var visa))
                                    emp.VisaNumber = visa.GetString();
                                if (oldData.TryGetProperty("VisaSymbol", out var visaSym))
                                    emp.VisaSymbol = visaSym.GetString();
                                if (oldData.TryGetProperty("TemporaryStay", out var ts) && ts.ValueKind != JsonValueKind.Null)
                                    emp.TemporaryStay = ts.GetDateTime();
                                if (oldData.TryGetProperty("Note", out var note))
                                    emp.Note = note.GetString();
                                // Thăm thân rollback
                                if (oldData.TryGetProperty("FamilyVisit", out var fv))
                                    emp.FamilyVisit = fv.GetInt32();
                                if (oldData.TryGetProperty("FamilyVisitRelativeName", out var frn))
                                    emp.FamilyVisitRelativeName = frn.GetString();
                                if (oldData.TryGetProperty("FamilyVisitRelationship", out var frl))
                                    emp.FamilyVisitRelationship = frl.GetString();
                                if (oldData.TryGetProperty("FamilyVisitRelativeIdCard", out var frid))
                                    emp.FamilyVisitRelativeIdCard = frid.GetString();
                                if (oldData.TryGetProperty("FamilyVisitStartDate", out var fsd) && fsd.ValueKind != JsonValueKind.Null)
                                    emp.FamilyVisitStartDate = fsd.GetDateTime();
                                if (oldData.TryGetProperty("FamilyVisitEndDate", out var fed) && fed.ValueKind != JsonValueKind.Null)
                                    emp.FamilyVisitEndDate = fed.GetDateTime();
                                if (oldData.TryGetProperty("FamilyVisitNote", out var fnote))
                                    emp.FamilyVisitNote = fnote.GetString();

                                await _db.SaveChangesAsync();
                            }
                        }
                    }

                    foreach (var companyId in affected) await RecomputeCompanyCountersAsync(companyId);
                    history.Status = "rolledback";
                    await _db.SaveChangesAsync();

                    await _audit.LogAsync("ROLLBACK", "Import", null,
                        $"Hoàn tác import session {sessionId}", history.Username);

                    rollbackAttemptedCommit = true;
                    await transaction.CommitAsync();
                }
                catch
                {
                    try { await transaction.RollbackAsync(); } catch { }
                    _db.ChangeTracker.Clear();
                    throw;
                }
            });

            return true;
        }
        catch
        {
            _db.ChangeTracker.Clear();
            return false;
        }
    }
    private async Task ValidateRollbackStateAsync(List<ImportBackup> backups)
    {
        var insertedEmployeeIds = backups.Where(b => b.ActionType == "INSERT" && b.EntityType == "Employee").Select(b => b.EntityId).ToArray();
        foreach (var backup in backups)
        {
            await ValidateV010RollbackStateAsync(backup.OldData);
            if (backup.EntityType == "Company" && backup.ActionType == "INSERT")
            {
                if (await _db.Employees.AnyAsync(e => e.IDCompany == backup.EntityId && e.Hidden_flag == 0 && !insertedEmployeeIds.Contains(e.IDEmployee)) ||
                    await _db.Investments.AnyAsync(i => i.IDCompany == backup.EntityId))
                    throw new InvalidOperationException("Công ty đã có dữ liệu phát sinh sau phiên import.");
            }
            if (backup.EntityType != "Employee" || string.IsNullOrWhiteSpace(backup.OldData)) continue;
            using var data = JsonDocument.Parse(backup.OldData);
            var property = backup.ActionType == "VERSION_HIDE" ? "HiddenFingerprint" : "NewFingerprint";
            if (!data.RootElement.TryGetProperty(property, out var fingerprint)) continue; // Historical UPDATE backup format remains supported.
            var current = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.IDEmployee == backup.EntityId);
            if (current == null || LegacyImportRules.EmployeeFingerprint(current) != fingerprint.GetString())
                throw new InvalidOperationException("Hồ sơ đã thay đổi sau phiên import. Hãy hoàn tác các phiên sau trước.");
        }
    }
}
