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
            var headerLower = header.HeaderName.ToLowerInvariant().Trim();
            var match = SystemFields.AutoMapKeywords
                .Where(item => !usedFields.Contains(item.Key))
                .SelectMany(item => item.Value.Select(keyword => new { Field = item.Key, Keyword = keyword }))
                .Where(item => headerLower.Length > 0 && headerLower.Contains(item.Keyword, StringComparison.Ordinal))
                .OrderByDescending(item => headerLower == item.Keyword)
                .ThenByDescending(item => item.Keyword.Length)
                .ThenBy(item => item.Field, StringComparer.Ordinal)
                .FirstOrDefault();
            // The published template uses full labels; substring matches are only a fallback
            // for other files. In particular, date labels share keywords with their parent fields.
            var exact = SystemFields.All.FirstOrDefault(item => item.Key != "skip" &&
                string.Equals(item.Value, header.HeaderName.Trim(), StringComparison.OrdinalIgnoreCase));
            var matchedField = exact.Key is not null
                ? usedFields.Contains(exact.Key) ? "skip" : exact.Key
                : match?.Field ?? "skip";
            var matchedLabel = SystemFields.All[matchedField];
            var isAuto = matchedField != "skip";
            if (isAuto) usedFields.Add(matchedField);

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
        if (DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var parsed) || LegacyImportRules.TryParseLegacyDate(value, out parsed)) return parsed.Date;
        throw new InvalidOperationException($"Ngày không hợp lệ: {value}.");
    }

    public static Employee ParseRowToEmployee(
        Dictionary<string, string> rowData, int companyId,
        List<NationalityEntity> nationalities, List<Career> careers,
        int careerGroupId, ISet<string> investorPassports, IReadOnlyCollection<string> wards)
    {
        var row = rowData.GetValueOrDefault("__RowNumber", "?");
        string Value(string key) => rowData.GetValueOrDefault(key, "").Trim();
        ImportRowException Error(string text) => new($"{text} tại dòng {row}");
        string Required(string key, string label)
        {
            var value = Value(key);
            if (value.Length == 0) throw Error($"Lỗi đọc {label}");
            return value;
        }
        var name = Required("StaffName", "tên nhân viên");
        var passport = Required("Passport", "số hộ chiếu");
        var gender = Required("Gender", "giới tính");
        if (!LegacyImportRules.TryParseLegacyDate(Value("Birthday"), out var birthday)) throw Error("Lỗi đọc ngày sinh nhật");
        var nationality = Required("Nationality", "quốc tịch");
        var address = Required("Address", "địa chỉ");
        var career = Required("Career", "tên nghề nghiệp");
        var visa = Required("VisaNumber", "số visa");
        if (!LegacyImportRules.TryParseLegacyDate(Value("TemporaryStay"), out var stay)) throw Error("Lỗi đọc thời hạn tạm trú");
        string? nationalityCode;
        try { nationalityCode = ResolveNationalityCode(nationality, nationalities); }
        catch (InvalidOperationException) { throw Error($"Quốc tịch {nationality} chưa có trong dữ liệu. Vui lòng thêm quốc tịch"); }
        if (!LegacyImportRules.AddressEndsWithWard(address, wards)) throw Error("Địa chỉ không có phường/ xã trong lưu trữ. Vui lòng kiểm tra lại");
        var careerId = LegacyImportRules.FindCareerId(career, careerGroupId, careers);
        if (!careerId.HasValue) throw Error("Nghề nghiệp chưa có trong dữ liệu. Vui lòng thêm nghề nghiệp");
        var combinedPermit = Value("WorkPermit");
        var separatePermit = Value("WorkPermitNumber");
        // A separate number is an explicit IRM extension; absence keeps WPF blank semantics.
        if (combinedPermit.Length == 0 && separatePermit.Length > 0) combinedPermit = separatePermit;
        var (permit, number) = LegacyImportRules.ParseWorkPermit(combinedPermit, investorPassports.Contains(passport));
        if (separatePermit.Length > 0 && (permit is not (1 or 4) || number != separatePermit))
            throw Error("Cột GPLĐ và Số GPLĐ không thống nhất");
        if (LegacyImportRules.IsRelativeCareer(careers.First(c => c.IDCareer == careerId))) { permit = 6; number = ""; }
        else if (permit == 6)
        {
            var relative = careers.Where(c => c.Delete_flag == 0 && LegacyImportRules.IsRelativeCareer(c)).OrderBy(c => c.IDCareer).FirstOrDefault();
            if (relative != null) careerId = relative.IDCareer;
        }
        var settlement = LegacyImportRules.ParseSettlement(Value("SettlementResults"), visa, stay);
        if (settlement.Error != null) throw Error(settlement.Error);
        DateTime? cardDate = settlement.CardCreationDate;
        if (Value("CardCreationDate").Length > 0)
        {
            if (!LegacyImportRules.TryParseLegacyDate(Value("CardCreationDate"), out var explicitDate)) throw Error("Lỗi đọc ngày cấp thẻ");
            if (cardDate.HasValue && cardDate.Value != explicitDate) throw Error("Ngày cấp thẻ và kết quả giải quyết là khác nhau");
            cardDate = explicitDate;
        }
        var emp = new Employee {
            StaffName = name, Passport = passport, Gender = (Gender)LegacyImportRules.ParseGender(gender),
            Birthday = birthday, Nationality = nationalityCode, Address = address, IDCareer = careerId,
            WorkPermit = (WorkPermitType)permit, WorkPermitNumber = number, VisaNumber = visa,
            VisaSymbol = Value("VisaSymbol"), TemporaryStay = stay, Note = Value("Note"),
            SettlementResults = settlement.Result, SettlementResultsString = settlement.Text, CardCreationDate = cardDate,
            IDCompany = companyId, IDUser = 1, Hidden_flag = 0, WorkingStatus = 0
        };
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
            try { emp.FamilyVisitStartDate = ParseNullableImportDate(fsd); }
            catch (InvalidOperationException) { throw Error("Lỗi đọc ngày bắt đầu thăm thân"); }
        }
        if (rowData.TryGetValue("FamilyVisitEndDate", out var fed) && !string.IsNullOrWhiteSpace(fed))
        {
            try { emp.FamilyVisitEndDate = ParseNullableImportDate(fed); }
            catch (InvalidOperationException) { throw Error("Lỗi đọc hạn thăm thân"); }
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
        string.Join(" ", LegacyImportRules.RemoveDiacritics(value)
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
