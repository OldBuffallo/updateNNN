using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.ForeignPersons;

public interface IForeignerRegistryService
{
    Task<IReadOnlyList<ForeignPersonDto>> SearchAsync(string? search, DateTime asOfDate, CancellationToken cancellationToken = default);
    Task<int> SaveDocumentAsync(ImmigrationDocumentDto document, CancellationToken cancellationToken = default);
    Task<int> SaveElectronicIdentityAsync(ElectronicIdentityDto identity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FamilyVisitorDto>> SearchFamilyVisitsAsync(ForeignerSearchFilter filter, CancellationToken cancellationToken = default);
    Task<int> SaveFamilyVisitAsync(FamilyVisitInput input, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int foreignPersonId, CancellationToken cancellationToken = default);
    Task<ForeignPersonDto> FindOrCreateMinimalAsync(string fullName, string? passportNumber, string? nationalityCode, CancellationToken cancellationToken = default);
    Task<BackfillResult> BackfillLegacyAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnifiedForeignPersonDto>> GetUnifiedProfilesAsync(string? search, DateTime asOfDate, CancellationToken cancellationToken = default);
    Task<int> SaveUnifiedProfileAsync(UnifiedForeignPersonInput input, CancellationToken cancellationToken = default);
    Task DeleteUnifiedProfileAsync(int? foreignPersonId, int? employeeId, int? studentId, CancellationToken cancellationToken = default);
    Task<LegacyDatabaseImportResult> ImportFromLegacyDatabaseAsync(string sourceConnectionStringOrDbName, CancellationToken cancellationToken = default);
}
