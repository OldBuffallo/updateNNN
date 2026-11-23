# Excel Field Converter & 2-Layer Live Notifications Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an intuitive Step 2 **Lọc trường & Chuẩn hóa Excel** mode inside `/import` that lets customers pick only the IRM fields they want (Passport required), auto-normalizes messy cells to IRM format (`dd/MM/yyyy` only), displays 2-layer live status & gap notifications, and outputs either a downloadable clean `.xlsx` or streams directly into Step 3 Preview & Import.

**Architecture:** Place pure cell normalization rules in `IRM.Application/Features/ImportExport/ExcelValueNormalizer.cs` and workbook analysis + `.xlsx` generation in `IRM.Infrastructure/Excel/ExcelWorkbookConverter.cs` behind `IExcelWorkbookConverter` (preserving strict 4-layer boundaries). Wire live selection state, 2-layer notifications, clean `.xlsx` download, and direct in-memory handoff to Step 3 inside `Import.razor.cs` and `ImportMappingSection.razor`, with a toggle to switch to the existing 35-column Advanced mapping table.

**Tech Stack:** .NET 8, ASP.NET Core Blazor Server, MudBlazor, ClosedXML, xUnit, bUnit.

**Spec:**
- `IRM/docs/superpowers/specs/2026-11-22-excel-field-converter-design.md`
- `IRM/docs/superpowers/specs/2026-11-22-excel-field-converter-mockup.html`

## Global Constraints

- **Dates are strictly `dd/MM/yyyy` (Vietnam standard):** convert `yyyy-MM-dd`, `yyyy/MM/dd`, `dd-MM-yyyy`, `dd.MM.yyyy`, `d/M/yyyy`, and Excel serial dates to `dd/MM/yyyy`; do NOT add `MM/dd/yyyy` reversal.
- **Deterministic safe fixes only:** never fabricate or generate passport, visa, or permit numbers. Unparseable/unknown values stay untouched and receive `NeedsReview` (`⚠`).
- **1 cell = 1 field:** no multi-field string splitting inside a single cell.
- **Passport-only required rule (`custom/kh2`):** `Passport` is the sole mandatory field; all other selected/unselected fields are optional and surface clearly in the Post-Conversion Gap Notification.
- **4-layer boundaries:** `IRM.Application` references `IRM.Core` only (no ClosedXML); `IRM.Infrastructure` implements `IExcelWorkbookConverter` with ClosedXML; `IRM` injects `IExcelWorkbookConverter`.
- **Backward compatibility:** Advanced 35-column mapping mode and saved `ColumnMappingTemplate`s remain accessible via the mode switcher in Step 2.

## Review Focus

- **Passport leading zeros preserved through full Convert → Download & Convert → Step 3 pipeline:** `" 0012345 "` written as ClosedXML Text cell (`SetValue(string)`) must survive re-reading by `ExcelReaderHelper.ReadAllRows` as `"0012345"`, never numeric `12345`.
- **Ambiguous date `"05/06/1990"` and invalid calendar date `"31/02/2027"`:** `"05/06/1990"` parses strictly as `5 June 1990` (`"05/06/1990"`), while `"31/02/2027"` stays `"31/02/2027"` with `NeedsReview = true` and appears in the Post-Conversion Gap Notification with its exact Excel row number.
- **WorkPermit status text (`"Chưa có"`, `"Chưa có GPLĐ"`, `"Không có"`, `"0"`) vs real permit (`"GP-2026-01"`):** Status text normalizes to `""` (`WasNormalized = true`) so `LegacyImportRules.ParseWorkPermit` interprets it as *Chưa có GPLĐ* instead of saving the Vietnamese phrase as a permit number; real permit numbers stay untouched.
- **2-Layer Notifications accuracy across full sheet (not just 5 preview rows):** Missing Passport rows, format-error rows (`⚠`), unselected core fields, and blank-cell row numbers must be counted across ALL data rows in the sheet, while the visual sample table renders the first 5 rows.
- **Empty/unmapped Passport column or punctuation-only Passport (`"---"`):** File Status Notification highlights missing/invalid Passport rows clearly, and Step 3 still receives accurate errors without crashing.

