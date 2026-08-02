# Giai đoạn 6 — Log Kiểm tra & Sửa lỗi

> **Trạng thái:** 📋 Todo

## Hướng dẫn

Khi bắt đầu giai đoạn kiểm tra, ghi kết quả vào các bảng dưới đây.

## Test Cases — Chức năng

| # | Chức năng | Hành động test | Kết quả | Bug ID | Ghi chú |
|---|---|---|---|---|---|
| TC-001 | Đăng nhập | Đăng nhập đúng username/password | ⬜ | | |
| TC-002 | Đăng nhập | Đăng nhập sai password | ⬜ | | |
| TC-003 | Đăng xuất | Click đăng xuất | ⬜ | | |
| TC-004 | Đổi mật khẩu | Đổi mật khẩu thành công | ⬜ | | |
| TC-005 | Dashboard | KPI cards hiển thị đúng số liệu | ⬜ | | |
| TC-006 | Dashboard | Biểu đồ quốc tịch render đúng | ⬜ | | |
| TC-007 | Dashboard | Bảng cảnh báo hết hạn đúng | ⬜ | | |
| TC-008 | Công ty | Thêm mới công ty | ⬜ | | |
| TC-009 | Công ty | Sửa thông tin công ty | ⬜ | | |
| TC-010 | Công ty | Xóa công ty | ⬜ | | |
| TC-011 | Công ty | Lọc theo lĩnh vực | ⬜ | | |
| TC-012 | NLĐ | Thêm mới nhân viên | ⬜ | | |
| TC-013 | NLĐ | Sửa thông tin nhân viên | ⬜ | | |
| TC-014 | NLĐ | Xóa nhân viên | ⬜ | | |
| TC-015 | NLĐ | Lưu trữ nhân viên (Archive) | ⬜ | | |
| TC-016 | NLĐ | Thêm thông tin thăm thân | ⬜ | | |
| TC-017 | Du học sinh | CRUD du học sinh | ⬜ | | |
| TC-018 | Import Excel | Upload file .xlsx hợp lệ | ⬜ | | |
| TC-019 | Import Excel | Upload file định dạng sai | ⬜ | | |
| TC-020 | Import Excel | Ghép cột tự động | ⬜ | | |
| TC-021 | Import Excel | Preview trước khi commit | ⬜ | | |
| TC-022 | Import Excel | Rollback sau import | ⬜ | | |
| TC-023 | Tìm kiếm | Tìm theo tên | ⬜ | | |
| TC-024 | Tìm kiếm | Tìm theo hộ chiếu | ⬜ | | |
| TC-025 | Tìm kiếm | Tìm theo công ty | ⬜ | | |
| TC-026 | Báo cáo | Chọn cột và lọc | ⬜ | | |
| TC-027 | Báo cáo | Xuất Excel | ⬜ | | |
| TC-028 | Báo cáo | Lưu template | ⬜ | | |
| TC-029 | Admin | Quản lý tài khoản | ⬜ | | |
| TC-030 | Admin | Xem Audit Log | ⬜ | | |
| TC-031 | Admin | Xem lịch sử Import | ⬜ | | |

**Chú thích:** ⬜ Chưa test · ✅ Pass · ❌ Fail

## Test Cases — Bảo mật

| # | Kiểm tra | Kết quả | Ghi chú |
|---|---|---|---|
| SEC-001 | Không truy cập được trang khi chưa đăng nhập | ⬜ | |
| SEC-002 | User không thể xóa dữ liệu (chỉ Admin) | ⬜ | |
| SEC-003 | Audit log ghi đầy đủ thao tác nhạy cảm | ⬜ | |

## Test Cases — Hiệu năng

| # | Kiểm tra | Ngưỡng | Kết quả | Thực tế |
|---|---|---|---|---|
| PERF-001 | Load danh sách 1000+ bản ghi | < 3 giây | ⬜ | |
| PERF-002 | Import file Excel 500 dòng | < 30 giây | ⬜ | |
| PERF-003 | Tìm kiếm toàn văn | < 1 giây | ⬜ | |

## Bug Log

| Bug ID | Ngày | Mô tả | Mức độ | Người fix | Trạng thái |
|---|---|---|---|---|---|
| BUG-001 | — | — | — | — | — |

**Mức độ:** 🔴 Critical · 🟡 High · 🟢 Medium · ⚪ Low
