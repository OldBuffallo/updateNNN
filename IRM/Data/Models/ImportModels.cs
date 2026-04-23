namespace IRM.Data.Models;

/// <summary>
/// DTOs cho quy trình Import Excel
/// </summary>

/// <summary>Thông tin một cột trong file Excel</summary>
public class ExcelColumnInfo
{
    public int Index { get; set; }
    public string HeaderName { get; set; } = "";
    public string? SampleValue { get; set; }
}

/// <summary>Mapping giữa cột Excel → trường hệ thống</summary>
public class ColumnMapping
{
    public int ExcelColumnIndex { get; set; }
    public string ExcelColumnName { get; set; } = "";
    public string SystemField { get; set; } = "";          // e.g. "StaffName", "Gender", "skip"
    public string SystemFieldLabel { get; set; } = "";     // e.g. "Họ tên", "Giới tính"
    public bool IsAutoMapped { get; set; }
}

/// <summary>Dòng xem trước import</summary>
public class ImportPreviewRow
{
    public int RowNumber { get; set; }
    public string Status { get; set; } = "new";  // "new", "duplicate", "error"
    public string? ErrorMessage { get; set; }
    public int? ExistingEmployeeId { get; set; }  // ID nhân viên trùng (nếu có)

    // Dữ liệu đã parse
    public string StaffName { get; set; } = "";
    public string? GenderDisplay { get; set; }
    public string? BirthdayDisplay { get; set; }
    public string? NationalityDisplay { get; set; }
    public string? Passport { get; set; }
    public string? Address { get; set; }
    public string? CareerDisplay { get; set; }
    public string? WorkPermitDisplay { get; set; }
    public string? VisaNumber { get; set; }
    public string? TemporaryStayDisplay { get; set; }

    // Dữ liệu thực tế (để insert/update)
    public Employee? ParsedEmployee { get; set; }
}

/// <summary>Kết quả import</summary>
public class ImportResult
{
    public string SessionId { get; set; } = "";
    public int TotalRows { get; set; }
    public int AddedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int ErrorCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success { get; set; }
}

/// <summary>Thông tin phiên import cho hiển thị UI</summary>
public class ImportSessionInfo
{
    public long Id { get; set; }
    public string SessionId { get; set; } = "";
    public string Time { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Company { get; set; } = "";
    public int Added { get; set; }
    public int Updated { get; set; }
    public int Errors { get; set; }
    public string Status { get; set; } = "";
    public string? User { get; set; }
}

/// <summary>Các trường hệ thống có thể map</summary>
public static class SystemFields
{
    public static readonly Dictionary<string, string> All = new()
    {
        { "StaffName",     "Họ tên" },
        { "Gender",        "Giới tính" },
        { "Birthday",      "Ngày sinh" },
        { "Nationality",   "Quốc tịch" },
        { "Passport",      "Số hộ chiếu" },
        { "Address",       "Địa chỉ" },
        { "Career",        "Nghề nghiệp/Chức danh" },
        { "WorkPermit",    "GPLĐ" },
        { "WorkPermitNumber", "Số GPLĐ" },
        { "VisaNumber",    "Số Visa" },
        { "TemporaryStay", "Hạn tạm trú" },
        { "Note",          "Ghi chú" },
        // Thăm thân
        { "FamilyVisit",              "Thăm thân (Có/Không)" },
        { "FamilyVisitRelativeName",  "Tên người thân" },
        { "FamilyVisitRelationship",  "Quan hệ người thân" },
        { "FamilyVisitRelativeIdCard","CMND/CCCD người thân" },
        { "FamilyVisitStartDate",     "Ngày bắt đầu thăm thân" },
        { "FamilyVisitEndDate",       "Hạn thăm thân" },
        { "FamilyVisitNote",          "Ghi chú thăm thân" },
        { "skip",          "— Bỏ qua —" }
    };

    /// <summary>Keywords tự động nhận dạng cột Excel → trường hệ thống</summary>
    public static readonly Dictionary<string, string[]> AutoMapKeywords = new()
    {
        { "StaffName",     new[] { "họ tên", "ho ten", "full name", "name", "tên", "ten", "họ và tên" } },
        { "Gender",        new[] { "giới tính", "gioi tinh", "gender", "phái", "gt" } },
        { "Birthday",      new[] { "ngày sinh", "ngay sinh", "birthday", "dob", "sinh" } },
        { "Nationality",   new[] { "quốc tịch", "quoc tich", "nationality", "nước", "nuoc" } },
        { "Passport",      new[] { "hộ chiếu", "ho chieu", "passport", "hc", "số hộ chiếu", "so ho chieu" } },
        { "Address",       new[] { "địa chỉ", "dia chi", "address", "đ/c" } },
        { "Career",        new[] { "nghề nghiệp", "nghe nghiep", "career", "chức danh", "chuc danh", "vị trí", "vi tri", "job" } },
        { "WorkPermit",    new[] { "gplđ", "gpld", "work permit", "giấy phép" } },
        { "WorkPermitNumber", new[] { "số gplđ", "so gpld", "wp number" } },
        { "VisaNumber",    new[] { "visa", "số visa", "so visa", "visa number" } },
        { "TemporaryStay", new[] { "tạm trú", "tam tru", "temporary stay", "hạn tạm trú", "han tam tru" } },
        { "Note",          new[] { "ghi chú", "ghi chu", "note", "chú thích" } },
        // Thăm thân
        { "FamilyVisit",              new[] { "thăm thân", "tham than", "family visit", "diện thăm thân", "dien tham than" } },
        { "FamilyVisitRelativeName",  new[] { "tên người thân", "ten nguoi than", "người bảo lãnh", "nguoi bao lanh", "relative name" } },
        { "FamilyVisitRelationship",  new[] { "quan hệ", "quan he", "mối quan hệ", "relationship", "moi quan he" } },
        { "FamilyVisitRelativeIdCard",new[] { "cmnd người thân", "cccd người thân", "cmnd nguoi than", "id card", "căn cước người thân" } },
        { "FamilyVisitStartDate",     new[] { "ngày thăm thân", "ngay tham than", "bắt đầu thăm thân", "bat dau tham than" } },
        { "FamilyVisitEndDate",       new[] { "hạn thăm thân", "han tham than", "hết hạn thăm thân", "het han tham than", "family visit expiry" } },
    };
}