---

## File Structure

| Area | Files | Responsibility |
|---|---|---|
| **Models & Normalizer** | `IRM/IRM.Application/Features/ImportExport/Models/ExcelConvertModels.cs`, `IRM/IRM.Application/Features/ImportExport/ExcelValueNormalizer.cs` | Pure cell normalization rules (`dd/MM/yyyy`, Gender, Nationality, WorkPermit, FamilyVisit, Passport, V0.1.0 codes) + analysis/output DTOs |
| **Workbook Converter** | `IRM/IRM.Application/Abstractions/IExcelWorkbookConverter.cs`, `IRM/IRM.Infrastructure/Excel/ExcelWorkbookConverter.cs`, `IRM/IRM.Infrastructure/DependencyInjection.cs` | Full-sheet analysis for 2-layer notifications + clean `.xlsx` generation (selected columns only, Text cells, 1:1 `ColumnMapping`) |
| **Step 2 UI & State** | `IRM/IRM/Features/ImportExport/Import.razor.cs`, `IRM/IRM/Features/ImportExport/Components/ImportMappingSection.razor` | Simple Converter mode (default) + Advanced 35-col toggle, 2-layer live notification banners, 3 field groups, 5-row sample table, download `.xlsx` & direct Step 3 handoff |
| **Tests** | `IRM/IRM.Tests/ExcelValueNormalizerTests.cs`, `IRM/IRM.Tests/ExcelWorkbookConverterTests.cs`, `IRM/IRM.Tests/ImportConverterRenderingTests.cs` | Unit tests for normalizer, workbook converter, and bUnit rendering of Step 2 notifications & mode switching |

---

### Task 1: Pure Cell Normalization Engine & Models (`ExcelValueNormalizer`)

**Files:**
- Create: `IRM/IRM.Application/Features/ImportExport/Models/ExcelConvertModels.cs`
- Create: `IRM/IRM.Application/Features/ImportExport/ExcelValueNormalizer.cs`
- Test: `IRM/IRM.Tests/ExcelValueNormalizerTests.cs`

**Interfaces:**
- Consumes: `IRM.Application.Features.ImportExport.LegacyImportRules`, `IRM.Application.Models.NationalityEntityDto`, `IRM.Data.Models.StayPurposeCodes`, `IRM.Data.Models.ImmigrationDocumentTypeCodes`.
- Produces:
  - `NormalizedCellStatus` (`Empty`, `Valid`, `AutoNormalized`, `NeedsReview`)
  - `NormalizedCellResult(string Value, NormalizedCellStatus Status, string? Note)`
  - `ExcelFieldSelection`, `ConvertedSampleCell`, `ConvertedSampleRow`, `FieldGapSummary`, `ExcelConvertAnalysis`, `ExcelConvertOutput`
  - `ExcelValueNormalizer.Normalize(string systemField, string? rawValue, IReadOnlyList<NationalityEntityDto> nationalities, IReadOnlySet<string>? activeAdminUnitCodes = null) -> NormalizedCellResult`

- [ ] **Step 1: Write failing tests in `IRM/IRM.Tests/ExcelValueNormalizerTests.cs`**

Cover 6 cases:
1. `Normalize_Passport_TrimsWhitespacePreservesLeadingZeros_AndFlagsPunctuationOnly` (`" 0012345 "` → `"0012345"` `AutoNormalized`; `"---"` → `NeedsReview`).
2. `Normalize_Dates_StrictlyUsesDdMmYyyyAndFlagsInvalidCalendarDate` (`"1990-06-03"` → `"03/06/1990"`, `"05/06/1990"` → `"05/06/1990"` `Valid`, `"03.11.1995"` → `"03/11/1995"` `AutoNormalized`, `"31/02/2027"` → `NeedsReview`).
3. `Normalize_Gender_MapsEnglishAndNumericToNamNu_AndFlagsUnknown` (`"Male"` / `"1"` → `"Nam"`, `"Female"` / `"0"` / `"F"` → `"Nữ"`, `"Alien"` → `NeedsReview`).
4. `Normalize_Nationality_MapsEnglishVietnameseAndIso2ToActiveCode` (`"Korea"` / `"KR"` / `"han quoc"` → `"KOR"`, `"ZZZ"` → `NeedsReview`).
5. `Normalize_WorkPermit_CleansStatusPhrasesAndPreservesRealPermit` (`"Chưa có GPLĐ"` / `"Không có"` / `"0"` → `""` `AutoNormalized`, `"Miễn GPLĐ"` → `"Miễn"` `AutoNormalized`, `"GP-2026-01"` → `"GP-2026-01"` `Valid`).
6. `Normalize_FamilyVisitAndV010Codes_NormalizesBooleansAndUppercasesValidCodes` (`"true"` → `"Có"`, `"false"` → `"Không"`, `"qn-w-001"` → `"QN-W-001"` when in active units, `"BAD"` → `NeedsReview`).

