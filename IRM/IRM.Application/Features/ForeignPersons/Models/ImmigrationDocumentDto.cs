using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for ImmigrationDocument; never tracked by EF.</summary>
public sealed class ImmigrationDocumentDto
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public string TypeCode { get; set; } = ImmigrationDocumentTypeCodes.Visa;
    public string Number { get; set; } = "";
    public DateTime? IssuedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? IssuedBy { get; set; }
    public string StatusCode { get; set; } = "VALID";
    public string? Note { get; set; }
    public ForeignPersonDto? ForeignPerson { get; set; }
}
