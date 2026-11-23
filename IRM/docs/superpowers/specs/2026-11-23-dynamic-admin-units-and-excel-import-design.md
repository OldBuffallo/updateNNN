# Dynamic Administrative Units & Excel Import Design Spec

**Date:** 2026-11-23
**Branch:** `custom/kh2`

## 1. Context & Problem

1. The application previously hardcoded **"54 địa bàn"** (Quảng Ninh's 54 wards/communes/special zones from Customer 1's seed data) in 5 UI/tour locations (`NavMenu.razor`, `Home.razor`, `Statistics.razor`, `SpotlightSearch.razor`, `app-product-tour.js`), plus `QN / HL / MC / CP` abbreviations in `MainLayout.razor` sidebar footer.
2. Backend queries in `SqlStatisticsStore` are already 100% dynamic against the `AdministrativeUnits` table configured by `ADMIN` at `/admin/admin-units`.
3. However, `/admin/admin-units` (`AdminAdministrativeUnitsSection.razor`) currently lacks:
   - `DateFormat="dd/MM/yyyy"` on its `MudDatePicker`.
   - Search filter for finding units quickly.
   - Friendly Vietnamese duplicate validation on `(Code, ValidFrom)` in `SqlCatalogStore`.
   - Excel template download and `.xlsx` bulk import (with preview + safe upsert).

## 2. Approved Design

### 2.1 Remove "54 địa bàn" & Local Hardcoding (Option A)
- Replace hardcoded `"54 địa bàn"` / `"54 xã/phường"` labels with dynamic **"Thống kê địa bàn"** / **"Địa bàn hành chính"** across:
  - `IRM/Components/Shared/NavMenu.razor`
  - `IRM/Features/Reports/Home.razor`
  - `IRM/Components/Shared/SpotlightSearch.razor`
  - `IRM/Features/Reports/Statistics.razor` (display live `@_units.Count` / `@_overview.Count` + Admin-only quick button to `/admin/admin-units`)
  - `IRM/wwwroot/js/app-product-tour.js` (clarify that units are dynamically configured by Admin under `Danh mục hệ thống → Địa bàn hành chính`)
- Replace hardcoded `QN / HL / MC / CP` bubbles in `MainLayout.razor` sidebar footer with neutral system badges (`IRM`, `HS`, `DN`, `BC`) or generic iconography.
- Preserve existing `DatabaseSeeder` behavior (Customer 2 Admin can edit/deactivate or import their own Excel list on top).

### 2.2 Harden & Expand `/admin/admin-units`
1. **Manual Configuration Improvements:**
   - Set `DateFormat="dd/MM/yyyy"` on `MudDatePicker` in `AdminAdministrativeUnitsSection.razor`.
   - Add search input (filters by `Code` or `Name`, case-insensitive) alongside existing `TypeCode` filter chips.
   - In `SqlCatalogStore`: trim `Code` & `Name`, validate non-empty, and check `(Code, ValidFrom)` uniqueness before saving so users receive a clear Vietnamese error instead of raw `DbUpdateException`.
2. **Excel Template & Bulk Import (`.xlsx`):**
   - **Download Template (`Mau-Dia-Ban-Hanh-Chinh.xlsx`):** 4 columns (`Mã địa bàn`, `Tên địa bàn`, `Loại`, `Hiệu lực từ`) with clear formatting and 2 sample rows.
   - **Preview before Commit:** Upload `.xlsx` → parse with `AdministrativeUnitExcelHelper` (ClosedXML + `ExcelValueNormalizer.TryParseStrictDdMmYyyy`) → show summary counters (`Hợp lệ`, `Thêm mới`, `Cập nhật`, `Lỗi`) and row-level table with exact Vietnamese error messages (missing Code/Name, duplicate Code in file, invalid `dd/MM/yyyy` date, unrecognized `TypeCode`).
   - **Safe Upsert on Confirm:**
     - Match existing records by `Code` (case-insensitive, including soft-deleted rows to avoid `(Code, ValidFrom)` unique index collisions).
     - If not found → insert new `AdministrativeUnit`.
     - If found → update `Name`, `TypeCode`, `ValidFrom`, and clear `ValidTo` / `IsDeleted = false` if previously deactivated (preserves all foreign keys from `ResidencePeriods`, `CompanySites`, `Accommodations`, `Inspections`).
     - Skip error rows and report exact counts (`CreatedCount`, `UpdatedCount`, `ReactivatedCount`, `SkippedErrorCount`).

## 3. Architecture Boundaries
- `IRM.Application`: Contracts (`AdministrativeUnitImportRow`, `AdministrativeUnitImportPreview`, `AdministrativeUnitImportResult`), methods on `ICatalogStore` & `ICatalogService`, delegation in `CatalogApplicationService`.
- `IRM.Infrastructure`: `AdministrativeUnitExcelHelper` (ClosedXML) + `SqlCatalogStore` implementation (guarded by `RequireCatalogEditorAsync`).
- `IRM`: UI in `AdminAdministrativeUnitsSection.razor` + handlers in `Admin.Catalogs.cs` + `IJSRuntime` for template download via `irmInterop.downloadFile`.
