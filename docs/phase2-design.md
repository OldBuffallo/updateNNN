# Giai đoạn 2 — Thiết kế Giải pháp

> **Trạng thái:** ✅ Done

## 1. Quyết định Kiến trúc

| Quyết định | Lựa chọn | Lý do |
|---|---|---|
| Nền tảng | ASP.NET Core 8 Blazor Server | Real-time UI, không cần API riêng, phù hợp intranet |
| UI Framework | MudBlazor v9.3 | Component phong phú, Material Design, miễn phí |
| Database | SQL Server 2014/2019 | Tương thích hệ thống cũ, khách hàng đã có license |
| Auth | Custom (tương thích DB cũ) | Hash mật khẩu, phân quyền Admin/User |
| ORM | Entity Framework Core 8 | Type-safe, Code First mapping, async support |
| Excel | ClosedXML 0.104.2 | Open-source, hỗ trợ đọc/ghi xlsx |
| Deploy | Windows Service (Self-hosted) | Không cần IIS, dễ cài đặt trên máy khách hàng |

> Xem so sánh chi tiết Tauri vs ASP.NET Core tại [tauri_vs_asp_comparison.md](../tauri_vs_asp_comparison.md)

## 2. Kiến trúc Hệ thống

```
┌──────────────────────────────────────────┐
│              SERVER (Máy chủ)            │
│                                          │
│  ┌────────────────────────────────────┐  │
│  │   IRM v2.0 (Windows Service)      │  │
│  │   Blazor Server + Kestrel         │  │
│  │   Port: 5050                      │  │
│  │                                    │  │
│  │   Components/Pages/ (UI)          │  │
│  │   Services/ (Business Logic)      │  │
│  │   Data/ (EF Core + Models)        │  │
│  └──────────┬─────────────────────────┘  │
│             │ localhost only             │
│  ┌──────────▼─────────────────────────┐  │
│  │   SQL Server (ReportManagerDB)     │  │
│  └────────────────────────────────────┘  │
│                                          │
│  Firewall: Port 5050 OPEN               │
└─────────────┬────────────────────────────┘
              │ HTTP (LAN)
   ┌──────────┼──────────┐
   │          │          │
Client 1  Client 2  Client 3
(Browser) (Browser) (Browser)
```

## 3. Database Design — Bảng Mới

### AuditLogs — Nhật ký hoạt động
| Cột | Kiểu | Mô tả |
|---|---|---|
| Id | BIGINT PK | |
| Action | NVARCHAR(50) | CREATE/UPDATE/DELETE/LOGIN... |
| EntityType | NVARCHAR(100) | Employee/Company/Account... |
| EntityId | INT | ID bản ghi liên quan |
| Description | NVARCHAR(MAX) | Mô tả chi tiết |
| Username | NVARCHAR(100) | Người thực hiện |
| Timestamp | DATETIME | Thời gian |
| IpAddress | NVARCHAR(50) | Địa chỉ IP |

### ImportHistories — Lịch sử Import
| Cột | Kiểu | Mô tả |
|---|---|---|
| Id | BIGINT PK | |
| SessionId | NVARCHAR(50) | ID phiên import |
| FileName | NVARCHAR(500) | Tên file Excel |
| CompanyId | INT | Công ty được import |
| TotalRows / AddedRows / UpdatedRows / ErrorRows | INT | Thống kê |
| Status | NVARCHAR(20) | committed/rolled_back |

### Students — Du học sinh
| Cột | Kiểu | Mô tả |
|---|---|---|
| IDStudent | INT PK | |
| FullName | NVARCHAR(200) | Họ tên |
| SchoolName | NVARCHAR(500) | Trường học |
| Major | NVARCHAR(200) | Chuyên ngành |
| Nationality | NVARCHAR(10) FK | Quốc tịch |
| VisaNumber / VisaExpiry | | Thông tin visa |
| Status | INT | 0=Đang học, 1=Tốt nghiệp, 2=Thôi học |

### Các bảng mới khác

- **ImportBackups** — Lưu dữ liệu gốc trước import (rollback)
- **ColumnMappingTemplates** — Template ghép cột Excel
- **ArchivedEmployees** — Lưu trữ NLĐ đã xóa/chuyển đi

### Cột mới thêm vào Employees

7 cột `FamilyVisit*` — Thông tin thăm thân nhân:
`FamilyVisit`, `FamilyVisitRelativeName`, `FamilyVisitRelationship`, `FamilyVisitRelativeIdCard`, `FamilyVisitStartDate`, `FamilyVisitEndDate`, `FamilyVisitNote`

### Cột mới thêm vào Companies

`RegistrationProfileIndex` — Chỉ số hồ sơ đăng ký

## 4. Wireframe Màn hình

| Trang | Route | Mô tả |
|---|---|---|
| Dashboard | `/` | KPI cards, biểu đồ, bảng cảnh báo |
| Companies | `/companies` | DataGrid, CRUD, lọc theo lĩnh vực |
| Employees | `/employees` | DataGrid, CRUD, chi tiết thăm thân |
| Students | `/students` | DataGrid, CRUD du học sinh |
| Import | `/import` | Wizard 4 bước |
| Search | `/search` | Tìm kiếm toàn văn |
| Reports | `/reports` | Chọn cột, lọc, xuất |
| Admin | `/admin` | Quản lý tài khoản, danh mục, audit log |
