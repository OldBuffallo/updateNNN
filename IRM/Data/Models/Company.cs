namespace IRM.Data.Models;

/// <summary>
/// Bảng Companies — Công ty sử dụng lao động nước ngoài
/// </summary>
public class Company
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
    public string? UpdateDay { get; set; }
    public int Delete_flag { get; set; }

    // Navigation properties
    public Field? Field { get; set; }
    public Account? Tracker { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();
    public ICollection<Email> Emails { get; set; } = new List<Email>();
    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
    public ICollection<Attach> Attachments { get; set; } = new List<Attach>();

    // Computed
    public bool IsActive => Delete_flag == 0;
}
