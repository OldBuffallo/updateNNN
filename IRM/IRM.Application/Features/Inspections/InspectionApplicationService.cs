using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Inspections;

public sealed class InspectionApplicationService(IInspectionStore store, IServiceAuthorizationGuard guard) : IInspectionService
{
    private readonly IInspectionStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<IReadOnlyList<InspectionDto>> SearchAsync(DateTime? from, DateTime? to, int? administrativeUnitId, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var result = await _store.SearchAsync(from, to, administrativeUnitId, cancellationToken);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<int> SaveAsync(InspectionDto inspection, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.Inspector);
        if (inspection.InspectedAt == default) throw new ArgumentException("Thời gian kiểm tra là bắt buộc.");
        if (string.IsNullOrWhiteSpace(inspection.LocationText)) throw new ArgumentException("Địa điểm kiểm tra là bắt buộc.");

        var inspectionData = ToEntity(inspection);
        var result = await _store.SaveAsync(inspectionData, cancellationToken);
        DtoMapper.CopyToDto(inspectionData, inspection);
        return result;
    }

    public async Task<int> AddSubjectAsync(int inspectionId, FamilyVisitInput personInput, InspectionSubjectDto subject, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.Inspector);
        if (string.IsNullOrWhiteSpace(personInput.FullName))
            throw new ArgumentException("Họ tên đối tượng kiểm tra là bắt buộc.");
        if ((subject.ResultCode is "VIOLATION" or "VERIFY") && string.IsNullOrWhiteSpace(subject.ViolationDetails))
            throw new ArgumentException("Chi tiết vi phạm / nội dung xác minh là bắt buộc khi chọn Vi phạm hoặc Xác minh.");
        var subjectData = ToEntity(subject);
        var result = await _store.AddSubjectAsync(inspectionId, personInput, subjectData, cancellationToken);
        DtoMapper.CopyToDto(subjectData, subject);
        return result;
    }

    public async Task<IReadOnlySet<int>> GetCheckedPersonIdsAsync(DateTime lookbackFrom, DateTime lookbackTo, int? administrativeUnitId, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        if (lookbackFrom.Date > lookbackTo.Date) throw new ArgumentException("Khoảng nhìn lại không hợp lệ.");

        var result = await _store.GetCheckedPersonIdsAsync(lookbackFrom, lookbackTo, administrativeUnitId, cancellationToken);
        return result;
    }
}
