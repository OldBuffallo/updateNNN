using IRM.Data;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Maintenance;

public static class DatabaseStartup
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration,
        bool bootstrapOnly, bool provisionDatabase, string? adminConnectionString)
    {
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IrmDbContext>();
            if (db.Database.IsSqlite())
            {
                await db.Database.EnsureCreatedAsync();
                await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL; PRAGMA busy_timeout = 5000;");
                if (bootstrapOnly || !await db.Accounts.AnyAsync())
                {
                    await DatabaseSeeder.SeedCatalogsIfEmptyAsync(
                        db,
                        configuration["Bootstrap:AdminUsername"] ?? "admin",
                        configuration["Bootstrap:AdminPassword"] ?? "AdminDemo@2026!");
                    await DatabaseSeeder.SeedOnboardingGuideIfEmptyAsync(db);
                }
            }
            else if (bootstrapOnly)
            {
                await db.Database.MigrateAsync();
                await DatabaseSeeder.SeedCatalogsIfEmptyAsync(
                    db,
                    configuration["Bootstrap:AdminUsername"],
                    configuration["Bootstrap:AdminPassword"]);
                await DatabaseSeeder.SeedOnboardingGuideIfEmptyAsync(db);
                if (provisionDatabase)
                    await DatabaseProvisioner.FinalizeAsync(adminConnectionString!);
            }
            else if (await db.Accounts.AnyAsync())
            {
                await DatabaseSeeder.SeedCatalogsIfEmptyAsync(db);
                await DatabaseSeeder.SeedOnboardingGuideIfEmptyAsync(db);
            }

            await scope.ServiceProvider.GetRequiredService<ISchemaVersionService>().ValidateAsync();
        }
    }
}
