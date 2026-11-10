# Unified Foreign Person & Company Profile Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Consolidate the fragmented person pages (`ForeignPersons`, `Employees`, `Students`, `FamilyVisitors`) into one unified multi-tab person profile page and merge `CompanyProfiles` into `Companies`, while preserving 100% compatibility with legacy databases and Excel imports.

**Architecture:** `IForeignerRegistryService` and `SqlFamilyVisitorStore` provide a unified read/write adapter (`UnifiedForeignPersonDto` / `UnifiedForeignPersonInput`) that merges `ForeignPersons`, active `Employees`, and visible `Students` on read (including unlinked legacy rows with missing or duplicate passports) and synchronizes writes/soft-deletes across both new (`ForeignPerson`, `StayCase`, `ResidencePeriod`, `ImmigrationDocument`, `ElectronicIdentity`, `FamilyVisitDetail`) and legacy (`Employees`, `Students`, `ForeignPersonSourceLinks`) tables. `ForeignPersons.razor` serves `/foreign-persons`, `/employees`, `/students`, and `/family-visitors` with a 5-tab add/edit dialog; `Companies.razor` serves `/companies` and `/company-profiles` with integrated extended profile tabs; `NavMenu.razor` simplifies the data section to 3 items.

**Tech Stack:** .NET 8, Blazor Server, MudBlazor 9.3.0, EF Core 8.0.31, xUnit, Node test runner.

**Spec:** [2026-11-10-unified-person-company-profile-design.md](../specs/2026-11-10-unified-person-company-profile-design.md)

## Global Constraints

- Work on `develop` in workspace `NNN/`; run .NET commands from `IRM/`; do not push or modify customer/production databases.
- Preserve all 43 EF Core entities, 2 migrations, and SQL Server schema baseline checksums in `IRM.Tests/Baselines/`.
- Enforce project boundaries verified by `ArchitectureCompatibilityTests`: `IRM.Components.Pages` classes must never declare fields of `IRM.Core` entity class types or `IRM.Infrastructure` types.
- Preserve all legacy routes (`/employees`, `/students`, `/family-visitors`, `/company-profiles`, `/companies`, `/foreign-persons`) and the `+ Thêm công ty` / `➕ Thêm công ty mới` dialog contract checked by `IRM.Tests/Browser/theme-ui.cjs`.
- All UI labels and validation messages must use proper Vietnamese diacritics.

## Review Focus

- Legacy database with rows in `Employees` or `Students` and 0 rows in `ForeignPersons` (including rows with null/empty or duplicate `Passport` or variable-length `Nationality` codes like `USA`, `KOR`, `D`): all active rows must appear in `GetUnifiedProfilesAsync` and open in the edit dialog without null/FK errors.
- Saving a person with `HasWorkProfile = true` when no Career or Company is selected: must return a clear Vietnamese validation message before hitting SQL `NOT NULL` / FK constraints on `Employees.IDCompany` or `Employees.IDCareer`.
- Editing an existing legacy `Employee` from the unified dialog and adding an `ElectronicIdentity` + `FamilyVisit`: must update the existing `Employees` row in-place (preserving `IDEmployee`) while creating/updating the linked `ForeignPerson`, `ForeignPersonSourceLink`, `StayCase`, `FamilyVisitDetail`, and `ElectronicIdentity`.
- Deleting a unified record that is linked to an `Employee` or `Student`: must soft-delete both `ForeignPerson` (`IsDeleted = true`) and the linked `Employee`/`Student` (`Hidden_flag = 1`) so the record does not reappear on reload.
- Visiting `/employees`, `/students`, `/family-visitors`, or `/company-profiles` directly (from Dashboard, Spotlight Search, Import Result, or `theme-ui.cjs`): must render without Blazor errors and pre-select the matching category filter/view.

---

### Task 1: Unified Foreign Person Application Model & Store Sync

**Files:**
- Create: `IRM/IRM.Application/Features/ForeignPersons/Models/UnifiedForeignPersonDto.cs`
- Create: `IRM/IRM.Application/Features/ForeignPersons/Contracts/UnifiedForeignPersonInput.cs`
- Modify: `IRM/IRM.Application/Abstractions/Persistence/IForeignerRegistryStore.cs`
- Modify: `IRM/IRM.Application/Features/ForeignPersons/IForeignerRegistryService.cs`
- Modify: `IRM/IRM.Application/Features/ForeignPersons/FamilyVisitorApplicationService.cs`
- Modify: `IRM/IRM.Infrastructure/Persistence/Stores/ForeignPersons/SqlFamilyVisitorStore.cs`
- Create: `IRM/IRM.Tests/UnifiedForeignPersonTests.cs`

