using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Accommodation; never tracked by EF.</summary>
public sealed class AccommodationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = "RENTED";
    public string AddressLine { get; set; } = "";
    public int? AdministrativeUnitId { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public int? Capacity { get; set; }
    public string? Note { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public AdministrativeUnitDto? AdministrativeUnit { get; set; }
    public ICollection<CompanyAccommodationAgreementDto> CompanyAgreements { get; set; } = new List<CompanyAccommodationAgreementDto>();
    public ICollection<ResidencePeriodDto> ResidencePeriods { get; set; } = new List<ResidencePeriodDto>();
}
