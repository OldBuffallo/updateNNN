# Giai đoạn 1 — Phân tích Hệ thống Cũ

> **Trạng thái:** ✅ Done

## 1. Tổng quan Hệ thống Cũ

| Thông tin | Chi tiết |
|---|---|
| **Loại ứng dụng** | WPF Desktop App (C#, .NET Framework) |
| **Database** | SQL Server 2014 (instance: BIRDIEPO hoặc SQLEXPRESS) |
| **Tên database** | `ReportManagerDB` |
| **Người dùng** | Cán bộ văn phòng trong mạng LAN (~4-10 tài khoản) |

## 2. Chức năng Hệ thống Cũ

1. **Quản lý Công ty** — CRUD công ty sử dụng lao động nước ngoài
2. **Quản lý Nhân viên (NLĐ)** — Theo dõi lao động nước ngoài, hộ chiếu, visa, GPLĐ
3. **Quản lý Danh mục** — Lĩnh vực, ngành nghề, quốc tịch, quận/huyện, phường/xã
4. **Import dữ liệu** — Nhập từ Excel (nhập số cột thủ công)
5. **Xuất báo cáo** — Xuất Excel với format cố định
6. **Tài khoản** — Đăng nhập, phân quyền Admin/User

## 3. Cấu trúc Database Gốc (13 bảng)

| Bảng | Mô tả | Ghi chú |
|---|---|---|
| `Accounts` | Tài khoản | Mật khẩu plain text ⚠️ |
| `Companies` | Công ty | FK → Fields, Accounts |
| `Employees` | Lao động nước ngoài | FK → Companies, Careers, Nationality |
| `Fields` | Lĩnh vực hoạt động | |
| `Careers` | Ngành nghề | FK → CareerGroups |
| `CareerGroups` | Nhóm ngành nghề | |
| `Nationality` | Quốc tịch | |
| `Investment` | Vốn đầu tư | FK → Companies |
| `PhoneNumbers` | SĐT liên hệ | FK → Companies |
| `Emails` | Email liên hệ | FK → Companies |
| `Districts` | Quận/Huyện | |
| `Wards` | Phường/Xã | |
| `Attach` | File đính kèm | FK → Companies |

## 4. Vấn đề Cần Cải thiện

### 🔴 Bảo mật

1. **Mật khẩu lưu plain text** — Không hash, không salt
2. **SQL Injection** — Chuỗi SQL ghép trực tiếp trong code
3. **Port DB mở ra LAN** — SQL Server port 1433 mở cho toàn bộ máy client
4. **Connection string trên máy user** — Username/password DB nằm trong config trên từng máy

### 🟡 Hạn chế Chức năng

5. **Không có Dashboard** — Không có tổng quan KPI, biểu đồ
6. **Import Excel thủ công** — Phải nhập số cột, không auto-detect
7. **Tìm kiếm hạn chế** — Chỉ tìm theo từng trường riêng lẻ
8. **Báo cáo cố định** — Không thể tùy chỉnh cột, điều kiện lọc
9. **Không có Audit Log** — Không ghi lại lịch sử thao tác
10. **Không hỗ trợ mobile** — Desktop only

### 🟡 Vận hành

11. **Cài đặt trên từng máy** — Mỗi máy phải cài phần mềm riêng
12. **Cập nhật thủ công** — Copy file từng máy khi có bản mới

## 5. Khối lượng Dữ liệu

| Bảng | Số bản ghi (ước tính) |
|---|---|
| Companies | ~156 |
| Employees | ~1,245 |
| Careers | ~50 |
| Nationality | ~20+ |

> Quy mô nhỏ, phù hợp để chạy trên 1 server đơn lẻ.
