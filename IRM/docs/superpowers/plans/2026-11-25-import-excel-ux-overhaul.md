# Import Excel UX Overhaul — Visual Diff & App Design-System Alignment Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `executing-plans` to implement this plan task-by-task using TDD.

**Goal:** Redesign the `/import` UI (Steps 0–3) so users immediately see **(1) what IRM changed in their Excel file (`~~Old~~ → New`)**, **(2) exactly what they must fix to import**, and **(3) exactly which database fields will be modified before committing**, while aligning all buttons, typography, icons, and spacing with the rest of the IRM application (Government Navy + Enterprise Structured Grid, Light & Dark mode safe).

**Architecture:** Enrich `ExcelConvertAnalysis` (in `IRM.Application` + `ExcelWorkbookConverter`) and `ImportPreviewRow` / `ImportPreviewRowDto` (in `IRM.Application` + `SqlImportStore.Preview.cs`) with structured field-level diff items; align `Import.razor`, `ImportPreparationSection.razor`, `ImportMappingSection.razor`, `ImportPreviewSection.razor`, and `import-guidance.css` with existing app CSS tokens and Material Outlined icons.

**Tech Stack:** .NET 8, Blazor Interactive Server, MudBlazor, ClosedXML, xUnit, Node.js test runner.

---

### Task 1: Structured Diff Models & Backend Computation (TDD)

**Files:**
- Modify: `IRM/IRM.Application/Features/ImportExport/Models/ExcelConvertModels.cs`
- Modify: `IRM/IRM.Infrastructure/Excel/ExcelWorkbookConverter.cs`
- Modify: `IRM/IRM.Application/Features/ImportExport/Models/ImportPreviewRow.cs`
- Modify: `IRM/IRM.Application/Features/ImportExport/Models/ImportPreviewRowDto.cs`
- Modify: `IRM/IRM.Application/Features/ImportExport/Mapping/DtoMapper.ImportExport.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Preview.cs`
- Test: `IRM/IRM.Tests/ImportVisualDiffTests.cs`

- [ ] **Step 1: Write failing tests in `IRM/IRM.Tests/ImportVisualDiffTests.cs`**
  Cover:
  1. `ExcelWorkbookConverter_Analyze_PopulatesNormalizedChangesActionIssuesAndIgnoredColumns` (verifies exact `RawValue -> NormalizedValue` entries, blocking missing-passport issue vs non-blocking date/nationality format issue with concrete Vietnamese fix instructions, and list of ignored source column names).
  2. `SqlImportStore_Preview_PopulatesDatabaseFieldChangesIncludingDestructiveClear` (verifies `Status == "changed"` rows include `FieldChanges` with `FieldLabel`, `OldValue`, `NewValue`, and `IsDestructiveClear = true` when a mapped empty cell would clear an existing DB value).

- [ ] **Step 2: Run tests to verify RED**
  Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ImportVisualDiffTests"`

- [ ] **Step 3: Implement diff models & computation**
  - Add `ExcelNormalizedDiffItem`, `ExcelActionIssueItem`, and `NormalizedChanges`, `ActionIssues`, `IgnoredColumnHeaders` to `ExcelConvertAnalysis` and populate them in `ExcelWorkbookConverter.Analyze`.
  - Add `ImportFieldDiff(string FieldLabel, string OldValue, string NewValue, bool IsDestructiveClear)` and `List<ImportFieldDiff> FieldChanges` to `ImportPreviewRow` & `ImportPreviewRowDto` (mapped in `DtoMapper.ImportExport.cs`), populated in `SqlImportStore.Preview.cs` when `Status == "changed"`.

- [ ] **Step 4: Run tests to verify GREEN & commit**
  ```powershell
  dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ImportVisualDiffTests"
  git add IRM/IRM.Application/Features/ImportExport/Models/ExcelConvertModels.cs IRM/IRM.Infrastructure/Excel/ExcelWorkbookConverter.cs IRM/IRM.Application/Features/ImportExport/Models/ImportPreviewRow.cs IRM/IRM.Application/Features/ImportExport/Models/ImportPreviewRowDto.cs IRM/IRM.Application/Features/ImportExport/Mapping/DtoMapper.ImportExport.cs IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Preview.cs IRM/IRM.Tests/ImportVisualDiffTests.cs
  git commit -m "feat(import-diff): add structured Excel normalization diffs, action issues, and DB field-level change diffs"
  ```

