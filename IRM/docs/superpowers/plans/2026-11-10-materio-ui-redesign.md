# Materio UI Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Redesign the IRM web application's theme palette, layout shell, sidebar navigation, cards, tables, and login screen to match the ThemeSelection Materio Free ASP.NET Core MVC Admin Template (`https://themeselection.com/item/materio-free-aspnet-core-mvc-admin-template/`) in both Light and Dark modes while preserving all WCAG AA contrast and DOM contracts.

**Architecture:** Update `IrmTheme.Create()` in `IRM/IRM/Components/Theming/IrmTheme.cs` with Materio's Royal Purple (`#6e39cb` / `#b794f4`), light canvas (`#f4f5fa` / `#ffffff`), and dark canvas (`#28243d` / `#312d4b`) tokens verified against `IrmThemePaletteTests`. Restyle `IRM/IRM/wwwroot/app.css`, `IRM/IRM/Components/Shared/MainLayout.razor`, and `IRM/IRM/Features/Administration/Login.razor` to deliver Materio's half-rounded gradient sidebar pills (`border-radius: 0 50px 50px 0`), section header dividers, floating glass topbar, soft elevated cards/tables, and centered Materio authentication card.

**Tech Stack:** ASP.NET Core Blazor Server (.NET 8), MudBlazor v9.3.0, CSS3 Custom Properties, xUnit, Node.js test runner.

**Spec:** `IRM/docs/superpowers/specs/2026-11-10-materio-ui-redesign-design.md`

## Global Constraints

- Run .NET commands from `IRM/`; solution file is `IRM/IRM.sln`.
- Every color pair in `IrmTheme.Create()` (`PaletteLight` and `PaletteDark`) MUST satisfy `IrmThemePaletteTests` WCAG contrast thresholds (`4.5:1` for text/filled/outlined actions, `3.0:1` for `ActionDefault`).
- Preserve all DOM selectors tested by `IRM.Tests/Browser/theme-ui.cjs`: `.irm-theme-loading`, `.theme-toggle-btn`, `.user-pill`, `.logout-menu-item`, `.irm-nav-link`, `#loginUser`, `#loginPass`, and button labels (`Đăng nhập`, `Tiếp tục`, `Đăng xuất`).
- Sidebar uses Materio's adaptive background (`#f4f5fa` in Light mode, `#28243d` in Dark mode) with readable text and icons in both modes.
- Maintain `ArchitectureCompatibilityTests` invariants (no `IRM.Core` entity fields or `IRM.Infrastructure` fields in `IRM.Components.Pages` classes).

## Review Focus

- Sidebar text and section headers in Light Mode becoming unreadable if `rgba(255, 255, 255, ...)` is left hardcoded in `.drawer-title`, `.drawer-sub`, `.nav-section-label`, or `.drawer-footer-text` when `DrawerBackground` changes from `#0f172a` to `#f4f5fa`.
- Active sidebar link (`.irm-nav-link.active`) icon or badge losing contrast against the Materio purple gradient background (`linear-gradient(98deg, #8c57ff, #6e39cb 94%)`).
- Dark mode filled buttons failing `IrmThemePaletteTests.FilledActions_HaveReadableLabelsInNormalAndHoverStates` if `PrimaryDarken` or `SecondaryDarken` drops below `4.5:1` against `#181526`.
- Outlined/text buttons in Light mode failing `IrmThemePaletteTests.OutlinedAndTextActions_AreReadableAgainstPageAndSurface` against `#f4f5fa` (which is slightly darker than `#ffffff`, requiring `Primary` luminance <= `0.161`).
- Login card input fields and labels in Dark mode (`html[data-theme="dark"]`) losing border/background distinction on `#312d4b`.

---

### Task 1: Materio Palette in `IrmTheme.cs` & Palette Unit Tests

**Files:**
- Modify: `IRM/IRM/Components/Theming/IrmTheme.cs:1-87`
- Modify: `IRM/IRM.Tests/IrmThemePaletteTests.cs:1-76`

