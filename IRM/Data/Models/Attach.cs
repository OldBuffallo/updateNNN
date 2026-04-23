namespace IRM.Data.Models;

/// <summary>
/// Bảng Attach — File đính kèm của công ty
/// </summary>
public class Attach
{
    public int IDAttach { get; set; }
    public int IDCompany { get; set; }
    public int Type { get; set; }
    public string? Name { get; set; }
    public string? Folder { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public int Delete_flag { get; set; }

    public Company? Company { get; set; }

    public string TypeString => Type switch
    {
        0 => "BC_NNN",
        1 => "HSPN",
        _ => ""
    };
    public bool IsActive => Delete_flag == 0;
}
