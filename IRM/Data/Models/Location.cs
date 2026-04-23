namespace IRM.Data.Models;

/// <summary>
/// Bảng Districts — Quận/Huyện
/// </summary>
public class District
{
    public int IDDistrict { get; set; }
    public string DisTrictName { get; set; } = "";
    public int Delete_flag { get; set; }
    public bool IsActive => Delete_flag == 0;
}

/// <summary>
/// Bảng Wards — Phường/Xã
/// </summary>
public class Ward
{
    public int IDWard { get; set; }
    public string WardName { get; set; } = "";
    public int Delete_flag { get; set; }
    public bool IsActive => Delete_flag == 0;
}
