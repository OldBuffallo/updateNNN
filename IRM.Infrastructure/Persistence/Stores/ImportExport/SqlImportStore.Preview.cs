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
        Stream stream, List<ColumnMapping> mappings, int companyId)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var allRows = ExcelReaderHelper.ReadAllRows(stream, mappings);
        var previewRows = new List<ImportPreviewRow>();

        // Cache dữ liệu lookup
        var nationalities = await _db.Nationality.Where(n => n.Delete_flag == 0).ToListAsync();
        var careers = await _db.Careers.Where(c => c.Delete_flag == 0).ToListAsync();
        var companies = await _db.Companies.Where(c => c.Delete_flag == 0).ToListAsync();
        var effectiveCompanyId = companyId > 0
            ? companyId
            : companies.Count == 1
                ? companies[0].IDCompany
                : 0;

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
                int resolvedCompanyId = effectiveCompanyId;
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
                    if (effectiveCompanyId > 0)
                    {
                        var fallbackCompany = companies.FirstOrDefault(c => c.IDCompany == effectiveCompanyId);
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
                else if (effectiveCompanyId > 0)
                {
                    // Không có cột CompanyName → dùng dropdown (hành vi cũ)
                    var fallbackCompany = companies.FirstOrDefault(c => c.IDCompany == effectiveCompanyId);
                    companyDisplay = fallbackCompany?.CompanyName;
                }
                else
                {
                    preview.Status = "error";
                    preview.ErrorMessage = "Chưa chọn công ty mặc định. Hãy tạo công ty và chọn công ty trước khi import.";
                    previewRows.Add(preview);
                    continue;
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
                preview.VisaSymbol = employee.VisaSymbol;
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
}
