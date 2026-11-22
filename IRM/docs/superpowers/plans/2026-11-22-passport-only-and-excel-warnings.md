# Passport-Only Required Fields & Excel Import Warnings Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Relax ForeignPerson / Employee / Excel Import validation so only **Passport (`Số hộ chiếu`)** is required across UI, Application, Store, and Database layers, while surfacing comprehensive preview warnings and fixing all downstream null-field defects for Customer 2 (standalone Web IRM).

**Architecture:** Isolate all Customer 2 work on a new Git branch cut from `v1.0.2-r6` so Customer 1's `main` baseline remains untouched. Make `IDCompany` nullable via a new EF Core migration (`MakeEmployeeCompanyNullable`), introduce a multi-error/warning row parser (`ParseRowWithWarnings`) and preview-time V0.1.0 validator, align Blazor form validation + preview UI to surface all warnings/errors, and guard every downstream consumer (Dashboard, Search, Export, LegacySync) against empty names and null companies.

**Tech Stack:** .NET 8, ASP.NET Core Blazor Server, MudBlazor, EF Core 8 (SQL Server production / SQLite tests), ClosedXML, xUnit, bUnit, Node test runner.

**Spec:**
- `C:\Users\Windows\.gemini\antigravity\brain\26c5467c-29f2-432b-a00f-e9a60615bb5e\database_and_forms_audit_report.md`
- `C:\Users\Windows\.gemini\antigravity\brain\26c5467c-29f2-432b-a00f-e9a60615bb5e\api_flow_and_passport_only_audit.md`

## Global Constraints

- Customer 2 uses **standalone Web IRM** (no WPF `ReportManager` backward compatibility required).
- Customer 1's `main` branch and `v1.0.2-r6` tag must NOT be modified; all changes happen on a dedicated branch cut from `v1.0.2-r6` (default `custom/kh2` unless user specifies another name).
- Keep `IRM/IRM.csproj` path, assembly name, and `IRM.exe` stable for installers; maintain strict 4-layer boundaries (`Core` ← `Application` ← `Infrastructure` ← `Web`, materialized stores only).
- **Required field rule for ForeignPerson / Employee / Excel Import:** **ONLY `Passport` (`Số hộ chiếu`) is mandatory.** All other fields (`StaffName`/`FullName`, `IDCompany`, `Gender`, `Birthday`, `Nationality`, `Address`, `IDCareer`, `VisaNumber`, `TemporaryStay`, V0.1.0 extensions) are optional — blank values succeed with clear Vietnamese warnings; non-blank invalid values still produce clear format errors.
- Secondary catalog/admin forms (`Companies` name, `Accommodations` name/address, `Accounts` username/name/password, etc.) keep their own minimal required fields, with UI `*` aligned to actual validation.
- Do not commit runtime databases, installer binaries, credentials, or operational data.

## Review Focus

- **Excel row with ONLY `Passport` filled and NO company/career group selected:** Must advance past Step 1, preview as `"new"` with warnings listing missing optional fields, and commit cleanly with `IDCompany = null` without FK violation.
- **Excel update row where `IDCompany` is `null` on both existing DB row and incoming Excel row:** Must match as duplicate/changed rather than inserting a duplicate row (avoid SQL `NULL = NULL` mismatch).
- **Excel row with 3+ invalid optional values AND invalid V0.1.0 codes (`StayPurposeCode`, `ValidTo < ValidFrom`):** Must surface ALL errors and warnings together at Step 3 (Preview), never silently pass Preview and fail/rollback at Step 4.
- **Excel row with silent-coercion inputs (`Gender = "Male"`, `WorkPermit = "Chưa có"`, `FamilyVisit = "true"`):** Must emit explicit Vietnamese warnings in Preview explaining how the value was interpreted (or reject ambiguous text) instead of silently saving wrong data.
- **Passport-only record (`StaffName = ""`, `IDCompany = null`, `Nationality = null`) viewed in Dashboard, Global Search, Spotlight, Reports, and Excel Export:** Must appear in Dashboard `NationalityStats` under `"Chưa rõ"`, display `"Chưa có tên (<Passport>)"` in Search/Spotlight/Reports, and export without fragmenting `"🏢 Khác"` group headers.

---

## File Structure

