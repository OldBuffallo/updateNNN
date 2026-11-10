

namespace IRM.Data.Models;

/// <summary>
/// Bảng ImportHistories — Lịch sử import Excel (MỞ RỘNG)
/// </summary>
public class ImportHistory
{
    public long Id { get; set; }
    public string SessionId { get; set; } = "";       // VD: "IMP-2026-016"
    public string FileName { get; set; } = "";
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int TotalRows { get; set; }
    public int AddedRows { get; set; }
    public int UpdatedRows { get; set; }
    public int ErrorRows { get; set; }
    public string Status { get; set; } = "committed";  // "committed" | "rolledback"
    public string? Username { get; set; }
    public DateTime ImportDate { get; set; } = DateTime.Now;
    public string? ErrorDetails { get; set; }           // JSON chi tiết lỗi từng dòng
}
