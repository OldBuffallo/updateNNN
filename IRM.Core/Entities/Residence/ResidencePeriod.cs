

namespace IRM.Data.Models;

public class ResidencePeriod
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

    public ForeignPerson? ForeignPerson { get; set; }
    public Accommodation? Accommodation { get; set; }
    public AdministrativeUnit? AdministrativeUnit { get; set; }
    public Company? ResponsibleCompany { get; set; }
}
