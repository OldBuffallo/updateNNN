using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for ForeignPersonSourceLink; never tracked by EF.</summary>
public sealed class ForeignPersonSourceLinkDto
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public string SourceType { get; set; } = "";
    public string SourceId { get; set; } = "";
    public string? SourceFingerprint { get; set; }
    public DateTime LastSynchronizedAt { get; set; } = DateTime.UtcNow;
    public ForeignPersonDto? ForeignPerson { get; set; }
}
