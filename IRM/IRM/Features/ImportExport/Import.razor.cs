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
    [Inject] public IRM.Application.Abstractions.IExcelWorkbookConverter ExcelConverter { get; set; } = default!;

    // ── State ──
    internal int _currentStep;
    internal bool _isDownloadingSample;
    internal int VisibleStep => _currentStep switch { 0 => 1, 1 or 2 => 2, _ => _currentStep };
    internal bool _showHistory = false;
    internal bool _isProcessing;
    internal bool _manualMode = false;
    internal bool _hasCompanies;

    // Manual entry
    internal EmployeeDto _mf = new() { Gender = Gender.Male };
    internal CompanyDto? _mfCompany;
    internal bool _manualSubmitted;
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
    internal bool _simpleConvertMode;
    internal bool _isDownloadingNormalized;
    internal List<ExcelColumnInfo> _excelHeaders = new();
    internal List<IRM.Application.Features.ImportExport.ExcelFieldSelection> _fieldSelections = new();
    internal IRM.Application.Features.ImportExport.ExcelConvertAnalysis? _convertAnalysis;
    internal HashSet<string> _activeAdminUnitCodes = new(StringComparer.OrdinalIgnoreCase);

    internal IReadOnlyList<(string Key, string Label)> MissingRequiredMappings =>
        _simpleConvertMode && _fieldSelections.Count > 0
            ? _fieldSelections
                .Where(f => f.IsRequired && (!f.IsSelected || f.SourceColumnIndex is null or < 0))
                .Select(f => (f.SystemField, f.Label))
                .ToList()
            : IRM.Application.Features.ImportExport.ImportMapping.GetMissingRequiredFields(_columnMappings);

    internal static bool IsRequiredSystemField(string? key) =>
        !string.IsNullOrWhiteSpace(key) &&
        IRM.Features.ImportExport.Help.ImportFieldHelpCatalog.All.TryGetValue(key, out var entry) &&
        entry.IsRequired;

    internal IReadOnlyList<string> GetMissingManualEntryFields()
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(_mf.Passport)) missing.Add("Số hộ chiếu");
        return missing;
    }

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
        try
        {
            var units = await CatalogSvc.GetAdministrativeUnitsAsync();
            _activeAdminUnitCodes = units.Where(u => !u.IsDeleted).Select(u => u.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch { _activeAdminUnitCodes = new(StringComparer.OrdinalIgnoreCase); }
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

        if (_fileStream is not null) _tourContextVersion++;
        _fileStream?.Dispose();
        _fileStream = new MemoryStream();

        try
        {
            await file.OpenReadStream(maxAllowedSize: 100 * 1024 * 1024).CopyToAsync(_fileStream); // 100MB max
            _uploadedFileName = file.Name;
            _sheetNames = ExcelReader.ReadSheetNames(_fileStream);
            _sheetIndex = 0; _headerRow = 0; _startRow = 0; _endRow = null;
            var (rowCount, colCount, sheetName) = ExcelReader.GetSheetInfo(_fileStream);
            _sheetInfo = $"Sheet: {sheetName} — {rowCount} dòng — {colCount} cột";
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Không thể đọc file: {ex.Message}", Severity.Error);
            _fileStream?.Dispose();
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
            _excelHeaders = headers;

            if (_selectedTemplateId.HasValue)
            {
                var template = _templates.FirstOrDefault(t => t.Id == _selectedTemplateId.Value);
                _columnMappings = template != null
                    ? ImportSvc.ApplyTemplate(template, headers)
                    : ImportSvc.AutoMapColumns(headers);
                _simpleConvertMode = false;
            }
            else
            {
                _columnMappings = ImportSvc.AutoMapColumns(headers);
                _simpleConvertMode = true;
            }

            _fieldSelections = BuildDefaultFieldSelections(headers, _columnMappings);
            RefreshConvertAnalysis();
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

    private static readonly string[] ConverterCoreOrder =
    [
        "StaffName", "Gender", "Birthday", "Nationality", "CompanyName",
        "Career", "WorkPermit", "WorkPermitNumber", "VisaNumber", "VisaSymbol",
        "TemporaryStay", "Address", "Note"
    ];

    private static readonly Dictionary<string, string> ConverterHints = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Passport"] = "Bắt buộc — Giữ số 0 đầu, xóa khoảng trắng thừa",
        ["StaffName"] = "Tự gom khoảng trắng thừa",
        ["Gender"] = "Male/M/1 → Nam · Female/F/0 → Nữ",
        ["Birthday"] = "Đổi chuẩn ngày dd/MM/yyyy",
        ["Nationality"] = "Tên Anh/Việt/2 ký tự → Mã 3 ký tự DB",
        ["CompanyName"] = "Tự khớp tên công ty trong DB",
        ["Career"] = "Tự đối chiếu danh mục nghề nghiệp",
        ["WorkPermit"] = "Chưa có/Không → trống · Miễn GPLĐ → Miễn",
        ["WorkPermitNumber"] = "Giữ nguyên dạng chữ (Text)",
        ["VisaNumber"] = "Giữ nguyên số 0 đầu (Text)",
        ["VisaSymbol"] = "Ký hiệu thị thực (VD: DN1, LĐ2)",
        ["TemporaryStay"] = "Đổi chuẩn ngày dd/MM/yyyy",
        ["Address"] = "Địa chỉ tạm trú (không ép đuôi phường/xã)",
        ["Note"] = "Ghi chú tự do"
    };

    internal static List<IRM.Application.Features.ImportExport.ExcelFieldSelection> BuildDefaultFieldSelections(
        List<ExcelColumnInfo> headers, List<ColumnMapping> mappings)
    {
        var mapByField = mappings
            .Where(m => !string.IsNullOrWhiteSpace(m.SystemField) && m.SystemField != "skip")
            .GroupBy(m => m.SystemField, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().ExcelColumnIndex, StringComparer.OrdinalIgnoreCase);

        var list = new List<IRM.Application.Features.ImportExport.ExcelFieldSelection>();

        void AddField(string key, string group, bool isRequired)
        {
            var matched = mapByField.TryGetValue(key, out var colIdx);
            list.Add(new IRM.Application.Features.ImportExport.ExcelFieldSelection
            {
                SystemField = key,
                Label = SystemFields.All.GetValueOrDefault(key, key),
                Group = group,
                Hint = ConverterHints.GetValueOrDefault(key, "Tự chuẩn hóa định dạng theo chuẩn IRM"),
                IsRequired = isRequired,
                IsSelected = isRequired || matched,
                SourceColumnIndex = matched ? colIdx : null
            });
        }

        AddField("Passport", "required", isRequired: true);
        foreach (var coreKey in ConverterCoreOrder)
            AddField(coreKey, "core", isRequired: false);

        var handled = new HashSet<string>(ConverterCoreOrder, StringComparer.OrdinalIgnoreCase) { "Passport", "skip" };
        foreach (var kv in SystemFields.All)
        {
            if (!handled.Contains(kv.Key))
                AddField(kv.Key, "extended", isRequired: false);
        }

        return list;
    }

    internal void RefreshConvertAnalysis()
    {
        if (_fileStream == null || _fieldSelections.Count == 0) return;
        _convertAnalysis = ExcelConverter.Analyze(
            _fileStream,
            _excelHeaders,
            _fieldSelections,
            _mfNationalities,
            _activeAdminUnitCodes,
            new IRM.Application.Features.ImportExport.ImportReadOptions
            {
                SheetIndex = _sheetIndex,
                HeaderRow = _headerRow,
                StartRow = _startRow,
                EndRow = _endRow
            },
            sampleLimit: 5);
    }

    internal void ToggleFieldSelection(IRM.Application.Features.ImportExport.ExcelFieldSelection item, bool selected)
    {
        if (item.IsRequired) return;
        item.IsSelected = selected;
        RefreshConvertAnalysis();
    }

    internal void ChangeFieldColumn(IRM.Application.Features.ImportExport.ExcelFieldSelection item, int? colIdx)
    {
        item.SourceColumnIndex = colIdx;
        if (colIdx.HasValue)
        {
            item.IsSelected = true;
        }
        else if (!item.IsRequired)
        {
            item.IsSelected = false;
        }
        RefreshConvertAnalysis();
    }

    internal void SelectOnlyPassport()
    {
        foreach (var item in _fieldSelections)
            item.IsSelected = item.IsRequired;
        RefreshConvertAnalysis();
    }

    internal void ResetAutoFieldSelections()
    {
        if (_excelHeaders.Count == 0) return;
        _columnMappings = ImportSvc.AutoMapColumns(_excelHeaders);
        _fieldSelections = BuildDefaultFieldSelections(_excelHeaders, _columnMappings);
        RefreshConvertAnalysis();
    }

    internal void SetConvertMode(bool simple)
    {
        _simpleConvertMode = simple;
        if (simple && _fieldSelections.Count == 0 && _excelHeaders.Count > 0)
        {
            _fieldSelections = BuildDefaultFieldSelections(_excelHeaders, _columnMappings);
            RefreshConvertAnalysis();
        }
    }

    internal async Task DownloadNormalizedWorkbookAsync()
    {
        if (_fileStream == null || _isDownloadingNormalized) return;
        _isDownloadingNormalized = true;
        StateHasChanged();
        try
        {
            var output = ExcelConverter.ConvertToCleanWorkbook(
                _fileStream,
                _fieldSelections,
                _mfNationalities,
                _activeAdminUnitCodes,
                new IRM.Application.Features.ImportExport.ImportReadOptions
                {
                    SheetIndex = _sheetIndex,
                    HeaderRow = _headerRow,
                    StartRow = _startRow,
                    EndRow = _endRow
                });
            var baseName = Path.GetFileNameWithoutExtension(_uploadedFileName ?? "DuLieu");
            var fileName = $"{baseName}_chuan_hoa.xlsx";
            await JS.InvokeVoidAsync("irmInterop.downloadFile",
                fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Convert.ToBase64String(output.WorkbookBytes));
            Snackbar.Add($"Đã tải xuống {fileName} ({output.CleanMappings.Count} cột đã chuẩn hóa).", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi xuất file chuẩn hóa: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isDownloadingNormalized = false;
            StateHasChanged();
        }
    }

    // ══════════════════════════════════════════
    // STEP 3: PREVIEW
    // ══════════════════════════════════════════

    internal async Task GoToStep3()
    {
        if (_fileStream == null) return;

        var missingRequired = MissingRequiredMappings;
        if (missingRequired.Count > 0)
        {
            Snackbar.Add(
                $"Cảnh báo: Còn thiếu {missingRequired.Count} trường bắt buộc chưa ghép cột ({string.Join(", ", missingRequired.Select(m => m.Label))}).",
                Severity.Warning);
        }

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

            if (_simpleConvertMode && _fieldSelections.Count > 0)
            {
                var converted = ExcelConverter.ConvertToCleanWorkbook(
                    _fileStream,
                    _fieldSelections,
                    _mfNationalities,
                    _activeAdminUnitCodes,
                    new IRM.Application.Features.ImportExport.ImportReadOptions
                    {
                        SheetIndex = _sheetIndex,
                        HeaderRow = _headerRow,
                        StartRow = _startRow,
                        EndRow = _endRow
                    });
                using var cleanStream = new MemoryStream(converted.WorkbookBytes);
                _previewRows = await ImportSvc.GeneratePreviewAsync(cleanStream, converted.CleanMappings, _selectedCompany?.IDCompany ?? 0,
                    new IRM.Application.Features.ImportExport.ImportReadOptions
                    {
                        SheetIndex = 0,
                        HeaderRow = 1,
                        StartRow = 2,
                        EndRow = null,
                        CareerGroupId = _careerGroupId,
                        AllowCreateCompanies = _allowCreateCompanies
                    });
            }
            else
            {
                _previewRows = await ImportSvc.GeneratePreviewAsync(_fileStream, _columnMappings, _selectedCompany?.IDCompany ?? 0,
                    new IRM.Application.Features.ImportExport.ImportReadOptions {
                        SheetIndex = _sheetIndex, HeaderRow = _headerRow, StartRow = _startRow, EndRow = _endRow,
                        CareerGroupId = _careerGroupId, AllowCreateCompanies = _allowCreateCompanies });
            }

            // Updates require an explicit choice.
            _duplicateDecisions = new();
            foreach (var row in _previewRows.Where(r => r.Status == "changed" && r.ExistingEmployeeId.HasValue))
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

    internal async Task ToggleCompanyCreationAsync(bool allow)
    {
        _allowCreateCompanies = allow;
        await GoToStep3();
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
                Snackbar.Add("Không thể hoàn tác phiên này. Kiểm tra dữ liệu đã chỉnh sửa và hoàn tác các phiên liên quan mới hơn trước.", Severity.Error);
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
        _tourContextVersion++;
        _currentStep = 0;
        _fileStream?.Dispose();
        _fileStream = null;
        _uploadedFileName = null;
        _sheetInfo = "";
        _sheetNames = new(); _sheetIndex = 0; _headerRow = 0; _startRow = 0; _endRow = null; _allowCreateCompanies = false;
        _columnMappings = new();
        _excelHeaders = new();
        _fieldSelections = new();
        _convertAnalysis = null;
        _simpleConvertMode = true;
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
        _manualSubmitted = true;
        var missing = GetMissingManualEntryFields();
        if (missing.Any())
        {
            Snackbar.Add($"Vui lòng nhập đầy đủ các trường bắt buộc: {string.Join(", ", missing)}", Severity.Warning);
            return;
        }

        try
        {
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

            await EmployeeSvc.CreateAsync(_mf);
            Snackbar.Add($"Đã thêm nhân viên \"{IRM.Core.Rules.PersonDisplayHelper.FormatName(_mf.StaffName, _mf.Passport)}\" thành công!", Severity.Success);
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
        _manualSubmitted = false;
    }

    internal async Task DownloadSampleFile()
    {
        if (_isDownloadingSample) return;
        _isDownloadingSample = true;
        StateHasChanged();
        try
        {
            var bytes = await ExportSvc.GenerateSampleExcelBytesAsync();
            var base64 = Convert.ToBase64String(bytes);
            await JS.InvokeVoidAsync("irmInterop.downloadFile",
                "Mau_Import_HoSo.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                base64);
            Snackbar.Add("Đã gửi file mẫu tới trình duyệt. Kiểm tra thư mục tải xuống và xem sheet Hướng dẫn trong file.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi tải file mẫu: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isDownloadingSample = false;
            StateHasChanged();
        }
    }

    internal void GoToUpload() => _currentStep = 1;

    internal void GoToPreparation() => _currentStep = 0;

    public Task RefreshAsync() => InvokeAsync(StateHasChanged);
}
