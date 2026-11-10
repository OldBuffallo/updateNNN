using IRM.Data.Models;

namespace IRM.Services;

public sealed class FamilyVisitInput
{
    public int? ForeignPersonId { get; set; }
    public int? StayCaseId { get; set; }
    public string FullName { get; set; } = "";
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? NationalityCode { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.Today;
    public DateTime? ValidTo { get; set; }
    public string RelativeName { get; set; } = "";
    public string Relationship { get; set; } = "";
    public string RelativeTypeCode { get; set; } = "VIETNAMESE_CITIZEN";
    public string? RelativeIdNumber { get; set; }
    public string? RelativePhone { get; set; }
    public string? RelativeAddress { get; set; }
    public int? SponsorForeignPersonId { get; set; }
    public int? SponsorCompanyId { get; set; }
    public int? AdministrativeUnitId { get; set; }
    public string? ResidenceAddress { get; set; }
    public string? ElectronicIdentityNumber { get; set; }
    public string? DocumentTypeCode { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? DocumentValidTo { get; set; }
    public string? Note { get; set; }
}
