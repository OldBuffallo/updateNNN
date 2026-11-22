# Kiến trúc IRM sau khi tách project

## Điểm bắt đầu

Git root là workspace `NNN/`. Mở `IRM/IRM.sln` từ workspace, hoặc `IRM.sln` khi đang ở thư mục dự án nâng cấp `IRM/`. Tất cả đường dẫn project và lệnh .NET/EF trong tài liệu này tính từ thư mục nâng cấp. Solution chỉ chứa bản web hiện hành và test; WPF nằm ngang cấp tại `../immigration-reportmanager-master/ReportManager.sln`, giữ làm source cũ để đối chiếu database và nghiệp vụ. Demo/Excel experiment nằm trong `reference/` và không tham gia build solution.

| Project | Trách nhiệm | Tham chiếu project |
|---|---|---|
| IRM.Core | Entity, enum, quy tắc chuẩn hóa hộ chiếu | Không có |
| IRM.Application | Interface nghiệp vụ, DTO, validation, quy tắc import/tài liệu và port dữ liệu/hạ tầng | Core |
| IRM.Infrastructure | EF Core, mapping SQL, truy vấn/ghi dữ liệu, giao dịch, Excel, file, antivirus, đồng bộ WPF, bảo trì | Application, Core |
| IRM | Blazor UI, authentication adapter, endpoint, ghép DI và khởi chạy bảo trì | Application, Infrastructure |
| IRM.Tests | Kiểm thử nghiệp vụ, ranh giới project, SQL schema, migration và dữ liệu cũ | Các project hiện hành |

Luồng: **Trang/Component → interface Application → Application service → store port → SQL store → database hiện có**. DTO tách khỏi entity và EF change tracker. Sau thao tác tạo/lưu, khóa và giá trị do hệ thống sinh được chép lại vào DTO đầu vào, kể cả các dòng preview import.

## Tìm mã theo chức năng

Web và Application chia theo Companies, Employees, Students, ForeignPersons, Residence, Accommodations, Inspections, Documents, ImportExport, Reports và Administration. Chức năng cư trú/tài liệu vẫn hiển thị trong màn hình hồ sơ hiện có; không thêm route chỉ để khớp thư mục.

- `IRM/Features/`: trang và component nghiệp vụ; route và namespace component cũ được giữ nguyên.
- `IRM/Features/ImportExport/Components/`: upload, mapping, preview, kết quả; trang giữ state dùng chung trong code-behind.
- `IRM/Features/Administration/Components/`: tài khoản, danh mục, nhật ký và lịch sử import; code-behind chia thành các partial theo mục.
- `IRM/Components/Shared/`: layout, navigation và thành phần UI dùng chung. FeatureSection chuyển sự kiện của component con về page coordinator để cập nhật toàn bộ bước đang hiển thị.
- `IRM.Application/Features/`: interface dịch vụ, service, DTO, mapping và quy tắc theo module. DTO có namespace `IRM.Application.Models`, phân biệt với entity.
- `IRM.Application/Abstractions/`: port dữ liệu, file, Excel, password hashing và kiểm tra database; không trả IQueryable/DbContext/DbSet.
- `IRM.Infrastructure/Persistence/`: DbContext, từng `IEntityTypeConfiguration<T>`, SQL store và migration lịch sử.
- `IRM.Infrastructure/Files/`, `Excel/`, `Security/`, `LegacySync/`, `Maintenance/`: thành phần phụ thuộc nền tảng.
- `IRM/Hosting/`: cấu hình Web, authentication adapter, endpoint và chuyển tiếp lệnh bảo trì.

Tên `Sql*Store` phân biệt phần truy vấn/ghi dữ liệu với service Application. Workflow phụ thuộc SQL giữ transaction, trạng thái tracking, định dạng backup và thứ tự audit. Validation thuần và quyền truy cập được kiểm tra tại Application; SQL store giữ kiểm tra phòng vệ tương thích. Import/rollback và cập nhật lịch sử hợp nhất vẫn nằm trong một giao dịch, không chia thành nhiều repository ghi độc lập. SqlImportStore được chia partial theo preview, execution, rollback, lịch sử mở rộng và template.

## Hợp đồng tương thích

Database chuẩn là schema hiện tại, gồm bảng WPF và bảng mở rộng. WPF tiếp tục chạy song song. Đợt tách project không tạo migration mới hoặc đổi bảng/cột, kiểu dữ liệu, khóa, index, trigger, enum, quy ước xóa mềm hay thuật toán password.

