namespace IRM.Data.Models;

/// <summary>
/// Bảng Students — Du học sinh nước ngoài
/// Thiết kế theo pattern của Employee, phù hợp với CSDL hiện tại.
/// </summary>
public class Student
{
    public int IDStudent { get; set; }
    public string FullName { get; set; } = "";
    public int Gender { get; set; }               // 0=Nữ, 1=Nam
    public DateTime? Birthday { get; set; }
    public string? Nationality { get; set; }       // FK → NationalityCode
    public string? Passport { get; set; }
    public string? Address { get; set; }           // Địa chỉ tạm trú tại VN

    // Thông tin học tập
    public string? SchoolName { get; set; }        // Tên trường
    public string? Major { get; set; }             // Ngành học
    public string? StudentCode { get; set; }       // Mã số sinh viên
    public int EducationLevel { get; set; }        // 0=Đại học, 1=Thạc sĩ, 2=Tiến sĩ, 3=Ngắn hạn, 4=Khác
    public DateTime? EnrollmentDate { get; set; }  // Ngày nhập học
    public DateTime? ExpectedGraduation { get; set; } // Dự kiến tốt nghiệp

    // Visa & tạm trú
    public string? VisaNumber { get; set; }
    public DateTime? VisaExpiry { get; set; }      // Hạn visa
    public DateTime? TemporaryStay { get; set; }   // Hạn tạm trú

    // Học bổng
    public int ScholarshipType { get; set; }       // 0=Tự túc, 1=HB Chính phủ VN, 2=HB nước ngoài, 3=Khác

    // Trạng thái & quản trị
    public int Status { get; set; }                // 0=Đang học, 1=Đã TN, 2=Tạm nghỉ, 3=Đã về nước
    public string? Note { get; set; }
    public int IDUser { get; set; }
    public DateTime? DateCreated { get; set; }
    public int Hidden_flag { get; set; }           // 0=Hiện, 1=Ẩn (soft delete)

    // Navigation
    public NationalityEntity? NationalityNav { get; set; }

    // ══════ Computed Properties ══════
    public string GenderString => Gender == 1 ? "Nam" : "Nữ";
    public bool IsVisible => Hidden_flag == 0;

    public string EducationLevelString => EducationLevel switch
    {
        0 => "Đại học",
        1 => "Thạc sĩ",
        2 => "Tiến sĩ",
        3 => "Ngắn hạn",
        4 => "Khác",
        _ => "Khác"
    };

    public string ScholarshipTypeString => ScholarshipType switch
    {
        0 => "Tự túc",
        1 => "HB Chính phủ VN",
        2 => "HB nước ngoài",
        3 => "Khác",
        _ => "Khác"
    };

    public string StatusString => Status switch
    {
        0 => "Đang học",
        1 => "Đã tốt nghiệp",
        2 => "Tạm nghỉ",
        3 => "Đã về nước",
        _ => "Khác"
    };

    public int? DaysUntilVisaExpiry => VisaExpiry.HasValue
        ? (int)(VisaExpiry.Value - DateTime.Today).TotalDays
        : null;

    public int? DaysUntilStayExpiry => TemporaryStay.HasValue
        ? (int)(TemporaryStay.Value - DateTime.Today).TotalDays
        : null;
}
