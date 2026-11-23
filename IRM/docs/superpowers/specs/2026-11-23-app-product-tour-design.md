# Design Spec — Product Tour Hướng Dẫn Sử Dụng Toàn Ứng Dụng (IRM)

- **Ngày lập:** 23/11/2026
- **Nhánh áp dụng:** `custom/kh2`
- **Phạm vi:** `MainLayout.razor`, `NavMenu.razor`, `wwwroot/js/app-product-tour.js` *(NEW)*, `App.razor` (script include), và bộ test JS (`IRM.Tests/Browser/app-product-tour.test.cjs`).

---

## 1. Mục tiêu & Ngữ cảnh

Hiện tại hướng dẫn trong ứng dụng mới chỉ tập trung vào màn hình `/import` và 1 spotlight giới thiệu nút **Khảo sát & Góp ý (R6)** khi đăng nhập lần đầu. Khách hàng cần một **Product Tour Hướng dẫn sử dụng tổng thể** cho toàn ứng dụng, bắt đầu bằng việc thiết lập ở **Quản trị hệ thống** (nếu có quyền Admin), sau đó giới thiệu tuần tự đúng theo thứ tự từng chức năng trên thanh điều hướng (Sidebar).

### Ngoài phạm vi (Non-goals)
- Không tự chuyển route sang từng trang con trong lúc chạy tour (tránh giật/chờ tải DB Blazor Server).
- Không thay đổi hay xóa tour chi tiết riêng của màn hình `/import` (`wwwroot/js/import-tour.js`).
- Không xóa nút Khảo sát; chỉ bỏ spotlight tự động lần đầu vào nút Khảo sát để nhường ưu tiên cho **Product Tour Hướng dẫn sử dụng**.

---

## 2. Phương án đã chọn (Approaches)

| Phương án | Mô tả | Đánh đổi | Quyết định |
|---|---|---|---|
| **Cách A** | Product Tour 1 mạch dùng **Driver.js 1.12.1** (có sẵn offline) chạy trực tiếp trên `MainLayout` + `NavMenu`, đi lần lượt từng mục trên Sidebar; tự bỏ qua Bước 1 nếu tài khoản không có quyền `ADMIN`. | Không tự mở từng trang con, nhưng cực kỳ mượt, ổn định trên Blazor Server, người dùng nắm trọn hệ thống trong ~30 giây. | **Đã chọn** |
| **Cách B** | Product Tour đa trang tự chuyển route qua từng trang thật. | Chậm do chờ tải DB mỗi bước và dễ mất trạng thái tour khi Blazor re-render. | Không chọn |

---

## 3. Thứ tự các bước Product Tour (Bám sát Sidebar)

Mỗi mục trên `NavMenu.razor` được gắn thuộc tính ổn định `data-app-tour="<key>"`. Khi chạy tour, script tự lọc chỉ giữ các bước có phần tử tồn tại trên DOM (nên tài khoản không phải `ADMIN` sẽ **tự động bỏ qua Bước 1**):

1. **[CHỈ ADMIN — bỏ qua nếu không có quyền Quản trị] Thiết lập Quản trị hệ thống & Danh mục**
   - `data-app-tour="admin-system"` (nhóm `Danh mục hệ thống` + `Quản trị hệ thống` trên Sidebar)
   - Nội dung: *Bước đầu tiên trước khi vận hành: kiểm tra/bổ sung Quốc tịch, Địa bàn hành chính, Lĩnh vực, Ngành nghề và phân quyền Tài khoản.*
2. **Bảng điều khiển** (`data-app-tour="nav-dashboard"`)
   - Nội dung: *Theo dõi tổng quan số lượng người nước ngoài, cảnh báo sắp hết hạn tạm trú và biểu đồ quốc tịch/GPLĐ.*
3. **Hồ sơ Người nước ngoài** (`data-app-tour="nav-foreign-persons"`)
   - Nội dung: *Quản lý hợp nhất Lao động, Du học sinh và Thăm thân — chỉ bắt buộc Số hộ chiếu, các trường còn lại linh hoạt bổ sung sau.*
4. **Doanh nghiệp** (`data-app-tour="nav-companies"`)
   - Nội dung: *Quản lý doanh nghiệp bảo lãnh, địa điểm hoạt động/KCN, người đại diện và hồ sơ pháp lý.*
