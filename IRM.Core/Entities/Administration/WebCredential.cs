

namespace IRM.Data.Models;

public class WebCredential
{
    public int AccountId { get; set; }
    public string PasswordHash { get; set; } = "";
    public bool MustChangePassword { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Account? Account { get; set; }
}
