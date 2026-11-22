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

    internal async Task LoadAuditAsync()
    {
        try
        {
            var action = string.IsNullOrWhiteSpace(_auditFilterAction) ? null : _auditFilterAction.Trim().ToUpperInvariant();
            var username = string.IsNullOrWhiteSpace(_auditFilterUsername) ? null : _auditFilterUsername.Trim();
            _auditLogs = await AuditSvc.SearchAsync(username, action, _auditFrom, _auditTo);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Loi tai audit log: {ex.Message}", Severity.Error);
        }
    }

    internal static Color GetActionColor(string? action) => action switch
    {
        "CREATE" or "INSERT" => Color.Success,
        "UPDATE" => Color.Warning,
        "DELETE" => Color.Error,
        "LOGIN" => Color.Info,
        "IMPORT" => Color.Primary,
        _ => Color.Default
    };
}
