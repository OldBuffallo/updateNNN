using System.Globalization;
using IRM.Application.Features.ImportExport;
using IRM.Services;
using ClosedXML.Excel;
using IRM.Data.Models;

namespace IRM.Infrastructure.Excel;

/// <summary>
/// Helper đọc và parse file Excel (.xlsx)
/// </summary>
public static class ExcelReaderHelper
{
    /// <summary>Khoá phụ trong mỗi dòng đọc được: số dòng Excel thật (1-based).</summary>
    public const string RowNumberKey = "__RowNumber";

    /// <summary>Các trường ngày: ô được chuẩn hoá về dd/MM/yyyy dù là ô ngày, số serial hay chữ.</summary>
    public static readonly IReadOnlySet<string> DateFields = new HashSet<string>(StringComparer.Ordinal)
    {
        "Birthday", "TemporaryStay", "CardCreationDate",
        "FamilyVisitStartDate", "FamilyVisitEndDate",
        "StayValidFrom", "StayValidTo",
        "DocumentValidFrom", "DocumentValidTo",
        "ResidenceValidFrom", "ResidenceValidTo"
    };

    /// <summary>Tên các sheet theo thứ tự (chỉ số 0-based dùng cho sheetIndex).</summary>
    public static List<string> ReadSheetNames(Stream stream)
    {
        stream.Position = 0;
        using var workbook = new XLWorkbook(stream);
        return workbook.Worksheets.Select(w => w.Name).ToList();
    }

    private static IXLWorksheet GetWorksheet(XLWorkbook workbook, int sheetIndex)
    {
        if (sheetIndex < 0 || sheetIndex >= workbook.Worksheets.Count)
            throw new ArgumentOutOfRangeException(nameof(sheetIndex), $"Không có sheet số {sheetIndex + 1}.");
        return workbook.Worksheets.ElementAt(sheetIndex);
    }

    /// <summary>
    /// Đọc danh sách tên cột (header). Mặc định lấy dòng đầu tiên có dữ liệu của sheet đầu;
    /// <paramref name="headerRow"/> &gt; 0 chỉ rõ dòng tiêu đề (số dòng Excel thật).
    /// </summary>
    public static List<ExcelColumnInfo> ReadHeaders(Stream stream, int sheetIndex = 0, int headerRow = 0)
    {
        var headers = new List<ExcelColumnInfo>();
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = GetWorksheet(workbook, sheetIndex);
        var firstRow = headerRow > 0 ? worksheet.Row(headerRow) : worksheet.FirstRowUsed();
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
    /// Đọc dữ liệu theo column mapping, trả về dictionary per row (kèm <see cref="RowNumberKey"/>).
    /// <paramref name="startRow"/> = 0: dòng đầu tiên có dữ liệu là tiêu đề, dữ liệu liền sau.
    /// <paramref name="startRow"/> &gt; 0: dữ liệu bắt đầu đúng dòng Excel đó (bỏ qua các dòng tiêu đề
    /// công ty phía trên như file xuất từ WPF). Dòng trống giữa vùng vẫn được trả về (giá trị rỗng) để
    /// báo lỗi; dòng không có dữ liệu ở cuối vùng bị bỏ.
    /// </summary>
    public static List<Dictionary<string, string>> ReadAllRows(
        Stream stream, List<ColumnMapping> mappings, int sheetIndex = 0, int startRow = 0, int? endRow = null)
    {
        var rows = new List<Dictionary<string, string>>();
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = GetWorksheet(workbook, sheetIndex);

        int firstDataRow;
        if (startRow > 0)
        {
            firstDataRow = startRow;
        }
        else
        {
            var headerRow = worksheet.FirstRowUsed();
            if (headerRow == null) return rows;
            firstDataRow = headerRow.RowNumber() + 1;
        }

        var sheetLastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        var lastDataRow = Math.Min(endRow ?? sheetLastRow, sheetLastRow);

        // Chỉ lấy các mapping thực tế (bỏ "skip")
        var activeMappings = mappings.Where(m => m.SystemField != "skip").ToList();

        var built = new List<(Dictionary<string, string> Data, bool HasData)>();
        for (var rowNumber = firstDataRow; rowNumber <= lastDataRow; rowNumber++)
        {
            var rowData = new Dictionary<string, string>();
            var hasData = false;
            foreach (var mapping in activeMappings)
            {
                var cell = worksheet.Cell(rowNumber, mapping.ExcelColumnIndex + 1); // 1-based
                var value = DateFields.Contains(mapping.SystemField)
                    ? ReadDateText(cell)
                    : (cell.IsEmpty() ? "" : cell.GetString().Trim());
                rowData[mapping.SystemField] = value;
                if (value.Length > 0) hasData = true;
            }
            // WPF footer rows may contain notes or formatting but no person identity.
            // Keep interior blank rows for validation; only trim the trailing region.
            if (activeMappings.Any(m => m.SystemField is "StaffName" or "Passport"))
                hasData = !string.IsNullOrWhiteSpace(rowData.GetValueOrDefault("StaffName")) ||
                          !string.IsNullOrWhiteSpace(rowData.GetValueOrDefault("Passport"));
            rowData[RowNumberKey] = rowNumber.ToString(CultureInfo.InvariantCulture);
            built.Add((rowData, hasData));
        }

        var lastWithData = built.FindLastIndex(r => r.HasData);
        for (var i = 0; i <= lastWithData; i++) rows.Add(built[i].Data);
        return rows;
    }

    /// <summary>
    /// Đọc một ô thành ngày: ô kiểu ngày, số serial Excel (như WPF <c>FromOADate</c>) hoặc chữ
    /// <c>dd/MM/yyyy</c> / <c>dd-MM-yyyy</c> (xem <see cref="LegacyImportRules.TryParseLegacyDate"/>).
    /// Trả về null nếu ô trống hoặc không phải ngày.
    /// </summary>
    public static DateTime? ReadCellDate(IXLCell cell)
    {
        if (cell.IsEmpty()) return null;

        if (cell.DataType == XLDataType.DateTime)
            return cell.GetDateTime().Date;

        if (cell.DataType == XLDataType.Number)
            return FromSerial(cell.GetDouble());

        var text = cell.GetString().Trim();
        if (long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var serial))
        {
            var fromSerial = FromSerial(serial);
            if (fromSerial.HasValue) return fromSerial;
        }

        return LegacyImportRules.TryParseLegacyDate(text, out var parsed) ? parsed : null;
    }

    private static DateTime? FromSerial(double serial) =>
        serial is >= 1 and <= 2958465 ? DateTime.FromOADate(serial).Date : null;

    private static string ReadDateText(IXLCell cell)
    {
        if (cell.IsEmpty()) return "";
        var date = ReadCellDate(cell);
        return date.HasValue
            ? date.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            : cell.GetString().Trim();
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
