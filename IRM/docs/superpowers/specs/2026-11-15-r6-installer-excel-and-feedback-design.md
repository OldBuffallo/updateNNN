# Thiết kế Gói Bàn Giao R6: Bộ cài mới, Chuẩn hóa File Excel Test & Phiếu Khảo Sát Khách Hàng

- **Ngày lập:** 15/11/2026 (Asia/Bangkok)
- **Trạng thái:** Đã thống nhất phương án (Phương án 1 + Cách tiếp cận 1)
- **Phạm vi:** Cập nhật phát hành `v1.0.2-R6-UNSIGNED-TEST`, chuẩn hóa `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx`, bổ sung cặp biểu mẫu phản hồi `PHIEU-KHAO-SAT-GOP-Y.html` và `PHIEU-KHAO-SAT-GOP-Y.xlsx` trong thư mục bàn giao.

---

## 1. Bối cảnh và vấn đề cần giải quyết

Sau khi sửa quy tắc nghiệp vụ WPF cũ tại commit `7559cb9` (mã quốc tịch 3 chữ cái `CHN`/`KOR`/`JPN`/`USA`..., cảnh báo trực tiếp dưới mọi ô bắt buộc, tự động nhận diện nghề `Thân nhân` khi `GPLĐ = Khác`), mã nguồn và 360 bài kiểm thử tự động đã đạt. Tuy nhiên thư mục bàn giao hiện hành `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/` vẫn còn hai khoảng trống thực tế:

1. **Bộ cài `R5` chưa chứa mã nguồn `7559cb9`:** `IRM-v1.0.2-windows-x64-offline-setup-R5.exe` được đóng gói từ commit cũ `b42a277`. Theo quy tắc bàn giao tại `AGENTS.md` và `IRM/docs/DELIVERY-LAYOUT.md`, không được ghi đè hoặc đổi nhãn gói R5 cũ mà phải lưu trữ nguyên vẹn R5 sang `02-LUU-TRU/` và đóng gói bản mới `R6`.
2. **File Excel mẫu trong gói bàn giao cũ (`IRM-MAU-IMPORT-TEST-DA-SUA.xlsx`) không tương thích hoàn toàn với database cài mới của IRM:**
   - File 10/11 dùng mã quốc tịch 2 chữ cái (`US`, `KR`, `JP`, `CN`, `AU`, `GB`, `IN`), địa chỉ kết thúc bằng `"Khu nhà mẫu"` (không khớp 3 phường/xã mặc định được seed là `Phường Hồng Gai`, `Phường Bãi Cháy`, `Phường Cẩm Trung`), trộn nhiều nghề không tồn tại trong nhóm mặc định (`Quản lý`, `Chuyên gia`, `Kỹ thuật viên`, `Nhà đầu tư` trong khi nhóm `Kỹ thuật` chỉ seed nghề `Kỹ sư`), và ghi chữ `"Đã có GPLĐ"` / `"Miễn GPLĐ"` / `"Chưa có"` ở cột GPLĐ.
   - Khi cài mới R6 bằng `IRM.exe --bootstrap`, `DatabaseSeeder.SeedCatalogsIfEmptyAsync` tạo sẵn: Công ty `CÔNG TY MẪU - HÃY ĐỔI TÊN`, 5 nhóm nghề (`Quản lý` → `Giám đốc`, `Kỹ thuật` → `Kỹ sư`, `Chuyên gia` → `Chuyên gia tư vấn`, `Lao động phổ thông` → `Công nhân`, `Khác` → `Thân nhân`), 34 quốc tịch mã 3 chữ cái, và 3 phường (`Phường Hồng Gai`, `Phường Bãi Cháy`, `Phường Cẩm Trung`). File Excel test mới phải nạp thành công **10/10 dòng ngay lần đầu** trên bộ danh mục mặc định này.
3. **Thiếu phiếu khảo sát chuẩn hóa cho khách hàng:** Khách hàng cần một công cụ phản hồi chi tiết theo từng phân hệ chức năng, giao diện (bao gồm cảnh báo trường bắt buộc) và định dạng/quy tắc Excel ngay trong thư mục bàn giao.

---

## 2. Phương án lựa chọn

