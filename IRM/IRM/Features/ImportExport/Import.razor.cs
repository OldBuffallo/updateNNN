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

public partial class Import : ComponentBase, IFeaturePage
{
    [Inject] public AuthenticationStateProvider AuthState { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;
    [Inject] public ICatalogService CatalogSvc { get; set; } = default!;
    [Inject] public IEmployeeService EmployeeSvc { get; set; } = default!;
    [Inject] public IExportService ExportSvc { get; set; } = default!;
    [Inject] public ICompanyService CompanySvc { get; set; } = default!;
    [Inject] public IImportService ImportSvc { get; set; } = default!;
    [Inject] public IRM.Application.Abstractions.IExcelReader ExcelReader { get; set; } = default!;

    // ── State ──
    internal int _currentStep = 1;
    internal bool _showHistory = false;
    internal bool _isProcessing;
    internal bool _manualMode = false;
    internal bool _hasCompanies;

    // Manual entry
    internal EmployeeDto _mf = new() { Gender = Gender.Male };
    internal CompanyDto? _mfCompany;
    internal List<NationalityEntityDto> _mfNationalities = new();
    internal List<CareerDto> _mfCareers = new();
    internal string _processingMessage = "";

    // Step 1
    internal MemoryStream? _fileStream;
    internal string? _uploadedFileName;
    internal string _sheetInfo = "";
    internal List<string> _sheetNames = new();
    internal int _sheetIndex;
    internal int _headerRow;
    internal int _startRow;
    internal int? _endRow;
    internal int _careerGroupId;
    internal List<CareerGroupDto> _careerGroups = new();
    internal bool _allowCreateCompanies;
    internal CompanyDto? _selectedCompany;
    internal int? _selectedTemplateId;
    internal List<ColumnMappingTemplateDto> _templates = new();

    // Step 2
    internal List<ColumnMapping> _columnMappings = new();
    internal bool _saveTemplate;
    internal string _templateName = "";

    // Step 3
    internal List<ImportPreviewRowDto> _previewRows = new();
    internal Dictionary<int, string> _duplicateDecisions = new();

    // Step 4
    internal ImportResult? _importResult;

    // History
    internal List<ImportSessionInfo> _importHistory = new();

    // ══════════════════════════════════════════
    // LIFECYCLE
    // ══════════════════════════════════════════

    protected override async Task OnInitializedAsync()
    {
        await LoadHistoryAsync();
        await LoadTemplatesAsync();
        var companies = await CompanySvc.GetAllAsync();
        _hasCompanies = companies.Any();
        if (companies.Any())
            _selectedCompany = companies[0];
        _mfNationalities = await CatalogSvc.GetNationalitiesAsync();
        _mfCareers = await CatalogSvc.GetCareersAsync();
        _careerGroups = await CatalogSvc.GetCareerGroupsAsync();
        if (_careerGroups.Count == 1) _careerGroupId = _careerGroups[0].IDCG;
    }

    // ══════════════════════════════════════════
    // STEP 1: UPLOAD
    // ══════════════════════════════════════════

    internal async Task HandleFileUpload(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null) return;

        if (!file.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            Snackbar.Add("Chỉ hỗ trợ file Excel (.xlsx)", Severity.Error);
            return;
        }

        _fileStream?.Dispose();
        _fileStream = new MemoryStream();
        await file.OpenReadStream(maxAllowedSize: 100 * 1024 * 1024).CopyToAsync(_fileStream); // 100MB max
        _uploadedFileName = file.Name;

        try
        {
            _sheetNames = ExcelReader.ReadSheetNames(_fileStream);
            _sheetIndex = 0; _headerRow = 0; _startRow = 0; _endRow = null;
            var (rowCount, colCount, sheetName) = ExcelReader.GetSheetInfo(_fileStream);
            _sheetInfo = $"Sheet: {sheetName} — {rowCount} dòng — {colCount} cột";
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Không thể đọc file: {ex.Message}", Severity.Error);
            _fileStream = null;
            _uploadedFileName = null;
        }

        StateHasChanged();
    }

