using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.JSInterop;
using IRM.Features.ImportExport.Help;

namespace IRM.Components.Pages;

public partial class Import
{
    [Inject] public IDialogService GuidanceDialogs { get; set; } = default!;
    private readonly string _tourRootId = $"irm-import-tour-{Guid.NewGuid():N}";
    private int _tourContextVersion;
    private bool _guidanceDisposed;

    private ImportTourSnapshot TourSnapshot() => new(_currentStep, _isProcessing || _isDownloadingSample, _manualMode, _tourContextVersion);

    internal async Task StartImportTourAsync()
    {
        if (_guidanceDisposed) return;
        try
        {
            if (!await JS.InvokeAsync<bool>("irmImportTour.start", _tourRootId, TourSnapshot()))
                Snackbar.Add("Hướng dẫn chưa sẵn sàng. Bạn vẫn có thể thao tác import và mở lại hướng dẫn.", Severity.Info);
        }
        catch (Exception ex) when (IsGuidanceInteropFailure(ex))
        {
            Snackbar.Add("Không thể mở hướng dẫn lúc này. Thao tác import vẫn dùng được.", Severity.Info);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_guidanceDisposed) return;
        try { await JS.InvokeVoidAsync("irmImportTour.sync", _tourRootId, TourSnapshot()); }
        catch (Exception ex) when (IsGuidanceInteropFailure(ex)) { /* Guidance must not break the import workflow. */ }
    }

    public async ValueTask DisposeAsync()
    {
        if (_guidanceDisposed) return;
        _guidanceDisposed = true;
        _fileStream?.Dispose();
        try { await JS.InvokeVoidAsync("irmImportTour.dispose", _tourRootId); }
        catch (Exception ex) when (IsGuidanceInteropFailure(ex)) { /* Circuit may already be closed. */ }
    }

    private static bool IsGuidanceInteropFailure(Exception ex) =>
        ex is JSException or JSDisconnectedException or ObjectDisposedException or InvalidOperationException;

    internal async Task ShowFieldHelpAsync(string? fieldKey)
    {
        try { await JS.InvokeVoidAsync("irmImportTour.stop", _tourRootId); }
        catch (Exception ex) when (IsGuidanceInteropFailure(ex)) { /* Field help also works without Driver. */ }
        var parameters = new DialogParameters<ImportFieldHelpDialog> { { dialog => dialog.FieldKey, fieldKey } };
        await GuidanceDialogs.ShowAsync<ImportFieldHelpDialog>("Hướng dẫn dữ liệu Excel", parameters,
            new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton = true, CloseOnEscapeKey = true });
    }
}
