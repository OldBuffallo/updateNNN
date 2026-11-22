using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Administration; query execution and transactions remain in Infrastructure.</summary>
public interface IAuthStore
{
    Task<ClaimsPrincipal?> AuthenticateWebAsync(string username, string password, string? ipAddress, CancellationToken cancellationToken = default);
    Task<List<Account>> GetAllAccountsAsync();
    Task CreateAccountAsync(Account account);
    Task UpdateAccountAsync(Account account);
    Task DeleteAccountAsync(int id);
}
