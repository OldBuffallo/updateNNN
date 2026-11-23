using System.Globalization;
using System.Text.RegularExpressions;
using IRM.Application.Models;
using IRM.Core.Rules;
using IRM.Data.Models;

namespace IRM.Application.Features.ImportExport;

/// <summary>Chuẩn hóa xác định (deterministic) từng ô Excel về đúng chuẩn IRM trước khi xem trước & import.</summary>
public static partial class ExcelValueNormalizer
{
    public static readonly HashSet<string> DateFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Birthday", "TemporaryStay", "CardCreationDate",
        "FamilyVisitStartDate", "FamilyVisitEndDate",
        "StayValidFrom", "StayValidTo",
        "DocumentValidFrom", "DocumentValidTo",
        "ResidenceValidFrom", "ResidenceValidTo"
    };

    private static readonly Dictionary<string, string> EnglishNationalityAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["korea"] = "KOR", ["south korea"] = "KOR", ["republic of korea"] = "KOR", ["korean"] = "KOR",
        ["japan"] = "JPN", ["japanese"] = "JPN",
        ["china"] = "CHN", ["chinese"] = "CHN", ["prc"] = "CHN",
        ["taiwan"] = "TWN", ["taiwanese"] = "TWN",
        ["usa"] = "USA", ["us"] = "USA", ["united states"] = "USA", ["united states of america"] = "USA", ["america"] = "USA", ["american"] = "USA",
        ["uk"] = "GBR", ["united kingdom"] = "GBR", ["britain"] = "GBR", ["great britain"] = "GBR", ["england"] = "GBR", ["british"] = "GBR",
        ["india"] = "IND", ["indian"] = "IND",
        ["australia"] = "AUS", ["australian"] = "AUS",
        ["france"] = "FRA", ["french"] = "FRA",
        ["germany"] = "DEU", ["german"] = "DEU",
        ["thailand"] = "THA", ["thai"] = "THA",
        ["singapore"] = "SGP", ["singaporean"] = "SGP",
        ["malaysia"] = "MYS", ["malaysian"] = "MYS",
        ["philippines"] = "PHL", ["filipino"] = "PHL",
        ["indonesia"] = "IDN", ["indonesian"] = "IDN",
        ["russia"] = "RUS", ["russian"] = "RUS",
        ["canada"] = "CAN", ["canadian"] = "CAN",
        ["vietnam"] = "VNM", ["viet nam"] = "VNM", ["vietnamese"] = "VNM",
        ["laos"] = "LAO", ["cambodia"] = "KHM", ["myanmar"] = "MMR",
        ["italy"] = "ITA", ["spain"] = "ESP", ["netherlands"] = "NLD", ["switzerland"] = "CHE",
        ["sweden"] = "SWE", ["poland"] = "POL", ["ukraine"] = "UKR", ["brazil"] = "BRA",
        ["new zealand"] = "NZL", ["south africa"] = "ZAF", ["belgium"] = "BEL", ["denmark"] = "DNK"
    };

    private static readonly HashSet<string> AllowedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ImmigrationDocumentTypeCodes.Visa,
        ImmigrationDocumentTypeCodes.VisaExtension,
        ImmigrationDocumentTypeCodes.TemporaryResidenceCard,
        ImmigrationDocumentTypeCodes.VisaExemption,
        ImmigrationDocumentTypeCodes.Other
    };

    public static string ToExcelColumnName(int zeroBasedIndex)
    {
        if (zeroBasedIndex < 0)
        {
            return "";
        }

        var index = zeroBasedIndex;
        var chars = new Stack<char>();
        while (index >= 0)
        {
            chars.Push((char)('A' + (index % 26)));
            index = (index / 26) - 1;
        }

        return new string(chars.ToArray());
    }

    public static NormalizedCellResult Normalize(
        string systemField,
        string? rawValue,
        IReadOnlyList<NationalityEntityDto> nationalities,
        IReadOnlySet<string>? activeAdminUnitCodes = null)
    {
        var original = rawValue ?? "";
        var trimmed = original.Trim();
        if (trimmed.Length == 0)
            return new NormalizedCellResult("", NormalizedCellStatus.Empty);

        if (systemField.Equals("Passport", StringComparison.OrdinalIgnoreCase))
            return NormalizePassport(original, trimmed);

        if (systemField.Equals("Gender", StringComparison.OrdinalIgnoreCase))
            return NormalizeGender(original, trimmed);

        if (DateFields.Contains(systemField))
            return NormalizeDate(original, trimmed);

        if (systemField.Equals("Nationality", StringComparison.OrdinalIgnoreCase))
            return NormalizeNationality(original, trimmed, nationalities);

        if (systemField.Equals("WorkPermit", StringComparison.OrdinalIgnoreCase))
            return NormalizeWorkPermit(original, trimmed);

        if (systemField.Equals("FamilyVisit", StringComparison.OrdinalIgnoreCase))
            return NormalizeFamilyVisit(original, trimmed);

        if (systemField.Equals("StayPurposeCode", StringComparison.OrdinalIgnoreCase))
            return NormalizeStayPurpose(original, trimmed);

        if (systemField.Equals("DocumentTypeCode", StringComparison.OrdinalIgnoreCase))
            return NormalizeDocumentType(original, trimmed);

        if (systemField.Equals("AdministrativeUnitCode", StringComparison.OrdinalIgnoreCase))
            return NormalizeAdminUnit(original, trimmed, activeAdminUnitCodes);

        return NormalizeText(original, trimmed);
    }

    private static NormalizedCellResult NormalizePassport(string original, string trimmed)
    {
        var cleaned = WhitespaceRegex().Replace(trimmed, "");
        if (!cleaned.Any(char.IsLetterOrDigit))
            return new NormalizedCellResult(trimmed, NormalizedCellStatus.NeedsReview, $"Số hộ chiếu không hợp lệ: \"{trimmed}\"");

        return cleaned != original
            ? new NormalizedCellResult(cleaned, NormalizedCellStatus.AutoNormalized, $"Xóa khoảng trắng (\"{original}\" → \"{cleaned}\")")
            : new NormalizedCellResult(cleaned, NormalizedCellStatus.Valid);
    }

    private static NormalizedCellResult NormalizeGender(string original, string trimmed)
    {
        var key = LegacyImportRules.RemoveDiacritics(trimmed).ToLowerInvariant();
        if (key is "nam" or "male" or "m" or "1")
        {
            return trimmed == "Nam" && original == "Nam"
                ? new NormalizedCellResult("Nam", NormalizedCellStatus.Valid)
                : new NormalizedCellResult("Nam", NormalizedCellStatus.AutoNormalized, $"Chuẩn hóa giới tính (\"{original}\" → \"Nam\")");
        }
        if (key is "nu" or "female" or "f" or "0" or "2")
        {
            return trimmed == "Nữ" && original == "Nữ"
                ? new NormalizedCellResult("Nữ", NormalizedCellStatus.Valid)
                : new NormalizedCellResult("Nữ", NormalizedCellStatus.AutoNormalized, $"Chuẩn hóa giới tính (\"{original}\" → \"Nữ\")");
        }
        return new NormalizedCellResult(trimmed, NormalizedCellStatus.NeedsReview, $"Không nhận diện được giới tính \"{trimmed}\"");
    }

    private static NormalizedCellResult NormalizeDate(string original, string trimmed)
    {
        // Strictly dd/MM/yyyy (Vietnam standard) + ISO yyyy-MM-dd / yyyy/MM/dd + dd-MM-yyyy / dd.MM.yyyy + Excel serial
        DateTime parsed;
        var isoFormats = new[] { "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd" };
        var vnFormats = new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "dd.MM.yyyy", "d.M.yyyy" };

        if (DateTime.TryParseExact(trimmed, isoFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
            || DateTime.TryParseExact(trimmed, vnFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
            || LegacyImportRules.TryParseLegacyDate(trimmed, out parsed))
        {
            if (parsed.Year is < 1900 or > 2099)
                return new NormalizedCellResult(trimmed, NormalizedCellStatus.NeedsReview, $"Năm không hợp lệ: \"{trimmed}\"");

            var formatted = parsed.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            return formatted == original
                ? new NormalizedCellResult(formatted, NormalizedCellStatus.Valid)
                : new NormalizedCellResult(formatted, NormalizedCellStatus.AutoNormalized, $"Đổi định dạng ngày (\"{original}\" → \"{formatted}\")");
        }

        return new NormalizedCellResult(trimmed, NormalizedCellStatus.NeedsReview, $"Ngày không hợp lệ theo chuẩn dd/MM/yyyy: \"{trimmed}\"");
    }

    private static NormalizedCellResult NormalizeNationality(string original, string trimmed, IReadOnlyList<NationalityEntityDto> nationalities)
    {
        var entities = nationalities.Select(n => new NationalityEntity
        {
            IDNationality = n.IDNationality,
            NationalityCode = n.NationalityCode,
            NationalityName = n.NationalityName,
            Delete_flag = n.Delete_flag
        }).ToList();

        try
        {
            var resolved = LegacyImportRules.ResolveNationalityCode(trimmed, entities);
            if (!string.IsNullOrWhiteSpace(resolved))
            {
                return resolved == original
                    ? new NormalizedCellResult(resolved, NormalizedCellStatus.Valid)
                    : new NormalizedCellResult(resolved, NormalizedCellStatus.AutoNormalized, $"Ánh xạ quốc tịch (\"{original}\" → \"{resolved}\")");
            }
        }
        catch (InvalidOperationException)
        {
            // Fall through to English alias lookup
        }

        var normalizedKey = WhitespaceRegex().Replace(LegacyImportRules.RemoveDiacritics(trimmed), " ");
        if (EnglishNationalityAliases.TryGetValue(normalizedKey, out var aliasCode))
        {
            var active = nationalities.FirstOrDefault(n => n.Delete_flag == 0 && n.NationalityCode.Equals(aliasCode, StringComparison.OrdinalIgnoreCase));
            if (active is not null)
            {
                return active.NationalityCode == original
                    ? new NormalizedCellResult(active.NationalityCode, NormalizedCellStatus.Valid)
                    : new NormalizedCellResult(active.NationalityCode, NormalizedCellStatus.AutoNormalized, $"Ánh xạ quốc tịch (\"{original}\" → \"{active.NationalityCode}\")");
            }
        }

        return new NormalizedCellResult(trimmed, NormalizedCellStatus.NeedsReview, $"Quốc tịch \"{trimmed}\" chưa có trong danh mục");
    }

    private static NormalizedCellResult NormalizeWorkPermit(string original, string trimmed)
    {
        var key = WhitespaceRegex().Replace(LegacyImportRules.RemoveDiacritics(trimmed).ToLowerInvariant(), " ");
        if (key is "chua co" or "chua co gpld" or "khong" or "khong co" or "0" or "none" or "no" or "n/a")
            return new NormalizedCellResult("", NormalizedCellStatus.AutoNormalized, $"Đưa \"{original}\" về trống (Chưa có GPLĐ)");

        if (key is "mien gpld" or "mien giay phep lao dong" or "exempt")
            return new NormalizedCellResult("Miễn", NormalizedCellStatus.AutoNormalized, $"Chuẩn hóa \"{original}\" → \"Miễn\"");

        var collapsed = WhitespaceRegex().Replace(trimmed, " ");
        return collapsed != original
            ? new NormalizedCellResult(collapsed, NormalizedCellStatus.AutoNormalized, $"Gom khoảng trắng (\"{original}\" → \"{collapsed}\")")
            : new NormalizedCellResult(collapsed, NormalizedCellStatus.Valid);
    }

    private static NormalizedCellResult NormalizeFamilyVisit(string original, string trimmed)
    {
        var key = LegacyImportRules.RemoveDiacritics(trimmed).ToLowerInvariant();
        if (key is "co" or "1" or "yes" or "true" or "x" or "dung")
        {
            return original == "Có"
                ? new NormalizedCellResult("Có", NormalizedCellStatus.Valid)
                : new NormalizedCellResult("Có", NormalizedCellStatus.AutoNormalized, $"Chuẩn hóa thăm thân (\"{original}\" → \"Có\")");
        }
        if (key is "khong" or "0" or "no" or "false" or "sai")
        {
            return original == "Không"
                ? new NormalizedCellResult("Không", NormalizedCellStatus.Valid)
                : new NormalizedCellResult("Không", NormalizedCellStatus.AutoNormalized, $"Chuẩn hóa thăm thân (\"{original}\" → \"Không\")");
        }
        return new NormalizedCellResult(trimmed, NormalizedCellStatus.NeedsReview, $"Giá trị thăm thân không hợp lệ: \"{trimmed}\"");
    }

    private static NormalizedCellResult NormalizeStayPurpose(string original, string trimmed)
    {
        var upper = trimmed.ToUpperInvariant();
        if (!StayPurposeCodes.All.Contains(upper))
            return new NormalizedCellResult(trimmed, NormalizedCellStatus.NeedsReview, $"Mã diện cư trú không hợp lệ: \"{trimmed}\"");

        return upper != original
            ? new NormalizedCellResult(upper, NormalizedCellStatus.AutoNormalized, $"Chuẩn hóa chữ hoa (\"{original}\" → \"{upper}\")")
            : new NormalizedCellResult(upper, NormalizedCellStatus.Valid);
    }

    private static NormalizedCellResult NormalizeDocumentType(string original, string trimmed)
    {
        var parts = trimmed.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var upperParts = new List<string>(parts.Length);
        foreach (var part in parts)
        {
            var upper = part.ToUpperInvariant();
            if (!AllowedDocumentTypes.Contains(upper))
                return new NormalizedCellResult(trimmed, NormalizedCellStatus.NeedsReview, $"Loại giấy tờ XNC không hợp lệ: \"{part}\"");
            upperParts.Add(upper);
        }
        var joined = string.Join(";", upperParts);
        return joined != original
            ? new NormalizedCellResult(joined, NormalizedCellStatus.AutoNormalized, $"Chuẩn hóa mã giấy tờ (\"{original}\" → \"{joined}\")")
            : new NormalizedCellResult(joined, NormalizedCellStatus.Valid);
    }

    private static NormalizedCellResult NormalizeAdminUnit(string original, string trimmed, IReadOnlySet<string>? activeAdminUnitCodes)
    {
        var upper = trimmed.ToUpperInvariant();
        if (activeAdminUnitCodes is not null && activeAdminUnitCodes.Count > 0 && !activeAdminUnitCodes.Contains(upper))
            return new NormalizedCellResult(trimmed, NormalizedCellStatus.NeedsReview, $"Mã địa bàn \"{trimmed}\" chưa có trong danh mục");

        return upper != original
            ? new NormalizedCellResult(upper, NormalizedCellStatus.AutoNormalized, $"Chuẩn hóa mã địa bàn (\"{original}\" → \"{upper}\")")
            : new NormalizedCellResult(upper, NormalizedCellStatus.Valid);
    }

    private static NormalizedCellResult NormalizeText(string original, string trimmed)
    {
        var collapsed = WhitespaceRegex().Replace(trimmed, " ");
        return collapsed != original
            ? new NormalizedCellResult(collapsed, NormalizedCellStatus.AutoNormalized, $"Gom khoảng trắng (\"{original}\" → \"{collapsed}\")")
            : new NormalizedCellResult(collapsed, NormalizedCellStatus.Valid);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
