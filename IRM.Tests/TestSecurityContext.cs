using System.Security.Claims;
using IRM.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace IRM.Tests;

public static class TestSecurityContext
{
    public static ClaimsPrincipal CreatePrincipal(string role, int accountId = 1, string username = "testuser")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, accountId.ToString()),
            new(ClaimTypes.Name, username)
        };
        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new(ClaimTypes.Role, role));
        }
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal CreateAnonymousPrincipal() => new(new ClaimsIdentity());

    public static IServiceAuthorizationGuard CreateGuard(string role, int accountId = 1, string username = "testuser")
    {
        var principal = CreatePrincipal(role, accountId, username);
        return new ServiceAuthorizationGuard(new FixedAuthStateProvider(principal));
    }

    public static IServiceAuthorizationGuard CreateAnonymousGuard()
    {
        return new ServiceAuthorizationGuard(new FixedAuthStateProvider(CreateAnonymousPrincipal()));
    }

    public static IServiceAuthorizationGuard CreateAllowAllGuard(int accountId = 1) => new AllowAllGuard(accountId);

    private sealed class FixedAuthStateProvider(ClaimsPrincipal principal) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(principal));
    }

    private sealed class AllowAllGuard(int accountId) : IServiceAuthorizationGuard
    {
        public Task RequireAnyRoleAsync(params string[] roles) => Task.CompletedTask;
        public Task<int> GetRequiredAccountIdAsync() => Task.FromResult(accountId);
    }
}