- [ ] **Step 2: Run test to verify it fails**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ExcelValueNormalizerTests"`
Expected: FAIL (`ExcelValueNormalizer` not defined).

- [ ] **Step 3: Implement `ExcelConvertModels.cs` and `ExcelValueNormalizer.cs`**

Implement the exact normalization rules from Section 4 of the Design Spec (`dd/MM/yyyy` only, English nationality dictionary mapped against active DB `NationalityEntityDto` codes, WorkPermit status-phrase cleanup, FamilyVisit boolean mapping, V0.1.0 uppercase + validation).

- [ ] **Step 4: Run tests to verify they pass**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ExcelValueNormalizerTests"`
Expected: PASS (6 passed, 0 failed).

- [ ] **Step 5: Commit**

```powershell
git add IRM/IRM.Application/Features/ImportExport/Models/ExcelConvertModels.cs IRM/IRM.Application/Features/ImportExport/ExcelValueNormalizer.cs IRM/IRM.Tests/ExcelValueNormalizerTests.cs
git commit -m "feat(convert): add pure ExcelValueNormalizer and conversion models"
```

---

### Task 2: Workbook Analyzer & Clean `.xlsx` Converter (`ExcelWorkbookConverter`)

**Files:**
- Create: `IRM/IRM.Application/Abstractions/IExcelWorkbookConverter.cs`
- Create: `IRM/IRM.Infrastructure/Excel/ExcelWorkbookConverter.cs`
- Modify: `IRM/IRM.Infrastructure/DependencyInjection.cs` (register `AddScoped<IExcelWorkbookConverter, ExcelWorkbookConverter>()`)
- Test: `IRM/IRM.Tests/ExcelWorkbookConverterTests.cs`

**Interfaces:**
- Consumes: `ExcelValueNormalizer`, `ExcelConvertModels` (Task 1), `ExcelReaderHelper`.
- Produces:
  - `IExcelWorkbookConverter.Analyze(Stream stream, IReadOnlyList<ExcelColumnInfo> headers, IReadOnlyList<ExcelFieldSelection> selections, IReadOnlyList<NationalityEntityDto> nationalities, IReadOnlySet<string> activeAdminUnitCodes, ImportReadOptions options, int sampleLimit = 5) -> ExcelConvertAnalysis`
  - `IExcelWorkbookConverter.ConvertToCleanWorkbook(Stream stream, IReadOnlyList<ExcelFieldSelection> selections, IReadOnlyList<NationalityEntityDto> nationalities, IReadOnlySet<string> activeAdminUnitCodes, ImportReadOptions options) -> ExcelConvertOutput` (returns `byte[] WorkbookBytes`, `List<ColumnMapping> CleanMappings`, `ExcelConvertAnalysis Analysis`)

- [ ] **Step 1: Write failing tests in `IRM/IRM.Tests/ExcelWorkbookConverterTests.cs`**

