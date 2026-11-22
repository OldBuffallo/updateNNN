# Thiết kế chuẩn hóa light/dark mode toàn IRM

Ngày: 10/11/2026. Trạng thái: **Đã được người dùng phê duyệt thiết kế và yêu cầu triển khai**. Phạm vi: toàn ứng dụng, giữ bố cục hiện tại.

## Mục tiêu và lý do

Khách phải đọc được nội dung, button, bảng ghép cột và ô chọn trong light mode lẫn dark mode, không phải đổi mode để nhìn rõ. Khôi phục lựa chọn sau F5/mở tab và điều hướng phải thống nhất giữa MudBlazor, CSS, login và biểu đồ.

Giữ nguyên bố cục, sidebar, thứ tự/menu nhãn chức năng, route và luồng thao tác nghiệp vụ. Cách phối màu xanh chủ đạo hiện có được giữ; chỉ điều chỉnh màu/độ tương phản và trạng thái tương tác khi cần để đọc được. Không dùng đợt này để thiết kế lại dashboard hay thêm chức năng quản lý database.

Không thay database, schema, migration, mã danh mục cũ, entity, quyền, thuật toán đăng nhập, import/rollback hoặc định dạng dữ liệu. Không thay phiên bản thư viện để chữa lỗi theme. Không sửa source WPF.

## Các phương án

1. Sửa riêng bảng và nút màn hình Import: thay đổi ít hơn nhưng theme vẫn có thể lệch ở nơi khác và sau tải lại.
2. Chuẩn hóa cơ chế theme và các cặp màu toàn Web: phạm vi rộng hơn, giải quyết lỗi nguồn và rà soát những control cùng cơ chế.

Người dùng đã chọn phương án 2 và phê duyệt thiết kế bên dưới bằng phản hồi “Duyệt thiết kế, triển khai theo tài liệu”.

## Nguồn theme duy nhất

- Giữ đúng hai lựa chọn Light/Dark, khóa localStorage `irm-theme`; mặc định Light khi chưa có lựa chọn hợp lệ. Không thêm chế độ Auto/System hoặc lưu tùy chọn vào database.
- Gom palette MudBlazor vào một cấu hình C# dùng chung trong Web, ví dụ `Components/Theming/IrmTheme.cs`. Đặt rõ các cặp màu chữ/nền cho primary, secondary, success, warning, error, info và trạng thái tương tác.
- Một component theme dùng chung ở gốc interactive `Components/Routes.razor` sở hữu trạng thái mode, khôi phục lựa chọn và cung cấp thao tác đổi mode cho MainLayout qua cascading context. Các provider MudTheme/Popover/Dialog/Snackbar tồn tại một bộ cho subtree, bỏ các bộ lặp riêng ở MainLayout/LoginLayout.
- Layout đăng nhập và layout đã đăng nhập cùng nhận palette/state này. Không chuyển authentication hoặc dịch vụ Application vào component theme.
- CSS cho nền/chữ/icon tương ứng tham chiếu các biến palette MudBlazor. Các token riêng như surface-variant, trạng thái mềm và chart lấy từ cùng palette hoặc biến thể có cặp foreground/background rõ ràng. Không duy trì một bộ primary/surface/text khác độc lập trong CSS.
- `data-theme` được cập nhật cùng trạng thái hiệu lực cho các chi tiết còn cần selector, nhưng không được là nguồn riêng quyết định màu nền cơ bản đối nghịch palette.

## Khởi tạo, lưu và đổi theme