**Interfaces:**
- Consumes: `MudBlazor.MudTheme`, `MudBlazor.PaletteLight`, `MudBlazor.PaletteDark`
- Produces: `IrmTheme.Create()` returning Materio Light (`Background = "#f4f5fa"`, `Surface = "#ffffff"`, `DrawerBackground = "#f4f5fa"`, `Primary = "#6e39cb"`) and Materio Dark (`Background = "#28243d"`, `Surface = "#312d4b"`, `DrawerBackground = "#28243d"`, `Primary = "#b794f4"`) palettes.

- [ ] **Step 1: Add Materio palette identity & drawer readability assertions to `IRM/IRM.Tests/IrmThemePaletteTests.cs`**

Add a test method `MaterioPalette_UsesExpectedCanvasAndReadableDrawerTokens(bool dark)` in `IRM/IRM.Tests/IrmThemePaletteTests.cs`:
```csharp
    [Theory]
    [InlineData(false, "#f4f5fa", "#ffffff", "#6e39cb")]
    [InlineData(true, "#28243d", "#312d4b", "#b794f4")]
    public void MaterioPalette_UsesExpectedCanvasAndReadableDrawerTokens(
        bool dark, string expectedBackground, string expectedSurface, string expectedPrimary)
    {
        var palette = Palette(dark);
        Assert.Equal(new MudColor(expectedBackground), palette.Background);
        Assert.Equal(new MudColor(expectedSurface), palette.Surface);
        Assert.Equal(new MudColor(expectedPrimary), palette.Primary);
        Assert.Equal(new MudColor(expectedBackground), palette.DrawerBackground);
        Readable(palette.DrawerText, palette.DrawerBackground, 4.5);
        Readable(palette.DrawerIcon, palette.DrawerBackground, 4.5);
    }
```

- [ ] **Step 2: Run xUnit test to verify it fails against the old blue palette**

Run: `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~IrmThemePaletteTests" --nologo` (from `IRM/`)
Expected: FAIL on `MaterioPalette_UsesExpectedCanvasAndReadableDrawerTokens`.

- [ ] **Step 3: Update `IrmTheme.Create()` in `IRM/IRM/Components/Theming/IrmTheme.cs` with the Materio Light and Dark palettes**

Set `PaletteLight` and `PaletteDark` in `IRM/IRM/Components/Theming/IrmTheme.cs` to the exact values defined in Spec Section 2.1 and Section 2.2:
- `PaletteLight`: `Primary = "#6e39cb"`, `PrimaryDarken = "#5b21b6"`, `PrimaryLighten = "#8c57ff"`, `PrimaryContrastText = "#ffffff"`, `Secondary = "#4f46e5"`, `SecondaryDarken = "#4338ca"`, `SecondaryContrastText = "#ffffff"`, `Info = "#0369a1"`, `InfoDarken = "#075985"`, `InfoContrastText = "#ffffff"`, `Success = "#15803d"`, `SuccessDarken = "#166534"`, `SuccessContrastText = "#ffffff"`, `Warning = "#b45309"`, `WarningDarken = "#92400e"`, `WarningContrastText = "#ffffff"`, `Error = "#b91c1c"`, `ErrorDarken = "#991b1b"`, `ErrorContrastText = "#ffffff"`, `Background = "#f4f5fa"`, `BackgroundGray = "#eceef5"`, `Surface = "#ffffff"`, `AppbarBackground = "rgba(244, 245, 250, 0.88)"`, `AppbarText = "#3a3541"`, `DrawerBackground = "#f4f5fa"`, `DrawerText = "#3a3541"`, `DrawerIcon = "#544f5a"`, `TextPrimary = "#3a3541"`, `TextSecondary = "#544f5a"`, `TextDisabled = "#787380"`, `Divider = "#dbdade"`, `LinesInputs = "#787380"`, `ActionDefault = "#544f5a"`, `ActionDisabled = "#787380"`, `ActionDisabledBackground = "#e6e5eb"`.
- `PaletteDark`: `Primary = "#b794f4"`, `PrimaryDarken = "#9f7aea"`, `PrimaryLighten = "#d6bcfa"`, `PrimaryContrastText = "#181526"`, `Secondary = "#c4b5fd"`, `SecondaryDarken = "#a78bfa"`, `SecondaryContrastText = "#181526"`, `Info = "#7dd3fc"`, `InfoDarken = "#38bdf8"`, `InfoContrastText = "#181526"`, `Success = "#86efac"`, `SuccessDarken = "#4ade80"`, `SuccessContrastText = "#181526"`, `Warning = "#fcd34d"`, `WarningDarken = "#fbbf24"`, `WarningContrastText = "#181526"`, `Error = "#fca5a5"`, `ErrorDarken = "#f87171"`, `ErrorContrastText = "#181526"`, `Background = "#28243d"`, `BackgroundGray = "#231f36"`, `Surface = "#312d4b"`, `AppbarBackground = "rgba(49, 45, 75, 0.88)"`, `AppbarText = "#e7e3fc"`, `DrawerBackground = "#28243d"`, `DrawerText = "#c9c4df"`, `DrawerIcon = "#c9c4df"`, `TextPrimary = "#e7e3fc"`, `TextSecondary = "#c9c4df"`, `TextDisabled = "#9e99b8"`, `Divider = "#474360"`, `LinesInputs = "#9e99b8"`, `ActionDefault = "#c9c4df"`, `ActionDisabled = "#9e99b8"`, `ActionDisabledBackground = "#3d3759"`.

