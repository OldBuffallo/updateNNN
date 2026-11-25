using ClosedXML.Excel;
using IRM.Application.Features.ImportExport;
using IRM.Application.Models;
using IRM.Data;
using IRM.Data.Models;
using IRM.Infrastructure.Excel;
using IRM.Infrastructure.Persistence.Stores.ImportExport;
using IRM.Services;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

public sealed class ImportVisualDiffTests
{
    private static readonly List<NationalityEntityDto> SampleNationalities =
    [
        new() { NationalityCode = "KOR", NationalityName = "Hàn Quốc" },
        new() { NationalityCode = "JPN", NationalityName = "Nhật Bản" },
        new() { NationalityCode = "CHN", NationalityName = "Trung Quốc" }
    ];

    [Fact]
    public void ExcelWorkbookConverter_Analyze_PopulatesNormalizedChangesActionIssuesAndIgnoredColumns()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Sheet1");
        ws.Cell(1, 1).Value = "So ho chieu";
        ws.Cell(1, 2).Value = "Ho ten";
        ws.Cell(1, 3).Value = "Gioi tinh";
        ws.Cell(1, 4).Value = "Ngay sinh";
        ws.Cell(1, 5).Value = "Quoc tich";
        ws.Cell(1, 6).Value = "Phong ban noi bo"; // Ignored source column

        // Row 2: auto-normalized values (Male -> Nam, 1990-06-03 -> 03/06/1990, Korea -> KOR)
        ws.Cell(2, 1).Value = " P001234 ";
        ws.Cell(2, 2).Value = "KIM MIN";
        ws.Cell(2, 3).Value = "Male";
        ws.Cell(2, 4).Value = "1990-06-03";
        ws.Cell(2, 5).Value = "Korea";
        ws.Cell(2, 6).Value = "IT";

        // Row 3: blocking issue (missing passport) + non-blocking date issue
        ws.Cell(3, 1).Value = "";
        ws.Cell(3, 2).Value = "NGUYEN B";
        ws.Cell(3, 3).Value = "Nam";
        ws.Cell(3, 4).Value = "1990/31/12";
        ws.Cell(3, 5).Value = "KOR";
        ws.Cell(3, 6).Value = "HR";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var converter = new ExcelWorkbookConverter();
        var headers = ExcelReaderHelper.ReadHeaders(ms, 0, 1);
        var selections = new List<ExcelFieldSelection>
        {
            new() { SystemField = "Passport", Label = "Số hộ chiếu", Group = "required", IsRequired = true, IsSelected = true, SourceColumnIndex = 0 },
            new() { SystemField = "StaffName", Label = "Họ và tên", Group = "core", IsSelected = true, SourceColumnIndex = 1 },
            new() { SystemField = "Gender", Label = "Giới tính", Group = "core", IsSelected = true, SourceColumnIndex = 2 },
            new() { SystemField = "Birthday", Label = "Ngày sinh", Group = "core", IsSelected = true, SourceColumnIndex = 3 },
            new() { SystemField = "Nationality", Label = "Quốc tịch", Group = "core", IsSelected = true, SourceColumnIndex = 4 }
        };

        var analysis = converter.Analyze(ms, headers, selections, SampleNationalities, new HashSet<string>(), new ImportReadOptions { SheetIndex = 0, HeaderRow = 1, StartRow = 2 });

        Assert.Contains("Phong ban noi bo", analysis.IgnoredColumnHeaders);
        Assert.Contains(analysis.NormalizedChanges, c => c.RowNumber == 2 && c.FieldLabel == "Giới tính" && c.RawValue == "Male" && c.NormalizedValue == "Nam");
        Assert.Contains(analysis.NormalizedChanges, c => c.RowNumber == 2 && c.FieldLabel == "Ngày sinh" && c.RawValue == "1990-06-03" && c.NormalizedValue == "03/06/1990");
        Assert.Contains(analysis.NormalizedChanges, c => c.RowNumber == 2 && c.FieldLabel == "Quốc tịch" && c.RawValue == "Korea" && c.NormalizedValue == "KOR");

        // Blocking issue for missing passport at row 3
        Assert.Contains(analysis.ActionIssues, i => i.RowNumber == 3 && i.IsBlocking && i.FieldLabel == "Số hộ chiếu");
        // Non-blocking format issue for invalid date at row 3
        Assert.Contains(analysis.ActionIssues, i => i.RowNumber == 3 && !i.IsBlocking && i.FieldLabel == "Ngày sinh" && i.RawValue == "1990/31/12");
    }

    [Fact]
    public async Task SqlImportStore_Preview_PopulatesDatabaseFieldChangesIncludingDestructiveClear()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();

        var field = await db.Context.Fields.FirstAsync();
        var account = await db.Context.Accounts.FirstAsync();
        var company = new Company { CompanyName = "Công ty Diff", IDField = field.IDField, TrackerID = account.IDUser, Delete_flag = 0 };
        db.Context.Companies.Add(company);
        await db.Context.SaveChangesAsync();

        db.Context.Employees.Add(new Employee
        {
            StaffName = "KIM MIN OLD",
            Gender = Gender.Male,
            Birthday = new DateTime(1990, 6, 3),
            Nationality = "KOR",
            Passport = "PDIFF001",
            Address = "Số 10 Lê Lợi",
            IDCompany = company.IDCompany,
            TemporaryStay = new DateTime(2026, 5, 1),
            Note = "Ghi chú cũ",
            IDUser = account.IDUser,
            Hidden_flag = 0
        });
        await db.Context.SaveChangesAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Sheet1");
        ws.Cell(1, 1).Value = "Số hộ chiếu";
        ws.Cell(1, 2).Value = "Họ tên";
        ws.Cell(1, 3).Value = "Hạn tạm trú";
        ws.Cell(1, 4).Value = "Ghi chú";

        ws.Cell(2, 1).Value = "PDIFF001";
        ws.Cell(2, 2).Value = "KIM MIN NEW";
        ws.Cell(2, 3).Value = "01/11/2026";
        ws.Cell(2, 4).Value = ""; // Destructive clear of existing Note!

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var audit = new SqlAuditStore(db.Context);
        var store = new SqlImportStore(db.Context, audit);
        var mappings = new List<ColumnMapping>
        {
            new() { ExcelColumnIndex = 0, ExcelColumnName = "Số hộ chiếu", SystemField = "Passport" },
            new() { ExcelColumnIndex = 1, ExcelColumnName = "Họ tên", SystemField = "StaffName" },
            new() { ExcelColumnIndex = 2, ExcelColumnName = "Hạn tạm trú", SystemField = "TemporaryStay" },
            new() { ExcelColumnIndex = 3, ExcelColumnName = "Ghi chú", SystemField = "Note" }
        };

        var rows = await store.GeneratePreviewAsync(ms, mappings, company.IDCompany, new ImportReadOptions { SheetIndex = 0, HeaderRow = 1, StartRow = 2 });

        var row = Assert.Single(rows);
        Assert.Equal("changed", row.Status);
        Assert.Contains(row.FieldChanges, f => f.FieldLabel == "Họ tên" && f.OldValue == "KIM MIN OLD" && f.NewValue == "KIM MIN NEW" && !f.IsDestructiveClear);
        Assert.Contains(row.FieldChanges, f => f.FieldLabel == "Hạn tạm trú" && f.OldValue == "01/05/2026" && f.NewValue == "01/11/2026" && !f.IsDestructiveClear);
        Assert.Contains(row.FieldChanges, f => f.FieldLabel == "Ghi chú" && f.OldValue == "Ghi chú cũ" && f.IsDestructiveClear);
    }
}
