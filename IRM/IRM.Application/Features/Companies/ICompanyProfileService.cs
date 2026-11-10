using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Companies;

public interface ICompanyProfileService
{
    Task<CompanyProfileAggregateDto?> GetAsync(int companyId, CancellationToken cancellationToken = default);
    Task SaveProfileAsync(CompanyProfileDto profile, CancellationToken cancellationToken = default);
    Task<int> SaveSiteAsync(CompanySiteDto site, IEnumerable<int> economicZoneIds, CancellationToken cancellationToken = default);
    Task<int> SaveRepresentativeAsync(CompanyRepresentativeDto representative, CancellationToken cancellationToken = default);
}