| Area | Files | Responsibility |
|---|---|---|
| **DB & Schema** | `IRM.Core/Entities/Employees/Employee.cs`, `ArchivedEmployee.cs`, `IRM.Core/Entities/ImportExport/ImportHistory.cs`, `IRM.Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs`, `IRM.Infrastructure/Persistence/Migrations/*MakeEmployeeCompanyNullable*.cs` | Nullable `IDCompany` / `CompanyId`, optional FK `SetNull` |
| **DTOs & Mappers** | `IRM.Application/Features/Employees/Models/EmployeeDto.cs`, `ArchivedEmployeeDto.cs`, `ImportHistoryDto.cs`, `DtoMapper.Employees.cs`, `DtoMapper.ImportExport.cs`, `IRM.Core/Rules/PersonDisplayHelper.cs` | Nullable company mapping + shared fallback name formatter |
| **Import Rules** | `IRM.Application/Features/ImportExport/ImportMapping.cs`, `LegacyImportRules.cs`, `Models/ParseRowResult.cs`, `ImportFieldHelpCatalog.cs` | Passport-only required rule, multi-error + warning collection, coercion warnings |
| **Import Stores** | `IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Preview.cs`, `Execution.cs`, `Comparison.cs`, `ExtensionHistory.cs`, `Rollback.cs`, `SqlEmployeeStore.cs` | Null-company matching, preview-time V0.1.0 validation, overwrite warnings |
| **ForeignPerson & Manual UI** | `IRM.Application/Features/ForeignPersons/FamilyVisitorApplicationService.cs`, `SqlFamilyVisitorStore.Unified.cs`, `SqlFamilyVisitorStore.cs`, `EmployeeApplicationService.cs`, `ForeignPersons.razor`, `Import.razor`, `Import.razor.cs` | Passport-only save in Unified 5-Tab modal & Manual Entry |
| **Import Wizard UI** | `ImportUploadSection.razor`, `ImportMappingSection.razor`, `ImportPreviewSection.razor`, `ImportResultSection.razor` | Unblock Step 1 without CareerGroup, dynamic Warning/Error summary banner, expandable per-row warnings/errors, remove `.Take(5)` cap |
| **Downstream & Secondary UI** | `SqlDashboardStore.cs`, `SqlExportStore.cs`, `LegacySyncService.cs`, `Search.razor`, `SpotlightSearch.razor`, `Home.razor`, `Reports.razor`, `Companies.razor`, `Inspections.razor` | `"Chưa rõ"` stats, grouped export fix, empty-name fallback, secondary form `*` alignment |

---

### Task 1: Isolate Customer 2 Branch & Make `Employee.IDCompany` Nullable (Schema Migration)

**Files:**
- Modify: `IRM/IRM.Core/Entities/Employees/Employee.cs:25-32`
- Modify: `IRM/IRM.Core/Entities/Employees/ArchivedEmployee.cs:28-34`
- Modify: `IRM/IRM.Core/Entities/ImportExport/ImportHistory.cs:10-16`
- Modify: `IRM/IRM.Infrastructure/Persistence/Configurations/EmployeeConfiguration.cs:15-24`
- Modify: `IRM/IRM.Application/Features/Employees/Models/EmployeeDto.cs:24-30`
- Modify: `IRM/IRM.Application/Features/Employees/Models/ArchivedEmployeeDto.cs:26-32`
- Modify: `IRM/IRM.Application/Features/ImportExport/Models/ImportHistoryDto.cs`
- Modify: `IRM/IRM.Application/Features/Employees/Mapping/DtoMapper.Employees.cs`
- Modify: `IRM/IRM.Application/Features/ImportExport/Mapping/DtoMapper.ImportExport.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/Employees/SqlEmployeeStore.cs:190-210`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Rollback.cs:60-95`
- Create: `IRM/IRM.Infrastructure/Persistence/Migrations/<timestamp>_MakeEmployeeCompanyNullable.cs` (via `dotnet ef`)
- Test: `IRM/IRM.Tests/PassportOnlySchemaTests.cs`

**Interfaces:**
- Consumes: Existing `IrmDbContext`, `TestDatabase` helper in `IRM/IRM.Tests/TestDatabase.cs`.
- Produces:
  - `Employee.IDCompany` (`int?`), `ArchivedEmployee.IDCompany` (`int?`), `ImportHistory.CompanyId` (`int?`)
  - `EmployeeDto.IDCompany` (`int?`), `ArchivedEmployeeDto.IDCompany` (`int?`), `ImportHistoryDto.CompanyId` (`int?`)
  - EF Core migration `MakeEmployeeCompanyNullable`

- [ ] **Step 1: Create Customer 2 branch from `v1.0.2-r6`**

Run: `git checkout -b custom/kh2 v1.0.2-r6` (replace `custom/kh2` if user specifies another name).
Expected: `Switched to a new branch 'custom/kh2'`

- [ ] **Step 2: Write the failing test in `IRM/IRM.Tests/PassportOnlySchemaTests.cs`**

```csharp
public sealed class PassportOnlySchemaTests
{
    [Fact]
    public async Task Employee_CanBeSavedWithOnlyPassportAndNullCompany()
    {
        await using var db = TestDatabase.CreateSqlite();
        var emp = new Employee
        {
            StaffName = "",
            Passport = "P99887766",
            IDCompany = null,
            IDUser = 1,
            Hidden_flag = 0,
            WorkingStatus = 0
        };
        db.Employees.Add(emp);
        await db.SaveChangesAsync();

        var loaded = await db.Employees.Include(e => e.Company).SingleAsync(e => e.Passport == "P99887766");
        Assert.Null(loaded.IDCompany);
        Assert.Null(loaded.Company);
    }
}
```

- [ ] **Step 3: Run test to verify it fails**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~PassportOnlySchemaTests"`
Expected: FAIL to compile (`Cannot convert null to 'int'` on `IDCompany = null`).

