

namespace IRM.Data.Models;

public class StayCase
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public string PurposeCode { get; set; } = StayPurposeCodes.Other;
    public bool IsPrimary { get; set; } = true;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? SponsorCompanyId { get; set; }
    public int? SponsorForeignPersonId { get; set; }
    public string StatusCode { get; set; } = "ACTIVE";
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ForeignPerson? ForeignPerson { get; set; }
    public Company? SponsorCompany { get; set; }
    public ForeignPerson? SponsorForeignPerson { get; set; }
    public FamilyVisitDetail? FamilyVisitDetail { get; set; }
}
