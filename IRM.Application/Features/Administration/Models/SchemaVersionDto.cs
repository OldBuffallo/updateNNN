using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for SchemaVersion; never tracked by EF.</summary>
public sealed class SchemaVersionDto
{
    public int Id { get; set; }
    public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    public string Checksum { get; set; } = "";
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public string AppliedBy { get; set; } = "";
}
