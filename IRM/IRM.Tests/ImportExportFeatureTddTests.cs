using IRM.Data;
using IRM.Data.Models;
using IRM.Services;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class ImportExportFeatureTddTests
{
    [Fact]
    public async Task ExecuteImportAsync_RejectsMissingCompanyBeforeSaving()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var service = new SqlImportStore(db.Context, new SqlAuditStore(db.Context), TestSecurityContext.CreateAllowAllGuard());

        var rows = new List<ImportPreviewRow>
        {
            new()
            {
                RowNumber = 2,
                Status = "new",
                ParsedEmployee = new Employee
                {
                    StaffName = "No Company Test",
                    Passport = "NO-COMPANY-001",
                    IDCompany = 0,
                    IDCareer = 1,
                    Nationality = "USA"
                }
            }
        };

        var result = await service.ExecuteImportAsync(rows, [], companyId: 0, "missing-company.xlsx", "admin");

        Assert.False(result.Success);
        Assert.Equal(0, result.AddedCount);
        Assert.Equal(1, result.ErrorCount);
        Assert.Contains(result.Errors, error => error.Contains("Công ty", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(db.Context.Employees, employee => employee.Passport == "NO-COMPANY-001");
        Assert.Empty(db.Context.ImportHistories);
        Assert.Empty(db.Context.ImportBackups);
    }

    [Fact]
    public async Task GeneratePreviewAsync_RejectsRowsWhenNoDefaultCompanyIsSelected()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        db.Context.Companies.RemoveRange(db.Context.Companies);
        await db.Context.SaveChangesAsync();
        var service = new SqlImportStore(db.Context, new SqlAuditStore(db.Context), TestSecurityContext.CreateAllowAllGuard());

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Mẫu Import");
        sheet.Cell(1, 1).Value = "Họ tên";
        sheet.Cell(1, 2).Value = "Số hộ chiếu";
        sheet.Cell(2, 1).Value = "TEST USER";
        sheet.Cell(2, 2).Value = "TEST-PASSPORT-001";
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var mappings = service.AutoMapColumns(ExcelReaderHelper.ReadHeaders(stream));
        var rows = await service.GeneratePreviewAsync(stream, mappings, companyId: 0);

        var row = Assert.Single(rows);
        Assert.Equal("error", row.Status);
        Assert.Contains("Chưa chọn công ty", row.ErrorMessage);
    }

    [Fact]
    public async Task GeneratePreviewAsync_AutoSelectsTheOnlyActiveCompany()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        var service = new SqlImportStore(db.Context, new SqlAuditStore(db.Context), TestSecurityContext.CreateAllowAllGuard());

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Mẫu Import");
        sheet.Cell(1, 1).Value = "Họ tên";
        sheet.Cell(1, 2).Value = "Số hộ chiếu";
        sheet.Cell(2, 1).Value = "TEST USER";
        sheet.Cell(2, 2).Value = "TEST-PASSPORT-002";
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var mappings = service.AutoMapColumns(ExcelReaderHelper.ReadHeaders(stream));
        var rows = await service.GeneratePreviewAsync(stream, mappings, companyId: 0);

        var row = Assert.Single(rows);
        Assert.Equal("new", row.Status);
        Assert.Equal(1, row.ResolvedCompanyId);
        Assert.Equal("Default Test Company", row.CompanyDisplay);
    }

    [Fact]
    public void AutoMapColumns_ResolvesVietnameseAndEnglishSynonyms()
    {
        var db = new IrmDbContext(new DbContextOptionsBuilder<IrmDbContext>().UseSqlite("Data Source=:memory:").Options);
        var service = new SqlImportStore(db, new SqlAuditStore(db));

        var headers = new List<ExcelColumnInfo>
        {
            new() { Index = 1, HeaderName = "Họ và tên" },
            new() { Index = 2, HeaderName = "Số Hộ Chiếu" },
            new() { Index = 3, HeaderName = "Passport Number" },
            new() { Index = 4, HeaderName = "Ngày sinh" },
            new() { Index = 5, HeaderName = "Tên Công ty" },
            new() { Index = 6, HeaderName = "Cột không liên quan" }
        };

        var mappings = service.AutoMapColumns(headers);

        Assert.Equal("StaffName", mappings.First(x => x.ExcelColumnIndex == 1).SystemField);
        Assert.Equal("Passport", mappings.First(x => x.ExcelColumnIndex == 2).SystemField);
        Assert.Equal("Birthday", mappings.First(x => x.ExcelColumnIndex == 4).SystemField);
        Assert.Equal("CompanyName", mappings.First(x => x.ExcelColumnIndex == 5).SystemField);
        Assert.Equal("skip", mappings.First(x => x.ExcelColumnIndex == 6).SystemField);
    }

    [Fact]
    public async Task ExecuteImportAsync_CreatesLegacyAndV010ExtendedEntities_AndRollsBackCleanly()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync(seedBaseline: true);
        await DatabaseSeeder.SeedV010Async(db.Context);

        var audit = new SqlAuditStore(db.Context);
        var importService = new SqlImportStore(db.Context, audit, TestSecurityContext.CreateAllowAllGuard());

        var company = new Company { CompanyName = "Import Test Corp", IDField = 1, TrackerID = 1 };
        db.Context.Companies.Add(company);
        await db.Context.SaveChangesAsync();

        var previewRow = new ImportPreviewRow
        {
            RowNumber = 1,
            Status = "new",
            ParsedEmployee = new Employee
            {
                StaffName = "Nguyen Van Test",
                Passport = "VN-9999",
                IDCompany = company.IDCompany,
                IDCareer = 1,
                Nationality = "USA",
                WorkingStatus = 0
            },
            ExtendedFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["StayPurposeCode"] = StayPurposeCodes.Work,
                ["StayValidFrom"] = "2026-01-01",
                ["StayValidTo"] = "2026-12-31",
                ["ElectronicIdentityNumber"] = "EID-VN-001",
                ["DocumentTypeCode"] = ImmigrationDocumentTypeCodes.Visa,
                ["DocumentNumber"] = "V-8888",
                ["AdministrativeUnitCode"] = "QN-W-001",
                ["ResidenceAddress"] = "Khu 1, Bãi Cháy",
                ["ResidenceValidFrom"] = "2026-01-01"
            }
        };

        var result = await importService.ExecuteImportAsync([previewRow], [], company.IDCompany, "import_test.xlsx", "admin");
        Assert.True(result.Success);
        Assert.Equal(1, result.AddedCount);

        // Verify Legacy Employee
        Assert.Contains(db.Context.Employees, x => x.StaffName == "Nguyen Van Test");

        // Verify V010 Entities
        Assert.Single(db.Context.ForeignPersons);
        Assert.Single(db.Context.StayCases);
        Assert.Single(db.Context.ElectronicIdentities);
        Assert.Single(db.Context.ImmigrationDocuments);
        Assert.Single(db.Context.ResidencePeriods);

        // Verify Rollback
        var rollbackSuccess = await importService.RollbackImportAsync(result.SessionId);
        Assert.True(rollbackSuccess);

        // Legacy employee is soft-deleted
        Assert.Equal(1, db.Context.Employees.First(x => x.StaffName == "Nguyen Van Test").Hidden_flag);

        // V010 created entities removed
        Assert.Empty(db.Context.ForeignPersons);
        Assert.Empty(db.Context.StayCases);
        Assert.Empty(db.Context.ElectronicIdentities);
        Assert.Empty(db.Context.ImmigrationDocuments);
        Assert.Empty(db.Context.ResidencePeriods);
    }

    [Theory]
    [InlineData("=SUM(A1:A10)", "'=SUM(A1:A10)")]
    [InlineData("@HYPERLINK(\"http://evil.com\")", "'@HYPERLINK(\"http://evil.com\")")]
    [InlineData("+1+1", "'+1+1")]
    [InlineData("-2+3", "'-2+3")]
    [InlineData("Normal text", "Normal text")]
    public void StatisticsExportService_SanitizesFormulaInjection(string input, string expected)
    {
        var sanitized = SqlStatisticsExportStore.Safe(input);
        Assert.Equal(expected, sanitized);
    }
}
