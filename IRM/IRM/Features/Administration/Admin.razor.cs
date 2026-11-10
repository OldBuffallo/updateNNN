using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using IRM.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MudBlazor;

namespace IRM.Components.Pages;

public partial class Admin : ComponentBase, IFeaturePage
{
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public IImportService ImportSvc { get; set; } = default!;
    [Inject] public ICatalogService CatalogSvc { get; set; } = default!;
    [Inject] public IAuditService AuditSvc { get; set; } = default!;
    [Inject] public IAuthService AuthSvc { get; set; } = default!;
    [Inject] public IForeignerRegistryService RegistrySvc { get; set; } = default!;

    [Parameter]
    public string? SectionKey { get; set; }

    internal record AdminCardInfo(string Key, string Icon, string Title, string Description);

    internal bool _loading;
    internal string _activeSection = "";
    internal string _activeSectionTitle = "";

    internal readonly List<AdminCardInfo> _adminCards = new();

    internal List<AccountDto> _accounts = new();
    internal List<FieldDto> _fields = new();
    internal List<CareerDto> _careers = new();
    internal List<CareerGroupDto> _careerGroups = new();
    internal List<NationalityEntityDto> _nationalities = new();
    internal List<DistrictDto> _districts = new();
    internal List<WardDto> _wards = new();
    internal List<AuditLogDto> _auditLogs = new();
    internal List<ImportSessionInfo> _importHistory = new();

    internal AccountDto _accountForm = new();
    internal bool _editingAccount;
    internal FieldDto _fieldForm = new();
    internal bool _editingField;
    internal CareerDto _careerForm = new();
    internal bool _editingCareer;
    internal NationalityEntityDto _nationalityForm = new();
    internal bool _editingNationality;
    internal DistrictDto _districtForm = new();
    internal bool _editingDistrict;
    internal WardDto _wardForm = new();
    internal bool _editingWard;

    internal List<AdministrativeUnitDto> _adminUnits = new();
    internal AdministrativeUnitDto _adminUnitForm = new() { ValidFrom = DateTime.Today };
    internal bool _editingAdminUnit;
    internal string _adminUnitTypeFilter = "";

    internal string? _auditFilterUsername;
    internal string? _auditFilterAction;
    internal DateTime? _auditFrom;
    internal DateTime? _auditTo;

    protected override async Task OnInitializedAsync()
    {
        await LoadCoreDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        _activeSection = (SectionKey ?? string.Empty).Trim().ToLowerInvariant();
        _activeSectionTitle = _adminCards.FirstOrDefault(c => c.Key == _activeSection)?.Title ?? _activeSection;
        await LoadActiveSectionDataAsync();
    }

    internal async Task LoadCoreDataAsync()
    {
        _loading = true;
        try
        {
            _accounts = await AuthSvc.GetAllAccountsAsync();
            _fields = await CatalogSvc.GetFieldsAsync();
            _careers = await CatalogSvc.GetCareersAsync();
            _careerGroups = await CatalogSvc.GetCareerGroupsAsync();
            _nationalities = await CatalogSvc.GetNationalitiesAsync();
            _districts = await CatalogSvc.GetDistrictsAsync();
            _wards = await CatalogSvc.GetWardsAsync();
            _adminUnits = await CatalogSvc.GetAllAdministrativeUnitsAsync();
            BuildCards();
            ResetFormsAfterDataLoaded();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Không tải được dữ liệu quản trị: {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    internal async Task LoadActiveSectionDataAsync()
    {
        if (string.IsNullOrWhiteSpace(_activeSection))
        {
            return;
        }

        _loading = true;
        try
        {
            switch (_activeSection)
            {
                case "accounts":
                    _accounts = await AuthSvc.GetAllAccountsAsync();
                    break;
                case "fields":
                    _fields = await CatalogSvc.GetFieldsAsync();
                    break;
                case "careers":
                    _careers = await CatalogSvc.GetCareersAsync();
                    _careerGroups = await CatalogSvc.GetCareerGroupsAsync();
                    if (_careerForm.IDCG == 0 && _careerGroups.Any())
                    {
                        _careerForm.IDCG = _careerGroups[0].IDCG;
                    }
                    break;
                case "nationalities":
                    _nationalities = await CatalogSvc.GetNationalitiesAsync();
                    break;
                case "districts":
                    _districts = await CatalogSvc.GetDistrictsAsync();
                    break;
                case "wards":
                    _wards = await CatalogSvc.GetWardsAsync();
                    break;
                case "admin-units":
                    _adminUnits = await CatalogSvc.GetAllAdministrativeUnitsAsync();
                    break;
                case "audit":
                    await LoadAuditAsync();
                    break;
                case "import-history":
                    _importHistory = await ImportSvc.GetHistoryAsync();
                    break;
            }
            BuildCards();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Không tải được dữ liệu module '{_activeSection}': {ex.Message}", Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    internal void BuildCards()
    {
        _adminCards.Clear();
        _adminCards.Add(new("accounts", Icons.Material.Outlined.ManageAccounts, "Tài khoản", $"{_accounts.Count} tài khoản"));
        _adminCards.Add(new("fields", Icons.Material.Outlined.Business, "Lĩnh vực", $"{_fields.Count} lĩnh vực"));
        _adminCards.Add(new("careers", Icons.Material.Outlined.Work, "Ngành nghề", $"{_careers.Count} ngành"));
        _adminCards.Add(new("nationalities", Icons.Material.Outlined.Public, "Quốc tịch", $"{_nationalities.Count} quốc tịch"));
        _adminCards.Add(new("districts", Icons.Material.Outlined.Map, "Quận / Huyện", $"{_districts.Count} quận/huyện"));
        _adminCards.Add(new("wards", Icons.Material.Outlined.Place, "Phường / Xã", $"{_wards.Count} phường/xã"));
        _adminCards.Add(new("admin-units", Icons.Material.Outlined.LocationCity, "Địa bàn hành chính", $"{_adminUnits.Count} địa bàn"));
        _adminCards.Add(new("legacy-sync", Icons.Material.Outlined.Storage, "Đồng bộ DB cũ", "Nhập từ ReportManagerDB"));
        _adminCards.Add(new("audit", Icons.Material.Outlined.ReceiptLong, "Nhật ký hệ thống", "Audit log"));
        _adminCards.Add(new("import-history", Icons.Material.Outlined.CloudUpload, "Lịch sử Import", "Lịch sử phiên import"));
    }

    internal void ResetFormsAfterDataLoaded()
    {
        ResetAccountForm();
        ResetFieldForm();
        ResetCareerForm();
        ResetNationalityForm();
        ResetDistrictForm();
        ResetWardForm();
        ResetAdminUnitForm();
    }

    internal void GoSection(string key)
    {
        Nav.NavigateTo($"/admin/{key}");
    }

    internal void OnCardKeyDown(KeyboardEventArgs e, string key)
    {
        if (e.Key is "Enter" or " ")
        {
            GoSection(key);
        }
    }

    public Task RefreshAsync() => InvokeAsync(StateHasChanged);
}
