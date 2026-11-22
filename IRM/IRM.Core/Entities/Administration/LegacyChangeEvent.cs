

namespace IRM.Data.Models;

public class LegacyChangeEvent
{
    public long Id { get; set; }
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string Operation { get; set; } = "";
    public string? BeforeXml { get; set; }
    public string? AfterXml { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string StatusCode { get; set; } = "PENDING";
    public string? LastError { get; set; }
}
