using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for ElectronicIdentity; never tracked by EF.</summary>
public sealed class ElectronicIdentityDto
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public string IdentityNumber { get; set; } = "";
    public string IdentitySearchKey { get; set; } = "";
    public DateTime? IssuedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string StatusCode { get; set; } = "ACTIVE";
    public ForeignPersonDto? ForeignPerson { get; set; }
}
