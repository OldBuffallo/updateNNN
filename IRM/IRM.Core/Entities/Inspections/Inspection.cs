

namespace IRM.Data.Models;

public class Inspection
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

    public AdministrativeUnit? AdministrativeUnit { get; set; }
    public Accommodation? Accommodation { get; set; }
    public CompanySite? CompanySite { get; set; }
    public ICollection<InspectionSubject> Subjects { get; set; } = new List<InspectionSubject>();
}
