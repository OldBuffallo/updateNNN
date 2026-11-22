using ClosedXML.Excel;
using IRM.Application.Features.ImportExport;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class NationalityImportTests
{
    [Theory]
    [InlineData("ZZZ")]
    [InlineData("China")]
    [InlineData("Quốc tịch chưa khai báo")]
    public async Task Preview_RejectsUnknownNationalityBeforeSqlWrite(string nationality)
    {
        await using var db = new TestDatabase();
        await InitializeCatalogAsync(db);
        var service = CreateService(db);
        using var stream = CreateWorkbook(nationality);

        var rows = await service.GeneratePreviewAsync(stream,
            service.AutoMapColumns(ExcelReaderHelper.ReadHeaders(stream)), 1);

        var row = Assert.Single(rows);
        Assert.Equal("error", row.Status);
        Assert.Contains(nationality, row.ErrorMessage);
        Assert.Contains("Quốc tịch", row.ErrorMessage);
        Assert.Empty(db.Context.Employees);
    }

    [Theory]
    [InlineData(" chn ")]
    [InlineData("CN")]
    [InlineData("Trung Quốc")]
    [InlineData("  Trung\u00a0Quo\u0302\u0301c  ")]
    public async Task PreviewAndExecute_UseExistingCatalogKey(string nationality)
    {
        await using var db = new TestDatabase();
        await InitializeCatalogAsync(db);
        var service = CreateService(db);
        using var stream = CreateWorkbook(nationality);
        var rows = await service.GeneratePreviewAsync(stream,
            service.AutoMapColumns(ExcelReaderHelper.ReadHeaders(stream)), 1);

        var row = Assert.Single(rows);
        Assert.Equal("new", row.Status);
        Assert.Equal("CHN", row.ParsedEmployee!.Nationality);
        var result = await service.ExecuteImportAsync(rows, [], 1, "nationality.xlsx");

        Assert.True(result.Success, string.Join("; ", result.Errors));
        db.Context.ChangeTracker.Clear();
        Assert.Equal("CHN", (await db.Context.Employees.SingleAsync()).Nationality);
    }

    [Fact]
    public async Task Execute_RevalidatesCatalogAndLeavesBatchUnwritten()
    {
        await using var db = new TestDatabase();
        await InitializeCatalogAsync(db);
        var service = CreateService(db);
        using var stream = CreateWorkbook("CHN");
        var rows = await service.GeneratePreviewAsync(stream,
            service.AutoMapColumns(ExcelReaderHelper.ReadHeaders(stream)), 1);
        var nationality = await db.Context.Nationality.SingleAsync(n => n.NationalityCode == "CHN");
        nationality.Delete_flag = 1;
        await db.Context.SaveChangesAsync();
        rows.Insert(0, new ImportPreviewRow { RowNumber = 1, Status = "new",
            ParsedEmployee = new Employee { StaffName = "Valid row", IDCompany = 1 } });

        var result = await service.ExecuteImportAsync(rows, [], 1, "changed-catalog.xlsx");

        Assert.False(result.Success);
        Assert.Contains(result.Errors, error => error.Contains("Quốc tịch") && error.Contains("CHN"));
        Assert.Contains(result.Errors, error => error.Contains($"Dòng {rows[1].RowNumber}:"));
        db.Context.ChangeTracker.Clear();
        Assert.Empty(await db.Context.Employees.ToListAsync());
        Assert.Empty(await db.Context.ImportBackups.ToListAsync());
        Assert.Empty(await db.Context.ImportHistories.ToListAsync());
    }

    [Fact]
    public async Task Preview_RejectsDeletedCatalogEntry()
    {
        await using var db = new TestDatabase();
        await InitializeCatalogAsync(db);
        var nationality = await db.Context.Nationality.SingleAsync(n => n.NationalityCode == "CHN");
        nationality.Delete_flag = 1;
        await db.Context.SaveChangesAsync();
        var service = CreateService(db);
        using var stream = CreateWorkbook("CHN");

        var rows = await service.GeneratePreviewAsync(stream,
            service.AutoMapColumns(ExcelReaderHelper.ReadHeaders(stream)), 1);

        Assert.Equal("error", Assert.Single(rows).Status);
    }

    private static SqlImportStore CreateService(TestDatabase db) =>
        new(db.Context, new SqlAuditStore(db.Context), TestSecurityContext.CreateAllowAllGuard());

    [Fact]
    public void NationalityResolution_PreservesLegacyKeysAndOptionalBlankValues()
    {
        List<NationalityEntity> catalog = [new() { NationalityCode = "LEGACY-CN", NationalityName = "Trung Quốc" }];
        Assert.Equal("LEGACY-CN", ImportMapping.ResolveNationalityCode("Trung Quốc", catalog));
        Assert.Equal("LEGACY-CN", ImportMapping.ResolveNationalityCode("legacy-cn", catalog));
        Assert.Null(ImportMapping.ResolveNationalityCode("  ", catalog));
        Assert.Null(ImportMapping.ResolveNationalityCode(null, catalog));
        Assert.Throws<InvalidOperationException>(() => ImportMapping.ResolveNationalityCode("CN", catalog));
    }

    [Theory]
    [InlineData("CHN", true)]
    [InlineData("usa", true)]
    [InlineData(" Kor ", true)]
    [InlineData("CN", false)]
    [InlineData("VN", false)]
    [InlineData("US_CUSTOM", false)]
    [InlineData("123", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidNationalityCode_EnforcesThreeAsciiLetters(string? code, bool expected)
    {
        Assert.Equal(expected, LegacyImportRules.IsValidNationalityCode(code));
    }

    private static async Task InitializeCatalogAsync(TestDatabase db)
    {
        await db.InitializeAsync();
        // Match a 3-letter production catalog: CHN exists, unseeded codes do not.
        db.Context.Nationality.RemoveRange(await db.Context.Nationality
            .Where(n => n.NationalityCode != "CHN").ToListAsync());
        db.Context.Wards.Add(new() { WardName = "Hải Châu" });
        await db.Context.SaveChangesAsync();
    }

    private static MemoryStream CreateWorkbook(string nationality)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Import");
        sheet.Cell(1, 1).Value = "Họ tên";
        sheet.Cell(1, 2).Value = "Quốc tịch";
        sheet.Cell(2, 1).Value = "Import nationality test";
        sheet.Cell(2, 2).Value = nationality;
        string[] headers = ["Giới tính", "Ngày sinh", "Số hộ chiếu", "Địa chỉ", "Nghề nghiệp", "Số visa", "Hạn tạm trú"];
        string[] values = ["Nam", "03/06/1990", "NAT-001", "12 Lê Duẩn, Hải Châu", "Kỹ sư", "DN1", "29/05/2028"];
        for (int i = 0; i < headers.Length; i++) { sheet.Cell(1, i + 3).Value = headers[i]; sheet.Cell(2, i + 3).Value = values[i]; }
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream;
    }
}
