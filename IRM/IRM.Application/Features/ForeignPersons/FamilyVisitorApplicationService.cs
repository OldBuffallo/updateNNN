using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.ForeignPersons;

public sealed class FamilyVisitorApplicationService(IForeignerRegistryStore store, IServiceAuthorizationGuard guard) : IForeignerRegistryService
{
    private readonly IForeignerRegistryStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<IReadOnlyList<ForeignPersonDto>> SearchAsync(string? search, DateTime asOfDate, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var result = await _store.SearchAsync(search, asOfDate, cancellationToken);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<int> SaveDocumentAsync(ImmigrationDocumentDto document, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(document.Number)) throw new ArgumentException("Số giấy tờ là bắt buộc.");
        if (document.ValidTo.HasValue && document.ValidTo < document.ValidFrom) throw new ArgumentException("Hiệu lực giấy tờ không hợp lệ.");

        var documentData = ToEntity(document);
        var result = await _store.SaveDocumentAsync(documentData, cancellationToken);
        DtoMapper.CopyToDto(documentData, document);
        return result;
    }

    public async Task<int> SaveElectronicIdentityAsync(ElectronicIdentityDto identity, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(identity.IdentityNumber)) throw new ArgumentException("Số định danh là bắt buộc.");

        var identityData = ToEntity(identity);
        var result = await _store.SaveElectronicIdentityAsync(identityData, cancellationToken);
        DtoMapper.CopyToDto(identityData, identity);
        return result;
    }

    public async Task<IReadOnlyList<FamilyVisitorDto>> SearchFamilyVisitsAsync(ForeignerSearchFilter filter, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        var result = await _store.SearchFamilyVisitsAsync(filter, cancellationToken);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<int> SaveFamilyVisitAsync(FamilyVisitInput input, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        FamilyVisitRules.Validate(input);
        var result = await _store.SaveFamilyVisitAsync(input, cancellationToken);
        return result;
    }

    public async Task SoftDeleteAsync(int foreignPersonId, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        await _store.SoftDeleteAsync(foreignPersonId, cancellationToken);
    }

    public async Task<ForeignPersonDto> FindOrCreateMinimalAsync(string fullName, string? passportNumber, string? nationalityCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Họ tên là bắt buộc.");

        var result = await _store.FindOrCreateMinimalAsync(fullName, passportNumber, nationalityCode, cancellationToken);
        return ToDto(result);
    }

    public async Task<BackfillResult> BackfillLegacyAsync(CancellationToken cancellationToken = default)
    {
        var result = await _store.BackfillLegacyAsync(cancellationToken);
        return result;
    }

    public async Task<IReadOnlyList<UnifiedForeignPersonDto>> GetUnifiedProfilesAsync(string? search, DateTime asOfDate, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor, IrmRoles.Inspector, IrmRoles.Reporter, IrmRoles.Viewer);
        return await _store.GetUnifiedProfilesAsync(search, asOfDate, cancellationToken);
    }

    public async Task<int> SaveUnifiedProfileAsync(UnifiedForeignPersonInput input, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(input.PassportNumber))
            throw new ArgumentException("Số hộ chiếu là bắt buộc.");
        return await _store.SaveUnifiedProfileAsync(input, cancellationToken);
    }

    public async Task DeleteUnifiedProfileAsync(int? foreignPersonId, int? employeeId, int? studentId, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        await _store.DeleteUnifiedProfileAsync(foreignPersonId, employeeId, studentId, cancellationToken);
    }

    public async Task<LegacyDatabaseImportResult> ImportFromLegacyDatabaseAsync(string sourceConnectionStringOrDbName, CancellationToken cancellationToken = default)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        return await _store.ImportFromLegacyDatabaseAsync(sourceConnectionStringOrDbName, cancellationToken);
    }
}
