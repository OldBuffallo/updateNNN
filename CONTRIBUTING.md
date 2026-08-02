# Hướng dẫn Đóng góp — IRM v2.0

## Quy trình Làm việc (GitHub Flow)

### 1. Nhận Task

- Mọi công việc **phải có GitHub Issue** trước khi code
- Tự assign issue cho mình, chuyển trạng thái sang "In Progress"
- Đọc kỹ mô tả issue và Definition of Done trước khi bắt đầu

### 2. Tạo Branch

Tạo branch từ `main` theo quy ước:

```bash
# Tính năng mới
git checkout -b feature/<tên-tính-năng>
# Ví dụ: feature/excel-import, feature/student-management

# Sửa lỗi
git checkout -b bugfix/<tên-lỗi>
# Ví dụ: bugfix/login-hash-password, bugfix/pagination-employees

# Change request từ khách hàng
git checkout -b fix/cr-<số>-<mô-tả>
# Ví dụ: fix/cr-003-them-cot-bao-cao
```

### 3. Commit

Theo chuẩn [Conventional Commits](https://www.conventionalcommits.org/):

```bash
feat: add student management page
fix: correct pagination on employee list
docs: update deploy guide for SQL Server 2019
refactor: extract Excel helper from ImportService
style: fix alignment in dashboard cards
chore: update MudBlazor to v9.3
```

### 4. Pull Request

- Mở PR khi hoàn thành, tag người còn lại review
- **KHÔNG tự merge code của mình**
- Mô tả PR bao gồm: thay đổi gì, tại sao, cách test
- Link đến GitHub Issue liên quan

### 5. Code Review

- Reviewer kiểm tra: logic, bảo mật, UI/UX, hiệu năng
- Approve hoặc Request Changes với comment cụ thể
- Sau khi approve → merge vào `main`

---

## Cấu trúc Code

### Thêm Trang Mới (Page)

1. Tạo file `IRM/Components/Pages/<TenTrang>.razor`
2. Đặt route: `@page "/<ten-trang>"`
3. Thêm vào NavMenu: `IRM/Components/Layout/NavMenu.razor`

### Thêm Service Mới

1. Tạo file `IRM/Services/<TenService>.cs`
2. Đăng ký trong `Program.cs`:
   ```csharp
   builder.Services.AddScoped<TenService>();
   ```

### Thêm Model/Table Mới

1. Tạo model: `IRM/Data/Models/<TenModel>.cs`
2. Thêm DbSet vào `IRM/Data/IrmDbContext.cs`
3. Cấu hình Fluent API trong `OnModelCreating()`
4. Nếu cần migration cho SQL Server production:
   - Thêm script SQL vào `deploy/sql/XX-ten-migration.sql`
   - Thêm `IF NOT EXISTS` check vào `EnsureNewTablesAsync()` trong `Program.cs`

---

## Quy ước Code

### C# / Blazor

- **Naming**: PascalCase cho public members, camelCase cho private
- **Nullable**: Sử dụng nullable reference types (`string?`)
- **Async**: Tất cả DB operations phải dùng async/await
- **EF Core**: Dùng parameterized queries, KHÔNG string interpolation cho SQL

### Blazor Pages

- Mỗi page nên < 500 dòng. Nếu quá lớn → tách thành component
- Dùng `@inject` để inject services
- UI text bằng tiếng Việt (phù hợp người dùng cuối)

### Git

- **Không commit** file build (`bin/`, `obj/`, `*.dll`, `*.exe`)
- **Không commit** file database (`*.db`, `*.bak`)
- **Không commit** credentials (mật khẩu, connection string production)
- **Không commit** file log (`*.log`)

---

## Chạy Development

```bash
# Clone repo
git clone <repository-url>
cd immigration-reportmanager-master

# Chạy app (tự dùng SQLite nếu không có SQL Server)
cd IRM
dotnet run

# Mở browser: http://localhost:5050
# Tài khoản demo: admin / (mật khẩu bất kỳ khi dùng SQLite)
```

### Kết nối SQL Server (tùy chọn)

Sửa `IRM/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ReportManagerDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Deploy / Build Package

```bash
# Đóng gói cho USB (self-contained, sẵn sàng deploy)
.\deploy\build-package.ps1

# Đóng gói + tạo ZIP
.\deploy\build-package.ps1 -Zip
```

Output tại: `deploy-package/`

---

## Tài liệu Dự án

Mỗi giai đoạn có tài liệu riêng trong `docs/`:

| Giai đoạn | File | Nội dung |
|---|---|---|
| GĐ1 | `docs/phase1-analysis.md` | Phân tích hệ thống cũ |
| GĐ2 | `docs/phase2-design.md` | Thiết kế kỹ thuật |
| GĐ4 | `docs/phase4-changelog.md` | Log change request |
| GĐ6 | `docs/phase6-testlog.md` | Log kiểm tra & bug |
| GĐ7 | `docs/phase7-handover.md` | Biên bản bàn giao |

> Tạo file tài liệu và commit qua PR khi hoàn thành mỗi giai đoạn.
