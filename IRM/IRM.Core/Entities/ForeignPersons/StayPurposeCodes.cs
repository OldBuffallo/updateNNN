

namespace IRM.Data.Models;

public static class StayPurposeCodes
{
    public const string Work = "WORK";
    public const string FamilyVisit = "FAMILY_VISIT";
    public const string Study = "STUDY";
    public const string Tourism = "TOURISM";
    public const string BorderVisaExempt = "BORDER_VISA_EXEMPT";
    public const string Other = "OTHER";

    public static readonly string[] All = [Work, FamilyVisit, Study, Tourism, BorderVisaExempt, Other];
}