- [ ] **Step 4: Update entities, DTOs, mappers, `EmployeeConfiguration`, and rollback/archive stores**

Change `IDCompany` / `CompanyId` to `int?` in the listed entity/DTO/mapper files. In `EmployeeConfiguration.cs`, set `.IsRequired(false).OnDelete(DeleteBehavior.SetNull)`. In `SqlImportStore.Rollback.cs`, collect `affected` using `if (emp.IDCompany.HasValue) affected.Add(emp.IDCompany.Value);`. Generate migration from `IRM/`:
```powershell
dotnet ef migrations add MakeEmployeeCompanyNullable --project IRM.Infrastructure --startup-project IRM
```

- [ ] **Step 5: Run tests and verify build**

Run (from `IRM/`):
```powershell
dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~PassportOnlySchemaTests"
dotnet build IRM.sln -c Release
```
Expected: `PassportOnlySchemaTests` PASS; solution builds with 0 errors.

- [ ] **Step 6: Commit**

```powershell
git add IRM/IRM.Core IRM/IRM.Application IRM/IRM.Infrastructure IRM/IRM.Tests/PassportOnlySchemaTests.cs
git commit -m "feat(db): make Employee.IDCompany and ImportHistory.CompanyId nullable for passport-only records"
```

---

### Task 2: Passport-Only Import Parsing with Multi-Error & Coercion Warnings (`ImportMapping`)

**Files:**
- Create: `IRM/IRM.Application/Features/ImportExport/Models/ParseRowResult.cs`
- Modify: `IRM/IRM.Application/Features/ImportExport/ImportMapping.cs:8-215`
- Modify: `IRM/IRM.Application/Features/ImportExport/LegacyImportRules.cs:75-105`
- Modify: `IRM/IRM.Application/Features/ImportExport/ImportFieldHelpCatalog.cs:20-115`
- Test: `IRM/IRM.Tests/PassportOnlyImportMappingTests.cs`

**Interfaces:**
- Consumes: `Employee.IDCompany` (`int?`) from Task 1.
- Produces:
  - `public sealed record ParseRowResult(Employee? Employee, List<string> Errors, List<string> Warnings);`
  - `ImportMapping.RequiredCoreFields` containing only `["Passport"]`
  - `ImportMapping.ParseRowWithWarnings(Dictionary<string, string> rowData, int? companyId, List<NationalityEntity> nationalities, List<Career> careers, int? careerGroupId, ISet<string> investorPassports, IReadOnlyCollection<string> wards) -> ParseRowResult`

- [ ] **Step 1: Write failing tests in `IRM/IRM.Tests/PassportOnlyImportMappingTests.cs`**

