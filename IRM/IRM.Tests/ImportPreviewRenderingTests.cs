using System.Net;
using System.Reflection;
using IRM.Components.Pages;
using IRM.Application.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;

namespace IRM.Tests;

public class ImportPreviewRenderingTests
{
    [Fact]
    public async Task Preview_ShowsExplicitCompanyCreationChoiceBeforeConfirmation()
    {
        var services=new ServiceCollection().AddLogging();services.AddMudServices();
        services.AddSingleton<IJSRuntime,StaticJs>();
        services.AddSingleton<NavigationManager,StaticNavigation>();
        await using var provider=services.BuildServiceProvider();
        await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var page=new Import();
        typeof(Import).GetField("_currentStep",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,3);
        typeof(Import).GetField("_previewRows",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,
            new List<ImportPreviewRowDto> {new() {RowNumber=9,Status="error",ErrorMessage="Công ty chưa có"}});
        var html=await renderer.Dispatcher.InvokeAsync(async ()=> {
            var result=await renderer.RenderComponentAsync<ImportPreviewSection>(ParameterView.FromDictionary(new Dictionary<string,object?> { ["Page"]=page }));
            return WebUtility.HtmlDecode(result.ToHtmlString());
        });
        Assert.Contains("Cho phép tạo công ty chưa có",html);
        Assert.Contains("Công ty chưa có",html);
    }

    [Fact]
    public async Task Preview_WithWarnings_RendersWarningBannerAndWarningCount()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddMudServices(options => options.PopoverOptions.CheckForPopoverProvider = false);
        services.AddSingleton<IJSRuntime, StaticJs>();
        services.AddSingleton<NavigationManager, StaticNavigation>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        var page = new Import();
        typeof(Import).GetField("_currentStep", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, 3);
        typeof(Import).GetField("_previewRows", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page,
            new List<ImportPreviewRowDto>
            {
                new() { RowNumber = 2, Status = "new", Passport = "P123", Warnings = ["Thiếu họ tên", "Chưa gán công ty"] }
            });
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<ImportPreviewSection>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Page"] = page }));
            return WebUtility.HtmlDecode(result.ToHtmlString());
        });
        Assert.Contains("1 cảnh báo", html);
        Assert.Contains("mud-alert-text-warning", html);
        Assert.Contains("Thiếu họ tên", html);
    }

    [Fact]
    public async Task UploadSection_WithFileAndNoCareerGroup_EnablesContinueButton()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddMudServices(options => options.PopoverOptions.CheckForPopoverProvider = false);
        services.AddSingleton<IJSRuntime, StaticJs>();
        services.AddSingleton<NavigationManager, StaticNavigation>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        var page = new Import();
        typeof(Import).GetField("_currentStep", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, 1);
        typeof(Import).GetField("_fileStream", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, new MemoryStream([1, 2, 3]));
        typeof(Import).GetField("_careerGroupId", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, 0);
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<ImportUploadSection>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Page"] = page }));
            return WebUtility.HtmlDecode(result.ToHtmlString());
        });
        Assert.Contains("data-import-tour=\"go-mapping\"", html);
        var btnIdx = html.IndexOf("data-import-tour=\"go-mapping\"", StringComparison.Ordinal);
        var tagStart = html.LastIndexOf("<button", btnIdx, StringComparison.Ordinal);
        var tagEnd = html.IndexOf(">", btnIdx, StringComparison.Ordinal);
        var buttonOpenTag = html.Substring(tagStart, tagEnd - tagStart + 1);
        Assert.DoesNotContain("disabled", buttonOpenTag, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResultSection_WithMoreThanFiveErrors_ShowsAllErrorsWithoutFiveItemCutoff()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddMudServices(options => options.PopoverOptions.CheckForPopoverProvider = false);
        services.AddSingleton<IJSRuntime, StaticJs>();
        services.AddSingleton<NavigationManager, StaticNavigation>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        var page = new Import();
        typeof(Import).GetField("_currentStep", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, 4);
        typeof(Import).GetField("_importResult", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page,
            new IRM.Data.Models.ImportResult
            {
                Success = false,
                TotalRows = 7,
                ErrorCount = 7,
                Errors = Enumerable.Range(1, 7).Select(i => $"Lỗi chi tiết số {i}").ToList()
            });
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<ImportResultSection>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Page"] = page }));
            return WebUtility.HtmlDecode(result.ToHtmlString());
        });
        Assert.Contains("Lỗi chi tiết số 6", html);
        Assert.Contains("Lỗi chi tiết số 7", html);
    }
    private sealed class StaticJs : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)=>ValueTask.FromResult(default(TValue)!);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier,CancellationToken cancellationToken,object?[]? args)=>ValueTask.FromResult(default(TValue)!);
    }
    private sealed class StaticNavigation : NavigationManager
    {
        public StaticNavigation()=>Initialize("http://localhost/","http://localhost/import");
        protected override void NavigateToCore(string uri,bool forceLoad)=>throw new NotSupportedException();
    }
}