    internal async Task<IEnumerable<CompanyDto>> SearchCompanyAsync(string? value, CancellationToken cancel)
    {
        return await CompanySvc.GetAllAsync(value);
    }

    // ══════════════════════════════════════════
    // STEP 2: COLUMN MAPPING
    // ══════════════════════════════════════════

    internal Task GoToStep2()
    {
        if (_fileStream == null) return Task.CompletedTask;

        _isProcessing = true;
        _processingMessage = "Đang đọc cấu trúc file Excel...";
        StateHasChanged();

        try
        {
            var headers = ExcelReader.ReadHeaders(_fileStream, _sheetIndex, _headerRow);

            if (_selectedTemplateId.HasValue)
            {
                var template = _templates.FirstOrDefault(t => t.Id == _selectedTemplateId.Value);
                if (template != null)
                {
                    _columnMappings = ImportSvc.ApplyTemplate(template, headers);
                }
                else
                {
                    _columnMappings = ImportSvc.AutoMapColumns(headers);
                }
            }
            else
            {
                _columnMappings = ImportSvc.AutoMapColumns(headers);
            }

            _currentStep = 2;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi đọc file: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isProcessing = false;
        }
        return Task.CompletedTask;
    }

    internal void OnMappingChanged(ColumnMapping mapping, string newValue)
    {
        mapping.SystemField = newValue;
        mapping.SystemFieldLabel = SystemFields.All.GetValueOrDefault(newValue, "");
        mapping.IsAutoMapped = false;
    }

    // ══════════════════════════════════════════
    // STEP 3: PREVIEW
    // ══════════════════════════════════════════

