using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace IRM.Services;

public sealed class ServiceAuthorizationGuard : IServiceAuthorizationGuard
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    public ServiceAuthorizationGuard(AuthenticationStateProvider authenticationStateProvider) =>
        _authenticationStateProvider = authenticationStateProvider;

    public async Task RequireAnyRoleAsync(params string[] roles)
    {
        var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
        if (user.Identity?.IsAuthenticated != true || !roles.Any(user.IsInRole))
            throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này.");
    }

    public async Task<int> GetRequiredAccountIdAsync()
    {
        var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
        var rawId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Identity?.IsAuthenticated != true || !int.TryParse(rawId, out var accountId) || accountId <= 0)
            throw new UnauthorizedAccessException("Không xác định được tài khoản đang đăng nhập.");
        return accountId;
    }
}
