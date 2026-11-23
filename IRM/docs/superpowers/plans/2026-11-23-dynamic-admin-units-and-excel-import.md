# Dynamic Administrative Units & Excel Import Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `executing-plans` to implement this plan task-by-task using TDD.

**Goal:** Remove hardcoded `"54 địa bàn"` and `QN/HL/MC/CP` labels across the UI/tour, harden `/admin/admin-units` manual configuration (`dd/MM/yyyy`, search, duplicate validation), and add `.xlsx` template download + preview + safe upsert import for Administrative Units.

**Architecture:** Contracts and service interfaces in `IRM.Application`, ClosedXML helper & EF Core upsert in `IRM.Infrastructure`, Blazor UI in `IRM` (`AdminAdministrativeUnitsSection.razor`, `Admin.Catalogs.cs`, `Statistics.razor`, `NavMenu.razor`, `Home.razor`, `SpotlightSearch.razor`, `MainLayout.razor`, `app-product-tour.js`).

**Tech Stack:** .NET 8, Blazor Server, MudBlazor, ClosedXML, EF Core, xUnit, Node.js test runner.

---

### Task 1: Application Contracts, Excel Helper & Store Upsert (TDD)

**Files:**
- Create: `IRM/IRM.Application/Features/Administration/Contracts/AdministrativeUnitImportContracts.cs`
- Modify: `IRM/IRM.Application/Abstractions/Persistence/ICatalogStore.cs`
- Modify: `IRM/IRM.Application/Features/Administration/ICatalogService.cs`
- Modify: `IRM/IRM.Application/Features/Administration/CatalogApplicationService.cs`
- Create: `IRM/IRM.Infrastructure/Excel/AdministrativeUnitExcelHelper.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/Administration/SqlCatalogStore.cs`
- Test: `IRM/IRM.Tests/AdministrativeUnitExcelImportTests.cs`

- [ ] **Step 1: Write failing tests in `IRM/IRM.Tests/AdministrativeUnitExcelImportTests.cs`**
  Cover:
  1. `BuildTemplateWorkbook_ProducesValidXlsxWithExpectedHeadersAndSampleRows`
  2. `ParseWorkbook_ClassifiesCreateUpdateAndValidationErrors` (missing Code, missing Name, invalid non-`dd/MM/yyyy` date, duplicate Code in file, Vietnamese `TypeCode` mapping `Phường/Xã/Đặc khu/Tỉnh/TP`)
  3. `SqlCatalogStore_ImportAdministrativeUnitsExcel_UpsertsAndReactivatesWithoutBreakingExistingIds` (SQLite in-memory `IrmDbContext`)
  4. `SqlCatalogStore_CreateAdministrativeUnit_RejectsDuplicateCodeAndValidFromWithVietnameseMessage`

- [ ] **Step 2: Run test to verify RED**
  Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~AdministrativeUnitExcelImportTests"`

- [ ] **Step 3: Implement contracts, `AdministrativeUnitExcelHelper`, `SqlCatalogStore`, and `CatalogApplicationService`**

- [ ] **Step 4: Run test to verify GREEN**
  Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~AdministrativeUnitExcelImportTests"`

- [ ] **Step 5: Commit**
  ```powershell
  git add IRM/IRM.Application/Features/Administration/Contracts/AdministrativeUnitImportContracts.cs IRM/IRM.Application/Abstractions/Persistence/ICatalogStore.cs IRM/IRM.Application/Features/Administration/ICatalogService.cs IRM/IRM.Application/Features/Administration/CatalogApplicationService.cs IRM/IRM.Infrastructure/Excel/AdministrativeUnitExcelHelper.cs IRM/IRM.Infrastructure/Persistence/Stores/Administration/SqlCatalogStore.cs IRM/IRM.Tests/AdministrativeUnitExcelImportTests.cs
  git commit -m "feat(admin-units): add Excel template, preview parser, and safe upsert import for administrative units"
  ```

---

### Task 2: UI — Remove "54 địa bàn" Hardcoding & Wire `/admin/admin-units` Excel Import

