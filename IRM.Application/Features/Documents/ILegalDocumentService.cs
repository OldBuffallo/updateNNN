using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Documents;

public interface ILegalDocumentService
{
    Task<int> SaveMetadataAsync(CompanyLegalDocumentDto document, CancellationToken cancellationToken = default);
    Task<StoredFileDto> StoreAsync(Stream content, string originalName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(int storedFileId, CancellationToken cancellationToken = default);
}
