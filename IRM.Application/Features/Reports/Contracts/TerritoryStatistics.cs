using IRM.Data.Models;

namespace IRM.Services;

public sealed class TerritoryStatistics
{
    public string AdministrativeUnitName { get; set; } = "";
    public int TotalForeigners { get; set; }
    public Dictionary<string, int> PeopleByPurpose { get; set; } = new();
    public int CompanyCount { get; set; }
    public int AccommodationCount { get; set; }
    public List<CompanySummary> Companies { get; set; } = [];
    public List<AccommodationSummary> Accommodations { get; set; } = [];
}
