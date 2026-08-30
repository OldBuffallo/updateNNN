using IRM.Data;
using IRM.Data.Models;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class DatabaseAndSchemaFeatureTddTests
{
    [Fact]
    public async Task SchemaVersionService_ValidatesVersion010_ThrowsWhenMissing()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: false);
        var service = new SchemaVersionService(db.Context);

        // Initially empty SchemaVersions table -> throws InvalidOperationException
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateAsync());
        Assert.Contains("0.1.0", ex.Message);

        // Add 0.1.0 version -> ValidateAsync completes successfully
        db.Context.SchemaVersions.Add(new SchemaVersion { Version = "0.1.0", AppliedAt = DateTime.UtcNow });
        await db.Context.SaveChangesAsync();

        await service.ValidateAsync(); // Should not throw
    }

    [Fact]
    public async Task DatabaseSeeder_SeedV010Async_IsIdempotent()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: false);

        // Run seed first time
        await DatabaseSeeder.SeedV010Async(db.Context);
        var unitCount1 = await db.Context.AdministrativeUnits.CountAsync();
        Assert.Equal(54, unitCount1);

        // Run seed second time -> counts must remain exactly identical
        await DatabaseSeeder.SeedV010Async(db.Context);
        var unitCount2 = await db.Context.AdministrativeUnits.CountAsync();
        Assert.Equal(unitCount1, unitCount2);
    }

    [Fact]
    public async Task ExportService_ExportsCompaniesToClosedXmlStream()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();

        var company = new Company { CompanyName = "Export Demo Corp", Address = "Hạ Long", IDField = 1, TrackerID = 1 };
        db.Context.Companies.Add(company);
        await db.Context.SaveChangesAsync();

        var factory = new TestDbContextFactory(db.Context);
        var exportService = new ExportService(factory, new AuditService(db.Context), TestSecurityContext.CreateAllowAllGuard());

        var bytes = await exportService.ExportCompaniesAsync(null, null);
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 100);

        // Valid Excel zip header (PK\x03\x04)
        Assert.Equal(0x50, bytes[0]);
        Assert.Equal(0x4B, bytes[1]);
    }

    private sealed class TestDbContextFactory(IrmDbContext context) : IDbContextFactory<IrmDbContext>
    {
        public IrmDbContext CreateDbContext() => context;
        public Task<IrmDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(context);
    }
}