1. Provider có trạng thái khởi tạo. Trong lúc chưa đọc được lựa chọn, hiển thị placeholder trung tính thay cho nội dung tương tác; không hiển thị bảng/form trong trạng thái pha trộn palette.
2. Sau render có JS, đọc và chuẩn hóa lựa chọn cũ. Đồng bộ palette, data-theme và context, sau đó hiển thị nội dung. Dữ liệu trình duyệt không hợp lệ dùng Light.
3. Không đọc localStorage ở giai đoạn prerender. Nếu JS/storage không khả dụng, có fallback Light và hiển thị nội dung; lỗi khởi tạo không để màn hình chờ vô hạn. Timeout bảo vệ đề xuất 3 giây; có thể tinh chỉnh trong triển khai nếu test môi trường yêu cầu.
4. Khi đổi mode, cập nhật một trạng thái hiệu lực, render lại palette/context, cập nhật thuộc tính và lưu lựa chọn. Lỗi lưu storage không làm văng Blazor circuit; mode vẫn sử dụng được trong phiên.
5. Sau khi palette/CSS đã áp dụng, biểu đồ được refresh từ màu đã có hiệu lực. Không refresh chart trước khi DOM nhận theme mới. Giữ số liệu và thứ tự series hiện có.
6. Không yêu cầu đồng bộ nhiều tab theo thời gian thực; mỗi tab đọc lựa chọn khi mở/tải lại. Không thêm cookie hoặc endpoint chỉ để lưu theme.

## Màu và control

Nguyên tắc nghiệm thu cho control hoạt động: chữ thông thường tối thiểu 4,5:1; chữ lớn theo ngưỡng WCAG tương ứng; icon/dấu hiệu cần thiết tối thiểu 3:1 với nền liền kề. Không tuyên bố cả ứng dụng đạt WCAG đầy đủ chỉ từ việc sửa màu.

- Nút Filled: khai báo cả nền và contrast text; không mặc định chữ trắng cho mọi nền sáng. Giữ sắc xanh chủ đạo, cho phép thay foreground thay vì đổi màu thương hiệu khi phù hợp.
- Nút Outlined/Text và icon button: đo màu nhãn/icon trên nền thực tế, kể cả hover/focus; phân biệt rõ thao tác chính/phụ/nguy hiểm theo ý nghĩa đang có.
- Disabled: giữ điều kiện vô hiệu hóa hiện có, có nhận diện rõ; không biến disabled thành enabled để làm nó nổi hơn. Không dùng ngoại lệ WCAG của disabled để bỏ qua yêu cầu dễ hiểu.
- Loading: vẫn đọc được nhãn nếu có, spinner đủ rõ và không thay đổi điều kiện chống thao tác lặp.
- Bảng/header/row/hover/selected: nền và chữ dùng cặp tương thích cùng theme. Dòng Bỏ qua trong Import có nhấn trạng thái mềm nhưng vẫn đọc được tên cột và dropdown; không tự đổi mapping.
- Select, autocomplete, text field, placeholder, label, chip, alert, menu, tooltip, dialog, snackbar: áp dụng cùng quy tắc; popover ngoài cây DOM trực tiếp của trang vẫn nhận theme.
- Custom HTML button ở dashboard, spotlight, login: nhận các token foreground/background/on-primary nhất quán với MudButton. Tương tác bàn phím có focus nhìn rõ.
- Biểu đồ: màu chữ/grid/border/surface đọc từ CSS theme hiệu lực thay cho bộ mã cố định riêng. Rà lại màu series cần đọc được, không đổi ý nghĩa dữ liệu và không chỉ dùng màu để thay nhãn đã có.

Chọn chính xác màu phụ/hover và tên token là quyết định của người triển khai, với điều kiện vượt ngưỡng đo và giữ cách nhận diện/trạng thái hiện có. Không cần người dùng chọn từng mã hex.

## Phạm vi file và ranh giới

Dự kiến thay đổi giới hạn ở Web:

- `Components/Routes.razor`, `Components/Shared/MainLayout.razor`, `Components/Shared/LoginLayout.razor`.
- Component/context/config theme dùng chung mới trong `Components/Theming/`.
- `wwwroot/app.css` và `wwwroot/irm-interop.js`; cập nhật cache version/tên asset khi cần để không dùng JS/CSS cũ sau publish.
- Các component feature có màu inline hoặc class gây xung đột được tìm thấy trong rà soát. Thay đổi thuộc tính màu/class, không đổi handler, validation hay dữ liệu.
- Test phù hợp và tài liệu kiểm chứng.

