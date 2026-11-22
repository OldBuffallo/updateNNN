using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Residence;

public sealed class ResidenceApplicationService(IResidenceStore store, IServiceAuthorizationGuard guard) : IResidenceService
{
    private readonly IResidenceStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<IReadOnlyList<ResidencePeriodDto>> GetHistoryAsync(int foreignPersonId, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var result = await _store.GetHistoryAsync(foreignPersonId, cancellationToken);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<int> SaveAsync(ResidencePeriodDto residence, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (residence.ValidTo.HasValue && residence.ValidTo.Value.Date < residence.ValidFrom.Date)
            throw new ArgumentException("Ngày kết thúc không được trước ngày bắt đầu.");

        var residenceData = ToEntity(residence);
        var result = await _store.SaveAsync(residenceData, cancellationToken);
        DtoMapper.CopyToDto(residenceData, residence);
        return result;
    }
}
