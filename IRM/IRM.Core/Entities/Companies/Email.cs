

namespace IRM.Data.Models;

/// <summary>
/// Bảng Emails — Email liên hệ công ty
/// </summary>
public class Email
{
    public int IDEmail { get; set; }
    public string? Name { get; set; }
    public string? Mail { get; set; }
    public int IDCompany { get; set; }
    public int Delete_flag { get; set; }

    public Company? Company { get; set; }
}
