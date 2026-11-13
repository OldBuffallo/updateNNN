using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using IRM.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;

namespace IRM.Components.Pages;

public partial class Admin
{

    internal async Task SaveFieldAsync()
    {
        if (string.IsNullOrWhiteSpace(_fieldForm.FieldName))
        {
            Snackbar.Add("Tên lĩnh vực không được để trống.", Severity.Warning);
            return;
        }

        try
        {
            if (_editingField)
            {
                await CatalogSvc.UpdateFieldAsync(_fieldForm);
            }
            else
            {
                await CatalogSvc.CreateFieldAsync(_fieldForm);
            }
            _fields = await CatalogSvc.GetFieldsAsync();
            BuildCards();
            ResetFieldForm();
            Snackbar.Add("Đã lưu lĩnh vực.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi lưu lĩnh vực: {ex.Message}", Severity.Error);
        }
    }

    internal void EditField(FieldDto field)
    {
        _fieldForm = new FieldDto
        {
            IDField = field.IDField,
            FieldName = field.FieldName,
            Description = field.Description,
            Delete_flag = field.Delete_flag
        };
        _editingField = true;
    }

    internal async Task DeleteFieldAsync(int id)
    {
        try
        {
            await CatalogSvc.DeleteFieldAsync(id);
            _fields = await CatalogSvc.GetFieldsAsync();
            BuildCards();
            Snackbar.Add("Đã xóa lĩnh vực.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi xóa lĩnh vực: {ex.Message}", Severity.Error);
        }
    }

    internal void ResetFieldForm()
    {
        _fieldForm = new FieldDto();
        _editingField = false;
    }

    internal async Task SaveCareerAsync()
    {
        if (string.IsNullOrWhiteSpace(_careerForm.CareerName))
        {
            Snackbar.Add("Tên ngành nghề không được để trống.", Severity.Warning);
            return;
        }

        if (_careerForm.IDCG == 0 && _careerGroups.Any())
        {
            _careerForm.IDCG = _careerGroups[0].IDCG;
        }

        try
        {
            if (_editingCareer)
            {
                await CatalogSvc.UpdateCareerAsync(_careerForm);
            }
            else
            {
                await CatalogSvc.CreateCareerAsync(_careerForm);
            }
            _careers = await CatalogSvc.GetCareersAsync();
            BuildCards();
            ResetCareerForm();
            Snackbar.Add("Đã lưu ngành nghề.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi lưu ngành nghề: {ex.Message}", Severity.Error);
        }
    }

    internal void EditCareer(CareerDto career)
    {
        _careerForm = new CareerDto
        {
            IDCareer = career.IDCareer,
            CareerName = career.CareerName,
            IDCG = career.IDCG,
            Delete_flag = career.Delete_flag
        };
        _editingCareer = true;
    }

    internal async Task DeleteCareerAsync(int id)
    {
        try
        {
            await CatalogSvc.DeleteCareerAsync(id);
            _careers = await CatalogSvc.GetCareersAsync();
            BuildCards();
            Snackbar.Add("Đã xóa ngành nghề.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi xóa ngành nghề: {ex.Message}", Severity.Error);
        }
    }

    internal void ResetCareerForm()
    {
        _careerForm = new CareerDto
        {
            IDCG = _careerGroups.FirstOrDefault()?.IDCG ?? 0
        };
        _editingCareer = false;
    }

    internal async Task SaveNationalityAsync()
    {
        if (string.IsNullOrWhiteSpace(_nationalityForm.NationalityCode) || string.IsNullOrWhiteSpace(_nationalityForm.NationalityName))
        {
            Snackbar.Add("Vui lòng nhập đầy đủ mã và tên quốc tịch.", Severity.Warning);
            return;
        }

        try
        {
            if (_editingNationality)
            {
                await CatalogSvc.UpdateNationalityAsync(_nationalityForm);
            }
            else
            {
                await CatalogSvc.CreateNationalityAsync(_nationalityForm);
            }
            _nationalities = await CatalogSvc.GetNationalitiesAsync();
            BuildCards();
            ResetNationalityForm();
            Snackbar.Add("Đã lưu quốc tịch.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi lưu quốc tịch: {ex.Message}", Severity.Error);
        }
    }

    internal void EditNationality(NationalityEntityDto nationality)
    {
        _nationalityForm = new NationalityEntityDto
        {
            IDNationality = nationality.IDNationality,
            NationalityCode = nationality.NationalityCode,
            NationalityName = nationality.NationalityName,
            Delete_flag = nationality.Delete_flag
        };
        _editingNationality = true;
    }

    internal async Task DeleteNationalityAsync(int id)
    {
        try
        {
            await CatalogSvc.DeleteNationalityAsync(id);
            _nationalities = await CatalogSvc.GetNationalitiesAsync();
            BuildCards();
            Snackbar.Add("Đã xóa quốc tịch.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi xóa quốc tịch: {ex.Message}", Severity.Error);
        }
    }

    internal void ResetNationalityForm()
    {
        _nationalityForm = new NationalityEntityDto();
        _editingNationality = false;
    }

