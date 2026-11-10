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
            if (bootstrapOnly)
            {
                await db.Database.MigrateAsync();
                await DatabaseSeeder.SeedCatalogsIfEmptyAsync(
                    db,
                    configuration["Bootstrap:AdminUsername"],
                    configuration["Bootstrap:AdminPassword"]);
                if (provisionDatabase)
                    await DatabaseProvisioner.FinalizeAsync(adminConnectionString!);
            }
            await scope.ServiceProvider.GetRequiredService<ISchemaVersionService>().ValidateAsync();
        }

    }
}
