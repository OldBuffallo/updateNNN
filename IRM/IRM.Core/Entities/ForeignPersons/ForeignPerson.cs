

namespace IRM.Data.Models;

public class ForeignPerson
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? NationalityCode { get; set; }
    public string? PassportNumber { get; set; }
    public string? PassportSearchKey { get; set; }
    public bool IsDataIncomplete { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public NationalityEntity? Nationality { get; set; }
    public ICollection<ForeignPersonSourceLink> SourceLinks { get; set; } = new List<ForeignPersonSourceLink>();
    public ICollection<StayCase> StayCases { get; set; } = new List<StayCase>();
    public ICollection<ResidencePeriod> ResidencePeriods { get; set; } = new List<ResidencePeriod>();
    public ICollection<ImmigrationDocument> ImmigrationDocuments { get; set; } = new List<ImmigrationDocument>();
    public ICollection<ElectronicIdentity> ElectronicIdentities { get; set; } = new List<ElectronicIdentity>();
}
