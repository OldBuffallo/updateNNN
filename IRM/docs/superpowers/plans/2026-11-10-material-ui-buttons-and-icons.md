# Material UI Government-Grade Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Transform the IRM interface into a clean, refined, high-clarity Material UI (MUI) design tailored for a Vietnamese state agency ("Cơ quan nhà nước — Đơn giản, Tinh tế, Rõ ràng") by adopting an authoritative Navy/Slate palette, standardizing MUI button & icon-button styling, switching all icons to `Icons.Material.Outlined`, removing decorative sidebar badges, and stripping informal emojis across all pages.

**Architecture:** Update `IrmTheme.Create()` in `IRM/IRM/Components/Theming/IrmTheme.cs` with the Government Navy (`#1d4ed8` / `#60a5fa`) and Crisp Slate (`#f4f6f9` / `#ffffff` in Light; `#0b1120` / `#0f172a` in Dark) palette plus `DefaultBorderRadius = "6px"`. Refine `IRM/IRM/wwwroot/app.css` so buttons, tables, cards, sidebar items, and login screens use clean borders, subtle shadows, and zero flashy gradients/animations. Convert `Icons.Material.Filled` to `Icons.Material.Outlined` and remove redundant emojis across `IRM/IRM/**/*.razor`.

**Tech Stack:** ASP.NET Core Blazor Server (.NET 8), MudBlazor v9.3.0 (`Icons.Material.Outlined`), CSS3 Custom Properties, xUnit, Node.js test runner.

**Spec:** `IRM/docs/superpowers/specs/2026-11-10-material-ui-buttons-and-icons-design.md`

## Global Constraints

- Run .NET commands from `IRM/`; solution file is `IRM/IRM.sln`.
- All color pairs in `IrmTheme.Create()` MUST pass `IrmThemePaletteTests` WCAG AA (`>= 4.5:1` contrast).
- Preserve all selectors and exact text contracts asserted by `IRM.Tests/Browser/theme-ui.cjs` (`Đăng nhập`, `Tiếp tục`, `Bắt đầu Import`, `Đăng xuất`, `Hủy`, `.theme-toggle-btn`, `.user-pill`, `.logout-menu-item`, `.irm-nav-link`).
- Maintain `ArchitectureCompatibilityTests` invariants.

## Review Focus

- `DrawerText` and `DrawerIcon` on `DrawerBackground` (`#ffffff` in Light mode, `#0f172a` in Dark mode) must maintain `>= 4.5:1` contrast.
- Active sidebar link (`.irm-nav-link.active`) in Light mode using a soft blue fill (`rgba(29, 78, 216, 0.10)`) must use `#1d4ed8` (not `#ffffff`) for text and icon so labels remain sharp and readable on the light sidebar!
- Outlined buttons (`Variant.Outlined`) must have a visible `1px solid var(--border-strong)` border and clear text contrast in both modes.
- Removing emojis from `Companies.razor` and `ImportMappingSection.razor` must either keep `theme-ui.cjs` in sync or preserve the exact strings tested by `theme-ui.cjs`. (We will update `theme-ui.cjs` lines 78, 136, 137 alongside `Companies.razor` and `ImportMappingSection.razor` so both code and test use clean formal Vietnamese labels).
- No `Icons.Material.Outlined` member name mismatch during compilation.

---

### Task 1: Government Navy & Slate Palette in `IrmTheme.cs`, Clean Sidebar in `NavMenu.razor` & MUI Styling in `app.css`

**Files:**
- Modify: `IRM/IRM/Components/Theming/IrmTheme.cs:1-92`
- Modify: `IRM/IRM.Tests/IrmThemePaletteTests.cs:1-76`
- Modify: `IRM/IRM/Components/Shared/NavMenu.razor:1-209`
- Modify: `IRM/IRM/wwwroot/app.css`

**Interfaces:**
- Consumes: `MudBlazor.MudTheme`, `MudBlazor.PaletteLight`, `MudBlazor.PaletteDark`
- Produces: `IrmTheme.Create()` with Government Navy (`#1d4ed8` / `#60a5fa`), `DrawerBackground = "#ffffff"` (Light) / `"#0f172a"` (Dark), `DrawerWidthLeft = "300px"`, `DefaultBorderRadius = "6px"`, and clean MUI button/card/sidebar CSS rules in `app.css`.

- [ ] **Step 1: Update `MaterioPalette_UsesExpectedCanvasAndReadableDrawerTokens` in `IRM/IRM.Tests/IrmThemePaletteTests.cs` to pin the Government Navy & Slate tokens**

```csharp
    [Theory]
    [InlineData(false, "#f4f6f9", "#ffffff", "#1d4ed8", "#ffffff")]
    [InlineData(true, "#0b1120", "#0f172a", "#60a5fa", "#0f172a")]
    public void GovernmentMvpPalette_UsesExpectedCanvasAndReadableDrawerTokens(
        bool dark, string expectedBackground, string expectedSurface, string expectedPrimary, string expectedDrawer)
    {
        var palette = Palette(dark);
        Assert.Equal(new MudColor(expectedBackground), palette.Background);
        Assert.Equal(new MudColor(expectedSurface), palette.Surface);
        Assert.Equal(new MudColor(expectedPrimary), palette.Primary);
        Assert.Equal(new MudColor(expectedDrawer), palette.DrawerBackground);
        Readable(palette.DrawerText, palette.DrawerBackground, 4.5);
        Readable(palette.DrawerIcon, palette.DrawerBackground, 4.5);
    }
```

