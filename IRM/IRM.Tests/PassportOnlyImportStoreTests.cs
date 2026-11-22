using ClosedXML.Excel;
using IRM.Application.Features.ImportExport;
using IRM.Data.Models;
using IRM.Infrastructure.Persistence.Stores.Administration;
using IRM.Infrastructure.Persistence.Stores.ImportExport;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class PassportOnlyImportStoreTests
{
    private static MemoryStream BuildWorkbook(string[] headers, params string[][] rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Sheet1");
        for (var c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];
        for (var r = 0; r < rows.Length; r++)
            for (var c = 0; c < rows[r].Length; c++)
                ws.Cell(r + 2, c + 1).Value = rows[r][c];
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static List<ColumnMapping> Map(params (int Index, string Field)[] cols) =>
        cols.Select(c => new ColumnMapping { ExcelColumnIndex = c.Index - 1, SystemField = c.Field }).ToList();

    [Fact]
    public async Task PreviewAndExecute_OnlyPassportNoCompanyNoCareerGroup_CommitsWithNullCompanyAndWarnings()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        // Add a second company so effectiveCompanyId does not auto-default to single company
        td.Context.Companies.AddRange(
            new Company { CompanyName = "Cty A", IDField = 1, TrackerID = 1 },
            new Company { CompanyName = "Cty B", IDField = 1, TrackerID = 1 });
        await td.Context.SaveChangesAsync();

        var store = new SqlImportStore(td.Context, new SqlAuditStore(td.Context));
        using var stream = BuildWorkbook(["Số hộ chiếu"], ["PP-ONLY-01"]);

        var preview = await store.GeneratePreviewAsync(stream, Map((1, "Passport")), companyId: 0, new ImportReadOptions { HeaderRow = 1, StartRow = 2, CareerGroupId = 0 });

        Assert.Single(preview);
        Assert.Equal("new", preview[0].Status);
        Assert.Null(preview[0].ResolvedCompanyId);
        Assert.NotEmpty(preview[0].Warnings);
        Assert.Contains(preview[0].Warnings, w => w.Contains("công ty", StringComparison.OrdinalIgnoreCase));

        var result = await store.ExecuteImportAsync(preview, new Dictionary<int, string>(), companyId: 0, "passport-only.xlsx", "admin");
        Assert.True(result.Success);
        Assert.Equal(1, result.AddedCount);

        var saved = await td.Context.Employees.SingleAsync(e => e.Passport == "PP-ONLY-01");
        Assert.Null(saved.IDCompany);
        var history = await td.Context.ImportHistories.SingleAsync(h => h.SessionId == result.SessionId);
        Assert.Null(history.CompanyId);
    }

    [Fact]
    public async Task Preview_SecondImportWithNullCompany_MatchesExistingPassportInsteadOfNew()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        td.Context.Companies.AddRange(
            new Company { CompanyName = "Cty A", IDField = 1, TrackerID = 1 },
            new Company { CompanyName = "Cty B", IDField = 1, TrackerID = 1 });
        td.Context.Employees.Add(new Employee { StaffName = "", Passport = "PP-NULL-02", IDCompany = null, IDUser = 1, Hidden_flag = 0, WorkingStatus = 0 });
        await td.Context.SaveChangesAsync();

        var store = new SqlImportStore(td.Context, new SqlAuditStore(td.Context));
        using var stream = BuildWorkbook(["Số hộ chiếu", "Họ và tên"], ["PP-NULL-02", "NGUYEN VAN B"]);

        var preview = await store.GeneratePreviewAsync(stream, Map((1, "Passport"), (2, "StaffName")), companyId: 0, new ImportReadOptions { HeaderRow = 1, StartRow = 2, CareerGroupId = 0 });

        Assert.Single(preview);
        Assert.Equal("changed", preview[0].Status);
        Assert.NotNull(preview[0].ExistingEmployeeId);
    }

    [Fact]
    public async Task Preview_InvalidV010PurposeCodeOrPartialResidence_FlagsErrorAndWarningInPreview()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        var store = new SqlImportStore(td.Context, new SqlAuditStore(td.Context));
        using var stream = BuildWorkbook(
            ["Số hộ chiếu", "Diện cư trú", "Địa chỉ lưu trú"],
            ["PP-V010-ERR", "INVALID_CODE", ""],
            ["PP-V010-WARN", "", "123 Đường ABC"]);

        var preview = await store.GeneratePreviewAsync(
            stream,
            Map((1, "Passport"), (2, "StayPurposeCode"), (3, "ResidenceAddress")),
            companyId: 0,
            new ImportReadOptions { HeaderRow = 1, StartRow = 2, CareerGroupId = 0 });

        Assert.Equal(2, preview.Count);
        Assert.Equal("error", preview[0].Status);
        Assert.Contains("Diện cư trú", preview[0].ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.Equal("new", preview[1].Status);
        Assert.Contains(preview[1].Warnings, w => w.Contains("địa bàn", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Preview_UpdateBlankNoteOverExistingNote_EmitsOverwriteWarning()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        var company = new Company { CompanyName = "Cty Test", IDField = 1, TrackerID = 1 };
        td.Context.Companies.Add(company);
        await td.Context.SaveChangesAsync();

        td.Context.Employees.Add(new Employee
        {
            StaffName = "USER OLD",
            Passport = "PP-NOTE-01",
            IDCompany = company.IDCompany,
            Note = "Ghi chú quan trọng cũ",
            IDUser = 1,
            Hidden_flag = 0,
            WorkingStatus = 0
        });
        await td.Context.SaveChangesAsync();

        var store = new SqlImportStore(td.Context, new SqlAuditStore(td.Context));
        using var stream = BuildWorkbook(["Số hộ chiếu", "Họ và tên"], ["PP-NOTE-01", "USER NEW"]);

        var preview = await store.GeneratePreviewAsync(
            stream,
            Map((1, "Passport"), (2, "StaffName")),
            companyId: company.IDCompany,
            new ImportReadOptions { HeaderRow = 1, StartRow = 2, CareerGroupId = 0 });

        Assert.Single(preview);
        Assert.Equal("changed", preview[0].Status);
        Assert.Contains(preview[0].Warnings, w => w.Contains("Ghi chú", StringComparison.OrdinalIgnoreCase));
    }
}
