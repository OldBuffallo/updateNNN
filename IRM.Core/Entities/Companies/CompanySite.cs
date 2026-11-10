

namespace IRM.Data.Models;

public class CompanySite
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

    public Company? Company { get; set; }
    public AdministrativeUnit? AdministrativeUnit { get; set; }
    public ICollection<SiteZoneMembership> ZoneMemberships { get; set; } = new List<SiteZoneMembership>();
}
