

namespace IRM.Data.Models;

/// <summary>Kết quả import</summary>
public class ImportResult
{
    public string SessionId { get; set; } = "";
    public int TotalRows { get; set; }
    public int AddedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int ErrorCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success { get; set; }
}
