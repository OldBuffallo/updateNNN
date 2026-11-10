using IRM.Services;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Persistence.Stores.Documents;

public sealed class SqlLegalDocumentStore : ILegalDocumentStore
{
    private readonly IrmDbContext _db;
    private readonly SqlAuditStore _audit;
    private readonly IPrivateFileStorage _files;
    private readonly IServiceAuthorizationGuard _guard;

    public SqlLegalDocumentStore(IrmDbContext db, SqlAuditStore audit, IPrivateFileStorage files, IServiceAuthorizationGuard guard)
    {
        _db = db;
        _audit = audit;
        _files = files;
        _guard = guard;
    }

    public async Task<int> SaveMetadataAsync(CompanyLegalDocument document, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (document.Id == 0) _db.CompanyLegalDocuments.Add(document); else _db.CompanyLegalDocuments.UpdateValues(_db, document);
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("UPSERT", "CompanyLegalDocument", document.Id);
        return document.Id;
    }

    public async Task<StoredFile> StoreAsync(Stream content, string originalName, string contentType,
        CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var accountId = await _guard.GetRequiredAccountIdAsync();
        var entity = await _files.StageAsync(content, originalName, contentType, accountId, cancellationToken);
        try
        {
            _db.StoredFiles.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await _audit.LogAsync("UPLOAD", "StoredFile", entity.Id, $"Name={entity.OriginalName}; Sha256={entity.Sha256}");
            return entity;
        }
        catch
        {
            _files.Delete(entity.StorageName);
            throw;
        }
    }

    public async Task<Stream> OpenReadAsync(int storedFileId, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var entity = await _db.StoredFiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == storedFileId && !x.IsDeleted, cancellationToken)
            ?? throw new FileNotFoundException();
        if (entity.ScanStatusCode != "CLEAN") throw new UnauthorizedAccessException("Tệp chưa vượt qua kiểm tra an toàn.");
        _files.ValidateReadPath(entity.StorageName);
        await _audit.LogAsync("DOWNLOAD", "StoredFile", storedFileId);
        return _files.OpenRead(entity.StorageName);
    }
}
