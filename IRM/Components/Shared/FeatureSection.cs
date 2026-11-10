using Microsoft.AspNetCore.Components;

namespace IRM.Components.Shared;

public interface IFeaturePage
{
    Task RefreshAsync();
}

/// <summary>Sections share their page coordinator; every child event also refreshes the parent.</summary>
public abstract class FeatureSection<TPage> : ComponentBase, IHandleEvent where TPage : ComponentBase, IFeaturePage
{
    [Parameter, EditorRequired] public TPage Page { get; set; } = default!;

    async Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg)
    {
        try { await callback.InvokeAsync(arg); }
        finally { await Page.RefreshAsync(); }
    }
}
