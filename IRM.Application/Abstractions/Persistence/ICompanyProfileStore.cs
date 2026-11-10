using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Companies; query execution and transactions remain in Infrastructure.</summary>
public interface ICompanyProfileStore
{
    Task<CompanyProfileAggregate?> GetAsync(int companyId, CancellationToken cancellationToken = default);
    Task SaveProfileAsync(CompanyProfile profile, CancellationToken cancellationToken = default);
    Task<int> SaveSiteAsync(CompanySite site, IEnumerable<int> economicZoneIds, CancellationToken cancellationToken = default);
    Task<int> SaveRepresentativeAsync(CompanyRepresentative representative, CancellationToken cancellationToken = default);
}
