using System.ComponentModel.DataAnnotations.Schema;

namespace IRM.Data.Models;

/// <summary>Projection dùng cho màn hình thăm thân; nguồn dữ liệu chuẩn là ForeignPerson/StayCase.</summary>
[NotMapped]
public class FamilyVisitor
{
    public int ForeignPersonId { get; set; }
    public int StayCaseId { get; set; }
    public string FullName { get; set; } = "";
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Nationality { get; set; }
    public string? Passport { get; set; }
    public string? Address { get; set; }
    public string? RelativeName { get; set; }
    public string? RelativeIdCard { get; set; }
    public string? RelativePhone { get; set; }
    public string? RelativeAddress { get; set; }
    public string? Relationship { get; set; }
    public string RelativeTypeCode { get; set; } = "VIETNAMESE_CITIZEN";
    public string? RelativeNationality { get; set; }
    public int? SponsorForeignPersonId { get; set; }
    public int? SponsorCompanyId { get; set; }
    public DateTime? VisitStartDate { get; set; }
    public DateTime? VisitEndDate { get; set; }
    public string? ElectronicIdNumber { get; set; }
    public string? Note { get; set; }

    public string GenderString => Gender == 1 ? "Nam" : "Nữ";
    public bool HasElectronicId => !string.IsNullOrWhiteSpace(ElectronicIdNumber);
    public string RelativeTypeString => RelativeTypeCode switch
    {
        "VIETNAMESE_CITIZEN" => "Công dân VN",
        "VIETNAMESE_ORIGIN" => "Người gốc VN",
        "OVERSEAS_VIETNAMESE_DUAL" => "Việt kiều 2 quốc tịch",
        _ => "Khác"
    };
}
