

namespace IRM.Data.Models;

/// <summary>Mapping giữa cột Excel → trường hệ thống</summary>
public class ColumnMapping
{
    public int ExcelColumnIndex { get; set; }
    public string ExcelColumnName { get; set; } = "";
    public string SystemField { get; set; } = "";          // e.g. "StaffName", "Gender", "skip"
    public string SystemFieldLabel { get; set; } = "";     // e.g. "Họ tên", "Giới tính"
    public bool IsAutoMapped { get; set; }
}
