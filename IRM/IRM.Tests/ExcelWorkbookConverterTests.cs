using ClosedXML.Excel;
using IRM.Application.Features.ImportExport;
using IRM.Application.Models;
using IRM.Data.Models;
using IRM.Infrastructure.Excel;
using IRM.Infrastructure.Persistence.Stores.Administration;
using IRM.Infrastructure.Persistence.Stores.ImportExport;

namespace IRM.Tests;

public sealed class ExcelWorkbookConverterTests
{
    private static readonly List<NationalityEntityDto> SampleNationalities =
    [
        new() { IDNationality = 1, NationalityCode = "KOR", NationalityName = "Hàn Quốc" },
        new() { IDNationality = 2, NationalityCode = "JPN", NationalityName = "Nhật Bản" },
        new() { IDNationality = 3, NationalityCode = "USA", NationalityName = "Hoa Kỳ" }
    ];

    private static MemoryStream BuildMessyWorkbook()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Sheet1");
        string[] headers = ["STT", "Full Name", "So HC", "Sex", "DOB", "Country", "Junk1", "Work Permit", "Han tam tru", "Junk2"];
        for (int c = 0; c < headers.Length; c++) ws.Cell(1, c + 1).Value = headers[c];

        string[][] rows =
        [
            ["1", "  nguyen van a ", " 0012345 ", "Male", "1990-06-03", "Korea", "x", "Chưa có GPLĐ", "2027-05-20", "y"],
            ["2", "KIM MIN", "M8845120", "1", "15-08-1988", "KR", "", "Miễn GPLĐ", "30/11/2026", ""],
            ["3", "TANAKA YUKI", "TK009911", "Female", "03.11.1995", "Japan", "", "GP-2026-01", "31/02/2027", ""],
            ["4", "", "P7766554", "F", "1992/01/10", "USA", "", "Không", "15/09/2027", ""],
            ["5", "NO PASSPORT", "", "Nam", "20/04/1985", "USA", "", "", "01/08/2028", ""],
            ["6", "EXTRA ROW", "P6666666", "Nam", "10/10/1990", "KOR", "", "", "10/10/2028", ""]
        ];

        for (int r = 0; r < rows.Length; r++)
            for (int c = 0; c < rows[r].Length; c++)
                ws.Cell(r + 2, c + 1).Value = rows[r][c];

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static List<ExcelFieldSelection> DefaultSelections() =>
    [
        new() { SystemField = "Passport", Label = "Số hộ chiếu", Group = "required", IsRequired = true, IsSelected = true, SourceColumnIndex = 2 },
        new() { SystemField = "StaffName", Label = "Họ và tên", Group = "core", IsSelected = true, SourceColumnIndex = 1 },
        new() { SystemField = "Gender", Label = "Giới tính", Group = "core", IsSelected = true, SourceColumnIndex = 3 },
        new() { SystemField = "Birthday", Label = "Ngày sinh", Group = "core", IsSelected = true, SourceColumnIndex = 4 },
        new() { SystemField = "Nationality", Label = "Quốc tịch", Group = "core", IsSelected = true, SourceColumnIndex = 5 },
        new() { SystemField = "WorkPermit", Label = "Giấy phép lao động", Group = "core", IsSelected = true, SourceColumnIndex = 7 },
        new() { SystemField = "TemporaryStay", Label = "Hạn tạm trú", Group = "core", IsSelected = true, SourceColumnIndex = 8 },
        new() { SystemField = "Address", Label = "Địa chỉ", Group = "core", IsSelected = false, SourceColumnIndex = null }
    ];

    [Fact]
    public void Analyze_CountsFullSheetGapsMissingPassportAutoFixesAndWarnings()
    {
        using var stream = BuildMessyWorkbook();
        var headers = ExcelReaderHelper.ReadHeaders(stream);
        var converter = new ExcelWorkbookConverter();

        var analysis = converter.Analyze(
            stream, headers, DefaultSelections(), SampleNationalities,
            new HashSet<string>(), new ImportReadOptions { HeaderRow = 1, StartRow = 2 }, sampleLimit: 5);

        Assert.Equal(6, analysis.TotalDataRows);
        Assert.Equal(10, analysis.TotalSourceColumns);
        Assert.Equal(7, analysis.SelectedColumnCount);
        Assert.Equal(3, analysis.IgnoredColumnCount);
        Assert.Equal([6], analysis.MissingPassportRows); // row 6 in Excel (5th data row)
        Assert.Equal(5, analysis.SampleRows.Count);
        Assert.True(analysis.TotalAutoNormalizedCells > 0);
        Assert.Contains(analysis.WarningFields, w => w.SystemField == "TemporaryStay" && w.RowNumbers.Contains(4));
        Assert.Contains(analysis.EmptyCellFields, e => e.SystemField == "StaffName" && e.RowNumbers.Contains(5));
        Assert.Contains("Địa chỉ", analysis.UnselectedCoreFieldLabels);
    }

    [Fact]
    public async Task ConvertToCleanWorkbook_OutputsOnlySelectedColumnsWithTextPassportAndRoundTripsToImportPreview()
    {
        await using var td = new TestDatabase();
        await td.InitializeAsync();
        using var stream = BuildMessyWorkbook();
        var converter = new ExcelWorkbookConverter();

        var output = converter.ConvertToCleanWorkbook(
            stream, DefaultSelections(), SampleNationalities,
            new HashSet<string>(), new ImportReadOptions { HeaderRow = 1, StartRow = 2 });

        Assert.Equal(7, output.CleanMappings.Count);
        using (var cleanStream = new MemoryStream(output.WorkbookBytes))
        {
            var cleanHeaders = ExcelReaderHelper.ReadHeaders(cleanStream);
            Assert.Equal(7, cleanHeaders.Count);
            var cleanRows = ExcelReaderHelper.ReadAllRows(cleanStream, output.CleanMappings, 0, 2);
            Assert.Equal("0012345", cleanRows[0]["Passport"]);
            Assert.Equal("Nam", cleanRows[0]["Gender"]);
            Assert.Equal("03/06/1990", cleanRows[0]["Birthday"]);
            Assert.Equal("KOR", cleanRows[0]["Nationality"]);
            Assert.Equal("", cleanRows[0]["WorkPermit"]);
        }

        // Round-trip clean stream into SqlImportStore.GeneratePreviewAsync
        var store = new SqlImportStore(td.Context, new SqlAuditStore(td.Context));
        using var importStream = new MemoryStream(output.WorkbookBytes);
        var preview = await store.GeneratePreviewAsync(importStream, output.CleanMappings, companyId: 0,
            new ImportReadOptions { SheetIndex = 0, HeaderRow = 1, StartRow = 2 });

        Assert.Equal("new", preview[0].Status);
        Assert.Equal("0012345", preview[0].Passport);
        Assert.Equal("error", preview[2].Status); // invalid date 31/02/2027 caught in Step 3 too
        Assert.Equal("error", preview[4].Status); // missing passport caught in Step 3 too
    }
}
