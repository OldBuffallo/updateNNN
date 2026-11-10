using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Reports;

public sealed class DashboardApplicationService(IDashboardStore store, IServiceAuthorizationGuard guard) : IDashboardService
{
    private readonly IDashboardStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<DashboardDataDto> GetDashboardAsync()
    {
        var result = await _store.GetDashboardAsync();
        return ToDto(result);
    }
}
