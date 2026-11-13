# Thiết kế hướng dẫn tại chỗ cho import Excel bằng Driver.js

## Ý định và trạng thái

Người dùng muốn đưa đặc tả import vào UI: bong bóng chỉ dẫn xuất hiện trên màn hình import thật trong lúc thao tác, giúp chuẩn bị file, ghép cột, hiểu lỗi và kiểm tra kết quả. Người dùng đã chọn **Driver.js** và yêu cầu lập kế hoạch. Lựa chọn công nghệ và trải nghiệm trực tiếp đã chốt; các chi tiết dưới đây là đề xuất của người viết để người dùng review cùng kế hoạch trước khi triển khai.

Căn cứ nội dung: [đặc tả 35 trường](../../IMPORT-EXCEL-SPEC.md), [hướng dẫn nhanh](../../HUONG-DAN-IMPORT-EXCEL-NHANH.md), source tại `51cecbd`. Đặc tả phản ánh mã ứng dụng `5c9808d`; source ứng dụng không đổi giữa hai mốc. Đây là một tính năng UI trong solution nâng cấp, không phải dự án thay đổi nghiệp vụ import.

## Các cách đã cân nhắc

| Cách | Đánh đổi | Kết luận |
|---|---|---|
| Demo Guideflow nhúng iframe | Học trên bản chụp/mô phỏng, không bám trường đang nhập thật | Người dùng chọn hướng dẫn trên UI thật nên không dùng |
| Driver.js tích hợp với Blazor | Có spotlight, bong bóng, điều khiển tour; cần đồng bộ vòng đời Blazor và popup MudBlazor | Người dùng đã chọn |
| Tự xây toàn bộ bằng MudBlazor | Cùng bộ UI nhưng phải tự xử lý spotlight, vị trí, bàn phím và vòng đời | Không chọn do tăng phần phải bảo trì |

## Global Constraints

- Chỉ thay UI hướng dẫn tại `/import`, static assets và test/tài liệu liên quan; không đổi parser, quy tắc import, schema, seed hoặc dữ liệu nghiệp vụ.
- Giữ Git root `NNN/`, nhánh `develop`, solution `IRM/IRM.sln`; không sửa WPF/reference, không push, không build lại hoặc đổi nhãn bộ cài cũ.
- UI tiếng Việt; ứng dụng chỉ ghi nhận số đã có, không cấp hoặc tự sinh số hộ chiếu, GPLĐ, giấy tờ hoặc định danh.
- Trợ giúp phủ đúng 35 khóa trong `SystemFields.All`, loại `skip`; chín trường bắt buộc giữ đúng như đặc tả, không thêm validator nghiệp vụ.
- Phân biệt rõ “Chương trình kiểm tra” và “Khuyến nghị nhập liệu”; ví dụ danh mục chỉ đúng khi có dữ liệu tương ứng tại database đích.
- Dùng Driver.js `1.12.1` (MIT), phục vụ JS/CSS cục bộ trong `wwwroot/vendor/driverjs/1.12.1/`; không CDN runtime, không analytics, không gửi dữ liệu import ra dịch vụ ngoài.
- Tour chỉ mở theo yêu cầu; không tự chọn file, đổi giá trị/ghép cột, bật tạo công ty, chọn cập nhật, xác nhận import hoặc hoàn tác.
- Tiến độ tour chỉ giữ trong bộ nhớ của trang; đóng/rời trang kết thúc tour, không lưu file, hộ chiếu hoặc dữ liệu hồ sơ vào storage trình duyệt.
- Trợ giúp vẫn dùng được khi tour lỗi; lỗi JS, phần tử thiếu hoặc mất kết nối Blazor không được làm hỏng luồng import thông thường.

## Trải nghiệm đề xuất

Đầu trang Excel có nút **Hướng dẫn từng bước**. Nhấn khi đang ở bước nào thì bắt đầu chương tương ứng, không đặt lại wizard. Không tự mở khi truy cập lần đầu. Chuyển sang nhập thủ công thì tour dừng; tour không hướng dẫn form nhập tay trong đợt này.

Ở bước Ghép cột: thêm nút **Tra cứu 35 trường**, dấu **?** bên cạnh trường đang ghép và dòng mô tả ngắn cho trường đó. Chọn Bỏ qua thì không có dấu ? của một trường dữ liệu. Một hộp tra cứu MudBlazor cho chọn cả 35 trường, xem nội dung chi tiết và đóng; không sửa dữ liệu hoặc ánh xạ khi tra cứu. Mở hộp này dừng tour để hai lớp hướng dẫn không chồng nhau; có thể mở lại tour từ nút đầu trang.

