using System.Security.Claims;
using IRM.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace IRM.Tests;

public sealed class AuthorizationTests
{
    [Fact]
    public async Task ServerGuard_AllowsConfiguredRoleAndRejectsOtherRole()
    {
        var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier,"7"),new Claim(ClaimTypes.Name,"viewer"),new Claim(ClaimTypes.Role,IrmRoles.Viewer) },"test"));
        var guard = new ServiceAuthorizationGuard(new FixedAuthStateProvider(viewer));
        await guard.RequireAnyRoleAsync(IrmRoles.Viewer,IrmRoles.Reporter);
        Assert.Equal(7,await guard.GetRequiredAccountIdAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>guard.RequireAnyRoleAsync(IrmRoles.Admin));
    }

    [Fact]
    public async Task ServerGuard_RejectsAnonymousUser()
    {
        var guard = new ServiceAuthorizationGuard(new FixedAuthStateProvider(new ClaimsPrincipal(new ClaimsIdentity())));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>guard.RequireAnyRoleAsync(IrmRoles.Viewer));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>guard.GetRequiredAccountIdAsync());
    }

    private sealed class FixedAuthStateProvider(ClaimsPrincipal principal) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(principal));
    }
}