    internal async Task SaveDistrictAsync()
    {
        if (string.IsNullOrWhiteSpace(_districtForm.DisTrictName))
        {
            Snackbar.Add("Tên quận/huyện không được để trống.", Severity.Warning);
            return;
        }

        try
        {
            if (_editingDistrict)
            {
                await CatalogSvc.UpdateDistrictAsync(_districtForm);
            }
            else
            {
                await CatalogSvc.CreateDistrictAsync(_districtForm);
            }
            _districts = await CatalogSvc.GetDistrictsAsync();
            BuildCards();
            ResetDistrictForm();
            Snackbar.Add("Đã lưu quận/huyện.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi lưu quận/huyện: {ex.Message}", Severity.Error);
        }
    }

    internal void EditDistrict(DistrictDto district)
    {
        _districtForm = new DistrictDto
        {
            IDDistrict = district.IDDistrict,
            DisTrictName = district.DisTrictName,
            Delete_flag = district.Delete_flag
        };
        _editingDistrict = true;
    }

    internal async Task DeleteDistrictAsync(int id)
    {
        try
        {
            await CatalogSvc.DeleteDistrictAsync(id);
            _districts = await CatalogSvc.GetDistrictsAsync();
            BuildCards();
            Snackbar.Add("Đã xóa quận/huyện.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi xóa quận/huyện: {ex.Message}", Severity.Error);
        }
    }

    internal void ResetDistrictForm()
    {
        _districtForm = new DistrictDto();
        _editingDistrict = false;
    }

    internal async Task SaveWardAsync()
    {
        if (string.IsNullOrWhiteSpace(_wardForm.WardName))
        {
            Snackbar.Add("Tên phường/xã không được để trống.", Severity.Warning);
            return;
        }

        try
        {
            if (_editingWard)
            {
                await CatalogSvc.UpdateWardAsync(_wardForm);
            }
            else
            {
                await CatalogSvc.CreateWardAsync(_wardForm);
            }
            _wards = await CatalogSvc.GetWardsAsync();
            BuildCards();
            ResetWardForm();
            Snackbar.Add("Đã lưu phường/xã.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi lưu phường/xã: {ex.Message}", Severity.Error);
        }
    }

    internal void EditWard(WardDto ward)
    {
        _wardForm = new WardDto
        {
            IDWard = ward.IDWard,
            WardName = ward.WardName,
            Delete_flag = ward.Delete_flag
        };
        _editingWard = true;
    }

    internal async Task DeleteWardAsync(int id)
    {
        try
        {
            await CatalogSvc.DeleteWardAsync(id);
            _wards = await CatalogSvc.GetWardsAsync();
            BuildCards();
            Snackbar.Add("Đã xóa phường/xã.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi xóa phường/xã: {ex.Message}", Severity.Error);
        }
    }

    internal void ResetWardForm()
    {
        _wardForm = new WardDto();
        _editingWard = false;
    }

    // ── AdministrativeUnits (Địa bàn hành chính) ──

    internal IEnumerable<AdministrativeUnitDto> FilteredAdminUnits =>
        string.IsNullOrWhiteSpace(_adminUnitTypeFilter)
            ? _adminUnits
            : _adminUnits.Where(u => u.TypeCode == _adminUnitTypeFilter);

    internal async Task SaveAdminUnitAsync()
    {
        if (string.IsNullOrWhiteSpace(_adminUnitForm.Code) || string.IsNullOrWhiteSpace(_adminUnitForm.Name))
        {
            Snackbar.Add("Vui lòng nhập đầy đủ mã và tên địa bàn.", Severity.Warning);
            return;
        }

        try
        {
            if (_editingAdminUnit)
            {
                await CatalogSvc.UpdateAdministrativeUnitAsync(_adminUnitForm);
            }
            else
            {
                await CatalogSvc.CreateAdministrativeUnitAsync(_adminUnitForm);
            }
            _adminUnits = await CatalogSvc.GetAllAdministrativeUnitsAsync();
            BuildCards();
            ResetAdminUnitForm();
            Snackbar.Add("Đã lưu địa bàn hành chính.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi lưu địa bàn: {ex.Message}", Severity.Error);
        }
    }

    internal void EditAdminUnit(AdministrativeUnitDto unit)
    {
        _adminUnitForm = new AdministrativeUnitDto
        {
            Id = unit.Id,
            Code = unit.Code,
            Name = unit.Name,
            TypeCode = unit.TypeCode,
            ParentId = unit.ParentId,
            PredecessorId = unit.PredecessorId,
            ValidFrom = unit.ValidFrom,
            ValidTo = unit.ValidTo,
            LegacyDistrictId = unit.LegacyDistrictId,
            LegacyWardId = unit.LegacyWardId
        };
        _editingAdminUnit = true;
    }

    internal async Task DeactivateAdminUnitAsync(int id)
    {
        try
        {
            await CatalogSvc.DeactivateAdministrativeUnitAsync(id, null);
            _adminUnits = await CatalogSvc.GetAllAdministrativeUnitsAsync();
            BuildCards();
            Snackbar.Add("Đã vô hiệu hóa địa bàn.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi vô hiệu hóa: {ex.Message}", Severity.Error);
        }
    }

    internal void ResetAdminUnitForm()
    {
        _adminUnitForm = new AdministrativeUnitDto { ValidFrom = DateTime.Today };
        _editingAdminUnit = false;
    }
}
