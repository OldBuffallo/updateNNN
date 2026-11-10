using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Students;

public sealed class StudentApplicationService(IStudentStore store, IServiceAuthorizationGuard guard) : IStudentService
{
    private readonly IStudentStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<List<StudentDto>> GetAllActiveAsync()
    {
        var result = await _store.GetAllActiveAsync();
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<StudentDto>> GetAllVisibleAsync()
    {
        var result = await _store.GetAllVisibleAsync();
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<StudentDto?> GetByIdAsync(int id)
    {
        var result = await _store.GetByIdAsync(id);
        return result is null ? null : ToDto(result);
    }

    public async Task<List<StudentDto>> GetBySchoolAsync(string schoolName)
    {
        var result = await _store.GetBySchoolAsync(schoolName);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<StudentDto>> GetExpiringVisaAsync(int days = 30)
    {
        var result = await _store.GetExpiringVisaAsync(days);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task CreateAsync(StudentDto student)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var studentData = ToEntity(student);
        await _store.CreateAsync(studentData);
        DtoMapper.CopyToDto(studentData, student);
    }

    public async Task UpdateAsync(StudentDto student)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var studentData = ToEntity(student);
        await _store.UpdateAsync(studentData);
        DtoMapper.CopyToDto(studentData, student);
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
}