Cover 4 cases:
1. `ParseRowWithWarnings_OnlyPassportFilled_SucceedsWithMissingFieldWarnings` — `Errors` empty, `Employee.Passport == "P123456"`, `Warnings` contains missing-field notices (`"Thiếu họ tên"`, `"Thiếu ngày sinh"`, `"Thiếu quốc tịch"`, `"Thiếu địa chỉ"`, `"Thiếu nghề nghiệp"`, `"Thiếu số visa"`, `"Thiếu hạn tạm trú"`).
2. `ParseRowWithWarnings_MissingPassport_ReturnsPassportRequiredError` — `Employee` is null, `Errors` contains `"Thiếu số hộ chiếu"`.
3. `ParseRowWithWarnings_MultipleInvalidOptionalFields_CollectsAllErrorsInOnePass` — bad `Birthday`, unknown `Nationality`, bad `TemporaryStay` all reported together (`Errors.Count >= 3`).
4. `ParseRowWithWarnings_AmbiguousGenderWorkPermitFamilyVisit_EmitsCoercionWarnings` — `Gender = "Male"`, `WorkPermit = "Chưa có"`, `FamilyVisit = "true"` each produce a clear Vietnamese warning in `Warnings`.

- [ ] **Step 2: Run test to verify it fails**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~PassportOnlyImportMappingTests"`
Expected: FAIL (`ParseRowWithWarnings` / `ParseRowResult` not defined).

- [ ] **Step 3: Implement `ParseRowResult` and `ImportMapping.ParseRowWithWarnings`**

- Set `RequiredCoreFields = ["Passport"]` and update `ImportFieldHelpCatalog` so only `Passport` has `IsRequired = true`.
- In `ParseRowWithWarnings`:
  - Require only `Passport`; if blank, add error and return `ParseRowResult(null, errors, warnings)`.
  - For every optional field (`StaffName`, `Gender`, `Birthday`, `Nationality`, `Address`, `Career`, `VisaNumber`, `TemporaryStay`): when blank, append a Vietnamese warning and leave nullable/empty; when non-blank, validate format/catalog and append any failure to `errors` (without throwing early).
  - Remove hard ward-suffix rejection on `Address`: if `wards.Count > 0 && !AddressEndsWithWard(address, wards)`, append a warning instead of an error.
  - For `Career`: if `careerGroupId is > 0`, search that group first then fall back to all active careers (with a warning if matched in another group).
  - Add coercion warnings for `Gender` (non-empty value other than `"Nam"`/`"Nữ"`/`"Nu"`), `WorkPermit` (text like `"Chưa có"`/`"Đã có"`), and `FamilyVisit` (non-empty unrecognized value).
  - Keep existing `ParseRowToEmployee(...)` delegating to `ParseRowWithWarnings(...)` (throwing `ImportRowException(string.Join("; ", result.Errors))` if `Errors.Count > 0`) for any legacy callers/tests.

- [ ] **Step 4: Run tests to verify they pass**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~PassportOnlyImportMappingTests"`
Expected: PASS (4 passed, 0 failed).

- [ ] **Step 5: Commit**

```powershell
git add IRM/IRM.Application/Features/ImportExport IRM/IRM.Tests/PassportOnlyImportMappingTests.cs
git commit -m "feat(import): require only Passport and collect multi-errors plus coercion warnings"
```

---

### Task 3: Preview-Time V0.1.0 Validation, Overwrite Warnings & Null-Company Matching (`SqlImportStore`)

**Files:**
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Preview.cs:24-182`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Execution.cs:12-180`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.ExtensionHistory.cs:21-186`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Comparison.cs:10-70`
- Test: `IRM/IRM.Tests/PassportOnlyImportStoreTests.cs`

**Interfaces:**
- Consumes: `ImportMapping.ParseRowWithWarnings` (Task 2), nullable `Employee.IDCompany` & `ImportHistory.CompanyId` (Task 1).
- Produces:
  - `SqlImportStore.ValidateV010Extensions(Dictionary<string, string> extendedFields, IReadOnlySet<string> activeAdminUnitCodes) -> (List<string> Errors, List<string> Warnings)`
  - Preview and Execute support `companyId == null` and `careerGroupId == 0`, populate `preview.Warnings` for missing optional fields / partial V0.1.0 columns / overwrite-with-blank updates, and surface V0.1.0 errors in Step 3 (`preview.Status = "error"`).

- [ ] **Step 1: Write failing tests in `IRM/IRM.Tests/PassportOnlyImportStoreTests.cs`**

