using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Employee; never tracked by EF.</summary>
public sealed class EmployeeDto
{
    public int IDEmployee { get; set; }
    public string StaffName { get; set; } = "";
    public Gender Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Nationality { get; set; }  // FK → NationalityCode
    public string? Passport { get; set; }
    public string? Address { get; set; }
    public int? IDCareer { get; set; }
    public WorkPermitType WorkPermit { get; set; }
    public string? WorkPermitNumber { get; set; }
    public string? VisaNumber { get; set; }
    public string? VisaSymbol { get; set; }          // Ký hiệu Visa
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
    public CompanyDto? Company { get; set; }
    public CareerDto? Career { get; set; }
    public NationalityEntityDto? NationalityNav { get; set; }
    // Computed
    public string GenderString => Gender.ToDisplay();
    public bool IsVisible => Hidden_flag == 0;
    public bool IsWorking => WorkingStatus == 0;
    public string WorkPermitString => WorkPermit.ToDisplay();
    public int? DaysUntilExpiry => TemporaryStay.HasValue
        ? (int)(TemporaryStay.Value - DateTime.Today).TotalDays
        : null;
    public bool HasFamilyVisit => FamilyVisit == 1;
    public string FamilyVisitString => FamilyVisit == 1 ? "Có" : "Không";
    public int? DaysUntilFamilyVisitExpiry => FamilyVisitEndDate.HasValue
        ? (int)(FamilyVisitEndDate.Value - DateTime.Today).TotalDays
        : null;
}
