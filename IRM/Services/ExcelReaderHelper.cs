using ClosedXML.Excel;
using IRM.Data.Models;

namespace IRM.Services;

/// <summary>
/// Helper đọc và parse file Excel (.xlsx)
/// </summary>
public static class ExcelReaderHelper
{
    /// <summary>
    /// Đọc danh sách tên cột (header) từ dòng đầu tiên của sheet đầu tiên
    /// </summary>
    public static List<ExcelColumnInfo> ReadHeaders(Stream stream)
    {
        var headers = new List<ExcelColumnInfo>();
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var firstRow = worksheet.FirstRowUsed();
        if (firstRow == null) return headers;

        // Đọc dòng dữ liệu đầu tiên để lấy sample
        var secondRow = firstRow.RowBelow();

        foreach (var cell in firstRow.CellsUsed())
        {
            var info = new ExcelColumnInfo
            {
                Index = cell.Address.ColumnNumber - 1,
                HeaderName = cell.GetString().Trim()
            };

            // Lấy sample value từ dòng thứ 2
            if (secondRow != null)
            {
                var sampleCell = secondRow.Cell(cell.Address.ColumnNumber);
                info.SampleValue = sampleCell.IsEmpty() ? "" : sampleCell.GetString().Trim();
            }

            headers.Add(info);
        }

        return headers;
    }

    /// <summary>
    /// Đọc tổng số dòng dữ liệu (không tính header)
    /// </summary>
    public static int CountDataRows(Stream stream)
    {
        stream.Position = 0;
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var lastRow = worksheet.LastRowUsed();
        if (lastRow == null) return 0;
        return lastRow.RowNumber() - 1; // minus header row
    }

    /// <summary>
    /// Đọc dữ liệu theo column mapping, trả về dictionary per row
    /// </summary>
    public static List<Dictionary<string, string>> ReadAllRows(Stream stream, List<ColumnMapping> mappings)
    {
        var rows = new List<Dictionary<string, string>>();
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var firstRow = worksheet.FirstRowUsed();
        if (firstRow == null) return rows;

        // Chỉ lấy các mapping thực tế (bỏ "skip")
        var activeMappings = mappings.Where(m => m.SystemField != "skip").ToList();

        var currentRow = firstRow.RowBelow(); // Bỏ qua header
        while (currentRow != null && !currentRow.IsEmpty())
        {
            var rowData = new Dictionary<string, string>();
            foreach (var mapping in activeMappings)
            {
                var cell = currentRow.Cell(mapping.ExcelColumnIndex + 1); // 1-based
                var value = cell.IsEmpty() ? "" : cell.GetString().Trim();
                rowData[mapping.SystemField] = value;
            }
            rows.Add(rowData);
            currentRow = currentRow.RowBelow();
            if (currentRow.RowNumber() > worksheet.LastRowUsed()?.RowNumber()) break;
        }

        return rows;
    }

    /// <summary>
    /// Đọc N dòng đầu để preview
    /// </summary>
    public static List<Dictionary<string, string>> ReadPreviewRows(Stream stream, List<ColumnMapping> mappings, int count = 5)
    {
        var allRows = ReadAllRows(stream, mappings);
        return allRows.Take(count).ToList();
    }

    /// <summary>
    /// Lấy thông tin sheet: số dòng, số cột
    /// </summary>
    public static (int rowCount, int colCount, string sheetName) GetSheetInfo(Stream stream)
    {
        stream.Position = 0;
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var lastRow = worksheet.LastRowUsed();
        var lastCol = worksheet.LastColumnUsed();

        return (
            lastRow?.RowNumber() - 1 ?? 0,
            lastCol?.ColumnNumber() ?? 0,
            worksheet.Name
        );
    }
}
