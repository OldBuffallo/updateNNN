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
            Snackbar.Add("TÍn linh v?c khÙng du?c d? tr?ng.", Severity.Warning);
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
            Snackbar.Add("–„ luu linh v?c.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi luu linh vuc: {ex.Message}", Severity.Error);
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
            Snackbar.Add("–„ xÛa linh v?c.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi xoa linh vuc: {ex.Message}", Severity.Error);
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
            Snackbar.Add("TÍn ng‡nh ngh? khÙng du?c d? tr?ng.", Severity.Warning);
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
            Snackbar.Add("–„ luu ng‡nh ngh?.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi luu nganh nghe: {ex.Message}", Severity.Error);
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
            Snackbar.Add("–„ xÛa ng‡nh ngh?.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi xoa nganh nghe: {ex.Message}", Severity.Error);
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
            Snackbar.Add("Vui lÚng nh?p d?y d? m„ v‡ tÍn qu?c t?ch.", Severity.Warning);
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
            Snackbar.Add("–„ luu qu?c t?ch.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi luu quoc tich: {ex.Message}", Severity.Error);
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
            Snackbar.Add("–„ xÛa qu?c t?ch.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi xoa quoc tich: {ex.Message}", Severity.Error);
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
            Snackbar.Add("TÍn qu?n/huy?n khÙng du?c d? tr?ng.", Severity.Warning);
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
            Snackbar.Add("–„ luu qu?n/huy?n.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi luu quan/huyen: {ex.Message}", Severity.Error);
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
            Snackbar.Add("–„ xÛa qu?n/huy?n.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi xoa quan/huyen: {ex.Message}", Severity.Error);
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
            Snackbar.Add("TÍn phu?ng/x„ khÙng du?c d? tr?ng.", Severity.Warning);
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
            Snackbar.Add("–„ luu phu?ng/x„.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi luu phuong/xa: {ex.Message}", Severity.Error);
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
            Snackbar.Add("–„ xÛa phu?ng/x„.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi xoa phuong/xa: {ex.Message}", Severity.Error);
        }
    }

    internal void ResetWardForm()
    {
        _wardForm = new WardDto();
        _editingWard = false;
    }

    // ‚îÄ‚îÄ AdministrativeUnits (ƒê·ªãa b√†n h√†nh ch√≠nh) ‚îÄ‚îÄ

    internal IEnumerable<AdministrativeUnitDto> FilteredAdminUnits =>
        string.IsNullOrWhiteSpace(_adminUnitTypeFilter)
            ? _adminUnits
            : _adminUnits.Where(u => u.TypeCode == _adminUnitTypeFilter);

    internal async Task SaveAdminUnitAsync()
    {
        if (string.IsNullOrWhiteSpace(_adminUnitForm.Code) || string.IsNullOrWhiteSpace(_adminUnitForm.Name))
        {
            Snackbar.Add("Vui l√≤ng nh·∫≠p ƒë·∫ßy ƒë·ªß m√£ v√† t√™n ƒë·ªãa b√†n.", Severity.Warning);
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
            Snackbar.Add("ƒê√£ l∆∞u ƒë·ªãa b√†n h√†nh ch√≠nh.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"L·ªói l∆∞u ƒë·ªãa b√†n: {ex.Message}", Severity.Error);
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
            Snackbar.Add("ƒê√£ v√¥ hi·ªáu h√≥a ƒë·ªãa b√†n.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"L·ªói v√¥ hi·ªáu h√≥a: {ex.Message}", Severity.Error);
        }
    }

    internal void ResetAdminUnitForm()
    {
        _adminUnitForm = new AdministrativeUnitDto { ValidFrom = DateTime.Today };
        _editingAdminUnit = false;
    }
}
