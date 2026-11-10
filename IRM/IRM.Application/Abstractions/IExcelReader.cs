using IRM.Data.Models;

namespace IRM.Application.Abstractions;

public interface IExcelReader
{
    List<ExcelColumnInfo> ReadHeaders(Stream stream);
    int CountDataRows(Stream stream);
    List<Dictionary<string, string>> ReadAllRows(Stream stream, List<ColumnMapping> mappings);
    List<Dictionary<string, string>> ReadPreviewRows(Stream stream, List<ColumnMapping> mappings, int count = 5);
    (int rowCount, int colCount, string sheetName) GetSheetInfo(Stream stream);
}
