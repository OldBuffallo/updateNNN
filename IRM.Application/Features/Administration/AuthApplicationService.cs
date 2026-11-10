using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Administration;

public sealed class AuthApplicationService(IAuthStore store, IServiceAuthorizationGuard guard) : IAuthService
{
    private readonly IAuthStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<ClaimsPrincipal?> AuthenticateWebAsync(string username, string password, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var result = await _store.AuthenticateWebAsync(username, password, ipAddress, cancellationToken);
        return result;
    }

    public async Task<List<AccountDto>> GetAllAccountsAsync()
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        var result = await _store.GetAllAccountsAsync();
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task CreateAccountAsync(AccountDto account)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        var accountData = ToEntity(account);
        await _store.CreateAccountAsync(accountData);
        DtoMapper.CopyToDto(accountData, account);
    }

    public async Task UpdateAccountAsync(AccountDto account)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        var accountData = ToEntity(account);
        await _store.UpdateAccountAsync(accountData);
        DtoMapper.CopyToDto(accountData, account);
    }

    public async Task DeleteAccountAsync(int id)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        await _store.DeleteAccountAsync(id);
    }
}
