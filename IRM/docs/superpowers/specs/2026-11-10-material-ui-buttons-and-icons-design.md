# Thiết kế Giao diện Material UI Tinh tế cho Cơ quan Nhà nước (Đơn giản · Tinh tế · Rõ ràng)

**Ngày:** 2026-11-10  
**Trạng thái:** Đã duyệt định hướng thiết kế  
**Mục tiêu:** Chuẩn hóa toàn bộ giao diện ứng dụng **IRM (Hệ thống Quản lý Người nước ngoài)** theo phong cách **Material UI (MUI) dành cho Cơ quan Nhà nước**: nghiêm túc, đơn giản, tinh tế, rõ ràng, không dùng màu sắc lòe loẹt hay biểu tượng Emoji cảm xúc, mọi nút bấm và bảng biểu đều tường minh, dễ đọc cho cán bộ nghiệp vụ.

---

## 1. Nguyên tắc Thiết kế cho Cơ quan Nhà nước

1. **Bảng màu Hành chính Công (Authoritative Navy Blue & Slate Neutral)**:
   - Bỏ dải màu tím neon/gradient lòe loẹt; chuyển sang tông **Xanh Hành chính đậm (`#1d4ed8` / `#1e40af`)** kết hợp **Xám Đá (`Slate #0f172a` / `#475569`)** tạo cảm giác tin cậy, trang trọng, dịu mắt khi làm việc nhiều giờ.
   - Nền trang sáng dịu `#f4f6f9`, bề mặt thẻ `#ffffff` có đường viền rõ ràng `#cbd5e1` (`1px solid`) để phân tách rành mạch từng vùng dữ liệu.
   - Chế độ tối (`Dark Mode`) dùng nền Slate chuẩn `#0f172a` / `#1e293b`, chữ `#f8fafc` / `#cbd5e1`, màu nhấn `#60a5fa`.
2. **Nút bấm (Buttons) chuẩn Material UI — Rõ ràng & Không rườm rà**:
   - Không viết hoa toàn bộ (`TextTransform = "none"`), cỡ chữ `13.5px`, `font-weight: 600`, bo góc chuẩn `6px`.
   - **Nút chính (`Variant.Filled`)**: Màu đơn sắc trang nhã (`#1d4ed8`), đổ bóng nhẹ `0 1px 2px rgba(15, 23, 42, 0.1)`, không dùng hiệu ứng nhún nhảy hay bóng phát sáng (`glow`).
   - **Nút phụ (`Variant.Outlined`)**: Viền rõ `1px solid var(--border-strong)`, nền trắng/tối phẳng, khi di chuột đổi nền nhẹ `rgba(29, 78, 216, 0.06)`.
   - **Nút thao tác trên bảng (`MudIconButton` Xem / Sửa / Xóa)**: Khung vuông bo góc `6px` (`30x30px`) có viền mảnh và nền màu nhạt theo chức năng (Xanh: Xem, Cam: Sửa, Đỏ: Xóa) để cán bộ thao tác chính xác, không bấm nhầm.
3. **Biểu tượng (Icons) chuẩn `Icons.Material.Outlined` & Loại bỏ 100% Emoji**:
   - **Xóa toàn bộ các ký tự Emoji (`👋`, `🪪`, `🏢`, `🏠`, `🔍`, `📊`, `📝`, `📥`, `📤`, `🗑️`, `✏️`, `➕`, `💾`, `🔄`, `🎓`, `👥`)** trên tiêu đề, nút bấm, lời chào và hộp thoại.
   - Chuyển toàn bộ sang bộ biểu tượng nét thanh **`Icons.Material.Outlined`** của Google Material UI.
