using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Field; never tracked by EF.</summary>
public sealed class FieldDto
{
    public int IDField { get; set; }
    public string FieldName { get; set; } = "";
    public string? Description { get; set; }
    public int Delete_flag { get; set; }
    public ICollection<CompanyDto> Companies { get; set; } = new List<CompanyDto>();
    public bool IsActive => Delete_flag == 0;
}
