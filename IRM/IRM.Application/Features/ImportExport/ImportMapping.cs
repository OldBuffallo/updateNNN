using IRM.Data.Models;
using System.Text.Json;

namespace IRM.Application.Features.ImportExport;

public static class ImportMapping
{
    /// <summary>Trường bắt buộc duy nhất của hồ sơ nhân viên khi import từ Excel: Số hộ chiếu.</summary>
    public static readonly IReadOnlyList<string> RequiredCoreFields =
    [
        "Passport"
    ];

    /// <summary>Trả về danh sách (Key, Label) các trường bắt buộc chưa được ghép cột.</summary>
    public static IReadOnlyList<(string Key, string Label)> GetMissingRequiredFields(IEnumerable<ColumnMapping>? mappings)
    {
        var mapped = (mappings ?? [])
            .Where(m => !string.IsNullOrWhiteSpace(m.SystemField) && m.SystemField != "skip")
            .Select(m => m.SystemField)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return RequiredCoreFields
            .Where(req => !mapped.Contains(req))
            .Select(req => (Key: req, Label: SystemFields.All.GetValueOrDefault(req, req)))
            .ToList();
    }

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
        var exactFields = SystemFields.All
            .Where(field => field.Key != "skip" && headers.Any(header =>
                string.Equals(field.Value, header.HeaderName.Trim(), StringComparison.OrdinalIgnoreCase)))
            .Select(field => field.Key)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var header in headers)
        {
            var headerLower = header.HeaderName.ToLowerInvariant().Trim();
            var match = SystemFields.AutoMapKeywords
                .Where(item => !usedFields.Contains(item.Key) && !exactFields.Contains(item.Key))
                .SelectMany(item => item.Value.Select(keyword => new { Field = item.Key, Keyword = keyword }))
                .Where(item => headerLower.Length > 0 && headerLower.Contains(item.Keyword, StringComparison.Ordinal))
                .OrderByDescending(item => headerLower == item.Keyword)
                .ThenByDescending(item => item.Keyword.Length)
                .ThenBy(item => item.Field, StringComparer.Ordinal)
                .FirstOrDefault();
            // Reserve canonical headers across the whole sheet before keyword fallback.
            // An auxiliary column must not consume a later canonical column's field.
            var exact = SystemFields.All.FirstOrDefault(item => item.Key != "skip" &&
                string.Equals(item.Value, header.HeaderName.Trim(), StringComparison.OrdinalIgnoreCase));
            var matchedField = match?.Field ?? "skip";
            if (exact.Key is not null)
                matchedField = usedFields.Contains(exact.Key) ? "skip" : exact.Key;
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
        Dictionary<string, string> rowData, int? companyId,
        List<NationalityEntity> nationalities, List<Career> careers,
        int? careerGroupId, ISet<string> investorPassports, IReadOnlyCollection<string> wards)
    {
        var parsed = ParseRowWithWarnings(rowData, companyId, nationalities, careers, careerGroupId, investorPassports, wards);
        if (parsed.Errors.Count > 0 || parsed.Employee is null)
            throw new ImportRowException(string.Join("; ", parsed.Errors));
        return parsed.Employee;
    }

    public static ParseRowResult ParseRowWithWarnings(
        Dictionary<string, string> rowData, int? companyId,
        List<NationalityEntity> nationalities, List<Career> careers,
        int? careerGroupId, ISet<string> investorPassports, IReadOnlyCollection<string> wards)
    {
        var row = rowData.GetValueOrDefault("__RowNumber", "?");
        var errors = new List<string>();
        var warnings = new List<string>();
        string Value(string key) => rowData.GetValueOrDefault(key, "").Trim();

        var passport = Value("Passport");
        if (passport.Length == 0)
            errors.Add($"Thiếu số hộ chiếu tại dòng {row}");

        var name = Value("StaffName");
        if (name.Length == 0)
            warnings.Add("Thiếu họ tên");

        var genderRaw = Value("Gender");
        Gender genderValue = Gender.Male;
        if (genderRaw.Length == 0)
        {
            warnings.Add("Thiếu giới tính (tạm để Nam)");
        }
        else
        {
            var normalizedGender = LegacyImportRules.RemoveDiacritics(genderRaw).ToUpperInvariant();
            if (normalizedGender is "NAM")
                genderValue = Gender.Male;
            else if (normalizedGender is "NU")
                genderValue = Gender.Female;
            else
            {
                genderValue = (Gender)LegacyImportRules.ParseGender(genderRaw);
                warnings.Add($"Giới tính '{genderRaw}' không theo chuẩn Nam/Nữ — tạm ghi nhận là {genderValue.ToDisplay()}");
            }
        }

        DateTime? birthday = null;
        var birthdayRaw = Value("Birthday");
        if (birthdayRaw.Length == 0)
        {
            warnings.Add("Thiếu ngày sinh");
        }
        else
        {
            try { birthday = ParseNullableImportDate(birthdayRaw); }
            catch (InvalidOperationException) { errors.Add($"Lỗi đọc ngày sinh nhật '{birthdayRaw}' tại dòng {row}"); }
        }

        string? nationalityCode = null;
        var nationalityRaw = Value("Nationality");
        if (nationalityRaw.Length == 0)
        {
            warnings.Add("Thiếu quốc tịch");
        }
        else
        {
            try { nationalityCode = ResolveNationalityCode(nationalityRaw, nationalities); }
            catch (InvalidOperationException) { errors.Add($"Quốc tịch {nationalityRaw} chưa có trong dữ liệu tại dòng {row}"); }
        }

        string? address = null;
        var addressRaw = Value("Address");
        if (addressRaw.Length == 0)
        {
            warnings.Add("Thiếu địa chỉ");
        }
        else
        {
            address = addressRaw;
            if (wards.Count > 0 && !LegacyImportRules.AddressEndsWithWard(addressRaw, wards))
                warnings.Add("Địa chỉ chưa khớp tên phường/xã trong danh mục");
        }

        var combinedPermit = Value("WorkPermit");
        var separatePermit = Value("WorkPermitNumber");
        if (combinedPermit.Length == 0 && separatePermit.Length > 0) combinedPermit = separatePermit;
        var (permit, number) = LegacyImportRules.ParseWorkPermit(combinedPermit, passport.Length > 0 && investorPassports.Contains(passport));
        if (separatePermit.Length > 0 && (permit is not (1 or 4) || number != separatePermit))
            errors.Add($"Cột GPLĐ và Số GPLĐ không thống nhất tại dòng {row}");
        if (combinedPermit.Length > 0 && permit is (1 or 4))
        {
            var lowerPermit = LegacyImportRules.RemoveDiacritics(combinedPermit).ToLowerInvariant();
            if (lowerPermit is "chua co" or "da co" or "khong" or "co" or "1" or "2")
                warnings.Add($"Cột GPLĐ '{combinedPermit}' có dạng trạng thái chữ — hệ thống đang lưu thành số GPLĐ, hãy kiểm tra lại");
        }

        int? careerId = null;
        var careerRaw = Value("Career");
        if (careerRaw.Length == 0)
        {
            warnings.Add("Thiếu nghề nghiệp");
        }
        else
        {
            if (careerGroupId is > 0)
                careerId = LegacyImportRules.FindCareerId(careerRaw, careerGroupId.Value, careers);
            if (!careerId.HasValue)
            {
                careerId = careers
                    .Where(c => c.Delete_flag == 0 && string.Equals(c.CareerName.Trim(), careerRaw, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.IDCareer)
                    .Select(c => (int?)c.IDCareer)
                    .FirstOrDefault();
                if (careerId.HasValue && careerGroupId is > 0)
                    warnings.Add($"Nghề nghiệp '{careerRaw}' thuộc nhóm nghề khác — đã tự động ánh xạ");
            }
            if (!careerId.HasValue && permit == LegacyImportRules.WorkPermitOther &&
                LegacyImportRules.RemoveDiacritics(careerRaw).Equals("THAN NHAN", StringComparison.OrdinalIgnoreCase))
            {
                careerId = careers.Where(c => c.Delete_flag == 0 && LegacyImportRules.IsRelativeCareer(c))
                    .OrderBy(c => c.IDCareer)
                    .Select(c => (int?)c.IDCareer)
                    .FirstOrDefault();
            }
            if (!careerId.HasValue)
                errors.Add($"Nghề nghiệp '{careerRaw}' chưa có trong dữ liệu tại dòng {row}");
        }

        if (careerId.HasValue && LegacyImportRules.IsRelativeCareer(careers.First(c => c.IDCareer == careerId)))
        {
            permit = 6;
            number = "";
        }
        else if (permit == 6)
        {
            var relative = careers.Where(c => c.Delete_flag == 0 && LegacyImportRules.IsRelativeCareer(c)).OrderBy(c => c.IDCareer).FirstOrDefault();
            if (relative != null) careerId = relative.IDCareer;
        }

        string? visa = null;
        var visaRaw = Value("VisaNumber");
        if (visaRaw.Length == 0) warnings.Add("Thiếu số visa");
        else visa = visaRaw;

        DateTime? stay = null;
        var stayRaw = Value("TemporaryStay");
        if (stayRaw.Length == 0)
        {
            warnings.Add("Thiếu hạn tạm trú");
        }
        else
        {
            try { stay = ParseNullableImportDate(stayRaw); }
            catch (InvalidOperationException) { errors.Add($"Lỗi đọc thời hạn tạm trú '{stayRaw}' tại dòng {row}"); }
        }

        var settlementRaw = Value("SettlementResults");
        var settlement = settlementRaw.Length > 0 && stay.HasValue && visa != null
            ? LegacyImportRules.ParseSettlement(settlementRaw, visa, stay.Value)
            : new LegacyImportRules.SettlementParse(3, settlementRaw.Length > 0 ? settlementRaw : " ", null, null);
        if (settlement.Error != null) errors.Add($"{settlement.Error} tại dòng {row}");

        DateTime? cardDate = settlement.CardCreationDate;
        var cardDateRaw = Value("CardCreationDate");
        if (cardDateRaw.Length > 0)
        {
            try
            {
                var explicitDate = ParseNullableImportDate(cardDateRaw);
                if (cardDate.HasValue && explicitDate.HasValue && cardDate.Value != explicitDate.Value)
                    errors.Add($"Ngày cấp thẻ và kết quả giải quyết là khác nhau tại dòng {row}");
                cardDate = explicitDate;
            }
            catch (InvalidOperationException) { errors.Add($"Lỗi đọc ngày cấp thẻ '{cardDateRaw}' tại dòng {row}"); }
        }

        DateTime? fvStart = null;
        if (rowData.TryGetValue("FamilyVisitStartDate", out var fsd) && !string.IsNullOrWhiteSpace(fsd))
        {
            try { fvStart = ParseNullableImportDate(fsd); }
            catch (InvalidOperationException) { errors.Add($"Lỗi đọc ngày bắt đầu thăm thân tại dòng {row}"); }
        }
        DateTime? fvEnd = null;
        if (rowData.TryGetValue("FamilyVisitEndDate", out var fed) && !string.IsNullOrWhiteSpace(fed))
        {
            try { fvEnd = ParseNullableImportDate(fed); }
            catch (InvalidOperationException) { errors.Add($"Lỗi đọc hạn thăm thân tại dòng {row}"); }
        }
        if (fvStart.HasValue && fvEnd.HasValue && fvEnd.Value < fvStart.Value)
            errors.Add($"Hạn thăm thân nhỏ hơn ngày bắt đầu tại dòng {row}");

        var familyVisitFlag = 0;
        if (rowData.TryGetValue("FamilyVisit", out var fv) && !string.IsNullOrWhiteSpace(fv))
        {
            var fvTrim = fv.Trim();
            var fvLower = fvTrim.ToLowerInvariant();
            if (fvLower is "có" or "co" or "1" or "yes" or "x")
                familyVisitFlag = 1;
            else if (fvLower is "không" or "khong" or "0" or "no")
                familyVisitFlag = 0;
            else
            {
                familyVisitFlag = 0;
                warnings.Add($"Cột Thăm thân '{fvTrim}' không theo chuẩn Có/Không — tạm ghi nhận là Không");
            }
        }

        if (errors.Count > 0)
            return new ParseRowResult(null, errors, warnings);

        var emp = new Employee
        {
            StaffName = name,
            Passport = passport,
            Gender = genderValue,
            Birthday = birthday,
            Nationality = nationalityCode,
            Address = address,
            IDCareer = careerId,
            WorkPermit = (WorkPermitType)permit,
            WorkPermitNumber = number,
            VisaNumber = visa,
            VisaSymbol = Value("VisaSymbol"),
            TemporaryStay = stay,
            Note = Value("Note"),
            SettlementResults = settlement.Result,
            SettlementResultsString = settlement.Text,
            CardCreationDate = cardDate,
            IDCompany = companyId is > 0 ? companyId : null,
            IDUser = 1,
            Hidden_flag = 0,
            WorkingStatus = 0,
            FamilyVisit = familyVisitFlag,
            FamilyVisitRelativeName = rowData.GetValueOrDefault("FamilyVisitRelativeName"),
            FamilyVisitRelationship = rowData.GetValueOrDefault("FamilyVisitRelationship"),
            FamilyVisitRelativeIdCard = rowData.GetValueOrDefault("FamilyVisitRelativeIdCard"),
            FamilyVisitStartDate = fvStart,
            FamilyVisitEndDate = fvEnd,
            FamilyVisitNote = rowData.GetValueOrDefault("FamilyVisitNote")
        };

        return new ParseRowResult(emp, errors, warnings);
    }

    public static string? ResolveNationalityCode(string? value, IReadOnlyCollection<NationalityEntity> nationalities) =>
        LegacyImportRules.ResolveNationalityCode(value, nationalities);

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
