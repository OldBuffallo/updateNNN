# IRM — source cũ, dự án nâng cấp và bàn giao

Workspace này dùng **một Git repository tại `NNN/`**, giữ lịch sử và nhánh `develop` hiện có.

| Thư mục | Mục đích | Điểm bắt đầu |
|---|---|---|
| `immigration-reportmanager-master/` | Dự án WPF cũ, nguồn tham chiếu database và nghiệp vụ kế thừa | `ReportManager.sln` |
| `IRM/` | Ứng dụng web nâng cấp, chia theo tính năng; source, test, deploy và tài liệu | [IRM/README.md](IRM/README.md), `IRM/IRM.sln` |
| `BAN-GIAO/` | Bộ cài, tài liệu và dữ liệu test; không đưa vào Git | [BAN-GIAO/README.md](BAN-GIAO/README.md) |

## Làm việc với bản nâng cấp

Từ thư mục này, chuyển vào solution root rồi chạy:

```powershell
Set-Location IRM
dotnet restore IRM.sln
dotnet build IRM.sln -c Release
dotnet test IRM.Tests/IRM.Tests.csproj -c Release -- RunConfiguration.TreatNoTestsAsError=true
```

Git dùng chung cho source cũ và mới, chạy được từ workspace root hoặc thư mục con. Mọi thay đổi source hoàn tất phải được kiểm tra và commit đầy đủ; chỉ push khi được yêu cầu.

Database SQL Server, các hợp đồng tương thích và nghiệp vụ kế thừa được giữ nguyên. Bản web sử dụng `IRM.Core`, `IRM.Application`, `IRM.Infrastructure` và Web host `IRM`; mã tính năng nằm trong các thư mục `Features`. Không tạo database hoặc chạy migration production chỉ để kiểm tra việc sắp xếp thư mục.

## Build và bàn giao

Docker dùng context thư mục nâng cấp: `docker build -t irm:check ./IRM`. Workflow GitHub vẫn nằm ở `.github/`, các bước build/test chạy từ `IRM/`. Demo và Excel experiment nằm trong `IRM/reference/`.

Chạy script đóng gói Windows từ `IRM/`; source phải được commit và Git tree sạch. Gói mới có commit nguồn trong `SOURCE-REVISION.json` và được lưu riêng trong `BAN-GIAO/04-CHO-NGHIEM-THU/`.

Bộ cài R4 đã giao được giữ nguyên, không chứa các thay đổi source được commit sau đó. Dùng workbook đã sửa trong `BAN-GIAO/03-DU-LIEU-TEST/2026-11-10/` khi thử import với danh mục mặc định.

Chi tiết: [kiến trúc](IRM/docs/ARCHITECTURE.md), [bàn giao](IRM/docs/DELIVERY-LAYOUT.md), [thiết kế sắp xếp workspace](IRM/docs/superpowers/specs/2026-11-10-workspace-layout-design.md).