- [ ] **Step 2: Run `IrmThemePaletteTests` to verify it fails (RED)**

Run: `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~IrmThemePaletteTests" --nologo` (from `IRM/`)
Expected: FAIL.

- [ ] **Step 3: Implement Government Navy & Slate palette in `IRM/IRM/Components/Theming/IrmTheme.cs` and clean MUI CSS in `IRM/IRM/wwwroot/app.css` & `NavMenu.razor`**

1. Update `IrmTheme.cs` with the exact `PaletteLight` and `PaletteDark` values from Spec Section 2 and `DefaultBorderRadius = "6px"`, `DrawerWidthLeft = "300px"`.
2. In `NavMenu.razor`, remove decorative pill badges (`5 Tab`, `FDI/KKT`, `Hiện trường`, `v1.0.2`) so menu items are clean and formal.
3. In `app.css`:
   - Set `--radius-sm: 4px; --radius: 6px; --radius-lg: 8px; --radius-xl: 10px;`
   - Style `.irm-drawer` with `border-right: 1px solid var(--border) !important;`
   - Style `.irm-nav-link` with `margin: 2px 10px !important; padding: 8px 14px !important; border-radius: 6px !important;`
   - Style `.irm-nav-link.active` with `background: rgba(var(--mud-palette-primary-rgb), 0.10) !important; color: var(--primary) !important; font-weight: 600; box-shadow: none;` and a crisp `3px` left indicator bar (`background: var(--primary)`), with `.irm-nav-link.active .mud-nav-link-icon, .irm-nav-link.active span { color: var(--primary) !important; }`.
   - Style Material UI buttons (`.mud-button-root`, `.mud-button-filled`, `.mud-button-outlined`, `.mud-icon-button`):
     - `.mud-button-root`: `text-transform: none !important; font-weight: 600 !important; font-size: 13.5px !important; letter-spacing: 0.01em !important; border-radius: 6px !important; padding: 6px 14px; box-shadow: none;`
     - `.mud-button-filled`: subtle elevation `0 1px 2px rgba(15, 23, 42, 0.12)`
     - `.mud-button-outlined`: `border-width: 1px !important;`
     - Table `.mud-icon-button`: `width: 30px; height: 30px; padding: 5px; border-radius: 6px; border: 1px solid transparent;` with subtle tonal hover.

- [ ] **Step 4: Run `IrmTheme` unit tests to verify GREEN**

Run: `dotnet test IRM.Tests/IRM.Tests.csproj --filter "FullyQualifiedName~IrmTheme" --nologo` (from `IRM/`)
Expected: PASS (17/17).

- [ ] **Step 5: Commit Task 1**

```powershell
git add IRM/IRM/Components/Theming/IrmTheme.cs IRM/IRM.Tests/IrmThemePaletteTests.cs IRM/IRM/Components/Shared/NavMenu.razor IRM/IRM/wwwroot/app.css
git commit -m "feat(theme): apply government-grade Material UI palette, buttons, and clean sidebar"
```

---

### Task 2: Convert All Icons to `Icons.Material.Outlined` & Strip Informal Emojis Across All Pages

**Files:**
- Modify: `IRM/IRM/Components/Shared/MainLayout.razor`
- Modify: `IRM/IRM/Features/**/*.razor`
- Modify: `IRM/IRM/Features/Administration/Admin.razor.cs`
- Modify: `IRM/IRM.Tests/Browser/theme-ui.cjs`

**Interfaces:**
- Consumes: `MudBlazor.Icons.Material.Outlined`
- Produces: Formal, emoji-free Vietnamese government UI with crisp Material Outlined icons across all pages.

- [ ] **Step 1: Replace `Icons.Material.Filled.` with `Icons.Material.Outlined.` across all `.razor` and `.cs` files in `IRM/IRM/`**

- [ ] **Step 2: Strip informal emojis from page headers, buttons, alerts, dialogs, and search result cards in `IRM/IRM/Features/**/*.razor` and `MainLayout.razor`, and align `theme-ui.cjs` button/dialog text selectors**

- [ ] **Step 3: Build Release and run all xUnit + Node.js tests**

Run from `IRM/`:
1. `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --nologo`
2. `node --test IRM.Tests/Browser/theme-preference.test.cjs IRM.Tests/Browser/theme-chart.test.cjs`
Expected: `137 Passed, 0 Failed` and `12/12 PASS`.

- [ ] **Step 4: Commit Task 2 and restart server on port 5050**

```powershell
git add IRM/IRM IRM/IRM.Tests/Browser/theme-ui.cjs IRM/docs/superpowers/specs/2026-11-10-material-ui-buttons-and-icons-design.md IRM/docs/superpowers/plans/2026-11-10-material-ui-buttons-and-icons.md
git commit -m "style(ui): use Material Outlined icons and remove informal emojis for state agency style"
```
