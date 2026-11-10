using IRM.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace IRM.Tests;

public sealed class FeatureSectionTests
{
    [Fact]
    public async Task AsyncChildEvent_RefreshesTheOwningPageAfterItsStateChanges()
    {
        var page = new PageStub();
        var section = new SectionStub(); section.SetPage(page);
        var callback = new EventCallbackWorkItem((Func<Task>)(async () => { await Task.Yield(); page.Step = 2; }));
        await ((IHandleEvent)section).HandleEventAsync(callback, null);
        Assert.Equal(2, page.Step);
        Assert.Equal(1, page.RefreshCount);
    }

    [Fact]
    public async Task FailedChildEvent_StillRefreshesPageAndPreservesTheException()
    {
        var page = new PageStub();
        var section = new SectionStub(); section.SetPage(page);
        var callback = new EventCallbackWorkItem((Action)(() => throw new InvalidOperationException("section error")));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => ((IHandleEvent)section).HandleEventAsync(callback, null));
        Assert.Equal("section error", error.Message);
        Assert.Equal(1, page.RefreshCount);
    }

    private sealed class PageStub : ComponentBase, IFeaturePage
    {
        public int Step { get; set; } = 1;
        public int RefreshCount { get; private set; }
        public Task RefreshAsync() { RefreshCount++; return Task.CompletedTask; }
    }
    private sealed class SectionStub : FeatureSection<PageStub>
    {
        public void SetPage(PageStub page) => Page = page;
    }
}
