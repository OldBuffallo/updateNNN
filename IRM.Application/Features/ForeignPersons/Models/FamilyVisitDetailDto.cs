using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for FamilyVisitDetail; never tracked by EF.</summary>
public sealed class FamilyVisitDetailDto
{
    public int StayCaseId { get; set; }
    public string RelativeName { get; set; } = "";
    public string? RelativeIdNumber { get; set; }
    public string? RelativePhone { get; set; }
    public string? RelativeAddress { get; set; }
    public string Relationship { get; set; } = "";
    public string RelativeTypeCode { get; set; } = "VIETNAMESE_CITIZEN";
    public string? RelativeNationalityCode { get; set; }
    public StayCaseDto? StayCase { get; set; }
}
