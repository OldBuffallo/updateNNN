using IRM.Data.Models;

namespace IRM.Services;

public sealed class DocumentIdentityStatistics
{
    public Dictionary<string, int> DocumentsByType { get; set; } = new();
    public int WithElectronicIdentity { get; set; }
    public int WithoutElectronicIdentity { get; set; }
}
