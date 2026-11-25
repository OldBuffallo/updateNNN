# Offline Deployment Hardening, SQL Server Fixes & Spec Sync Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `executing-plans` to implement this plan task-by-task using TDD.

**Goal:** Eliminate all 6 Critical and 5 Important issues uncovered by the Offline Deployment & SQL Server Runtime Crash Audits so `custom/kh2` installs and runs reliably on air-gapped Windows + SQL Server machines, and synchronize the master technical specification (`DAC-TA-CHUC-NANG-HE-THONG-IRM.md`) and installation guide (`INSTALLATION-v1.0.2.md`).

**Architecture:** Fixes span `IRM.Infrastructure` (EF Core migration, SQL stores, malware scanner, release self-test), `IRM` (Blazor circuit guards, JS interop exception handling), `IRM/deploy/` (PowerShell install/backup/repair/restore scripts, Inno Setup, SQL schema verification), `IRM.Tests` (regression tests), and `IRM/docs/` (specs & installation guide).

**Tech Stack:** .NET 8, Blazor Server, EF Core (SQL Server + SQLite test), PowerShell, Inno Setup, xUnit, Node.js test runner.

---

### Task 1: SQL Server Migration & Store Runtime Crash Fixes (TDD)

**Files:**
- Modify: `IRM/IRM.Infrastructure/Persistence/Migrations/20261122093015_MakeEmployeeCompanyNullable.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/Reports/SqlStatisticsStore.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/Administration/SqlCatalogStore.cs`
- Modify: `IRM/IRM.Infrastructure/Excel/AdministrativeUnitExcelHelper.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.ExtensionHistory.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ForeignPersons/SqlFamilyVisitorStore.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ForeignPersons/SqlFamilyVisitorStore.Unified.cs`
- Test: `IRM/IRM.Tests/OfflineSqlServerHardeningTests.cs`

- [ ] **Step 1: Write failing regression tests in `IRM/IRM.Tests/OfflineSqlServerHardeningTests.cs`**
  Cover:
  1. `MakeEmployeeCompanyNullable_DropsAndRecreatesIxEmployeesIdCompanyAroundAlterColumn` (verifies `DropIndex("IX_Employees_IDCompany", "Employees")` precedes `AlterColumn` and `CreateIndex` follows it in both `Up` and `Down`).
  2. `SqlStatisticsStore_GetTerritoryAsync_SucceedsWhenActiveCompanyAccommodationAgreementExists` (reproduces the guaranteed `NullReferenceException` when `CompanyAccommodationAgreement` exists!).
  3. `SqlCatalogStore_ImportAdministrativeUnitsExcel_RejectsCodeExceeding450CharsAndHandlesDuplicateDates`
  4. `ElectronicIdentity_NonAlphanumericInput_DoesNotInsertNullSearchKey`
  5. `UnifiedForeignPerson_CompanyIdZero_NormalizesSponsorAndResponsibleCompanyToNull`

- [ ] **Step 2: Run tests to verify RED**
  Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~OfflineSqlServerHardeningTests"`

- [ ] **Step 3: Implement all Task 1 fixes**
  - Drop/recreate `IX_Employees_IDCompany` in `20261122093015_MakeEmployeeCompanyNullable.cs` (`Up` & `Down`).
  - Project `{ x.AccommodationId, CompanyName = x.Company!.CompanyName }` in SQL in `SqlStatisticsStore.cs:55-61` and use `FirstOrDefaultAsync` at line 67.
  - Validate `Code.Length <= 450` in `AdministrativeUnitExcelHelper.cs` and `SqlCatalogStore.cs`; replace `ToDictionary` at `SqlCatalogStore.cs:347` with safe `GroupBy(...).ToDictionary(...)`.
  - Guard `IdentitySearchKey` against `null` (when `SearchKey(...)` returns `null`, skip/reject instead of `!`) in `SqlImportStore.ExtensionHistory.cs:190`, `SqlFamilyVisitorStore.cs:51,208`, and `SqlFamilyVisitorStore.Unified.cs:705`.
  - Normalize `CompanyId = input.CompanyId is > 0 ? input.CompanyId : null` in `SqlFamilyVisitorStore.Unified.cs:481,490,657,666`.