Cover 4 cases:
1. `PreviewAndExecute_OnlyPassportNoCompanyNoCareerGroup_CommitsWithNullCompanyAndWarnings` — preview returns `Status == "new"` with warnings; execute inserts `Employee` with `IDCompany == null` and `ImportHistory.CompanyId == null`.
2. `Preview_SecondImportWithNullCompany_MatchesExistingPassportAsUnchangedOrChanged` — does not insert duplicate when both DB and Excel have `IDCompany == null`.
3. `Preview_InvalidV010PurposeCodeOrInvertedDates_FlagsErrorInPreviewBeforeExecute` — `StayPurposeCode = "INVALID"` or `StayValidTo < StayValidFrom` sets `preview.Status == "error"` at Step 3; partial `ResidenceAddress` without `AdministrativeUnitCode` adds a warning in `preview.Warnings`.
4. `Preview_UpdateBlankNoteOrSettlementOverExisting_EmitsOverwriteWarning` — warns when an update row would clear existing non-empty `Note` or `SettlementResultsString`.

- [ ] **Step 2: Run test to verify it fails**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~PassportOnlyImportStoreTests"`
Expected: FAIL.

- [ ] **Step 3: Implement preview validation, null-company matching, and safe execution in `SqlImportStore.*`**

- In `SqlImportStore.Preview.cs`:
  - Allow `resolvedCompanyId = null` when no company is provided (append `"Chưa gán công ty"` to `preview.Warnings` instead of setting error); remove hard error when `careerGroupId <= 0`.
  - Call `ImportMapping.ParseRowWithWarnings(...)`; attach all `Warnings` to `preview.Warnings` and join all `Errors` into `preview.ErrorMessage` (newline-separated).
  - Run `ValidateV010Extensions(...)` on `preview.ExtendedFields` during Preview, appending errors/warnings directly to the preview row.
  - Fix existing-record lookup for nullable `IDCompany`: match `e.Passport == passport && e.Hidden_flag == 0 && e.IDCompany == resolvedCompanyId`; guard other-company warning with `e.IDCompany.HasValue && resolvedCompanyId.HasValue && e.IDCompany != resolvedCompanyId`.
  - When status is `"changed"`, emit overwrite warning if blank/unmapped `Note` or `SettlementResults` would overwrite a non-empty existing value.
- In `SqlImportStore.Execution.cs`:
  - Allow `targetId == null`; only validate `Companies.AnyAsync` and call `RecomputeCompanyCountersAsync` when `targetId.HasValue && targetId.Value > 0`.
  - Set `ImportHistory.CompanyId = companyId > 0 ? companyId : null`.
- In `SqlImportStore.ExtensionHistory.cs`:
  - Extract `ValidateV010Extensions(...)` shared by Preview and Execution.
  - Replace `SingleOrDefaultAsync` at L35 with `FirstOrDefaultAsync` and guard `(employee.StaffName ?? "").Trim()`.

- [ ] **Step 4: Run tests to verify they pass**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~PassportOnlyImportStoreTests"`
Expected: PASS (4 passed, 0 failed).

- [ ] **Step 5: Commit**

```powershell
git add IRM/IRM.Infrastructure/Persistence/Stores/ImportExport IRM/IRM.Tests/PassportOnlyImportStoreTests.cs
git commit -m "feat(import): validate V0.1.0 in preview, emit overwrite warnings, and support null company"
```

---

### Task 4: Passport-Only Validation for Unified ForeignPerson & Manual Entry

**Files:**
- Modify: `IRM/IRM.Application/Features/ForeignPersons/FamilyVisitorApplicationService.cs:65-95`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ForeignPersons/SqlFamilyVisitorStore.Unified.cs:300-460`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ForeignPersons/SqlFamilyVisitorStore.cs:245-270`
- Modify: `IRM/IRM.Application/Features/Employees/EmployeeApplicationService.cs:35-55`
- Modify: `IRM/IRM/Features/ForeignPersons/ForeignPersons.razor:355-606,825-920`
- Modify: `IRM/IRM/Features/ImportExport/Import.razor:103-208`
- Modify: `IRM/IRM/Features/ImportExport/Import.razor.cs:70-90,385-425`
- Test: `IRM/IRM.Tests/PassportOnlyManualEntryTests.cs`

**Interfaces:**
- Consumes: Nullable `Employee.IDCompany` (Task 1).
- Produces:
  - `FamilyVisitorApplicationService.SaveUnifiedProfileAsync`: requires only `PassportNumber` (non-whitespace); computes `IsDataIncomplete = string.IsNullOrWhiteSpace(FullName) || passportKey is null`.
  - `EmployeeApplicationService.CreateAsync` / `UpdateAsync`: requires only `Passport` (non-whitespace).
  - UI forms `ForeignPersons.razor` and `Import.razor` (Manual): mark ONLY `PassportNumber` / `Passport` with `*` + `Required="true"` + inline `Error`; show non-blocking info note for optional blank fields.

