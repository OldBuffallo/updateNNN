namespace IRM.Data.Models;

/// <summary>
/// Bảng Investment — Vốn đầu tư của công ty
/// </summary>
public class Investment
{
    public int IDInvestment { get; set; }
    public string? Name { get; set; }
    public string? Nationality { get; set; }
    public decimal AmountOfMoney { get; set; }
    public int IDCompany { get; set; }
    public string? Passport { get; set; }

    public Company? Company { get; set; }
}
