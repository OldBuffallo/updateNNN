using IRM.Data;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure;

internal sealed class DatabaseReadiness(IDbContextFactory<IrmDbContext> factory) : IDatabaseReadiness
{
    public string SchemaVersion => SchemaVersionService.RequiredVersion;
    public string SchemaChecksum => SchemaVersionService.RequiredChecksum;
    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        return await db.Database.CanConnectAsync(cancellationToken);
    }
}
