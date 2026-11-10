using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for CompanyRepresentative; never tracked by EF.</summary>
public sealed class CompanyRepresentativeDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string FullName { get; set; } = "";
    public string? Title { get; set; }
    public string? IdNumber { get; set; }
    public string? Phone { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public CompanyDto? Company { get; set; }
}
