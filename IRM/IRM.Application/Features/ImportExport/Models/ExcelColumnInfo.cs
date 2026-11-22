

namespace IRM.Data.Models;

/// <summary>
/// DTOs cho quy trình Import Excel
/// </summary>

/// <summary>Thông tin một cột trong file Excel</summary>
public class ExcelColumnInfo
{
    public int Index { get; set; }
    public string HeaderName { get; set; } = "";
    public string? SampleValue { get; set; }
}
