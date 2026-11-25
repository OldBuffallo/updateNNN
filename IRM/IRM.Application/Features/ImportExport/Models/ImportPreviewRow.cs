

namespace IRM.Data.Models;

/// <summary>Dòng xem trước import</summary>
public sealed record ImportFieldDiff(
    string FieldLabel,
    string OldValue,
    string NewValue,
    bool IsDestructiveClear = false);

public class ImportPreviewRow
{
    public int RowNumber { get; set; }
    public string Status { get; set; } = "new";  // "new", "duplicate", "error"
    public List<string> Warnings { get; set; } = new();
    public List<ImportFieldDiff> FieldChanges { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? ExistingFingerprint { get; set; }
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
    public string? VisaSymbol { get; set; }
    public string? TemporaryStayDisplay { get; set; }

    // Công ty (resolve từ Excel)
    public string? CompanyDisplay { get; set; }
    public int? ResolvedCompanyId { get; set; }

    // Dữ liệu thực tế (để insert/update)
    public Employee? ParsedEmployee { get; set; }
    public Dictionary<string, string> ExtendedFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
