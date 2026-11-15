# Thiết kế giao diện bộ cài offline IRM v1.0.2 R5 theo Landing Page (Phương án A)

Ngày: 15/11/2026 (Asia/Bangkok).

## Mục tiêu

Đồng bộ hoàn toàn giao diện bộ cài Windows offline R5 với màn hình đăng nhập (Landing Page) của ứng dụng (`IRM/Components/Shared/LoginView.razor` và `IRM/wwwroot/app.css`), áp dụng **Phương án A (Split 2 cột)** và hoàn thiện gói bàn giao tại `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/`.

## Bố cục và Bảng màu Landing Page

- **Bố cục Split 2 cột (`960×650`, `MinimumSize 920×650`)**:
  - **Nền toàn cửa sổ**: Gradient xanh olive góc 125° (`#C2D67C` 0% → `#9AB55C` 48% → `#6D8446` 100%) kết hợp 4 mảng đa giác vát khối bán trong suốt khớp SVG `.login-facets` của `LoginView.razor`.
  - **Header trên cùng**: Vẽ Công an hiệu (`logo-cand.png`) kích thước `56×56` bên trái tiêu đề `PHẦN MỀM QUẢN LÝ NGƯỜI NƯỚC NGOÀI` (`#16240B`) và phụ đề `Bộ cài đặt offline v1.0.2 • Không cần kết nối Internet khi máy đủ điều kiện` (`#293D14`) cùng đường kẻ phân tách mảnh.
  - **Cột trái**: Hiển thị nhãn bước (`BƯỚC 1/3`, `BƯỚC 2/3`, `BƯỚC 3/3`), bảng kiểm tra môi trường (`ListView`) hoặc biểu mẫu cấu hình (`Panel`), thanh tiến trình (`DarkProgressBar`), dòng trạng thái và nhóm 3 nút thao tác (`Đóng`, `Mở báo cáo`, nút chính).
  - **Cột phải**: Hiển thị nhân vật 3D Công an Nhân dân (`police-3d.png`) sắc nét trên bệ đổ bóng elip, kèm watermark chìm mờ phía góc dưới bên trái tương đồng trang đăng nhập.
- **Bảng màu**:
  - Gradient nền cửa sổ: `#C2D67C` (0%) → `#9AB55C` (48%) → `#6D8446` (100%); màu fallback `#B5CA6E`.
  - Surface / Panel: `#FAFCF5`
  - Input: `#FFFFFF`
  - Header / Tiêu đề: `#16240B` (fallback `#3F5223`)
  - Body text: `#1E2D10`
  - Muted: `#4D6136`
  - Border: `#C5D6A7`
  - Primary CTA: `#EA580C`, Hover `#C2410C`, Active `#9A3412`, chữ `#FFFFFF`
  - Trạng thái: Success `#15803D`, Warning `#8A4B0F`, Error `#BE123C`
- **Báo cáo HTML (`Save-EnvironmentReport`)**: Bảng màu olive sáng (`background:#eef5dc`, `color:#1e2d10`, `h1{color:#16240b}`, `th{background:#3f5223;color:white}`, `border:#c5d6a7`), giữ nguyên nền trắng khi in và HTML encode đầy đủ.

## Ràng buộc kỹ thuật

- Đọc ảnh `logo-cand.png` và `police-3d.png` từ `$PayloadRoot\app\wwwroot\images\` (hoặc fallback từ `IRM\wwwroot\images\` khi chạy từ source) bằng `[IO.File]::ReadAllBytes` qua `MemoryStream` và clone `Bitmap` rồi giải phóng stream ngay, tuyệt đối không giữ khóa file trên đĩa trong `$PayloadRoot`.
- Giữ nguyên tên các lớp `WindowTheme`, `DarkCheckBox`, `DarkButton`, `DarkListView`, `DarkProgressBar` để tương thích ngược, bổ sung `LandingForm : Form`.
- Giữ nguyên hoàn toàn worker cài đặt (`install.ps1`), logic kiểm tra môi trường (`windows-environment.ps1`), database schema, migrations, cổng `5050/5443/14331`, dịch vụ `IRM` và SQL media/CU27.
- Giữ ranh giới `$form = New-Object Windows.Forms.Form` để tương thích script dựng ảnh kiểm chứng `render-installer.ps1`.
- Không ghi đè hoặc xóa các gói đã bàn giao cũ (`01-HIEN-HANH/v1.0.2-DARK-R4-UNSIGNED-TEST/`, `02-LUU-TRU/`, `04-CHO-NGHIEM-THU/`).
