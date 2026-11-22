using Microsoft.JSInterop;

namespace IRM.Components.Theming;

public sealed class IrmThemeController(IJSRuntime js)
{
    private bool _initializing;
    private long _version;
    private long _appliedVersion = -1;

    public bool IsReady { get; private set; }
    public bool IsDarkMode { get; private set; }
    public bool NeedsApply => IsReady && _appliedVersion < _version;

    public async Task InitializeAsync()
    {
        if (_initializing) return;
        _initializing = true;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        try
        {
            var saved = await js.InvokeAsync<string?>("irmInterop.getTheme", timeout.Token);
            IsDarkMode = saved == "dark";
        }
        catch (JSException) { IsDarkMode = false; }
        catch (OperationCanceledException) { IsDarkMode = false; }
        finally { IsReady = true; }
    }

    public void Toggle()
    {
        if (!IsReady) return;
        IsDarkMode = !IsDarkMode;
        _version++;
    }

    public async Task ApplyRenderedAsync()
    {
        if (!NeedsApply) return;
        var renderedVersion = _version;
        var mode = IsDarkMode ? "dark" : "light";
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        try
        {
            await js.InvokeVoidAsync("irmInterop.applyTheme", timeout.Token, mode);
        }
        catch (JSException) { /* Palette remains usable when the browser disconnects. */ }
        catch (OperationCanceledException) { /* Do not leave initialization waiting on JS. */ }
        finally { _appliedVersion = Math.Max(_appliedVersion, renderedVersion); }
    }
}
