using IRM.Data.Models;

namespace IRM.Services;

public interface IServiceAuthorizationGuard
{
    Task RequireAnyRoleAsync(params string[] roles);
    Task<int> GetRequiredAccountIdAsync();
}
