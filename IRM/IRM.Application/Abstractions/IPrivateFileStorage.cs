using IRM.Data.Models;

namespace IRM.Application.Abstractions;

public interface IPrivateFileStorage
{
    Task<StoredFile> StageAsync(Stream content, string originalName, string contentType, int accountId, CancellationToken cancellationToken = default);
    void ValidateReadPath(string storageName);
    Stream OpenRead(string storageName);
    void Delete(string storageName);
}