- [ ] **Step 4: Run `IrmThemePaletteTests` and `IrmThemeControllerTests` to verify all pass**

Run: `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~IrmTheme" --nologo` (from `IRM/`)
Expected: PASS (8/8 tests passing).

- [ ] **Step 5: Commit Task 1**

```powershell
git add IRM/IRM/Components/Theming/IrmTheme.cs IRM/IRM.Tests/IrmThemePaletteTests.cs
git commit -m "feat(theme): update IrmTheme palette to Materio Royal Purple and Slate canvas"
```

---

### Task 2: Materio Layout Shell, Half-Rounded Gradient Sidebar, Cards, Tables & Login Styling

**Files:**
- Modify: `IRM/IRM/wwwroot/app.css:1-2465`
- Modify: `IRM/IRM/Components/Shared/MainLayout.razor:1-180`
- Modify: `IRM/IRM/Features/Administration/Login.razor:1-25`

**Interfaces:**
- Consumes: CSS variables generated by `MudThemeProvider` from `IrmTheme.Create()`
- Produces: Materio visual styling across `.irm-drawer`, `.irm-nav-link`, `.nav-group-header`, `.irm-topbar`, `.kpi-card`, `.chart-card`, `.mud-table-container`, `.mud-data-grid`, `.mud-simple-table`, and `.login-screen`.

- [ ] **Step 1: Update CSS tokens, Sidebar, Topbar, Cards, Tables, and Login styles in `IRM/IRM/wwwroot/app.css`**

Update `IRM/IRM/wwwroot/app.css` so that:
1. `:root` and `[data-theme="dark"]` define Materio tokens:
   - `--materio-gradient: linear-gradient(98deg, #8c57ff, #6e39cb 94%)`
   - `--materio-shadow-active: 0 4px 8px -4px rgba(110, 57, 203, 0.45)`
   - Light mode sidebar tokens: `--bg-sidebar: #f4f5fa`, `--bg-sidebar-hover: rgba(58, 53, 65, 0.06)`, `--bg-sidebar-active: linear-gradient(98deg, #8c57ff, #6e39cb 94%)`, `--text-sidebar: #3a3541`, `--text-sidebar-muted: #544f5a`, `--text-sidebar-active: #ffffff`, `--border-sidebar: rgba(58, 53, 65, 0.12)`.
   - Dark mode sidebar tokens (`[data-theme="dark"]`): `--bg-sidebar: #28243d`, `--bg-sidebar-hover: rgba(231, 227, 252, 0.06)`, `--text-sidebar: #c9c4df`, `--text-sidebar-muted: #9e99b8`, `--text-sidebar-active: #ffffff`, `--border-sidebar: rgba(231, 227, 252, 0.12)`.
   - Materio card shadows: `--shadow-sm: 0 2px 10px 0 rgba(58, 53, 65, 0.08)`, `--shadow: 0 4px 18px -4px rgba(58, 53, 65, 0.1)`, `--radius: 10px`, `--radius-lg: 12px`.
