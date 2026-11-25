using ClosedXML.Excel;
using IRM.Application.Features.Administration.Contracts;
using IRM.Data;
using IRM.Data.Models;
using IRM.Infrastructure.Excel;
using IRM.Infrastructure.Persistence.Stores.Reports;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class OfflineSqlServerHardeningTests
{
    private static string ResolveSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "IRM.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }

    [Fact]
    public void MakeEmployeeCompanyNullable_DropsAndRecreatesIxEmployeesIdCompanyAroundAlterColumn()
    {
        var migrationPath = Path.Combine(
            ResolveSolutionRoot(),
            "IRM.Infrastructure", "Persistence", "Migrations",
            "20261122093015_MakeEmployeeCompanyNullable.cs");
        var code = File.ReadAllText(migrationPath);

        var upStart = code.IndexOf("protected override void Up", StringComparison.Ordinal);
        var downStart = code.IndexOf("protected override void Down", StringComparison.Ordinal);
        Assert.True(upStart > 0 && downStart > upStart);

        var upBlock = code[upStart..downStart];
        var downBlock = code[downStart..];

        foreach (var block in new[] { upBlock, downBlock })
        {
            var dropIdx = block.IndexOf("DropIndex", StringComparison.Ordinal);
            var alterIdx = block.IndexOf("name: \"IDCompany\",\r\n                table: \"Employees\"", StringComparison.Ordinal);
            if (alterIdx < 0)
            {
                alterIdx = block.IndexOf("name: \"IDCompany\",\n                table: \"Employees\"", StringComparison.Ordinal);
            }
            var createIdx = block.IndexOf("CreateIndex", StringComparison.Ordinal);

            Assert.True(dropIdx > 0, "Migration must DropIndex IX_Employees_IDCompany before AlterColumn on SQL Server.");
            Assert.True(alterIdx > dropIdx, "DropIndex must appear before AlterColumn on Employees.IDCompany.");
            Assert.True(createIdx > alterIdx, "CreateIndex must appear after AlterColumn on Employees.IDCompany.");
            Assert.Contains("IX_Employees_IDCompany", block, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task SqlStatisticsStore_GetTerritoryAsync_SucceedsWhenActiveCompanyAccommodationAgreementExists()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();

        var unit = await db.Context.AdministrativeUnits.FirstAsync();
        var field = await db.Context.Fields.FirstAsync();
        var account = await db.Context.Accounts.FirstAsync();
        var company = new Company { CompanyName = "Công ty TNHH Alpha", IDField = field.IDField, TrackerID = account.IDUser, Delete_flag = 0 };
        db.Context.Companies.Add(company);
        await db.Context.SaveChangesAsync();

        var acc = new Accommodation
        {
            Name = "KTX Alpha",
            TypeCode = "DORMITORY",
            AddressLine = "Số 10",
            AdministrativeUnitId = unit.Id,
            IsDeleted = false
        };
        db.Context.Accommodations.Add(acc);
        await db.Context.SaveChangesAsync();

        db.Context.CompanyAccommodationAgreements.Add(new CompanyAccommodationAgreement
        {
            CompanyId = company.IDCompany,
            AccommodationId = acc.Id,
            ScopeCode = "FULL",
            ValidFrom = DateTime.Today.AddDays(-10)
        });
        await db.Context.SaveChangesAsync();

        var audit = new SqlAuditStore(db.Context);
        var statsStore = new SqlStatisticsStore(db.Context, audit);

        var result = await statsStore.GetTerritoryAsync(new TemporalReportFilter
        {
            AdministrativeUnitId = unit.Id,
            AsOfDate = DateTime.Today
        });

        var row = Assert.Single(result.Accommodations);
        Assert.Equal("Công ty TNHH Alpha", row.Companies);
    }

    [Fact]
    public async Task SqlCatalogStore_ImportAdministrativeUnitsExcel_RejectsCodeOver450CharsAndHandlesDuplicateDates()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: false);
        var store = new SqlCatalogStore(db.Context);

        // Two legacy rows with same Code and same calendar date (different time component)
        db.Context.AdministrativeUnits.AddRange(
            new AdministrativeUnit { Code = "DUP-TIME", Name = "Cũ 1", TypeCode = AdministrativeUnitTypeCodes.Ward, ValidFrom = new DateTime(2025, 7, 1, 0, 0, 0) },
            new AdministrativeUnit { Code = "DUP-TIME", Name = "Cũ 2", TypeCode = AdministrativeUnitTypeCodes.Ward, ValidFrom = new DateTime(2025, 7, 1, 8, 0, 0), IsDeleted = true });
        await db.Context.SaveChangesAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("DiaBan");
        ws.Cell(1, 1).Value = "Mã địa bàn";
        ws.Cell(1, 2).Value = "Tên địa bàn";
        ws.Cell(1, 3).Value = "Loại";
        ws.Cell(1, 4).Value = "Hiệu lực từ";

        // Row 2: code > 450 chars -> must be flagged as Error (prevent SQL Server truncation)
        ws.Cell(2, 1).Value = new string('A', 451);
        ws.Cell(2, 2).Value = "Mã quá dài";
        ws.Cell(2, 3).Value = "Phường";
        ws.Cell(2, 4).Value = "01/07/2025";

        // Row 3: matches DUP-TIME -> must NOT crash ToDictionary
        ws.Cell(3, 1).Value = "DUP-TIME";
        ws.Cell(3, 2).Value = "Phường Đã Chuẩn Hóa";
        ws.Cell(3, 3).Value = "Phường";
        ws.Cell(3, 4).Value = "01/07/2025";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var result = await store.ImportAdministrativeUnitsExcelAsync(ms, "long-code.xlsx");

        Assert.Equal(1, result.SkippedErrorCount);
        Assert.Equal(1, result.UpdatedCount);
        Assert.Contains(result.Errors, e => e.Contains("450", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ElectronicIdentity_NonAlphanumericInput_DoesNotInsertNullSearchKey()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();
        var audit = new SqlAuditStore(db.Context);
        var store = new SqlFamilyVisitorStore(db.Context, audit);

        var person = new ForeignPerson
        {
            FullName = "TEST USER",
            PassportNumber = "P998877",
            PassportSearchKey = IRM.Core.Rules.PassportRules.SearchKey("P998877")!
        };
        db.Context.ForeignPersons.Add(person);
        await db.Context.SaveChangesAsync();

        // Direct SaveElectronicIdentityAsync must reject "---" cleanly with ArgumentException
        await Assert.ThrowsAsync<ArgumentException>(() => store.SaveElectronicIdentityAsync(new ElectronicIdentity
        {
            ForeignPersonId = person.Id,
            IdentityNumber = "---",
            ValidFrom = DateTime.Today
        }));

        // Unified save with non-alphanumeric EID must skip invalid EID rather than inserting NULL IdentitySearchKey
        var savedId = await store.SaveUnifiedProfileAsync(new UnifiedForeignPersonInput
        {
            PassportNumber = "P998878",
            FullName = "TEST EID",
            ElectronicIdentityNumber = "---",
            HasWorkProfile = true,
            CompanyId = 0 // Also verifies CompanyId=0 is normalized to null (no FK violation)
        });

        Assert.True(savedId > 0);
        var eids = await db.Context.ElectronicIdentities.Where(e => e.ForeignPersonId == savedId).ToListAsync();
        Assert.Empty(eids);
        var stay = await db.Context.StayCases.SingleAsync(s => s.ForeignPersonId == savedId);
        Assert.Null(stay.SponsorCompanyId);
    }
}
