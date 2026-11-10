# IRM Theme Consistency Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make existing IRM screens readable and consistent in light/dark mode, including restored preferences and Excel mapping.

**Architecture:** One theme provider at the interactive Routes root owns mode initialization and transitions, and supplies a context to both layouts. A Web-only controller coordinates preference restoration and post-render JS application; one palette supplies CSS tokens and chart colors. Preserve all feature handlers, permissions and data behavior.

**Tech Stack:** .NET 8, Blazor Server, existing MudBlazor 9.3.0 and Chart.js, xUnit, Node built-in test runner, browser checks using existing tooling where possible.

**Spec:** [approved design](../specs/2026-11-10-irm-theme-consistency-design.md).

## Global Constraints

- Work in current workspace on `develop`, preserving root history/branch and existing delivery packages; no push or customer deployment.
- All product changes remain in Web; no database/schema/migration/entity, legacy country codes, business rules, role or import/rollback changes.
- Preserve layout, routes, control labels and conditions for enabled/disabled/loading; keep Light/Dark and `irm-theme`, default Light.
- Keep existing dependency versions; browser tooling stays test-only. Run .NET commands from `IRM/`.
- Never run app/test bootstrap against customer or operational databases. Browser data must be isolated; explicitly record any unverified acceptance cases.

## Review Focus

- Saved dark preference on reload: CSS backgrounds and Mud text must converge before real controls are exposed; cover in Task 1 controller/JS tests and Task 3 browser reload.
- Blocked, invalid or slow browser storage: usable Light fallback and no circuit-ending exception; cover in Task 1 error/normalization tests.
- Fast consecutive toggles: the last rendered mode wins and charts use that rendered palette; cover Task 1 controller transition tests and Task 2 async chart test.
- Soft warning rows and popovers: mapping labels/selects stay readable in both modes; cover Task 2 palette contrast and Task 3 browser mapping/dropdown.
- App asset cache and login boundary: a fresh page uses matching JS/CSS, one provider and the saved preference; cover Task 2 asset revision and Task 3 login/new-tab/navigation checks.

## File map

- Create `IRM/Components/Theming/IrmThemeController.cs`: restore, toggle and apply rendered theme, bounded JS calls.
- Create `IRM/Components/Theming/IrmThemeContext.cs`: immutable mode snapshot and toggle callback for layouts.
- Create `IRM/Components/Theming/IrmThemeProvider.razor`: single shared providers, neutral initialization and after-render apply.
- Create `IRM/Components/Theming/IrmTheme.cs`: shared Mud palette with paired contrast colors.
- Modify `IRM/Components/Routes.razor`, `Shared/MainLayout.razor`, `Shared/LoginLayout.razor`: root wrapping and remove repeated theme/providers/state.
- Modify `IRM/wwwroot/irm-interop.js`, `IRM/wwwroot/app.css`, `IRM/Components/App.razor`: safe preference/DOM sync, semantic tokens, chart colors and versioned assets.
- Modify feature color/class attributes only when audit/browser identifies conflicts.
- Create `IRM.Tests/IrmThemeControllerTests.cs`, `IRM.Tests/IrmThemePaletteTests.cs`, `IRM.Tests/Browser/theme-preference.test.cjs`, `IRM.Tests/Browser/theme-chart.test.cjs` and verification documentation.

Paths above are relative to solution `IRM/`; docs paths are relative to that same directory.

### Task 1: Shared theme lifecycle

**Interfaces:** `IrmThemeController(IJSRuntime js)` exposes `bool IsReady`, `bool IsDarkMode`, `bool NeedsApply`, `Task InitializeAsync()`, `void Toggle()`, `Task ApplyRenderedAsync()`. It reads preferences once, normalizes to Light/Dark, bounds JS calls at 3 seconds and never lets storage/interop failure terminate normal UI. `ApplyRenderedAsync` invokes `irmInterop.applyTheme(mode)` after component rendering; applied version tracking preserves later toggles. `IrmThemeContext(bool IsDarkMode, Func<Task> ToggleAsync)` is a new snapshot each provider render.

