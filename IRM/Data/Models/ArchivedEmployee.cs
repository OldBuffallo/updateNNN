namespace IRM.Data.Models;

/// <summary>
/// Bảng ArchivedEmployees — Lưu trữ lao động đã hết hạn (di chuyển từ Employees)
/// </summary>
public class ArchivedEmployee
{
    public long Id { get; set; }

    // ── Thông tin gốc từ bảng Employees ──
    public int OriginalId { get; set; }           // IDEmployee gốc
    public string StaffName { get; set; } = "";
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Nationality { get; set; }
    public string? Passport { get; set; }
    public string? Address { get; set; }
    public int? IDCareer { get; set; }
    public int WorkPermit { get; set; }
    public string? WorkPermitNumber { get; set; }
    public string? VisaNumber { get; set; }
    public DateTime? TemporaryStay { get; set; }
    public string? Note { get; set; }
    public int SettlementResults { get; set; }
    public string? SettlementResultsString { get; set; }
    public int IDUser { get; set; }
    public int IDCompany { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? CardCreationDate { get; set; }
    public int WorkingStatus { get; set; }
    public DateTime? DateOfJoin { get; set; }
    public DateTime? DateOfLeave { get; set; }

    // Thăm thân
    public int FamilyVisit { get; set; }
    public string? FamilyVisitRelativeName { get; set; }
    public string? FamilyVisitRelationship { get; set; }
    public string? FamilyVisitRelativeIdCard { get; set; }
    public DateTime? FamilyVisitStartDate { get; set; }
    public DateTime? FamilyVisitEndDate { get; set; }
    public string? FamilyVisitNote { get; set; }

    // ── Metadata lưu trữ ──
    public string? CompanyName { get; set; }       // Snapshot tên công ty tại thời điểm archive
    public string? CareerName { get; set; }        // Snapshot tên nghề nghiệp
    public string ArchiveReason { get; set; } = "";// "HET_HAN_TAM_TRU", "HET_HAN_THAM_THAN"
    public string? ArchivedBy { get; set; }        // Username thực hiện
    public DateTime ArchivedAt { get; set; } = DateTime.Now;
}
