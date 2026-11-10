using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Documents; query execution and transactions remain in Infrastructure.</summary>
public interface ILegalDocumentStore
{
    Task<int> SaveMetadataAsync(CompanyLegalDocument document, CancellationToken cancellationToken = default);
    Task<StoredFile> StoreAsync(Stream content, string originalName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(int storedFileId, CancellationToken cancellationToken = default);
}
