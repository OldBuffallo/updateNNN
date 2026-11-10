# Phân tích cấu trúc IRM và phản hồi lỗi hiển thị

Ngày: 10/11/2026 (Asia/Bangkok). Source được khảo sát tại commit `1081a21`, nhánh `develop`. Đây là báo cáo đọc source và chẩn đoán; chưa triển khai bản sửa UI hoặc phát hành bộ cài mới.

## 1. Kết luận

Cấu trúc solution nâng cấp đã có phân tầng rõ ràng. Vấn đề hiển thị khách phản ánh tập trung ở cơ chế theme dùng chung và cách phối hợp CSS với MudBlazor, không cần đổi cấu trúc database hay viết lại nghiệp vụ để giải quyết.

Ảnh khách gửi cho thấy lỗi nghiêm trọng nhất ở bước Ghép cột: chữ và ô chọn gần như trắng trên nền trắng/vàng nhạt. Ảnh dark mode vẫn đọc được nhiều thành phần. Đây là vấn đề tương phản và tính nhất quán giao diện; không phải lỗi định dạng Excel.

Source có một lỗi đã xác minh: khi khôi phục lựa chọn dark đã lưu, MudBlazor chuyển sang dark nhưng thuộc tính `data-theme` điều khiển CSS không được cập nhật. CSS đồng thời ghi đè nền bảng bằng biến riêng. Sự kết hợp này có thể tạo chữ sáng của MudBlazor trên nền sáng của CSS, phù hợp với biểu hiện trong ảnh. Chưa lấy computed styles hoặc xác nhận commit/binary trực tiếp trên trình duyệt khách, nên không coi đây là phép tái hiện đầy đủ môi trường khách hàng.

## 2. Cấu trúc hiện hành

```text
NNN/                                      Git root chung
├── immigration-reportmanager-master/     WPF cũ để đối chiếu nghiệp vụ/database
├── IRM/                                  Solution nâng cấp
│   ├── IRM.sln
│   ├── IRM/                              Blazor Server, UI, hosting/auth adapter
│   ├── IRM.Core/                         Entity, enum, quy tắc thuần
│   ├── IRM.Application/                  Dịch vụ nghiệp vụ, DTO, validation, port
│   ├── IRM.Infrastructure/               SQL/EF, Excel, file, đồng bộ, bảo trì
│   ├── IRM.Tests/                        Kiểm thử và baseline tương thích
│   ├── deploy/                          Đóng gói, cài đặt, SQL hỗ trợ
│   ├── docs/                            Kiến trúc, hướng dẫn, thiết kế, kiểm chứng
│   └── reference/                       Demo và thử nghiệm, ngoài solution build
└── BAN-GIAO/                             Bộ cài và dữ liệu bàn giao; ngoài Git
```

Luồng thông thường: **UI → interface/service Application → store port → Infrastructure → SQL Server**. Web ghép các phụ thuộc qua DI. SQLite phục vụ test, không phải database vận hành của bản web.

| Project | File C# được Git quản lý | File Razor | Phụ thuộc project |
|---|---:|---:|---|
| IRM (Web) | 12 | 36 | Application, Infrastructure |
| IRM.Core | 50 | 0 | Không có |
| IRM.Application | 149 | 0 | Core |
| IRM.Infrastructure | 94 | 0 | Application, Core |
| IRM.Tests | 25 | 0 | Các project ứng dụng |

Số liệu tính từ `git ls-files`; không tính bin/obj, cache, media cài đặt hoặc source tham chiếu. Đây là số file, không phải số test, màn hình hay entity.

Các module UI đang có: Reports (dashboard/search/statistics/report), Companies, Employees, ForeignPersons, Students, Accommodations, Inspections, ImportExport và Administration. Residence/Documents có dịch vụ riêng nhưng UI được tích hợp trong hồ sơ hiện có. Không cần tạo thêm route để khớp tên thư mục.

## 3. Điểm tốt và điểm cần cải thiện

| Nội dung | Nhận xét | Hệ quả |
|---|---|---|
| Phân tầng project | Các ProjectReference phù hợp hướng phụ thuộc đã quy định | Sửa theme có thể giới hạn trong Web |
| Chia theo tính năng | Import và Admin đã tách component/partial | Dễ khoanh vùng hơn một trang chứa mọi chức năng |
| Database kế thừa | Tên entity, schema và migration có baseline kiểm tra | Vẫn phải nghiệm thu với database cũ thật; baseline model không chứng minh mọi database cũ đã được nâng cấp |
| CSS toàn cục | app.css có 2.456 dòng, 41 `!important` | Rủi ro ảnh hưởng chéo và sửa chỗ này lệch chỗ khác |
| Trang lớn | Employees 700 dòng, Home 471 dòng; MainLayout 328 dòng | UI/state/style còn tập trung; chưa cần tách hàng loạt để sửa theme |
| Theme | Palette C#, biến CSS và màu biểu đồ JS được định nghĩa riêng | Cùng một ý nghĩa màu có nhiều giá trị và nhiều thời điểm cập nhật |
| Kiểm thử | Có test nghiệp vụ, tương thích, quyền và điều phối component; chưa thấy bộ kiểm tra browser/light-dark/tương phản trong source test | Build/test C# đạt vẫn có thể bỏ sót chữ trắng trên nền trắng |

Chi tiết kiến trúc nền: [ARCHITECTURE.md](ARCHITECTURE.md). Chênh lệch với database WPF thực tế: [biên bản database/import](IMPORT-LEGACY-DATABASE-AUDIT-2026-11-10.md).

## 4. Chẩn đoán theme và lỗi hiển thị

### F1 — Khôi phục theme không đồng bộ hai hệ màu

