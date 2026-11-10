

namespace IRM.Data.Models;

public class StoredFile
{
    public int Id { get; set; }
    public string StorageName { get; set; } = "";
    public string OriginalName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long Size { get; set; }
    public string Sha256 { get; set; } = "";
    public string ScanStatusCode { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByAccountId { get; set; }
    public bool IsDeleted { get; set; }
}