**Files:**
- Modify: `IRM/IRM/Components/Shared/NavMenu.razor`
- Modify: `IRM/IRM/Components/Shared/MainLayout.razor`
- Modify: `IRM/IRM/Components/Shared/SpotlightSearch.razor`
- Modify: `IRM/IRM/Features/Reports/Home.razor`
- Modify: `IRM/IRM/Features/Reports/Statistics.razor`
- Modify: `IRM/IRM/wwwroot/js/app-product-tour.js`
- Modify: `IRM/IRM/Features/Administration/Admin.razor.cs`
- Modify: `IRM/IRM/Features/Administration/Admin.Catalogs.cs`
- Modify: `IRM/IRM/Features/Administration/Components/AdminAdministrativeUnitsSection.razor`
- Test: `IRM/IRM.Tests/DynamicAdminUnitsMarkupTests.cs`
- Modify (if needed): `IRM/IRM.Tests/Browser/app-product-tour.test.cjs`

- [ ] **Step 1: Write failing markup test `IRM/IRM.Tests/DynamicAdminUnitsMarkupTests.cs`**
  Verify:
  1. `NavMenu.razor`, `Home.razor`, `Statistics.razor`, `SpotlightSearch.razor`, `app-product-tour.js` do NOT contain `"54 địa bàn"` or `"54 xã"`.
  2. `MainLayout.razor` does NOT contain hardcoded `>QN<`, `>HL<`, `>MC<`, `>CP<`.
  3. `Statistics.razor` links Admin to `/admin/admin-units`.
  4. `AdminAdministrativeUnitsSection.razor` includes `DateFormat="dd/MM/yyyy"`, search input, template download button, and Excel upload/preview/import UI.

- [ ] **Step 2: Run test to verify RED**
  Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~DynamicAdminUnitsMarkupTests"`

- [ ] **Step 3: Implement all UI updates**
  - Update the 5 `"54 địa bàn"` files + `MainLayout.razor` sidebar footer bubbles.
  - Update `Admin.razor.cs`, `Admin.Catalogs.cs`, and `AdminAdministrativeUnitsSection.razor` with `DateFormat="dd/MM/yyyy"`, search box, Excel template download (`irmInterop.downloadFile`), `.xlsx` upload + preview table + confirm import button.

- [ ] **Step 4: Run tests to verify GREEN**
  Run (from `IRM/`):
  ```powershell
  dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~DynamicAdminUnitsMarkupTests"
  node --test IRM.Tests/Browser/app-product-tour.test.cjs
  ```

- [ ] **Step 5: Commit**
  ```powershell
  git add IRM/IRM/Components/Shared/NavMenu.razor IRM/IRM/Components/Shared/MainLayout.razor IRM/IRM/Components/Shared/SpotlightSearch.razor IRM/IRM/Features/Reports/Home.razor IRM/IRM/Features/Reports/Statistics.razor IRM/IRM/wwwroot/js/app-product-tour.js IRM/IRM/Features/Administration/Admin.razor.cs IRM/IRM/Features/Administration/Admin.Catalogs.cs IRM/IRM/Features/Administration/Components/AdminAdministrativeUnitsSection.razor IRM/IRM.Tests/DynamicAdminUnitsMarkupTests.cs
  git commit -m "feat(admin-units-ui): remove hardcoded 54-unit labels and add Excel import UI in /admin/admin-units"
  ```

---

### Task 3: Full Regression Verification & Docs Commit

**Files:**
- Commit: `IRM/docs/superpowers/specs/2026-11-23-dynamic-admin-units-and-excel-import-design.md`, `IRM/docs/superpowers/plans/2026-11-23-dynamic-admin-units-and-excel-import.md`

- [ ] **Step 1: Stop dev server (if running), run full Release build, full `.NET` tests, and full Node tests**
  ```powershell
  dotnet build IRM.sln -c Release
  dotnet test IRM.Tests/IRM.Tests.csproj -c Release --no-build
  node --test IRM.Tests/Browser/*.test.cjs
  ```
  Expected: `0 Warning(s), 0 Error(s)`, all `.NET` tests PASS, all 54/54 Node tests PASS.

- [ ] **Step 2: Commit docs, update `walkthrough.md`, and restart dev server on `http://localhost:5023`**