**Interfaces:**
- Produces:
  - `IRM.Application.Models.UnifiedForeignPersonDto` containing unified identity (`ForeignPersonId`, `EmployeeId`, `StudentId`, `FamilyVisitStayCaseId`, `FullName`, `Gender`, `Birthday`, `NationalityCode`, `NationalityName`, `PassportNumber`, `ResidenceAddress`, `AdministrativeUnitId`, `AdministrativeUnitName`, `Note`), purpose flags (`HasWorkProfile`, `HasStudyProfile`, `HasFamilyVisitProfile`, `PurposeCodes`), work fields (`CompanyId`, `CompanyName`, `CareerId`, `CareerName`, `WorkPermit`, `WorkPermitNumber`, `TemporaryStay`, `VisaNumber`, `VisaSymbol`), study fields (`SchoolName`, `Major`, `EducationLevel`, `ScholarshipType`, `EnrollmentDate`, `GraduationDate`, `StudentStatus`, `VisaExpiry`, `PhoneNumber`), family visit fields (`RelativeName`, `Relationship`, `RelativeTypeCode`, `RelativeNationalityCode`, `RelativeIdNumber`, `RelativePhone`, `RelativeAddress`, `VisitStartDate`, `VisitEndDate`), and document/EID summaries (`ElectronicIdNumber`, `ValidDocumentCount`, `DocumentSummary`, `ImmigrationDocuments`, `ElectronicIdentities`).
  - `IRM.Services.UnifiedForeignPersonInput` with all editable fields for the 5-tab dialog.
  - `IForeignerRegistryService.GetUnifiedProfilesAsync(string? search, DateTime asOfDate, CancellationToken cancellationToken = default)`
  - `IForeignerRegistryService.SaveUnifiedProfileAsync(UnifiedForeignPersonInput input, CancellationToken cancellationToken = default)`
  - `IForeignerRegistryService.DeleteUnifiedProfileAsync(int? foreignPersonId, int? employeeId, int? studentId, CancellationToken cancellationToken = default)`
  - `IForeignerRegistryService.ImportFromLegacyDatabaseAsync(string sourceConnectionStringOrDbName, CancellationToken cancellationToken = default)` returning `LegacyDatabaseImportResult` (`NationalitiesAdded`, `FieldsAdded`, `CareersAdded`, `DistrictsAdded`, `WardsAdded`, `CompaniesAdded`, `EmployeesAdded`, `ProfilesLinked`)

- [x] **Step 1: Write unit tests in `IRM/IRM.Tests/UnifiedForeignPersonTests.cs`**
  Test cases:
  1. `GetUnifiedProfilesAsync_IncludesUnlinkedLegacyEmployeesAndStudents_EvenWithoutPassport`: seeds `Employees` (one with passport, one without passport) and `Students` with `ForeignPersons` empty; verifies all appear in `GetUnifiedProfilesAsync`.
  2. `SaveUnifiedProfileAsync_CreatesAndLinksEmployeeStudentFamilyVisitAndEid`: saves a `UnifiedForeignPersonInput` with `HasWorkProfile = true`, `HasFamilyVisitProfile = true`, and `ElectronicIdentityNumber`; verifies `ForeignPersons`, `Employees`, `ForeignPersonSourceLinks`, `StayCases`, `FamilyVisitDetails`, and `ElectronicIdentities` are persisted and linked.
  3. `DeleteUnifiedProfileAsync_SoftDeletesBothForeignPersonAndLegacyRecords`: calls `DeleteUnifiedProfileAsync` and verifies `ForeignPerson.IsDeleted == true` and `Employee.Hidden_flag == 1`.

- [x] **Step 2: Create `UnifiedForeignPersonDto.cs` and `UnifiedForeignPersonInput.cs` and update interfaces**
  Add the DTO, input contract, `IForeignerRegistryStore` methods, `IForeignerRegistryService` methods, and `FamilyVisitorApplicationService` forwarding with RBAC guards (`RequireAnyRoleAsync`).

