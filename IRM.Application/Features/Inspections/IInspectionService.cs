using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Inspections;

public interface IInspectionService
{
    Task<IReadOnlyList<InspectionDto>> SearchAsync(DateTime? from, DateTime? to, int? administrativeUnitId, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(InspectionDto inspection, CancellationToken cancellationToken = default);
    Task<int> AddSubjectAsync(int inspectionId, FamilyVisitInput personInput, InspectionSubjectDto subject, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<int>> GetCheckedPersonIdsAsync(DateTime lookbackFrom, DateTime lookbackTo, int? administrativeUnitId, CancellationToken cancellationToken = default);
}
