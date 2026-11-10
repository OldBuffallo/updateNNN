using IRM.Data.Models;
using IRM.Services;
using System.Security.Cryptography;
using static IRM.Application.Features.Documents.LegalFileRules;

namespace IRM.Infrastructure.Files;

public sealed class LocalPrivateFileStorage(PrivateFileStorageOptions options, IMalwareScanner malwareScanner,
    IStorageCapacityGuard storageCapacityGuard) : IPrivateFileStorage
{
    private const long MaxBytes = 10 * 1024 * 1024;
    private readonly string _storageRoot = Path.GetFullPath(options.Root);
    private readonly IMalwareScanner _malwareScanner = malwareScanner;
    private readonly IStorageCapacityGuard _storageCapacityGuard = storageCapacityGuard;

    public async Task<StoredFile> StageAsync(Stream content, string originalName, string contentType,
        int accountId, CancellationToken cancellationToken = default)
    {
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
            return entity;
        }
        catch
        {
            if (File.Exists(target)) File.Delete(target);
            throw;
        }
    }

    public void ValidateReadPath(string storageName) => Resolve(storageName);
    public Stream OpenRead(string storageName) => new FileStream(Resolve(storageName), FileMode.Open,
        FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous);
    public void Delete(string storageName)
    {
        var path = Resolve(storageName);
        if (File.Exists(path)) File.Delete(path);
    }
    private string Resolve(string storageName)
    {
        var path = Path.GetFullPath(Path.Combine(_storageRoot, storageName));
        if (!path.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedAccessException();
        return path;
    }
}
