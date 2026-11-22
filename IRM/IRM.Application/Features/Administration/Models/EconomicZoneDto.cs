using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for EconomicZone; never tracked by EF.</summary>
public sealed class EconomicZoneDto
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = "INDUSTRIAL_PARK";
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<SiteZoneMembershipDto> SiteMemberships { get; set; } = new List<SiteZoneMembershipDto>();
}
