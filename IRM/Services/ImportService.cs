using System.Text.Json;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

/// <summary>
/// Service xử lý nghiệp vụ Import Excel vào CSDL
/// </summary>
public class ImportService
{
    private static readonly HashSet<string> V010ImportFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "StayPurposeCode", "StayValidFrom", "StayValidTo", "ElectronicIdentityNumber",
        "DocumentTypeCode", "DocumentNumber", "DocumentValidFrom", "DocumentValidTo",
        "AdministrativeUnitCode", "ResidenceAddress", "ResidenceValidFrom", "ResidenceValidTo"
    };
    private readonly IrmDbContext _db;
    private readonly AuditService _audit;
    private readonly IServiceAuthorizationGuard? _guard;

    public ImportService(IrmDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }
    public ImportService(IrmDbContext db, AuditService audit, IServiceAuthorizationGuard guard) : this(db, audit) => _guard = guard;

    // ══════════════════════════════════════════
    // AUTO-MAP COLUMNS
    // ══════════════════════════════════════════

    /// <summary>
    /// Tự động ghép cột Excel → trường hệ thống dựa trên header name
    /// </summary>
    public List<ColumnMapping> AutoMapColumns(List<ExcelColumnInfo> headers)
    {
        var mappings = new List<ColumnMapping>();
        var usedFields = new HashSet<string>();

        foreach (var header in headers)
        {
            var headerLower = header.HeaderName.ToLower().Trim();
            string matchedField = "skip";
            string matchedLabel = "— Bỏ qua —";
            bool isAuto = false;

            foreach (var (field, keywords) in SystemFields.AutoMapKeywords)
            {
                if (usedFields.Contains(field)) continue;

                foreach (var keyword in keywords)
                {
                    if (headerLower.Contains(keyword) || keyword.Contains(headerLower))
                    {
                        matchedField = field;
                        matchedLabel = SystemFields.All[field];
                        isAuto = true;
                        usedFields.Add(field);
                        break;
                    }
                }
                if (isAuto) break;
            }

            mappings.Add(new ColumnMapping
            {
                ExcelColumnIndex = header.Index,
                ExcelColumnName = header.HeaderName,
                SystemField = matchedField,
                SystemFieldLabel = matchedLabel,
                IsAutoMapped = isAuto
            });
        }

        return mappings;
    }

    // ══════════════════════════════════════════
    // GENERATE PREVIEW
    // ══════════════════════════════════════════

    /// <summary>
    /// Đọc file, check trùng passport, trả về danh sách preview
    /// </summary>
    public async Task<List<ImportPreviewRow>> GeneratePreviewAsync(
        Stream stream, List<ColumnMapping> mappings, int companyId)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var allRows = ExcelReaderHelper.ReadAllRows(stream, mappings);
        var previewRows = new List<ImportPreviewRow>();

        // Cache dữ liệu lookup
        var nationalities = await _db.Nationality.Where(n => n.Delete_flag == 0).ToListAsync();
        var careers = await _db.Careers.Where(c => c.Delete_flag == 0).ToListAsync();
        var companies = await _db.Companies.Where(c => c.Delete_flag == 0).ToListAsync();

        // Kiểm tra xem có cột CompanyName được map không
        var hasCompanyColumn = mappings.Any(m => m.SystemField == "CompanyName");

        int rowNum = 0;
        foreach (var rowData in allRows)
        {
            rowNum++;
            var preview = new ImportPreviewRow { RowNumber = rowNum };

            try
            {
                // Resolve công ty theo từng dòng
                int resolvedCompanyId = companyId;
                string? companyDisplay = null;

                if (hasCompanyColumn && rowData.TryGetValue("CompanyName", out var companyName)
                    && !string.IsNullOrWhiteSpace(companyName))
                {
                    var companyNameTrimmed = companyName.Trim();
                    var matchedCompany = companies.FirstOrDefault(c =>
                        c.CompanyName.Equals(companyNameTrimmed, StringComparison.OrdinalIgnoreCase)
                        || c.CompanyName.Contains(companyNameTrimmed, StringComparison.OrdinalIgnoreCase)
                        || companyNameTrimmed.Contains(c.CompanyName, StringComparison.OrdinalIgnoreCase));

                    if (matchedCompany != null)
                    {
                        resolvedCompanyId = matchedCompany.IDCompany;
                        companyDisplay = matchedCompany.CompanyName;
                    }
                    else
                    {
                        preview.Status = "error";
                        preview.ErrorMessage = $"Công ty \"{companyNameTrimmed}\" không tồn tại trong hệ thống";
                        preview.CompanyDisplay = $"❌ {companyNameTrimmed}";
                        previewRows.Add(preview);
                        continue;
                    }
                }
                else if (hasCompanyColumn)
                {
                    // Có cột CompanyName nhưng ô trống → dùng công ty dropdown (nếu có)
                    if (companyId > 0)
                    {
                        var fallbackCompany = companies.FirstOrDefault(c => c.IDCompany == companyId);
                        companyDisplay = fallbackCompany?.CompanyName ?? "(Công ty mặc định)";
                    }
                    else
                    {
                        preview.Status = "error";
                        preview.ErrorMessage = "Thiếu tên công ty (ô trống) và không chọn công ty mặc định";
                        previewRows.Add(preview);
                        continue;
                    }
                }
                else if (companyId > 0)
                {
                    // Không có cột CompanyName → dùng dropdown (hành vi cũ)
                    var fallbackCompany = companies.FirstOrDefault(c => c.IDCompany == companyId);
                    companyDisplay = fallbackCompany?.CompanyName;
                }

                preview.ResolvedCompanyId = resolvedCompanyId;
                preview.CompanyDisplay = companyDisplay;

                // Parse row data
                var employee = ParseRowToEmployee(rowData, resolvedCompanyId, nationalities, careers);
                preview.ParsedEmployee = employee;
                preview.ExtendedFields = rowData
                    .Where(item => V010ImportFields.Contains(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                    .ToDictionary(item => item.Key, item => item.Value.Trim(), StringComparer.OrdinalIgnoreCase);

                // Set display values
                preview.StaffName = employee.StaffName;
                preview.GenderDisplay = employee.GenderString;
                preview.BirthdayDisplay = employee.Birthday?.ToString("dd/MM/yyyy");
                preview.Passport = employee.Passport;
                preview.Address = employee.Address;
                preview.VisaNumber = employee.VisaNumber;
                preview.TemporaryStayDisplay = employee.TemporaryStay?.ToString("dd/MM/yyyy");
                preview.WorkPermitDisplay = employee.WorkPermitString;

                // Lookup nationality name
                var nat = nationalities.FirstOrDefault(n => n.NationalityCode == employee.Nationality);
                preview.NationalityDisplay = nat?.NationalityName ?? employee.Nationality;

                // Lookup career name
                var career = careers.FirstOrDefault(c => c.IDCareer == employee.IDCareer);
                preview.CareerDisplay = career?.CareerName;

                // Validate required fields
                if (string.IsNullOrWhiteSpace(employee.StaffName))
                {
                    preview.Status = "error";
                    preview.ErrorMessage = "Thiếu họ tên";
                    continue;
                }

                // Check duplicate passport
                if (!string.IsNullOrWhiteSpace(employee.Passport))
                {
                    var existing = await _db.Employees
                        .FirstOrDefaultAsync(e => e.Passport == employee.Passport
                            && e.Hidden_flag == 0);
                    if (existing != null)
                    {
                        preview.Status = "duplicate";
                        preview.ExistingEmployeeId = existing.IDEmployee;
                    }
                }
            }
            catch (Exception ex)
            {
                preview.Status = "error";
                preview.ErrorMessage = $"Lỗi parse dòng {rowNum}: {ex.Message}";
            }

            previewRows.Add(preview);
        }

        return previewRows;
    }


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

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            int added = 0, updated = 0, errors = 0, skipped = 0;
            var errorDetails = new List<string>();

            foreach (var row in previewRows)
            {
                if (row.Status == "error" || row.ParsedEmployee == null)
                {
                    errors++;
                    errorDetails.Add($"Dòng {row.RowNumber}: {row.ErrorMessage}");
                    continue;
                }

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
                            EmployeeId = emp.IDEmployee,
                            OldData = JsonSerializer.Serialize(new { V010Changes = v010Changes }),
                            CreatedAt = DateTime.Now
                        });

                        added++;
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        errorDetails.Add($"Dòng {row.RowNumber}: Insert lỗi — {ex.Message}");
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
                                    EmployeeId = existing.IDEmployee,
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
                            errorDetails.Add($"Dòng {row.RowNumber}: Update lỗi — {ex.Message}");
                        }
                    }
                    else
                    {
                        skipped++;
                    }
                }
            }

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

            await transaction.CommitAsync();

            // Ghi audit log
            await _audit.LogAsync("IMPORT", "Employee", null,
                $"Import {fileName}: +{added} mới, ~{updated} cập nhật, {errors} lỗi (session: {sessionId})",
                username);

            result.TotalRows = previewRows.Count;
            result.AddedCount = added;
            result.UpdatedCount = updated;
            result.ErrorCount = errors;
            result.SkippedCount = skipped;
            result.Errors = errorDetails;
            result.Success = true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Success = false;
            result.Errors.Add($"Import thất bại: {ex.Message}");
        }

        return result;
    }

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

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var backup in backups)
            {
                await RollbackV010ChangesAsync(backup.OldData);
                if (backup.ActionType == "INSERT")
                {
                    // Xóa record đã insert (soft delete)
                    var emp = await _db.Employees.FindAsync(backup.EmployeeId);
                    if (emp != null)
                    {
                        emp.Hidden_flag = 1;
                        await _db.SaveChangesAsync();
                    }
                }
                else if (backup.ActionType == "UPDATE" && !string.IsNullOrEmpty(backup.OldData))
                {
                    // Khôi phục dữ liệu cũ
                    var emp = await _db.Employees.FindAsync(backup.EmployeeId);
                    if (emp != null)
                    {
                        var oldData = JsonSerializer.Deserialize<JsonElement>(backup.OldData);
                        if (oldData.TryGetProperty("StaffName", out var name))
                            emp.StaffName = name.GetString() ?? "";
                        if (oldData.TryGetProperty("Gender", out var gender))
                            emp.Gender = gender.GetInt32();
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
                            emp.WorkPermit = wp.GetInt32();
                        if (oldData.TryGetProperty("WorkPermitNumber", out var wpn))
                            emp.WorkPermitNumber = wpn.GetString();
                        if (oldData.TryGetProperty("VisaNumber", out var visa))
                            emp.VisaNumber = visa.GetString();
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
            await transaction.CommitAsync();

            await _audit.LogAsync("ROLLBACK", "Import", null,
                $"Hoàn tác import session {sessionId}", history.Username);

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    // ══════════════════════════════════════════
    // HISTORY
    // ══════════════════════════════════════════

    /// <summary>
    /// Lấy danh sách phiên import
    /// </summary>
    public async Task<List<ImportSessionInfo>> GetHistoryAsync()
    {
        return await _db.ImportHistories
            .OrderByDescending(h => h.ImportDate)
            .Take(50)
            .Select(h => new ImportSessionInfo
            {
                Id = h.Id,
                SessionId = h.SessionId,
                Time = h.ImportDate.ToString("dd/MM/yyyy HH:mm"),
                FileName = h.FileName,
                Company = h.CompanyName ?? "",
                Added = h.AddedRows,
                Updated = h.UpdatedRows,
                Errors = h.ErrorRows,
                Status = h.Status,
                User = h.Username
            })
            .ToListAsync();
    }

    // ══════════════════════════════════════════
    // TEMPLATE
    // ══════════════════════════════════════════

    /// <summary>
    /// Lưu template column mapping
    /// </summary>
    public async Task SaveTemplateAsync(string name, int? companyId, List<ColumnMapping> mappings, string? username)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var json = JsonSerializer.Serialize(mappings.Select(m => new { m.ExcelColumnIndex, m.SystemField }));

        var existing = await _db.ColumnMappingTemplates
            .FirstOrDefaultAsync(t => t.TemplateName == name && t.CompanyId == companyId);

        if (existing != null)
        {
            existing.MappingJson = json;
            existing.UpdatedAt = DateTime.Now;
        }
        else
        {
            _db.ColumnMappingTemplates.Add(new ColumnMappingTemplate
            {
                TemplateName = name,
                CompanyId = companyId,
                MappingJson = json,
                CreatedBy = username,
                CreatedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Lấy danh sách template
    /// </summary>
    public async Task<List<ColumnMappingTemplate>> GetTemplatesAsync(int? companyId = null)
    {
        var query = _db.ColumnMappingTemplates.AsQueryable();
        if (companyId.HasValue)
            query = query.Where(t => t.CompanyId == companyId || t.CompanyId == null);
        return await query.OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Áp dụng template vào danh sách headers
    /// </summary>
    public List<ColumnMapping> ApplyTemplate(ColumnMappingTemplate template, List<ExcelColumnInfo> headers)
    {
        var savedMappings = JsonSerializer.Deserialize<List<JsonElement>>(template.MappingJson);
        var mappings = AutoMapColumns(headers); // Start with auto-map

        if (savedMappings != null)
        {
            foreach (var saved in savedMappings)
            {
                var idx = saved.GetProperty("ExcelColumnIndex").GetInt32();
                var field = saved.GetProperty("SystemField").GetString() ?? "skip";
                var existing = mappings.FirstOrDefault(m => m.ExcelColumnIndex == idx);
                if (existing != null && SystemFields.All.ContainsKey(field))
                {
                    existing.SystemField = field;
                    existing.SystemFieldLabel = SystemFields.All[field];
                    existing.IsAutoMapped = false;
                }
            }
        }

        return mappings;
    }

    // ══════════════════════════════════════════
    // PRIVATE HELPERS
    // ══════════════════════════════════════════

    private async Task<V010ImportChanges> UpsertV010ExtensionsAsync(Employee employee, Dictionary<string, string> fields, int companyId)
    {
        var changes = new V010ImportChanges();
        if (fields.Count == 0) return changes;

        var sourceId = employee.IDEmployee.ToString();
        var link = await _db.ForeignPersonSourceLinks
            .Include(item => item.ForeignPerson)
            .SingleOrDefaultAsync(item => item.SourceType == "EMPLOYEE" && item.SourceId == sourceId);
        var person = link?.ForeignPerson;
        var createdSourceLink = false;
        var passport = FamilyVisitorService.NormalizePassport(employee.Passport);
        var passportKey = FamilyVisitorService.SearchKey(passport);
        if (person is null && passportKey is not null)
            person = await _db.ForeignPersons.SingleOrDefaultAsync(item => item.PassportSearchKey == passportKey && !item.IsDeleted);
        if (person is null)
        {
            person = new ForeignPerson
            {
                FullName = employee.StaffName.Trim(), Gender = employee.Gender, Birthday = employee.Birthday,
                NationalityCode = employee.Nationality, PassportNumber = passport, PassportSearchKey = passportKey,
                IsDataIncomplete = passportKey is null
            };
            _db.ForeignPersons.Add(person);
            changes.CreatedForeignPerson = true;
        }
        if (link is null)
        {
            link = new ForeignPersonSourceLink
            {
                ForeignPerson = person, SourceType = "EMPLOYEE", SourceId = sourceId,
                SourceFingerprint = $"IMPORT:{employee.DateCreated:O}"
            };
            _db.ForeignPersonSourceLinks.Add(link);
            createdSourceLink = true;
        }
        await _db.SaveChangesAsync();
        changes.ForeignPersonId = person.Id;
        if (createdSourceLink) changes.CreatedSourceLinkId = link.Id;

        if (fields.TryGetValue("StayPurposeCode", out var purposeValue))
        {
            var purpose = purposeValue.Trim().ToUpperInvariant();
            if (!StayPurposeCodes.All.Contains(purpose)) throw new InvalidOperationException($"Diện cư trú không hợp lệ: {purposeValue}.");
            var validFrom = ParseImportDate(fields.GetValueOrDefault("StayValidFrom"), employee.DateCreated ?? DateTime.Today);
            var validTo = ParseNullableImportDate(fields.GetValueOrDefault("StayValidTo"));
            if (validTo.HasValue && validTo < validFrom) throw new InvalidOperationException("Khoảng hiệu lực diện cư trú không hợp lệ.");
            var overlap = await _db.StayCases.FirstOrDefaultAsync(item => item.ForeignPersonId == person.Id && item.IsPrimary
                && item.StatusCode != "DELETED" && item.ValidFrom <= (validTo ?? DateTime.MaxValue)
                && (!item.ValidTo.HasValue || item.ValidTo.Value >= validFrom));
            if (overlap is null)
            {
                var stay = new StayCase { ForeignPersonId = person.Id, PurposeCode = purpose, IsPrimary = true,
                    SponsorCompanyId = purpose == StayPurposeCodes.Work ? companyId : null, ValidFrom = validFrom, ValidTo = validTo };
                _db.StayCases.Add(stay); changes.CreatedStayCases.Add(stay);
            }
            else if (overlap.PurposeCode != purpose)
                throw new InvalidOperationException("Diện cư trú import chồng lấn một diện chính khác.");
        }

        if (fields.TryGetValue("ElectronicIdentityNumber", out var identityNumber))
        {
            var identityKey = FamilyVisitorService.SearchKey(identityNumber)!;
            if (!await _db.ElectronicIdentities.AnyAsync(item => item.ForeignPersonId == person.Id && item.IdentitySearchKey == identityKey))
            {
                var identity = new ElectronicIdentity { ForeignPersonId = person.Id,
                    IdentityNumber = identityNumber.Trim(), IdentitySearchKey = identityKey,
                    ValidFrom = ParseImportDate(fields.GetValueOrDefault("StayValidFrom"), employee.DateCreated ?? DateTime.Today) };
                _db.ElectronicIdentities.Add(identity); changes.CreatedIdentities.Add(identity);
            }
        }

        if (fields.TryGetValue("DocumentNumber", out var documentValue))
        {
            var numbers = SplitImportValues(documentValue);
            var types = SplitImportValues(fields.GetValueOrDefault("DocumentTypeCode"));
            var validToValues = SplitImportValues(fields.GetValueOrDefault("DocumentValidTo"));
            var validFrom = ParseImportDate(fields.GetValueOrDefault("DocumentValidFrom"), employee.DateCreated ?? DateTime.Today);
            for (var index = 0; index < numbers.Length; index++)
            {
                var type = (index < types.Length ? types[index] : ImmigrationDocumentTypeCodes.Visa).ToUpperInvariant();
                var allowedTypes = new[] { ImmigrationDocumentTypeCodes.Visa, ImmigrationDocumentTypeCodes.VisaExtension,
                    ImmigrationDocumentTypeCodes.TemporaryResidenceCard, ImmigrationDocumentTypeCodes.VisaExemption, ImmigrationDocumentTypeCodes.Other };
                if (!allowedTypes.Contains(type)) throw new InvalidOperationException($"Loại giấy tờ không hợp lệ: {type}.");
                var number = numbers[index];
                if (await _db.ImmigrationDocuments.AnyAsync(item => item.ForeignPersonId == person.Id && item.TypeCode == type && item.Number == number)) continue;
                var document = new ImmigrationDocument { ForeignPersonId = person.Id, TypeCode = type,
                    Number = number, ValidFrom = validFrom,
                    ValidTo = index < validToValues.Length ? ParseNullableImportDate(validToValues[index]) : null };
                _db.ImmigrationDocuments.Add(document); changes.CreatedDocuments.Add(document);
            }
        }

        if (fields.TryGetValue("AdministrativeUnitCode", out var unitCode)
            && fields.TryGetValue("ResidenceAddress", out var residenceAddress))
        {
            var unit = await _db.AdministrativeUnits.SingleOrDefaultAsync(item => item.Code == unitCode && !item.IsDeleted)
                ?? throw new InvalidOperationException($"Không tìm thấy mã địa bàn: {unitCode}.");
            var residenceFrom = ParseImportDate(fields.GetValueOrDefault("ResidenceValidFrom"), employee.DateCreated ?? DateTime.Today);
            var residenceTo = ParseNullableImportDate(fields.GetValueOrDefault("ResidenceValidTo"));
            if (residenceTo.HasValue && residenceTo < residenceFrom) throw new InvalidOperationException("Khoảng lịch sử lưu trú không hợp lệ.");
            if (!await _db.ResidencePeriods.AnyAsync(item => item.ForeignPersonId == person.Id
                && item.AdministrativeUnitId == unit.Id && item.AddressLine == residenceAddress && item.ValidFrom == residenceFrom))
            {
                var residence = new ResidencePeriod { ForeignPersonId = person.Id, AdministrativeUnitId = unit.Id,
                    AddressLine = residenceAddress, ResponsibleCompanyId = companyId, ValidFrom = residenceFrom, ValidTo = residenceTo };
                _db.ResidencePeriods.Add(residence); changes.CreatedResidences.Add(residence);
            }
        }

        await _db.SaveChangesAsync();
        changes.CreatedStayCaseIds = changes.CreatedStayCases.Select(item => item.Id).ToList();
        changes.CreatedDocumentIds = changes.CreatedDocuments.Select(item => item.Id).ToList();
        changes.CreatedIdentityIds = changes.CreatedIdentities.Select(item => item.Id).ToList();
        changes.CreatedResidenceIds = changes.CreatedResidences.Select(item => item.Id).ToList();
        return changes;
    }

    private async Task RollbackV010ChangesAsync(string? oldData)
    {
        if (string.IsNullOrWhiteSpace(oldData)) return;
        using var document = JsonDocument.Parse(oldData);
        if (!document.RootElement.TryGetProperty("V010Changes", out var element)) return;
        var changes = element.Deserialize<V010ImportChanges>();
        if (changes is null) return;
        _db.StayCases.RemoveRange(await _db.StayCases.Where(item => changes.CreatedStayCaseIds.Contains(item.Id)).ToListAsync());
        _db.ImmigrationDocuments.RemoveRange(await _db.ImmigrationDocuments.Where(item => changes.CreatedDocumentIds.Contains(item.Id)).ToListAsync());
        _db.ElectronicIdentities.RemoveRange(await _db.ElectronicIdentities.Where(item => changes.CreatedIdentityIds.Contains(item.Id)).ToListAsync());
        _db.ResidencePeriods.RemoveRange(await _db.ResidencePeriods.Where(item => changes.CreatedResidenceIds.Contains(item.Id)).ToListAsync());
        if (changes.CreatedSourceLinkId.HasValue)
        {
            var sourceLink = await _db.ForeignPersonSourceLinks.FindAsync(changes.CreatedSourceLinkId.Value);
            if (sourceLink is not null) _db.ForeignPersonSourceLinks.Remove(sourceLink);
        }
        if (changes.CreatedForeignPerson && changes.ForeignPersonId.HasValue)
        {
            var createdLinkId = changes.CreatedSourceLinkId ?? -1;
            var hasAnotherSource = await _db.ForeignPersonSourceLinks.AnyAsync(item => item.ForeignPersonId == changes.ForeignPersonId
                && item.Id != createdLinkId);
            var hasInspection = await _db.InspectionSubjects.AnyAsync(item => item.ForeignPersonId == changes.ForeignPersonId);
            if (!hasAnotherSource && !hasInspection)
            {
                var person = await _db.ForeignPersons.FindAsync(changes.ForeignPersonId.Value);
                if (person is not null) _db.ForeignPersons.Remove(person);
            }
        }
        await _db.SaveChangesAsync();
    }

    private sealed class V010ImportChanges
    {
        public V010ImportChanges() { }
        public int? ForeignPersonId { get; set; }
        public bool CreatedForeignPerson { get; set; }
        public int? CreatedSourceLinkId { get; set; }
        public List<int> CreatedStayCaseIds { get; set; } = [];
        public List<int> CreatedDocumentIds { get; set; } = [];
        public List<int> CreatedIdentityIds { get; set; } = [];
        public List<int> CreatedResidenceIds { get; set; } = [];
        [System.Text.Json.Serialization.JsonIgnore] public List<StayCase> CreatedStayCases { get; } = [];
        [System.Text.Json.Serialization.JsonIgnore] public List<ImmigrationDocument> CreatedDocuments { get; } = [];
        [System.Text.Json.Serialization.JsonIgnore] public List<ElectronicIdentity> CreatedIdentities { get; } = [];
        [System.Text.Json.Serialization.JsonIgnore] public List<ResidencePeriod> CreatedResidences { get; } = [];
    }

    private static string[] SplitImportValues(string? value) => string.IsNullOrWhiteSpace(value)
        ? [] : value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private DateTime ParseImportDate(string? value, DateTime fallback) =>
        ParseNullableImportDate(value) ?? fallback.Date;

    private DateTime? ParseNullableImportDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParse(value, out var parsed) || TryParseVietnameseDate(value, out parsed)) return parsed.Date;
        throw new InvalidOperationException($"Ngày không hợp lệ: {value}.");
    }

    private Employee ParseRowToEmployee(
        Dictionary<string, string> rowData,
        int companyId,
        List<NationalityEntity> nationalities,
        List<Career> careers)
    {
        var emp = new Employee
        {
            IDCompany = companyId,
            IDUser = 1,
            Hidden_flag = 0,
            WorkingStatus = 0
        };

        if (rowData.TryGetValue("StaffName", out var name))
            emp.StaffName = name;

        if (rowData.TryGetValue("Gender", out var gender))
        {
            emp.Gender = gender.ToLower() switch
            {
                "nam" or "male" or "1" or "m" => 1,
                _ => 0
            };
        }

        if (rowData.TryGetValue("Birthday", out var bday) && !string.IsNullOrWhiteSpace(bday))
        {
            if (DateTime.TryParse(bday, out var dt))
                emp.Birthday = dt;
            else if (TryParseVietnameseDate(bday, out dt))
                emp.Birthday = dt;
        }

        if (rowData.TryGetValue("Nationality", out var nat) && !string.IsNullOrWhiteSpace(nat))
        {
            // Try match by name first, then code
            var match = nationalities.FirstOrDefault(n =>
                n.NationalityName.Equals(nat, StringComparison.OrdinalIgnoreCase) ||
                n.NationalityCode.Equals(nat, StringComparison.OrdinalIgnoreCase));
            emp.Nationality = match?.NationalityCode ?? nat;
        }

        if (rowData.TryGetValue("Passport", out var passport))
            emp.Passport = passport;

        if (rowData.TryGetValue("Address", out var addr))
            emp.Address = addr;

        if (rowData.TryGetValue("Career", out var career) && !string.IsNullOrWhiteSpace(career))
        {
            var match = careers.FirstOrDefault(c =>
                c.CareerName.Equals(career, StringComparison.OrdinalIgnoreCase));
            emp.IDCareer = match?.IDCareer;
        }

        if (rowData.TryGetValue("WorkPermit", out var wp) && !string.IsNullOrWhiteSpace(wp))
        {
            emp.WorkPermit = wp.ToLower() switch
            {
                "có gplđ" or "đã có gplđ" or "có" or "1" => 1,
                "chưa có gplđ" or "chưa có" or "2" => 2,
                "miễn" or "miễn gplđ" or "0" => 0,
                "nhà đầu tư miễn" or "3" => 3,
                "nhà đầu tư có" or "4" => 4,
                "nhà đầu tư chưa" or "5" => 5,
                _ => 0
            };
        }

        if (rowData.TryGetValue("WorkPermitNumber", out var wpn))
            emp.WorkPermitNumber = wpn;

        if (rowData.TryGetValue("VisaNumber", out var visa))
            emp.VisaNumber = visa;

        if (rowData.TryGetValue("TemporaryStay", out var ts) && !string.IsNullOrWhiteSpace(ts))
        {
            if (DateTime.TryParse(ts, out var dt))
                emp.TemporaryStay = dt;
            else if (TryParseVietnameseDate(ts, out dt))
                emp.TemporaryStay = dt;
        }

        if (rowData.TryGetValue("Note", out var note))
            emp.Note = note;

        // Thăm thân
        if (rowData.TryGetValue("FamilyVisit", out var fv))
            emp.FamilyVisit = fv.ToLower() switch { "có" or "co" or "1" or "yes" or "x" => 1, _ => 0 };
        if (rowData.TryGetValue("FamilyVisitRelativeName", out var frn))
            emp.FamilyVisitRelativeName = frn;
        if (rowData.TryGetValue("FamilyVisitRelationship", out var frl))
            emp.FamilyVisitRelationship = frl;
        if (rowData.TryGetValue("FamilyVisitRelativeIdCard", out var frid))
            emp.FamilyVisitRelativeIdCard = frid;
        if (rowData.TryGetValue("FamilyVisitStartDate", out var fsd) && !string.IsNullOrWhiteSpace(fsd))
        {
            if (DateTime.TryParse(fsd, out var dt)) emp.FamilyVisitStartDate = dt;
            else if (TryParseVietnameseDate(fsd, out dt)) emp.FamilyVisitStartDate = dt;
        }
        if (rowData.TryGetValue("FamilyVisitEndDate", out var fed) && !string.IsNullOrWhiteSpace(fed))
        {
            if (DateTime.TryParse(fed, out var dt)) emp.FamilyVisitEndDate = dt;
            else if (TryParseVietnameseDate(fed, out dt)) emp.FamilyVisitEndDate = dt;
        }
        if (rowData.TryGetValue("FamilyVisitNote", out var fnote))
            emp.FamilyVisitNote = fnote;

        return emp;
    }

    /// <summary>Parse date dạng dd/MM/yyyy</summary>
    private bool TryParseVietnameseDate(string input, out DateTime result)
    {
        return DateTime.TryParseExact(input.Trim(),
            new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "MM/dd/yyyy" },
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out result);
    }
}
