using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for ForeignPersons; query execution and transactions remain in Infrastructure.</summary>
public interface IForeignerRegistryStore
{
    Task<IReadOnlyList<ForeignPerson>> SearchAsync(string? search, DateTime asOfDate, CancellationToken cancellationToken = default);
    Task<int> SaveDocumentAsync(ImmigrationDocument document, CancellationToken cancellationToken = default);
    Task<int> SaveElectronicIdentityAsync(ElectronicIdentity identity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FamilyVisitor>> SearchFamilyVisitsAsync(ForeignerSearchFilter filter, CancellationToken cancellationToken = default);
    Task<int> SaveFamilyVisitAsync(FamilyVisitInput input, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int foreignPersonId, CancellationToken cancellationToken = default);
    Task<ForeignPerson> FindOrCreateMinimalAsync(string fullName, string? passportNumber, string? nationalityCode, CancellationToken cancellationToken = default);
    Task<BackfillResult> BackfillLegacyAsync(CancellationToken cancellationToken = default);
}
