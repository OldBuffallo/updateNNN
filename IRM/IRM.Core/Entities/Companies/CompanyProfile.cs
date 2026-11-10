

namespace IRM.Data.Models;

public class CompanyProfile
{
    public int CompanyId { get; set; }
    public string OwnershipTypeCode { get; set; } = "DOMESTIC";
    public string? TaxCode { get; set; }
    public string? BusinessRegistrationNumber { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public Company? Company { get; set; }
}
