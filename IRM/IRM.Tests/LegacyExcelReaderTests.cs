using ClosedXML.Excel;
using IRM.Data.Models;
using IRM.Infrastructure.Excel;
using Xunit;

namespace IRM.Tests;

/// <summary>Task 3: đọc file theo dòng bắt đầu/kết thúc, ô có kiểu, số dòng Excel thật.</summary>
public class LegacyExcelReaderTests
{
    // Chỉ số cột 0-based, giống ColumnMapping.ExcelColumnIndex
    private static List<ColumnMapping> WpfMappings() => new()
    {
        new() { ExcelColumnIndex = 1, SystemField = "StaffName" },
        new() { ExcelColumnIndex = 2, SystemField = "Gender" },
        new() { ExcelColumnIndex = 3, SystemField = "Birthday" },
        new() { ExcelColumnIndex = 5, SystemField = "Passport" },
        new() { ExcelColumnIndex = 10, SystemField = "TemporaryStay" },
    };

    private static MemoryStream Save(XLWorkbook wb)
    {
        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    /// <summary>File xuất từ WPF: 6 dòng tiêu đề, tiêu đề cột dòng 8, dữ liệu từ dòng 9.</summary>
    private static MemoryStream WpfStyleWorkbook()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Sheet1");
        ws.Cell(1, 2).Value = "CÔNG TY TNHH ABC";
        ws.Cell(2, 2).Value = "Địa chỉ: 1 Lê Duẩn";
        ws.Cell(6, 1).Value = "DANH SÁCH NGƯỜI NƯỚC NGOÀI LÀM VIỆC TẠI CÔNG TY";
        string[] headers = { "Stt", "Họ và tên", "Giới tính", "Ngày sinh", "Quốc tịch", "Số hộ chiếu",
            "Địa chỉ tạm trú", "Nghề nghiệp", "Số GPLĐ", "Số visa", "Thời hạn tạm trú" };
        for (var i = 0; i < headers.Length; i++) ws.Cell(8, i + 1).Value = headers[i];

        // dòng 9: ngày kiểu DateTime, hộ chiếu kiểu số
        ws.Cell(9, 1).Value = 1;
        ws.Cell(9, 2).Value = "JOHN SMITH";
        ws.Cell(9, 3).Value = "Nam";
        ws.Cell(9, 4).Value = new DateTime(1980, 3, 15);
        ws.Cell(9, 6).Value = 123456;
        ws.Cell(9, 11).Value = 45000; // số serial Excel = 15/03/2023

        // dòng 10: ngày dạng chữ
        ws.Cell(10, 1).Value = 2;
        ws.Cell(10, 2).Value = "MARY JONES";
        ws.Cell(10, 3).Value = "Nữ";
        ws.Cell(10, 4).Value = "03/06/1990";
        ws.Cell(10, 6).Value = "B9876543";
        ws.Cell(10, 11).Value = "1-2-27";

        // dòng 11: trống hoàn toàn (giữa vùng)
        // dòng 12: có dữ liệu
        ws.Cell(12, 1).Value = 3;
        ws.Cell(12, 2).Value = "LI WEI";
        ws.Cell(12, 3).Value = "Nam";
        ws.Cell(12, 4).Value = "không phải ngày";
        ws.Cell(12, 6).Value = "E111";
        ws.Cell(12, 11).Value = "15/03/1980 (hết hạn)";

        // dòng 13-14: chỉ có định dạng, không có dữ liệu (cuối vùng)
        ws.Cell(13, 2).Style.Font.Bold = true;
        ws.Cell(14, 2).Style.Fill.BackgroundColor = XLColor.Yellow;
        return Save(wb);
    }

    [Fact]
    public void WpfStyleFile_StartRow9_ReadsRowsWithRealExcelRowNumbers()
    {
        using var stream = WpfStyleWorkbook();

        var rows = ExcelReaderHelper.ReadAllRows(stream, WpfMappings(), startRow: 9);

        Assert.Equal(new[] { "9", "10", "11", "12" }, rows.Select(r => r["__RowNumber"]).ToArray());
        Assert.Equal("JOHN SMITH", rows[0]["StaffName"]);
        Assert.Equal("LI WEI", rows[3]["StaffName"]);
    }

    [Fact]
    public void BlankRowInsideTheRange_IsEmittedEmpty_NotTreatedAsTheEnd()
    {
        using var stream = WpfStyleWorkbook();

        var rows = ExcelReaderHelper.ReadAllRows(stream, WpfMappings(), startRow: 9);

        var blank = rows.Single(r => r["__RowNumber"] == "11");
        Assert.Equal("", blank["StaffName"]);
        Assert.Equal("", blank["Passport"]);
        Assert.Equal("LI WEI", rows.Last()["StaffName"]); // dòng 12 vẫn được đọc
    }

    [Fact]
    public void FormattingOnlyRowsAtTheEnd_AreNotEmitted()
    {
        using var stream = WpfStyleWorkbook();

        var rows = ExcelReaderHelper.ReadAllRows(stream, WpfMappings(), startRow: 9);

        Assert.DoesNotContain(rows, r => r["__RowNumber"] is "13" or "14");
    }

