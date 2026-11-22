using IRM.Data.Models;

namespace IRM.Services;

/// <summary>
/// Dữ liệu đầu vào khi thêm/sửa hồ sơ người nước ngoài hợp nhất (5 tab).
/// </summary>
public sealed class UnifiedForeignPersonInput
{
    public int? ForeignPersonId { get; set; }
    public int? EmployeeId { get; set; }
    public int? StudentId { get; set; }
    public int? FamilyVisitStayCaseId { get; set; }

    // ── Tab 1: Thông tin chung & Cư trú ──
    public string FullName { get; set; } = "";
    public int Gender { get; set; } = 1;
    public DateTime? Birthday { get; set; }
    public string? NationalityCode { get; set; }
    public string? PassportNumber { get; set; }
    public int? AdministrativeUnitId { get; set; }
    public string? ResidenceAddress { get; set; }
    public string? Note { get; set; }

    // ── Công tắc diện hồ sơ ──
    public bool HasWorkProfile { get; set; }
    public bool HasStudyProfile { get; set; }
    public bool HasFamilyVisitProfile { get; set; }
    public string PrimaryPurposeCode { get; set; } = StayPurposeCodes.Work;

    // ── Tab 2: Lao động & Việc làm ──
    public int? CompanyId { get; set; }
    public int? CareerId { get; set; }
    public WorkPermitType WorkPermit { get; set; } = WorkPermitType.WorkerNoPermit;
    public string? WorkPermitNumber { get; set; }
    public DateTime? TemporaryStay { get; set; }

    // ── Tab 3: Du học sinh ──
    public string? SchoolName { get; set; }
    public string? Major { get; set; }
    public EducationLevel EducationLevel { get; set; } = EducationLevel.University;
    public ScholarshipType ScholarshipType { get; set; } = ScholarshipType.SelfFunded;
    public DateTime? EnrollmentDate { get; set; }
    public DateTime? GraduationDate { get; set; }
    public StudentStatus StudentStatus { get; set; } = StudentStatus.Studying;
    public DateTime? VisaExpiry { get; set; }
    public string? PhoneNumber { get; set; }

    // ── Tab 4: Thăm thân ──
    public string? RelativeName { get; set; }
    public string? Relationship { get; set; }
    public string RelativeTypeCode { get; set; } = "VIETNAMESE_CITIZEN";
    public string? RelativeNationalityCode { get; set; }
    public string? RelativeIdNumber { get; set; }
    public string? RelativePhone { get; set; }
    public string? RelativeAddress { get; set; }
    public DateTime? VisitStartDate { get; set; } = DateTime.Today;
    public DateTime? VisitEndDate { get; set; }
    public int? SponsorCompanyId { get; set; }
    public int? SponsorForeignPersonId { get; set; }

    // ── Tab 5: Giấy tờ XNC & Định danh điện tử ──
    public string? DocumentTypeCode { get; set; } = ImmigrationDocumentTypeCodes.Visa;
    public string? DocumentNumber { get; set; }
    public string? VisaSymbol { get; set; }
    public DateTime? DocumentValidFrom { get; set; } = DateTime.Today;
    public DateTime? DocumentValidTo { get; set; }
    public string? ElectronicIdentityNumber { get; set; }
    public DateTime? ElectronicIdentityValidFrom { get; set; } = DateTime.Today;
    public DateTime? ElectronicIdentityValidTo { get; set; }
}
