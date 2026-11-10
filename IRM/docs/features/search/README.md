# Tìm kiếm Toàn cục

## 1. Tổng quan

Module tìm kiếm toàn cục (global search) cho phép tìm kiếm đồng thời trên 3 loại entity: Nhân viên, Công ty, và Du học sinh. Tìm kiếm theo nhiều trường: tên, hộ chiếu, visa, địa chỉ, ghi chú, công ty, quốc tịch, nghề nghiệp, thăm thân.

**Route:** `/search`

## 2. Kiến trúc kỹ thuật

### Files liên quan

| Layer | File | Vai trò |
|---|---|---|
| Page | `Components/Pages/Search.razor` | UI tìm kiếm + hiển thị kết quả |
| Service | `Services/SearchService.cs` | Query tìm kiếm 3 entity |
| DTO | `SearchResult` (trong SearchService.cs) | Kết quả tìm kiếm |

### DI Registration

```csharp
builder.Services.AddScoped<SearchService>();
```

## 3. Service Interface

### `SearchService`

| Method | Tham số | Return |
|---|---|---|
| `SearchAsync` | `keyword, filter` | `Task<SearchResult>` |

**Filter options:** `"all"`, `"employee"`, `"company"`, `"student"`

### `SearchResult` DTO

| Property | Type | Mô tả |
|---|---|---|
| `Keyword` | `string` | Từ khóa đã tìm |
| `Employees` | `List<Employee>` | Max 100 kết quả |
| `Companies` | `List<Company>` | Max 50 kết quả |
| `Students` | `List<Student>` | Max 100 kết quả |
| `TotalCount` | `int` | Tổng = Employees + Companies + Students |

### Các trường được tìm kiếm

| Entity | Trường tìm |
|---|---|
| Employee | StaffName, Passport, VisaNumber, WorkPermitNumber, Address, Note, Company.CompanyName, Nationality.NationalityName, Career.CareerName, FamilyVisitRelativeName, FamilyVisitRelativeIdCard |
| Company | CompanyName, Address, LegalRepresentative, Note, Field.FieldName, TypeOfBusiniess |
| Student | FullName, Passport, SchoolName, Major, VisaNumber, StudentCode, Address, Note, Nationality.NationalityName |

## 4. Ràng buộc Nghiệp vụ

- **Case-insensitive:** Tất cả so sánh dùng `.ToLower().Contains()`
- **Chỉ tìm active:** Employee `Hidden_flag == 0`, Company `Delete_flag == 0`, Student `Hidden_flag == 0`
- **Giới hạn kết quả:** Employee 100, Company 50, Student 100
- **Navigation includes:** Employee include Company + Nationality + Career, Company include Field, Student include Nationality

## 5. Hướng dẫn Test

- Tìm theo tên → verify kết quả đúng
- Tìm theo hộ chiếu → verify cross-entity
- Tìm từ khóa rỗng → trả `SearchResult` rỗng
- Filter = "employee" → chỉ trả Employees
- Verify không trả kết quả đã bị soft delete

## 6. Hướng dẫn Mở rộng

- **Thêm entity tìm kiếm:** Thêm block tương tự trong `SearchAsync`, thêm property vào `SearchResult`, thêm filter option
- **Thêm trường tìm kiếm:** Thêm điều kiện `||` trong LINQ Where clause
- **Full-text search:** Hiện tại dùng `LIKE %keyword%` (EF Core `.Contains()`). Để tối ưu, có thể dùng SQL Server Full-Text Index
