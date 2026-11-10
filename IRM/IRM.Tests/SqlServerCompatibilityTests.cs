using IRM.Application.Features.Companies;
using IRM.Application.Features.Employees;
using IRM.Application.Features.ImportExport;
using IRM.Application.Models;
using IRM.Data;
using IRM.Data.Models;
using IRM.Infrastructure;
using IRM.Infrastructure.Maintenance;
using IRM.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IRM.Tests;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("IRM_TEST_SQLSERVER")))
            Skip = "Set IRM_TEST_SQLSERVER to a test SQL Server with CREATE DATABASE permission. Only a new IRM_RefactorTest_* database is used.";
    }
}

public sealed class SqlServerCompatibilityTests
{
    [SqlServerFact]
    public async Task ExistingSchema_WebAndLegacyWritesSyncAndRollbackRemainCompatible()
    {
        var supplied = Environment.GetEnvironmentVariable("IRM_TEST_SQLSERVER")!;
        var master = new SqlConnectionStringBuilder(supplied) { InitialCatalog = "master", TrustServerCertificate = true };
        var databaseName = "IRM_RefactorTest_" + Guid.NewGuid().ToString("N");
        var target = new SqlConnectionStringBuilder(master.ConnectionString) { InitialCatalog = databaseName, MultipleActiveResultSets = true };
        await using var admin = new SqlConnection(master.ConnectionString);
        await admin.OpenAsync();
        await using (var create = new SqlCommand($"CREATE DATABASE [{databaseName}]", admin)) await create.ExecuteNonQueryAsync();
        try
        {
            var options = new DbContextOptionsBuilder<IrmDbContext>().UseSqlServer(target.ConnectionString).Options;
            await using var db = new IrmDbContext(options);
            await db.Database.MigrateAsync();
            await DatabaseSeeder.SeedCatalogsIfEmptyAsync(db, "refactor_admin", "Only-for-isolated-refactor-test-1!");
            var schemaBefore = await FingerprintAsync(db);
            var migrationsBefore = (await db.Database.GetAppliedMigrationsAsync()).ToArray();
            Assert.Equal(ArchitectureCompatibilityTests.ReadBaseline("Migrations.txt").Split(['\r','\n'], StringSplitOptions.RemoveEmptyEntries), migrationsBefore);
            var guard = TestSecurityContext.CreateAllowAllGuard();
            var audit = new SqlAuditStore(db);
            var companyId = await db.Companies.Select(x => x.IDCompany).FirstAsync();
            var companyService = new CompanyApplicationService(new SqlCompanyStore(db, guard), guard);
            var company = (await companyService.GetByIdAsync(companyId))!;
            company.CompanyName = "Shared company written by web";
            await companyService.UpdateAsync(company);
            Assert.Equal(company.CompanyName, await ScalarAsync(db, $"SELECT CompanyName FROM Companies WHERE IDCompany={companyId}"));

            var employeeService = new EmployeeApplicationService(new SqlEmployeeStore(db, guard), guard);
            var form = new EmployeeDto { StaffName = "Web employee", Passport = "SQL-LEGACY-001", IDCompany = companyId, IDUser = 1 };
            await employeeService.CreateAsync(form);
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Employees SET StaffName={"Written by legacy client"} WHERE IDEmployee={form.IDEmployee}");
            db.ChangeTracker.Clear();
            Assert.Equal("Written by legacy client", (await employeeService.GetByIdAsync(form.IDEmployee))!.StaffName);
            db.LegacyChangeEvents.Add(new LegacyChangeEvent { EntityType = "Employees", EntityId = form.IDEmployee.ToString(), StatusCode = "PENDING",
                AfterXml = "<row><StaffName>Written by legacy client</StaffName><Passport>SQL-LEGACY-001</Passport></row>" });
            await db.SaveChangesAsync();
            var registry = new SqlFamilyVisitorStore(db, audit, guard);
            var sync = new LegacySyncService(db, registry);
            Assert.Equal(1, await sync.ProcessPendingAsync(10));
            Assert.Equal(0, await sync.ProcessPendingAsync(10));
            Assert.Single(await db.ForeignPersonSourceLinks.ToListAsync());
            Assert.Single(await db.ForeignPersons.ToListAsync());
            db.LegacyChangeEvents.Add(new LegacyChangeEvent { EntityType = "Employees", EntityId = form.IDEmployee.ToString(), StatusCode = "PENDING",
                AfterXml = "<row><StaffName>Written by legacy client</StaffName><Passport>SQL-LEGACY-001</Passport></row>" });
            await db.SaveChangesAsync();
            Assert.Equal(1, await sync.ProcessPendingAsync(10));
            Assert.Single(await db.ForeignPersonSourceLinks.ToListAsync());
            Assert.Single(await db.ForeignPersons.ToListAsync());

            var template = new ColumnMappingTemplate { TemplateName = "Before refactor", MappingJson = "[{\"ExcelColumnIndex\":0,\"SystemField\":\"StaffName\"}]" };
            db.ColumnMappingTemplates.Add(template);
            db.ImportHistories.Add(new ImportHistory { SessionId = "old-session", Status = "committed", CompanyId = companyId });
            db.ImportBackups.Add(new ImportBackup { ImportSessionId = "old-session", EntityType = "Employee", EntityId = form.IDEmployee,
                ActionType = "UPDATE", OldData = ArchitectureCompatibilityTests.ReadBaseline("legacy-import-backup.json") });
            await db.SaveChangesAsync();
            var import = new ImportApplicationService(new SqlImportStore(db, audit, guard), guard);
            var templates = await import.GetTemplatesAsync();
            Assert.Equal("StaffName", Assert.Single(import.ApplyTemplate(Assert.Single(templates), [new ExcelColumnInfo { Index = 0, HeaderName = "Legacy column" }])).SystemField);
            Assert.True(await import.RollbackImportAsync("old-session"));
            db.ChangeTracker.Clear();
            Assert.Equal("Before refactor", (await db.Employees.FindAsync(form.IDEmployee))!.StaffName);

            // Both normal startup and an explicit repeat bootstrap must preserve schema and migration history.
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
                ["Bootstrap:AdminUsername"] = "refactor_admin", ["Bootstrap:AdminPassword"] = "Only-for-isolated-refactor-test-1!"
            }).Build();
            var services = new ServiceCollection().AddLogging().AddIrmInfrastructure(target.ConnectionString)
                .AddScoped<IServiceAuthorizationGuard>(_ => guard).BuildServiceProvider();
            await using (services)
            {
                await DatabaseStartup.InitializeAsync(services, configuration, false, false, null);
                await DatabaseStartup.InitializeAsync(services, configuration, true, false, null);
            }
            Assert.Equal(schemaBefore, await FingerprintAsync(db));
            Assert.Equal(migrationsBefore, await db.Database.GetAppliedMigrationsAsync());
        }
        finally
        {
            // Name is generated here, never taken from user configuration. Never drop an existing application DB.
            if (!databaseName.StartsWith("IRM_RefactorTest_", StringComparison.Ordinal)) throw new InvalidOperationException();
            SqlConnection.ClearAllPools();
            await using var drop = new SqlCommand($"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}];", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }

    private static async Task<string> ScalarAsync(IrmDbContext db, string sql)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();
        await using var command = connection.CreateCommand(); command.CommandText = sql;
        return Convert.ToString(await command.ExecuteScalarAsync())!;
    }
    private static Task<string> FingerprintAsync(IrmDbContext db) => ScalarAsync(db, """
        SELECT CONCAT(
          (SELECT o.name,o.type,s.name AS schema_name,m.definition FROM sys.objects o JOIN sys.schemas s ON o.schema_id=s.schema_id LEFT JOIN sys.sql_modules m ON o.object_id=m.object_id WHERE o.is_ms_shipped=0 ORDER BY o.name FOR JSON PATH),
          (SELECT OBJECT_NAME(c.object_id) AS table_name,c.name,c.column_id,c.system_type_id,c.max_length,c.precision,c.scale,c.is_nullable,c.is_identity,dc.definition FROM sys.columns c LEFT JOIN sys.default_constraints dc ON c.default_object_id=dc.object_id JOIN sys.objects o ON c.object_id=o.object_id WHERE o.is_ms_shipped=0 ORDER BY OBJECT_NAME(c.object_id),c.column_id FOR JSON PATH),
          (SELECT OBJECT_NAME(i.object_id) AS table_name,i.name,i.type,i.is_unique,i.filter_definition,c.column_id,c.key_ordinal,c.is_included_column FROM sys.indexes i JOIN sys.index_columns c ON i.object_id=c.object_id AND i.index_id=c.index_id JOIN sys.objects o ON i.object_id=o.object_id WHERE o.is_ms_shipped=0 ORDER BY OBJECT_NAME(i.object_id),i.name,c.index_column_id FOR JSON PATH))
        """);
}
