

namespace IRM.Data.Models;

public class MigrationIssue
{
    public long Id { get; set; }
    public string SourceType { get; set; } = "";
    public string? SourceId { get; set; }
    public string IssueCode { get; set; } = "";
    public string Description { get; set; } = "";
    public string StatusCode { get; set; } = "OPEN";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
