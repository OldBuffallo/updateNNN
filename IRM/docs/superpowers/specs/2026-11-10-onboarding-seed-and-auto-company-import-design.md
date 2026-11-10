# Onboarding Default Sample Data & Auto-Company Creation on Excel Import — Design Spec

- **Date:** 2026-11-10
- **Status:** Approved
- **Branch:** `develop`

## 1. Intent & Why

When a user installs or launches IRM for the first time on a fresh database (or when `Companies` is empty), two friction points occur:
1. There is no sample/guide data across the 3 main modules (`Hồ sơ Người nước ngoài`, `Doanh nghiệp`, `Cơ sở lưu trú`) to show the user how a complete record looks, and if `DatabaseStartup` runs without `--bootstrap`, `SeedCatalogsIfEmptyAsync` is skipped so `CÔNG TY MẪU - HÃY ĐỔI TÊN` is missing.
2. On `/import`, Step 1 disables the `Tiếp tục` button when `_hasCompanies` is false, only auto-selects `_selectedCompany` when `companies.Count == 1`, and marks Excel rows as errors when the `"Công ty"` column contains a company name that is not yet in `Companies` (such as the companies in `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx`).

The user requested:
1. Creating default guide/sample data on first install so users have an immediate interactive example and default company.
2. Allowing Excel import and manual entry to work seamlessly without requiring pre-existing company records.

## 2. Goals & Non-Goals

- **Goals:**
  - Always ensure standard catalogs and `CÔNG TY MẪU - HÃY ĐỔI TÊN` exist on application startup (`DatabaseStartup.InitializeAsync`), and seed 1 complete onboarding guide dataset (1 Sample Company with extended FDI profile & site, 1 Sample Accommodation, and 1 Sample 5-Tab Foreign Person/Employee profile) when the database has never had any person or accommodation records.
  - Pre-select the first available company as fallback in `/import` Step 1, never block the `Tiếp tục` button when a file is uploaded, and automatically create new `Companies` during `ExecuteImportAsync` when the Excel `"Công ty"` column names a company not yet in `Companies` (with full Rollback support).
- **Non-Goals:**
  - Do not alter the behavior of `DatabaseSeeder.SeedCatalogsIfEmptyAsync` when called directly by unit tests (`EnumAndReleaseFeatureTddTests` / `SqlServerCompatibilityTests`), nor `GeneratePreviewAsync` when called with no `"Công ty"` column and an empty `Companies` table (`ImportExportFeatureTddTests`).

## 3. Approaches

1. **Startup Onboarding Seeder + Auto-Create Company from Excel Column (Selected):**
   - `DatabaseSeeder.SeedCatalogsIfEmptyAsync` continues to seed catalogs and `CÔNG TY MẪU - HÃY ĐỔI TÊN`.
   - `DatabaseSeeder.SeedOnboardingGuideIfEmptyAsync(IrmDbContext db)` is called by `DatabaseStartup.InitializeAsync` on startup: if `!await db.Employees.AnyAsync() && !await db.Students.AnyAsync() && !await db.ForeignPersons.AnyAsync()`, it seeds the extended profile for `CÔNG TY MẪU - HÃY ĐỔI TÊN`, 1 sample `Accommodation` (`CƠ SỞ LƯU TRÚ MẪU - HƯỚNG DẪN SỬ DỤNG`), and 1 sample `Employee` + linked `ForeignPerson` (`KIM MIN-SU (HỒ SƠ MẪU HƯỚNG DẪN - HÃY SỬA HOẶC XÓA)`).
   - `SqlImportStore.Preview.cs` & `SqlImportStore.Execution.cs` auto-create companies when a non-empty `"Công ty"` (`CompanyName`) cell does not match an existing company.
2. **Require Manual Company Creation Before Import (Rejected):**
   - Forces users to manually inspect their Excel file and pre-create every company before importing.
