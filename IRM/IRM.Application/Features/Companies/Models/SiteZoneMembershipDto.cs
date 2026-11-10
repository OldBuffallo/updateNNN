using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for SiteZoneMembership; never tracked by EF.</summary>
public sealed class SiteZoneMembershipDto
{
    public int Id { get; set; }
    public int CompanySiteId { get; set; }
    public int EconomicZoneId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public CompanySiteDto? CompanySite { get; set; }
    public EconomicZoneDto? EconomicZone { get; set; }
}
