using IRM.Data.Models;

namespace IRM.Services;

public sealed class AccommodationSummary
{
    public int AccommodationId { get; set; }
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public int CurrentResidentCount { get; set; }
    public string Companies { get; set; } = "";
}
