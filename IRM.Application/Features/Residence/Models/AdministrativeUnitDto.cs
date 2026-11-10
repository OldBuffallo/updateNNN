using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for AdministrativeUnit; never tracked by EF.</summary>
public sealed class AdministrativeUnitDto
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
    public AdministrativeUnitDto? Parent { get; set; }
    public AdministrativeUnitDto? Predecessor { get; set; }
}