Cover 3 cases:
1. `Analyze_CountsFullSheetGapsMissingPassportAutoFixesAndWarnings` — messy 6-row workbook with 10 columns (4 junk), 1 row missing Passport, 1 row with invalid date `"31/02/2027"`, 1 row missing StaffName, unselected core fields: verifies Layer-1 stats (`TotalRows == 6`, `TotalSourceColumns == 10`, `MissingPassportRows == [row]`) and Layer-2 gaps (`AutoNormalizedCount > 0`, `WarningDetails` includes bad date row, `EmptyCellDetails` includes blank StaffName row, `UnselectedCoreFields` lists unchecked core fields, `SampleRows.Count == 5`).
2. `ConvertToCleanWorkbook_OutputsOnlySelectedColumnsWithTextPassportAndRoundTripsToImportPreview` — verifies output `.xlsx` has ONLY the selected columns with canonical `SystemFields.All` headers, leading zero `"0012345"` is preserved when re-read via `ExcelReaderHelper.ReadAllRows`, `"Male"` became `"Nam"`, `"Korea"` became `"KOR"`, `"1990-06-03"` became `"03/06/1990"`, and `CleanMappings` feeds directly into `SqlImportStore.GeneratePreviewAsync` with 0 errors.
3. `ConvertToCleanWorkbook_RespectsCustomHeaderStartEndRowsAndSheetIndex` — verifies sheet/row options match `ImportReadOptions`.

- [ ] **Step 2: Run test to verify it fails**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ExcelWorkbookConverterTests"`
Expected: FAIL (`IExcelWorkbookConverter` / `ExcelWorkbookConverter` not defined).

- [ ] **Step 3: Implement `IExcelWorkbookConverter`, `ExcelWorkbookConverter`, and DI registration**

Use ClosedXML to read all rows from the selected sheet/range using the exact same trailing-empty-row logic as `ExcelReaderHelper`, normalize every selected column via `ExcelValueNormalizer`, compute full-sheet Layer-1 & Layer-2 notification stats, build the 5-row sample table, and write clean `.xlsx` bytes with `XLCellValue` text formatting for `Passport`, `VisaNumber`, `WorkPermitNumber`, and date cells formatted as `dd/MM/yyyy` text.

- [ ] **Step 4: Run tests to verify they pass**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ExcelWorkbookConverterTests"`
Expected: PASS (3 passed, 0 failed).

- [ ] **Step 5: Commit**

```powershell
git add IRM/IRM.Application/Abstractions/IExcelWorkbookConverter.cs IRM/IRM.Infrastructure/Excel/ExcelWorkbookConverter.cs IRM/IRM.Infrastructure/DependencyInjection.cs IRM/IRM.Tests/ExcelWorkbookConverterTests.cs
git commit -m "feat(convert): implement ExcelWorkbookConverter with full-sheet gap analysis and clean xlsx export"
```

---

### Task 3: Step 2 Converter UI, 2-Layer Live Notifications & Direct Step 3 Handoff

**Files:**
- Modify: `IRM/IRM/Features/ImportExport/Import.razor.cs` (inject `IExcelWorkbookConverter`; add `_simpleConvertMode`, `_excelHeaders`, `_fieldSelections`, `_convertAnalysis`, `_activeAdminUnitCodes`; wire `BuildDefaultFieldSelections`, `RefreshConvertAnalysis`, `ToggleFieldSelection`, `ChangeFieldColumn`, `SelectOnlyPassport`, `ResetAutoFieldSelections`, `DownloadNormalizedWorkbookAsync`, and update `GoToStep3` to convert via `ExcelConverter` when `_simpleConvertMode` is true)
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportMappingSection.razor` (add mode switcher Simple vs Advanced 35-col; render Layer-1 File Status Notification + Layer-2 Post-Conversion Gap Notification, 3 field groups [Required / Core / Collapsible Extended], 5-row live normalized preview table with `✨`/`⚠` tooltips, and bottom bar with `Tải file Excel đã chuẩn hóa (.xlsx)` + `Tiếp tục Xem trước`)
- Test: `IRM/IRM.Tests/ImportConverterRenderingTests.cs`

**Interfaces:**
- Consumes: `IExcelWorkbookConverter` (Task 2), `ExcelConvertAnalysis` & `ExcelFieldSelection` (Task 1).
- Produces: Complete customer-facing Step 2 Converter UX inside `/import`, preserving Advanced 35-column mapping mode for power users and existing templates.

- [ ] **Step 1: Write failing bUnit tests in `IRM/IRM.Tests/ImportConverterRenderingTests.cs`**

Cover 3 cases:
1. `ImportMappingSection_SimpleMode_RendersTwoLayerNotificationsAndSelectedPreviewColumns` — renders `ImportMappingSection` with `_simpleConvertMode = true` and a populated `_convertAnalysis`, asserting Layer-1 file status text, Layer-2 missing-fields/warnings summary, `Tải file Excel đã chuẩn hóa`, and `✨`/`⚠` preview badges.
2. `ImportMappingSection_AdvancedMode_PreservesClassic35ColumnMappingTable` — renders with `_simpleConvertMode = false` and verifies the classic `"Cột A:"` mapping table and `Tra cứu 35 trường` button render identically.
3. `ImportMappingSection_RequiredPassportUnmapped_ShowsWarningBanner` — verifies missing Passport warning renders in both modes.

- [ ] **Step 2: Run test to verify it fails**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ImportConverterRenderingTests"`
Expected: FAIL.

