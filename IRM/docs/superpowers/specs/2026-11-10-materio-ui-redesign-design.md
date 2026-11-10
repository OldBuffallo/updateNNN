# Thiết kế Giao diện Materio Admin Template cho IRM (Phương án 1)

**Ngày:** 2026-11-10  
**Trạng thái:** Đã duyệt (Phương án 1 — Áp dụng Materio Design System lên Blazor Server + MudBlazor, Sidebar đồng bộ Sáng/Tối theo chuẩn mặc định của Materio)  
**Tham chiếu giao diện:** ThemeSelection — Materio Free ASP.NET Core MVC Admin Template (`https://themeselection.com/item/materio-free-aspnet-core-mvc-admin-template/`)

---

## 1. Mục tiêu

Tái thiết kế toàn diện giao diện ứng dụng **IRM (Immigration Report Manager)** theo đúng ngôn ngữ thiết kế **Materio Material Design Admin Template** của ThemeSelection trên nền tảng **ASP.NET Core Blazor Server (.NET 8) + MudBlazor v9.3.0** hiện có, bao gồm:
1. **Bảng màu Materio (Royal Purple & Indigo Slate)** hỗ trợ cả **Light Mode** (`#F4F5FA` / `#FFFFFF`) và **Dark Mode** (`#28243D` / `#312D4B`), đạt chuẩn độ tương phản nghiêm ngặt **WCAG AA (>= 4.5:1)** của bộ kiểm thử tự động `IrmThemePaletteTests` và `theme-ui.cjs`.
2. **Sidebar kiểu Materio**:
   - Ở chế độ Sáng (Light Mode): Nền sáng `#F4F5FA` hòa quyện với nền trang, chữ `#3A3541` / `#544F5A`.
   - Ở chế độ Tối (Dark Mode): Nền tối `#28243D`, chữ `#E7E3FC` / `#C9C4DF`.
   - Các mục điều hướng (`.irm-nav-link`) bo tròn nửa vòm bên phải (`border-radius: 0 50px 50px 0`, cách lề phải `14px`).
   - Mục đang chọn (`.irm-nav-link.active`) mang dải màu **gradient tím đặc trưng của Materio** (`linear-gradient(98deg, #C6A7FE, #9155FD 94%)` hoặc tương đương đạt chuẩn tương phản) kèm đổ bóng tím mềm (`0 4px 8px -4px rgba(145, 85, 253, 0.42)`).
   - Tiêu đề phân nhóm (`.nav-group-header`) có đường kẻ ngang mảnh kèm chữ in hoa nhỏ giống hệt `menu-header` của Materio.
3. **Floating Glass Navbar (`.irm-topbar`)**:
   - Thanh công cụ phía trên thu gọn nổi trong vùng nội dung với nền kính mờ (`backdrop-filter: blur(10px)`), bo góc `10px`, đổ bóng mềm Materio (`0 4px 18px -4px rgba(58, 53, 65, 0.1)`).
4. **Thẻ nội dung (Cards), KPI Cards, Bảng dữ liệu (`MudDataGrid`, `MudTable`, `MudSimpleTable`) và Hộp thoại (`MudDialog`)**:
   - Bo góc `10px–12px`, đổ bóng chuẩn Materio (`0 2px 10px 0 rgba(58, 53, 65, 0.1)` ở Light Mode, `0 2px 10px 0 rgba(15, 13, 23, 0.38)` ở Dark Mode).
   - Header bảng (`thead th`) nền `#F9FAFC` (Light) / `#3D3759` (Dark), chữ in hoa `11.5px`, `letter-spacing: 0.8px`, `font-weight: 700`.
5. **Trang Đăng nhập (`Login.razor`)**:
   - Thiết kế theo mẫu Auth Card của Materio: nền `#F4F5FA` (Light) / `#28243D` (Dark) điểm họa tiết hình học trang nhã, thẻ đăng nhập chính giữa (`max-width: 450px`, bo góc `12px`, đổ bóng Materio), logo Materio-style, lời chào thân thiện và nút đăng nhập tím Materio.

