using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Investment; never tracked by EF.</summary>
public sealed class InvestmentDto
{
    public int IDInvestment { get; set; }
    public string? Name { get; set; }
    public string? Nationality { get; set; }
    public decimal AmountOfMoney { get; set; }
    public int IDCompany { get; set; }
    public string? Passport { get; set; }
    public CompanyDto? Company { get; set; }
}
