using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.ImportExport;

public sealed class ExportApplicationService(IExportStore store, IServiceAuthorizationGuard guard) : IExportService
{
    private readonly IExportStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<byte[]> ExportCompaniesAsync(string? search = null, int? fieldId = null)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.Reporter);
        var result = await _store.ExportCompaniesAsync(search, fieldId);
        return result;
    }

    public async Task<byte[]> ExportEmployeesAsync(int? companyId = null, string? nationality = null, WorkPermitType? workPermit = null, bool expiringOnly = false)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.Reporter);
        var result = await _store.ExportEmployeesAsync(companyId, nationality, workPermit, expiringOnly);
        return result;
    }

    public async Task<byte[]> ExportReportAsync(List<string> selectedColumns, string groupBy = "company", WorkPermitType? filterWorkPermit = null, string? filterNationality = null, int? filterExpiringDays = null)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.Reporter);
        var result = await _store.ExportReportAsync(selectedColumns, groupBy, filterWorkPermit, filterNationality, filterExpiringDays);
        return result;
    }

    public async Task<byte[]> GenerateSampleExcelBytesAsync()
    {
        await _guard.GetRequiredAccountIdAsync();
        var result = await _store.GenerateSampleExcelBytesAsync();
        return result;
    }

    public async Task<byte[]> ExportStudentsAsync(string? nationality = null, string? school = null, EducationLevel? educationLevel = null, ScholarshipType? scholarshipType = null, StudentStatus? status = null, bool expiringOnly = false)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.Reporter);
        var result = await _store.ExportStudentsAsync(nationality, school, educationLevel, scholarshipType, status, expiringOnly);
        return result;
    }
}
