using IRM.Data;

namespace IRM.Hosting;

internal static class MaintenanceCommands
{
    internal sealed record ConfigurationResult(bool Handled, string ConnectionString);
    public static async Task<ConfigurationResult> ConfigureAsync(IConfiguration configuration,
        bool bootstrapOnly, bool provisionDatabase, bool backupOnly, bool restoreOnly)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection cho SQL Server.");

        if (backupOnly)
        {
            await DatabaseBackup.CreateAndRotateAsync(
                connectionString,
                configuration["Backup:Directory"] ?? throw new InvalidOperationException("Thiếu Backup:Directory."));
            return new(true, connectionString);
        }

        if (restoreOnly)
        {
            await DatabaseBackup.RestoreAsync(
                configuration["Restore:AdminConnectionString"]
                    ?? throw new InvalidOperationException("Thiếu Restore:AdminConnectionString."),
                configuration["Restore:ArchivePath"]
                    ?? throw new InvalidOperationException("Thiếu Restore:ArchivePath."),
                configuration["Backup:Directory"]
                    ?? throw new InvalidOperationException("Thiếu Backup:Directory."));
            return new(true, connectionString);
        }

        var adminConnectionString = configuration["Provisioning:AdminConnectionString"];
        if (provisionDatabase)
        {
            if (string.IsNullOrWhiteSpace(adminConnectionString))
                throw new InvalidOperationException("Thiếu Provisioning:AdminConnectionString.");
            await DatabaseProvisioner.PrepareAsync(
                adminConnectionString,
                configuration["Provisioning:AppPassword"],
                configuration["Provisioning:DbaPassword"]);
        }

        var efConnectionString = bootstrapOnly
            ? DatabaseProvisioner.ForIrmDatabase(adminConnectionString
                ?? throw new InvalidOperationException("Bootstrap yêu cầu Provisioning:AdminConnectionString."))
            : connectionString;

        return new(false, efConnectionString);
    }
}
