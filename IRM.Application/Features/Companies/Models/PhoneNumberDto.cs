using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for PhoneNumber; never tracked by EF.</summary>
public sealed class PhoneNumberDto
{
    public int IDPhoneNumber { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public int IDCompany { get; set; }
    public int Delete_flag { get; set; }
    public CompanyDto? Company { get; set; }
}
