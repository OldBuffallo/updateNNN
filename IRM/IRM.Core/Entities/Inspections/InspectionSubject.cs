

namespace IRM.Data.Models;

public class InspectionSubject
{
    public int Id { get; set; }
    public int InspectionId { get; set; }
    public int ForeignPersonId { get; set; }
    public string PurposeCodeSnapshot { get; set; } = "OTHER";
    public string? DocumentsSnapshot { get; set; }
    public string? WorkDescriptionSnapshot { get; set; }
    public string? AddressSnapshot { get; set; }
    public string ResultCode { get; set; } = "VALID";
    public string? ViolationDetails { get; set; }
    public string? ActionTaken { get; set; }
    public string? Note { get; set; }

    public Inspection? Inspection { get; set; }
    public ForeignPerson? ForeignPerson { get; set; }
}
