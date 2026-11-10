using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Email; never tracked by EF.</summary>
public sealed class EmailDto
{
    public int IDEmail { get; set; }
    public string? Name { get; set; }
    public string? Mail { get; set; }
    public int IDCompany { get; set; }
    public int Delete_flag { get; set; }
    public CompanyDto? Company { get; set; }
}