- [ ] **Step 1: Write failing tests in `IRM/IRM.Tests/PassportOnlyManualEntryTests.cs`**

Cover 3 cases:
1. `SaveUnifiedProfileAsync_OnlyPassportFilled_SucceedsAndMarksDataIncomplete` — blank `FullName`, null `CompanyId`, `HasWorkProfile = false` saves cleanly and sets `IsDataIncomplete = true`.
2. `SaveUnifiedProfileAsync_BlankPassport_ThrowsArgumentException` — rejects when `PassportNumber` is blank even if `FullName` is filled.
3. `EmployeeApplicationService_CreateAsync_OnlyPassportFilled_SucceedsAndBlankPassportThrows` — saves with `Passport = "P554433"`, `StaffName = ""`, `IDCompany = null`; throws `ArgumentException` when `Passport` is blank.

- [ ] **Step 2: Run test to verify it fails**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~PassportOnlyManualEntryTests"`
Expected: FAIL.

- [ ] **Step 3: Update Application Services, Stores, and Blazor UI Forms**

- In `FamilyVisitorApplicationService.cs` & `SqlFamilyVisitorStore.Unified.cs`:
  - Replace `FullName` required check with `if (string.IsNullOrWhiteSpace(input.PassportNumber)) throw new ArgumentException("Số hộ chiếu là bắt buộc.");`.
  - Normalize `FullName = (input.FullName ?? "").Trim()`; set `IsDataIncomplete = string.IsNullOrWhiteSpace(person.FullName) || passportKey is null`.
  - If `HasWorkProfile` is true and `CompanyId` is null, allow saving `Employee` with `IDCompany = null` (remove `.Value` crash at L446 and hard `CompanyId`/`CareerId` throws).
  - If `HasFamilyVisitProfile` is true, allow blank `RelativeName`/`Relationship` (store `""`).
- In `EmployeeApplicationService.cs`: validate `!string.IsNullOrWhiteSpace(dto.Passport)` in `CreateAsync` and `UpdateAsync`.
- In `ForeignPersons.razor`:
  - Move `*` + `Required="true"` + inline `Error` to `PassportNumber`; remove `*` from `FullName`, `NationalityCode`, `CompanyId`, `RelativeName`, `Relationship`.
  - Validate `PassportNumber` in `SaveUnifiedProfileAsync()` before calling `Registry.SaveUnifiedProfileAsync(_form)`.
- In `Import.razor` & `Import.razor.cs`:
  - Update `GetMissingManualEntryFields()` to require ONLY `Passport`; remove `*` and `Required="true"` from the other 9 fields.

- [ ] **Step 4: Run tests to verify they pass**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~PassportOnlyManualEntryTests"`
Expected: PASS (3 passed, 0 failed).

- [ ] **Step 5: Commit**

```powershell
git add IRM/IRM.Application/Features/ForeignPersons IRM/IRM.Application/Features/Employees IRM/IRM.Infrastructure/Persistence/Stores/ForeignPersons IRM/IRM/Features/ForeignPersons IRM/IRM/Features/ImportExport/Import.razor IRM/IRM/Features/ImportExport/Import.razor.cs IRM/IRM.Tests/PassportOnlyManualEntryTests.cs
git commit -m "feat(forms): enforce passport-only required rule on unified profile and manual entry"
```

---

### Task 5: Upgrade Excel Import Wizard UI (Unblock Step 1 + Prominent Warnings + Full Error List)

**Files:**
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportUploadSection.razor:70-118`
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportMappingSection.razor:10-40`
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportPreviewSection.razor:5-113`
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportResultSection.razor:35-65`
- Test: `IRM/IRM.Tests/ImportPreviewRenderingTests.cs`

**Interfaces:**
- Consumes: `ImportPreviewRowDto.Warnings` and multi-line `ErrorMessage` populated in Task 3.
- Produces:
  - Step 1 "Tiếp tục" enabled as soon as `_fileStream != null` (`CareerGroupId` optional).
  - Step 3 dynamic summary alert (`Severity.Error` / `Severity.Warning` / `Severity.Success`) with explicit warning row count + per-row warning chips/list + full multi-error rendering.
  - Step 4 full error list with toggle (no silent `.Take(5)` cutoff).

