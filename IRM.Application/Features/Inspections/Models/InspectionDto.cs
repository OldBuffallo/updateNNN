using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Inspection; never tracked by EF.</summary>
public sealed class InspectionDto
{
    public int Id { get; set; }
    public DateTime InspectedAt { get; set; }
    public string LocationText { get; set; } = "";
    public int? AdministrativeUnitId { get; set; }
    public int? AccommodationId { get; set; }
    public int? CompanySiteId { get; set; }
    public string InspectorNames { get; set; } = "";
    public string? InspectionUnit { get; set; }
    public string TypeCode { get; set; } = "PLANNED";
    public string? Summary { get; set; }
    public string? Note { get; set; }
    public int CreatedByAccountId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public AdministrativeUnitDto? AdministrativeUnit { get; set; }
    public AccommodationDto? Accommodation { get; set; }
    public CompanySiteDto? CompanySite { get; set; }
    public ICollection<InspectionSubjectDto> Subjects { get; set; } = new List<InspectionSubjectDto>();
}
