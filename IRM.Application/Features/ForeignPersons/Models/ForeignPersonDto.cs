using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for ForeignPerson; never tracked by EF.</summary>
public sealed class ForeignPersonDto
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
    public NationalityEntityDto? Nationality { get; set; }
    public ICollection<ForeignPersonSourceLinkDto> SourceLinks { get; set; } = new List<ForeignPersonSourceLinkDto>();
    public ICollection<StayCaseDto> StayCases { get; set; } = new List<StayCaseDto>();
    public ICollection<ResidencePeriodDto> ResidencePeriods { get; set; } = new List<ResidencePeriodDto>();
    public ICollection<ImmigrationDocumentDto> ImmigrationDocuments { get; set; } = new List<ImmigrationDocumentDto>();
    public ICollection<ElectronicIdentityDto> ElectronicIdentities { get; set; } = new List<ElectronicIdentityDto>();
}