4. **Sidebar & Bố cục Khung (`300px`) — Đơn giản, Không rối mắt**:
   - Chiều rộng Sidebar `300px`, nền sáng sạch sẽ (`#ffffff` ở Light Mode, `#0f172a` ở Dark Mode) phân tách với vùng nội dung bằng đường viền dọc mảnh `1px solid var(--border)`.
   - Mục menu bo góc nhẹ `6px` (`margin: 2px 10px`), mục đang chọn (`active`) có nền xanh nhạt (`rgba(29, 78, 216, 0.10)`), chữ và icon màu xanh đậm `#1d4ed8` (`#60a5fa` ở Dark Mode) kèm vạch chỉ thị dọc `3px` bên trái.
   - **Loại bỏ các huy hiệu trang trí gây rối mắt trên Sidebar** (`5 Tab`, `FDI/KKT`, `Hiện trường`, `v1.0.2`) để danh mục menu phẳng, thoáng và nghiêm túc.

---

## 2. Chi tiết Bảng màu (`IrmTheme.cs`) Đạt chuẩn Tương phản WCAG AA (>= 4.5:1)

### 2.1. `PaletteLight`
- `Primary = "#1d4ed8"`, `PrimaryDarken = "#1e40af"`, `PrimaryLighten = "#3b82f6"`, `PrimaryContrastText = "#ffffff"`
- `Secondary = "#3730a3"`, `SecondaryDarken = "#312e81"`, `SecondaryContrastText = "#ffffff"`
- `Info = "#0369a1"`, `InfoDarken = "#075985"`, `InfoContrastText = "#ffffff"`
- `Success = "#15803d"`, `SuccessDarken = "#166534"`, `SuccessContrastText = "#ffffff"`
- `Warning = "#b45309"`, `WarningDarken = "#92400e"`, `WarningContrastText = "#ffffff"`
- `Error = "#b91c1c"`, `ErrorDarken = "#991b1b"`, `ErrorContrastText = "#ffffff"`
- `Background = "#f4f6f9"`, `BackgroundGray = "#eef2f6"`, `Surface = "#ffffff"`
- `AppbarBackground = "rgba(255, 255, 255, 0.95)"`, `AppbarText = "#0f172a"`
- `DrawerBackground = "#ffffff"`, `DrawerText = "#334155"`, `DrawerIcon = "#475569"`
- `TextPrimary = "#0f172a"`, `TextSecondary = "#475569"`, `TextDisabled = "#64748b"`
- `Divider = "#cbd5e1"`, `LinesInputs = "#64748b"`, `ActionDefault = "#475569"`, `ActionDisabled = "#64748b"`, `ActionDisabledBackground = "#e2e8f0"`

### 2.2. `PaletteDark`
- `Primary = "#60a5fa"`, `PrimaryDarken = "#3b82f6"`, `PrimaryLighten = "#93c5fd"`, `PrimaryContrastText = "#0f172a"`
- `Secondary = "#a5b4fc"`, `SecondaryDarken = "#818cf8"`, `SecondaryContrastText = "#0f172a"`
- `Info = "#7dd3fc"`, `InfoDarken = "#38bdf8"`, `InfoContrastText = "#0f172a"`
- `Success = "#86efac"`, `SuccessDarken = "#4ade80"`, `SuccessContrastText = "#0f172a"`
- `Warning = "#fcd34d"`, `WarningDarken = "#fbbf24"`, `WarningContrastText = "#0f172a"`
- `Error = "#fca5a5"`, `ErrorDarken = "#f87171"`, `ErrorContrastText = "#0f172a"`
- `Background = "#0b1120"`, `BackgroundGray = "#1e293b"`, `Surface = "#0f172a"`
- `AppbarBackground = "rgba(15, 23, 42, 0.95)"`, `AppbarText = "#f8fafc"`
- `DrawerBackground = "#0f172a"`, `DrawerText = "#cbd5e1"`, `DrawerIcon = "#94a3b8"`
- `TextPrimary = "#f8fafc"`, `TextSecondary = "#cbd5e1"`, `TextDisabled = "#94a3b8"`
- `Divider = "#334155"`, `LinesInputs = "#94a3b8"`, `ActionDefault = "#cbd5e1"`, `ActionDisabled = "#94a3b8"`, `ActionDisabledBackground = "#1e293b"`
