

namespace IRM.Data.Models;

public class ForeignPersonSourceLink
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public string SourceType { get; set; } = "";
    public string SourceId { get; set; } = "";
    public string? SourceFingerprint { get; set; }
    public DateTime LastSynchronizedAt { get; set; } = DateTime.UtcNow;

    public ForeignPerson? ForeignPerson { get; set; }
}
