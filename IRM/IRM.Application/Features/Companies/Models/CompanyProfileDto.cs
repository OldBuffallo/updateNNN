using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for CompanyProfile; never tracked by EF.</summary>
public sealed class CompanyProfileDto
{
    public int CompanyId { get; set; }
    public string OwnershipTypeCode { get; set; } = "DOMESTIC";
    public string? TaxCode { get; set; }
    public string? BusinessRegistrationNumber { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public CompanyDto? Company { get; set; }
}
