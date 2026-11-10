# Thiết kế sắp xếp workspace IRM

Ngày: 10/11/2026. Trạng thái: người dùng đã duyệt phương án một Git repo chung và yêu cầu thực hiện kế hoạch.

## Mục tiêu và phạm vi

Tách rõ dự án WPF cũ, ứng dụng nâng cấp và sản phẩm bàn giao. Bản nâng cấp kế thừa database và logic nghiệp vụ cũ, tổ chức phần phát triển mới theo tính năng. Đợt này chỉ sắp xếp thư mục, đường dẫn build và tài liệu; không bổ sung chức năng nghiệp vụ.

## Phương án đã cân nhắc

- **Một repo Git tại `NNN/`: người dùng chọn phương án 1.** Giữ lịch sử và branch hiện tại, theo dõi source cũ/nâng cấp trong cùng repo; loại các gói bàn giao và dữ liệu runtime khỏi Git.
- Hai repo riêng: tách quản lý source nhưng phải tách lịch sử và phối hợp hai luồng commit. Không chọn.

Không khởi tạo lại Git, không viết lại commit cũ và không tạo bản sao source thay cho việc di chuyển.

## Cấu trúc đích

```text
NNN/
├── .git/                              # Giữ Git hiện có và branch develop
├── .github/                           # Workflow chạy từ Git root
├── .gitignore
├── AGENTS.md
├── README.md                          # Chỉ dẫn mở đúng dự án
├── immigration-reportmanager-master/  # WPF cũ, giữ nội dung nguyên trạng
│   ├── ReportManager.sln
│   └── ReportManager/
├── IRM/                               # Thư mục dự án nâng cấp
│   ├── IRM.sln
│   ├── IRM/                           # Web host: Features/, Components/, Hosting/
│   ├── IRM.Core/                      # Entity, enum và quy tắc tương thích
│   ├── IRM.Application/               # Nghiệp vụ, DTO, interface theo tính năng
│   ├── IRM.Infrastructure/            # SQL/EF, import, file, đồng bộ và bảo trì
│   ├── IRM.Tests/
│   ├── deploy/
│   ├── docs/
│   ├── reference/                     # TestExcelGen và mockup-demo hiện có
│   ├── .config/
│   ├── Dockerfile
│   └── Các cấu hình và tệp hỗ trợ của bản nâng cấp
└── BAN-GIAO/                          # Giữ bố cục và file đã tập trung
```

Ba thư mục công việc ở cấp gốc là `immigration-reportmanager-master`, `IRM`, `BAN-GIAO`. Các tệp Git/hướng dẫn và thư mục ẩn quản lý repo vẫn nằm ở cấp gốc. Tên `BAN-GIAO` được giữ để ổn định đường dẫn bộ cài.

Giữ tên project Web bên trong là `IRM` để bảo toàn đường dẫn tương đối `IRM/IRM.csproj` từ thư mục solution, assembly `IRM` và executable `IRM.exe`. Đây là lựa chọn triển khai đề xuất, không đổi tên ứng dụng thành `IRM.Web` trong đợt sắp xếp.

## Kế thừa database và nghiệp vụ

- Giữ nguyên 146 tệp WPF đang được Git quản lý và các tài nguyên cũ đi kèm; kiểm tra fingerprint trước/sau khi di chuyển.
- Giữ nguyên database SQL Server, bảng/cột, enum, quy tắc xóa mềm, xác thực tương thích và transaction import/rollback.
- Giữ nguyên migration, snapshot và compatibility baseline; không tạo migration do đổi thư mục, không chạy cập nhật schema lên database khách hàng.
- Bản nâng cấp tiếp tục dùng các module tính năng hiện có: doanh nghiệp, lao động, du học sinh, người nước ngoài/thăm thân, cư trú, lưu trú, kiểm tra, tài liệu, import/export, báo cáo và quản trị.
- Phần kế thừa vẫn được kiểm tra bằng các test tương thích hiện có; không mở rộng hoặc viết lại thuật toán nghiệp vụ trong đợt này.

## Các đường dẫn phải cập nhật

Workflow ở `.github/` dùng `IRM/` làm thư mục làm việc cho restore/build/test và đóng gói; sửa các đường dẫn upload artifact tương ứng. Docker build dùng context `IRM/`. Workflow demo trỏ tới `IRM/reference/mockup-demo/`.

Mở solution nâng cấp tại `NNN/IRM/IRM.sln`; lệnh .NET chạy từ `NNN/IRM/`. Git chạy được từ repo root hoặc các thư mục con. Cập nhật tài liệu và AGENTS theo hai phạm vi này, tránh nhầm Git root với solution root. Các kiểm tra tìm `IRM.sln` theo thư mục cha vẫn tìm được solution tại `IRM/`.

Script đóng gói vẫn tìm source từ vị trí tương đối trong `IRM/deploy/`, yêu cầu Git tree sạch và ghi commit nguồn. Gói mới tiếp tục đi vào `NNN/BAN-GIAO/04-CHO-NGHIEM-THU/`. Tệp checksum và nội dung bộ cài R4 đã giao được giữ nguyên. Các đường dẫn lịch sử trong danh mục di chuyển được giữ làm bằng chứng; cập nhật các liên kết hướng dẫn đang sử dụng.

Tài liệu hỗ trợ thuộc bản nâng cấp nằm dưới `IRM/docs/`; thư mục runtime/cache hiện có được giữ và bỏ qua trong Git. Các đầu ra hỗ trợ ngoài ba thư mục chính được gom vào `BAN-GIAO/05-HO-TRO/` khi có thể di chuyển an toàn. Nếu workbook vẫn bị Excel khóa, giữ dữ liệu và báo rõ mục còn chờ; không tự đóng Excel hoặc bỏ thay đổi đang mở.

## Kiểm chứng và hoàn tất

Trước khi di chuyển, lưu branch, HEAD, danh sách file và fingerprint cần bảo toàn; xác nhận mọi đường dẫn nguồn/đích nằm trong workspace, không ghi đè đích đã tồn tại. Không xóa dữ liệu để giải quyết trùng tên.

Sau khi di chuyển: kiểm tra lịch sử Git và source cũ; kiểm tra project references, restore/build Release, bộ test .NET, parser/kiểm tra môi trường PowerShell, cấu hình workflow/Docker và checksum bàn giao. Test SQL Server chỉ chạy trên môi trường test riêng khi được cấu hình. Nếu Windows Application Control tiếp tục chặn test assembly hoặc công cụ Docker không khả dụng, ghi rõ giới hạn; không thay chính sách bảo mật để vượt kiểm tra.

Commit đầy đủ việc di chuyển và cập nhật đường dẫn sau khi kiểm tra, báo mã commit và trạng thái Git. Không push, cài bộ cài, restart dịch vụ hoặc thao tác database production. Tài liệu thiết kế này sẽ chuyển theo vào `IRM/docs/superpowers/specs/`.
