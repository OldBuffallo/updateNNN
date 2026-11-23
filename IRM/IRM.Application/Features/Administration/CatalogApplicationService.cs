using static IRM.Application.Models.DtoMapper;
using IRM.Application.Features.Administration.Contracts;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Administration;

public sealed class CatalogApplicationService(ICatalogStore store, IServiceAuthorizationGuard guard) : ICatalogService
{
    private readonly ICatalogStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<List<FieldDto>> GetFieldsAsync()
    {
        var result = await _store.GetFieldsAsync();
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<CareerDto>> GetCareersAsync(int? careerGroupId = null)
    {
        var result = await _store.GetCareersAsync(careerGroupId);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<CareerGroupDto>> GetCareerGroupsAsync()
    {
        var result = await _store.GetCareerGroupsAsync();
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<NationalityEntityDto>> GetNationalitiesAsync()
    {
        var result = await _store.GetNationalitiesAsync();
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<DistrictDto>> GetDistrictsAsync()
    {
        var result = await _store.GetDistrictsAsync();
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<WardDto>> GetWardsAsync()
    {
        var result = await _store.GetWardsAsync();
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<AdministrativeUnitDto>> GetAdministrativeUnitsAsync(DateTime? asOfDate = null)
    {
        var result = await _store.GetAdministrativeUnitsAsync(asOfDate);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<List<EconomicZoneDto>> GetEconomicZonesAsync(DateTime? asOfDate = null)
    {
        var result = await _store.GetEconomicZonesAsync(asOfDate);
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<int> SaveEconomicZoneAsync(EconomicZoneDto zone)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        if (string.IsNullOrWhiteSpace(zone.Code) || string.IsNullOrWhiteSpace(zone.Name))
            throw new ArgumentException("Mã và tên khu/cụm là bắt buộc.");

        var zoneData = ToEntity(zone);
        var result = await _store.SaveEconomicZoneAsync(zoneData);
        DtoMapper.CopyToDto(zoneData, zone);
        return result;
    }

    public async Task<FieldDto> CreateFieldAsync(FieldDto field)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var fieldData = ToEntity(field);
        var result = await _store.CreateFieldAsync(fieldData);
        DtoMapper.CopyToDto(fieldData, field);
        return ToDto(result);
    }

    public async Task<bool> UpdateFieldAsync(FieldDto field)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var fieldData = ToEntity(field);
        var result = await _store.UpdateFieldAsync(fieldData);
        DtoMapper.CopyToDto(fieldData, field);
        return result;
    }

    public async Task<bool> DeleteFieldAsync(int id)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var result = await _store.DeleteFieldAsync(id);
        return result;
    }

    public async Task<CareerDto> CreateCareerAsync(CareerDto career)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var careerData = ToEntity(career);
        var result = await _store.CreateCareerAsync(careerData);
        DtoMapper.CopyToDto(careerData, career);
        return ToDto(result);
    }

    public async Task<bool> UpdateCareerAsync(CareerDto career)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var careerData = ToEntity(career);
        var result = await _store.UpdateCareerAsync(careerData);
        DtoMapper.CopyToDto(careerData, career);
        return result;
    }

    public async Task<bool> DeleteCareerAsync(int id)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var result = await _store.DeleteCareerAsync(id);
        return result;
    }

    public async Task<NationalityEntityDto> CreateNationalityAsync(NationalityEntityDto nationality)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var nationalityData = ToEntity(nationality);
        var result = await _store.CreateNationalityAsync(nationalityData);
        DtoMapper.CopyToDto(nationalityData, nationality);
        return ToDto(result);
    }

    public async Task<bool> UpdateNationalityAsync(NationalityEntityDto nationality)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var nationalityData = ToEntity(nationality);
        var result = await _store.UpdateNationalityAsync(nationalityData);
        DtoMapper.CopyToDto(nationalityData, nationality);
        return result;
    }

    public async Task<bool> DeleteNationalityAsync(int id)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var result = await _store.DeleteNationalityAsync(id);
        return result;
    }

    public async Task<DistrictDto> CreateDistrictAsync(DistrictDto district)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var districtData = ToEntity(district);
        var result = await _store.CreateDistrictAsync(districtData);
        DtoMapper.CopyToDto(districtData, district);
        return ToDto(result);
    }

    public async Task<bool> UpdateDistrictAsync(DistrictDto district)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var districtData = ToEntity(district);
        var result = await _store.UpdateDistrictAsync(districtData);
        DtoMapper.CopyToDto(districtData, district);
        return result;
    }

    public async Task<bool> DeleteDistrictAsync(int id)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var result = await _store.DeleteDistrictAsync(id);
        return result;
    }

    public async Task<WardDto> CreateWardAsync(WardDto ward)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var wardData = ToEntity(ward);
        var result = await _store.CreateWardAsync(wardData);
        DtoMapper.CopyToDto(wardData, ward);
        return ToDto(result);
    }

    public async Task<bool> UpdateWardAsync(WardDto ward)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var wardData = ToEntity(ward);
        var result = await _store.UpdateWardAsync(wardData);
        DtoMapper.CopyToDto(wardData, ward);
        return result;
    }

    public async Task<bool> DeleteWardAsync(int id)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin, IrmRoles.DataEditor);
        var result = await _store.DeleteWardAsync(id);
        return result;
    }

    // ── AdministrativeUnits (Địa bàn hành chính) ──

    public async Task<List<AdministrativeUnitDto>> GetAllAdministrativeUnitsAsync()
    {
        var result = await _store.GetAllAdministrativeUnitsAsync();
        return result.Select(item0 => ToDto(item0)).ToList();
    }

    public async Task<AdministrativeUnitDto> CreateAdministrativeUnitAsync(AdministrativeUnitDto unit)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        var entity = ToEntity(unit);
        var result = await _store.CreateAdministrativeUnitAsync(entity);
        return ToDto(result);
    }

    public async Task<bool> UpdateAdministrativeUnitAsync(AdministrativeUnitDto unit)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        var entity = ToEntity(unit);
        return await _store.UpdateAdministrativeUnitAsync(entity);
    }

    public async Task<bool> DeactivateAdministrativeUnitAsync(int id, int? successorId)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        return await _store.DeactivateAdministrativeUnitAsync(id, successorId);
    }

    public byte[] GenerateAdministrativeUnitTemplate() => _store.GenerateAdministrativeUnitTemplate();

    public async Task<AdministrativeUnitImportPreview> PreviewAdministrativeUnitsExcelAsync(Stream stream, string fileName)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        return await _store.PreviewAdministrativeUnitsExcelAsync(stream, fileName);
    }

    public async Task<AdministrativeUnitImportResult> ImportAdministrativeUnitsExcelAsync(Stream stream, string fileName)
    {
        await _guard.RequireAnyRoleAsync(IrmRoles.Admin);
        return await _store.ImportAdministrativeUnitsExcelAsync(stream, fileName);
    }
}
