# 📖 HƯỚNG DẪN CÀI ĐẶT IRM v1.0.2

## Hệ thống Quản lý Người nước ngoài

---

## 📋 Yêu cầu hệ thống

| Thành phần | Yêu cầu tối thiểu | Khuyến nghị |
|---|---|---|
| Hệ điều hành | Windows 10/11 hoặc Server 2019+ (64-bit) | Windows 11 Pro |
| CPU | 2 cores, 2 GHz | 4 cores |
| RAM | 2 GB trống | 4 GB trống |
| Ổ đĩa | 10 GB trống | 20 GB trống |
| Quyền | Administrator | Administrator |

## 🚀 Cách cài đặt

### Bước 1: Giải nén bộ cài

1. Giải nén file `IRM-v1.0.2-offline-installer.zip` ra thư mục bất kỳ
2. Mở thư mục vừa giải nén

### Bước 2: Chạy trình cài đặt

**Double-click vào file `CÀI ĐẶT IRM.hta`**

> ⚠️ Windows có thể hỏi "Windows protected your PC" → Nhấn **More info** → **Run anyway**

### Bước 3: Theo hướng dẫn trên màn hình

Trình cài đặt có 4 bước:

#### 📌 Bước 1 — Màn hình chào mừng
- Xem thông tin về phần mềm
- Nhấn **"Bắt đầu cài đặt"** để tiếp tục

#### 📌 Bước 2 — Quét môi trường
Trình cài đặt sẽ tự động kiểm tra máy tính:

| Mục kiểm tra | Ý nghĩa |
|---|---|
| ✅ Quyền Administrator | Cần có quyền admin để cài |
| ✅ Hệ điều hành | Windows 10/11/Server 64-bit |
| ✅ CPU | Ít nhất 2 cores |
| ✅ RAM | Ít nhất 2 GB |
| ✅ Ổ đĩa | Ít nhất 10 GB trống |
| ✅/⚠️ SQL Server | Nếu chưa có sẽ cài ở bước sau |
| ✅ Cổng 5050 | Cổng mạng cần trống |

Nếu tất cả mục bắt buộc đều ✅ → Nhấn **"Cài đặt"**

#### 📌 Bước 3 — Cài đặt tự động
- Cài SQL Server (nếu chưa có) — khoảng 5-10 phút
- Sao chép ứng dụng
- Tạo database
- Đăng ký service tự khởi động
- Mở cổng firewall

Theo dõi tiến trình qua thanh tiến trình và log bên dưới.

#### 📌 Bước 4 — Hoàn tất
- Nhấn **"Mở IRM trong trình duyệt"** để bắt đầu sử dụng

## 🌐 Cách truy cập sau khi cài

| Từ đâu | Địa chỉ |
|---|---|
| Máy cài đặt | `http://localhost:5050` |
| Máy khác trong mạng LAN | `https://TÊN-MÁY:5443` |

> 💡 Lần đầu truy cập từ LAN, trình duyệt có thể cảnh báo chứng thư SSL — nhấn **Advanced** → **Proceed** (đây là bình thường với chứng thư tự ký).

## 👤 Đăng nhập lần đầu

- Sử dụng tài khoản admin đã được tạo sẵn trong hệ thống
- Đổi mật khẩu ngay sau khi đăng nhập lần đầu

## 🔧 Xử lý sự cố

### IRM không mở được sau khi cài?

1. Mở **Services** (nhấn Win+R, gõ `services.msc`, Enter)
2. Tìm service **"IRM - Quản lý NNN"**
3. Nhấn chuột phải → **Start**
4. Chờ 10 giây rồi thử truy cập lại `http://localhost:5050`

### Quên mật khẩu?

Liên hệ bộ phận kỹ thuật để reset mật khẩu.

### Cần backup dữ liệu?

Dữ liệu được lưu tại: `C:\IRM\data\`
Database: SQL Server Express → `IRM`

## 📞 Hỗ trợ kỹ thuật

Nếu gặp vấn đề, vui lòng:
1. Chụp ảnh màn hình lỗi
2. Gửi cho bộ phận hỗ trợ kỹ thuật
3. Ghi rõ bước nào gặp lỗi

---

*IRM v1.0.2 — Hệ thống Quản lý Người nước ngoài*
*© 2026*