    [Fact]
    public void DateFields_AreNormalisedToDayMonthYear_WhateverTheCellType()
    {
        using var stream = WpfStyleWorkbook();

        var rows = ExcelReaderHelper.ReadAllRows(stream, WpfMappings(), startRow: 9);

        Assert.Equal("15/03/1980", rows[0]["Birthday"]);      // ô kiểu DateTime
        Assert.Equal("15/03/2023", rows[0]["TemporaryStay"]); // số serial 45000
        Assert.Equal("03/06/1990", rows[1]["Birthday"]);      // chữ dd/MM/yyyy giữ nguyên ngày 3 tháng 6
        Assert.Equal("01/02/2027", rows[1]["TemporaryStay"]); // chữ d-M-yy
        Assert.Equal("15/03/1980", rows[3]["TemporaryStay"]); // ô nhiều từ lấy từ đầu tiên là ngày
    }

    [Fact]
    public void UnparsableDate_IsKeptAsRawText_SoTheRowCanBeReported()
    {
        using var stream = WpfStyleWorkbook();

        var rows = ExcelReaderHelper.ReadAllRows(stream, WpfMappings(), startRow: 9);

        Assert.Equal("không phải ngày", rows[3]["Birthday"]);
    }

    [Fact]
    public void NumericCellInANonDateField_StaysText()
    {
        using var stream = WpfStyleWorkbook();

        var rows = ExcelReaderHelper.ReadAllRows(stream, WpfMappings(), startRow: 9);

        Assert.Equal("123456", rows[0]["Passport"]);
    }

    [Fact]
    public void EndRow_LimitsTheRange()
    {
        using var stream = WpfStyleWorkbook();

        var rows = ExcelReaderHelper.ReadAllRows(stream, WpfMappings(), startRow: 9, endRow: 10);

        Assert.Equal(new[] { "9", "10" }, rows.Select(r => r["__RowNumber"]).ToArray());
    }

    [Fact]
    public void DefaultMode_HeaderOnFirstRow_StillWorksLikeBefore()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Data");
        ws.Cell(1, 1).Value = "Họ tên";
        ws.Cell(1, 2).Value = "Giới tính";
        ws.Cell(2, 1).Value = "A";
        ws.Cell(2, 2).Value = "Nam";
        ws.Cell(3, 1).Value = "B";
        ws.Cell(3, 2).Value = "Nữ";
        using var stream = Save(wb);
        var mappings = new List<ColumnMapping>
        {
            new() { ExcelColumnIndex = 0, SystemField = "StaffName" },
            new() { ExcelColumnIndex = 1, SystemField = "Gender" },
            new() { ExcelColumnIndex = 2, SystemField = "skip" },
        };

        var rows = ExcelReaderHelper.ReadAllRows(stream, mappings);

        Assert.Equal(2, rows.Count);
        Assert.Equal(new[] { "2", "3" }, rows.Select(r => r["__RowNumber"]).ToArray());
        Assert.Equal("Nữ", rows[1]["Gender"]);
    }

    [Fact]
    public void DefaultMode_ContinuesPastABlankRow()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Data");
        ws.Cell(1, 1).Value = "Họ tên";
        ws.Cell(2, 1).Value = "A";
        ws.Cell(4, 1).Value = "C"; // dòng 3 trống
        using var stream = Save(wb);
        var mappings = new List<ColumnMapping> { new() { ExcelColumnIndex = 0, SystemField = "StaffName" } };

        var rows = ExcelReaderHelper.ReadAllRows(stream, mappings);

        Assert.Equal(new[] { "A", "", "C" }, rows.Select(r => r["StaffName"]).ToArray());
    }

    [Fact]
    public void SheetNames_AndSheetIndex_SelectTheRightSheet()
    {
        using var wb = new XLWorkbook();
        wb.Worksheets.Add("Đầu").Cell(1, 1).Value = "x";
        var second = wb.Worksheets.Add("Hai");
        second.Cell(1, 1).Value = "Họ tên";
        second.Cell(2, 1).Value = "SECOND";
        using var stream = Save(wb);
        var mappings = new List<ColumnMapping> { new() { ExcelColumnIndex = 0, SystemField = "StaffName" } };

        Assert.Equal(new[] { "Đầu", "Hai" }, ExcelReaderHelper.ReadSheetNames(stream));
        var rows = ExcelReaderHelper.ReadAllRows(stream, mappings, sheetIndex: 1);

        Assert.Equal("SECOND", Assert.Single(rows)["StaffName"]);
    }

    [Fact]
    public void ReadHeaders_CanTakeAnExplicitHeaderRow()
    {
        using var stream = WpfStyleWorkbook();

        var headers = ExcelReaderHelper.ReadHeaders(stream, sheetIndex: 0, headerRow: 8);

        Assert.Equal("Họ và tên", headers.Single(h => h.Index == 1).HeaderName);
        Assert.Equal("JOHN SMITH", headers.Single(h => h.Index == 1).SampleValue);
    }
}
