using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Accommodations;

public sealed class AccommodationApplicationService(IAccommodationStore store, IServiceAuthorizationGuard guard) : IAccommodationService
{
    private readonly IAccommodationStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<IReadOnlyList<AccommodationDto>> SearchAsync(string? search, int? administrativeUnitId, int? companyId, DateTime? asOfDate, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var result = await _store.SearchAsync(search, administrativeUnitId, companyId, asOfDate, cancellationToken);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<int> SaveAsync(AccommodationDto accommodation, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(accommodation.Name) || string.IsNullOrWhiteSpace(accommodation.AddressLine))
            throw new ArgumentException("Tên và địa chỉ cơ sở lưu trú là bắt buộc.");

        var accommodationData = ToEntity(accommodation);
        var result = await _store.SaveAsync(accommodationData, cancellationToken);
        DtoMapper.CopyToDto(accommodationData, accommodation);
        return result;
    }

    public async Task<int> SaveAgreementAsync(CompanyAccommodationAgreementDto agreement, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (agreement.ValidTo.HasValue && agreement.ValidTo < agreement.ValidFrom)
            throw new ArgumentException("Khoảng thuê không hợp lệ.");

        var agreementData = ToEntity(agreement);
        var result = await _store.SaveAgreementAsync(agreementData, cancellationToken);
        DtoMapper.CopyToDto(agreementData, agreement);
        return result;
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        await _store.SoftDeleteAsync(id, cancellationToken);
    }
}
