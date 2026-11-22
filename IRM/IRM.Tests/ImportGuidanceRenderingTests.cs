using System.Net;
using System.Reflection;
using IRM.Components.Pages;
using IRM.Features.ImportExport.Help;
using IRM.Data.Models;
using IRM.Application.Models;
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
    public async Task Result_RollbackCopyStatesGuards()
    {
        var page = new Import(); GuidanceRender.Set(page, "_currentStep", 4);
        GuidanceRender.Set(page, "_importResult", new ImportResult { Success = true });
        var html = WebUtility.HtmlDecode(await GuidanceRender.Render<ImportResultSection>(new() { ["Page"] = page }));
        Assert.DoesNotContain("bất cứ lúc nào", html);
        Assert.Contains("chưa bị chỉnh sửa", html);
        Assert.Contains("phiên/phụ thuộc mới hơn", html);
    }

    [Theory]
    [InlineData(1, "upload,company,career-group,read-options,create-company,go-mapping")]
    [InlineData(2, "field-lookup,mapping,go-preview")]
    [InlineData(3, "preview-summary,preview-table,execute-import")]
    [InlineData(4, "result-summary,result-errors,result-actions")]
    public async Task MappingAndWizard_ExposeStableTourAnchors(int stage, string anchors)
    {
        var page = new Import(); GuidanceRender.Set(page, "_currentStep", stage);
        GuidanceRender.Set(page, "_previewRows", new List<ImportPreviewRowDto> { new() { Status = "new", RowNumber = 2 } });
        GuidanceRender.Set(page, "_importResult", new ImportResult { Success = true, Errors = ["Test error"] });
        var parameters = new Dictionary<string, object?> { ["Page"] = page };
        var html = stage switch {
            1 => await GuidanceRender.Render<ImportUploadSection>(parameters),
            2 => await GuidanceRender.Render<ImportMappingSection>(parameters),
            3 => await GuidanceRender.Render<ImportPreviewSection>(parameters),
            _ => await GuidanceRender.Render<ImportResultSection>(parameters)
        };
        foreach (var anchor in anchors.Split(',')) Assert.Contains($"data-import-tour=\"{anchor}\"", html);
    }

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

    [Fact]
    public async Task Mapping_WarnsWhenRequiredCoreFieldsAreMissing_AndShowsSuccessWhenAllNineMapped()
    {
        var page = new Import();
        GuidanceRender.Set(page, "_currentStep", 2);
        GuidanceRender.Set(page, "_columnMappings", new List<ColumnMapping>
        {
            new() { ExcelColumnIndex = 0, ExcelColumnName = "Hộ chiếu", SystemField = "Passport" }
        });

        var partialHtml = WebUtility.HtmlDecode(await GuidanceRender.Render<ImportMappingSection>(new() { ["Page"] = page }));
        Assert.Contains("Cảnh báo trường bắt buộc", partialHtml);
        Assert.Contains("8/9", partialHtml);
        Assert.Contains("Họ tên", partialHtml);
        Assert.Contains("Quốc tịch", partialHtml);

        var allNineMappings = IRM.Application.Features.ImportExport.ImportMapping.RequiredCoreFields
            .Select((field, index) => new ColumnMapping
            {
                ExcelColumnIndex = index,
                ExcelColumnName = field,
                SystemField = field
            })
            .ToList();
        GuidanceRender.Set(page, "_columnMappings", allNineMappings);

        var completeHtml = WebUtility.HtmlDecode(await GuidanceRender.Render<ImportMappingSection>(new() { ["Page"] = page }));
        Assert.Contains("Đã ghép đủ", completeHtml);
        Assert.Contains("9/9", completeHtml);
        Assert.DoesNotContain("Cảnh báo trường bắt buộc", completeHtml);
    }

    [Fact]
    public async Task UploadSection_WithFileAndMissingCareerGroup_RendersInlineErrorAndWarning()
    {
        var page = new Import();
        GuidanceRender.Set(page, "_currentStep", 1);
        GuidanceRender.Set(page, "_fileStream", new MemoryStream([1, 2, 3]));
        GuidanceRender.Set(page, "_careerGroupId", 0);

        var html = WebUtility.HtmlDecode(await GuidanceRender.Render<ImportUploadSection>(new() { ["Page"] = page }));
        Assert.Contains("Bắt buộc chọn nhóm nghề trước khi tiếp tục", html);
        Assert.Contains("Cảnh báo trường bắt buộc", html);
    }

    [Fact]
    public async Task ManualEntry_SaveWithMissingRequiredFields_BlocksSaveAndMarksSubmitted()
    {
        var snackbar = DispatchProxy.Create<ISnackbar, VietnameseUiTextTests.SnackbarProxy>();
        var empSvc = DispatchProxy.Create<IRM.Application.Features.Employees.IEmployeeService, VietnameseUiTextTests.NoopEmployeeService>();
        var page = new Import
        {
            Snackbar = snackbar,
            EmployeeSvc = empSvc
        };

        var dto = (EmployeeDto)typeof(Import).GetField("_mf", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(page)!;
        dto.StaffName = "Nguyen Van B";
        dto.IDCompany = 1;

        await (Task)typeof(Import).GetMethod("SaveManualEntryAsync", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(page, null)!;

        Assert.False(((VietnameseUiTextTests.NoopEmployeeService)(object)empSvc).CreateCalled);
        Assert.True((bool)typeof(Import).GetField("_manualSubmitted", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(page)!);
        var msg = Assert.Single(((VietnameseUiTextTests.SnackbarProxy)(object)snackbar).Messages);
        Assert.Contains("Quốc tịch", msg);
        Assert.Contains("Địa chỉ", msg);
    }

    [Fact]
    public void ManualEntrySource_WiresInlineErrorsWithoutBlockingSubmitClick()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "IRM", "IRM.sln"))) directory = directory.Parent;
        Assert.NotNull(directory);

        var importRazor = File.ReadAllText(Path.Combine(directory!.FullName, "IRM", "IRM", "Features", "ImportExport", "Import.razor"));
        Assert.Contains("_manualSubmitted", importRazor);
        Assert.Contains("Bắt buộc nhập địa chỉ tạm trú", importRazor);
        Assert.DoesNotContain("Disabled=\"@(string.IsNullOrWhiteSpace(_mf.StaffName) || _mf.IDCompany == 0)\"", importRazor);
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
