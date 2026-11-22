

namespace IRM.Data.Models;

public class SiteZoneMembership
{
    public int Id { get; set; }
    public int CompanySiteId { get; set; }
    public int EconomicZoneId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public CompanySite? CompanySite { get; set; }
    public EconomicZone? EconomicZone { get; set; }
}