- **Lựa chọn hình thức phiếu khảo sát (Phương án 1):** Kết hợp đồng thời **`PHIEU-KHAO-SAT-GOP-Y.html`** (mở offline trên trình duyệt, lưu nháp tự động, xuất `.csv`/`.json`, in PDF A4) và **`PHIEU-KHAO-SAT-GOP-Y.xlsx`** (bảng Excel có menu thả xuống Data Validation và công thức tự tổng hợp) để phù hợp với cả người thích điền trên web lẫn cán bộ quen dùng Excel. Không thêm công nghệ ngoài: dùng chính thư viện `ClosedXML` có sẵn trong `IRM.Infrastructure` để tạo các file Excel.
- **Lựa chọn nội dung Excel test (Cách 1):** Chuẩn hóa 10 hồ sơ `IRMTEST26001`–`IRMTEST26010` theo danh mục cài mới mặc định (Nhóm nghề `Kỹ thuật`, 3 phường mặc định, 34 quốc tịch 3 chữ cái), phủ đủ 4 trạng thái GPLĐ hợp lệ (`Mã số GPLĐ`, `Miễn`, ô trống, và `Khác` + `Thân nhân`) cùng hồ sơ có thăm thân và hết hạn/sắp hết hạn, đảm bảo nạp thành công **10/10 dòng mới** ngay lần đầu và **10/10 dòng trùng giữ nguyên** ở lần nạp lại thứ hai.
- **Lựa chọn dẫn hướng khách hàng (Hiệu ứng UI ban đầu):** Tận dụng `Driver.js` (`wwwroot/vendor/driverjs/`) và `irmInterop` sẵn có để làm nổi bật nút **"Khảo sát & Góp ý"** trên Topbar ứng dụng ngay lần đầu đăng nhập, đồng thời phục vụ trực tiếp `PHIEU-KHAO-SAT-GOP-Y.html`/`.xlsx` trong `wwwroot/` và thư mục bàn giao.

---

## 3. Chi tiết thiết kế

### 3.1. Bộ cài Windows Offline `R6` (`v1.0.2-R6-UNSIGNED-TEST`)

- Chuyển nguyên vẹn thư mục `BAN-GIAO/01-HIEN-HANH/v1.0.2-R5-UNSIGNED-TEST/` sang `BAN-GIAO/02-LUU-TRU/v1.0.2-R5-UNSIGNED-TEST/` (giữ nguyên toàn bộ tệp và SHA-256 của R5).
- Cập nhật `IRM/deploy/release-manifest.json` (`windows.artifact` → `IRM-v1.0.2-windows-x64-offline-setup-R6.exe`), `IRM/deploy/windows/irm.iss` (`OutputBaseFilename=IRM-v1.0.2-windows-x64-offline-setup-R6`), `IRM/docs/INSTALLATION-v1.0.2.md`.
- Commit sạch mã nguồn trước khi chạy `deploy/windows/build-windows-installer.ps1 -AllowUnsigned` để `SOURCE-REVISION.json` ghi nhận đúng commit mới và `dirty=false`.
- Lưu ý giới hạn ký số & nghiệm thu: giữ nhãn `UNSIGNED-TEST` (`NotSigned`) khi máy build không có Code Signing certificate; ghi rõ phạm vi đã kiểm thử và các phần chưa chạy trên VM desktop nếu có.

### 3.2. File Excel test chuẩn hóa (`IRM-MAU-IMPORT-TEST-DA-SUA.xlsx`)

Giữ nguyên 19 tên cột của file mẫu cũ ở dòng 1 (tất cả đều tự động ghép chính xác bởi `ImportMapping.AutoMapColumns` với 0 cột `skip`):
`Họ tên | Giới tính | Ngày sinh | Quốc tịch | Số hộ chiếu | Địa chỉ | Nghề nghiệp | GPLĐ | Số GPLĐ | Số Visa | Hạn tạm trú | Ghi chú | Thăm thân | Tên người thân | Quan hệ | CMND người thân | Ngày bắt đầu thăm thân | Hạn thăm thân | Ghi chú thăm thân`

Điều kiện nạp thử trên DB mới cài đặt R6: Chọn **Công ty mặc định:** `CÔNG TY MẪU - HÃY ĐỔI TÊN` và **Nhóm nghề:** `Kỹ thuật`.

