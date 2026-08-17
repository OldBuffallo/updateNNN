# Immigration Report Manager v0.1.1 (IRM)

Hệ thống quản lý người nước ngoài với web Blazor và WPF chạy song song trên cùng database. v0.1.1 là bản hotfix giao diện trên nền schema additive v0.1.0, không thay đổi cột của các bảng legacy.

> **Deployment status:** demo/staging `v0.1.0` từ commit `0723e4e`, deployment `dep_HW3aH3BKVfSKa_1z`, đang chạy tại `https://irm.180.93.103.206.nip.io`; customer production chưa được phê duyệt. Xem [báo cáo triển khai và nghiệm thu kỹ thuật](docs/IRM-v0.1.0-implementation-report.md).

## 📌 Trạng thái dự án

| Giai đoạn | Tên | Trạng thái |
|:---:|:---|:---:|
| GĐ1 | Khảo sát Hệ thống Cũ | ✅ Done |
| GĐ2 | Thiết kế Giải pháp | ✅ Done |
| GĐ3 | Demo Khách hàng | ✅ Done |
| GĐ4 | Chỉnh sửa theo Yêu cầu | ✅ Dev/Test complete |
| GĐ5 | Deploy Test Server Thật | ✅ Demo/staging verified |
| GĐ6 | Kiểm tra & Sửa lỗi | 🔄 17 automated tests pass; UAT pending |
| GĐ7 | Bàn giao & Hướng dẫn | 📋 Todo |

> Xem chi tiết tại [PROJECT_MANAGEMENT.md](PROJECT_MANAGEMENT.md).

## Demo trực tiếp

**[Xem demo tại đây →](https://oldbuffallo.github.io/updateNNN/)**

> Demo tĩnh cũ không phải bằng chứng nghiệm thu hoặc production của v0.1.0.

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
| 👨‍👩‍👧 Thăm thân | Hồ sơ người và thân nhân, sponsor công ty/người lao động |
| 🏠 Lưu trú | CSLT, doanh nghiệp thuê, người ở và lịch sử địa chỉ |
| ✅ Kiểm tra | Đợt kiểm tra, snapshot kết quả, lookback |
| 📍 Thống kê địa bàn | As-of, distinct người, khu vực, giấy tờ/định danh |

## Tech Stack

| Thành phần | Công nghệ |
|---|---|
| **Backend** | ASP.NET Core 8 (.NET 8) |
| **Frontend** | Blazor Server (Interactive Server-Side Rendering) |
| **UI Framework** | MudBlazor v9.3 (Material Design) |
| **ORM** | Entity Framework Core 8.0 |
| **Database** | SQL Server 2014+ (production) / SQLite (development/demo) |
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

### Bảng mở rộng có trước baseline v0.1.0

`AuditLogs`, `ImportHistories`, `ImportBackups`, `ColumnMappingTemplates`, `Students`, `ArchivedEmployees`

### Bảng additive của v0.1.0

`SchemaVersions`, `ForeignPersons`, `ForeignPersonSourceLinks`, `StayCases`, `FamilyVisitDetails`, `AdministrativeUnits`, `ResidencePeriods`, `ImmigrationDocuments`, `ElectronicIdentities`, `CompanyProfiles`, `CompanySites`, `EconomicZones`, `SiteZoneMemberships`, `AccommodationsV010`, `CompanyAccommodationAgreements`, `CompanyRepresentatives`, `StoredFiles`, `CompanyLegalDocuments`, `InspectionsV010`, `InspectionSubjects`, `WebCredentials`, `WebRoleAssignments`, `LegacyChangeEvents`, `MigrationIssues`

> **Lưu ý:** IRM v0.1.0 chạy song song với phần mềm desktop cũ, dùng chung database. Migration v0.1.0 chỉ thêm bảng mới; ứng dụng không tự ALTER schema lúc khởi động.

## Tài liệu liên quan

| Tài liệu | Nội dung |
|---|---|
| [PROJECT_MANAGEMENT.md](PROJECT_MANAGEMENT.md) | Quản lý 7 giai đoạn, tasks, GitHub Issues |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Quy trình làm việc, branching, code review |
| [deploy-guide.md](deploy-guide.md) | Hướng dẫn triển khai production |
| [DEPLOYMENT.md](DEPLOYMENT.md) | Cấu hình server chi tiết |
| [demo.md](demo.md) | Mô tả demo cho khách hàng |
| [docs/IRM-v0.1.0-database-architecture-report.md](docs/IRM-v0.1.0-database-architecture-report.md) | ERD tổng thể/theo miền và catalog 43 bảng |
| [docs/](docs/) | Tài liệu từng giai đoạn dự án |

## Thành viên

| Vai trò | Người phụ trách |
|---|---|
| Project Lead | — |
| Dev A | Backend, DB, Import/Export, Deploy |
| Dev B | Frontend, UI/UX, Dashboard, Reports |

---

*IRM v0.1.1 — quản lý người nước ngoài theo dữ liệu hợp nhất và lịch sử hiệu lực.*