Entity giữ namespace `IRM.Data.Models`. Một số input, read model và interface dùng chung giữ namespace `IRM.Services`/`IRM.Data.Models` để hạn chế thay đổi không cần thiết; tên này không còn chỉ vị trí project. DTO và dịch vụ mới có namespace Application rõ ràng.

DbContext vẫn tên `IRM.Data.IrmDbContext`, nằm cùng migration trong `IRM.Infrastructure`. Nội dung migration và snapshot được giữ nguyên; EF dùng migration assembly Infrastructure. Phiên bản ứng dụng 1.0.2 và schema baseline 1.0.1 là hai khái niệm khác nhau: không đổi checksum/schema version để đồng bộ nhãn ứng dụng.

Khóa JSON trong audit, mapping template và backup/rollback import giữ nguyên. Legacy sync giữ chu kỳ 15 giây, retry/dead-letter và các loại sự kiện hiện có. Không bổ sung đồng bộ hai chiều hoặc thay đổi cách xử lý xung đột trong đợt này.

Khởi động thông thường chỉ kiểm tra schema. Các chế độ `--bootstrap`, `--provision-database`, `--self-test`, `--backup`, `--restore` giữ nguyên. Đường dẫn appsettings, dữ liệu, port, cookie, Windows Service, executable và tên artifact cài đặt giữ nguyên.

## Build, test và EF

```powershell
dotnet restore IRM.sln
dotnet build IRM.sln -c Release
dotnet test IRM.Tests/IRM.Tests.csproj -c Release -- RunConfiguration.TreatNoTestsAsError=true
dotnet tool restore
dotnet ef migrations list --project IRM.Infrastructure --startup-project IRM --no-connect
dotnet ef migrations has-pending-model-changes --project IRM.Infrastructure --startup-project IRM
```

Tool EF khóa ở 8.0.31, cùng phiên bản EF của ứng dụng. Không chạy `database update` trên database khách hàng để kiểm tra refactor.

Test SQL Server là tùy chọn có điều kiện; dùng tài khoản máy test có quyền tạo database:

```powershell
$env:IRM_TEST_SQLSERVER = 'Server=.\SQLEXPRESS;Integrated Security=True;TrustServerCertificate=True'
dotnet test IRM.Tests/IRM.Tests.csproj -c Release -- RunConfiguration.TreatNoTestsAsError=true
```

Test chỉ tạo/xóa database tên ngẫu nhiên `IRM_RefactorTest_*`; không gọi provision login, đổi mật khẩu SQL, cấu hình server hoặc sửa database IRM đang dùng. Test SQL mô phỏng client cũ ghi cột và đưa sự kiện vào queue; vẫn cần nghiệm thu hai client thực tế trên môi trường bàn giao.

`IRM.Tests/Baselines/` lưu SQL tạo schema trước refactor, migration ID, hash nội dung migration/snapshot và JSON rollback mẫu. Test yêu cầu SQL sinh bởi model không đổi, giữ đủ 43 entity và 2 migration. Không cập nhật baseline để che lỗi thay đổi mapping.

## Đóng gói và triển khai

Publish tiếp tục dùng `IRM/IRM.csproj`; thư viện mới đi vào publish qua ProjectReference. Bộ cài Windows/Linux không cần đổi tên executable hoặc đường dẫn ứng dụng. Dockerfile restore/copy đủ bốn project, không đưa WPF, runtime state hoặc bộ bàn giao vào build context.

```powershell
dotnet publish IRM/IRM.csproj -c Release -r win-x64 --self-contained true -o build-output/architecture/win-x64
dotnet publish IRM/IRM.csproj -c Release -r linux-x64 --self-contained true -o build-output/architecture/linux-x64
docker build -t irm:architecture-check .
docker build --target test -t irm:architecture-tests .
```

Docker target test chạy trên Linux, bỏ qua test SQL nếu không có `IRM_TEST_SQLSERVER`. CI và test container xem trường hợp không tìm thấy test là lỗi.

Refactor không triển khai production. Bộ cài trên máy sạch, WPF thực tế và backup/restore cần được nghiệm thu trong môi trường cô lập trước phát hành. Hạn chế có sẵn như Docker image chưa chứa ClamAV được giữ riêng, không sửa lẫn với thay kiến trúc.
