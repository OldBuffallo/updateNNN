using ClosedXML.Excel;
using IRM.Core.Rules;
using IRM.Data;
using IRM.Data.Models;
using IRM.Infrastructure.Excel;
using IRM.Infrastructure.LegacySync;
using IRM.Infrastructure.Persistence.Stores.Administration;
using IRM.Infrastructure.Persistence.Stores.ForeignPersons;
using IRM.Infrastructure.Persistence.Stores.Reports;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class PassportOnlyDownstreamTests
{
    [Fact]
    public void PersonDisplayHelper_FormatName_ReturnsFallbackWithPassportWhenNameBlank()
    {
        Assert.Equal("Chưa có tên (P123456)", PersonDisplayHelper.FormatName("", "P123456"));
        Assert.Equal("Chưa có tên (P123456)", PersonDisplayHelper.FormatName("   ", " P123456 "));
        Assert.Equal("NGUYEN VAN A", PersonDisplayHelper.FormatName(" NGUYEN VAN A ", "P123456"));
        Assert.Equal("Chưa có tên", PersonDisplayHelper.FormatName("", null));
    }

    [Fact]
    public async Task DashboardStore_NationalityStats_IncludesNullNationalityUnderChuaRo()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        td.Context.Employees.AddRange(
            new Employee { StaffName = "", Passport = "PP-DASH-1", Nationality = null, IDCompany = null, IDUser = 1, Hidden_flag = 0, WorkingStatus = 0 },
            new Employee { StaffName = "USER USA", Passport = "PP-DASH-2", Nationality = "USA", IDCompany = null, IDUser = 1, Hidden_flag = 0, WorkingStatus = 0 });
        await td.Context.SaveChangesAsync();

        var store = new SqlDashboardStore(td.Context);
        var data = await store.GetDashboardAsync();

        Assert.Equal(2, data.TotalEmployees);
        Assert.Contains(data.NationalityStats, x => x.Label == "Chưa rõ" && x.Value == 1);
        Assert.Equal(2, data.NationalityStats.Sum(x => x.Value));
    }

    [Fact]
    public async Task ExportStore_GroupByNationality_DoesNotFragmentKhacHeadersAndExcludesHidden()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        var c1 = new Company { CompanyName = "A Company", IDField = 1, TrackerID = 1 };
        var c2 = new Company { CompanyName = "Z Company", IDField = 1, TrackerID = 1 };
        td.Context.Companies.AddRange(c1, c2);
        await td.Context.SaveChangesAsync();

        td.Context.Employees.AddRange(
            new Employee { StaffName = "E1", Passport = "PP-EXP-1", Nationality = null, IDCompany = c1.IDCompany, IDUser = 1, Hidden_flag = 0, WorkingStatus = 0 },
            new Employee { StaffName = "E2", Passport = "PP-EXP-2", Nationality = "USA", IDCompany = c1.IDCompany, IDUser = 1, Hidden_flag = 0, WorkingStatus = 0 },
            new Employee { StaffName = "E3", Passport = "PP-EXP-3", Nationality = null, IDCompany = c2.IDCompany, IDUser = 1, Hidden_flag = 0, WorkingStatus = 0 },
            new Employee { StaffName = "HIDDEN", Passport = "PP-HID", Nationality = "USA", IDCompany = c1.IDCompany, IDUser = 1, Hidden_flag = 1, WorkingStatus = 0 });
        await td.Context.SaveChangesAsync();

        var store = new SqlExportStore(new FixedDbContextFactory(td.Context));
        var reportBytes = await store.ExportReportAsync(["Họ tên", "Hộ chiếu", "Quốc tịch"], groupBy: "nationality");
        using (var wb = new XLWorkbook(new MemoryStream(reportBytes)))
        {
            var ws = wb.Worksheet(1);
            var headers = ws.RowsUsed().Select(r => r.Cell(1).GetString()).Where(s => s.StartsWith("🏢")).ToList();
            Assert.Equal(1, headers.Count(h => h.Contains("Khác", StringComparison.OrdinalIgnoreCase) || h.Contains("Chưa rõ", StringComparison.OrdinalIgnoreCase)));
        }

        var empBytes = await store.ExportEmployeesAsync();
        using (var wb = new XLWorkbook(new MemoryStream(empBytes)))
        {
            var ws = wb.Worksheet(1);
            var allText = string.Join(" ", ws.RowsUsed().Select(r => r.Cell(2).GetString()));
            Assert.DoesNotContain("HIDDEN", allText);
        }
    }

    [Fact]
    public async Task LegacySyncService_EmptyStaffName_ProcessesWithoutDeadLetter()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        td.Context.LegacyChangeEvents.Add(new LegacyChangeEvent
        {
            EntityType = "Employees",
            EntityId = "999",
            StatusCode = "PENDING",
            AfterXml = "<row StaffName=\"\" Passport=\"PP-SYNC-1\" />"
        });
        await td.Context.SaveChangesAsync();

        var registry = new SqlFamilyVisitorStore(td.Context, new SqlAuditStore(td.Context));
        var sync = new LegacySyncService(td.Context, registry);
        var count = await sync.ProcessPendingAsync(10);

        Assert.Equal(1, count);
        var ev = await td.Context.LegacyChangeEvents.SingleAsync();
        Assert.Equal("PROCESSED", ev.StatusCode);
    }

    private sealed class FixedDbContextFactory(IrmDbContext db) : IDbContextFactory<IrmDbContext>
    {
        private readonly System.Data.Common.DbConnection _connection = db.Database.GetDbConnection();
        public IrmDbContext CreateDbContext() => new(new DbContextOptionsBuilder<IrmDbContext>().UseSqlite(_connection).Options);
        public Task<IrmDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }
}
