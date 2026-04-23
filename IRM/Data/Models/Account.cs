namespace IRM.Data.Models;

/// <summary>
/// Bảng Accounts — Tài khoản người dùng
/// Map trực tiếp tới bảng Accounts trong ReportManagerDB
/// </summary>
public class Account
{
    public int IDUser { get; set; }
    public string Username { get; set; } = "";
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public int Permission { get; set; }
    public int Delete_flag { get; set; }

    // Computed
    public string PermissionString => Permission == 1 ? "Admin" : "User";
    public bool IsActive => Delete_flag == 0;
}
