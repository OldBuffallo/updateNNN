using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for MigrationIssue; never tracked by EF.</summary>
public sealed class MigrationIssueDto
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
