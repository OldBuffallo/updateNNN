using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Reports; query execution and transactions remain in Infrastructure.</summary>
public interface IStatisticsStore
{
    Task<TerritoryStatistics> GetTerritoryAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TerritoryOverview>> GetTerritoryOverviewAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ZoneStatistics>> GetZoneStatisticsAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
    Task<DocumentIdentityStatistics> GetDocumentIdentityStatisticsAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
}