    internal async Task GoToStep3()
    {
        if (_fileStream == null) return;

        _isProcessing = true;
        _processingMessage = "Đang phân tích dữ liệu và kiểm tra trùng lặp...";
        StateHasChanged();

        try
        {
            // Save template if checked
            if (_saveTemplate && !string.IsNullOrWhiteSpace(_templateName))
            {
                var templateUser = (await AuthState.GetAuthenticationStateAsync()).User.Identity?.Name ?? "unknown";
                await ImportSvc.SaveTemplateAsync(_templateName, _selectedCompany?.IDCompany, _columnMappings, templateUser);
                await LoadTemplatesAsync();
                Snackbar.Add($"Đã lưu template '{_templateName}'", Severity.Success);
            }

            _previewRows = await ImportSvc.GeneratePreviewAsync(_fileStream, _columnMappings, _selectedCompany?.IDCompany ?? 0,
                new IRM.Application.Features.ImportExport.ImportReadOptions {
                    SheetIndex = _sheetIndex, HeaderRow = _headerRow, StartRow = _startRow, EndRow = _endRow,
                    CareerGroupId = _careerGroupId, AllowCreateCompanies = _allowCreateCompanies });

            // Updates require an explicit choice.
            _duplicateDecisions = new();
            foreach (var row in _previewRows.Where(r => r.Status == "duplicate" && r.ExistingEmployeeId.HasValue))
            {
                _duplicateDecisions[row.ExistingEmployeeId!.Value] = "skip";
            }

            _currentStep = 3;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi phân tích: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    internal string GetDuplicateDecision(int employeeId)
    {
        return _duplicateDecisions.GetValueOrDefault(employeeId, "skip");
    }

    internal void SetDuplicateDecision(int employeeId, string decision)
    {
        _duplicateDecisions[employeeId] = decision;
    }

    // ══════════════════════════════════════════
    // STEP 4: EXECUTE
    // ══════════════════════════════════════════

    internal async Task ExecuteImport()
    {
        if (_fileStream == null) return;

        _isProcessing = true;
        _processingMessage = "Đang import dữ liệu vào CSDL...";
        StateHasChanged();

        try
        {
            var currentUser = (await AuthState.GetAuthenticationStateAsync()).User.Identity?.Name ?? "unknown";
            _importResult = await ImportSvc.ExecuteImportAsync(
                _previewRows,
                _duplicateDecisions,
                _selectedCompany?.IDCompany ?? 0,
                _uploadedFileName ?? "unknown.xlsx",
                currentUser
            );

            await LoadHistoryAsync();
            _currentStep = 4;

            if (_importResult.Success)
                Snackbar.Add($"Import hoàn tất: +{_importResult.AddedCount} mới, ~{_importResult.UpdatedCount} cập nhật", Severity.Success);
            else
                Snackbar.Add("Import thất bại!", Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Import lỗi: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    // ══════════════════════════════════════════
    // ROLLBACK & HISTORY
    // ══════════════════════════════════════════

    internal async Task RollbackSession(string sessionId)
    {
        _isProcessing = true;
        _processingMessage = "Đang hoàn tác import...";
        StateHasChanged();

        try
        {
            var success = await ImportSvc.RollbackImportAsync(sessionId);
            if (success)
            {
                Snackbar.Add($"Đã hoàn tác phiên {sessionId} — Dữ liệu đã khôi phục.", Severity.Success);
                await LoadHistoryAsync();
            }
            else
            {
                Snackbar.Add("Không thể hoàn tác phiên này.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi hoàn tác: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    internal async Task LoadHistoryAsync()
    {
        try { _importHistory = await ImportSvc.GetHistoryAsync(); }
        catch { _importHistory = new(); }
    }

    internal async Task LoadTemplatesAsync()
    {
        try { _templates = await ImportSvc.GetTemplatesAsync(); }
        catch { _templates = new(); }
    }

    internal void ResetWizard()
    {
        _currentStep = 1;
        _fileStream?.Dispose();
        _fileStream = null;
        _uploadedFileName = null;
        _sheetInfo = "";
        _sheetNames = new(); _sheetIndex = 0; _headerRow = 0; _startRow = 0; _endRow = null; _allowCreateCompanies = false;
        _columnMappings = new();
        _previewRows = new();
        _duplicateDecisions = new();
        _importResult = null;
        _saveTemplate = false;
        _templateName = "";
    }

    // ══════════════════════════════════════════
    // MANUAL ENTRY
    // ══════════════════════════════════════════

    internal async Task SaveManualEntryAsync()
    {
        if (string.IsNullOrWhiteSpace(_mf.StaffName))
        {
            Snackbar.Add("Họ tên không được trống", Severity.Error);
            return;
        }
        if (_mf.IDCompany == 0)
        {
            Snackbar.Add("Chưa chọn công ty", Severity.Error);
            return;
        }

        // Check duplicate passport
        if (!string.IsNullOrWhiteSpace(_mf.Passport))
        {
            var isDup = await EmployeeSvc.CheckDuplicatePassportAsync(_mf.Passport);
            if (isDup)
            {
                Snackbar.Add($"Hộ chiếu {_mf.Passport} đã tồn tại trong hệ thống!", Severity.Warning);
                return;
            }
        }

        try
        {
            await EmployeeSvc.CreateAsync(_mf);
            Snackbar.Add($"Đã thêm nhân viên \"{_mf.StaffName}\" thành công!", Severity.Success);
            ResetManualForm();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi: {ex.Message}", Severity.Error);
        }
    }

    internal void ResetManualForm()
    {
        _mf = new EmployeeDto { Gender = Gender.Male };
        _mfCompany = null;
    }

    internal async Task DownloadSampleFile()
    {
        try
        {
            var bytes = ExportSvc.GenerateSampleExcelBytes();
            var base64 = Convert.ToBase64String(bytes);
            await JS.InvokeVoidAsync("irmInterop.downloadFile",
                "Mau_Import_NLD.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                base64);
            Snackbar.Add("Đã tải file mẫu — Xem sheet 'Hướng dẫn' trong file", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi tải file mẫu: {ex.Message}", Severity.Error);
        }
    }

    public void Dispose()
    {
        _fileStream?.Dispose();
    }

    public Task RefreshAsync() => InvokeAsync(StateHasChanged);
}
