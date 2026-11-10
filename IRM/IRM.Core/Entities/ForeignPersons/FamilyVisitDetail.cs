

namespace IRM.Data.Models;

public class FamilyVisitDetail
{
    public int StayCaseId { get; set; }
    public string RelativeName { get; set; } = "";
    public string? RelativeIdNumber { get; set; }
    public string? RelativePhone { get; set; }
    public string? RelativeAddress { get; set; }
    public string Relationship { get; set; } = "";
    public string RelativeTypeCode { get; set; } = "VIETNAMESE_CITIZEN";
    public string? RelativeNationalityCode { get; set; }

    public StayCase? StayCase { get; set; }
}
