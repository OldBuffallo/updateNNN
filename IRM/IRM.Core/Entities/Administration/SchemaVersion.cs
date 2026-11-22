

namespace IRM.Data.Models;

public class SchemaVersion
{
    public int Id { get; set; }
    public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    public string Checksum { get; set; } = "";
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public string AppliedBy { get; set; } = "";
}
