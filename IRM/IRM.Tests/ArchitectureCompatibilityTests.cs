using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IRM.Application.Features.Companies;
using IRM.Application.Features.Employees;
using IRM.Application.Features.ImportExport;
using IRM.Application.Models;
using IRM.Data;
using IRM.Data.Models;
using IRM.Infrastructure;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class ArchitectureCompatibilityTests
{
    [Fact]
    public void Assemblies_EnforceTheFourProjectBoundary()
    {
        var core = typeof(Company).Assembly;
        var application = typeof(ICompanyService).Assembly;
        var infrastructure = typeof(IrmDbContext).Assembly;
        Assert.Equal("IRM.Core", core.GetName().Name);
        Assert.Equal("IRM.Application", application.GetName().Name);
        Assert.Equal("IRM.Infrastructure", infrastructure.GetName().Name);
        Assert.DoesNotContain(core.GetReferencedAssemblies(), x => x.Name!.StartsWith("IRM.") || IsPlatformDependency(x.Name!));
        Assert.DoesNotContain(application.GetReferencedAssemblies(), x => x.Name is "IRM" or "IRM.Infrastructure" || IsPlatformDependency(x.Name!));
        Assert.DoesNotContain(infrastructure.GetReferencedAssemblies(), x => x.Name == "IRM");
        foreach (var contract in application.GetTypes().Where(x => x.IsInterface))
        foreach (var method in contract.GetMethods())
        {
            Assert.DoesNotContain("IQueryable", method.ReturnType.ToString());
            Assert.DoesNotContain("DbContext", method.ToString()!);
            Assert.DoesNotContain("DbSet", method.ToString()!);
        }
    }

    [Fact]
    public void WebForms_UseApplicationDtosAndServiceInterfaces()
    {
        var entityTypes = typeof(Company).Assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract).ToHashSet();
        var pages = typeof(Program).Assembly.GetTypes().Where(x => x.Namespace == "IRM.Components.Pages");
        foreach (var page in pages)
        foreach (var field in page.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.DoesNotContain(Unwrap(field.FieldType), entityTypes.Contains);
            Assert.NotEqual("IRM.Infrastructure", field.FieldType.Assembly.GetName().Name);
        }
    }

    [Fact]
    public void SqlServerMappingAndMigrationIds_IncludePassportOnlyMigration()
    {
        using var db = new IrmDbContext(new DbContextOptionsBuilder<IrmDbContext>()
            .UseSqlServer("Server=localhost;Database=IRM;Integrated Security=True;TrustServerCertificate=True").Options);
        var migrations = db.Database.GetMigrations().ToList();
        Assert.Contains("20260921030335_InitialSqlServerV101", migrations);
        Assert.Contains("20260927120137_AddVisaSymbol", migrations);
        Assert.Contains(migrations, m => m.EndsWith("_MakeEmployeeCompanyNullable", StringComparison.Ordinal));
        Assert.Equal(43, db.Model.GetEntityTypes().Count());
    }

    [Fact]
    public void HistoricalMigrationSources_AreUnmodified()
    {
        var hashes = JsonSerializer.Deserialize<Dictionary<string,string>>(ReadBaseline("migration-hashes.json"))!;
        var folder = Path.Combine(FindRepository(), "IRM.Infrastructure", "Persistence", "Migrations");
        foreach (var item in hashes.Where(h => !h.Key.Equals("IrmDbContextModelSnapshot.cs", StringComparison.OrdinalIgnoreCase)))
        {
            var source = File.ReadAllText(Path.Combine(folder, item.Key)).Replace("\r\n", "\n");
            Assert.Equal(item.Value, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))).ToLowerInvariant());
        }
    }

    [Fact]
    public async Task CompanyDto_IsDetachedAndCanBeUpdatedInTheSameScope()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        var guard = TestSecurityContext.CreateAllowAllGuard();
        var service = new CompanyApplicationService(new SqlCompanyStore(db.Context, guard), guard);
        var form = await service.GetByIdAsync(1);
        Assert.NotNull(form);
        var originalName = db.Context.Companies.Local.Single().CompanyName;
        form!.CompanyName = "Edited through DTO";
        Assert.Equal(originalName, db.Context.Companies.Local.Single().CompanyName);
        await service.UpdateAsync(form);
        db.Context.ChangeTracker.Clear();
        Assert.Equal(form.CompanyName, (await db.Context.Companies.FindAsync(1))!.CompanyName);
    }

    [Fact]
    public async Task DtoCreation_CopiesGeneratedKeysBackAndPreservesNavigationIdentity()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        var guard = TestSecurityContext.CreateAllowAllGuard();
        var companies = new CompanyApplicationService(new SqlCompanyStore(db.Context, guard), guard);
        var form = new CompanyDto { CompanyName = "New company", IDField = 1 };
        await companies.CreateAsync(form);
        Assert.True(form.IDCompany > 1);
        Assert.Equal(1, form.TrackerID);
        var employee = new EmployeeDto { StaffName = "New employee", IDCompany = form.IDCompany, Passport = "DTO-001" };
        var employees = new EmployeeApplicationService(new SqlEmployeeStore(db.Context, guard), guard);
        await employees.CreateAsync(employee);
        Assert.True(employee.IDEmployee > 0);
        var detail = await companies.GetByIdAsync(form.IDCompany);
        Assert.Single(detail!.Employees);
        Assert.Same(detail, detail.Employees.Single().Company);
        await employees.DeleteAsync(employee.IDEmployee);
        Assert.Empty(await employees.GetAllActiveAsync());
    }

    [Fact]
    public async Task ApplicationAuthorization_RejectsViewerWritesBeforePersistence()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        var guard = TestSecurityContext.CreateGuard(IrmRoles.Viewer);
        // Store intentionally has no guard: this proves authorization is enforced in Application.
        var service = new CompanyApplicationService(new SqlCompanyStore(db.Context), guard);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(new CompanyDto { CompanyName = "Denied", IDField = 1 }));
        Assert.Single(db.Context.Companies);
    }

    [Fact]
    public async Task ImportRollback_ReadsBackupsWrittenBeforeTheRefactor()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        var employee = new Employee { StaffName = "After import", IDCompany = 1, Passport = "LEGACY-001" };
        db.Context.Employees.Add(employee); await db.Context.SaveChangesAsync();
        db.Context.ImportHistories.Add(new ImportHistory { SessionId = "before-refactor", Status = "committed" });
        db.Context.ImportBackups.Add(new ImportBackup { ImportSessionId = "before-refactor", EntityType = "Employee", EntityId = employee.IDEmployee, ActionType = "UPDATE", OldData = ReadBaseline("legacy-import-backup.json") });
        await db.Context.SaveChangesAsync();
        var guard = TestSecurityContext.CreateAllowAllGuard();
        var service = new ImportApplicationService(new SqlImportStore(db.Context, new SqlAuditStore(db.Context), guard), guard);
        Assert.True(await service.RollbackImportAsync("before-refactor"));
        Assert.Equal("Before refactor", employee.StaffName);
        Assert.Equal("LD2", employee.VisaSymbol);
        Assert.Equal("rolledback", (await db.Context.ImportHistories.SingleAsync()).Status);
        Assert.False(await service.RollbackImportAsync("before-refactor"));
    }

    [Fact]
    public async Task ApplicationImport_PreservesPreviewIdentitiesAndOldTemplateJson()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        var guard = TestSecurityContext.CreateAllowAllGuard();
        var service = new ImportApplicationService(new SqlImportStore(db.Context, new SqlAuditStore(db.Context), guard), guard);
        var row = new ImportPreviewRowDto { RowNumber = 2, Status = "new", StaffName = "DTO import", ParsedEmployee = new EmployeeDto {
            StaffName = "DTO import", IDCompany = 1, Passport = "DTO-IMPORT-001", IDUser = 1 } };
        var result = await service.ExecuteImportAsync([row], [], 1, "dto-import.xlsx");
        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.True(row.ParsedEmployee!.IDEmployee > 0);
        var template = new ColumnMappingTemplateDto { MappingJson = "[{\"ExcelColumnIndex\":0,\"SystemField\":\"StaffName\"}]" };
        Assert.Equal("StaffName", Assert.Single(service.ApplyTemplate(template, [new ExcelColumnInfo { Index = 0, HeaderName = "Legacy column" }])).SystemField);
        Assert.True(await service.RollbackImportAsync(result.SessionId));
    }

    [Fact]
    public async Task StatisticsValidation_PreservesTheOriginalErrorPrecedence()
    {
        await using var db = new TestDatabase(); await db.InitializeAsync();
        var guard = TestSecurityContext.CreateAllowAllGuard();
        var service = new IRM.Application.Features.Reports.StatisticsApplicationService(
            new SqlStatisticsStore(db.Context, new SqlAuditStore(db.Context), guard), guard);
        var error = await Assert.ThrowsAsync<ArgumentException>(() => service.GetTerritoryAsync(
            new TemporalReportFilter { AsOfDate = DateTime.Today, From = DateTime.Today }));
        Assert.Contains("AsOfDate", error.Message);
    }

    private static bool IsPlatformDependency(string name) => name.StartsWith("Microsoft.EntityFrameworkCore") || name.StartsWith("Microsoft.AspNetCore") || name == "ClosedXML";
    private static IEnumerable<Type> Unwrap(Type type) => new[] {type}.Concat(type.IsGenericType ? type.GetGenericArguments().SelectMany(Unwrap) : []);
    internal static string ReadBaseline(string name) => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Baselines", name));
    internal static string FindRepository()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "IRM.sln"))) return dir.FullName;
        throw new DirectoryNotFoundException("IRM.sln was not found.");
    }
}
