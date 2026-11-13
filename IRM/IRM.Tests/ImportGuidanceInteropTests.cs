using System.Reflection;
using System.Text.Json;
using IRM.Components.Pages;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using IRM.Infrastructure.Excel;

namespace IRM.Tests;

public class ImportGuidanceInteropTests
{
    [Fact]
    public async Task ReplacingLoadedFileOrResetInvalidatesTourButFirstUploadDoesNot()
    {
        var page = new TestImport { JS = new RecordingJs(), ExcelReader = new ExcelReader() };
        await using var provider = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new AttachedRenderer(provider);
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            renderer.Attach(page);
            using var workbook = LegacyImportEndToEndTests.Workbook([LegacyImportEndToEndTests.ValidRow()]);
            using var copy = new MemoryStream(); workbook.CopyTo(copy);
            var first = new InputFileChangeEventArgs([new TestFile("first.xlsx", copy.ToArray())]);
            await Call(page, "HandleFileUpload", first);
            Assert.Equal(0, ContextVersion(page));
            await Call(page, "HandleFileUpload", new InputFileChangeEventArgs([new TestFile("second.xlsx", copy.ToArray())]));
            Assert.Equal(1, ContextVersion(page));
            typeof(Import).GetMethod("ResetWizard", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, null);
            Assert.Equal(2, ContextVersion(page));
            Assert.Null(typeof(Import).GetField("_fileStream", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page));
        });
    }

    [Fact]
    public async Task OpeningHelpStopsTourBeforeDialog()
    {
        var order = new List<string>();
        var js = new RecordingJs { OnCall = name => order.Add(name) };
        var dialog = DispatchProxy.Create<IDialogService, DialogProxy>();
        ((DialogProxy)(object)dialog).OnShow = () => order.Add("dialog");
        var page = new TestImport { JS = js, GuidanceDialogs = dialog };
        await Call(page, "ShowFieldHelpAsync", "Passport");
        Assert.Equal(new[] { "irmImportTour.stop", "dialog" }, order);
    }

    private static int ContextVersion(Import page) => (int)typeof(Import)
        .GetField("_tourContextVersion", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page)!;
    [Fact]
    public async Task StartTour_SendsOnlyUiSnapshotAndNeverRunsImport()
    {
        var js = new RecordingJs();
        var page = new TestImport { JS = js }; // No import service: guidance must not call it.
        GuidanceRender.Set(page, "_currentStep", 3);
        await Call(page, "StartImportTourAsync");
        var call = Assert.Single(js.Calls);
        Assert.Equal("irmImportTour.start", call.Name);
        var json = JsonSerializer.SerializeToElement(call.Args[1], new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(new[] { "contextVersion", "isBusy", "isManual", "stage" }, json.EnumerateObject().Select(p => p.Name).OrderBy(n => n));
        Assert.Equal(3, json.GetProperty("stage").GetInt32());
        var other = new TestImport { JS = js };
        await Call(other, "StartImportTourAsync");
        Assert.NotEqual(call.Args[0], js.Calls[1].Args[0]);
    }

    [Fact]
    public async Task RepeatedRender_SyncsWithoutRestart()
    {
        var js = new RecordingJs(); var page = new TestImport { JS = js };
        await page.RenderOnce(); await page.RenderOnce();
        Assert.Equal(2, js.Calls.Count);
        Assert.All(js.Calls, call => Assert.Equal("irmImportTour.sync", call.Name));
    }

    [Fact]
    public async Task Dispose_DisconnectedCircuitIsSafe()
    {
        var js = new RecordingJs { Disconnected = true };
        var page = new TestImport { JS = js }; var stream = new MemoryStream([1, 2, 3]);
        GuidanceRender.Set(page, "_fileStream", stream);
        await ((IAsyncDisposable)page).DisposeAsync();
        await ((IAsyncDisposable)page).DisposeAsync();
        Assert.False(stream.CanRead);
        Assert.Single(js.Calls);
        Assert.Equal("irmImportTour.dispose", js.Calls[0].Name);
    }

    internal static Task Call(Import page, string method, params object?[] args) =>
        (Task)typeof(Import).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, args)!;

    public sealed class TestImport : Import
    {
        public Task RenderOnce() => OnAfterRenderAsync(false);
        protected override void BuildRenderTree(RenderTreeBuilder builder) { }
    }
    public sealed class RecordingJs : IJSRuntime
    {
        public List<(string Name, object?[] Args)> Calls { get; } = [];
        public bool Disconnected { get; init; }
        public Action<string>? OnCall { get; init; }
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args)
        {
            Calls.Add((identifier, args ?? []));
            OnCall?.Invoke(identifier);
            if (Disconnected) return ValueTask.FromException<T>(new JSDisconnectedException("Test circuit closed"));
            return ValueTask.FromResult(typeof(T) == typeof(bool) ? (T)(object)true : default!);
        }
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<T>(identifier, args);
    }

    public class DialogProxy : DispatchProxy
    {
        public Action? OnShow { get; set; }
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod!.Name == "ShowAsync") { OnShow?.Invoke(); return Task.FromResult<IDialogReference>(null!); }
            throw new NotSupportedException(targetMethod.Name);
        }
    }

    private sealed class TestFile(string name, byte[] data) : IBrowserFile
    {
        public string Name => name;
        public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
        public long Size => data.Length;
        public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) => new MemoryStream(data);
    }

#pragma warning disable BL0006 // Attach the real component render handle to exercise upload/reset events without a UI test package.
    private sealed class AttachedRenderer(IServiceProvider services) : Renderer(services, services.GetRequiredService<ILoggerFactory>())
    {
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
        public void Attach(IComponent component) => AssignRootComponentId(component);
        protected override void HandleException(Exception exception) => throw exception;
        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    }
#pragma warning restore BL0006
}
