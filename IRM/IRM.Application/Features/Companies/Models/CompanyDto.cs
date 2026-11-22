using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Company; never tracked by EF.</summary>
public sealed class CompanyDto
{
    public int IDCompany { get; set; }
    public string CompanyName { get; set; } = "";
    public string? TypeOfBusiniess { get; set; }
    public int IDField { get; set; }
    public string? LegalRepresentative { get; set; }
    public string? Address { get; set; }
    public int TotalAmount { get; set; }
    public int AmountOfExemption { get; set; }
    public int QuantityAvailable { get; set; }
    public int QuantityNotYet { get; set; }
    public int NumberOfPersonalities { get; set; }
    public string? RegistrationProfile { get; set; }
    public int RegistrationProfileIndex { get; set; }
    public string? DescriptionOfActivities { get; set; }
    public int TrackerID { get; set; }
    public string? Note { get; set; }
    public DateTime? UpdateDay { get; set; }
    public int Delete_flag { get; set; }
    // Navigation properties
    public FieldDto? Field { get; set; }
    public AccountDto? Tracker { get; set; }
    public ICollection<EmployeeDto> Employees { get; set; } = new List<EmployeeDto>();
    public ICollection<PhoneNumberDto> PhoneNumbers { get; set; } = new List<PhoneNumberDto>();
    public ICollection<EmailDto> Emails { get; set; } = new List<EmailDto>();
    public ICollection<InvestmentDto> Investments { get; set; } = new List<InvestmentDto>();
    public ICollection<AttachDto> Attachments { get; set; } = new List<AttachDto>();
    // Computed
    public bool IsActive => Delete_flag == 0;
}