| STT | Hộ chiếu | Họ tên | GT | Ngày sinh | QT | Địa chỉ (kết thúc đúng Phường mặc định) | Nghề | GPLĐ | Số GPLĐ | Số Visa | Hạn tạm trú | Thăm thân |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | `IRMTEST26001` | `TEST ALEX MORGAN` | Nam | `15/03/1985` | `USA` | `Lô A01, Khu nhà mẫu, Phường Hồng Gai` | `Kỹ sư` | `TEST-GP-001` | `TEST-GP-001` | `TEST-V-001` | `15/06/2027` | Có (`Vợ/Chồng`) |
| 2 | `IRMTEST26002` | `TEST KIM HANA` | Nữ | `22/07/1990` | `KOR` | `Lô A02, Khu nhà mẫu, Phường Bãi Cháy` | `Kỹ sư` | `Miễn` | `` | `TEST-V-002` | `30/11/2026` | Không |
| 3 | `IRMTEST26003` | `TEST SATO KEN` | Nam | `25/04/1988` | `JPN` | `Lô A03, Khu nhà mẫu, Phường Cẩm Trung` | `Kỹ sư` | `` | `` | `TEST-V-003` | `15/10/2026` | Không |
| 4 | `IRMTEST26004` | `TEST LI MEI` | Nữ | `18/08/1992` | `CHN` | `Lô A04, Khu nhà mẫu, Phường Hồng Gai` | `Kỹ sư` | `TEST-GP-004` | `TEST-GP-004` | `TEST-V-004` | `25/01/2027` | Có (`Con`) |
| 5 | `IRMTEST26005` | `TEST DANIEL WEBER` | Nam | `20/09/1980` | `AUS` | `Lô A05, Khu nhà mẫu, Phường Bãi Cháy` | `Kỹ sư` | `Miễn` | `` | `TEST-V-005` | `20/03/2028` | Không |
| 6 | `IRMTEST26006` | `TEST EMMA MARTIN` | Nữ | `16/11/1991` | `GBR` | `Lô A06, Khu nhà mẫu, Phường Cẩm Trung` | `Kỹ sư` | `TEST-GP-006` | `TEST-GP-006` | `TEST-V-006` | `16/09/2027` | Không |
| 7 | `IRMTEST26007` | `TEST JAMES WILSON` | Nam | `17/02/1987` | `GBR` | `Lô A07, Khu nhà mẫu, Phường Hồng Gai` | `Kỹ sư` | `` | `` | `TEST-V-007` | `17/02/2027` | Không |
| 8 | `IRMTEST26008` | `TEST OLIVIA BROWN` | Nữ | `24/05/1994` | `AUS` | `Lô A08, Khu nhà mẫu, Phường Bãi Cháy` | `Kỹ sư` | `Miễn` | `` | `TEST-V-008` | `24/08/2027` | Có (`Vợ/Chồng`) |
| 9 | `IRMTEST26009` | `TEST ARJUN KUMAR` | Nam | `19/06/1986` | `IND` | `Lô A09, Khu nhà mẫu, Phường Cẩm Trung` | `Kỹ sư` | `TEST-GP-009` | `TEST-GP-009` | `TEST-V-009` | `19/07/2027` | Không |
| 10 | `IRMTEST26010` | `TEST ANNA TRAN` | Nữ | `21/12/1993` | `USA` | `Lô A10, Khu nhà mẫu, Phường Hồng Gai` | `Thân nhân` | `Khác` | `` | `TEST-V-010` | `21/12/2027` | Có (`Vợ/Chồng`) |

- **Sheet 2 (`Hướng dẫn`)** giải thích rõ: chọn Công ty `CÔNG TY MẪU - HÃY ĐỔI TÊN`, chọn Nhóm nghề `Kỹ thuật`, giải thích mã quốc tịch 3 chữ cái, đuôi Phường/Xã mặc định, quy tắc GPLĐ (`số GPLĐ` / `Miễn` / trống / `Khác` → `Thân nhân`), cách thử nhập lại phát hiện trùng & cập nhật/hoàn tác.
- Định dạng cột Text (`@`) cho các mã định danh (`Số hộ chiếu`, `Số Visa`, `Số GPLĐ`, `CMND người thân`) để bảo toàn số 0 đầu (`000000000001`).

### 3.3. Bộ đôi Phiếu Khảo Sát Khách Hàng (`PHIEU-KHAO-SAT-GOP-Y.html` & `.xlsx`)

Đồng bộ bảng màu thương hiệu Olive + Cam (`#3f5223`, `#eef5dc`, `#ea580c`) với `HUONG-DAN-CAI-DAT.html`, gồm 4 nhóm đánh giá chính + kết luận:
1. **Thông tin đơn vị & người thử nghiệm**.
2. **Đánh giá từng chức năng (8 phân hệ / 10 hạng mục):** Bộ cài R6, Đăng nhập/Phân quyền/Đổi mật khẩu, Tổng quan Dashboard, Quản lý Nhân viên (5 tab), Công ty & Nhà đầu tư (6 tab), Du học sinh, Xuất báo cáo & Kiểm tra/Đối chiếu, Cấu hình Danh mục & Quản trị.
3. **Đánh giá Giao diện & Cảnh báo trường bắt buộc (4 hạng mục):** Giao diện bộ cài, Bố cục & Light/Dark Web, Tiếng Việt có dấu & thuật ngữ, Cảnh báo viền đỏ + chữ báo lỗi dưới từng ô bắt buộc (`*`).
4. **Đánh giá Định dạng & Quy tắc Excel (8 hạng mục + Bảng đề xuất cột thực tế):** File mẫu test, Chọn Công ty & Nhóm nghề trước khi đọc, Bước Ghép cột & Trợ giúp `?`, Quy tắc Quốc tịch 3 chữ cái, Quy tắc Địa chỉ kết thúc bằng Phường/Xã, Quy tắc GPLĐ & Thân nhân, Bước Xem trước/Cập nhật/Hoàn tác, Biểu mẫu Excel Xuất báo cáo; kèm bảng cho khách hàng điền cấu trúc file Excel thực tế của đơn vị nếu cần bổ sung/điều chỉnh cột.
5. **Kết luận nghiệm thu tổng thể & Đề xuất ưu tiên**.

