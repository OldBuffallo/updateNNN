using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Reports;

public interface IStatisticsService
{
    Task<TerritoryStatistics> GetTerritoryAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TerritoryOverview>> GetTerritoryOverviewAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ZoneStatistics>> GetZoneStatisticsAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
    Task<DocumentIdentityStatistics> GetDocumentIdentityStatisticsAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
}
