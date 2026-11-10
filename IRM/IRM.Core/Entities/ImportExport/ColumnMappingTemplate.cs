

namespace IRM.Data.Models;

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