**Hỗ trợ thao tác trên HTML (`PHIEU-KHAO-SAT-GOP-Y.html`):** Hoạt động 100% offline không cần CDN/Internet; tự lưu nháp `localStorage`; thanh đếm tiến độ điền; xuất kết quả `.csv` (kèm UTF-8 BOM cho Excel), `.json`, sao chép tóm tắt, và in/xuất PDF A4 (`@media print`).
**Hỗ trợ thao tác trên Excel (`PHIEU-KHAO-SAT-GOP-Y.xlsx`):** 3 sheet (`Tổng hợp & Thông tin`, `Chi tiết khảo sát`, `Đề xuất cột Excel thực tế`), gắn `DataValidation` dropdown cho cột mức độ đánh giá (`Đạt yêu cầu`, `Cần chỉnh sửa nhỏ`, `Không đạt / Lỗi`, `Chưa thử`) và công thức `COUNTIF` tự thống kê kết quả.

### 3.4. Hiệu ứng UI ban đầu chỉ dẫn vị trí Feedback

Để khách hàng vừa mở ứng dụng Web hay vừa mở file trong thư mục bàn giao đều nhận ra ngay chỗ phản hồi góp ý:

1. **Nút **"Khảo sát & Góp ý"** trên Topbar ứng dụng (`MainLayout.razor`):** Đặt ở góc phải thanh công cụ (`#btnCustomerFeedback`), hiển thị cho mọi vai trò đăng nhập, có menu truy cập nhanh:
   - *Mở phiếu khảo sát trực tuyến (HTML)* → mở `/PHIEU-KHAO-SAT-GOP-Y.html` (phục vụ nội bộ từ `IRM/IRM/wwwroot/` qua `app.UseStaticFiles()`).
   - *Tải phiếu khảo sát Excel (.xlsx)* → tải `/PHIEU-KHAO-SAT-GOP-Y.xlsx` từ `IRM/IRM/wwwroot/`.
   - *Xem lại chỉ dẫn vị trí góp ý* → kích hoạt lại hiệu ứng spotlight bất kỳ lúc nào.
2. **Hiệu ứng spotlight + viền sáng nhịp đập ở lần đầu truy cập (`irm-interop.js` + `Driver.js` sẵn có + `app.css`):**
   - Trong `MainLayout.razor` (`OnAfterRenderAsync`), kiểm tra khóa `localStorage['irm-r6-feedback-seen']`. Ở lần đầu đăng nhập, bật viền sáng nhịp đập (`irm-feedback-highlight`, tắt animation nếu bật `prefers-reduced-motion: reduce`) quanh `#btnCustomerFeedback` và hiển thị 1 bước `driver.js` spotlight trỏ thẳng vào nút này kèm thông báo giải thích nơi đánh giá từng chức năng/giao diện/Excel.
   - Bấm **"Đã hiểu — Bắt đầu dùng thử"** (hoặc bấm mở nút) sẽ lưu `irm-r6-feedback-seen = "1"` để không làm phiền ở các phiên sau, đồng thời giữ lại nút trên Topbar dễ nhận biết.
3. **Hiệu ứng dẫn hướng ngay khi mở `PHIEU-KHAO-SAT-GOP-Y.html`:** Hiển thị dải hướng dẫn 3 bước nổi bật ở đầu trang (*1. Chọn mức đánh giá → 2. Nhập góp ý / Bảng cột Excel thực tế → 3. Bấm Xuất CSV (Excel) hoặc In PDF gửi lại*) kèm hiệu ứng sáng viền 2,5 giây ở thanh nút xuất kết quả khi mới mở file lần đầu.

---

## 4. Rủi ro và giới hạn đã biết

- Bộ cài `R6` tiếp tục dùng `-AllowUnsigned` (`NotSigned`) do môi trường build không có chứng thư Code Signing; cần ghi rõ trong `DOC-TRUOC-KHI-CAI.txt`, `VERIFICATION.json` và `BAN-GIAO/README.md`.
- Nếu khách hàng nhập dữ liệu thật ngoài 3 phường mặc định hoặc ngoài 5 nghề mặc định, khách hàng cần khai báo trước tại **Quản trị → Danh mục** trước khi import Excel (hoặc phản hồi ngay danh mục cần bổ sung vào Phiếu khảo sát).
