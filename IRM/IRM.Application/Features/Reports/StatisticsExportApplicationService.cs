using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Reports;

public sealed class StatisticsExportApplicationService(IStatisticsExportStore store, IServiceAuthorizationGuard guard) : IStatisticsExportService
{
    private readonly IStatisticsExportStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<byte[]> ExportTerritoryAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.Reporter);
        var result = await _store.ExportTerritoryAsync(filter, cancellationToken);
        return result;
    }
}
