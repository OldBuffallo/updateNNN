using IRM.Data.Models;

namespace IRM.Services;

public sealed class ZoneStatistics
{
    public int EconomicZoneId { get; set; }
    public string ZoneName { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public int CompanyCount { get; set; }
    public int ForeignPersonCount { get; set; }
}
