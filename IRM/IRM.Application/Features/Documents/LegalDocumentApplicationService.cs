using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Documents;

public sealed class LegalDocumentApplicationService(ILegalDocumentStore store, IServiceAuthorizationGuard guard) : ILegalDocumentService
{
    private readonly ILegalDocumentStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<int> SaveMetadataAsync(CompanyLegalDocumentDto document, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var documentData = ToEntity(document);
        var result = await _store.SaveMetadataAsync(documentData, cancellationToken);
        DtoMapper.CopyToDto(documentData, document);
        return result;
    }

    public async Task<StoredFileDto> StoreAsync(Stream content, string originalName, string contentType, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var result = await _store.StoreAsync(content, originalName, contentType, cancellationToken);
        return ToDto(result);
    }

    public async Task<Stream> OpenReadAsync(int storedFileId, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var result = await _store.OpenReadAsync(storedFileId, cancellationToken);
        return result;
    }
}