2. Sidebar items (`.irm-drawer .mud-nav-link`, `.irm-nav-link`):
   - Right-rounded pill geometry: `margin: 2px 14px 2px 0 !important; padding: 9px 16px 9px 22px !important; border-radius: 0 50px 50px 0 !important; color: var(--text-sidebar) !important;`
   - Active state (`.irm-drawer .mud-nav-link.active`, `.irm-nav-link.active`): `background: var(--materio-gradient) !important; color: #ffffff !important; box-shadow: var(--materio-shadow-active) !important; font-weight: 600 !important;` with icon and child spans inheriting `#ffffff`.
   - Section headers (`.nav-group-header`): Materio horizontal dash prefix (`::before` line `width: 18px; height: 1px; background: var(--border-sidebar)`) and `.nav-section-label` using `color: var(--text-sidebar-muted)`.
   - Drawer header/footer text (`.drawer-title`, `.drawer-sub`, `.drawer-footer`, `.drawer-footer-text`, `.drawer-version-tag`, `.nav-key-hint`): use `var(--text-primary)`, `var(--text-sidebar-muted)`, and `var(--border-sidebar)` instead of hardcoded white `rgba(255,255,255,...)`.
3. Topbar (`.irm-topbar`):
   - Floating glass bar appearance with `backdrop-filter: blur(10px)`, subtle bottom/card shadow `0 4px 18px -4px rgba(58, 53, 65, 0.08)`, and Materio purple brand badge.
4. Tables & Cards (`.kpi-card`, `.chart-card`, `.mud-table-container`, `.mud-data-grid`, `.mud-simple-table`):
   - `border-radius: 10px`, `box-shadow: var(--shadow-sm)`, table headers (`thead th`, `.mud-table-head .mud-table-cell`, `.mud-data-grid-header-cell`) using `#f9fafc` in Light mode and `#383354` in Dark mode with `color: var(--text-primary)`.
5. Login screen (`.login-bg`, `.login-card`, `.btn-login`):
   - Clean Materio auth page background (`#f4f5fa` in Light mode, `#28243d` in Dark mode with subtle decorative radial glow and SVG ring accents), elevated `.login-card` (`#ffffff` in Light mode, `#312d4b` in Dark mode, `border-radius: 12px`, `box-shadow: 0 6px 24px -4px rgba(58, 53, 65, 0.14)`), and `.btn-login` using `var(--materio-gradient)`.

- [ ] **Step 2: Refine Materio brand header in `IRM/IRM/Components/Shared/MainLayout.razor` and auth card in `IRM/IRM/Features/Administration/Login.razor`**

Update the brand mark in `MainLayout.razor` and `Login.razor` to display Materio's clean geometric brand header while keeping all existing element IDs (`#loginUser`, `#loginPass`), classes (`.theme-toggle-btn`, `.user-pill`, `.logout-menu-item`, `.irm-nav-link`), and button texts (`Đăng nhập`, `Đăng xuất`).

- [ ] **Step 3: Build solution in Release mode and run all unit & browser tests**

Run from `IRM/`:
1. `dotnet build IRM.sln -c Release --nologo`
2. `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --nologo`
3. `node --test IRM.Tests/Browser/theme-preference.test.cjs IRM.Tests/Browser/theme-chart.test.cjs`
Expected: `0 Warning(s), 0 Error(s)`, all xUnit tests (including `IrmThemePaletteTests` and `ArchitectureCompatibilityTests`) PASS, and all 12 Node.js theme tests PASS.

- [ ] **Step 4: Restart the local server on port 5050 and commit Task 2**

```powershell
git add IRM/IRM/wwwroot/app.css IRM/IRM/Components/Shared/MainLayout.razor IRM/IRM/Features/Administration/Login.razor IRM/docs/superpowers/specs/2026-11-10-materio-ui-redesign-design.md IRM/docs/superpowers/plans/2026-11-10-materio-ui-redesign.md
git commit -m "feat(ui): redesign layout, sidebar, cards, tables and login to Materio template"
```

