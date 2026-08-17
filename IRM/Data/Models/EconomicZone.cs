namespace IRM.Data.Models;

public class EconomicZone
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = "INDUSTRIAL_PARK";
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<SiteZoneMembership> SiteMemberships { get; set; } = new List<SiteZoneMembership>();
}
