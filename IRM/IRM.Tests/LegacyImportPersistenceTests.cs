using ClosedXML.Excel;
using IRM.Application.Features.ImportExport;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public class LegacyImportPersistenceTests
{
    [Theory]
    [InlineData("Số GPLĐ", "WorkPermitNumber")]
    [InlineData("Tên người thân", "FamilyVisitRelativeName")]
    [InlineData("Ghi chú thăm thân", "FamilyVisitNote")]
    [InlineData("Địa chỉ tạm trú", "Address")]
    [InlineData("Số GPLĐ/Thời hạn", "WorkPermit")]
    [InlineData("Số visa/Thẻ tạm trú", "VisaNumber")]
    [InlineData("Thời hạn tạm trú", "TemporaryStay")]
    [InlineData("Kết quả giải quyết", "SettlementResults")]
    public void AutoMap_UsesTheMostSpecificKeyword(string header, string expected)
    {
        Assert.Equal(expected, Assert.Single(ImportMapping.AutoMapColumns([new() { HeaderName = header }])).SystemField);
    }

    [Fact]
    public async Task Sqlite_VersioningCountersWarningsAndRollback()
    {
        await using var database = new TestDatabase(); await database.InitializeAsync();
        await VerifyVersioningAsync(database.Context);
    }

    [SqlServerFact]
    public async Task SqlServer_VersioningCountersWarningsAndRollback()
    {
        await using var database = await IsolatedImportSqlDatabase.CreateAsync();
        await VerifyVersioningAsync(database.Context);
    }

    internal static async Task SeedAsync(IrmDbContext db)
    {
        if (!await db.Accounts.AnyAsync()) {
            db.Accounts.Add(new() { IDUser = 1, Username = "test", Name = "Test", Password = "test" });
            await SaveIdentityAsync(db, "Accounts");
            db.Fields.Add(new() { IDField = 1, FieldName = "Test" });
            await SaveIdentityAsync(db, "Fields");
            db.CareerGroups.Add(new() { IDCG = 1, CareerGroupName = "Test" });
            await SaveIdentityAsync(db, "CareerGroups");
            db.Careers.Add(new() { IDCareer = 1, IDCG = 1, CareerName = "Kỹ sư" });
            await SaveIdentityAsync(db, "Careers");
            db.Nationality.Add(new() { NationalityCode = "CHN", NationalityName = "Trung Quốc" });
            await db.SaveChangesAsync();
            db.Companies.Add(new() { IDCompany = 1, CompanyName = "Test Company", IDField = 1, TrackerID = 1 });
            await SaveIdentityAsync(db, "Companies");
        }
        db.CareerGroups.Add(new() { IDCG = 9, CareerGroupName = "Khác" });
        await SaveIdentityAsync(db, "CareerGroups");
        db.Careers.Add(new() { IDCareer = 14, IDCG = 9, CareerName = "Thân nhân" });
        await SaveIdentityAsync(db, "Careers");
        db.Wards.Add(new() { WardName = "Hải Châu" });
        db.Companies.Add(new() { CompanyName = "Second Company", IDField = 1, TrackerID = 1 });
        await db.SaveChangesAsync();
    }

    private static async Task SaveIdentityAsync(IrmDbContext db, string table)
    {
        if (!db.Database.IsSqlServer()) { await db.SaveChangesAsync(); return; }
        if (table is not ("Accounts" or "Fields" or "CareerGroups" or "Careers" or "Companies")) throw new ArgumentException("Unknown test table");
        await db.Database.OpenConnectionAsync();
        var enable = "SET IDENTITY_INSERT [" + table + "] ON";
        var disable = "SET IDENTITY_INSERT [" + table + "] OFF";
        try {
            await db.Database.ExecuteSqlRawAsync(enable);
            try { await db.SaveChangesAsync(); }
            finally { await db.Database.ExecuteSqlRawAsync(disable); }
        }
        finally { await db.Database.CloseConnectionAsync(); }
    }

    internal static SqlImportStore Store(IrmDbContext db) => new(db, new SqlAuditStore(db));
    internal static async Task<List<ImportPreviewRow>> Preview(IrmDbContext db, Dictionary<string,string> row, int company = 1)
    {
        using var stream = LegacyImportEndToEndTests.Workbook([row]);
        return await Store(db).GeneratePreviewAsync(stream, LegacyImportEndToEndTests.Mappings(), company,
            new ImportReadOptions { StartRow = 9, CareerGroupId = 1 });
    }

    private static async Task VerifyVersioningAsync(IrmDbContext db)
    {
        await SeedAsync(db);
        var store = Store(db); var row = LegacyImportEndToEndTests.ValidRow();
        var first = await store.ExecuteImportAsync(await Preview(db, row), [], 1, "first.xlsx");
        Assert.True(first.Success, string.Join(";", first.Errors));
        var original = await db.Employees.SingleAsync();
        var originalId = original.IDEmployee;
        Assert.Equal("unchanged", Assert.Single(await Preview(db, row)).Status);
        row["WorkPermit"] = "Khác";
        row["StaffName"] = "Changed user";
        var changed = await Preview(db, row);
        Assert.Equal("changed", changed[0].Status);
        var update = await store.ExecuteImportAsync(changed, new() { [originalId] = "update" }, 1, "update.xlsx");
        Assert.True(update.Success, string.Join(";", update.Errors));
        Assert.NotEqual(first.SessionId, update.SessionId);
        db.ChangeTracker.Clear();
        var versions = await db.Employees.OrderBy(e => e.IDEmployee).ToListAsync();
        Assert.Equal(2, versions.Count);
        Assert.Equal(1, versions[0].Hidden_flag);
        Assert.Equal("TEST USER", versions[0].StaffName);
        Assert.Equal(0, versions[1].Hidden_flag);
        Assert.Equal(14, versions[1].IDCareer);
        var company = await db.Companies.FindAsync(1);
        Assert.Equal(1, company!.TotalAmount);
        Assert.Equal(1, company.NumberOfPersonalities);
        Assert.Equal(0, company.QuantityAvailable);
        Assert.True(await store.RollbackImportAsync(update.SessionId));
        db.ChangeTracker.Clear();
        Assert.Equal(0, (await db.Employees.FindAsync(originalId))!.Hidden_flag);
        Assert.Single(await db.Employees.ToListAsync());
        company = await db.Companies.FindAsync(1);
        Assert.Equal(1, company!.QuantityAvailable);
        Assert.Equal(0, company.NumberOfPersonalities);
        Assert.False(await store.RollbackImportAsync(update.SessionId));

        // Another company never causes a duplicate in the selected company.
        var company2 = await db.Companies.Where(c => c.IDCompany != 1).Select(c => c.IDCompany).FirstAsync();
        var other = await Preview(db, LegacyImportEndToEndTests.ValidRow(), company2);
        Assert.Equal("new", other[0].Status);
        Assert.Contains("Nhân viên đang làm ở một công ty khác", other[0].Warnings);
        var otherResult = await store.ExecuteImportAsync(other, [], company2, "other.xlsx");
        Assert.True(otherResult.Success);
        Assert.Equal(0, (await db.Employees.FindAsync(originalId))!.Hidden_flag);

        // Hidden versions in the target company do not count as a duplicate.
        var secondEmployee = await db.Employees.SingleAsync(e => e.IDCompany == company2);
        secondEmployee.Hidden_flag = 1; await db.SaveChangesAsync();
        Assert.Equal("new", Assert.Single(await Preview(db, LegacyImportEndToEndTests.ValidRow(), company2)).Status);
        // Verify all five counters independently, including departure-day and expiry boundaries.
        foreach (var e in await db.Employees.ToListAsync()) e.Hidden_flag = 1;
        for (int permit = 0; permit <= 6; permit++)
            db.Employees.Add(new() { StaffName = "Counter " + permit, Passport = "COUNT-" + permit, IDCompany = 1,
                IDCareer = 1, Nationality = "CHN", WorkPermit = (WorkPermitType)permit, TemporaryStay = DateTime.Today });
        db.Employees.Add(new() { StaffName = "Expired", IDCompany = 1, IDCareer = 1, TemporaryStay = DateTime.Today.AddDays(-1) });
        db.Employees.Add(new() { StaffName = "Left", IDCompany = 1, IDCareer = 1, TemporaryStay = DateTime.Today.AddDays(1), DateOfLeave = DateTime.Today });
        await db.SaveChangesAsync();
        await store.RecomputeCompanyCountersAsync(1);
        company = await db.Companies.FindAsync(1);
        Assert.Equal(7, company!.TotalAmount);
        Assert.Equal(2, company.AmountOfExemption);
        Assert.Equal(2, company.QuantityAvailable);
        Assert.Equal(2, company.QuantityNotYet);
        Assert.Equal(1, company.NumberOfPersonalities);
    }

    [Fact]
    public async Task CreateCompany_RequiresAnExplicitOptIn_AndExactNameMatch()
    {
        await using var database = new TestDatabase(); await database.InitializeAsync();
        await SeedAsync(database.Context);
        var store = Store(database.Context); var row = LegacyImportEndToEndTests.ValidRow();
        using var workbook = new XLWorkbook(); var sheet = workbook.AddWorksheet("Import");
        int c = 1;
        foreach (var item in row.Where(x => x.Key != "__RowNumber")) {
            sheet.Cell(1,c).Value = item.Key; sheet.Cell(2,c).Value = item.Value; c++;
        }
        sheet.Cell(1,c).Value = "CompanyName"; sheet.Cell(2,c).Value = "Default Test";
        var mappings = LegacyImportEndToEndTests.Mappings(); mappings.Add(new() { ExcelColumnIndex = c-1, SystemField = "CompanyName" });
        using var stream = new MemoryStream(); workbook.SaveAs(stream);
        var denied = await store.GeneratePreviewAsync(stream,mappings,1,new() { CareerGroupId = 1 });
        Assert.Equal("error", denied[0].Status);
        var allowed = await store.GeneratePreviewAsync(stream,mappings,1,new() { CareerGroupId = 1, AllowCreateCompanies = true });
        Assert.Equal("new", allowed[0].Status);
        // Dropping the opt-in must also block the write, even if preview had allowed it.
        allowed[0].ExtendedFields.Remove("__AllowCreateCompany");
        var deniedWrite = await store.ExecuteImportAsync(allowed,[],1,"companies.xlsx");
        Assert.False(deniedWrite.Success);
        Assert.Equal(2, await database.Context.Companies.CountAsync());
        allowed[0].ExtendedFields["__AllowCreateCompany"] = "true";
        var accepted = await store.ExecuteImportAsync(allowed, [], 1, "companies.xlsx");
        Assert.True(accepted.Success, string.Join(";", accepted.Errors));
        Assert.Equal(3, await database.Context.Companies.CountAsync());
        var created = await database.Context.Companies.SingleAsync(c => c.CompanyName == "Default Test");
        Assert.Equal(created.IDCompany, (await database.Context.Employees.SingleAsync()).IDCompany);
    }
}

internal sealed class IsolatedImportSqlDatabase : IAsyncDisposable
{
    public IrmDbContext Context { get; }
    private IsolatedImportSqlDatabase(IrmDbContext context) => Context = context;
    public static async Task<IsolatedImportSqlDatabase> CreateAsync()
    {
        var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("IRM_TEST_SQLSERVER")!) {
            InitialCatalog = "IRM_RefactorTest_" + Guid.NewGuid().ToString("N"), TrustServerCertificate = true
        };
        var context = new IrmDbContext(new DbContextOptionsBuilder<IrmDbContext>().UseSqlServer(connection.ConnectionString,
            sql => sql.UseCompatibilityLevel(120)).Options);
        await context.Database.EnsureCreatedAsync();
        return new(context);
    }
    public async ValueTask DisposeAsync()
    {
        var name = Context.Database.GetDbConnection().Database;
        if (!name.StartsWith("IRM_RefactorTest_", StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe test database");
        await Context.Database.EnsureDeletedAsync(); await Context.DisposeAsync();
    }
}
