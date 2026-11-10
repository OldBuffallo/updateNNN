using IRM.Data.Models;
using IRM.Services;

namespace IRM.Infrastructure.Excel;

public sealed class ExcelReader : IExcelReader
{
    public List<ExcelColumnInfo> ReadHeaders(Stream stream) => ExcelReaderHelper.ReadHeaders(stream);
    public int CountDataRows(Stream stream) => ExcelReaderHelper.CountDataRows(stream);
    public List<Dictionary<string, string>> ReadAllRows(Stream stream, List<ColumnMapping> mappings) => ExcelReaderHelper.ReadAllRows(stream, mappings);
    public List<Dictionary<string, string>> ReadPreviewRows(Stream stream, List<ColumnMapping> mappings, int count = 5) => ExcelReaderHelper.ReadPreviewRows(stream, mappings, count);
    public (int rowCount, int colCount, string sheetName) GetSheetInfo(Stream stream) => ExcelReaderHelper.GetSheetInfo(stream);
}
