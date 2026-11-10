using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Administration;

public interface IAuthService
{
    Task<ClaimsPrincipal?> AuthenticateWebAsync(string username, string password, string? ipAddress, CancellationToken cancellationToken = default);
    Task<List<AccountDto>> GetAllAccountsAsync();
    Task CreateAccountAsync(AccountDto account);
    Task UpdateAccountAsync(AccountDto account);
    Task DeleteAccountAsync(int id);
}
