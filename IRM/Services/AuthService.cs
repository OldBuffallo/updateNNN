using System.Security.Claims;
using System.Security.Cryptography;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class AuthService
{
    private const int MaxFailures = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);
    private readonly IrmDbContext _db;
    private readonly PasswordHasher<Account> _hasher = new();
    private readonly AuditService _audit;
    private readonly IServiceAuthorizationGuard? _guard;
    public AuthService(IrmDbContext db, AuditService audit) { _db = db; _audit = audit; }
    public AuthService(IrmDbContext db, AuditService audit, IServiceAuthorizationGuard guard) : this(db, audit) => _guard = guard;

    public async Task<ClaimsPrincipal?> AuthenticateWebAsync(string username, string password,
        string? ipAddress, CancellationToken cancellationToken = default)
    {
        var normalized = username.Trim();
        var account = await _db.Accounts.SingleOrDefaultAsync(x => x.Delete_flag == 0 && x.Username == normalized, cancellationToken);
        if (account is null)
        {
            await _audit.LogAsync("LOGIN_FAILED", "Account", null, "Unknown username", normalized);
            return null;
        }
        var credential = await _db.WebCredentials.SingleOrDefaultAsync(x => x.AccountId == account.IDUser, cancellationToken);
        if (credential?.LockedUntil > DateTime.UtcNow)
        {
            await _audit.LogAsync("LOGIN_BLOCKED", "Account", account.IDUser, $"LockedUntil={credential.LockedUntil:o}", account.Username);
            return null;
        }

        var valid = credential is null
            ? CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(account.Password ?? ""),
                System.Text.Encoding.UTF8.GetBytes(password ?? ""))
            : _hasher.VerifyHashedPassword(account, credential.PasswordHash, password ?? "") != PasswordVerificationResult.Failed;

        if (!valid)
        {
            credential ??= new WebCredential { AccountId = account.IDUser, PasswordHash = _hasher.HashPassword(account, account.Password ?? "") };
            if (_db.Entry(credential).State == EntityState.Detached) _db.WebCredentials.Add(credential);
            credential.FailedLoginCount++;
            if (credential.FailedLoginCount >= MaxFailures) credential.LockedUntil = DateTime.UtcNow.Add(LockDuration);
            credential.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            await _audit.LogAsync("LOGIN_FAILED", "Account", account.IDUser, $"IP={ipAddress}", account.Username);
            return null;
        }

        if (credential is null)
        {
            credential = new WebCredential { AccountId = account.IDUser, PasswordHash = _hasher.HashPassword(account, password ?? ""), UpdatedAt = DateTime.UtcNow };
            _db.WebCredentials.Add(credential);
        }
        else
        {
            credential.FailedLoginCount = 0;
            credential.LockedUntil = null;
            credential.UpdatedAt = DateTime.UtcNow;
        }
        var roles = await _db.WebRoleAssignments.Where(x => x.AccountId == account.IDUser).Select(x => x.RoleCode).Distinct().ToListAsync(cancellationToken);
        if (roles.Count == 0)
        {
            roles.Add(account.Permission == 1 ? IrmRoles.Admin : IrmRoles.DataEditor);
            _db.WebRoleAssignments.Add(new WebRoleAssignment { AccountId = account.IDUser, RoleCode = roles[0] });
        }
        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync("LOGIN", "Account", account.IDUser, $"IP={ipAddress}", account.Username);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.IDUser.ToString()),
            new(ClaimTypes.Name, account.Username),
            new("display_name", account.Name ?? account.Username)
        };
        claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public async Task<List<Account>> GetAllAccountsAsync()
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        return await _db.Accounts.Where(x => x.Delete_flag == 0).OrderBy(x => x.Username).ToListAsync();
    }

    public async Task CreateAccountAsync(Account account)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        account.Delete_flag = 0;
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        _db.WebCredentials.Add(new WebCredential
        {
            AccountId = account.IDUser,
            PasswordHash = _hasher.HashPassword(account, account.Password ?? ""),
            UpdatedAt = DateTime.UtcNow
        });
        _db.WebRoleAssignments.Add(new WebRoleAssignment { AccountId = account.IDUser,
            RoleCode = account.Permission == 1 ? IrmRoles.Admin : IrmRoles.DataEditor });
        await _db.SaveChangesAsync();
        await _audit.LogAsync("CREATE", "Account", account.IDUser, null, account.Username);
    }

    public async Task UpdateAccountAsync(Account account)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        _db.Accounts.Update(account);
        var credential = await _db.WebCredentials.SingleOrDefaultAsync(x => x.AccountId == account.IDUser);
        if (credential is null)
        {
            _db.WebCredentials.Add(new WebCredential
            {
                AccountId = account.IDUser,
                PasswordHash = _hasher.HashPassword(account, account.Password ?? ""),
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            credential.PasswordHash = _hasher.HashPassword(account, account.Password ?? "");
            credential.FailedLoginCount = 0;
            credential.LockedUntil = null;
            credential.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        await _audit.LogAsync("UPDATE", "Account", account.IDUser, null, account.Username);
    }

    public async Task DeleteAccountAsync(int id)
    {
        if (_guard is not null) await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        var account = await _db.Accounts.FindAsync(id);
        if (account is null) return;
        account.Delete_flag = 1;
        await _db.SaveChangesAsync();
        await _audit.LogAsync("DELETE", "Account", id, "Soft delete", account.Username);
    }
}
