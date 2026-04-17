# Immigration Report Manager v2.0

Hệ thống quản lý báo cáo người lao động nước ngoài — phiên bản web application hiện đại, thay thế hoàn toàn phần mềm desktop cũ.

## Demo trực tiếp

**[Xem demo tại đây →](https://YOUR_GITHUB_USERNAME.github.io/immigration-reportmanager-master/)**

> Đăng nhập với tài khoản: `admin` / mật khẩu: bất kỳ

## Tính năng chính

| Tính năng | Mô tả |
|---|---|
| 📊 Dashboard | KPI cards, biểu đồ quốc tịch/GPLĐ/hết hạn, bảng cảnh báo |
| 🏢 Quản lý Công ty | Danh sách, thêm/sửa, lọc theo lĩnh vực |
| 👥 Quản lý Nhân viên | Theo dõi lao động nước ngoài, hộ chiếu, visa |
| 📤 Import Excel | Wizard 4 bước: upload → ghép cột → xem trước → kết quả |
| 🔍 Tìm kiếm toàn cục | Tìm theo tên, hộ chiếu, công ty, quốc tịch |
| 📝 Báo cáo tùy chỉnh | Chọn cột, điều kiện lọc, nhóm, xuất Excel/PDF |
| ⚙️ Quản trị | Tài khoản, danh mục, nhật ký hệ thống |

## So sánh phiên bản

| | Phần mềm cũ (Desktop) | IRM v2.0 (Web) |
|---|---|---|
| Truy cập | Cài đặt từng máy | Trình duyệt, không cài đặt |
| Nhiều người dùng | ❌ | ✅ |
| Dark Mode | ❌ | ✅ |
| Import Excel | Thủ công | 4 bước tự động |
| Tìm kiếm | Hạn chế | Toàn cục real-time |
| Báo cáo | Cố định | Tùy chỉnh linh hoạt |

## Tech Stack

- **Frontend:** ASP.NET Core Blazor Server (.NET 8)
- **UI:** MudBlazor v9.3 (Material Design)
- **Demo:** HTML5 / CSS3 / JavaScript thuần
- **Deploy:** GitHub Pages (demo) · Windows Server (production)

## Chạy locally (Blazor App)

```bash
cd IRM
dotnet run
# Mở http://localhost:5023
```

**Yêu cầu:** .NET 8 SDK — tải tại [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

## Deploy lên GitHub Pages

1. Push code lên GitHub (nhánh `main`)
2. Vào **Settings → Pages → Source:** chọn `GitHub Actions`
3. GitHub Actions tự động deploy — URL sẽ là:
   `https://<username>.github.io/<repo-name>/`

## Deploy production (Windows Server)

Xem hướng dẫn chi tiết trong [deploy-guide.md](deploy-guide.md).

```bash
cd IRM
dotnet publish -c Release --self-contained -r win-x64 -o ./publish
```

Yêu cầu server: RAM ≥ 2GB · Disk ≥ 200MB · CPU ≥ 2 nhân

## Cấu trúc dự án

```
immigration-reportmanager-master/
├── IRM/                    # Web app Blazor Server (.NET 8)
│   ├── Components/Pages/   # Dashboard, Companies, Employees, Import, Search, Reports, Admin
│   ├── Components/Layout/  # MainLayout, NavMenu
│   └── wwwroot/            # CSS, favicon
├── mockup-demo/            # Demo tĩnh HTML/CSS/JS (deploy GitHub Pages)
├── .github/workflows/      # GitHub Actions → GitHub Pages
├── deploy-guide.md         # Hướng dẫn deploy production
└── demo.md                 # Mô tả tính năng chi tiết
```

---

*IRM v2.0 — Quản lý lao động nước ngoài hiện đại, bảo mật, dễ dùng.*
