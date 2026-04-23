namespace IRM.Data.Models;

/// <summary>
/// Bảng Employees — Lao động nước ngoài
/// </summary>
public class Employee
{
    public int IDEmployee { get; set; }
    public string StaffName { get; set; } = "";
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Nationality { get; set; }  // FK → NationalityCode
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
    public string? CardCreationDate { get; set; }
    public int WorkingStatus { get; set; }
    public DateTime? DateOfJoin { get; set; }
    public DateTime? DateOfLeave { get; set; }
    public int Hidden_flag { get; set; }

    // Thăm thân (Family Visit)
    public int FamilyVisit { get; set; }                    // 0=Không, 1=Có
    public string? FamilyVisitRelativeName { get; set; }    // Tên người thân bảo lãnh
    public string? FamilyVisitRelationship { get; set; }    // Mối quan hệ: Vợ/Chồng/Con/Cha mẹ
    public string? FamilyVisitRelativeIdCard { get; set; }  // CMND/CCCD người thân
    public DateTime? FamilyVisitStartDate { get; set; }     // Ngày bắt đầu thăm thân
    public DateTime? FamilyVisitEndDate { get; set; }       // Ngày hết hạn thăm thân
    public string? FamilyVisitNote { get; set; }            // Ghi chú thăm thân

    // Navigation
    public Company? Company { get; set; }
    public Career? Career { get; set; }
    public NationalityEntity? NationalityNav { get; set; }

    // Computed
    public string GenderString => Gender == 1 ? "Nam" : "Nữ";
    public bool IsVisible => Hidden_flag == 0;
    public bool IsWorking => WorkingStatus == 0;

    public string WorkPermitString => WorkPermit switch
    {
        0 => "NLĐ miễn GPLĐ",
        1 => "NLĐ đã có GPLĐ",
        2 => "NLĐ chưa có GPLĐ",
        3 => "Nhà đầu tư miễn GPLĐ",
        4 => "Nhà đầu tư đã có GPLĐ",
        5 => "Nhà đầu tư chưa có GPLĐ",
        _ => "Khác"
    };

    public int? DaysUntilExpiry => TemporaryStay.HasValue
        ? (int)(TemporaryStay.Value - DateTime.Today).TotalDays
        : null;

    public bool HasFamilyVisit => FamilyVisit == 1;
    public string FamilyVisitString => FamilyVisit == 1 ? "Có" : "Không";
    public int? DaysUntilFamilyVisitExpiry => FamilyVisitEndDate.HasValue
        ? (int)(FamilyVisitEndDate.Value - DateTime.Today).TotalDays
        : null;
}