---

## 2. Chi tiết Bảng màu (`IrmTheme.cs`) & Kiểm chứng Độ tương phản WCAG AA

Mọi màu trong `IrmTheme.Create()` phải vượt qua 3 bài test trong `IRM.Tests/IrmThemePaletteTests.cs`:
- `BodyAndSecondaryText_AreReadableOnBothPageAndSurface`: `TextPrimary` và `TextSecondary` >= `4.5:1` trên `Background`, `Surface`, `BackgroundGray`; `ActionDefault` >= `3.0:1`.
- `FilledActions_HaveReadableLabelsInNormalAndHoverStates`: `ContrastText` >= `4.5:1` trên cả màu thường và màu `Darken`.
- `OutlinedAndTextActions_AreReadableAgainstPageAndSurface`: `Primary`, `Secondary`, `Info`, `Success`, `Warning`, `Error` >= `4.5:1` trên cả `Background` và `Surface`.

### 2.1. `PaletteLight` (Materio Light Mode)
| Thuộc tính | Mã màu | Giải thích & Độ tương phản |
|---|---|---|
| `Primary` | `#6e39cb` | Tím Hoàng gia Materio (Tương phản `6.73:1` trên `#ffffff`, `6.11:1` trên `#f4f5fa`) |
| `PrimaryDarken` | `#5b21b6` | Tím đậm khi hover (`8.90:1` với `#ffffff`) |
| `PrimaryLighten` | `#8c57ff` | Tím sáng Materio cho điểm nhấn / gradient |
| `PrimaryContrastText` | `#ffffff` | Chữ trắng trên nút Primary |
| `Secondary` | `#4f46e5` | Indigo (`6.29:1` trên `#ffffff`) |
| `SecondaryDarken` | `#4338ca` | Indigo đậm (`7.85:1` với `#ffffff`) |
| `SecondaryContrastText` | `#ffffff` | Chữ trắng |
| `Info` | `#0369a1` | Xanh lam thông tin (`5.93:1` trên `#ffffff`) |
| `InfoDarken` | `#075985` | Xanh lam đậm |
| `Success` | `#15803d` | Xanh lá thành công (`5.02:1` trên `#ffffff`) |
| `SuccessDarken` | `#166534` | Xanh lá đậm |
| `Warning` | `#b45309` | Cam cảnh báo (`5.02:1` trên `#ffffff`) |
| `WarningDarken` | `#92400e` | Cam đậm |
| `Error` | `#b91c1c` | Đỏ lỗi (`6.47:1` trên `#ffffff`) |
| `ErrorDarken` | `#991b1b` | Đỏ đậm |
| `Background` | `#f4f5fa` | Nền trang chuẩn Materio Light |
| `BackgroundGray` | `#eceef5` | Nền phụ Materio Light |
| `Surface` | `#ffffff` | Nền thẻ trắng tinh |
| `AppbarBackground` | `rgba(244, 245, 250, 0.88)` | Nền Topbar kính mờ |
| `AppbarText` | `#3a3541` | Chữ chính Materio (`#3A3541`) |
| `DrawerBackground` | `#f4f5fa` | Nền Sidebar sáng đồng bộ nền trang chuẩn Materio |
| `DrawerText` | `#3a3541` | Chữ Sidebar rõ nét |
| `DrawerIcon` | `#544f5a` | Icon Sidebar |
| `TextPrimary` | `#3a3541` | Chữ chính Materio (`10.28:1` trên `#eceef5`) |
| `TextSecondary` | `#544f5a` | Chữ phụ Materio (`6.85:1` trên `#eceef5`) |
| `TextDisabled` | `#787380` | Chữ vô hiệu hóa |
| `Divider` | `#dbdade` | Đường kẻ phân cách Materio |
| `LinesInputs` | `#787380` | Viền ô nhập liệu |
| `ActionDefault` | `#544f5a` | Nút hành động mặc định (`6.85:1` >= `3.0:1`) |
| `ActionDisabled` | `#787380` | Hành động vô hiệu hóa |
| `ActionDisabledBackground` | `#e6e5eb` | Nền nút vô hiệu hóa |

