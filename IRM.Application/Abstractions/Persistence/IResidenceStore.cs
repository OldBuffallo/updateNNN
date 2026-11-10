using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Residence; query execution and transactions remain in Infrastructure.</summary>
public interface IResidenceStore
{
    Task<IReadOnlyList<ResidencePeriod>> GetHistoryAsync(int foreignPersonId, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(ResidencePeriod residence, CancellationToken cancellationToken = default);
}
