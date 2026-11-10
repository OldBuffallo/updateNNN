using IRM.Data.Models;
using System.Text.Json;

namespace IRM.Application.Features.ImportExport;

public static class ImportMapping
{

    // ══════════════════════════════════════════
    // AUTO-MAP COLUMNS
    // ══════════════════════════════════════════

    /// <summary>
    /// Tự động ghép cột Excel → trường hệ thống dựa trên header name
    /// </summary>
    public static List<ColumnMapping> AutoMapColumns(List<ExcelColumnInfo> headers)
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

    /// <summary>
    /// Áp dụng template vào danh sách headers
    /// </summary>
    public static List<ColumnMapping> ApplyTemplate(ColumnMappingTemplate template, List<ExcelColumnInfo> headers)
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

    public static string[] SplitImportValues(string? value) => string.IsNullOrWhiteSpace(value)
        ? [] : value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static DateTime ParseImportDate(string? value, DateTime fallback) =>
        ParseNullableImportDate(value) ?? fallback.Date;

    public static DateTime? ParseNullableImportDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParse(value, out var parsed) || TryParseVietnameseDate(value, out parsed)) return parsed.Date;
        throw new InvalidOperationException($"Ngày không hợp lệ: {value}.");
    }

    public static Employee ParseRowToEmployee(
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
                "nam" or "male" or "1" or "m" => Gender.Male,
                _ => Gender.Female
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
            emp.Nationality = ResolveNationalityCode(nat, nationalities);
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
                "có gplđ" or "đã có gplđ" or "có" or "1" => WorkPermitType.WorkerHasPermit,
                "chưa có gplđ" or "chưa có" or "2" => WorkPermitType.WorkerNoPermit,
                "miễn" or "miễn gplđ" or "0" => WorkPermitType.WorkerExempt,
                "nhà đầu tư miễn" or "3" => WorkPermitType.InvestorExempt,
                "nhà đầu tư có" or "4" => WorkPermitType.InvestorHasPermit,
                "nhà đầu tư chưa" or "5" => WorkPermitType.InvestorNoPermit,
                _ => WorkPermitType.WorkerExempt
            };
        }

        if (rowData.TryGetValue("WorkPermitNumber", out var wpn))
            emp.WorkPermitNumber = wpn;

        if (rowData.TryGetValue("VisaNumber", out var visa))
            emp.VisaNumber = visa;

        if (rowData.TryGetValue("VisaSymbol", out var visaSym))
            emp.VisaSymbol = visaSym;

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

    public static string? ResolveNationalityCode(string? value, IReadOnlyCollection<NationalityEntity> nationalities)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = NormalizeNationality(value);
        var active = nationalities.Where(n => n.Delete_flag == 0).ToList();
        // Preserve the actual database key, including legacy/custom codes.
        var byCode = active.FirstOrDefault(n =>
            NormalizeNationality(n.NationalityCode).Equals(normalized, StringComparison.OrdinalIgnoreCase));
        if (byCode is not null) return byCode.NationalityCode;

        var byName = active.FirstOrDefault(n =>
            NormalizeNationality(n.NationalityName).Equals(normalized, StringComparison.OrdinalIgnoreCase));
        if (byName is not null) return byName.NationalityCode;

        throw new InvalidOperationException($"Quốc tịch \"{value.Trim()}\" không có trong danh mục đang hoạt động. Hãy dùng đúng tên hoặc mã tại Quản trị → Quốc tịch.");
    }

    private static string NormalizeNationality(string value) =>
        string.Join(" ", value.Normalize(System.Text.NormalizationForm.FormC)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Parse date dạng dd/MM/yyyy</summary>
    private static bool TryParseVietnameseDate(string input, out DateTime result)
    {
        return DateTime.TryParseExact(input.Trim(),
            new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "MM/dd/yyyy" },
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out result);
    }
}
