using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for Account; never tracked by EF.</summary>
public sealed class AccountDto
{
    public int IDUser { get; set; }
    public string Username { get; set; } = "";
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public AccountPermission Permission { get; set; }
    public int Delete_flag { get; set; }
    // Computed
    public string PermissionString => Permission.ToDisplay();
    public bool IsActive => Delete_flag == 0;
}
