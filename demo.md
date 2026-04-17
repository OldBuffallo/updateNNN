# 🎯 Demo — Hệ Thống Quản Lý Lao Động Nước Ngoài v2.0

> **Immigration Report Manager** — Phiên bản Web Application
> Demo tương tác: `mockup-demo/index.html`

---

## 🎬 Video Demo Toàn Bộ Flow

![Demo recording toàn bộ flow](C:/Users/OTP/.gemini/antigravity/brain/2f23a5d3-a84b-44c0-ad23-345381150a49/demo_recording.webp)

---

## 📱 Các Màn Hình Chính

### 1. 🔐 Đăng nhập — Bảo mật HTTPS

![Trang đăng nhập với thiết kế hiện đại, kết nối bảo mật](C:/Users/OTP/.gemini/antigravity/brain/2f23a5d3-a84b-44c0-ad23-345381150a49/screenshot_dashboard.png)

- Giao diện đăng nhập hiện đại, tối giản
- Mật khẩu được mã hóa (hash), không lưu plain text
- Khóa tài khoản sau 5 lần đăng nhập sai
- Kết nối HTTPS bảo mật

---

### 2. 📊 Dashboard — Tổng quan hệ thống

![Dashboard với KPI cards, biểu đồ cột, biểu đồ tròn](C:/Users/OTP/.gemini/antigravity/brain/2f23a5d3-a84b-44c0-ad23-345381150a49/screenshot_companies.png)

**Tính năng mới:**
- 4 thẻ KPI: Tổng CT, Tổng NLĐ, Sắp hết hạn, Đã có GPLĐ
- Biểu đồ NLĐ theo quốc tịch (Top 10)
- Biểu đồ tỷ lệ GPLĐ (Doughnut chart)
- Biểu đồ xu hướng hết hạn tạm trú (12 tháng)
- Bảng cảnh báo NLĐ sắp hết hạn (màu đỏ/vàng/xanh)

---

### 3. 🔍 Tìm kiếm toàn cục — Full-text Search

![Tìm kiếm toàn cục với kết quả phân loại và thống kê](C:/Users/OTP/.gemini/antigravity/brain/2f23a5d3-a84b-44c0-ad23-345381150a49/screenshot_search.png)

**Tính năng mới:**
- Tìm kiếm trên **tất cả trường dữ liệu** cùng lúc
- Kết quả phân loại: Nhân viên / Công ty
- Thống kê tự động: số kết quả, liên quan, có GPLĐ, sắp hết hạn
- Highlight từ khóa trong kết quả
- Tốc độ: < 0.2 giây

---

### 4. 📝 Báo cáo tự tạo — Report Builder

![Report Builder với panel chọn cột, điều kiện lọc và preview](C:/Users/OTP/.gemini/antigravity/brain/2f23a5d3-a84b-44c0-ad23-345381150a49/screenshot_reports.png)

**Tính năng mới:**
- Chọn cột hiển thị (tick/bỏ tick)
- Đặt điều kiện lọc linh hoạt (VD: Hạn tạm trú ≤ 30 ngày)
- Nhóm theo: Công ty / Quốc tịch / Lĩnh vực
- Xem trước kết quả trước khi xuất
- Lưu template để tái sử dụng
- Xuất Excel / PDF / In trực tiếp

---

### 5. ⚙️ Quản trị hệ thống

![Trang quản trị với 8 thẻ chức năng](C:/Users/OTP/.gemini/antigravity/brain/2f23a5d3-a84b-44c0-ad23-345381150a49/screenshot_admin.png)

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
| **Mật khẩu** | ❌ Lưu plain text | ✅ Hash BCrypt |
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

> [!TIP]
> Demo hoạt động offline, không cần internet. Chỉ cần gửi 3 file: `index.html`, `style.css`, `app.js`.

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
