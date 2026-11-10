using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for InspectionSubject; never tracked by EF.</summary>
public sealed class InspectionSubjectDto
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
    public InspectionDto? Inspection { get; set; }
    public ForeignPersonDto? ForeignPerson { get; set; }
}
