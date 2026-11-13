# Kiểm chứng hướng dẫn import và tiếng Việt trên UI

## Cập nhật bước chuẩn bị file

Đợt triển khai đầu của phương án A bổ sung màn hình **Chuẩn bị file**, nút tải mẫu và tra cứu 35 trường trước phần nạp file. Nội dung dưới phần này lưu bằng chứng của đợt hướng dẫn trước đó.

File mẫu mới thuộc commit `54f4a66`, thay workbook 19 cột bằng workbook 35 cột với ba sheet **Hồ sơ**, **Hướng dẫn**, **Danh mục tham chiếu**. Vùng nhập bắt đầu trống từ dòng 2. Danh mục lấy từ database đích và lọc mục đã xóa theo quy tắc hiện có. Tải mẫu có kiểm tra tài khoản đăng nhập do workbook chứa danh mục thực tế.

Màn hình tiến trình có bốn bước. Ghép cột hiện tại nằm trong bước nạp file. Người dùng có thể quay lại chuẩn bị và tiếp tục với file đã có. Các phần nạp, ghép cột và bảng xem trước hiện tại tiếp tục phục vụ đợt đầu. Bản xem trước theo bố cục hồ sơ thuộc các đợt tiếp theo của [thiết kế A](superpowers/specs/2026-11-13-import-template-first-design.md).

Bằng chứng tự động gồm kiểm thử round-trip 35 tiêu đề qua bộ đọc/ghép cột, parse một dòng dữ liệu bắt buộc giữ số 0 đầu, danh mục đích/mục đã xóa/danh mục rỗng, truy cập ẩn danh, trạng thái đầu/reset/quay lại, tải lỗi rồi thử lại, chặn bấm tải lặp và tour từ chuẩn bị sang nạp file. Build Release đạt với 0 cảnh báo và 0 lỗi. Toàn bộ .NET đạt 336/336 với SQL integration bật, JavaScript đạt 47/47, đều 0 bỏ qua.

Review độc lập phát hiện cột phụ như **Số Visa cũ** có thể chiếm trường trước cột chuẩn **Số Visa**. Hai regression thất bại tái hiện việc bỏ cột chuẩn khi cột phụ đứng trước. Đã sửa bằng cách dành trường cho mọi tiêu đề chuẩn trên cả sheet trước khi dò từ khóa cho các cột còn lại. Ba ca thứ tự cột và tiêu đề trùng cùng bộ kiểm thử toàn bộ đạt sau sửa.

Kết nối `@oai/sky` ban đầu gặp lỗi native pipe `The system cannot find the file specified (os error 2)`. Sau khi công cụ CUA mới khả dụng, đã dùng Chrome thật tại `127.0.0.1:5052/import` với bản Release `ce63c9a`. Đã quan sát màn hình chuẩn bị, bấm tải mẫu, lưu bằng hộp Save As vào `BAN-GIAO/03-DU-LIEU-TEST/2026-11-13-preparation/`, đi qua cả ba gợi ý chuẩn bị và bấm nút ứng dụng để sang nạp file. Tour chuyển đúng sang gợi ý upload. File đã lưu có ba sheet, 35 cột, chỉ dòng tiêu đề có nội dung tại sheet Hồ sơ. SHA-256 và kết quả kiểm tra cấu trúc nằm cạnh workbook.

Lượt browser này dùng mức zoom 67% sẵn có. Kiểm tra kích thước màn hình nhỏ, zoom 200%, tải lỗi trên browser và thao tác ghi dữ liệu toàn luồng cần được nghiệm thu tiếp ở các đợt sau. Kiểm thử tự động đã kiểm tra tải lỗi và thử lại.

Ngày 13/11/2026. Source ứng dụng được kiểm tra: `3573857` (bao gồm sửa tiếng Việt `c5d299d`); nghiệp vụ đọc/ghi import giữ mốc `5c9808d`. Các commit tài liệu kế tiếp không thay mã ứng dụng. Không build bộ cài, không gán source mới cho R4/các gói đã giao.

## Đã triển khai

- Driver.js 1.12.1, JS/CSS/LICENSE cục bộ, manifest SHA-256 và integrity tarball; không CDN/Guideflow hoặc analytics runtime.
- Tour mở theo yêu cầu, bốn chương theo wizard. Không tự chọn file, đổi ánh xạ/cập nhật hoặc gọi import/hoàn tác. Bridge gửi đúng bốn thuộc tính UI, không gửi file/hồ sơ.
- Tra cứu đủ 35 trường, chín bắt buộc, theo tên SystemFields; hỗ trợ ? trên cột đã ghép và tra cứu toàn bộ. Text Razor được encode, không dùng HTML từ file.
- Tạm ẩn lúc busy, giữ vị trí khi render lặp, đổi chương theo trạng thái thật; đóng, đổi ngữ cảnh và dispose có hủy observer/timer. Lỗi hướng dẫn không chặn thao tác import.
- Sửa thứ tự lớp popup MudBlazor bằng CSS scope riêng, có override cho z-index inline. Cache token CSS/JS đổi thành `20261113-r2`.
- Đồng nhất lời điều kiện hoàn tác: dữ liệu chưa bị chỉnh sửa **và chưa có** phiên/phụ thuộc mới hơn.

## Lỗi tiếng Việt đã tái hiện và sửa

`IRM/Features/Administration/Admin.Catalogs.cs` chứa lẫn byte codepage cũ và UTF-8; một số dấu đã mất thành dấu hỏi trong chính chuỗi gốc. Không thể chỉ đổi font để khôi phục chúng. Đã phục hồi 25 thông báo lưu/xóa/kiểm tra lĩnh vực, nghề, quốc tịch, quận/huyện, phường/xã và lưu toàn bộ tệp UTF-8; phần code ngoài chuỗi giữ nguyên.

