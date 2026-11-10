using IRM.Data;
using IRM.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIrmInfrastructure(this IServiceCollection services, string connectionString)
    {
        void ConfigureSqlServer(DbContextOptionsBuilder options) => options.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(IrmDbContext).Assembly.GetName().Name);
            sql.CommandTimeout(60);
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        });
        services.AddDbContextFactory<IrmDbContext>(ConfigureSqlServer);
        services.AddDbContext<IrmDbContext>(ConfigureSqlServer);
        services.AddScoped<SqlCompanyStore>();
        services.AddScoped<ICompanyStore>(sp => sp.GetRequiredService<SqlCompanyStore>());
        services.AddScoped<SqlCompanyProfileStore>();
        services.AddScoped<ICompanyProfileStore>(sp => sp.GetRequiredService<SqlCompanyProfileStore>());
        services.AddScoped<SqlEmployeeStore>();
        services.AddScoped<IEmployeeStore>(sp => sp.GetRequiredService<SqlEmployeeStore>());
        services.AddScoped<SqlStudentStore>();
        services.AddScoped<IStudentStore>(sp => sp.GetRequiredService<SqlStudentStore>());
        services.AddScoped<SqlFamilyVisitorStore>();
        services.AddScoped<IForeignerRegistryStore>(sp => sp.GetRequiredService<SqlFamilyVisitorStore>());
        services.AddScoped<SqlResidenceStore>();
        services.AddScoped<IResidenceStore>(sp => sp.GetRequiredService<SqlResidenceStore>());
        services.AddScoped<SqlAccommodationStore>();
        services.AddScoped<IAccommodationStore>(sp => sp.GetRequiredService<SqlAccommodationStore>());
        services.AddScoped<SqlInspectionStore>();
        services.AddScoped<IInspectionStore>(sp => sp.GetRequiredService<SqlInspectionStore>());
        services.AddScoped<IRM.Application.Abstractions.IPrivateFileStorage>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var environment = sp.GetRequiredService<IHostEnvironment>();
            var root = configuration["FileStorage:Root"] ?? Path.Combine(environment.ContentRootPath, "..", "irm-private-files");
            return new IRM.Infrastructure.Files.LocalPrivateFileStorage(new(root), sp.GetRequiredService<IMalwareScanner>(), sp.GetRequiredService<IStorageCapacityGuard>());
        });
        services.AddScoped<SqlLegalDocumentStore>();
        services.AddScoped<ILegalDocumentStore>(sp => sp.GetRequiredService<SqlLegalDocumentStore>());
        services.AddScoped<SqlDashboardStore>();
        services.AddScoped<IDashboardStore>(sp => sp.GetRequiredService<SqlDashboardStore>());
        services.AddScoped<SqlSearchStore>();
        services.AddScoped<ISearchStore>(sp => sp.GetRequiredService<SqlSearchStore>());
        services.AddScoped<SqlStatisticsStore>();
        services.AddScoped<IStatisticsStore>(sp => sp.GetRequiredService<SqlStatisticsStore>());
        services.AddSingleton<IPasswordHashService, IRM.Infrastructure.Security.AspNetPasswordHashService>();
        services.AddScoped<SqlAuthStore>();
        services.AddScoped<IAuthStore>(sp => sp.GetRequiredService<SqlAuthStore>());
        services.AddScoped<SqlAuditStore>();
        services.AddScoped<IAuditStore>(sp => sp.GetRequiredService<SqlAuditStore>());
        services.AddScoped<SqlCatalogStore>();
        services.AddScoped<ICatalogStore>(sp => sp.GetRequiredService<SqlCatalogStore>());
        services.AddScoped<SqlImportStore>();
        services.AddScoped<IImportStore>(sp => sp.GetRequiredService<SqlImportStore>());
        services.AddScoped<SqlExportStore>();
        services.AddScoped<IExportStore>(sp => sp.GetRequiredService<SqlExportStore>());
        services.AddScoped<SqlStatisticsExportStore>();
        services.AddScoped<IStatisticsExportStore>(sp => sp.GetRequiredService<SqlStatisticsExportStore>());
        services.AddScoped<ILegacySyncService, LegacySyncService>();
        services.AddScoped<ISchemaVersionService, SchemaVersionService>();
        services.AddSingleton<IStorageCapacityGuard, StorageCapacityGuard>();
        services.AddSingleton<IMalwareScanner>(_ => OperatingSystem.IsWindows()
            ? new WindowsDefenderMalwareScanner()
            : OperatingSystem.IsLinux() ? new ClamAvMalwareScanner()
            : throw new PlatformNotSupportedException("IRM chỉ hỗ trợ Windows x64 và Ubuntu 22.04 amd64."));
        services.AddScoped<IRM.Application.Abstractions.IExcelReader, IRM.Infrastructure.Excel.ExcelReader>();
        services.AddScoped<IRM.Application.Abstractions.IDatabaseReadiness, DatabaseReadiness>();
        services.AddHostedService<LegacySyncWorker>();
        return services;
    }
}
