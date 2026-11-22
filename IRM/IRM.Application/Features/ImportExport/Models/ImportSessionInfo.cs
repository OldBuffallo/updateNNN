

namespace IRM.Data.Models;

/// <summary>Thông tin phiên import cho hiển thị UI</summary>
public class ImportSessionInfo
{
    public long Id { get; set; }
    public string SessionId { get; set; } = "";
    public string Time { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Company { get; set; } = "";
    public int Added { get; set; }
    public int Updated { get; set; }
    public int Errors { get; set; }
    public string Status { get; set; } = "";
    public string? User { get; set; }
}
