using System.Globalization;
using System.Text;

namespace IRM.Application.Features.ImportExport;

/// <summary>
/// Luật đọc/kiểm tra giá trị khi import Excel, chép từ WPF ReportManager
/// (ReadFileViewModel.cs, Bases/MethodHandler.cs, Bases/Constants.cs).
/// Lớp thuần: không phụ thuộc EF, Excel hay culture của máy chủ.
/// Không đổi nghĩa/số của dữ liệu cũ; chỉ thêm khi có nhu cầu mở rộng riêng.
/// </summary>
public static class LegacyImportRules
{
    // Giá trị WorkPermit của DB cũ (Constants.cs)
    public const int WorkPermitExemption = 0;
    public const int WorkPermitAvailable = 1;
    public const int WorkPermitNotYet = 2;
    public const int WorkPermitExemptionInvest = 3;
    public const int WorkPermitAvailableInvest = 4;
    public const int WorkPermitNotYetInvest = 5;
    public const int WorkPermitOther = 6;

    // ── Ngày tháng ────────────────────────────────────────────────

    /// <summary>
    /// Nhận <c>dd/MM/yyyy</c> hoặc <c>dd-MM-yyyy</c> (ngày/tháng 1–2 chữ số).
    /// Năm 2 chữ số: &lt; 40 → 20xx, 40–99 → 19xx. Ô có nhiều từ thì lấy từ đầu tiên
    /// đúng dạng ngày. Không phụ thuộc culture; không nhận yyyy-MM-dd hay MM/dd/yyyy.
    /// </summary>
    public static bool TryParseLegacyDate(string? text, out DateTime value)
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            if (TryParseToken(token, out value)) return true;
        }

        value = default;
        return false;
    }

    private static bool TryParseToken(string token, out DateTime value)
    {
        value = default;
        var parts = token.Split('-');
        if (parts.Length != 3) parts = token.Split('/');
        if (parts.Length != 3) return false;

        if (!TryInt(parts[0], out var day) ||
            !TryInt(parts[1], out var month) ||
            !TryInt(parts[2], out var year))
            return false;

        if (year >= 0 && year < 40) year += 2000;
        else if (year >= 40 && year < 100) year += 1900;

        if (year < 1 || year > 9999 || month < 1 || month > 12) return false;
        if (day < 1 || day > DateTime.DaysInMonth(year, month)) return false;

        value = new DateTime(year, month, day);
        return true;
    }

    private static bool TryInt(string s, out int n) =>
        int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out n);

    // ── Giới tính ─────────────────────────────────────────────────

    /// <summary>"NAM" (không phân biệt hoa/thường) → 1; mọi chữ khác → 2 (như WPF).</summary>
    public static int ParseGender(string? text) =>
        string.Equals(text?.Trim(), "NAM", StringComparison.OrdinalIgnoreCase) ? 1 : 2;

    // ── GPLĐ ──────────────────────────────────────────────────────

    /// <summary>
    /// Ô trống → chưa có; có chữ "Miễn" → miễn; có chữ "Khác" → thân nhân sống cùng (6);
    /// còn lại → đã có và chính ô đó là số GPLĐ. Nhà đầu tư cộng thêm 3.
    /// Thứ tự kiểm tra: Miễn → Khác → còn lại (như WPF).
    /// </summary>
    public static (int WorkPermit, string Number) ParseWorkPermit(string? cell, bool isInvestor)
    {
        if (string.IsNullOrWhiteSpace(cell))
            return (isInvestor ? WorkPermitNotYetInvest : WorkPermitNotYet, " ");

        var normalized = RemoveDiacritics(cell.Trim()).ToUpperInvariant();
        if (normalized.Contains("MIEN", StringComparison.Ordinal))
            return (isInvestor ? WorkPermitExemptionInvest : WorkPermitExemption, " ");
        if (normalized.Contains("KHAC", StringComparison.Ordinal))
            return (WorkPermitOther, " ");

        return (isInvestor ? WorkPermitAvailableInvest : WorkPermitAvailable, cell.Trim());
    }

    // ── Văn bản ───────────────────────────────────────────────────

    /// <summary>
    /// Bỏ dấu tiếng Việt (kể cả đ/Đ) và các ký tự không hiển thị (U+200B–U+200D, U+FEFF)
    /// rồi Trim. Dùng để so khớp tên quốc tịch, nghề, phường/xã.
    /// </summary>
    public static string RemoveDiacritics(string text)
    {
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
        {
            if (ch is '\u200B' or '\u200C' or '\u200D' or '\uFEFF') continue;
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(ch switch { 'đ' => 'd', 'Đ' => 'D', _ => ch });
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).Trim();
    }

    // ── Kết quả giải quyết ────────────────────────────────────────

    public const string SettlementFormatError = "Sai định dạng kết quả giải quyết";
    public const string SettlementTemporaryStayError =
        "Thời hạn tạm trú nhập vào và kết quả giải quyết là khác nhau";
    public const string SettlementVisaError =
        "Thẻ tạm trú nhập vào và kết quả giải quyết là khác nhau";

    /// <summary>Kết quả đọc ô "Kết quả giải quyết". <c>Error</c> không kèm số dòng.</summary>
    public sealed record SettlementParse(int Result, string Text, DateTime? CardCreationDate, string? Error);

    /// <summary>
    /// Tiền tố <c>GHTT:</c> → 0, <c>Visa:</c> → 1, <c>TTT:</c> → 2 (bỏ cả tiền tố lẫn dấu hai chấm),
    /// chữ khác → 3. Ô trống → (3, " "). Nội dung phải đúng
    /// <c>[số thẻ] dd/MM/yyyy-dd/MM/yyyy</c> (hoặc <c>dd-MM-yyyy-dd-MM-yyyy</c>), như
    /// <c>MethodHandler.checkSettlementResultsString</c>. Số thẻ lấy từ từ thứ 2 khi có 3 từ,
    /// từ đầu khi có 2 từ (trừ khi đó là VISA/GHTT/TTT). Ngày sau phải bằng hạn tạm trú;
    /// số thẻ (nếu có) phải bằng số visa (không phân biệt hoa/thường).
    /// </summary>
    public static SettlementParse ParseSettlement(string? cell, string visaNumber, DateTime temporaryStay)
    {
        if (string.IsNullOrWhiteSpace(cell))
            return new SettlementParse(3, " ", null, null);

        var trimmed = cell.Trim();
        var result = 3;
        var text = trimmed;
        var folded = RemoveDiacritics(trimmed).ToUpperInvariant();
        foreach (var (prefix, value) in new[] { ("GHTT:", 0), ("VISA:", 1), ("TTT:", 2) })
        {
            if (!folded.StartsWith(prefix, StringComparison.Ordinal)) continue;
            result = value;
            text = trimmed[(trimmed.IndexOf(':') + 1)..].Trim();
            break;
        }

        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
            return new SettlementParse(result, text, null, SettlementFormatError);

        var visa = "";
        if (tokens.Length == 3)
        {
            visa = tokens[1];
        }
        else if (tokens.Length == 2)
        {
            var head = tokens[0].ToUpperInvariant();
            if (head is not ("VISA" or "GHTT" or "TTT")) visa = tokens[0];
        }

        if (!TryParseDateRange(tokens[^1], out var created, out var expires))
            return new SettlementParse(result, text, null, SettlementFormatError);

        if (expires.Date != temporaryStay.Date)
            return new SettlementParse(result, text, created, SettlementTemporaryStayError);

        if (visa.Length > 0 &&
            !string.Equals(visaNumber?.Trim(), visa.Trim(), StringComparison.OrdinalIgnoreCase))
            return new SettlementParse(result, text, created, SettlementVisaError);

        return new SettlementParse(result, text, created, null);
    }

    private static bool TryParseDateRange(string token, out DateTime created, out DateTime expires)
    {
        created = expires = default;
        var parts = token.Split('-');
        string first, second;
        if (parts.Length == 2)
        {
            first = parts[0];
            second = parts[1];
        }
        else if (parts.Length == 6)
        {
            first = $"{parts[0]}-{parts[1]}-{parts[2]}";
            second = $"{parts[3]}-{parts[4]}-{parts[5]}";
        }
        else
        {
            return false;
        }

        return TryParseLegacyDate(first, out created) && TryParseLegacyDate(second, out expires);
    }

    // ── Địa chỉ ───────────────────────────────────────────────────

    /// <summary>
    /// Địa chỉ phải kết thúc bằng tên một phường/xã có trong danh mục (không dấu, không phân
    /// biệt hoa/thường, bỏ ký tự ẩn như U+200D). Tên phường trống bị bỏ qua.
    /// </summary>
    public static bool AddressEndsWithWard(string? address, IEnumerable<string> wards)
    {
        if (string.IsNullOrWhiteSpace(address)) return false;
        var normalizedAddress = RemoveDiacritics(address).ToUpperInvariant();
        foreach (var ward in wards)
        {
            var normalizedWard = RemoveDiacritics(ward ?? "").ToUpperInvariant();
            if (normalizedWard.Length == 0) continue;
            if (normalizedAddress.EndsWith(normalizedWard, StringComparison.Ordinal)) return true;
        }
        return false;
    }

    // ── Nghề nghiệp ───────────────────────────────────────────────

    /// <summary>
    /// Tra nghề theo tên <b>trong đúng nhóm nghề đã chọn</b>, bỏ nghề đã xoá
    /// (<c>IDCG = nhóm and Delete_flag = 0 and CareerName like tên</c>). Không phân biệt
    /// hoa/thường, vẫn phân biệt dấu như SQL cũ. Nhiều dòng trùng thì lấy ID nhỏ nhất.
    /// </summary>
    public static int? FindCareerId(string? name, int careerGroupId, IEnumerable<IRM.Data.Models.Career> careers)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var wanted = name.Normalize(NormalizationForm.FormC).Trim();
        return careers
            .Where(c => c.Delete_flag == 0 && c.IDCG == careerGroupId &&
                        string.Equals(c.CareerName.Normalize(NormalizationForm.FormC).Trim(), wanted,
                            StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.IDCareer)
            .Select(c => (int?)c.IDCareer)
            .FirstOrDefault();
    }

    /// <summary>Nghề "Thân nhân" của nhóm "Khác" (ID 9 trong ReportManagerDB).</summary>
    public static bool IsRelativeCareer(IRM.Data.Models.Career career, int otherCareerGroupId = 9) =>
        career.IDCG == otherCareerGroupId &&
        RemoveDiacritics(career.CareerName).ToUpperInvariant() == "THAN NHAN";
    /// <summary>13 trường nghiệp vụ dùng nhận diện phiên bản trùng của WPF.</summary>
    public static bool SameAs13Fields(IRM.Data.Models.Employee a, IRM.Data.Models.Employee b) =>
        a.StaffName == b.StaffName && a.Gender == b.Gender && a.Birthday == b.Birthday &&
        a.Nationality == b.Nationality && a.Passport == b.Passport && a.Address == b.Address &&
        a.IDCareer == b.IDCareer && a.WorkPermit == b.WorkPermit && a.WorkPermitNumber == b.WorkPermitNumber &&
        a.VisaNumber == b.VisaNumber && a.TemporaryStay == b.TemporaryStay &&
        a.SettlementResults == b.SettlementResults &&
        string.Equals(a.SettlementResultsString?.Trim(), b.SettlementResultsString?.Trim(), StringComparison.Ordinal) &&
        string.Equals(a.Note?.Trim(), b.Note?.Trim(), StringComparison.Ordinal);
}
