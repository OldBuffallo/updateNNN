# Quản lý Du học sinh

## 1. Tổng quan

Module quản lý du học sinh nước ngoài đang học tập tại địa phương. Thiết kế theo pattern của Employee nhưng có thêm thông tin học tập (trường, ngành, mã SV, học bổng, trình độ).

**Route:** `/students`

## 2. Kiến trúc kỹ thuật

### Files liên quan

| Layer | File | Vai trò |
|---|---|---|
| Page | `Components/Pages/Students.razor` | CRUD du học sinh |
| Service | `Services/StudentService.cs` | CRUD + lọc + kiểm tra trùng |
| Model | `Data/Models/Student.cs` | Entity Student |

### DI Registration

```csharp
builder.Services.AddScoped<StudentService>();
```

## 3. Database Schema

| Bảng | PK | Mô tả |
|---|---|---|
| `Students` | `IDStudent` | Du học sinh nước ngoài |

### Cột quan trọng

| Cột | Type | Mô tả |
|---|---|---|
| `EducationLevel` | `int` | 0=Đại học, 1=Thạc sĩ, 2=Tiến sĩ, 3=Ngắn hạn, 4=Khác |
| `ScholarshipType` | `int` | 0=Tự túc, 1=HB Chính phủ VN, 2=HB nước ngoài, 3=Khác |
| `Status` | `int` | 0=Đang học, 1=Đã TN, 2=Tạm nghỉ, 3=Đã về nước |
| `Hidden_flag` | `int` | 0=Hiện, 1=Soft delete |
| `VisaExpiry` | `DateTime?` | Hạn visa |
| `TemporaryStay` | `DateTime?` | Hạn tạm trú |
| `SchoolName` | `string?` | Tên trường |
| `Major` | `string?` | Ngành học |
| `StudentCode` | `string?` | Mã sinh viên |

## 4. Service Interface

### `StudentService`

| Method | Return | Mô tả |
|---|---|---|
| `GetAllActiveAsync()` | `Task<List<Student>>` | Du học sinh đang học (Status=0, Hidden=0) |
| `GetAllVisibleAsync()` | `Task<List<Student>>` | Tất cả du học sinh chưa xóa |
| `GetByIdAsync(id)` | `Task<Student?>` | Chi tiết (include Nationality) |
| `GetBySchoolAsync(schoolName)` | `Task<List<Student>>` | Lọc theo trường |
| `GetExpiringVisaAsync(days)` | `Task<List<Student>>` | Visa sắp hết hạn |
| `CreateAsync(student)` | `Task` | Tạo mới (Admin/DataEditor) |
| `UpdateAsync(student)` | `Task` | Cập nhật |
| `DeleteAsync(id)` | `Task` | Soft delete |
| `CheckDuplicatePassportAsync(passport, excludeId)` | `Task<bool>` | Kiểm tra trùng hộ chiếu |

## 5. Ràng buộc Nghiệp vụ

- **Soft delete:** `Hidden_flag = 1`
- **Authorization:** Tạo/sửa/xóa yêu cầu `Admin` hoặc `DataEditor`
- **Duplicate passport:** Kiểm tra trước khi tạo/sửa
- **Computed properties:** `DaysUntilVisaExpiry`, `DaysUntilStayExpiry` tính từ `VisaExpiry`/`TemporaryStay` so với `DateTime.Today`
- **Backfill v0.1.0:** Trong `FamilyVisitorService.BackfillLegacyAsync()`, Students được link vào ForeignPersons với `StayPurposeCode = STUDY`

## 6. Hướng dẫn Test

- CRUD: tạo du học sinh → sửa → check duplicate passport → soft delete
- Lọc theo trường: tạo SV ở trường A và B → `GetBySchoolAsync("A")` → chỉ trả SV trường A
- Visa expiring: tạo SV visa hết trong 10 ngày → `GetExpiringVisaAsync(30)` → có trong kết quả
- Enum display: verify `EducationLevelString`, `ScholarshipTypeString`, `StatusString` render đúng

## 7. Hướng dẫn Mở rộng

- **Thêm cột:** Thêm property vào `Student.cs`, map column name trong `IrmDbContext` nếu cần
- **Thêm trạng thái mới:** Thêm case vào `StatusString` switch expression
- **Thêm loại học bổng:** Thêm case vào `ScholarshipTypeString`
