using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Employees;

public sealed class EmployeeApplicationService(IEmployeeStore store, IServiceAuthorizationGuard guard) : IEmployeeService
{
    private readonly IEmployeeStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<List<EmployeeDto>> GetAllActiveAsync()
    {
        var result = await _store.GetAllActiveAsync();
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<EmployeeDto>> GetByCompanyAsync(int companyId, bool includeHidden = false)
    {
        var result = await _store.GetByCompanyAsync(companyId, includeHidden);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var result = await _store.GetByIdAsync(id);
        return result is null ? null : ToDto(result);
    }

    public async Task<List<EmployeeDto>> GetExpiringAsync(int days = 30)
    {
        var result = await _store.GetExpiringAsync(days);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task CreateAsync(EmployeeDto employee)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var employeeData = ToEntity(employee);
        await _store.CreateAsync(employeeData);
        DtoMapper.CopyToDto(employeeData, employee);
    }

    public async Task UpdateAsync(EmployeeDto employee)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var employeeData = ToEntity(employee);
        await _store.UpdateAsync(employeeData);
        DtoMapper.CopyToDto(employeeData, employee);
    }

    public async Task DeleteAsync(int id)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        await _store.DeleteAsync(id);
    }

    public async Task<bool> CheckDuplicatePassportAsync(string passport, int excludeId = 0)
    {
        var result = await _store.CheckDuplicatePassportAsync(passport, excludeId);
        return result;
    }

    public async Task<int> CountExpiredTemporaryStayAsync()
    {
        var result = await _store.CountExpiredTemporaryStayAsync();
        return result;
    }

    public async Task<int> CountExpiredFamilyVisitAsync()
    {
        var result = await _store.CountExpiredFamilyVisitAsync();
        return result;
    }

    public async Task<int> ArchiveExpiredTemporaryStayAsync(string? archivedBy = null)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var result = await _store.ArchiveExpiredTemporaryStayAsync(archivedBy);
        return result;
    }

    public async Task<int> ArchiveExpiredFamilyVisitAsync(string? archivedBy = null)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var result = await _store.ArchiveExpiredFamilyVisitAsync(archivedBy);
        return result;
    }
}
