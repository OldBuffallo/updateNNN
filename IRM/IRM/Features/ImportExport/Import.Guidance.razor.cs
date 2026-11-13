using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace IRM.Components.Pages;

public partial class Import
{
    [Inject] public IDialogService GuidanceDialogs { get; set; } = default!;

    internal async Task ShowFieldHelpAsync(string? fieldKey)
    {
        var parameters = new DialogParameters<ImportFieldHelpDialog> { { dialog => dialog.FieldKey, fieldKey } };
        await GuidanceDialogs.ShowAsync<ImportFieldHelpDialog>("Hướng dẫn dữ liệu Excel", parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true, CloseOnEscapeKey = true });
    }
}
