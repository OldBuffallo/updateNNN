# Onboarding Default Guide Data & Auto-Company Excel Import Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Seed default onboarding guide data on first install/startup (1 sample company with extended profile, 1 sample accommodation, and 1 sample 5-tab foreign person profile) and auto-create missing companies referenced in the `"Công ty"` column during Excel import.

**Architecture:** `DatabaseSeeder.SeedOnboardingGuideIfEmptyAsync` populates rich onboarding guide records when an initialized database has no person records yet, and `DatabaseStartup.InitializeAsync` ensures catalogs and onboarding guide data are present on startup. `SqlImportStore` (`Preview`, `Execution`, `Rollback`) marks unmatched `"Công ty"` column values for automatic company creation (`ExtendedFields["__AutoCreateCompanyName"]`), creates them inside the import transaction, and soft-deletes them if the import session is rolled back.

**Tech Stack:** .NET 8, Blazor Server, MudBlazor, EF Core 8.

**Spec:** [2026-11-10-onboarding-seed-and-auto-company-import-design.md](../specs/2026-11-10-onboarding-seed-and-auto-company-import-design.md)

## Global Constraints

- Preserve existing unit test expectations in `EnumAndReleaseFeatureTddTests`, `SqlServerCompatibilityTests`, and `ImportExportFeatureTddTests`.
- Keep `Disabled="@(Page._fileStream == null)"` on `/import` Step 1 `Tiếp tục` button so `theme-ui.cjs` continues to verify that `Tiếp tục` is disabled before choosing a workbook and enabled once a workbook is selected.

## Review Focus

- Importing `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` when its companies are not yet in `Companies`: `GeneratePreviewAsync` must mark rows as `"new"` with `CompanyDisplay = "🆕 <CompanyName> (Tự động tạo mới)"`, and `ExecuteImportAsync` must create each distinct company once, link the imported employees to it, and record an `ImportBackup` entry for rollback.
- Rolling back an import session that auto-created companies: `RollbackImportAsync` must soft-delete (`Delete_flag = 1`) the auto-created `Company` records in addition to the imported `Employee` and V010 records.
- Starting IRM on a database where `Accounts` exists: `DatabaseStartup.InitializeAsync` must ensure `SeedCatalogsIfEmptyAsync` and `SeedOnboardingGuideIfEmptyAsync` run idempotently without requiring `--bootstrap`.

---

### Task 1: Onboarding Guide Data Seeder & Auto-Company Creation on Import

**Files:**
- Modify: `IRM/IRM.Infrastructure/Maintenance/DatabaseSeeder.cs`
- Modify: `IRM/IRM.Infrastructure/Maintenance/DatabaseStartup.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Preview.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Execution.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.Rollback.cs`
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportUploadSection.razor`
- Modify: `IRM/IRM/Features/ImportExport/Import.razor.cs`
- Modify: `IRM/IRM.Tests/EnumAndReleaseFeatureTddTests.cs`

- [x] **Step 1: Add `SeedOnboardingGuideIfEmptyAsync` in `DatabaseSeeder.cs` and invoke in `DatabaseStartup.cs`**
- [x] **Step 2: Support automatic company creation from `"Công ty"` column in `SqlImportStore.Preview.cs`, `SqlImportStore.Execution.cs`, and `SqlImportStore.Rollback.cs`**
- [x] **Step 3: Update `ImportUploadSection.razor` and `Import.razor.cs` so `/import` never blocks `Tiếp tục` after file upload and defaults `_selectedCompany` to the first active company**
- [x] **Step 4: Build Solution in Release mode (`0 Warning(s), 0 Error(s)`) and commit**