---

### Task 2: UI Overhaul & Design-System Alignment Across Steps 0–3 (TDD)

**Files:**
- Modify: `IRM/IRM/wwwroot/css/import-guidance.css` (add theme-safe `.irm-diff-old`, `.irm-diff-new`, `.irm-diff-clear`, `.irm-import-kpi-grid` styles)
- Modify: `IRM/IRM/Features/ImportExport/Import.razor` (use `.section-header` + consistent `Size.Small` outlined/filled buttons)
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportPreparationSection.razor` (fix all 3 stale `"chín trường bắt buộc"` → `"1 trường bắt buộc duy nhất: Số hộ chiếu"`, align typography & buttons)
- Modify: `IRM/IRM/Features/ImportExport/Import.razor.cs` (auto-check field when user picks a column in 1-click mapping; auto-uncheck when cleared)
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportMappingSection.razor` (4-card KPI summary, ignored-column chips, 1-click `MudSelect` mapping, 3-tab visual diff inspector `Đã tự sửa` / `Cần bạn sửa` / `Mẫu file sạch` with inline `~~Gốc~~ → Mới`, consistent Material icons & buttons at top and bottom, preserving existing `data-convert-*` and `data-import-*` tour/test hooks)
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportPreviewSection.razor` (4-card DB Impact KPI summary, status filter chips, inline `FieldChanges` diff `~~Cũ trong DB~~ → Mới từ Excel` with destructive-clear warning)
- Test: `IRM/IRM.Tests/ImportVisualDiffTests.cs`

- [ ] **Step 1: Add failing UI/markup tests to `IRM/IRM.Tests/ImportVisualDiffTests.cs`**
  Verify:
  1. `ImportPreparationSection.razor` does NOT contain `"chín trường bắt buộc"` or `"Chín cột bắt buộc"`.
  2. `Import.razor` uses `class="section-header"` consistent with the rest of the app.
  3. `ImportMappingSection.razor` enables 1-click column selection (no `Disabled="@(!item.IsSelected)"` blocking column pick), renders the 3-tab visual diff (`NormalizedChanges`, `ActionIssues`), and lists `IgnoredColumnHeaders`.
  4. `ImportPreviewSection.razor` renders `FieldChanges` DB diff (`OldValue` → `NewValue`) and quick status filter chips.

- [ ] **Step 2: Run tests to verify RED**

- [ ] **Step 3: Implement all Task 2 UI & CSS updates**

- [ ] **Step 4: Run targeted `.NET` + Node tests to verify GREEN & commit**
  ```powershell
  dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~Import"
  node --test IRM.Tests/Browser/*.test.cjs
  git add IRM/IRM/wwwroot/css/import-guidance.css IRM/IRM/Features/ImportExport/Import.razor IRM/IRM/Features/ImportExport/Import.razor.cs IRM/IRM/Features/ImportExport/Components/ImportPreparationSection.razor IRM/IRM/Features/ImportExport/Components/ImportMappingSection.razor IRM/IRM/Features/ImportExport/Components/ImportPreviewSection.razor IRM/IRM.Tests/ImportVisualDiffTests.cs
  git commit -m "feat(import-ui): redesign Import & Excel converter with visual before/after diffs, DB change preview, and unified app design system"
  ```

---

### Task 3: Full Regression Verification & Docs Sync

**Files:**
- Modify: `IRM/docs/DAC-TA-CHUC-NANG-HE-THONG-IRM.md` (sync Step 2/3 visual diff & test count)
- Commit: `IRM/docs/superpowers/plans/2026-11-25-import-excel-ux-overhaul.md`

- [ ] **Step 1: Run full Release build, full `.NET` test suite, and full Node test suite**
  ```powershell
  dotnet build IRM.sln -c Release
  dotnet test IRM.Tests/IRM.Tests.csproj -c Release --no-build
  node --test IRM.Tests/Browser/*.test.cjs
  ```
- [ ] **Step 2: Commit docs, update `walkthrough.md`, and start local dev server on `http://localhost:5023` for visual review**
