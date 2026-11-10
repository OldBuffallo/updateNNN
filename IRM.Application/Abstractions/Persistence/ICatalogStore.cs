using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Administration; query execution and transactions remain in Infrastructure.</summary>
public interface ICatalogStore
{
    Task<List<Field>> GetFieldsAsync();
    Task<List<Career>> GetCareersAsync(int? careerGroupId = null);
    Task<List<CareerGroup>> GetCareerGroupsAsync();
    Task<List<NationalityEntity>> GetNationalitiesAsync();
    Task<List<District>> GetDistrictsAsync();
    Task<List<Ward>> GetWardsAsync();
    Task<List<AdministrativeUnit>> GetAdministrativeUnitsAsync(DateTime? asOfDate = null);
    Task<List<EconomicZone>> GetEconomicZonesAsync(DateTime? asOfDate = null);
    Task<int> SaveEconomicZoneAsync(EconomicZone zone);
    Task<Field> CreateFieldAsync(Field field);
    Task<bool> UpdateFieldAsync(Field field);
    Task<bool> DeleteFieldAsync(int id);
    Task<Career> CreateCareerAsync(Career career);
    Task<bool> UpdateCareerAsync(Career career);
    Task<bool> DeleteCareerAsync(int id);
    Task<NationalityEntity> CreateNationalityAsync(NationalityEntity nationality);
    Task<bool> UpdateNationalityAsync(NationalityEntity nationality);
    Task<bool> DeleteNationalityAsync(int id);
    Task<District> CreateDistrictAsync(District district);
    Task<bool> UpdateDistrictAsync(District district);
    Task<bool> DeleteDistrictAsync(int id);
    Task<Ward> CreateWardAsync(Ward ward);
    Task<bool> UpdateWardAsync(Ward ward);
    Task<bool> DeleteWardAsync(int id);
}