Phác thảo nội dung, không phải màn hình mới đã triển khai:

```text
Import dữ liệu từ Excel           [Hướng dẫn từng bước]
Upload file → Ghép cột → Xem trước → Kết quả

Cột Excel          Trường hệ thống                    [Tra cứu 35 trường]
Số hộ chiếu    →    [Số hộ chiếu ▼] [?]
                   Bắt buộc · Text · Giữ số 0 đầu

           ┌ Ghép đúng trường ──────────────────────┐
           │ Rà trường bắt buộc và cột GPLĐ riêng.  │
           │ Chương 2 · 2/3    [Quay lại] [Tiếp] [×] │
           └───────────────────────────────────────┘
```

Hộp tra cứu dùng cùng nguồn nội dung với mô tả ngắn, không nhét cả 35 mục vào một tour dài. Mỗi trường có tên, nhóm/bắt buộc hay có điều kiện, kiểu Excel, giá trị/kiểm tra, không ghép so với ô trống/cập nhật, ví dụ có điều kiện, lỗi/cách sửa và khuyến nghị riêng. Tên hiển thị lấy từ `SystemFields.All`; không dùng nội dung file người dùng làm HTML cho bong bóng. Hộp có tiêu đề, nút đóng và focus quay lại nơi mở; đọc được trên màn hình hẹp.

## Bốn chương và điểm bám

Mọi điểm bám dùng `data-import-tour` trên phần tử/wrapper do ứng dụng sở hữu, đặt trong root có ID riêng cho mỗi instance trang. Không bám bằng text dịch, chỉ số cột, ID nội bộ MudBlazor hoặc ID hồ sơ.

| Chương | Điểm bám theo thứ tự | Nội dung cốt lõi |
|---|---|---|
| 1 — Chuẩn bị | `upload`, `company`, `career-group`, `read-options`, `create-company`, `go-mapping` | `.xlsx`, Text/số 0 đầu, Nam/Nữ trong file; công ty mặc định; nghề đúng nhóm; sheet/vùng dòng; tạo công ty chỉ khi chủ động bật; bấm Tiếp tục thật |
| 2 — Ghép cột | `field-lookup`, `mapping`, `go-preview` | Tra cứu 35 trường; chín bắt buộc, GPLĐ hai cột, ô trống/không ghép, cặp mở rộng; người dùng tự bấm Tiếp tục để tạo preview |
| 3 — Xem trước | `preview-summary`, `preview-table`, `execute-import` | Mới/đã có/thay đổi/lỗi; mặc định bỏ qua thay đổi; cảnh báo, lỗi ghi cả giao dịch; người dùng tự quyết định và bấm Bắt đầu Import |
| 4 — Kết quả | `result-summary`, `result-errors` (nếu có), `result-actions` | Thành công không có nghĩa mọi dòng được ghi; đọc bộ đếm/lỗi; kiểm tra hồ sơ sau import, đặc biệt giới tính; giới hạn hoàn tác |

Ở ba điểm chờ `go-mapping`, `go-preview`, `execute-import`, nút Tiếp của tour không thay hành động ứng dụng. Hiển thị lời “Thực hiện thao tác trên màn hình để tiếp tục”; người dùng nhấn nút ứng dụng thật. Chỉ khi Blazor báo bước đã đổi và render xong mới mở chương kế. Nếu đọc file/preview thất bại, vẫn ở chương cũ, không giả vờ đã xong.

Quay lại trong bong bóng chỉ đổi lời hướng dẫn trong cùng chương. Muốn quay lại bước nghiệp vụ thì dùng nút Quay lại thật, tour đồng bộ theo bước đó. Kết thúc chương 4 hoặc đóng bằng ×/Esc dừng tour; render tiếp theo không tự mở lại.

## Đồng bộ vòng đời và tương tác

