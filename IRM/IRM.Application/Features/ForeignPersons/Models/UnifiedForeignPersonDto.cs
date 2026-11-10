using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>
/// Hồ sơ người nước ngoài hợp nhất (kết hợp ForeignPerson, Employee, Student và FamilyVisit).
/// </summary>
public sealed class UnifiedForeignPersonDto
{
    public int? ForeignPersonId { get; set; }
    public int? EmployeeId { get; set; }
    public int? StudentId { get; set; }
    public int? FamilyVisitStayCaseId { get; set; }

    public string FullName { get; set; } = "";
    public int Gender { get; set; } = 1;
    public DateTime? Birthday { get; set; }
    public string? NationalityCode { get; set; }
    public string? NationalityName { get; set; }
    public string? PassportNumber { get; set; }
    public string? ResidenceAddress { get; set; }
    public int? AdministrativeUnitId { get; set; }
    public string? AdministrativeUnitName { get; set; }
    public string? Note { get; set; }

    public bool HasWorkProfile { get; set; }
    public bool HasStudyProfile { get; set; }
    public bool HasFamilyVisitProfile { get; set; }
    public List<string> PurposeCodes { get; set; } = new();

    // ── Lao động (Employee) ──
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public int? CareerId { get; set; }
    public string? CareerName { get; set; }
    public WorkPermitType WorkPermit { get; set; } = WorkPermitType.WorkerNoPermit;
    public string? WorkPermitNumber { get; set; }
    public DateTime? TemporaryStay { get; set; }
    public string? VisaNumber { get; set; }
    public string? VisaSymbol { get; set; }

    // ── Du học sinh (Student) ──
    public string? SchoolName { get; set; }
    public string? Major { get; set; }
    public EducationLevel EducationLevel { get; set; } = EducationLevel.University;
    public ScholarshipType ScholarshipType { get; set; } = ScholarshipType.SelfFunded;
    public DateTime? EnrollmentDate { get; set; }
    public DateTime? GraduationDate { get; set; }
    public StudentStatus StudentStatus { get; set; } = StudentStatus.Studying;
    public DateTime? VisaExpiry { get; set; }
    public string? PhoneNumber { get; set; }

    // ── Thăm thân (FamilyVisit) ──
    public string? RelativeName { get; set; }
    public string? Relationship { get; set; }
    public string RelativeTypeCode { get; set; } = "VIETNAMESE_CITIZEN";
    public string? RelativeNationalityCode { get; set; }
    public string? RelativeIdNumber { get; set; }
    public string? RelativePhone { get; set; }
    public string? RelativeAddress { get; set; }
    public DateTime? VisitStartDate { get; set; }
    public DateTime? VisitEndDate { get; set; }

    // ── Giấy tờ XNC & Định danh điện tử ──
    public string? ElectronicIdNumber { get; set; }
    public DateTime? ElectronicIdValidFrom { get; set; }
    public DateTime? ElectronicIdValidTo { get; set; }
    public int ValidDocumentCount { get; set; }
    public string? PrimaryDocumentType { get; set; }
    public string? PrimaryDocumentNumber { get; set; }
    public DateTime? PrimaryDocumentValidFrom { get; set; }
    public DateTime? PrimaryDocumentValidTo { get; set; }
    public List<ImmigrationDocumentDto> ImmigrationDocuments { get; set; } = new();
    public List<ElectronicIdentityDto> ElectronicIdentities { get; set; } = new();

    public string GenderString => Gender == 1 ? "Nam" : "Nữ";

    public string WorkPermitString => WorkPermit switch
    {
        WorkPermitType.WorkerExempt => "Miễn GPLĐ",
        WorkPermitType.WorkerHasPermit => "Đã có GPLĐ",
        WorkPermitType.WorkerNoPermit => "Chưa có GPLĐ",
        WorkPermitType.InvestorExempt => "NĐT - Miễn GPLĐ",
        WorkPermitType.InvestorHasPermit => "NĐT - Có GPLĐ",
        WorkPermitType.InvestorNoPermit => "NĐT - Chưa có",
        _ => "Không xác định"
    };

    public string EducationLevelString => EducationLevel switch
    {
        EducationLevel.University => "Đại học",
        EducationLevel.Master => "Thạc sĩ",
        EducationLevel.Doctorate => "Tiến sĩ",
        EducationLevel.ShortTerm => "Ngắn hạn",
        _ => "Khác"
    };

    public string ScholarshipTypeString => ScholarshipType switch
    {
        ScholarshipType.SelfFunded => "Tự túc",
        ScholarshipType.VietnamGovernment => "HB Chính phủ VN",
        ScholarshipType.ForeignScholarship => "HB nước ngoài",
        _ => "Khác"
    };

    public string StudentStatusString => StudentStatus switch
    {
        StudentStatus.Studying => "Đang học",
        StudentStatus.Graduated => "Đã tốt nghiệp",
        StudentStatus.OnLeave => "Tạm nghỉ",
        StudentStatus.Returned => "Đã về nước",
        _ => "Đang học"
    };

    public string RelativeTypeString => RelativeTypeCode switch
    {
        "VIETNAMESE_CITIZEN" => "Công dân Việt Nam",
        "VIETNAMESE_ORIGIN" => "Người gốc Việt Nam",
        "OVERSEAS_VIETNAMESE_DUAL" => "Người VN định cư ở nước ngoài",
        _ => RelativeTypeCode
    };

    public DateTime? EffectiveExpiryDate =>
        TemporaryStay ?? VisaExpiry ?? VisitEndDate ?? PrimaryDocumentValidTo;

    public int? DaysUntilExpiry =>
        EffectiveExpiryDate.HasValue ? (int)(EffectiveExpiryDate.Value.Date - DateTime.Today).TotalDays : null;

    public bool IsLegacyUnlinked => !ForeignPersonId.HasValue && (EmployeeId.HasValue || StudentId.HasValue);
}
