# App Product Tour & Passport-Required Notification Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an interactive offline Driver.js **Product Tour** starting with Step 1 (Admin System & Catalog Setup — automatically skipped for non-Admin users) followed by every feature in exact Sidebar order, replace the initial R6 feedback spotlight auto-trigger with this tour, and surface a prominent **"Phải có Số hộ chiếu"** notification banner across the main shell and data-entry screens.

**Architecture:** Implement a self-contained browser module `IRM/IRM/wwwroot/js/app-product-tour.js` that queries `[data-app-tour="..."]` anchors rendered by `NavMenu.razor` and `#btnAppUserGuide` in `MainLayout.razor`. Because `NavMenu.razor` already wraps Admin links in `<AuthorizeView Roles="ADMIN">`, filtering steps by DOM presence naturally skips Step 1 for non-Admin accounts without duplicating RBAC logic in JS. Add a dismissible Passport-required notification banner in `MainLayout.razor` plus contextual reminders in `ImportUploadSection.razor` and `ForeignPersons.razor`.

**Tech Stack:** .NET 8, ASP.NET Core Blazor Server, MudBlazor, Driver.js 1.12.1 (offline vendor), Node `node:test`, xUnit/bUnit.

**Spec:** `IRM/docs/superpowers/specs/2026-11-23-app-product-tour-design.md`

## Global Constraints

- Exact tour sequence: **Step 1 = Quản trị hệ thống & Danh mục** (Admin only; automatically skipped if non-Admin), then **exact top-to-bottom Sidebar order** (`Bảng điều khiển` → `Hồ sơ Người nước ngoài` → `Doanh nghiệp` → `Cơ sở lưu trú` → `Đợt kiểm tra NNN` → `Tra cứu toàn cục` → `Báo cáo định kỳ` → `Thống kê 54 địa bàn` → `Nhập dữ liệu Excel`), ending on `#btnAppUserGuide`.
- Keep existing `wwwroot/js/import-tour.js` and `irmInterop.initFeedbackSpotlight` intact so all existing 50 Node tests stay green.
- Passport-required notification must explicitly state that **Số hộ chiếu là trường bắt buộc duy nhất**.
- 100% offline-safe (no CDN), graceful no-op if `window.driver` is absent or `localStorage` is blocked.

## Review Focus

- **Non-Admin user login:** `[data-app-tour="admin-system"]` is absent from DOM → Step 1 is skipped automatically and the tour starts cleanly on `Bảng điều khiển` (`1/10` instead of crashing on a missing selector).
- **Admin user login:** Step 1 highlights `[data-app-tour="admin-system"]`, then steps 2–10 walk down the Sidebar in exact visual order, ending on `#btnAppUserGuide` (`11/11`).
- **First visit vs repeat visit vs manual replay:** First visit runs once and persists `localStorage['irm-product-tour-v1-seen'] = '1'`; subsequent loads do not auto-run; clicking `#btnAppUserGuide` replays (`forceReplay = true`) and opens the drawer if collapsed.
- **Blocked `localStorage` or missing `window.driver`:** Must never throw an uncaught JS exception or terminate the Blazor Server circuit.
- **Passport-required notification:** Visible immediately on `MainLayout` (dismissible), `ImportUploadSection`, and `ForeignPersons` modal without blocking form clicks.

---

## File Structure

| Area | Files | Responsibility |
|---|---|---|
| **Product Tour JS** | `IRM/IRM/wwwroot/js/app-product-tour.js` *(NEW)*, `IRM/IRM/Components/App.razor` | Driver.js step builder, role-aware DOM filtering, `localStorage` persistence, safe replay |
| **Layout & Sidebar** | `IRM/IRM/Components/Shared/NavMenu.razor`, `IRM/IRM/Components/Shared/MainLayout.razor` | `data-app-tour` anchors in exact Sidebar order, `#btnAppUserGuide` topbar button, first-visit tour trigger, dismissible Passport-required notification banner |
| **Data-Entry Notices** | `IRM/IRM/Features/ImportExport/Components/ImportUploadSection.razor`, `IRM/IRM/Features/ForeignPersons/ForeignPersons.razor` | Contextual "Phải có Số hộ chiếu" notification at data-entry points |
| **Tests** | `IRM/IRM.Tests/Browser/app-product-tour.test.cjs` *(NEW)*, `IRM/IRM.Tests/AppProductTourMarkupTests.cs` *(NEW)* | Node unit tests for Admin vs non-Admin step order/skipping/storage + .NET markup verification for anchors and Passport-required notification |

---

### Task 1: Product Tour JS Engine (`app-product-tour.js`) + Node Tests

**Files:**
- Create: `IRM/IRM/wwwroot/js/app-product-tour.js`
- Test: `IRM/IRM.Tests/Browser/app-product-tour.test.cjs`

**Interfaces:**
- Consumes: `window.driver.js.driver`, DOM anchors `[data-app-tour="..."]` and `#btnAppUserGuide`.
- Produces:
  - `window.irmAppTour.init(forceReplay: boolean) -> boolean`
  - `window.irmAppTour.buildSteps(doc?: Document) -> DriverStep[]`
  - `window.irmAppTour.markSeen() -> void`

- [ ] **Step 1: Write failing Node tests in `IRM/IRM.Tests/Browser/app-product-tour.test.cjs`**

