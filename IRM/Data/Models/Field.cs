namespace IRM.Data.Models;

/// <summary>
/// Bảng Fields — Lĩnh vực hoạt động
/// </summary>
public class Field
{
    public int IDField { get; set; }
    public string FieldName { get; set; } = "";
    public string? Description { get; set; }
    public int Delete_flag { get; set; }

    public ICollection<Company> Companies { get; set; } = new List<Company>();
    public bool IsActive => Delete_flag == 0;
}
