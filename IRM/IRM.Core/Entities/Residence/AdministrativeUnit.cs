

namespace IRM.Data.Models;

public class AdministrativeUnit
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = AdministrativeUnitTypeCodes.Commune;
    public int? ParentId { get; set; }
    public int? PredecessorId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsDeleted { get; set; }
    public int? LegacyDistrictId { get; set; }   // Cross-ref District legacy
    public int? LegacyWardId { get; set; }       // Cross-ref Ward legacy

    public AdministrativeUnit? Parent { get; set; }
    public AdministrativeUnit? Predecessor { get; set; }
}
