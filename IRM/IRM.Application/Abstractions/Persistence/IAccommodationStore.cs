using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Accommodations; query execution and transactions remain in Infrastructure.</summary>
public interface IAccommodationStore
{
    Task<IReadOnlyList<Accommodation>> SearchAsync(string? search, int? administrativeUnitId, int? companyId, DateTime? asOfDate, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(Accommodation accommodation, CancellationToken cancellationToken = default);
    Task<int> SaveAgreementAsync(CompanyAccommodationAgreement agreement, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