- [x] **Step 3: Implement `GetUnifiedProfilesAsync`, `SaveUnifiedProfileAsync`, and `DeleteUnifiedProfileAsync` in `SqlFamilyVisitorStore.cs`**
  - In `GetUnifiedProfilesAsync`: load active `ForeignPersons` (with `StayCases.FamilyVisitDetail`, `ResidencePeriods.AdministrativeUnit`, `ImmigrationDocuments`, `ElectronicIdentities`, `SourceLinks`, `Nationality`), active `Employees` (`Hidden_flag == 0 && WorkingStatus == 0` with `Company`, `Career`, `NationalityNav`), and visible `Students` (`Hidden_flag == 0` with `NationalityNav`). Merge by `ForeignPersonSourceLink` (`EMPLOYEE`/`STUDENT`), then fallback to `PassportSearchKey` match, and emit standalone `UnifiedForeignPersonDto` rows for any remaining unlinked `Employees`/`Students`.
  - Also enhance `BackfillLegacyAsync` so legacy `Employees`/`Students` with missing passports still create a `ForeignPerson` (`IsDataIncomplete = true`) and `ForeignPersonSourceLink` alongside the `MigrationIssue` record.
  - In `SaveUnifiedProfileAsync`: validate `FullName`, validate `NationalityCode` against active `Nationality` table (`Delete_flag == 0`), validate `CompanyId` & `CareerId` when `HasWorkProfile` is true (using first active career as fallback if not specified and one exists, or throwing a clear Vietnamese error), validate `RelativeName` & `Relationship` when `HasFamilyVisitProfile` is true, and upsert `ForeignPerson`, `Employees`, `Students`, `ForeignPersonSourceLinks`, `StayCases`, `FamilyVisitDetails`, `ResidencePeriods`, `ImmigrationDocuments`, and `ElectronicIdentities` inside an execution-strategy transaction.
  - In `DeleteUnifiedProfileAsync`: soft-delete `ForeignPerson` (`IsDeleted = true`, `StayCases` → `"DELETED"`), `Employee` (`Hidden_flag = 1`), and `Student` (`Hidden_flag = 1`).
  - In `ImportFromLegacyDatabaseAsync`: accept either a database name on the current SQL Server instance (e.g. `ReportManagerDB`) or a full SQL Server connection string (`ADMIN` only); read-only `SELECT` from source `Nationality`, `Fields`, `CareerGroups`, `Careers`, `Districts`, `Wards`, `Companies`, and `Employees`; idempotently insert missing catalog entries (preserving variable-length `NationalityCode`), companies, and employees into `_db`, then run `BackfillLegacyAsync` so all imported records are immediately linked into `ForeignPersons`.

- [x] **Step 4: Build Solution in Release mode**
  Run: `dotnet build IRM.sln -c Release` from `IRM/`
  Expected: `0 Warning(s)`, `0 Error(s)`.

- [x] **Step 5: Commit Task 1**
  Commit message: `feat: add unified foreign person read/write store with legacy database merge`

---

### Task 2: Unified Foreign Persons UI & Multi-Tab Profile Dialog

**Files:**
- Modify: `IRM/IRM/Features/ForeignPersons/ForeignPersons.razor`
- Modify: `IRM/IRM/Features/Employees/Employees.razor`
- Modify: `IRM/IRM/Features/Students/Students.razor`
- Modify: `IRM/IRM/Features/ForeignPersons/FamilyVisitors.razor`

**Interfaces:**
- Consumes: `IForeignerRegistryService.GetUnifiedProfilesAsync`, `SaveUnifiedProfileAsync`, `DeleteUnifiedProfileAsync`, `BackfillLegacyAsync`, `IEmployeeService` (for expired archive counts/execution), `ICompanyService`, `ICatalogService`, `IExportService`.
- Produces: Unified `/foreign-persons` (and `/employees`, `/students`, `/family-visitors`) page with category filter chips (`Tất cả`, `Lao động`, `Du học sinh`, `Thăm thân`, `Sắp hết hạn`), detailed filters, Excel export, expired archive dialog, legacy backfill action, and the 5-tab Add/Edit dialog.

