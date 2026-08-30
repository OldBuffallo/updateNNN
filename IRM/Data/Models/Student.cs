namespace IRM.Data.Models;

/// <summary>
/// Bảng Students — Du học sinh nước ngoài
/// Thiết kế theo pattern của Employee, phù hợp với CSDL hiện tại.
/// </summary>
public class Student
{
    public int IDStudent { get; set; }
    public string FullName { get; set; } = "";
    public Gender Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Nationality { get; set; }       // FK → NationalityCode
    public string? Passport { get; set; }
    public string? Address { get; set; }           // Địa chỉ tạm trú tại VN

    // Thông tin học tập
    public string? SchoolName { get; set; }        // Tên trường
    public string? Major { get; set; }             // Ngành học
    public string? StudentCode { get; set; }       // Mã số sinh viên
    public EducationLevel EducationLevel { get; set; }
    public DateTime? EnrollmentDate { get; set; }  // Ngày nhập học
    public DateTime? ExpectedGraduation { get; set; } // Dự kiến tốt nghiệp

    // Visa & tạm trú
    public string? VisaNumber { get; set; }
    public DateTime? VisaExpiry { get; set; }      // Hạn visa
    public DateTime? TemporaryStay { get; set; }   // Hạn tạm trú

    // Học bổng
    public ScholarshipType ScholarshipType { get; set; }

    // Trạng thái & quản trị
    public StudentStatus Status { get; set; }
    public string? Note { get; set; }
    public int IDUser { get; set; }
    public DateTime? DateCreated { get; set; }
    public int Hidden_flag { get; set; }           // 0=Hiện, 1=Ẩn (soft delete)

    // Navigation
    public NationalityEntity? NationalityNav { get; set; }

    // ══════ Computed Properties ══════
    public string GenderString => Gender.ToDisplay();
    public bool IsVisible => Hidden_flag == 0;

    public string EducationLevelString => EducationLevel.ToDisplay();
    public string ScholarshipTypeString => ScholarshipType.ToDisplay();
    public string StatusString => Status.ToDisplay();

    public int? DaysUntilVisaExpiry => VisaExpiry.HasValue
        ? (int)(VisaExpiry.Value - DateTime.Today).TotalDays
        : null;

    public int? DaysUntilStayExpiry => TemporaryStay.HasValue
        ? (int)(TemporaryStay.Value - DateTime.Today).TotalDays
        : null;
}
