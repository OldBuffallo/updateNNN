using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for ResidencePeriod; never tracked by EF.</summary>
public sealed class ResidencePeriodDto
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public int? AccommodationId { get; set; }
    public int AdministrativeUnitId { get; set; }
    public string AddressLine { get; set; } = "";
    public string? UnitLabel { get; set; }
    public string ArrangementCode { get; set; } = "SELF_RENTED";
    public int? ResponsibleCompanyId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Note { get; set; }
    public ForeignPersonDto? ForeignPerson { get; set; }
    public AccommodationDto? Accommodation { get; set; }
    public AdministrativeUnitDto? AdministrativeUnit { get; set; }
    public CompanyDto? ResponsibleCompany { get; set; }
}