Giữ `app.css` là điểm vào hiện có. Không tách toàn bộ CSS hoặc refactor các trang lớn chỉ để sắp xếp lại. Không thay project references, phân tầng Core/Application/Infrastructure hoặc cấu trúc workspace.

## Kiểm chứng

1. Kiểm thử trạng thái light/dark, dữ liệu lưu hợp lệ/không hợp lệ, lỗi storage, toggle và cập nhật data-theme. Kiểm tra palette/context trước hiển thị nội dung và thời điểm refresh chart.
2. Browser verification chạy bản source xác định commit với dữ liệu thử cô lập. Không tự bootstrap/migrate/ghi vào database khách hoặc database nghiệp vụ cũ. Nếu thiếu môi trường thử an toàn thì ghi rõ ca nào chưa chạy, không thay bằng ảnh HTML mô phỏng rồi coi là đã nghiệm thu app.
3. Import: upload, ghép cột, xem trước và kết quả; ưu tiên tái hiện đúng ảnh khách. Bảng có cả mapping thường và Bỏ qua; dropdown đóng/mở; nút tiếp tục disabled/enabled/loading.
4. Rà soát light/dark: dashboard, công ty, lao động, hồ sơ NNN, thăm thân, du học sinh, lưu trú, kiểm tra, báo cáo/tìm kiếm/thống kê, quản trị và login. Kiểm tra menu/dialog/popover và custom button.
5. F5 sau lưu mỗi mode, mở tab mới, điều hướng không reload, đăng xuất/đăng nhập; storage lỗi fallback; desktop 1366×768 và 1920×1080, kiểm tra hẹp 390 px để tránh phát sinh tràn do sửa style. Không mở rộng đợt này thành thiết kế lại responsive.
6. Chụp cặp ảnh cùng màn hình/light-dark, ghi computed foreground/background của các thành phần lỗi và ngưỡng tương phản. Giữ bằng chứng bản chạy/revision, dữ liệu thử và những ca chưa nghiệm thu.
7. Chạy restore/build Release/test phù hợp từ `IRM/`, kiểm tra diff không có thay đổi nghiệp vụ; báo rõ Application Control hoặc test SQL bị chặn/bỏ qua.

Có thể thêm công cụ test browser vào môi trường kiểm thử nếu cần sau khi thiết kế được duyệt; không đưa browser/test dependencies vào payload ứng dụng. Việc chọn test harness là quyết định triển khai, ưu tiên công cụ hiện có.

## Tiêu chí hoàn thành và bàn giao

- Các ô chữ, select và button tương ứng ảnh khách đọc được trong cả hai mode; không cần chuyển dark để tiếp tục làm việc.
- Palette, CSS, login và chart cùng mode sau khởi tạo, toggle, điều hướng và reload; không còn bảng light với chữ dark-theme.
- Bố cục và luồng thao tác giữ nguyên; quyền và điều kiện disabled không đổi; không thay dữ liệu/database.
- Mỗi phần sửa hoàn tất được kiểm tra, review staged diff và commit cục bộ. Không push.
- Tài liệu báo đúng checks đã chạy/chưa chạy; không coi sửa source là đã sửa máy khách. Đóng gói bộ cài mới hoặc triển khai là việc tiếp theo theo yêu cầu bàn giao; giữ nguyên bộ cài R4 đã có.

## Phê duyệt

Builder check độc lập đã đọc thiết kế và kết luận `nothing important left`: không còn câu hỏi quan trọng cản trở lập kế hoạch. Đây là rà soát tính đầy đủ của thiết kế, không phải nghiệm thu UI hoặc phê duyệt thay người dùng.

Đã nhận phê duyệt ngày 10/11/2026. Kế hoạch triển khai: [2026-11-10-irm-theme-consistency.md](../plans/2026-11-10-irm-theme-consistency.md). Giữ yêu cầu kiểm chứng và commit cục bộ trước khi kết thúc.
