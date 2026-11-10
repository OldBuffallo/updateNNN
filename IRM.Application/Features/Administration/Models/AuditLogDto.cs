using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for AuditLog; never tracked by EF.</summary>
public sealed class AuditLogDto
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