- [ ] **Step 4: Run tests to verify GREEN & commit**
  ```powershell
  dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~OfflineSqlServerHardeningTests"
  git add IRM/IRM.Infrastructure/Persistence/Migrations/20261122093015_MakeEmployeeCompanyNullable.cs IRM/IRM.Infrastructure/Persistence/Stores/Reports/SqlStatisticsStore.cs IRM/IRM.Infrastructure/Persistence/Stores/Administration/SqlCatalogStore.cs IRM/IRM.Infrastructure/Excel/AdministrativeUnitExcelHelper.cs IRM/IRM.Infrastructure/Persistence/Stores/ImportExport/SqlImportStore.ExtensionHistory.cs IRM/IRM.Infrastructure/Persistence/Stores/ForeignPersons/SqlFamilyVisitorStore.cs IRM/IRM.Infrastructure/Persistence/Stores/ForeignPersons/SqlFamilyVisitorStore.Unified.cs IRM/IRM.Tests/OfflineSqlServerHardeningTests.cs
  git commit -m "fix(sql-server): fix MakeEmployeeCompanyNullable index drop, Statistics NRE, and SQL Server null/truncation traps"
  ```

---

### Task 2: Blazor Server Circuit Stability & Third-Party AV Malware Scanner Resilience (TDD)

**Files:**
- Modify: `IRM/IRM/Features/ImportExport/Import.razor.cs`
- Modify: `IRM/IRM/Features/ImportExport/Import.Guidance.razor.cs`
- Modify: `IRM/IRM/Features/Reports/Statistics.razor`
- Modify: `IRM/IRM/Components/Shared/MainLayout.razor`
- Modify: `IRM/IRM/Features/ForeignPersons/ForeignPersons.razor`
- Modify: `IRM/IRM.Infrastructure/Files/WindowsDefenderMalwareScanner.cs`
- Modify: `IRM/IRM.Infrastructure/DependencyInjection.cs`
- Modify: `IRM/IRM/Program.cs`
- Test: `IRM/IRM.Tests/OfflineSqlServerHardeningTests.cs`

- [ ] **Step 1: Add failing tests for MalwareScanner fallback & Blazor circuit guards**
  Verify:
  1. `WindowsDefenderMalwareScanner` when `requireDefender = false` and `MpCmdRun.exe` is missing does NOT throw/block offline machines with third-party AV (Bkav/Kaspersky/ESET), and drains stdout/stderr safely.
  2. `MainLayout.razor`, `Import.Guidance.razor.cs`, `Import.razor.cs`, `Statistics.razor`, `ForeignPersons.razor` have proper `JSDisconnectedException`/`OperationCanceledException`, `_loading` concurrency guard, `OpenReadStream` try/catch, and `_profileSubmitted = false` reset.

- [ ] **Step 2: Run tests to verify RED**

- [ ] **Step 3: Implement all Task 2 fixes**
  - `WindowsDefenderMalwareScanner.cs`: accept `bool requireDefender = false` (from `Security:RequireDefenderScanner`, default `false` on `custom/kh2`), read stdout/stderr asynchronously before `WaitForExitAsync()` to prevent pipe deadlock, and return clean pass with warning when Defender is disabled by third-party AV and `requireDefender == false`.
  - `Import.razor.cs`: move `OpenReadStream` (line 142) and `CheckDuplicatePassportAsync` (line 605) inside `try/catch`; dispose `_fileStream` on catch.
  - `Statistics.razor`: guard `LoadOverview()`, `LoadDocuments()`, `LoadZones()` with `_loading` flag, `try/catch`, and `Disabled="_loading"` on buttons.
  - `MainLayout.razor` & `Import.Guidance.razor.cs`: catch `JSDisconnectedException` and `OperationCanceledException`; isolate `localStorage.getItem` into its own `try/catch` so blocked storage never breaks shortcut/tour setup.
  - `ForeignPersons.razor`: reset `_profileSubmitted = false` in `OpenAddPerson()` and `OpenEditPerson()`.

- [ ] **Step 4: Run tests to verify GREEN & commit**
  ```powershell
  dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~OfflineSqlServerHardeningTests"
  git add IRM/IRM/Features/ImportExport/Import.razor.cs IRM/IRM/Features/ImportExport/Import.Guidance.razor.cs IRM/IRM/Features/Reports/Statistics.razor IRM/IRM/Components/Shared/MainLayout.razor IRM/IRM/Features/ForeignPersons/ForeignPersons.razor IRM/IRM.Infrastructure/Files/WindowsDefenderMalwareScanner.cs IRM/IRM.Infrastructure/DependencyInjection.cs IRM/IRM/Program.cs IRM/IRM.Tests/OfflineSqlServerHardeningTests.cs
  git commit -m "fix(stability): harden Blazor circuit guards, concurrent DbContext in Statistics, and offline AV scanner fallback"
  ```

