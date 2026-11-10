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
        var history = await _db.ImportHistories
            .FirstOrDefaultAsync(h => h.SessionId == sessionId);
        if (history == null || history.Status == "rolledback") return false;

        var backups = await _db.ImportBackups
            .Where(b => b.ImportSessionId == sessionId)
            .OrderByDescending(b => b.Id)
            .ToListAsync();

        try
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    foreach (var backup in backups)
                    {
                        await RollbackV010ChangesAsync(backup.OldData);
                        if (backup.ActionType == "INSERT")
                        {
                            // Xóa record đã insert (soft delete)
                            var emp = await _db.Employees.FindAsync(backup.EntityId);
                            if (emp != null)
                            {
                                emp.Hidden_flag = 1;
                                await _db.SaveChangesAsync();
                            }
                        }
                        else if (backup.ActionType == "UPDATE" && !string.IsNullOrEmpty(backup.OldData))
                        {
                            // Khôi phục dữ liệu cũ
                            var emp = await _db.Employees.FindAsync(backup.EntityId);
                            if (emp != null)
                            {
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

                    history.Status = "rolledback";
                    await _db.SaveChangesAsync();

                    await _audit.LogAsync("ROLLBACK", "Import", null,
                        $"Hoàn tác import session {sessionId}", history.Username);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return true;
        }
        catch
        {
            return false;
        }
    }
}