- [ ] **Step 3: Implement Step 2 state in `Import.razor.cs` and UI in `ImportMappingSection.razor`**

- Note on existing `ImportGuidanceRenderingTests`: keep `_simpleConvertMode = false` when `_fieldSelections` is empty (so tests that only inject `_columnMappings` via reflection without calling `GoToStep2()` still render the classic mapping section cleanly without breaking!), while `GoToStep2()` sets `_simpleConvertMode = true` (or `false` if a saved `_selectedTemplateId` was explicitly chosen) and populates `_fieldSelections` + `_convertAnalysis`.
- In `GoToStep3()`: when `_simpleConvertMode && _fieldSelections.Any()`, call `ExcelConverter.ConvertToCleanWorkbook(...)`, pass a `new MemoryStream(output.WorkbookBytes)` + `output.CleanMappings` + `new ImportReadOptions { SheetIndex = 0, HeaderRow = 1, StartRow = 2, CareerGroupId = _careerGroupId, AllowCreateCompanies = _allowCreateCompanies }` to `ImportSvc.GeneratePreviewAsync(...)`, and keep `_convertedWorkbookBytes` for `ToggleCompanyCreationAsync` re-preview!

- [ ] **Step 4: Run tests to verify they pass**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ImportConverterRenderingTests|FullyQualifiedName~ImportGuidanceRenderingTests"`
Expected: PASS (all tests in both classes pass).

- [ ] **Step 5: Commit**

```powershell
git add IRM/IRM/Features/ImportExport/Import.razor.cs IRM/IRM/Features/ImportExport/Components/ImportMappingSection.razor IRM/IRM.Tests/ImportConverterRenderingTests.cs
git commit -m "feat(import-ui): integrate Step 2 Excel field converter with 2-layer live notifications"
```

---

### Task 4: Full Regression Suite Verification & Spec/Mockup Commit

**Files:**
- Commit design doc & mockup: `IRM/docs/superpowers/specs/2026-11-22-excel-field-converter-design.md`, `IRM/docs/superpowers/specs/2026-11-22-excel-field-converter-mockup.html`, `IRM/docs/superpowers/plans/2026-11-23-excel-field-converter.md`

- [ ] **Step 1: Run full Release build, `.NET` test suite, and Node browser tests**

Run (from `IRM/`):
```powershell
dotnet build IRM.sln -c Release
dotnet test IRM.Tests/IRM.Tests.csproj -c Release --no-build
node --test IRM.Tests/Browser/*.test.cjs
```
Expected: 0 build warnings/errors; all `.NET` tests PASS (0 failed); all 50 Node tests PASS.

- [ ] **Step 2: Commit design spec, mockup, and plan docs**

```powershell
git add IRM/docs/superpowers/specs/2026-11-22-excel-field-converter-design.md IRM/docs/superpowers/specs/2026-11-22-excel-field-converter-mockup.html IRM/docs/superpowers/plans/2026-11-23-excel-field-converter.md
git commit -m "docs(plan): add Excel field converter design spec, mockup, and implementation plan"
```
