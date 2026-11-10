

namespace IRM.Data.Models;

public class CompanyAccommodationAgreement
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int AccommodationId { get; set; }
    public string ScopeCode { get; set; } = "WHOLE_PROPERTY";
    public string? UnitLabel { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Note { get; set; }

    public Company? Company { get; set; }
    public Accommodation? Accommodation { get; set; }
}
