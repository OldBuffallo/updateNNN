

namespace IRM.Data.Models;

public class ImmigrationDocument
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

    public ForeignPerson? ForeignPerson { get; set; }
}
