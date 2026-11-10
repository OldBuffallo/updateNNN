

namespace IRM.Data.Models;

/// <summary>
/// Bảng AuditLog — Nhật ký hoạt động hệ thống (BẢNG MỚI)
/// </summary>
public class AuditLog
{
    public long Id { get; set; }
    public string Action { get; set; } = "";        // "CREATE", "UPDATE", "DELETE", "LOGIN", "IMPORT"
    public string EntityType { get; set; } = "";     // "Company", "Employee", "Account"...
    public int? EntityId { get; set; }
    public string? Description { get; set; }
    public string? ChangesJson { get; set; }  // JSON: [{"Field":"Name","Old":"A","New":"B"}]
    public string? Username { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? IpAddress { get; set; }
}
