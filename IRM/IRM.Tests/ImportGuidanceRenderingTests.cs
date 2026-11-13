using System.Net;
using System.Reflection;
using IRM.Components.Pages;
using IRM.Features.ImportExport.Help;
using IRM.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using MudBlazor;

namespace IRM.Tests;

public class ImportGuidanceRenderingTests
{
    [Fact]
    public async Task Mapping_ExposesLookupAndHelpForMappedFields()
    {
        var page = new Import();
        GuidanceRender.Set(page, "_currentStep", 2);
        GuidanceRender.Set(page, "_columnMappings", new List<ColumnMapping> {
            new() { ExcelColumnIndex = 0, ExcelColumnName = "Passport", SystemField = "Passport" },
            new() { ExcelColumnIndex = 1, ExcelColumnName = "STT", SystemField = "skip" }
        });
        var html = WebUtility.HtmlDecode(await GuidanceRender.Render<ImportMappingSection>(new() { ["Page"] = page }));
        Assert.Contains("Tra cứu 35 trường", html);
        Assert.Contains("Hướng dẫn Số hộ chiếu", html);
        Assert.Contains("Text", html);
        Assert.Contains("số 0 đầu", html);
        Assert.DoesNotContain("Hướng dẫn — Bỏ qua —", html);
    }

    [Theory]
    [InlineData("Gender", "không hiển thị giới tính")]
    [InlineData("WorkPermit", "Chưa có")]
    [InlineData("WorkPermitNumber", "giống chính xác")]
    [InlineData("CardCreationDate", "ngày đầu")]
    [InlineData("DocumentValidFrom", "một ngày chung")]
    [InlineData("DocumentValidTo", ";29/05/2028")]
    [InlineData("ResidenceAddress", "kể cả hạn đến mới khác")]
    public async Task HelpContent_RendersPermitDatesAndConditionalGroups(string key, string requiredMeaning)
    {
        var html = WebUtility.HtmlDecode(await GuidanceRender.Render<ImportFieldHelpContent>(new() { ["Entry"] = ImportFieldHelpCatalog.All[key] }));
        Assert.Contains(requiredMeaning, html);
        Assert.Contains("Chương trình kiểm tra", html);
        Assert.Contains("Khuyến nghị nhập liệu", html);
    }

    [Fact]
    public async Task HelpContent_EncodesExamples()
    {
        var entry = ImportFieldHelpCatalog.All["Passport"] with { ValidExample = "<img src=x onerror=alert(1)>" };
        var html = await GuidanceRender.Render<ImportFieldHelpContent>(new() { ["Entry"] = entry });
        Assert.Contains("&lt;img", html);
        Assert.DoesNotContain("<img", html);
        Assert.Contains("Khuyến nghị nhập liệu", WebUtility.HtmlDecode(html));
    }
}

internal static class GuidanceRender
{
    public static void Set(Import page, string field, object? value) =>
        typeof(Import).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, value);

    public static async Task<string> Render<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        var services = new ServiceCollection().AddLogging();
        services.AddMudServices();
        services.AddSingleton<IJSRuntime, StaticJs>();
        services.AddSingleton<NavigationManager, StaticNavigation>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.RenderComponentAsync<MudPopoverProvider>();
            var result = await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters));
            return result.ToHtmlString();
        });
    }

    private sealed class StaticJs : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => ValueTask.FromResult(default(T)!);
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<T>(identifier, args);
    }
    private sealed class StaticNavigation : NavigationManager
    {
        public StaticNavigation() => Initialize("http://localhost/", "http://localhost/import");
        protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException();
    }
}
