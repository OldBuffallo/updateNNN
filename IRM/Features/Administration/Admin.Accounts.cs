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

    internal async Task SaveAccountAsync()
    {
        if (string.IsNullOrWhiteSpace(_accountForm.Username) || string.IsNullOrWhiteSpace(_accountForm.Name) || string.IsNullOrWhiteSpace(_accountForm.Password))
        {
            Snackbar.Add("Vui long nhap day du thong tin tai khoan.", Severity.Warning);
            return;
        }

        try
        {
            if (_editingAccount)
            {
                await AuthSvc.UpdateAccountAsync(_accountForm);
                Snackbar.Add("Cập nhật tài khoản thành công.", Severity.Success);
            }
            else
            {
                await AuthSvc.CreateAccountAsync(_accountForm);
                Snackbar.Add("Thêm tài khoản thành công.", Severity.Success);
            }
            _accounts = await AuthSvc.GetAllAccountsAsync();
            BuildCards();
            ResetAccountForm();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi luu tai khoan: {ex.Message}", Severity.Error);
        }
    }

    internal void EditAccount(AccountDto account)
    {
        _accountForm = new AccountDto
        {
            IDUser = account.IDUser,
            Username = account.Username,
            Name = account.Name,
            Password = account.Password,
            Permission = account.Permission,
            Delete_flag = account.Delete_flag
        };
        _editingAccount = true;
    }

    internal async Task DeleteAccountAsync(int id)
    {
        try
        {
            await AuthSvc.DeleteAccountAsync(id);
            _accounts = await AuthSvc.GetAllAccountsAsync();
            BuildCards();
            Snackbar.Add("Da xoa tai khoan.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi xoa tai khoan: {ex.Message}", Severity.Error);
        }
    }

    internal void ResetAccountForm()
    {
        _accountForm = new AccountDto { Permission = 0 };
        _editingAccount = false;
    }
}