- [ ] **Step 1: Add failing bUnit tests in `IRM/IRM.Tests/ImportPreviewRenderingTests.cs`**

Add tests verifying:
1. `ImportPreviewSection_WithWarnings_RendersWarningSeverityBannerAndWarningCount` — summary banner uses warning styling and displays warning count + row warning text.
2. `ImportUploadSection_WithFileAndNoCareerGroup_EnablesContinueButton` — Next button is NOT disabled when `_careerGroupId == 0`.
3. `ImportResultSection_WithMoreThanFiveErrors_CanDisplayAllErrors` — all error items are accessible (no hard 5-item truncation).

- [ ] **Step 2: Run test to verify it fails**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ImportPreviewRenderingTests"`
Expected: FAIL.

- [ ] **Step 3: Update the 4 Import Wizard Razor components**

- `ImportUploadSection.razor`: Remove `*`/`Required` from `CareerGroupId` and change Next button to `Disabled="@(Page._fileStream == null)"`.
- `ImportMappingSection.razor`: Highlight only `Passport` as `* (Bắt buộc)`; keep optional fields clearly labeled.
- `ImportPreviewSection.razor`: Compute `warningRowCount = Page._previewRows.Count(r => r.Warnings.Count > 0)` and `errorRowCount`; pick banner `Severity` dynamically (`Error` > `Warning` > `Success`); show warning count badge and render each row's `Warnings` with a warning icon/alert chip; render multi-line `ErrorMessage` cleanly.
- `ImportResultSection.razor`: Add `_showAllErrors` toggle (defaulting to showing first 10 with a button `"Xem tất cả N lỗi"` so nothing is lost).

- [ ] **Step 4: Run tests to verify they pass**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~ImportPreviewRenderingTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add IRM/IRM/Features/ImportExport/Components IRM/IRM.Tests/ImportPreviewRenderingTests.cs
git commit -m "feat(import-ui): unblock Step 1 without career group and surface preview warnings and full error list"
```

---

### Task 6: Downstream Consumers Safety & Display Fallbacks (Dashboard, Export, LegacySync, Search/Reports UI)

**Files:**
- Create: `IRM/IRM.Core/Rules/PersonDisplayHelper.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/Reports/SqlDashboardStore.cs:65-80`
- Modify: `IRM/IRM.Infrastructure/Excel/SqlExportStore.cs:74-88,158-240`
- Modify: `IRM/IRM.Infrastructure/LegacySync/LegacySyncService.cs:22-35`
- Modify: `IRM/IRM/Features/Reports/Search.razor:85-98`
- Modify: `IRM/IRM/Components/Shared/SpotlightSearch.razor:275-308`
- Modify: `IRM/IRM/Features/Reports/Home.razor:258-270`
- Modify: `IRM/IRM/Features/Reports/Reports.razor:200-215`
- Modify: `IRM/IRM/Features/ForeignPersons/ForeignPersons.razor:220-235`
- Modify: `IRM/IRM/Features/Companies/Companies.razor:468-478`
- Test: `IRM/IRM.Tests/PassportOnlyDownstreamTests.cs`

**Interfaces:**
- Consumes: Nullable `Employee.IDCompany` (Task 1).
- Produces:
  - `PersonDisplayHelper.FormatName(string? fullName, string? passport) -> string` (returns `fullName.Trim()` if non-empty, else `$"Chưa có tên ({passport.Trim()})"` if passport present, else `"Chưa có tên"`).

- [ ] **Step 1: Write failing tests in `IRM/IRM.Tests/PassportOnlyDownstreamTests.cs`**

Cover 4 cases:
1. `PersonDisplayHelper_FormatName_ReturnsFallbackWithPassportWhenNameBlank`.
2. `DashboardStore_NationalityStats_IncludesNullNationalityUnderChuaRo` — passport-only employee with `Nationality == null` is counted under `"Chưa rõ"` so sum matches `TotalEmployees`.
3. `ExportStore_ExportReportGroupedByNationality_DoesNotDuplicateKhacHeaders` — sorting matches `groupBy`, producing a single `"Chưa rõ"` / `"Khác"` header; `ExportEmployeesAsync` excludes `Hidden_flag == 1`.
4. `LegacySyncService_EmptyStaffName_UsesFallbackInsteadOfThrowing`.

- [ ] **Step 2: Run test to verify it fails**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~PassportOnlyDownstreamTests"`
Expected: FAIL.

