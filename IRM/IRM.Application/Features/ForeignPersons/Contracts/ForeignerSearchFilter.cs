using IRM.Data.Models;

namespace IRM.Services;

public sealed class ForeignerSearchFilter
{
    public string? Search { get; set; }
    public string? PurposeCode { get; set; }
    public int? AdministrativeUnitId { get; set; }
    public DateTime AsOfDate { get; set; } = DateTime.Today;
}
