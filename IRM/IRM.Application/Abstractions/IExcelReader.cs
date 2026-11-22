using IRM.Data.Models;

namespace IRM.Application.Abstractions;

public interface IExcelReader
{
    List<string> ReadSheetNames(Stream stream);
    List<ExcelColumnInfo> ReadHeaders(Stream stream, int sheetIndex = 0, int headerRow = 0);
    int CountDataRows(Stream stream);
    List<Dictionary<string, string>> ReadAllRows(Stream stream, List<ColumnMapping> mappings);
    List<Dictionary<string, string>> ReadPreviewRows(Stream stream, List<ColumnMapping> mappings, int count = 5);
    (int rowCount, int colCount, string sheetName) GetSheetInfo(Stream stream);
}
