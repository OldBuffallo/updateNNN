using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Companies;

public sealed class CompanyProfileApplicationService(ICompanyProfileStore store, IServiceAuthorizationGuard guard) : ICompanyProfileService
{
    private readonly ICompanyProfileStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<CompanyProfileAggregateDto?> GetAsync(int companyId, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var result = await _store.GetAsync(companyId, cancellationToken);
        return result is null ? null : ToDto(result);
    }

    public async Task SaveProfileAsync(CompanyProfileDto profile, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (profile.CompanyId <= 0) throw new ArgumentException("Doanh nghiệp không hợp lệ.");

        var profileData = ToEntity(profile);
        await _store.SaveProfileAsync(profileData, cancellationToken);
        DtoMapper.CopyToDto(profileData, profile);
    }

    public async Task<int> SaveSiteAsync(CompanySiteDto site, IEnumerable<int> economicZoneIds, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(site.Name) || string.IsNullOrWhiteSpace(site.AddressLine))
            throw new ArgumentException("Tên và địa chỉ địa điểm là bắt buộc.");

        var siteData = ToEntity(site);
        var result = await _store.SaveSiteAsync(siteData, economicZoneIds, cancellationToken);
        DtoMapper.CopyToDto(siteData, site);
        return result;
    }

    public async Task<int> SaveRepresentativeAsync(CompanyRepresentativeDto representative, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(representative.FullName)) throw new ArgumentException("Tên người đại diện là bắt buộc.");

        var representativeData = ToEntity(representative);
        var result = await _store.SaveRepresentativeAsync(representativeData, cancellationToken);
        DtoMapper.CopyToDto(representativeData, representative);
        return result;
    }
}
