using IRM.Components.Theming;
using Microsoft.JSInterop;

namespace IRM.Tests;

public sealed class IrmThemeControllerTests
{
    [Theory]
    [InlineData("dark", true)]
    [InlineData("light", false)]
    [InlineData("unexpected", false)]
    [InlineData(null, false)]
    public async Task Restore_ReadsOnceAndWaitsForRenderedPaletteBeforeApplying(string? saved, bool dark)
    {
        var js = new ThemeBrowser { Saved = saved };
        var theme = new IrmThemeController(js);
        await theme.InitializeAsync();
        await theme.InitializeAsync();
        Assert.True(theme.IsReady);
        Assert.Equal(dark, theme.IsDarkMode);
        Assert.True(theme.NeedsApply);
        Assert.Equal(1, js.Reads);
        Assert.Empty(js.Applied);
        await theme.ApplyRenderedAsync();
        Assert.Equal(new[] { dark ? "dark" : "light" }, js.Applied);
        Assert.False(theme.NeedsApply);
    }

    [Fact]
    public async Task FailedPreferenceRestore_ShowsUsableLightMode()
    {
        var theme = new IrmThemeController(new ThemeBrowser { ReadFailure = new JSException("Storage unavailable") });
        await theme.InitializeAsync();
        Assert.True(theme.IsReady);
        Assert.False(theme.IsDarkMode);
    }

    [Fact]
    public async Task CancelledPreferenceRestore_DoesNotLeaveThePageWaiting()
    {
        var theme = new IrmThemeController(new ThemeBrowser { ReadFailure = new OperationCanceledException() });
        await theme.InitializeAsync();
        Assert.True(theme.IsReady);
        Assert.False(theme.IsDarkMode);
    }

    [Fact]
    public async Task ToggleBeforeRestore_DoesNotApplyAnUninitializedPreference()
    {
        var js = new ThemeBrowser();
        var theme = new IrmThemeController(js);
        theme.Toggle();
        await theme.ApplyRenderedAsync();
        Assert.False(theme.IsDarkMode);
        Assert.False(theme.IsReady);
        Assert.Empty(js.Applied);
    }

    [Fact]
    public async Task FailedApply_DoesNotBreakTheCurrentModeOrLoopForever()
    {
        var js = new ThemeBrowser { ApplyFailure = new JSException("Browser disconnected") };
        var theme = new IrmThemeController(js);
        await theme.InitializeAsync();
        theme.Toggle();
        await theme.ApplyRenderedAsync();
        Assert.True(theme.IsReady);
        Assert.True(theme.IsDarkMode);
        Assert.False(theme.NeedsApply);
        theme.Toggle();
        js.ApplyFailure = null;
        await theme.ApplyRenderedAsync();
        Assert.False(theme.IsDarkMode);
        Assert.Equal(new[] { "dark", "light" }, js.Applied);
    }

    [Fact]
    public async Task ToggleWhileOlderRenderIsApplying_PreservesTheLatestPendingMode()
    {
        var js = new ThemeBrowser();
        var theme = new IrmThemeController(js);
        await theme.InitializeAsync();
        await theme.ApplyRenderedAsync();
        js.Applied.Clear();
        theme.Toggle();
        js.ApplyGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstApply = theme.ApplyRenderedAsync();
        theme.Toggle();
        js.ApplyGate.SetResult();
        await firstApply;
        Assert.False(theme.IsDarkMode);
        Assert.True(theme.NeedsApply);
        await theme.ApplyRenderedAsync();
        Assert.Equal(new[] { "dark", "light" }, js.Applied);
        Assert.False(theme.NeedsApply);
    }

    private sealed class ThemeBrowser : IJSRuntime
    {
        public string? Saved { get; init; } = "light";
        public Exception? ReadFailure { get; init; }
        public Exception? ApplyFailure { get; set; }
        public TaskCompletionSource? ApplyGate { get; set; }
        public int Reads { get; private set; }
        public List<string> Applied { get; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "irmInterop.getTheme")
            {
                Reads++;
                if (ReadFailure is not null) throw ReadFailure;
                return (TValue)(object)Saved!;
            }
            Assert.Equal("irmInterop.applyTheme", identifier);
            Applied.Add(Assert.IsType<string>(Assert.Single(args!)));
            if (ApplyGate is not null) await ApplyGate.Task.WaitAsync(cancellationToken);
            if (ApplyFailure is not null) throw ApplyFailure;
            return default!;
        }
    }
}