- Một wrapper JS quản lý Driver.js; Blazor chỉ gửi trạng thái UI sau render: `stage` (1–4), `isBusy`, `isManual`, `contextVersion`. Không gửi nội dung file, tên, số hoặc preview rows.
- `start(rootId, snapshot): boolean` bật tour tại chương hiện tại; trả false khi thiếu Driver/root để UI báo hướng dẫn chưa sẵn sàng mà import vẫn dùng được.
- `sync(rootId, snapshot): void` chỉ cập nhật tour đã bật, không tự bật tour. Render lặp cùng trạng thái không reset lời đang xem. Đổi chương do thao tác thật mới bắt đầu chương mới.
- Khi busy, ẩn spotlight và giữ vị trí; khi hết busy, tiếp tục tại chương đúng. Việc hủy tạm này không được nhầm với người dùng chủ động đóng tour.
- `contextVersion` tăng khi thay một file đã tải bằng file khác hoặc ResetWizard; sync thấy thay phiên thì dừng tour để tránh chỉ dẫn cũ. Lần chọn file đầu tiên không tăng phiên và không tự dừng tour đang hướng dẫn tải file.
- `stop(rootId): void` tắt tour và giữ trang; `dispose(rootId): void` hủy Driver, observer/listener và registry của root, gọi lặp an toàn. Root có ID instance riêng để callback muộn không tác động trang mới.
- Chuyển nhập thủ công, mở hộp trợ giúp, reset/thay file hoặc rời trang đều dừng theo quy tắc trên. Dùng `IAsyncDisposable` để hủy interop và stream; mất circuit không làm dispose ném lỗi ra UI.
- Driver dùng selector scoped root, `overlayClickBehavior: "none"`, nút tiếng Việt, cho Esc/Đóng; không tự `.click()` hoặc gọi hàm nghiệp vụ. Các mục cuối chờ thao tác dùng action instruction của bản Driver đã khóa, không lấy click là bằng chứng request thành công.
- Cho phép tương tác với popup chọn công ty/nhóm/ghép cột đang mở của MudBlazor ngoài root (`.mud-popover-open`) qua `allowedElements`; thử thật để không chặn chọn dropdown. Không cho tour gọi hoặc xác nhận nghiệp vụ thay người dùng.
- Điểm bám thiếu: chờ tối đa 1.500 ms với tài nguyên observer được hủy khi dừng; optional `result-errors` được bỏ khi không có. Điểm bắt buộc vẫn thiếu thì đóng tour và báo có thể mở lại, không hiện bong bóng vô chủ hoặc treo vô hạn. Kiểm tra trước/đồng bộ quanh API Driver thay vì dựa mặc định missing element hiển thị giữa trang.
- Dành một vùng thông báo trống `data-import-tour="notice"`, `role="status"`, `aria-live="polite"` cho wrapper ghi text khi điểm bám mất sau khi đã mở. Nội dung cố định: “Hướng dẫn chưa sẵn sàng ở vị trí này. Bạn có thể mở lại; thao tác import vẫn dùng được.” Không chèn HTML/file vào vùng này. Thiếu thư viện/root lúc start được trả false cho Snackbar phía Blazor.
- Tôn trọng `prefers-reduced-motion`, light/dark, cuộn và đổi kích thước; kiểm tra bàn phím, focus, popup/dropdown, sidebar và processing overlay. CSS hướng dẫn nằm riêng, không đổi token theme toàn ứng dụng.

## Nội dung bắt buộc giữ đúng

- Nam → Nam, nội dung không trống khác → Nữ; preview hiện không có cột giới tính. Rà file trước, kiểm tra hồ sơ sau, không hứa xem được giới tính tại preview.
- Quốc tịch theo mã/tên thật của đích; nghề đúng dấu/nhóm; địa chỉ lõi kết thúc tên Wards, khác mã địa bàn mở rộng.
- GPLĐ trống, Miễn, Khác, số thật; nhà đầu tư theo đầu tư đúng công ty; Đã có/Chưa có không phải nhãn trạng thái hợp lệ để nhập như form. Hai cột mâu thuẫn báo lỗi.
- Ngày lõi/ngày mở rộng; năm bốn số; danh sách giấy tờ `;` giữ vị trí, ngày từ dùng chung; cột có điều kiện không tự tạo bản ghi khi thiếu chủ/cặp.
- Không ghép khác ô trống khi cập nhật; ghi chú/GPLĐ/kết quả lõi có thể bị thay bằng mặc định. V010 trống không xóa bản ghi cũ; không hứa import cập nhật hạn kỳ lưu trú đã có.
- Preview không bảo đảm ghi; lỗi phần mở rộng có thể hoàn tác cả giao dịch; dữ liệu thay đổi sau preview cần xem lại. Hoàn tác có chốt phiên/phụ thuộc/sửa thủ công.
- Sửa câu kết quả “có thể hoàn tác bất cứ lúc nào” thành: “Có thể hoàn tác từ Lịch sử Import khi dữ liệu chưa bị chỉnh sửa hoặc có phiên/phụ thuộc mới hơn.” Đây là chỉnh lời UI, không đổi quyền hay logic rollback.