Cover 4 cases:
1. `admin user gets 11 steps starting with admin-system followed by exact sidebar order`.
2. `non-admin user (no admin-system anchor) skips step 1 and starts at nav-dashboard (10 steps)`.
3. `init runs once on first visit, marks irm-product-tour-v1-seen, skips repeat visit unless forceReplay is true`.
4. `missing driver.js or blocked localStorage never throws`.

- [ ] **Step 2: Run Node test to verify it fails**

Run (from `IRM/`): `node --test IRM.Tests/Browser/app-product-tour.test.cjs`
Expected: FAIL (`app-product-tour.js` does not exist).

- [ ] **Step 3: Implement `IRM/IRM/wwwroot/js/app-product-tour.js`**

Implement `window.irmAppTour` with all 11 step definitions (Admin setup first, then exact Sidebar order, highlighting Passport requirement on `nav-foreign-persons` and `nav-import`, ending at `#btnAppUserGuide`), filtering out steps whose selector is not in DOM, scrolling target into view, and safely persisting `irm-product-tour-v1-seen`.

- [ ] **Step 4: Run Node test to verify it passes**

Run (from `IRM/`): `node --test IRM.Tests/Browser/app-product-tour.test.cjs`
Expected: PASS (4 passed, 0 failed).

- [ ] **Step 5: Commit**

```powershell
git add IRM/IRM/wwwroot/js/app-product-tour.js IRM/IRM.Tests/Browser/app-product-tour.test.cjs
git commit -m "feat(tour): add Driver.js app-wide product tour with admin-first and sidebar-ordered steps"
```

---

### Task 2: Wire Sidebar Anchors, Topbar Guide Button & Passport-Required Notification

**Files:**
- Modify: `IRM/IRM/Components/App.razor:20-26` (include `js/app-product-tour.js?v=20261123-kh2`)
- Modify: `IRM/IRM/Components/Shared/NavMenu.razor:10-175` (add `data-app-tour` attributes in exact Sidebar order)
- Modify: `IRM/IRM/Components/Shared/MainLayout.razor:90-130,200-220,260-340` (add `#btnAppUserGuide` button, switch first-render trigger to `irmAppTour.init(false)`, add dismissible Passport-required notification banner `data-passport-required-notice="true"`)
- Modify: `IRM/IRM/Features/ImportExport/Components/ImportUploadSection.razor:5-20` (add Passport-required notice banner)
- Modify: `IRM/IRM/Features/ForeignPersons/ForeignPersons.razor:360-370` (add Passport-required notice inside unified profile dialog)
- Test: `IRM/IRM.Tests/AppProductTourMarkupTests.cs`

**Interfaces:**
- Consumes: `window.irmAppTour.init(bool)` (Task 1).
- Produces: Complete in-app Product Tour UX + Passport-required notifications.

- [ ] **Step 1: Write failing test in `IRM/IRM.Tests/AppProductTourMarkupTests.cs`**

Verify:
1. `NavMenu.razor` contains all 10 `data-app-tour` anchors (`admin-system` inside `<AuthorizeView Roles="ADMIN">` plus all 9 sidebar features in exact order).
2. `MainLayout.razor` contains `#btnAppUserGuide`, calls `irmAppTour.init`, and renders the Passport-required notification (`data-passport-required-notice` + `"PHẢI có Số hộ chiếu"`).
3. `App.razor` loads `js/app-product-tour.js`.

- [ ] **Step 2: Run test to verify it fails**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~AppProductTourMarkupTests"`
Expected: FAIL.

- [ ] **Step 3: Update `App.razor`, `NavMenu.razor`, `MainLayout.razor`, `ImportUploadSection.razor`, and `ForeignPersons.razor`**

Add the exact `data-app-tour` anchors, topbar `#btnAppUserGuide` ("Hướng dẫn sử dụng") button, `irmAppTour.init(false)` on first render, and the Passport-required notification banners.

- [ ] **Step 4: Run test to verify it passes**

Run (from `IRM/`): `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~AppProductTourMarkupTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add IRM/IRM/Components/App.razor IRM/IRM/Components/Shared/NavMenu.razor IRM/IRM/Components/Shared/MainLayout.razor IRM/IRM/Features/ImportExport/Components/ImportUploadSection.razor IRM/IRM/Features/ForeignPersons/ForeignPersons.razor IRM/IRM.Tests/AppProductTourMarkupTests.cs
git commit -m "feat(tour-ui): wire sidebar tour anchors, user guide button, and passport-required notifications"
```

---

### Task 3: Full Regression Verification & Docs Commit

**Files:**
- Commit: `IRM/docs/superpowers/specs/2026-11-23-app-product-tour-design.md`, `IRM/docs/superpowers/plans/2026-11-23-app-product-tour-and-passport-notice.md`

- [ ] **Step 1: Run full Release build, `.NET` test suite, and Node test suite**

Run (from `IRM/`):
```powershell
dotnet build IRM.sln -c Release
dotnet test IRM.Tests/IRM.Tests.csproj -c Release --no-build
node --test IRM.Tests/Browser/*.test.cjs
```
Expected: 0 build warnings/errors; all `.NET` tests PASS; all Node tests (54/54) PASS.

- [ ] **Step 2: Commit docs**

```powershell
git add IRM/docs/superpowers/specs/2026-11-23-app-product-tour-design.md IRM/docs/superpowers/plans/2026-11-23-app-product-tour-and-passport-notice.md
git commit -m "docs(plan): add app product tour and passport-required notification spec and plan"
```
