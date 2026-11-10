using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for ColumnMappingTemplate; never tracked by EF.</summary>
public sealed class ColumnMappingTemplateDto
{
    public int Id { get; set; }
    public string TemplateName { get; set; } = "";
    public int? CompanyId { get; set; }
    public string MappingJson { get; set; } = "";       // JSON: {"0":"StaffName","1":"Gender",...}
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
