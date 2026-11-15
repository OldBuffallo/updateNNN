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
    [InlineData("SaveFieldAsync", "Tên lĩnh vực không được để trống.", "_fieldSubmitted")]
    [InlineData("SaveCareerAsync", "Tên ngành nghề không được để trống.", "_careerSubmitted")]
    [InlineData("SaveNationalityAsync", "Vui lòng nhập đầy đủ mã và tên quốc tịch.", "_nationalitySubmitted")]
    [InlineData("SaveDistrictAsync", "Tên quận/huyện không được để trống.", "_districtSubmitted")]
    [InlineData("SaveWardAsync", "Tên phường/xã không được để trống.", "_wardSubmitted")]
    [InlineData("SaveAdminUnitAsync", "Vui lòng nhập đầy đủ mã và tên địa bàn.", "_adminUnitSubmitted")]
    [InlineData("SaveAccountAsync", "Vui lòng nhập đầy đủ thông tin tài khoản.", "_accountSubmitted")]
    public async Task EmptyCatalogForm_ShowsAccentedVietnamese_AndMarksSubmitted(string method, string expected, string submittedField)
    {
        var snackbar = DispatchProxy.Create<ISnackbar, SnackbarProxy>();
        var page = new Admin { Snackbar = snackbar }; // Validation requires no database or service writes.
        await (Task)typeof(Admin).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(page, null)!;
        Assert.Equal(expected, Assert.Single(((SnackbarProxy)(object)snackbar).Messages));
        var submitted = (bool)typeof(Admin).GetField(submittedField, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(page)!;
        Assert.True(submitted);
    }

    [Fact]
    public async Task InvalidNationalityCode_ShowsThreeLetterWarning()
    {
        var snackbar = DispatchProxy.Create<ISnackbar, SnackbarProxy>();
        var page = new Admin { Snackbar = snackbar };
        var form = (IRM.Application.Models.NationalityEntityDto)typeof(Admin)
            .GetField("_nationalityForm", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(page)!;
        form.NationalityCode = "CN";
        form.NationalityName = "Trung Quốc";

        await (Task)typeof(Admin).GetMethod("SaveNationalityAsync", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(page, null)!;
        Assert.Equal("Mã quốc tịch phải gồm đúng 3 chữ cái (VD: CHN, KOR, USA).", Assert.Single(((SnackbarProxy)(object)snackbar).Messages));
    }

    [Fact]
    public async Task AdminCatalogSections_WhenSubmittedEmpty_RenderInlineErrorsAndAccentedButtons()
    {
        var page = new Admin { Snackbar = DispatchProxy.Create<ISnackbar, SnackbarProxy>() };
        foreach (var field in new[]
        {
            "_fieldSubmitted", "_careerSubmitted", "_nationalitySubmitted",
            "_districtSubmitted", "_wardSubmitted", "_adminUnitSubmitted", "_accountSubmitted"
        })
        {
            typeof(Admin).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(page, true);
        }

        var wardsHtml = WebUtility.HtmlDecode(await GuidanceRender.Render<AdminWardsSection>(new() { ["Page"] = page }));
        Assert.Contains("Bắt buộc nhập tên phường/xã", wardsHtml);
        Assert.DoesNotContain(">Them<", wardsHtml);
        Assert.Contains("Thêm", wardsHtml);

        var careersHtml = WebUtility.HtmlDecode(await GuidanceRender.Render<AdminCareersSection>(new() { ["Page"] = page }));
        Assert.Contains("Bắt buộc nhập tên ngành nghề", careersHtml);
        Assert.Contains("Tên ngành nghề", careersHtml);
        Assert.DoesNotContain("Ten nganh nghe", careersHtml);

        var fieldsHtml = WebUtility.HtmlDecode(await GuidanceRender.Render<AdminFieldsSection>(new() { ["Page"] = page }));
        Assert.Contains("Bắt buộc nhập tên lĩnh vực", fieldsHtml);
        Assert.Contains("Mô tả", fieldsHtml);
        Assert.DoesNotContain("Ten linh vuc", fieldsHtml);

        var districtsHtml = WebUtility.HtmlDecode(await GuidanceRender.Render<AdminDistrictsSection>(new() { ["Page"] = page }));
        Assert.Contains("Bắt buộc nhập tên quận/huyện", districtsHtml);
        Assert.Contains("Thêm", districtsHtml);

        var natHtml = WebUtility.HtmlDecode(await GuidanceRender.Render<AdminNationalitiesSection>(new() { ["Page"] = page }));
        Assert.Contains("Bắt buộc nhập mã quốc tịch", natHtml);
        Assert.Contains("Bắt buộc nhập tên quốc tịch", natHtml);

        var accountsHtml = WebUtility.HtmlDecode(await GuidanceRender.Render<AdminAccountsSection>(new() { ["Page"] = page }));
        Assert.Contains("Bắt buộc nhập tên đăng nhập", accountsHtml);
        Assert.Contains("Bắt buộc nhập họ tên", accountsHtml);
        Assert.Contains("Bắt buộc nhập mật khẩu", accountsHtml);

        var adminUnitsHtml = WebUtility.HtmlDecode(await GuidanceRender.Render<AdminAdministrativeUnitsSection>(new() { ["Page"] = page }));
        Assert.Contains("Bắt buộc nhập mã địa bàn", adminUnitsHtml);
        Assert.Contains("Bắt buộc nhập tên địa bàn", adminUnitsHtml);
    }

    [Fact]
    public async Task EmployeesDialog_SaveWithMissingRequiredFields_BlocksSaveAndReportsAddressAndMissingFields()
    {
        var snackbar = DispatchProxy.Create<ISnackbar, SnackbarProxy>();
        var empSvc = DispatchProxy.Create<IRM.Application.Features.Employees.IEmployeeService, NoopEmployeeService>();
        var page = new Employees();
        typeof(Employees).GetProperty("Snackbar", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)!.SetValue(page, snackbar);
        typeof(Employees).GetProperty("EmployeeSvc", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)!.SetValue(page, empSvc);

        var dto = (IRM.Application.Models.EmployeeDto)typeof(Employees).GetField("_f", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(page)!;
        dto.StaffName = "Nguyen Van A";
        dto.IDCompany = 1;

        await (Task)typeof(Employees).GetMethod("SaveEmployeeAsync", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(page, null)!;

        Assert.False(((NoopEmployeeService)(object)empSvc).CreateCalled);
        Assert.True((bool)typeof(Employees).GetField("_dlgSubmitted", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(page)!);
        var msg = Assert.Single(((SnackbarProxy)(object)snackbar).Messages);
        Assert.Contains("Địa chỉ", msg);
        Assert.Contains("Số hộ chiếu", msg);
    }

    [Fact]
    public void DialogSources_ExposeAddressAndRequiredFieldErrorTexts()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "IRM", "IRM.sln"))) directory = directory.Parent;
        Assert.NotNull(directory);

        var employeesRazor = File.ReadAllText(Path.Combine(directory!.FullName, "IRM", "IRM", "Features", "Employees", "Employees.razor"));
        Assert.Contains("@bind-Value=\"_f.Address\"", employeesRazor);
        Assert.Contains("Bắt buộc nhập địa chỉ tạm trú", employeesRazor);

        var companiesRazor = File.ReadAllText(Path.Combine(directory.FullName, "IRM", "IRM", "Features", "Companies", "Companies.razor"));
        Assert.Contains("Bắt buộc nhập tên công ty", companiesRazor);
        Assert.Contains("_dlgSubmitted", companiesRazor);

        var studentsRazor = File.ReadAllText(Path.Combine(directory.FullName, "IRM", "IRM", "Features", "Students", "Students.razor"));
        Assert.Contains("Bắt buộc nhập họ tên", studentsRazor);
        Assert.Contains("Bắt buộc nhập tên trường", studentsRazor);
        Assert.Contains("_dlgSubmitted", studentsRazor);
    }

    public class NoopEmployeeService : DispatchProxy
    {
        public bool CreateCalled { get; private set; }
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod!.Name == "CreateAsync" || targetMethod.Name == "UpdateAsync")
            {
                CreateCalled = true;
                return Task.CompletedTask;
            }
            if (targetMethod.Name == "GetAllActiveAsync" || targetMethod.Name == "GetByCompanyAsync")
            {
                return Task.FromResult(new List<IRM.Application.Models.EmployeeDto>());
            }
            throw new NotSupportedException(targetMethod.Name);
        }
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
