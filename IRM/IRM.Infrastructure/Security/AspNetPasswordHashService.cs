using IRM.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace IRM.Infrastructure.Security;

public sealed class AspNetPasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<Account> _hasher = new();
    public string Hash(Account account, string password) => _hasher.HashPassword(account, password);
    public bool Verify(Account account, string hash, string password) =>
        _hasher.VerifyHashedPassword(account, hash, password) != PasswordVerificationResult.Failed;
}