- [x] **Step 1: Remove duplicate `@page` directives from legacy person pages**
  Remove `@page "/employees"` from `Employees.razor`, `@page "/students"` from `Students.razor`, and `@page "/family-visitors"` from `FamilyVisitors.razor`, and add `@page "/employees"`, `@page "/students"`, `@page "/family-visitors"` to `ForeignPersons.razor` so all 4 routes render the unified page and automatically set the initial category filter based on `NavigationManager.Uri`.

- [x] **Step 2: Implement unified list, category chips, filters, Excel export, and archive actions in `ForeignPersons.razor`**
  - Summary stat cards: Tổng hồ sơ NNN, Diện lao động, Du học & Thăm thân, Đã cấp định danh ĐT / Sắp hết hạn.
  - Category filter chips: `Tất cả`, `Lao động`, `Du học sinh`, `Thăm thân`, `Sắp hết hạn (≤30 ngày)`.
  - Filter toolbar: Search text (họ tên, hộ chiếu, công ty, trường học, thân nhân), Công ty (`MudAutocomplete<CompanyDto>`), Quốc tịch (`MudSelect<string>` loaded from `Catalog.GetNationalitiesAsync()`), Địa bàn xã/phường (`MudSelect<int?>` from `Catalog.GetAdministrativeUnitsAsync()`), GPLĐ filter.
  - Action buttons: `📥 Xuất Excel`, Admin `🗑️ Xóa hết hạn tạm trú` & `🗑️ Xóa hết hạn thăm thân` (using `IEmployeeService`), Admin `🔄 Đồng bộ DB cũ` (when unlinked legacy records exist), and `+ Thêm hồ sơ NNN`.
  - Adaptive DataGrid columns showing: Họ và tên (with gender & passport), Quốc tịch, Diện cư trú (chips for Lao động / Du học / Thăm thân), Đơn vị / Chi tiết diện (Công ty + Nghề nghiệp + GPLĐ, Trường + Ngành, hoặc Thân nhân bảo lãnh), Giấy tờ & Thời hạn (Hạn tạm trú / Visa / Còn lại ngày), Định danh điện tử (EID), and Thao tác (Xem/Sửa hồ sơ 5 tab, Cấp nhanh giấy tờ, Cấp nhanh EID, Xóa).

- [x] **Step 3: Implement the 5-Tab Unified Add/Edit Profile Dialog in `ForeignPersons.razor`**
  - Tab 1 (`1. THÔNG TIN CHUNG & CƯ TRÚ`): Họ và tên `*`, Giới tính, Ngày sinh, Quốc tịch (`MudSelect` from active catalog + preserves existing code), Số hộ chiếu, Địa bàn xã/phường, Địa chỉ tạm trú, Ghi chú, và các công tắc chọn diện áp dụng (`Diện Lao động`, `Diện Du học`, `Diện Thăm thân`).
  - Tab 2 (`2. LAO ĐỘNG & VIỆC LÀM`): Công ty làm việc (`MudAutocomplete<CompanyDto>`), Nghề nghiệp (`MudSelect<int?>`), Giấy phép lao động (`WorkPermitType`), Số GPLĐ, Hạn tạm trú.
  - Tab 3 (`3. DU HỌC SINH`): Trường học, Ngành học, Trình độ (`EducationLevel`), Học bổng (`ScholarshipType`), Ngày nhập học, Ngày tốt nghiệp, Trạng thái (`StudentStatus`), Số điện thoại.
  - Tab 4 (`4. THĂM THÂN`): Họ tên thân nhân bảo lãnh, Mối quan hệ, Phân loại thân nhân, Quốc tịch thân nhân (nếu gốc VN/2 quốc tịch), Số CCCD/Hộ chiếu thân nhân, Số điện thoại, Địa chỉ thường trú thân nhân, Thời hạn thăm thân (Từ ngày – Đến ngày).
  - Tab 5 (`5. GIẤY TỜ XNC & ĐỊNH DANH ĐT`): Loại giấy tờ (`Visa`, `Gia hạn tạm trú`, `Thẻ tạm trú TRC`), Số Visa / Số thẻ, Ký hiệu Visa, Từ ngày, Hạn đến ngày, Mã số định danh điện tử (EID), Ngày kích hoạt, Hạn định danh.

- [x] **Step 4: Build Solution in Release mode**
  Run: `dotnet build IRM.sln -c Release` from `IRM/`
  Expected: `0 Warning(s)`, `0 Error(s)`.

- [x] **Step 5: Commit Task 2**
  Commit message: `feat: unify foreign person, employee, student, and family visitor UI into multi-tab profile`

