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
            Snackbar.Add("Ten linh vuc khong duoc de trong.", Severity.Warning);
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
            Snackbar.Add("Da luu linh vuc.", Severity.Success);
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
            Snackbar.Add("Da xoa linh vuc.", Severity.Success);
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
            Snackbar.Add("Ten nganh nghe khong duoc de trong.", Severity.Warning);
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
            Snackbar.Add("Da luu nganh nghe.", Severity.Success);
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
            Snackbar.Add("Da xoa nganh nghe.", Severity.Success);
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
            Snackbar.Add("Vui long nhap day du ma va ten quoc tich.", Severity.Warning);
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
            Snackbar.Add("Da luu quoc tich.", Severity.Success);
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
            Snackbar.Add("Da xoa quoc tich.", Severity.Success);
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
            Snackbar.Add("Ten quan/huyen khong duoc de trong.", Severity.Warning);
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
            Snackbar.Add("Da luu quan/huyen.", Severity.Success);
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
            Snackbar.Add("Da xoa quan/huyen.", Severity.Success);
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
            Snackbar.Add("Ten phuong/xa khong duoc de trong.", Severity.Warning);
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
            Snackbar.Add("Da luu phuong/xa.", Severity.Success);
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
            Snackbar.Add("Da xoa phuong/xa.", Severity.Success);
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
}
