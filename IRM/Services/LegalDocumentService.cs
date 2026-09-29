using System.Diagnostics;
using System.Security.Cryptography;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class LegalDocumentService : ILegalDocumentService
{
    private const long MaxBytes = 10 * 1024 * 1024;
    private static readonly Dictionary<string, string[]> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ["application/pdf"],
        [".png"] = ["image/png"],
        [".jpg"] = ["image/jpeg"],
        [".jpeg"] = ["image/jpeg"]
    };
    private readonly IrmDbContext _db;
    private readonly AuditService _audit;
    private readonly string _storageRoot;
    private readonly IServiceAuthorizationGuard _guard;
    private readonly IMalwareScanner _malwareScanner;
    private readonly IStorageCapacityGuard _storageCapacityGuard;
    public LegalDocumentService(IrmDbContext db, AuditService audit, IConfiguration configuration, IWebHostEnvironment environment,
        IServiceAuthorizationGuard guard, IMalwareScanner malwareScanner, IStorageCapacityGuard? storageCapacityGuard = null)
    {
        _db = db;
        _audit = audit;
        _guard = guard;
        _malwareScanner = malwareScanner;
        _storageCapacityGuard = storageCapacityGuard ?? AllowAllStorageCapacityGuard.Instance;
        _storageRoot = Path.GetFullPath(configuration["FileStorage:Root"]
            ?? Path.Combine(environment.ContentRootPath, "..", "irm-private-files"));
    }

    public async Task<int> SaveMetadataAsync(CompanyLegalDocument document, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (document.Id == 0) _db.CompanyLegalDocuments.Add(document); else _db.CompanyLegalDocuments.Update(document);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UPSERT", "CompanyLegalDocument", document.Id);
        return document.Id;
    }

    public async Task<StoredFile> StoreAsync(Stream content, string originalName, string contentType,
        CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var accountId = await _guard.GetRequiredAccountIdAsync();
        var extension = Path.GetExtension(Path.GetFileName(originalName));
        ValidateUploadMetadata(extension, contentType);
        Directory.CreateDirectory(_storageRoot);
        _storageCapacityGuard.EnsureCanWrite(_storageRoot);
        var storageName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var target = Path.GetFullPath(Path.Combine(_storageRoot, storageName));
        if (!target.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Đường dẫn tệp không hợp lệ.");
        try
        {
            await using (var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous))
            {
                var buffer = new byte[81920];
                long total = 0;
                int read;
                while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    total += read;
                    if (total > MaxBytes) throw new InvalidOperationException("Tệp vượt quá giới hạn 10 MB.");
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }
            }
            var header = new byte[8];
            await using (var signatureStream = File.OpenRead(target)) await signatureStream.ReadExactlyAsync(header, cancellationToken);
            if (!HasAllowedSignature(header, extension)) throw new InvalidOperationException("Nội dung tệp không khớp phần mở rộng.");
            await _malwareScanner.ScanAsync(target, cancellationToken);
            await using var hashStream = File.OpenRead(target);
            var entity = new StoredFile
            {
                StorageName = storageName, OriginalName = Path.GetFileName(originalName), ContentType = contentType,
                Size = new FileInfo(target).Length, Sha256 = Convert.ToHexString(await SHA256.HashDataAsync(hashStream, cancellationToken)),
                ScanStatusCode = "CLEAN", CreatedByAccountId = accountId
            };
            _db.StoredFiles.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await _audit.LogAsync("UPLOAD", "StoredFile", entity.Id, $"Name={entity.OriginalName}; Sha256={entity.Sha256}");
            return entity;
        }
        catch
        {
            if (File.Exists(target)) File.Delete(target);
            throw;
        }
    }

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

    public async Task<Stream> OpenReadAsync(int storedFileId, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var entity = await _db.StoredFiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == storedFileId && !x.IsDeleted, cancellationToken)
            ?? throw new FileNotFoundException();
        if (entity.ScanStatusCode != "CLEAN") throw new UnauthorizedAccessException("Tệp chưa vượt qua kiểm tra an toàn.");
        var target = Path.GetFullPath(Path.Combine(_storageRoot, entity.StorageName));
        if (!target.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedAccessException();
        await _audit.LogAsync("DOWNLOAD", "StoredFile", storedFileId);
        return new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous);
    }

    private sealed class AllowAllStorageCapacityGuard : IStorageCapacityGuard
    {
        public static readonly AllowAllStorageCapacityGuard Instance = new();
        public void EnsureCanWrite(string path) { }
    }

}

public sealed class WindowsDefenderMalwareScanner : IMalwareScanner
{
    public async Task ScanAsync(string path, CancellationToken cancellationToken = default)
    {
        var defender = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Microsoft", "Windows Defender", "Platform");
        var executable = Directory.Exists(defender)
            ? Directory.GetDirectories(defender).OrderByDescending(x => x).Select(x => Path.Combine(x, "MpCmdRun.exe")).FirstOrDefault(File.Exists)
            : null;
        executable ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender", "MpCmdRun.exe");
        if (!File.Exists(executable)) throw new InvalidOperationException("Windows Defender không khả dụng; tệp không được lưu.");
        using var process = Process.Start(new ProcessStartInfo(executable, $"-Scan -ScanType 3 -File \"{path}\" -DisableRemediation")
        { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true })
            ?? throw new InvalidOperationException("Không khởi động được Windows Defender.");
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode != 0) throw new InvalidOperationException("Tệp không vượt qua kiểm tra mã độc.");
    }
}

public sealed class ClamAvMalwareScanner : IMalwareScanner
{
    public async Task ScanAsync(string path, CancellationToken cancellationToken = default)
    {
        const string executable = "/usr/bin/clamscan";
        if (!File.Exists(executable))
            throw new InvalidOperationException("ClamAV không khả dụng; tệp không được lưu.");

        var startInfo = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add("--no-summary");
        startInfo.ArgumentList.Add("--infected");
        startInfo.ArgumentList.Add(path);
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Không khởi động được ClamAV.");
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode == 1)
            throw new InvalidOperationException("Tệp không vượt qua kiểm tra mã độc.");
        if (process.ExitCode != 0)
            throw new InvalidOperationException("ClamAV không thể hoàn thành việc quét; tệp không được lưu.");
    }
}
