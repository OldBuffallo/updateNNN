namespace IRM.Data.Models;

/// <summary>
/// Bảng Nationality — Quốc tịch
/// Đặt tên NationalityEntity để tránh trùng namespace
/// </summary>
public class NationalityEntity
{
    public int IDNationality { get; set; }
    public string NationalityCode { get; set; } = "";
    public string NationalityName { get; set; } = "";
    public int Delete_flag { get; set; }

    public bool IsActive => Delete_flag == 0;
}
