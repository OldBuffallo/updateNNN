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