5. **Cơ sở lưu trú** (`data-app-tour="nav-accommodations"`)
   - Nội dung: *Quản lý cơ sở lưu trú, sức chứa và hợp đồng thuê lưu trú với doanh nghiệp.*
6. **Đợt kiểm tra NNN** (`data-app-tour="nav-inspections"`)
   - Nội dung: *Tạo đợt kiểm tra hiện trường, ghi nhận đối tượng kiểm tra và chi tiết vi phạm/xác minh.*
7. **Tra cứu toàn cục (`Ctrl+K`)** (`data-app-tour="nav-search"`)
   - Nội dung: *Tìm kiếm nhanh theo Số hộ chiếu, họ tên, doanh nghiệp hoặc địa bàn trên toàn hệ thống.*
8. **Báo cáo định kỳ** (`data-app-tour="nav-reports"`)
   - Nội dung: *Tùy chọn cột hiển thị, gom nhóm theo Công ty/Quốc tịch/Lĩnh vực và xuất file Excel báo cáo.*
9. **Thống kê 54 địa bàn** (`data-app-tour="nav-statistics"`)
   - Nội dung: *Xem thống kê phân bố người nước ngoài theo 54 địa bàn hành chính, KCN và định danh điện tử.*
10. **Nhập dữ liệu Excel** (`data-app-tour="nav-import"`)
    - Nội dung: *Tải file Excel lộn xộn, lọc đúng các trường cần lấy, tự chuẩn hóa ngày `dd/MM/yyyy`, xem cảnh báo và hoàn tác an toàn nếu cần.*
11. **Xem lại Hướng dẫn bất cứ lúc nào** (`#btnAppUserGuide` trên Topbar)
    - Nội dung: *Bấm nút Hướng dẫn sử dụng trên thanh tiêu đề để mở lại tour bất cứ khi nào cần.*

---

## 4. Thông báo bắt buộc "Phải có Số hộ chiếu" (Passport-Required Notification)

Bổ sung **1 thông báo nổi bật nhắc quy tắc cốt lõi của hệ thống**:
- **Nội dung:** *"Lưu ý bắt buộc: Mọi hồ sơ Người nước ngoài (nhập tay hoặc nhập Excel) đều **PHẢI có Số hộ chiếu** — đây là trường định danh bắt buộc duy nhất; các thông tin khác có thể bổ sung sau."*
- **Vị trí hiển thị:**
  1. **Banner thông báo (có nút đóng) ở đầu màn hình chính (`MainLayout.razor`, lưu trạng thái đóng theo phiên hoặc `localStorage`)** và hiển thị sẵn trên các màn hình nhập liệu (`ForeignPersons.razor`, `ImportUploadSection.razor`, `ImportMappingSection.razor`).
  2. **Nhấn mạnh trong Product Tour** ở bước giới thiệu **Hồ sơ Người nước ngoài** và **Nhập dữ liệu Excel** (và hiện toast notification nhắc lại khi hoàn tất tour).

---

## 5. Hành vi kích hoạt & Lưu trạng thái

- **Lần đầu đăng nhập:** Tự động mở sidebar (nếu đang đóng) và chạy Product Tour 1 lần; lưu cờ `localStorage['irm-product-tour-v1-seen'] = '1'` khi người dùng đóng hoặc hoàn tất tour.
- **Mở lại thủ công:** Nút **Hướng dẫn sử dụng** (`#btnAppUserGuide`) trên Topbar `MainLayout.razor` gọi `irmAppTour.start(true)` (bỏ qua cờ `seen` và tự mở sidebar trước khi chạy).
- **An toàn & Tương thích:** Nếu `window.driver` chưa tải hoặc `localStorage` bị chặn, không ném lỗi JS; tự cuộn mục Sidebar vào vùng nhìn thấy (`scrollIntoView`) để highlight chính xác cả trên màn hình laptop 1366px.
- **Thay thế spotlight Khảo sát lần đầu:** Không tự bật spotlight vào `#btnCustomerFeedback` khi vào trang nữa (vẫn giữ nguyên nút Khảo sát cho ai cần bấm mở).