## Tổ chức nguồn và đóng gói

Nội dung 35 trường là catalog chỉ đọc trong Web feature, tách khỏi `SystemFields` và Application parser để không trộn tài liệu với luật nghiệp vụ. Test đối chiếu tập khóa/tên/bắt buộc với đặc tả và source. Không đọc Markdown trực tiếp trên server hoặc sao chép source vào wwwroot; khi luật thay đổi phải review cả Markdown và catalog UI.

Driver.js đóng gói dưới dạng IIFE cùng CSS và LICENSE bản `1.12.1`; thêm manifest nguồn, version, SHA-256 từng tệp và integrity tarball `sha512-upBRqQW+bz+yfH7ee1gjWBaq0ue7i2343f2U3WzIUc3bhs5hwb2jz86cuB8gArRh3nfNp77CLNgkUtArTYTKFA==`. Thông tin package đã đọc từ npm registry chính thức ngày 13/11/2026; không dùng `@latest` trong bản chạy. Không thêm npm build pipeline hoặc thay version các package .NET.

Wrapper và CSS đặt riêng tại `wwwroot/js/import-tour.js`, `wwwroot/css/import-guidance.css`. Đăng ký asset cục bộ trong App.razor; không sửa `irm-interop.js` cho tính năng độc lập này. Đồng bộ feature ở partial code-behind mới, không nhét thêm controller tour vào khối xử lý dữ liệu hiện có.

## Kiểm chứng và nghiệm thu

- .NET: catalog đúng 35 khóa, chín bắt buộc; nội dung đặc biệt và tên khớp; HtmlRenderer render trợ giúp bằng text được encode, không thực thi HTML từ tên/cột; các section có điểm bám đúng theo trạng thái; bridge chỉ gửi snapshot UI, không gọi import/rollback.
- JavaScript `node:test`: điều khiển có bật/tắt, busy, render lặp, đổi chương, phiên mới, thiếu phần tử, dispose, dropdown allowedElements và thiếu thư viện; fixture dùng Driver spy cho wrapper. Đây không thay cho thử với Driver thật.
- Browser thật, UI hiện tại: mở/đóng/mở lại ở từng bước, chọn file và dropdown thật, đổi bước/quay lại, tra cứu, bàn phím, 390 px/1.280 px, light/dark, mạng ngoài bị chặn. Kiểm tra request không gửi dữ liệu ra ngoài và tour không tự ghi dữ liệu.
- Thử luồng cần ghi/hoàn tác chỉ trên database test biệt lập `IRM_RefactorTest_*` với dữ liệu giả; không bấm xác nhận ghi trên database local/nghiệp vụ. Nếu không có môi trường/browser automation phù hợp thì báo phần chưa xác minh, không thay bằng assertion trên chuỗi markup.
- Chạy build Release, bộ test .NET có SQL Server biệt lập khi cấu hình được và toàn bộ test JS; review staged diff, commit local, không push. Không tạo bộ cài trong đợt này.

## Phần người viết tự đề xuất để review

Tour mở theo yêu cầu; bốn chương theo wizard; tra cứu cả 35 trường bằng hộp MudBlazor; tiến độ chỉ trong bộ nhớ; Driver cục bộ khóa version; tour dừng khi đổi ngữ cảnh; chỉ sửa lời rollback đang sai. Các lựa chọn này chưa được gán thành yêu cầu do người dùng tự nêu. Chi tiết bố trí, khoảng cách và cách tách helper test giao cho implementer trong các ràng buộc trên.

## Builder check

Đã có một lượt đọc độc lập, chỉ đọc source/thiết kế: “nothing important left”. Không phát sinh câu hỏi cần người dùng quyết định thêm để viết kế hoạch. Kế hoạch và các đề xuất UI vẫn cần được người dùng review trước khi triển khai.

## Nguồn kỹ thuật

- [Driver.js installation và IIFE](https://driverjs.com/docs/installation).
- [Configuration: action instruction, allowedElements, missing elements, hooks](https://driverjs.com/docs/configuration).
- [Wait for a Save: đợi ứng dụng xác nhận thay đổi](https://driverjs.com/docs/wait-for-a-save).
- Package metadata: `https://registry.npmjs.org/driver.js/1.12.1` (đã đọc qua HTTP, không cài đặt).
