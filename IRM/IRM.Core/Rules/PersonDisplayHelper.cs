namespace IRM.Core.Rules;

/// <summary>Chuẩn hóa tên hiển thị khi hồ sơ chỉ có Số hộ chiếu (chưa có họ tên).</summary>
public static class PersonDisplayHelper
{
    public static string FormatName(string? fullName, string? passport)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
            return fullName.Trim();
        if (!string.IsNullOrWhiteSpace(passport))
            return $"Chưa có tên ({passport.Trim()})";
        return "Chưa có tên";
    }
}
