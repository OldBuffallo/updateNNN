using IRM.Data.Models;
using IRM.Services;

namespace IRM.Infrastructure.Excel;

public sealed class ExcelReader : IExcelReader
{
    public List<string> ReadSheetNames(Stream stream) => ExcelReaderHelper.ReadSheetNames(stream);
    public List<ExcelColumnInfo> ReadHeaders(Stream stream, int sheetIndex = 0, int headerRow = 0) => ExcelReaderHelper.ReadHeaders(stream, sheetIndex, headerRow);
    public int CountDataRows(Stream stream) => ExcelReaderHelper.CountDataRows(stream);
    public List<Dictionary<string, string>> ReadAllRows(Stream stream, List<ColumnMapping> mappings) => ExcelReaderHelper.ReadAllRows(stream, mappings);
    public List<Dictionary<string, string>> ReadPreviewRows(Stream stream, List<ColumnMapping> mappings, int count = 5) => ExcelReaderHelper.ReadPreviewRows(stream, mappings, count);
    public (int rowCount, int colCount, string sheetName) GetSheetInfo(Stream stream) => ExcelReaderHelper.GetSheetInfo(stream);
}