- [x] Write Node regression tests against real `irm-interop.js`: get/init restores saved dark into data-theme; invalid value becomes light; read/write storage failure still applies a usable theme. Use controlled DOM/storage boundaries, not a second implementation.
- [x] Run `node --test IRM.Tests/Browser/theme-preference.test.cjs` from solution. Expected: current source fails saved-dark sync/error cases.
- [x] Add xUnit tests for valid/invalid/failed restore, initialized fallback, pre-ready toggle, rapid toggles and apply ordering/latest mode. Use IJSRuntime boundary fake, literal expected modes and effective controller state.
- [x] Implement controller/context/provider and safe JS preference functions. Wrap Router in provider; MainLayout consumes context; remove layout-local palette/state/providers and LoginLayout default providers. Preserve non-theme initialization and shortcuts.
- [x] Run Node regression and `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --filter FullyQualifiedName~IrmThemeControllerTests` plus build. Expected: green; if native Windows loader is blocked use Linux Docker with the same source/tests, documenting the failure.
- [x] Review diff and commit `fix: synchronize shared theme lifecycle across IRM layouts`.

### Task 2: Palette, CSS and chart consistency

**Interfaces:** `IrmTheme.Create()` produces the shared MudTheme consumed by Task 1 provider. JS `getTheme()/initTheme()/setTheme(mode)` retain existing entry points; async `applyTheme(mode)` applies normalized mode, waits a rendering frame and refreshes chart colors. Chart helpers read computed CSS properties; CSS aliases Mud variables for primary/on-primary/background/surface/text and semantic paired status colors.

- [ ] Write failing palette tests calculating contrast independently from actual light/dark palette values: all normal text foreground/background, Filled button colors and representative muted/surface pairs meet 4.5; meaningful action/icon pairs meet 3. Include real existing failing combinations.
- [ ] Write Node chart test: computed palette values feed axes/legend/grid/doughnut borders after apply frame, storage failure does not suppress refresh and the final toggle wins. Expected: existing fixed chart colors fail.
- [ ] Implement shared palette contrast pairs, CSS aliases and paired status/soft backgrounds. Keep the CSS entry point; repair custom HTML buttons, text-muted, focus, mapping table/select and any inline color conflicts, without changing feature behavior.
- [ ] Implement chart computed colors and post-render refresh, retaining data/series order. Version CSS/interop asset URLs in App to prevent old cached scripts/styles.
- [ ] Run both Node files, palette/controller tests and whole .NET suite. Expected: green except explicitly conditional SQL integration skip; report any loader/environment failure separately.
- [ ] Review staged feature diffs for handlers/conditions/data changes and commit `fix: unify readable light-dark colors and chart styling`.

### Task 3: Browser acceptance and final review

**Interfaces:** consumes the complete theme lifecycle/palette from Tasks 1–2; produces `docs/THEME-VERIFICATION-2026-11-10.md` with actual revision, commands, outcomes, screenshots location, measured contrast and untested cases.

- [ ] Run the actual built Blazor UI against safe isolated fixture data. Prefer disposable SQL Server when available; never point bootstrap at existing legacy/IRM databases. If a UI-only host/fixture is needed, label backend/auth limitations explicitly rather than claiming production acceptance.
- [ ] Check import mapping with normal/skip rows, open select, enabled/disabled/loading buttons, and paired light/dark screenshots. Verify actual foreground/background contrast from browser styles, not JPEG pixels.
- [ ] Check shared menus/dialogs, login, representative pages and all route/theme source usage; F5/new tab/navigation/rapid toggle/storage fallback at desktop and a narrow viewport. Record precise gaps if environment blocks any case.
- [ ] Run restore/build Release, full .NET suite and both Node tests from solution. Expected: successful build and green tests, with conditional SQL skip or environment blockers explicitly recorded.
- [ ] Get one fresh whole-change code review against design/plan and Review Focus. Fix important findings with regression RED→GREEN and rerun affected/full checks; preserve deferred minor findings in report.
- [ ] Update plan/spec/report status, review staged diff and commit final verification. Finish with clean Git tree and report commit IDs, actual acceptance scope and any limitations. No installer rebuild/push/customer deployment in this task.

## Execution decisions

User has already approved design and explicitly requested implementation; proceed inline using executing-plans without another authorization round. Root instructions require preserving current branch/workspace; work directly on clean `develop` and keep task commits separable. The skill ledger lives under the ignored `.superpowers/` directory; final evidence is retained in committed verification documentation.