Mức ưu tiên cao vì có thể làm không đọc được trường thao tác.

- `IRM/Components/Shared/MainLayout.razor`: `_darkMode` mặc định false; sau render đầu gọi `irmInterop.getTheme()` và gán `_darkMode`.
- `IRM/wwwroot/irm-interop.js`: `getTheme()` chỉ đọc localStorage. `initTheme()` có cập nhật `data-theme` nhưng không được gọi tại các điểm khởi tạo đã khảo sát.
- CSS mặc định ở `:root` là light; CSS dark chỉ có hiệu lực khi `[data-theme="dark"]` hiện diện.
- `ToggleTheme()` lại gọi `setTheme()`, nên bấm nút có thể làm hai trạng thái khớp lại. Điều này giải thích vì sao chuyển chế độ có thể làm giao diện dễ đọc hơn.

Harness Node chạy chính file JavaScript hiện có cho kết quả:

| Lựa chọn đã lưu | Giá trị getTheme trả cho layout | data-theme sau khi đọc | CSS áp dụng khi không có thuộc tính |
|---|---|---|---|
| light | light | Không có | Light |
| dark | dark | Không có | Light |

Đây là kiểm chứng hành vi JS kết hợp đọc code C#, không phải screenshot/browser test hay khẳng định toàn bộ nguyên nhân trên máy khách.

### F2 — CSS nền bảng và màu chữ không đi cùng một nguồn

`app.css` đặt nền `.mud-simple-table`, `.mud-table-container` và `.mud-data-grid` bằng `--bg-surface`; nền header/hover cũng dùng biến CSS riêng. Trong khi đó, chữ và select của MudBlazor dùng palette của MudThemeProvider.

`ImportMappingSection.razor` dùng MudSimpleTable, MudSelect và chip trạng thái. Các dòng Bỏ qua có nền `--mud-palette-warning-hover`. Cần kiểm tra đồng thời nền bảng, nền dòng, chữ trong ô và chữ trong select; đổi riêng màu button không giải quyết hết ảnh khách gửi.

### F3 — Một số màu hiện tại có tương phản thấp

Tính theo công thức relative luminance/sRGB từ các giá trị source; đây không phải phép đo pixel ảnh JPEG hay kết quả computed style của từng control.

| Cặp màu trong source/đối chiếu | Tương phản xấp xỉ | Ý nghĩa |
|---|---:|---|
| Chữ muted light #94a3b8 / nền trắng #ffffff | 2,56:1 | Khó đọc đối với chữ nội dung kích thước thường |
| Chữ muted light #94a3b8 / nền #f6f8fb | 2,41:1 | Placeholder và nội dung phụ cần rà soát |
| Chữ muted dark #6b7280 / nền #1c1f26 | 3,41:1 | Dark mode cũng chưa phải mọi vị trí đều đạt |
| Chữ trắng / primary light #2563eb | 5,17:1 | Không có căn cứ kết luận mọi nút primary light vốn đều sai |
| Chữ trắng / primary dark #3b82f6 | 3,68:1 | Nếu control thực tế dùng chữ trắng kích thước thường thì cần điều chỉnh cặp màu |

WCAG 2.2 yêu cầu chữ thông thường đạt 4,5:1; chữ lớn có ngoại lệ 3:1. Thành phần nhận diện/trạng thái cần thiết của control có yêu cầu 3:1 với màu liền kề. Control bị vô hiệu hóa có ngoại lệ; nút Tiếp tục màu xám trong ảnh upload chưa chọn file chưa đủ căn cứ xem là lỗi.

Nguồn: [W3C — Contrast Minimum](https://www.w3.org/WAI/WCAG22/Understanding/contrast-minimum.html), [W3C — Non-text Contrast](https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast.html).

### F4 — Theme đăng nhập và biểu đồ chưa thống nhất

LoginLayout tạo MudThemeProvider mặc định riêng; MainLayout có palette tùy chỉnh riêng. JS `_getChartColors()` còn chọn các mã màu cố định theo data-theme. Cần thống nhất cả route login, menu/dialog/popover và chart khi sửa toàn ứng dụng.

## 5. Phạm vi người dùng đã chọn

Người dùng chọn **chuẩn hóa light/dark mode toàn IRM, giữ bố cục hiện tại**. Phương án sửa riêng Import có phạm vi nhỏ hơn nhưng không giải quyết cơ chế theme dùng chung nên không được chọn.

Thiết kế cụ thể: [2026-11-10-irm-theme-consistency-design.md](superpowers/specs/2026-11-10-irm-theme-consistency-design.md). Việc chọn phạm vi chưa được ghi nhận là phê duyệt thiết kế hoặc xác nhận một bản UI đã sửa.

## 6. Giới hạn và bước nghiệm thu

- Chưa thay code sản phẩm, database, migration, business rule hoặc bộ cài R4 trong đợt báo cáo này.
- Chưa có ứng dụng lắng nghe trên các cổng kiểm tra thông thường 5050/5000/5001/8080 tại thời điểm khảo sát; không chạy ứng dụng vào database vận hành để tạo ảnh kiểm tra.
- Cần tái hiện trên bản chạy đã xác định revision, đo computed styles và chụp light/dark cùng trang trước khi kết luận lỗi khách đã hết.
- Nghiệm thu ưu tiên: import/ghép cột ở cả hai mode; chọn mode rồi F5; mở lại tab; điều hướng; menu/dialog/select; tài khoản quản trị và nhập liệu; trạng thái enabled/disabled/loading/focus.
- Source hiện tại và bộ cài đã giao là hai đối tượng khác nhau. Sửa/commit source không tự cập nhật ứng dụng trên máy khách.
