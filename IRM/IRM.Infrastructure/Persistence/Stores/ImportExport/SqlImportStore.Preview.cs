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
    // GENERATE PREVIEW
    // ══════════════════════════════════════════

    /// <summary>
    /// Đọc file, check trùng passport, trả về danh sách preview
    /// </summary>
    public async Task<List<ImportPreviewRow>> GeneratePreviewAsync(
        Stream stream, List<ColumnMapping> mappings, int companyId, ImportReadOptions? options = null)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        options ??= new();
        if (options.SheetIndex < 0 || options.HeaderRow < 0 || options.StartRow < 0 ||
            options.EndRow is <= 0 || (options.StartRow > 0 && options.EndRow < options.StartRow))
            throw new InvalidOperationException("Vùng dòng hoặc sheet không hợp lệ.");
        var allRows = ExcelReaderHelper.ReadAllRows(stream, mappings, options.SheetIndex, options.StartRow > 0 ? options.StartRow : options.HeaderRow > 0 ? options.HeaderRow + 1 : 0, options.EndRow);
        var previewRows = new List<ImportPreviewRow>();

        // Cache dữ liệu lookup
        var nationalities = await _db.Nationality.Where(n => n.Delete_flag == 0).ToListAsync();
        var careers = await _db.Careers.Include(c => c.CareerGroup).Where(c => c.Delete_flag == 0).ToListAsync();
        var wards = await _db.Wards.Where(w => w.Delete_flag == 0).Select(w => w.WardName).ToListAsync();
        var adminUnitCodes = (await _db.AdministrativeUnits.Where(u => !u.IsDeleted).Select(u => u.Code).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var investors = await _db.Investments.AsNoTracking().ToListAsync();
        var groups = await _db.CareerGroups.Select(g => g.IDCG).ToListAsync();
        var groupId = options.CareerGroupId > 0 ? options.CareerGroupId : groups.Count == 1 ? groups[0] : 0;
        var companies = await _db.Companies.Where(c => c.Delete_flag == 0).ToListAsync();
        int? effectiveCompanyId = companyId > 0
            ? companyId
            : companies.Count == 1
                ? companies[0].IDCompany
                : null;

        // Kiểm tra xem có cột CompanyName được map không
        var hasCompanyColumn = mappings.Any(m => m.SystemField == "CompanyName");
        var mappedFields = mappings.Select(m => m.SystemField).ToHashSet(StringComparer.OrdinalIgnoreCase);

        int rowNum = 0;
        foreach (var rowData in allRows)
        {
            rowNum = int.Parse(rowData[ExcelReaderHelper.RowNumberKey], System.Globalization.CultureInfo.InvariantCulture);
            var preview = new ImportPreviewRow { RowNumber = rowNum };

            try
            {
                // Resolve công ty theo từng dòng (cho phép null nếu chưa chọn công ty)
                int? resolvedCompanyId = effectiveCompanyId;
                string? companyDisplay = null;
                string? autoCreateCompanyName = null;

                if (hasCompanyColumn && rowData.TryGetValue("CompanyName", out var companyName)
                    && !string.IsNullOrWhiteSpace(companyName))
                {
                    var companyNameTrimmed = companyName.Trim();
                    var matchedCompany = companies.FirstOrDefault(c =>
                        c.CompanyName.Equals(companyNameTrimmed, StringComparison.OrdinalIgnoreCase));

                    if (matchedCompany != null)
                    {
                        resolvedCompanyId = matchedCompany.IDCompany;
                        companyDisplay = matchedCompany.CompanyName;
                    }
                    else if (options.AllowCreateCompanies)
                    {
                        resolvedCompanyId = null;
                        autoCreateCompanyName = companyNameTrimmed;
                        companyDisplay = $"{companyNameTrimmed} (Tạo mới khi xác nhận import)";
                    }
                    else throw new ImportRowException($"Công ty {companyNameTrimmed} chưa có; hãy bật cho phép tạo công ty hoặc chọn lại công ty tại dòng {rowNum}");
                }
                else if (effectiveCompanyId.HasValue)
                {
                    var fallbackCompany = companies.FirstOrDefault(c => c.IDCompany == effectiveCompanyId.Value);
                    companyDisplay = fallbackCompany?.CompanyName ?? "(Công ty mặc định)";
                }
                else
                {
                    resolvedCompanyId = null;
                    companyDisplay = "(Chưa gán công ty)";
                    preview.Warnings.Add("Chưa gán công ty");
                }

                preview.ResolvedCompanyId = resolvedCompanyId;
                preview.CompanyDisplay = companyDisplay;

                var investorSet = resolvedCompanyId.HasValue
                    ? investors.Where(i => i.IDCompany == resolvedCompanyId.Value && i.Passport != null)
                        .Select(i => i.Passport!.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var parsed = ParseRowWithWarnings(rowData, resolvedCompanyId, nationalities, careers, groupId > 0 ? groupId : null, investorSet, wards);
                preview.Warnings.AddRange(parsed.Warnings);

                preview.ExtendedFields = rowData
                    .Where(item => V010ImportFields.Contains(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                    .ToDictionary(item => item.Key, item => item.Value.Trim(), StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(autoCreateCompanyName))
                {
                    preview.ExtendedFields["__AutoCreateCompanyName"] = autoCreateCompanyName;
                    preview.ExtendedFields["__AllowCreateCompany"] = "true";
                }

                var (v010Errors, v010Warnings) = ValidateV010Extensions(preview.ExtendedFields, adminUnitCodes, rowNum);
                preview.Warnings.AddRange(v010Warnings);
                var allErrors = new List<string>(parsed.Errors);
                allErrors.AddRange(v010Errors);

                if (allErrors.Count > 0 || parsed.Employee is null)
                {
                    preview.Status = "error";
                    preview.ErrorMessage = string.Join("\n", allErrors);
                    preview.Passport = rowData.GetValueOrDefault("Passport", "").Trim();
                    preview.StaffName = rowData.GetValueOrDefault("StaffName", "").Trim();
                    previewRows.Add(preview);
                    continue;
                }

                var employee = parsed.Employee;
                preview.ParsedEmployee = employee;

                // Set display values
                preview.StaffName = employee.StaffName;
                preview.GenderDisplay = employee.GenderString;
                preview.BirthdayDisplay = employee.Birthday?.ToString("dd/MM/yyyy");
                preview.Passport = employee.Passport;
                preview.Address = employee.Address;
                preview.VisaNumber = employee.VisaNumber;
                preview.VisaSymbol = employee.VisaSymbol;
                preview.TemporaryStayDisplay = employee.TemporaryStay?.ToString("dd/MM/yyyy");
                preview.WorkPermitDisplay = employee.WorkPermitString;

                // Lookup nationality name
                var nat = nationalities.FirstOrDefault(n => n.NationalityCode == employee.Nationality);
                preview.NationalityDisplay = nat?.NationalityName ?? employee.Nationality;

                // Lookup career name
                var career = careers.FirstOrDefault(c => c.IDCareer == employee.IDCareer);
                preview.CareerDisplay = career?.CareerName;

                var existing = await _db.Employees.AsNoTracking()
                    .Where(e => e.Passport == employee.Passport && e.IDCompany == resolvedCompanyId && e.Hidden_flag == 0)
                    .OrderByDescending(e => e.IDEmployee).FirstOrDefaultAsync();
                if (existing != null)
                {
                    PreserveUnmappedExtensions(employee, existing, mappedFields);
                    var isChanged = !(LegacyImportRules.SameAsImportedEmployee(existing, employee) &&
                        !await HasMappedExtensionChangesAsync(existing, preview.ExtendedFields));
                    preview.Status = isChanged ? "changed" : "unchanged";
                    preview.ExistingEmployeeId = existing.IDEmployee;
                    preview.ExistingFingerprint = LegacyImportRules.EmployeeFingerprint(existing);

                    if (isChanged)
                    {
                        if (!string.IsNullOrWhiteSpace(existing.Note) && string.IsNullOrWhiteSpace(employee.Note))
                            preview.Warnings.Add("Cập nhật sẽ ghi đè trống cột Ghi chú hiện có");
                        if (!string.IsNullOrWhiteSpace(existing.SettlementResultsString) && string.IsNullOrWhiteSpace(employee.SettlementResultsString))
                            preview.Warnings.Add("Cập nhật sẽ ghi đè trống Kết quả giải quyết hiện có");
                    }
                }
                if (resolvedCompanyId.HasValue && await _db.Employees.AnyAsync(e => e.Passport == employee.Passport &&
                    e.IDCompany.HasValue && e.IDCompany != resolvedCompanyId &&
                    e.Hidden_flag == 0 && e.WorkingStatus == 0))
                    preview.Warnings.Add("Nhân viên đang làm ở một công ty khác");
            }
            catch (Exception ex)
            {
                preview.Status = "error";
                preview.ErrorMessage = ex is ImportRowException ? ex.Message : $"Lỗi parse dòng {rowNum}: {ex.Message}";
            }

            previewRows.Add(preview);
        }

        var importedPassports = allRows.Select(r => r.GetValueOrDefault("Passport", "").Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var importedCompanies = previewRows.Where(r => r.ResolvedCompanyId > 0).Select(r => r.ResolvedCompanyId!.Value).Distinct().ToList();
        if (previewRows.Count > 0 && importedCompanies.Count > 0 && await _db.Employees.AnyAsync(e => e.IDCompany.HasValue && importedCompanies.Contains(e.IDCompany.Value) && e.Hidden_flag == 0 &&
            e.TemporaryStay >= DateTime.Today && e.Passport != null && !importedPassports.Contains(e.Passport)))
            previewRows[0].Warnings.Add("Nhân viên còn hạn tạm trú nhưng không được cập nhật trong file");
        return previewRows;
    }
}
