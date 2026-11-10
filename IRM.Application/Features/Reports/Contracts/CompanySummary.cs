using IRM.Data.Models;

namespace IRM.Services;

public sealed class CompanySummary
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = "";
    public string OwnershipTypeCode { get; set; } = "";
    public int CurrentWorkerCount { get; set; }
}
