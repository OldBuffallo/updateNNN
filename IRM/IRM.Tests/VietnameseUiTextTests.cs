using System.Net;
using System.Reflection;
using System.Text;
using IRM.Components.Pages;
using MudBlazor;

namespace IRM.Tests;

public class VietnameseUiTextTests
{
    [Fact]
    public void CatalogSource_IsValidUtf8()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "IRM", "IRM.sln"))) directory = directory.Parent;
        Assert.NotNull(directory);
        var path = Path.Combine(directory!.FullName, "IRM", "IRM", "Features", "Administration", "Admin.Catalogs.cs");
        var text = new UTF8Encoding(false, true).GetString(File.ReadAllBytes(path));
        Assert.DoesNotContain('\ufffd', text);
    }

    [Theory]
    [InlineData("SaveFieldAsync", "Tên lĩnh vực không được để trống.")]
    [InlineData("SaveCareerAsync", "Tên ngành nghề không được để trống.")]
    [InlineData("SaveNationalityAsync", "Vui lòng nhập đầy đủ mã và tên quốc tịch.")]
    [InlineData("SaveDistrictAsync", "Tên quận/huyện không được để trống.")]
    [InlineData("SaveWardAsync", "Tên phường/xã không được để trống.")]
    public async Task EmptyCatalogForm_ShowsAccentedVietnamese(string method, string expected)
    {
        var snackbar = DispatchProxy.Create<ISnackbar, SnackbarProxy>();
        var page = new Admin { Snackbar = snackbar }; // Validation requires no database or service writes.
        await (Task)typeof(Admin).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(page, null)!;
        Assert.Equal(expected, Assert.Single(((SnackbarProxy)(object)snackbar).Messages));
    }

    [Fact]
    public async Task AuditFiltersAndEmptyState_RenderAccentedVietnamese()
    {
        var html = WebUtility.HtmlDecode(await GuidanceRender.Render<AdminAuditSection>(new() { ["Page"] = new Admin() }));
        foreach (var label in new[] { "Tên người dùng", "Hành động", "Loại", "Mô tả", "Lọc", "Không có dữ liệu" })
            Assert.Contains(label, html);
        Assert.DoesNotContain("Khong co du lieu", html);
    }

    public class SnackbarProxy : DispatchProxy
    {
        public List<string> Messages { get; } = [];
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod!.Name == "Add") { Messages.Add((string)args![0]!); return null; }
            throw new NotSupportedException(targetMethod.Name);
        }
    }
}
