using System.Security.Cryptography;
using System.Text;

namespace IRM.Core.Rules;

public static class PassportRules
{

    public static string? NormalizePassport(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = string.Concat(value.Where(char.IsLetterOrDigit)).ToUpperInvariant();
        return normalized.Length == 0 ? null : normalized;
    }

    public static string? SearchKey(string? value)
    {
        var normalized = NormalizePassport(value);
        return normalized is null ? null : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }
}