---

### Task 3: Offline Windows Installer Scripts, Retry Loops & Schema Sync (TDD)

**Files:**
- Modify: `IRM/deploy/windows/backup.ps1`
- Modify: `IRM/deploy/windows/install.ps1`
- Modify: `IRM/deploy/windows/installer-ui.ps1`
- Modify: `IRM/deploy/windows/repair.ps1`
- Modify: `IRM/deploy/windows/restore.ps1`
- Modify: `IRM/deploy/windows/irm.iss`
- Modify: `IRM/deploy/release-manifest.json`
- Modify: `IRM/deploy/database/verify-installed-database.sql`
- Regenerate/Update: `IRM/deploy/database/IRM-v1.0.1-schema.sql` (via `dotnet ef migrations script --idempotent` or appending the 2 missing migrations idempotently)
- Test: `IRM/IRM.Tests/OfflineDeploymentScriptTests.cs`

- [ ] **Step 1: Write failing tests in `IRM/IRM.Tests/OfflineDeploymentScriptTests.cs`**
  Verify:
  1. `backup.ps1` sets working directory to `$appRoot` (`Push-Location $appRoot` / `Pop-Location`).
  2. `installer-ui.ps1` and `install.ps1` do NOT hard-block CPUs with `MaxClockSpeed < 2000`, and `installer-ui.ps1` warns rather than hard-blocks when Defender is disabled by third-party AV.
  3. `install.ps1`, `repair.ps1`, and `restore.ps1` retry `/health/ready` in a loop instead of single-shot `Invoke-RestMethod`.
  4. `install.ps1` uses `127.0.0.1,14331` consistently on upgrades too.
  5. `irm.iss` and `release-manifest.json` target `KH2` (not `R6`).
  6. `verify-installed-database.sql` checks `20261122093015_MakeEmployeeCompanyNullable` and `IRM-v1.0.1-schema.sql` includes `VisaSymbol` and nullable `Employees.IDCompany`.

- [ ] **Step 2: Run tests to verify RED**

- [ ] **Step 3: Implement all Task 3 deployment script & schema fixes**

- [ ] **Step 4: Run tests to verify GREEN & commit**
  ```powershell
  dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~OfflineDeploymentScriptTests"
  git add IRM/deploy/windows/backup.ps1 IRM/deploy/windows/install.ps1 IRM/deploy/windows/installer-ui.ps1 IRM/deploy/windows/repair.ps1 IRM/deploy/windows/restore.ps1 IRM/deploy/windows/irm.iss IRM/deploy/release-manifest.json IRM/deploy/database/verify-installed-database.sql IRM/deploy/database/IRM-v1.0.1-schema.sql IRM/IRM.Tests/OfflineDeploymentScriptTests.cs
  git commit -m "fix(deploy): harden offline Windows installer, backup cwd, health retries, CPU/AV preflight, and SQL schema sync"
  ```

---

### Task 4: Sync Technical Spec, Installation Guide & Full Regression Verification

**Files:**
- Modify: `IRM/docs/DAC-TA-CHUC-NANG-HE-THONG-IRM.md`
- Modify: `IRM/docs/INSTALLATION-v1.0.2.md`
- Commit: `IRM/docs/superpowers/plans/2026-11-25-offline-hardening-and-sqlserver-fixes.md`

- [ ] **Step 1: Update `IRM/docs/DAC-TA-CHUC-NANG-HE-THONG-IRM.md` and `IRM/docs/INSTALLATION-v1.0.2.md`**
  - Add Section 7 (*Chuẩn triển khai Offline & Bảo vệ Runtime SQL Server / Blazor Server*) to `DAC-TA-CHUC-NANG-HE-THONG-IRM.md`.
  - Remove removed R6 feedback survey copy from `INSTALLATION-v1.0.2.md`, document `Security:RequireHttpsForRemoteClients` (LAN HTTP vs HTTPS) and `Security:RequireDefenderScanner` (third-party AV like Bkav/Kaspersky/ESET).

- [ ] **Step 2: Run full Release build, full `.NET` test suite, and full Node test suite**
  ```powershell
  dotnet build IRM.sln -c Release
  dotnet test IRM.Tests/IRM.Tests.csproj -c Release --no-build
  node --test IRM.Tests/Browser/*.test.cjs
  ```
  Expected: `0 Warning(s), 0 Error(s)`, all `.NET` tests PASS, `54/54` Node tests PASS.

- [ ] **Step 3: Commit docs & update `walkthrough.md`**