- [ ] **Step 3: Implement `PersonDisplayHelper` and fix Dashboard, Export, LegacySync, and Razor display sites**

- Implement `PersonDisplayHelper.FormatName`.
- In `SqlDashboardStore.cs`: group active employees using a left-join / dictionary lookup against `_db.Nationality`, mapping `null` or unmatched codes to `"Chưa rõ"`.
- In `SqlExportStore.cs`: add `Where(e => e.Hidden_flag == 0)` in `ExportEmployeesAsync`; in `ExportReportAsync`, sort by the active `groupBy` selector (`nationality`, `field`, `company`) before iterating group headers.
- In `LegacySyncService.cs:28`: replace `Value(xml, nameField) ?? "Chưa xác định"` with `string.IsNullOrWhiteSpace(rawName) ? "Chưa xác định" : rawName`.
- Use `PersonDisplayHelper.FormatName(...)` in `Search.razor`, `SpotlightSearch.razor`, `Home.razor`, `Reports.razor`, `ForeignPersons.razor`, and `Companies.razor`, and hide empty middle-dot separators in `Search.razor:90-94`.

- [ ] **Step 4: Run tests to verify they pass**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~PassportOnlyDownstreamTests"`
Expected: PASS (4 passed, 0 failed).

- [ ] **Step 5: Commit**

```powershell
git add IRM/IRM.Core/Rules/PersonDisplayHelper.cs IRM/IRM.Infrastructure/Persistence/Stores/Reports IRM/IRM.Infrastructure/Excel/SqlExportStore.cs IRM/IRM.Infrastructure/LegacySync/LegacySyncService.cs IRM/IRM/Features IRM/IRM/Components/Shared/SpotlightSearch.razor IRM/IRM.Tests/PassportOnlyDownstreamTests.cs
git commit -m "fix(downstream): handle passport-only records in dashboard, export, legacy sync, and UI displays"
```

---

### Task 7: Align Secondary Forms Validation & Run Full Regression Suite

**Files:**
- Modify: `IRM/IRM/Features/Companies/Companies.razor:380-432,488-505` (add `*` + inline error indicators for Site `Name`/`AddressLine`, Zone `Code`/`Name`, Representative `FullName`)
- Modify: `IRM/IRM/Features/Inspections/Inspections.razor:258-316,440-459` (validate `ViolationDetails` when `ResultCode` is `VIOLATION`/`VERIFY` to match `*`)
- Modify: `IRM/IRM.Application/Features/Inspections/InspectionApplicationService.cs:34-41` (validate `FullName` and conditional `ViolationDetails`)
- Modify: Existing tests in `IRM/IRM.Tests/LegacyImport*.cs` or `ImportExportFeatureTddTests.cs` if any still assert old 9-field WPF hard rejection (update expectations cleanly to match Customer 2's Passport-only spec).

**Interfaces:**
- Consumes: All changes from Tasks 1–6.
- Produces: Fully green build and test suite on Customer 2 branch.

- [ ] **Step 1: Align `Companies.razor` and `Inspections.razor` / `InspectionApplicationService.cs` validation**

Add matching `*` indicators and inline validation for Site (`Name`, `AddressLine`), Economic Zone (`Code`, `Name`), Representative (`FullName`), and Inspection Subject (`ViolationDetails` when `ResultCode` is `VIOLATION` or `VERIFY`).

- [ ] **Step 2: Run full `.NET`, EF migrations, and Node test suites**

Run (from `IRM/`):
```powershell
dotnet ef migrations list --project IRM.Infrastructure --startup-project IRM --no-connect
dotnet build IRM.sln -c Release
dotnet test IRM.Tests/IRM.Tests.csproj -c Release --no-build
node --test IRM.Tests/Browser/*.test.cjs
```
Expected: `MakeEmployeeCompanyNullable` listed; 0 build warnings/errors; all `.NET` and Node tests PASS (update any legacy WPF-only test assertions that explicitly tested the removed 9-field requirement, documenting the reason in commit message).

- [ ] **Step 3: Commit**

```powershell
git add IRM/IRM/Features/Companies IRM/IRM/Features/Inspections IRM/IRM.Application/Features/Inspections IRM/IRM.Tests
git commit -m "fix(forms): align secondary form validation and update tests for customer 2 passport-only rules"
```
