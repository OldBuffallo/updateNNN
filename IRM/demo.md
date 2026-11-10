# 🎯 Demo — Hệ Thống Quản Lý Lao Động Nước Ngoài v2.0

> **Immigration Report Manager** — Phiên bản Web Application
> Demo tương tác: `mockup-demo/index.html`

---

## 📱 Các Màn Hình Chính

### 1. 🔐 Đăng nhập — Bảo mật

- Giao diện đăng nhập hiện đại, tối giản
- Mật khẩu được mã hóa (hash), không lưu plain text
- Khóa tài khoản sau 5 lần đăng nhập sai

---

### 2. 📊 Dashboard — Tổng quan hệ thống

**Tính năng mới:**
- 4 thẻ KPI: Tổng CT, Tổng NLĐ, Sắp hết hạn, Đã có GPLĐ
- Biểu đồ NLĐ theo quốc tịch (Top 10)
- Biểu đồ tỷ lệ GPLĐ (Doughnut chart)
- Biểu đồ xu hướng hết hạn tạm trú (12 tháng)
- Bảng cảnh báo NLĐ sắp hết hạn (màu đỏ/vàng/xanh)

---

### 3. 🔍 Tìm kiếm toàn cục — Full-text Search

**Tính năng mới:**
- Tìm kiếm trên **tất cả trường dữ liệu** cùng lúc
- Kết quả phân loại: Nhân viên / Công ty
- Thống kê tự động: số kết quả, liên quan, có GPLĐ, sắp hết hạn
- Highlight từ khóa trong kết quả
- Tốc độ: < 0.2 giây

---

### 4. 📝 Báo cáo tự tạo — Report Builder

**Tính năng mới:**
- Chọn cột hiển thị (tick/bỏ tick)
- Đặt điều kiện lọc linh hoạt (VD: Hạn tạm trú ≤ 30 ngày)
- Nhóm theo: Công ty / Quốc tịch / Lĩnh vực
- Xem trước kết quả trước khi xuất
- Lưu template để tái sử dụng
- Xuất Excel / PDF / In trực tiếp

---

### 5. 📤 Import Excel — Wizard 4 bước

| Bước | Mô tả |
|---|---|
| 1. Upload | Kéo thả hoặc chọn file .xlsx |
| 2. Ghép cột | Auto-detect + chỉnh sửa mapping |
| 3. Preview | Xem trước dữ liệu, đánh dấu lỗi |
| 4. Kết quả | Thống kê: thêm mới / cập nhật / lỗi |

---

### 6. 🎓 Quản lý Du học sinh

**Tính năng mới (CR-001):**
- Danh sách du học sinh với DataGrid
- Thông tin: trường, chuyên ngành, visa, học bổng
- Lọc theo trạng thái: Đang học / Tốt nghiệp / Thôi học

---

### 7. ⚙️ Quản trị hệ thống

- Quản lý: Tài khoản, Lĩnh vực, Ngành nghề, Quốc tịch, Quận/Huyện, Phường/Xã
- **MỚI:** Nhật ký hệ thống (Audit Log) — ghi lại mọi thao tác
- **MỚI:** Lịch sử Import — xem lại các lần import file Excel

---

## ⚡ So Sánh: Hệ Thống Cũ vs Mới

| Tính năng | Phiên bản CŨ (Desktop) | Phiên bản MỚI (Web) |
|-----------|:---:|:---:|
| **Cài đặt trên máy trạm** | ❌ Phải cài từng máy | ✅ Không cần — chỉ mở trình duyệt |
| **Cập nhật phần mềm** | ❌ Copy file từng máy | ✅ Cập nhật 1 chỗ trên server |
| **Bảo mật SQL** | ❌ Mở port 1433 ra mạng | ✅ SQL chỉ cho localhost truy cập |
| **Mật khẩu** | ❌ Lưu plain text | ✅ Hash mã hóa |
| **Dashboard** | ❌ Không có | ✅ 4 KPI + 4 biểu đồ + cảnh báo |
| **Import Excel** | ⚠️ Nhập số cột thủ công | ✅ Auto-detect, preview, xử lý trùng |
| **Tìm kiếm** | ⚠️ Chỉ theo từng trường | ✅ Full-text trên mọi trường |
| **Báo cáo** | ⚠️ Cố định, không tùy chỉnh | ✅ Tự tạo, lưu template, xuất Excel/PDF |
| **Nhật ký** | ❌ Không có | ✅ Ghi log mọi thao tác |
| **Truy cập từ điện thoại** | ❌ Không thể | ✅ Responsive, mở trên mọi thiết bị |

---

## 🖥️ Cách Chạy Demo

Gửi khách hàng thư mục `mockup-demo/`, hướng dẫn:

1. Mở file `index.html` bằng **Chrome** hoặc **Edge**
2. Nhấn **"Đăng nhập"** để vào Dashboard
3. Click các mục ở sidebar trái để xem từng trang
4. Trang **Import Excel**: nhấn "Tiếp tục" để xem 4 bước
5. Trang **Tìm kiếm**: nhấn nút "Tìm kiếm" để xem kết quả
6. Trang **Báo cáo**: xem preview bảng bên phải + template đã lưu

> **Lưu ý:** Demo hoạt động offline, không cần internet. Chỉ cần gửi 3 file: `index.html`, `style.css`, `app.js`.

---

## 📅 Lộ Trình Triển Khai

| Giai đoạn | Thời gian | Nội dung |
|-----------|:---------:|---------|
| **1. Nền tảng** | 2 tuần | Setup project, DB migration, Authentication |
| **2. CRUD** | 3 tuần | Quản lý CT, NLĐ, Danh mục, File đính kèm |
| **3. Chức năng mới** | 4 tuần | Import thông minh, Tìm kiếm, Báo cáo, Dashboard |
| **4. Nâng cao** | 2 tuần | Widget tùy chỉnh, Cảnh báo tự động |
| **5. Deploy** | 1 tuần | Testing, Migration data, Triển khai |
| **Tổng** | **12 tuần** | |
