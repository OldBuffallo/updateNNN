using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for CompanyAccommodationAgreement; never tracked by EF.</summary>
public sealed class CompanyAccommodationAgreementDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int AccommodationId { get; set; }
    public string ScopeCode { get; set; } = "WHOLE_PROPERTY";
    public string? UnitLabel { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Note { get; set; }
    public CompanyDto? Company { get; set; }
    public AccommodationDto? Accommodation { get; set; }
}