---

### Task 3: Unified Companies UI (`/companies` + `/company-profiles`), Admin Legacy DB Sync (`/admin/legacy-sync`) & Sidebar Navigation

**Files:**
- Modify: `IRM/IRM/Components/Shared/NavMenu.razor`
- Modify: `IRM/IRM/Features/Companies/Companies.razor`
- Modify: `IRM/IRM/Features/Companies/CompanyProfiles.razor`
- Create: `IRM/IRM/Features/Administration/Components/AdminLegacySyncSection.razor`
- Modify: `IRM/IRM/Features/Administration/Admin.razor`
- Modify: `IRM/IRM/Features/Administration/Admin.razor.cs`
- Modify: `IRM/IRM/Features/Administration/Admin.Catalogs.cs`

**Interfaces:**
- Consumes: `ICompanyService`, `ICompanyProfileService`, `ILegalDocumentService`, `ICatalogService`, `IExportService`, `IForeignerRegistryService`.
- Produces: Simplified 3-item `HỒ SƠ & DỮ LIỆU` sidebar in `NavMenu.razor`, unified `Companies.razor` handling both `/companies` and `/company-profiles` with integrated 6-tab Company Detail & Extended Profile dialog, and `/admin/legacy-sync` card/section for importing from `ReportManagerDB` and running legacy backfill.

- [x] **Step 1: Simplify `NavMenu.razor`, add `/admin/legacy-sync`, and fix remaining ASCII message in `Admin.razor.cs`**
  - In `NavMenu.razor`, replace the 7 links under `HỒ SƠ & DỮ LIỆU` with 3 links:
    1. `/foreign-persons` — `Hồ sơ Người nước ngoài`
    2. `/companies` — `Doanh nghiệp`
    3. `/accommodations` — `Cơ sở lưu trú`
  - Add `AdminLegacySyncSection.razor` and card `legacy-sync` ("Đồng bộ DB cũ") in `Admin.razor` / `Admin.razor.cs` / `Admin.Catalogs.cs` so `ADMIN` can import directly from `ReportManagerDB` (or custom connection string) and trigger internal legacy backfill.
  - Fix the two remaining ASCII snackbar strings in `Admin.razor.cs` (`Không tải được dữ liệu quản trị` / `Không tải được dữ liệu module`).

- [x] **Step 2: Integrate `CompanyProfiles` 6-tab management into `Companies.razor`**
  - Move `@page "/company-profiles"` from `CompanyProfiles.razor` to `Companies.razor` while keeping `+ Thêm công ty` and `➕ Thêm công ty mới` dialog intact.
  - Upgrade the Company Detail dialog (`_detailVisible` / `OpenDetailDialog`) in `Companies.razor` into a comprehensive 6-tab dialog:
    1. `Thông tin chung & Lao động` (Company info, contact list, employee counts, button to switch to Edit basic info)
    2. `Phân loại & Pháp lý (FDI/MST)` (`CompanyProfileDto`: OwnershipTypeCode, TaxCode, BusinessRegistrationNumber + Save button)
    3. `Địa điểm & Khu kinh tế` (`CompanySiteDto` list + add site form + `EconomicZoneDto` quick create)
    4. `Cơ sở lưu trú đã thuê` (`CompanyAccommodationAgreementDto` list)
    5. `Người đại diện & Thân nhân NLĐ` (`CompanyRepresentativeDto` list + add form + `SponsoredFamilyVisitors` list)
    6. `Hồ sơ pháp nhân Scan` (`CompanyLegalDocumentDto` list + file upload & download)
  - When visiting `/company-profiles` directly, automatically open or display a quick company selector banner on `/companies` so the user can pick a company to open its 6-tab extended profile immediately.

- [x] **Step 3: Build & Verify Tests**
  - Run `dotnet build IRM.sln -c Release` from `IRM/` → `0 Warning(s)`, `0 Error(s)`.
  - Run `node --test IRM.Tests/Browser/theme-preference.test.cjs IRM.Tests/Browser/theme-chart.test.cjs` from `IRM/` → `12/12 PASS`.
  - Restart local app at `http://127.0.0.1:5050` so user can immediately test the unified workflow.

- [x] **Step 4: Commit Task 3**
  Commit message: `feat: merge company extended profiles into companies page and simplify sidebar navigation`
