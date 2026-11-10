using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for StayCase; never tracked by EF.</summary>
public sealed class StayCaseDto
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
    public ForeignPersonDto? ForeignPerson { get; set; }
    public CompanyDto? SponsorCompany { get; set; }
    public ForeignPersonDto? SponsorForeignPerson { get; set; }
    public FamilyVisitDetailDto? FamilyVisitDetail { get; set; }
}
