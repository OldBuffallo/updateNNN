using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Administration;

public interface ICatalogService
{
    Task<List<FieldDto>> GetFieldsAsync();
    Task<List<CareerDto>> GetCareersAsync(int? careerGroupId = null);
    Task<List<CareerGroupDto>> GetCareerGroupsAsync();
    Task<List<NationalityEntityDto>> GetNationalitiesAsync();
    Task<List<DistrictDto>> GetDistrictsAsync();
    Task<List<WardDto>> GetWardsAsync();
    Task<List<AdministrativeUnitDto>> GetAdministrativeUnitsAsync(DateTime? asOfDate = null);
    Task<List<EconomicZoneDto>> GetEconomicZonesAsync(DateTime? asOfDate = null);
    Task<int> SaveEconomicZoneAsync(EconomicZoneDto zone);
    Task<FieldDto> CreateFieldAsync(FieldDto field);
    Task<bool> UpdateFieldAsync(FieldDto field);
    Task<bool> DeleteFieldAsync(int id);
    Task<CareerDto> CreateCareerAsync(CareerDto career);
    Task<bool> UpdateCareerAsync(CareerDto career);
    Task<bool> DeleteCareerAsync(int id);
    Task<NationalityEntityDto> CreateNationalityAsync(NationalityEntityDto nationality);
    Task<bool> UpdateNationalityAsync(NationalityEntityDto nationality);
    Task<bool> DeleteNationalityAsync(int id);
    Task<DistrictDto> CreateDistrictAsync(DistrictDto district);
    Task<bool> UpdateDistrictAsync(DistrictDto district);
    Task<bool> DeleteDistrictAsync(int id);
    Task<WardDto> CreateWardAsync(WardDto ward);
    Task<bool> UpdateWardAsync(WardDto ward);
    Task<bool> DeleteWardAsync(int id);
    // ── AdministrativeUnits (Địa bàn hành chính) ──
    Task<List<AdministrativeUnitDto>> GetAllAdministrativeUnitsAsync();
    Task<AdministrativeUnitDto> CreateAdministrativeUnitAsync(AdministrativeUnitDto unit);
    Task<bool> UpdateAdministrativeUnitAsync(AdministrativeUnitDto unit);
    Task<bool> DeactivateAdministrativeUnitAsync(int id, int? successorId);
}
