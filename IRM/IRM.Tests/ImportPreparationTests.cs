using System.Net;
using System.Reflection;
using IRM.Application.Features.ImportExport;
using IRM.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;

namespace IRM.Tests;

public class ImportPreparationTests
{
    [Fact]
    public void WizardStartsAndResetsAtPreparationAndKeepsFileWhenGoingBack()
    {
        var page = new Import();
        Assert.Equal(0, Stage(page));
        using var file = new MemoryStream([1, 2]);
        GuidanceRender.Set(page, "_fileStream", file);
        Invoke(page, "GoToUpload");
        Assert.Equal(1, Stage(page));
        Invoke(page, "GoToPreparation");
        Assert.Equal(0, Stage(page));
        Assert.True(file.CanRead);
        Invoke(page, "ResetWizard");
        Assert.Equal(0, Stage(page));
        Assert.False(file.CanRead);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    public void ProgressTreatsAdvancedMappingAsPartOfFileUpload(int stage, int visibleStep)
    {
        var page = new Import();
        GuidanceRender.Set(page, "_currentStep", stage);
        var property = typeof(Import).GetProperty("VisibleStep", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(property);
        Assert.Equal(visibleStep, property.GetValue(page));
    }

    [Fact]
    public async Task PreparationOffersTemplateAndFieldGuidanceBeforeFilePicker()
    {
        var componentType = typeof(Import).Assembly.GetType("IRM.Components.Pages.ImportPreparationSection");
        Assert.NotNull(componentType);
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        var page = new Import();
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.RenderComponentAsync<MudPopoverProvider>();
            var rendered = await renderer.RenderComponentAsync(componentType, ParameterView.FromDictionary(new Dictionary<string, object?> { ["Page"] = page }));
            return WebUtility.HtmlDecode(rendered.ToHtmlString());
        });
        Assert.Contains("Tải file Excel mẫu", html);
        Assert.Contains("Tôi đã chuẩn bị file", html);
        Assert.Contains("Tra cứu 35 trường", html);
        Assert.Contains("ô trống", html);
        Assert.Contains("cập nhật", html);
        foreach (var name in new[] { "Họ tên", "Số hộ chiếu", "Giới tính", "Ngày sinh", "Quốc tịch", "Địa chỉ", "Nghề nghiệp", "Số Visa", "Hạn tạm trú" })
            Assert.Contains(name, html);
        Assert.DoesNotContain("type=\"file\"", html);
        foreach (var anchor in new[] { "prepare-download", "prepare-fields", "prepare-next" })
            Assert.Contains($"data-import-tour=\"{anchor}\"", html);
    }

    [Fact]
    public async Task DownloadFailureReleasesBusyStateAndAllowsRetryWithoutAdvancing()
    {
        await using var provider = Services().BuildServiceProvider();
        await using var renderer = new AttachedRenderer(provider);
        var service = DispatchProxy.Create<IExportService, ExportProxy>();
        var export = (ExportProxy)(object)service;
        var pending = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        export.Result = () => pending.Task;
        var js = new ImportGuidanceInteropTests.RecordingJs();
        var snackbar = provider.GetRequiredService<ISnackbar>();
        var page = new ImportGuidanceInteropTests.TestImport { ExportSvc = service, JS = js, Snackbar = snackbar };
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            renderer.Attach(page);
            GuidanceRender.Set(page, "_currentStep", 0);
            var download = ImportGuidanceInteropTests.Call(page, "DownloadSampleFile");
            Assert.True(Downloading(page));
            await ImportGuidanceInteropTests.Call(page, "DownloadSampleFile");
            Assert.Equal(1, export.Calls);
            pending.SetException(new InvalidOperationException("Catalog unavailable"));
            await download;
            Assert.False(Downloading(page));
            Assert.Equal(0, Stage(page));
            Assert.DoesNotContain(js.Calls, call => call.Name == "irmInterop.downloadFile");
            Assert.Contains(snackbar.ShownSnackbars, item => item.Severity == Severity.Error);

            export.Result = () => Task.FromResult(new byte[] { 1, 2, 3 });
            await ImportGuidanceInteropTests.Call(page, "DownloadSampleFile");
            Assert.False(Downloading(page));
            Assert.Equal(0, Stage(page));
            var call = Assert.Single(js.Calls, call => call.Name == "irmInterop.downloadFile");
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", call.Args[1]);
            Assert.Equal(Convert.ToBase64String([1, 2, 3]), call.Args[2]);
        });
    }

    private static int Stage(Import page) => (int)typeof(Import).GetField("_currentStep", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page)!;
    private static bool Downloading(Import page)
    {
        var field = typeof(Import).GetField("_isDownloadingSample", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return (bool)field.GetValue(page)!;
    }
    private static void Invoke(Import page, string method)
    {
        var target = typeof(Import).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(target);
        target.Invoke(page, null);
    }
    private static IServiceCollection Services() => new ServiceCollection().AddLogging()
        .AddSingleton<NavigationManager, TestNavigation>()
        .AddSingleton<IJSRuntime, ImportGuidanceInteropTests.RecordingJs>()
        .AddMudServices();

    public class ExportProxy : DispatchProxy
    {
        public Func<Task<byte[]>> Result { get; set; } = () => throw new InvalidOperationException();
        public int Calls { get; private set; }
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod!.Name != "GenerateSampleExcelBytesAsync") throw new NotSupportedException(targetMethod.Name);
            Calls++;
            return Result();
        }
    }
    private sealed class TestNavigation : NavigationManager
    {
        public TestNavigation() => Initialize("http://localhost/", "http://localhost/import");
        protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException();
    }
#pragma warning disable BL0006
    private sealed class AttachedRenderer(IServiceProvider services) : Renderer(services, services.GetRequiredService<ILoggerFactory>())
    {
        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
        public void Attach(IComponent component) => AssignRootComponentId(component);
        protected override void HandleException(Exception exception) => throw exception;
        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch) => Task.CompletedTask;
    }
#pragma warning restore BL0006
}
