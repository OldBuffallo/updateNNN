using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for CompanySite; never tracked by EF.</summary>
public sealed class CompanySiteDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = "";
    public string SiteTypeCode { get; set; } = "OPERATING_SITE";
    public string AddressLine { get; set; } = "";
    public int AdministrativeUnitId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsDeleted { get; set; }
    public CompanyDto? Company { get; set; }
    public AdministrativeUnitDto? AdministrativeUnit { get; set; }
    public ICollection<SiteZoneMembershipDto> ZoneMemberships { get; set; } = new List<SiteZoneMembershipDto>();
}