### 2.2. `PaletteDark` (Materio Dark Mode)
| Thuộc tính | Mã màu | Giải thích & Độ tương phản |
|---|---|---|
| `Primary` | `#b794f4` | Tím Lavender sáng Materio Dark (`5.33:1` trên `#312d4b`) |
| `PrimaryDarken` | `#9f7aea` | Tím Lavender đậm khi hover (`5.40:1` với `#181526`) |
| `PrimaryLighten` | `#d6bcfa` | Tím Lavender nhạt |
| `PrimaryContrastText` | `#181526` | Chữ tối đậm trên nút Primary ở Dark Mode |
| `Secondary` | `#c4b5fd` | Indigo sáng (`7.07:1` trên `#312d4b`) |
| `SecondaryDarken` | `#a78bfa` | Indigo vừa (`6.70:1` với `#181526`) |
| `SecondaryContrastText` | `#181526` | Chữ tối đậm |
| `Info` | `#7dd3fc` | Xanh sáng (`7.97:1` trên `#312d4b`) |
| `InfoDarken` | `#38bdf8` | Xanh vừa (`8.37:1` với `#181526`) |
| `InfoContrastText` | `#181526` | Chữ tối đậm |
| `Success` | `#86efac` | Xanh lá sáng (`9.46:1` trên `#312d4b`) |
| `SuccessDarken` | `#4ade80` | Xanh lá vừa (`10.4:1` với `#181526`) |
| `SuccessContrastText` | `#181526` | Chữ tối đậm |
| `Warning` | `#fcd34d` | Vàng cam sáng (`9.21:1` trên `#312d4b`) |
| `WarningDarken` | `#fbbf24` | Vàng cam vừa (`10.9:1` với `#181526`) |
| `WarningContrastText` | `#181526` | Chữ tối đậm |
| `Error` | `#fca5a5` | Đỏ hồng sáng (`6.72:1` trên `#312d4b`) |
| `ErrorDarken` | `#f87171` | Đỏ hồng vừa (`6.15:1` với `#181526`) |
| `ErrorContrastText` | `#181526` | Chữ tối đậm |
| `Background` | `#28243d` | Nền trang chuẩn Materio Dark |
| `BackgroundGray` | `#231f36` | Nền phụ Materio Dark |
| `Surface` | `#312d4b` | Nền thẻ chuẩn Materio Dark |
| `AppbarBackground` | `rgba(49, 45, 75, 0.88)` | Nền Topbar kính mờ Dark |
| `AppbarText` | `#e7e3fc` | Chữ chính Materio Dark |
| `DrawerBackground` | `#28243d` | Nền Sidebar tối đồng bộ nền trang chuẩn Materio Dark |
| `DrawerText` | `#c9c4df` | Chữ Sidebar Dark |
| `DrawerIcon` | `#c9c4df` | Icon Sidebar Dark |
| `TextPrimary` | `#e7e3fc` | Chữ chính Materio Dark (`10.4:1` trên `#312d4b`) |
| `TextSecondary` | `#c9c4df` | Chữ phụ Materio Dark (`7.66:1` trên `#312d4b`) |
| `TextDisabled` | `#9e99b8` | Chữ vô hiệu hóa Dark |
| `Divider` | `#474360` | Đường kẻ phân cách Materio Dark |
| `LinesInputs` | `#9e99b8` | Viền ô nhập liệu Dark |
| `ActionDefault` | `#c9c4df` | Hành động mặc định Dark |
| `ActionDisabled` | `#9e99b8` | Hành động vô hiệu hóa Dark |
| `ActionDisabledBackground` | `#3d3759` | Nền nút vô hiệu hóa Dark |

---

## 3. Các Thành phần Giao diện được Nâng cấp

