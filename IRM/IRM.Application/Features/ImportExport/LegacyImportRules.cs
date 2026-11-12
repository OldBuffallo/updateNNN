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
}
