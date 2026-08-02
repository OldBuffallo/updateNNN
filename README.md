# Immigration Report Manager v2.0 (IRM)

Hệ thống quản lý báo cáo người lao động nước ngoài — phiên bản web application hiện đại, thay thế hoàn toàn phần mềm desktop cũ.

## 📌 Trạng thái dự án

| Giai đoạn | Tên | Trạng thái |
|:---:|:---|:---:|
| GĐ1 | Khảo sát Hệ thống Cũ | ✅ Done |
| GĐ2 | Thiết kế Giải pháp | ✅ Done |
| GĐ3 | Demo Khách hàng | ✅ Done |
| GĐ4 | Chỉnh sửa theo Yêu cầu | 🔄 In Progress |
| GĐ5 | Deploy Test Server Thật | 🔄 In Progress |
| GĐ6 | Kiểm tra & Sửa lỗi | 📋 Todo |
| GĐ7 | Bàn giao & Hướng dẫn | 📋 Todo |

> Xem chi tiết tại [PROJECT_MANAGEMENT.md](PROJECT_MANAGEMENT.md).

## Demo trực tiếp

**[Xem demo tại đây →](https://oldbuffallo.github.io/updateNNN/)**

> Đăng nhập với tài khoản: `admin` / mật khẩu: bất kỳ

## Tính năng chính

| Tính năng | Mô tả |
|---|---|
| 📊 Dashboard | KPI cards, biểu đồ quốc tịch/GPLĐ/hết hạn, bảng cảnh báo |
| 🏢 Quản lý Công ty | Danh sách, thêm/sửa, lọc theo lĩnh vực |
| 👥 Quản lý NLĐ | Theo dõi lao động nước ngoài, hộ chiếu, visa, thăm thân |
| 🎓 Quản lý Du học sinh | Theo dõi du học sinh, trường học, visa, học bổng |
| 📤 Import Excel | Wizard 4 bước: upload → ghép cột → xem trước → kết quả |
| 🔍 Tìm kiếm toàn cục | Tìm theo tên, hộ chiếu, công ty, quốc tịch |
| 📝 Báo cáo tùy chỉnh | Chọn cột, điều kiện lọc, nhóm, xuất Excel/PDF |
| ⚙️ Quản trị | Tài khoản, danh mục, nhật ký hệ thống, lịch sử import |

## Tech Stack

| Thành phần | Công nghệ |
|---|---|
| **Backend** | ASP.NET Core 8 (.NET 8) |
| **Frontend** | Blazor Server (Interactive Server-Side Rendering) |
| **UI Framework** | MudBlazor v9.3 (Material Design) |
| **ORM** | Entity Framework Core 8.0 |
| **Database** | SQL Server 2014+ (production) / SQLite (fallback/demo) |
| **Excel** | ClosedXML 0.104.2 |
| **Deploy** | Windows Service / Docker / GitHub Pages (demo) |

## Cấu trúc dự án

```
immigration-reportmanager-master/
├── IRM/                        # Web app Blazor Server (.NET 8)
│   ├── Components/
│   │   ├── Pages/              # 8 trang: Dashboard, Companies, Employees,
│   │   │                       #          Students, Import, Search, Reports, Admin
│   │   └── Layout/             # MainLayout, NavMenu
│   ├── Data/
│   │   ├── Models/             # 14 entity models
│   │   ├── IrmDbContext.cs     # DbContext (19 DbSets)
│   │   └── DatabaseSeeder.cs   # Seed data mẫu
│   ├── Services/               # 11 services: Import, Export, Dashboard,
│   │                           #              Search, Auth, Audit, Catalog...
│   ├── wwwroot/                # Static files
│   ├── appsettings.json        # Cấu hình DB + Port
│   └── Program.cs              # Entry point + DB initialization
├── deploy/                     # Scripts triển khai
│   ├── build-package.ps1       # Đóng gói USB cho deploy
│   ├── install.ps1             # Cài đặt 1-click (8 bước)
│   ├── quick-install.ps1       # Cài đặt nhanh (auto-detect SQL)
│   ├── uninstall.ps1           # Gỡ bỏ
│   ├── backup-old-server.ps1   # Backup DB máy cũ
│   └── sql/                    # SQL migration scripts
├── mockup-demo/                # Demo tĩnh HTML/CSS/JS (GitHub Pages)
├── docs/                       # Tài liệu dự án theo giai đoạn
├── Dockerfile                  # Docker deploy (Render cloud)
├── .github/workflows/          # GitHub Actions → GitHub Pages
├── CONTRIBUTING.md             # Hướng dẫn đóng góp
├── deploy-guide.md             # Hướng dẫn deploy production
├── DEPLOYMENT.md               # Hướng dẫn triển khai chi tiết
├── PROJECT_MANAGEMENT.md       # Quản lý dự án 7 giai đoạn
└── demo.md                     # Mô tả tính năng demo
```

## Yêu cầu phát triển

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2014+ (hoặc để trống để dùng SQLite)
- IDE: Visual Studio 2022 / VS Code / Rider

## Chạy locally

```bash
cd IRM
dotnet run
# Mở http://localhost:5050
```

> Nếu không cấu hình SQL Server, app sẽ tự chuyển sang SQLite với data mẫu.

## Build & Publish

```bash
# Framework-dependent (nhẹ, cần .NET Runtime trên server)
cd IRM
dotnet publish -c Release -o ./publish

# Self-contained (không cần .NET Runtime, nặng hơn)
cd IRM
dotnet publish -c Release --self-contained -r win-x64 -o ./publish
```

## Deploy production

Xem hướng dẫn chi tiết:
- [deploy-guide.md](deploy-guide.md) — Quy trình triển khai 6 bước
- [DEPLOYMENT.md](DEPLOYMENT.md) — Cấu hình server, database, troubleshooting

**Tóm tắt nhanh:**

```bash
# Trên máy dev: đóng gói
.\deploy\build-package.ps1 -Zip

# Trên máy chủ: cài đặt 1-click
.\quick-install.ps1 -SqlInstance ".\SQLEXPRESS"
```

Yêu cầu server: RAM ≥ 2GB · Disk ≥ 200MB · CPU ≥ 2 nhân

## Database

### Bảng cũ (giữ nguyên từ hệ thống WPF)

`Accounts`, `Companies`, `Employees`, `Fields`, `Careers`, `CareerGroups`, `Nationality`, `Investment`, `PhoneNumbers`, `Emails`, `Districts`, `Wards`, `Attach`

### Bảng mới (thêm bởi IRM v2.0)

`AuditLogs`, `ImportHistories`, `ImportBackups`, `ColumnMappingTemplates`, `Students`, `ArchivedEmployees`

> **Lưu ý:** IRM v2.0 chạy song song với phần mềm desktop cũ, dùng CHUNG database. Migration chỉ THÊM bảng/cột mới, KHÔNG sửa/xóa dữ liệu cũ.

## Tài liệu liên quan

| Tài liệu | Nội dung |
|---|---|
| [PROJECT_MANAGEMENT.md](PROJECT_MANAGEMENT.md) | Quản lý 7 giai đoạn, tasks, GitHub Issues |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Quy trình làm việc, branching, code review |
| [deploy-guide.md](deploy-guide.md) | Hướng dẫn triển khai production |
| [DEPLOYMENT.md](DEPLOYMENT.md) | Cấu hình server chi tiết |
| [demo.md](demo.md) | Mô tả demo cho khách hàng |
| [docs/](docs/) | Tài liệu từng giai đoạn dự án |

## Thành viên

| Vai trò | Người phụ trách |
|---|---|
| Project Lead | — |
| Dev A | Backend, DB, Import/Export, Deploy |
| Dev B | Frontend, UI/UX, Dashboard, Reports |

---

*IRM v2.0 — Quản lý lao động nước ngoài hiện đại, bảo mật, dễ dùng.*
