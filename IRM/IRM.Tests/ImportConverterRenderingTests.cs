using System.Net;
using System.Reflection;
using IRM.Application.Features.ImportExport;
using IRM.Components.Pages;
using IRM.Data.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;

namespace IRM.Tests;

public sealed class ImportConverterRenderingTests
{
    private static async Task<string> RenderMappingAsync(Import page)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddMudServices(o => o.PopoverOptions.CheckForPopoverProvider = false);
        services.AddSingleton<IJSRuntime, StaticJs>();
        services.AddSingleton<NavigationManager, StaticNavigation>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<ImportMappingSection>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["Page"] = page }));
            return WebUtility.HtmlDecode(result.ToHtmlString());
        });
    }

    private static void SetField(Import page, string name, object? value) =>
        typeof(Import).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(page, value);

    [Fact]
    public async Task SimpleMode_RendersTwoLayerNotificationsAndSelectedPreviewColumns()
    {
        var page = new Import();
        SetField(page, "_currentStep", 2);
        SetField(page, "_uploadedFileName", "messy.xlsx");
        SetField(page, "_simpleConvertMode", true);
        SetField(page, "_excelHeaders", new List<ExcelColumnInfo>
        {
            new() { Index = 0, HeaderName = "So HC" },
            new() { Index = 1, HeaderName = "Sex" },
            new() { Index = 2, HeaderName = "Junk" }
        });
        SetField(page, "_fieldSelections", new List<ExcelFieldSelection>
        {
            new() { SystemField = "Passport", Label = "Số hộ chiếu", Group = "required", IsRequired = true, IsSelected = true, SourceColumnIndex = 0 },
            new() { SystemField = "Gender", Label = "Giới tính", Group = "core", IsSelected = true, SourceColumnIndex = 1 },
            new() { SystemField = "Address", Label = "Địa chỉ", Group = "core", IsSelected = false, SourceColumnIndex = null }
        });
        SetField(page, "_convertAnalysis", new ExcelConvertAnalysis
        {
            TotalDataRows = 5,
            TotalSourceColumns = 3,
            AutoMatchedColumnCount = 2,
            SelectedColumnCount = 2,
            IgnoredColumnCount = 1,
            TotalAutoNormalizedCells = 3,
            TotalWarningCells = 1,
            UnselectedCoreFieldLabels = ["Địa chỉ"],
            WarningFields = [new FieldIssueSummary("Gender", "Giới tính", 1, [4], ["Alien"])],
            EmptyCellFields = [new FieldIssueSummary("Gender", "Giới tính", 1, [5], [])],
            SampleRows =
            [
                new ConvertedSampleRow(2, [
                    new ConvertedSampleCell("Passport", " 001234 ", "001234", NormalizedCellStatus.AutoNormalized, "Xóa khoảng trắng"),
                    new ConvertedSampleCell("Gender", "Male", "Nam", NormalizedCellStatus.AutoNormalized, "Chuẩn hóa giới tính")
                ])
            ]
        });

        var html = await RenderMappingAsync(page);

        Assert.Contains("Tình trạng file mới tải lên", html);
        Assert.Contains("Thông báo sau khi lọc", html);
        Assert.Contains("Địa chỉ", html);
        Assert.Contains("Tải file Excel đã chuẩn hóa", html);
        Assert.Contains("001234", html);
        Assert.Contains("Nam", html);
    }

    [Fact]
    public async Task AdvancedMode_PreservesClassic35ColumnMappingTable()
    {
        var page = new Import();
        SetField(page, "_currentStep", 2);
        SetField(page, "_simpleConvertMode", false);
        SetField(page, "_columnMappings", new List<ColumnMapping>
        {
            new() { ExcelColumnIndex = 0, ExcelColumnName = "Hộ chiếu", SystemField = "Passport", IsAutoMapped = true }
        });

        var html = await RenderMappingAsync(page);

        Assert.Contains("Cột A: \"Hộ chiếu\"", html);
        Assert.Contains("Tra cứu 35 trường", html);
    }

    private sealed class StaticJs : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => ValueTask.FromResult(default(TValue)!);
    }

    private sealed class StaticNavigation : NavigationManager
    {
        public StaticNavigation() => Initialize("http://localhost/", "http://localhost/import");
        protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException();
    }
}
