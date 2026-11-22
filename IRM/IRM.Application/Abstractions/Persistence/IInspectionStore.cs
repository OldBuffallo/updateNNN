using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Inspections; query execution and transactions remain in Infrastructure.</summary>
public interface IInspectionStore
{
    Task<IReadOnlyList<Inspection>> SearchAsync(DateTime? from, DateTime? to, int? administrativeUnitId, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(Inspection inspection, CancellationToken cancellationToken = default);
    Task<int> AddSubjectAsync(int inspectionId, FamilyVisitInput personInput, InspectionSubject subject, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<int>> GetCheckedPersonIdsAsync(DateTime lookbackFrom, DateTime lookbackTo, int? administrativeUnitId, CancellationToken cancellationToken = default);
}
