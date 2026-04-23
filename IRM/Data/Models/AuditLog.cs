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
    public string? Username { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? IpAddress { get; set; }
}

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

/// <summary>
/// Bảng ImportBackups — Lưu dữ liệu cũ để rollback
/// </summary>
public class ImportBackup
{
    public long Id { get; set; }
    public string ImportSessionId { get; set; } = "";
    public string ActionType { get; set; } = "";       // "INSERT" hoặc "UPDATE"
    public int EmployeeId { get; set; }
    public string? OldData { get; set; }                // JSON snapshot dữ liệu cũ (cho UPDATE)
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Bảng ColumnMappingTemplates — Template ghép cột tái sử dụng
/// </summary>
public class ColumnMappingTemplate
{
    public int Id { get; set; }
    public string TemplateName { get; set; } = "";
    public int? CompanyId { get; set; }
    public string MappingJson { get; set; } = "";       // JSON: {"0":"StaffName","1":"Gender",...}
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
