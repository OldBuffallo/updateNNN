using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Companies;

public sealed class CompanyApplicationService(ICompanyStore store, IServiceAuthorizationGuard guard) : ICompanyService
{
    private readonly ICompanyStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<List<CompanyDto>> GetAllAsync(string? search = null, int? fieldId = null)
    {
        var result = await _store.GetAllAsync(search, fieldId);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        var result = await _store.GetByIdAsync(id);
        return result is null ? null : ToDto(result);
    }

    public async Task<int> GetEmployeeCountAsync(int companyId)
    {
        var result = await _store.GetEmployeeCountAsync(companyId);
        return result;
    }

    public async Task CreateAsync(CompanyDto company)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var companyData = ToEntity(company);
        await _store.CreateAsync(companyData);
        DtoMapper.CopyToDto(companyData, company);
    }

    public async Task UpdateAsync(CompanyDto company)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var companyData = ToEntity(company);
        await _store.UpdateAsync(companyData);
        DtoMapper.CopyToDto(companyData, company);
    }

    public async Task DeleteAsync(int id)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        await _store.DeleteAsync(id);
    }
}
