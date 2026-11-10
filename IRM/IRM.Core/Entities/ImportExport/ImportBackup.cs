

namespace IRM.Data.Models;

/// <summary>
/// Bảng ImportBackups — Lưu dữ liệu cũ để rollback
/// </summary>
public class ImportBackup
{
    public long Id { get; set; }
    public string ImportSessionId { get; set; } = "";
    public string ActionType { get; set; } = "";       // "INSERT" hoặc "UPDATE"
    public string EntityType { get; set; } = "Employee"; // "Employee" | "Student" | "ForeignPerson"
    public int EntityId { get; set; }
    public string? OldData { get; set; }                // JSON snapshot dữ liệu cũ (cho UPDATE)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