Bảng nhật ký có “Loc”, “Loai”, “Mo ta”, “Khong co du lieu”; đã sửa thành “Lọc”, “Loại”, “Mô tả”, “Không có dữ liệu”, cùng nhãn lọc tiếng Việt. Không dịch mã hành động, khóa máy, alias không dấu được hỗ trợ hoặc tên ví dụ theo hộ chiếu.

Bảy test trước sửa đều thất bại: strict UTF-8 báo byte EA không hợp lệ, năm thông báo kiểm tra form thiếu dấu, phần render nhật ký thiếu nhãn đúng. Sau sửa 7/7 đạt. Audit 61 tệp nguồn Web ngoài vendor đọc strict UTF-8, không có ký tự thay thế U+FFFD. Tour/tra cứu import không có bằng chứng nguồn bị mất dấu; không thay font khi chưa có bằng chứng lỗi font.

## Kiểm tra tự động đã chạy

| Kiểm tra | Kết quả |
|---|---|
| `dotnet build IRM.sln -c Release` | 0 warning, 0 error |
| `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --no-build` | 318 đạt, 0 lỗi, 0 bỏ qua |
| Toàn bộ `IRM.Tests/Browser/*.test.cjs` bằng Node | 46 đạt, 0 lỗi, 0 bỏ qua |
| Catalog và render trợ giúp | 35 khóa chính xác, chín bắt buộc; text ví dụ được encode |
| Lỗi popup | Quan hệ lớp và precedence z-index inline có test RED → GREEN |
| Lời hoàn tác | Test chỉ dẫn không chấp nhận “hoặc có phiên” RED → GREEN |
| Vendor manifest | Ba checksum JS/CSS/LICENSE khớp; giữ byte nguyên bản qua Git -text |

SQL tests bật bằng `IRM_TEST_SQLSERVER` với Windows integrated authentication tới SQL Express; chỉ tạo/drop database generated `IRM_RefactorTest_*`. Không chạy bài ghi/hoàn tác trên database local/nghiệp vụ. Bài UI chuẩn bị một database biệt lập với dữ liệu giả từ fixture hiện có.

## Quan sát browser thật và giới hạn

Đã mở Chrome thật với ứng dụng thử tại cổng 5051, database biệt lập; quan sát tour chương 1, chuyển từ chuẩn bị file sang công ty, tải workbook golden bằng hộp chọn file thật và thấy tour không tự dừng ở lần tải đầu. Autocomplete công ty mở được, nhưng popup bị che một phần bởi tour; đây là căn cứ sửa CSS.

Người dùng dừng Computer Use bằng Escape trước khi recheck bản CSS cuối. Không thực hiện thêm hành động UI sau đó. Không gọi spy/CSS assertions là đã nghiệm thu tương tác browser thật.

| Trường hợp còn thiếu nghiệm thu browser | Trạng thái |
|---|---|
| Chọn dropdown chuột/bàn phím sau CSS cuối, focus dialog | Chưa xác minh lại |
| Chương 2–4, preview lỗi, busy/latency thực và quay lại | Có test state/render; chưa chạy đủ trên browser thật |
| Ghi 12 mới/15 lỗi và hoàn tác bằng UI với golden | Chưa thực hiện; kỳ vọng này chỉ có điều kiện database test chuẩn bị đúng |
| Viewport 390×844/1280×800, zoom 200%, light/dark, reduced motion | Chưa nghiệm thu đủ trên browser thật |
| Chặn mạng ngoài nhưng giữ circuit, thiếu asset/anchor thực, rời trang | Có coverage adapter/fallback; browser case chưa chạy đủ |

**Không coi biên bản này là nghiệm thu UI đầy đủ.** Có thể dùng test tự động để đánh giá source; cần tiếp tục phần browser trên môi trường biệt lập trước khi tuyên bố hoàn tất nghiệm thu thực tế.

## Review và phạm vi giữ lại

Một reviewer fresh-context rà toàn bộ feature import, không thấy Critical, thấy một Important về câu hoàn tác trong tour; đã đối chiếu source và sửa bằng regression test. Không có Minor bị hoãn. Lỗi encoding Quản trị được tác giả tìm và tái hiện bằng test trong yêu cầu sửa tiếng Việt tiếp theo, không gán thành phần reviewer đã độc lập kiểm chứng.

Review cũng phát hiện **file mẫu tải từ nút Tải file mẫu đang có nội dung cũ** trong `IRM.Infrastructure/Excel/SqlExportStore.cs`: hướng dẫn bắt buộc/GPLĐ không thống nhất đặc tả và ví dụ hai cột GPLĐ có thể mâu thuẫn. Generator chưa đổi vì nằm ngoài đợt UI/copy đã chốt; đây là rủi ro tồn tại, không dùng file tải sẵn làm bằng chứng mọi dòng sẽ import thành công. Hai workbook kiểm chứng lịch sử và checksum không bị thay đổi.

Giữ `develop` theo hướng dẫn workspace, không cách ly thay đổi đồng thời. Thêm `.gitattributes` chỉ trong thư mục vendor để core.autocrlf không đổi byte/checksum. Giữ mã/alias ASCII có ý nghĩa tương thích. Sửa AND cho lời rollback, không đổi quyền hoặc logic hoàn tác. Không push, không có schema/parser/seed thay đổi.

Log, source revision và checksum build phục vụ đối chiếu nằm tại `BAN-GIAO/05-HO-TRO/2026-11-13-import-guidance-3573857/` (Git-ignored), không chứa credentials hoặc database runtime. Đây là build source phục vụ kiểm tra, không phải bộ cài mới đã bàn giao.
