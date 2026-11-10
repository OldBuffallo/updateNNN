# Kiểm tra sắp xếp workspace ngày 10/11/2026

## Kết quả cấu trúc

Một Git repo hiện đặt tại `NNN/`, tiếp tục nhánh `develop`. Source WPF cũ ở `immigration-reportmanager-master/`; solution nâng cấp ở `IRM/IRM.sln`; các gói và dữ liệu bàn giao ở `BAN-GIAO/`. Workflow nằm tại `.github/`, còn demo và Excel experiment nằm dưới `IRM/reference/`.

Mốc trước di chuyển: commit `75403fe214da64f7626d5dfeeba4a2c500f51c5c`. Đã lưu Git bundle và fingerprint trước di chuyển trong thư mục tạm của phiên làm việc. Sau di chuyển, cấu hình Git và lịch sử cũ vẫn được giữ nguyên.

## Bảo toàn nội dung

- Cả 702 tệp source được đối chiếu ngay sau di chuyển, trước khi sửa tài liệu/cấu hình: byte không đổi.
- Sau cập nhật đường dẫn, 688 tệp ngoài danh sách tài liệu/cấu hình được phép sửa tiếp tục khớp fingerprint, gồm toàn bộ 146 tệp WPF, mã nghiệp vụ, migration và compatibility baseline.
- 160 tệp bàn giao giữ nguyên fingerprint; không theo junction `node_modules` của công cụ hỗ trợ khi kiểm tra. Không sửa EXE, ZIP/RAR, dữ liệu test hoặc checksum gốc của R4.
- Không đổi dependency, schema, API, entity, enum, xác thực, transaction hoặc dịch vụ đã cài. Không chạy lệnh cập nhật database hoặc triển khai production.

## Kiểm tra đường dẫn và công cụ

| Nội dung | Kết quả |
|---|---|
| Git root, branch, cấu hình và lịch sử | Đạt |
| Project/solution references | Đạt |
| YAML workflow, working directory, đường dẫn lấy artifact và demo | Đạt |
| Docker COPY/context và loại mã tham chiếu khỏi context | Đạt kiểm tra tĩnh |
| Git ignore cho gói bàn giao, cache, database, khóa bảo mật, log và cấu hình bí mật | Đạt |
| README workspace, source nâng cấp và bàn giao | Liên kết hiện hành tồn tại |
| Restore và build Release tại vị trí mới | Đạt, 0 lỗi / 0 cảnh báo |
| PowerShell bộ cài | x64: 46 assertion đạt; x86: 47 assertion đạt |
| Đường dẫn đóng gói Windows | Đúng source `IRM/`, output `BAN-GIAO/04-CHO-NGHIEM-THU/` |
| Chặn đóng gói khi Git chưa sạch | Đạt; kiểm tra dừng trước publish/cài đặt |
| Test .NET trực tiếp trên Windows | Bị Application Control chặn `IRM.Tests.dll`, mã `0x800711C7` |
| EF CLI trên Windows | Bị cùng chính sách chặn `IRM.Infrastructure.dll`; không thao tác database |
| Docker Linux target test | Đạt: 117 test đạt, 1 test SQL Server bỏ qua do không cấu hình kết nối test |
| Docker runtime image | Build thành công với context thư mục `IRM/`; image cục bộ `irm:layout-check` |

## Các quyết định khi thực hiện

- Làm trực tiếp trong workspace theo yêu cầu, giữ nhánh `develop`; không tạo worktree hoặc branch khác.
- PowerShell `Move-Item` gặp thuộc tính ẩn của thư mục `.git` và dừng giữa quá trình. Đã kiểm tra phần đã chuyển, tiếp tục bằng thao tác đổi tên thư mục cùng volume với đường dẫn được xác minh, rồi đối chiếu toàn bộ 702 fingerprint và Git config. Các thư mục rỗng còn lại được giữ trong cache cục bộ để không phải xóa dữ liệu.
- File Excel cũ trong `outputs/irm-import-fixed-20261110/` vẫn bị Excel giữ mở theo kiểm tra chia sẻ file. Giữ nguyên bản đó và bản đã tập trung trong `BAN-GIAO`; `outputs/` được Git bỏ qua. Không đóng Excel hoặc xóa file đang mở. Đây là ngoại lệ tạm thời ngoài ba thư mục công việc chính, đã được kế hoạch cho phép báo riêng.
- Không thay chính sách bảo mật Windows để chạy test/EF. Dùng quy trình Docker Linux đã có của dự án để kiểm chứng riêng tính tương thích build/test trên nền tảng Linux.
