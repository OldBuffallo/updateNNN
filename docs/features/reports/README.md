# Báo cáo Tùy chỉnh

## 1. Tổng quan

Module cho phép người dùng tạo báo cáo tùy chỉnh bằng cách chọn cột, điều kiện lọc, và xuất ra file Excel. Sử dụng `ExportService` để generate file bằng ClosedXML.

**Route:** `/reports`

## 2. Kiến trúc kỹ thuật

### Files liên quan

| Layer | File | Vai trò |
|---|---|---|
| Page | `Components/Pages/Reports.razor` | UI builder báo cáo |
| Service | `Services/ExportService.cs` | Export dữ liệu ra Excel |
| Library | ClosedXML 0.104.2 | Thư viện tạo file Excel |

### DI Registration

```csharp
builder.Services.AddScoped<ExportService>();
```

## 3. Service Interface

### Export Methods

| Method | Tham số | Mô tả |
|---|---|---|
| `ExportCompaniesAsync` | `search?, fieldId?` | Xuất danh sách công ty |
| `ExportEmployeesAsync` | `companyId?, nationality?, workPermit?, expiringOnly` | Xuất NLĐ theo bộ lọc |
| `ExportStudentsAsync` | tương tự | Xuất du học sinh |
| `ExportCustomReportAsync` | `ReportConfig` | Xuất báo cáo tùy chỉnh (chọn cột + lọc) |

### Excel Output Format

- Header: Bold, nền xanh `#2563eb`, chữ trắng
- Cột tự động resize (`ws.Columns().AdjustToContents()`)
- Dùng `IDbContextFactory<IrmDbContext>` để tránh blocking SignalR circuit

## 4. Ràng buộc Nghiệp vụ

- **Authorization:** Yêu cầu role `Admin` hoặc `Reporter`
- **DbContextFactory:** ExportService dùng `IDbContextFactory` thay vì inject `IrmDbContext` trực tiếp — vì Blazor Server có thể chạy export lâu, cần DbContext riêng không block UI
- **Audit logging:** Mỗi lần export ghi vào AuditLog

## 5. Hướng dẫn Test

- Export companies với filter → verify file Excel có đúng dữ liệu
- Export employees `expiringOnly = true` → chỉ chứa NLĐ sắp hết hạn
- Export với DB rỗng → file Excel chỉ có header

## 6. Hướng dẫn Mở rộng

- **Thêm sheet/entity mới:** Tạo method mới trong `ExportService`, define headers + data mapping
- **Thêm export PDF:** Có thể dùng thêm thư viện QuestPDF hoặc iTextSharp, tạo service riêng
- **Custom columns:** Mở rộng `ReportConfig` DTO để cho phép user chọn cột cụ thể