### 3.1. `IRM/IRM/wwwroot/app.css` — Hệ thống CSS Tokens & Component Styles của Materio
1. **CSS Variables (`:root` và `[data-theme="dark"]`)**:
   - Cập nhật `--bg-body`, `--bg-surface`, `--bg-surface-variant`, `--bg-muted`, `--bg-sidebar`, `--text-primary`, `--text-secondary`, `--text-sidebar`, `--text-sidebar-active`, `--border`, `--shadow-*`, `--chart-1..5` theo hệ màu Materio.
   - Bổ sung biến `--materio-primary-gradient: linear-gradient(98deg, #8c57ff, #6e39cb 94%)` (Light) và `linear-gradient(98deg, #c6a7fe, #9155fd 94%)` (Sidebar pill active).
2. **Sidebar (`.irm-drawer`, `.irm-nav-link`, `.nav-group-header`)**:
   - `.irm-drawer`: bỏ viền cứng, đồng bộ màu `--bg-sidebar` (`#f4f5fa` ở Light, `#28243d` ở Dark).
   - `.irm-nav-link`: `margin: 2px 14px 2px 0 !important`, `border-radius: 0 50px 50px 0 !important`, `padding: 9px 18px 9px 22px !important`, chuyển màu mượt `0.2s`.
   - `.irm-nav-link.active`: nền `linear-gradient(98deg, #8c57ff, #6e39cb 94%)`, chữ `#ffffff !important`, icon `#ffffff !important`, `box-shadow: 0 4px 8px -4px rgba(110, 57, 203, 0.45)`.
   - `.nav-group-header`: hiển thị đường gạch ngang mảnh bên trái giống `menu-header` của Materio (`— TỔNG QUAN`, `— HỒ SƠ & DỮ LIỆU`...).
   - `.drawer-title`, `.drawer-sub`, `.drawer-footer`: dùng biến màu động theo theme (`var(--text-primary)` / `var(--text-secondary)`) thay vì hardcode `rgba(255,255,255,...)` để hiển thị sắc nét trên cả nền Sidebar Sáng và Tối.
3. **Topbar nổi (`.irm-topbar`)**:
   - Tạo hiệu ứng Floating Navbar của Materio: cách lề trên và hai bên một khoảng nhẹ hoặc bo góc dưới mềm mại, nền kính mờ `backdrop-filter: blur(10px)`, đổ bóng `0 4px 18px -4px rgba(58, 53, 65, 0.08)`.
4. **Thẻ KPI, Bảng dữ liệu & Hộp thoại**:
   - `.kpi-card`, `.chart-card`, `.mud-table-container`, `.mud-data-grid`, `.mud-simple-table`: bo góc `10px`, viền mảnh `1px solid var(--border)`, đổ bóng chuẩn Materio `0 2px 10px 0 rgba(58, 53, 65, 0.08)`.
   - Header bảng (`thead th`, `.mud-table-head .mud-table-cell`): nền `#f9fafc` (Light) / `#383354` (Dark), chữ `#3a3541` / `#e7e3fc`.

### 3.2. `IRM/IRM/Components/Shared/MainLayout.razor` & `IRM/IRM/Features/Administration/Login.razor`
- Giữ nguyên 100% các class/id hợp đồng kiểm thử (`.irm-theme-loading`, `.theme-toggle-btn`, `.user-pill`, `.logout-menu-item`, `#loginUser`, `#loginPass`, nút `Đăng nhập`, `.irm-nav-link`).
- Tinh chỉnh header logo trong `MainLayout.razor` và thẻ đăng nhập trong `Login.razor` mang phong cách Materio hiện đại, trang nhã.

---

## 4. Các Ràng buộc Không được Phá vỡ
- Giữ nguyên toàn bộ route, tính năng nghiệp vụ và phân quyền RBAC.
- Vượt qua 100% unit tests trong `IRM.Tests` (đặc biệt là `IrmThemePaletteTests`, `IrmThemeControllerTests`, `ArchitectureCompatibilityTests`) và browser tests (`theme-preference.test.cjs`, `theme-chart.test.cjs`).

