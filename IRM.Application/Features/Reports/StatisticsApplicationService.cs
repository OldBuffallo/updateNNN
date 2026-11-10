using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Reports;

public sealed class StatisticsApplicationService(IStatisticsStore store, IServiceAuthorizationGuard guard) : IStatisticsService
{
    private readonly IStatisticsStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<TerritoryStatistics> GetTerritoryAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        filter.Validate();
        if (!filter.AdministrativeUnitId.HasValue) throw new ArgumentException("Phải chọn địa bàn.");

        var result = await _store.GetTerritoryAsync(filter, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<TerritoryOverview>> GetTerritoryOverviewAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var result = await _store.GetTerritoryOverviewAsync(filter, cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<ZoneStatistics>> GetZoneStatisticsAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var result = await _store.GetZoneStatisticsAsync(filter, cancellationToken);
        return result;
    }

    public async Task<DocumentIdentityStatistics> GetDocumentIdentityStatisticsAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var result = await _store.GetDocumentIdentityStatisticsAsync(filter, cancellationToken);
        return result;
    }
}
