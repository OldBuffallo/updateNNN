namespace IRM.Data.Models;

// ══════════════════════════════════════════════
//  Enums — Thay thế magic numbers trong Legacy
//  Giá trị int giữ nguyên khớp database column.
// ══════════════════════════════════════════════

/// <summary>Giới tính: 0 = Nữ, 1 = Nam</summary>
public enum Gender
{
    Female = 0,  // Nữ
    Male = 1     // Nam
}

/// <summary>Loại GPLĐ (6 giá trị)</summary>
public enum WorkPermitType
{
    WorkerExempt = 0,      // NLĐ miễn GPLĐ
    WorkerHasPermit = 1,   // NLĐ đã có GPLĐ
    WorkerNoPermit = 2,    // NLĐ chưa có GPLĐ
    InvestorExempt = 3,    // NĐT miễn GPLĐ
    InvestorHasPermit = 4, // NĐT đã có GPLĐ
    InvestorNoPermit = 5   // NĐT chưa có GPLĐ
}

/// <summary>Quyền tài khoản</summary>
public enum AccountPermission
{
    User = 0,
    Admin = 1
}

/// <summary>Trình độ đào tạo du học sinh</summary>
public enum EducationLevel
{
    University = 0,  // Đại học
    Master = 1,      // Thạc sĩ
    Doctorate = 2,   // Tiến sĩ
    ShortTerm = 3,   // Ngắn hạn
    Other = 4        // Khác
}

/// <summary>Loại học bổng du học sinh</summary>
public enum ScholarshipType
{
    SelfFunded = 0,          // Tự túc
    VietnamGovernment = 1,   // HB Chính phủ VN
    ForeignScholarship = 2,  // HB nước ngoài
    Other = 3                // Khác
}

/// <summary>Trạng thái du học sinh</summary>
public enum StudentStatus
{
    Studying = 0,    // Đang học
    Graduated = 1,   // Đã tốt nghiệp
    OnLeave = 2,     // Tạm nghỉ
    Returned = 3     // Đã về nước
}

/// <summary>Loại file đính kèm công ty</summary>
public enum AttachType
{
    BC_NNN = 0,  // Báo cáo NNN
    HSPN = 1     // Hồ sơ pháp nhân
}

// ══════════════════════════════════════════════
//  Extension methods — Display strings
// ══════════════════════════════════════════════

public static class EnumExtensions
{
    public static string ToDisplay(this Gender gender) => gender switch
    {
        Gender.Male => "Nam",
        Gender.Female => "Nữ",
        _ => "Khác"
    };

    public static string ToDisplay(this WorkPermitType wp) => wp switch
    {
        WorkPermitType.WorkerExempt => "NLĐ miễn GPLĐ",
        WorkPermitType.WorkerHasPermit => "NLĐ đã có GPLĐ",
        WorkPermitType.WorkerNoPermit => "NLĐ chưa có GPLĐ",
        WorkPermitType.InvestorExempt => "NĐT miễn GPLĐ",
        WorkPermitType.InvestorHasPermit => "NĐT đã có GPLĐ",
        WorkPermitType.InvestorNoPermit => "NĐT chưa có GPLĐ",
        _ => "Khác"
    };

    public static string ToDisplay(this AccountPermission perm) => perm switch
    {
        AccountPermission.Admin => "Admin",
        AccountPermission.User => "User",
        _ => "Khác"
    };

    public static string ToDisplay(this EducationLevel level) => level switch
    {
        EducationLevel.University => "Đại học",
        EducationLevel.Master => "Thạc sĩ",
        EducationLevel.Doctorate => "Tiến sĩ",
        EducationLevel.ShortTerm => "Ngắn hạn",
        EducationLevel.Other => "Khác",
        _ => "Khác"
    };

    public static string ToDisplay(this ScholarshipType st) => st switch
    {
        ScholarshipType.SelfFunded => "Tự túc",
        ScholarshipType.VietnamGovernment => "HB Chính phủ VN",
        ScholarshipType.ForeignScholarship => "HB nước ngoài",
        ScholarshipType.Other => "Khác",
        _ => "Khác"
    };

    public static string ToDisplay(this StudentStatus status) => status switch
    {
        StudentStatus.Studying => "Đang học",
        StudentStatus.Graduated => "Đã tốt nghiệp",
        StudentStatus.OnLeave => "Tạm nghỉ",
        StudentStatus.Returned => "Đã về nước",
        _ => "Khác"
    };

    public static string ToDisplay(this AttachType type) => type switch
    {
        AttachType.BC_NNN => "BC_NNN",
        AttachType.HSPN => "HSPN",
        _ => ""
    };
}
