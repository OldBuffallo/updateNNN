

namespace IRM.Application.Features.Documents;

public static class LegalFileRules
{
    private static readonly Dictionary<string, string[]> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ["application/pdf"],
        [".png"] = ["image/png"],
        [".jpg"] = ["image/jpeg"],
        [".jpeg"] = ["image/jpeg"]
    };

    public static void ValidateUploadMetadata(string extension, string contentType)
    {
        if (!Allowed.TryGetValue(extension, out var mimeTypes) || !mimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Định dạng tệp không được phép.");
    }

    public static bool HasAllowedSignature(ReadOnlySpan<byte> header, string extension)
    {
        if (header.Length < 8) return false;
        return extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            ? header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46
            : extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
                ? header.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })
                : header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
    }
}
