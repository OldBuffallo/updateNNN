using IRM.Data.Models;

namespace IRM.Services;

public sealed class TerritoryOverview
{
    public int AdministrativeUnitId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public int TotalForeigners { get; set; }
    public Dictionary<string, int> PeopleByPurpose { get; set; } = new();
}
