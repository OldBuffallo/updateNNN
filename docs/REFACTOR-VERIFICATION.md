# Kiểm tra tách project IRM

Ngày kiểm tra: 02/11/2026. Mốc đối chiếu là working tree trước refactor, bao gồm các thay đổi đang có của người dùng.

## Bằng chứng

- Solution gồm bốn project ứng dụng và IRM.Tests; không đưa WPF/thử nghiệm vào build.
- Build Release không lỗi/cảnh báo; 109/109 test đạt trên Windows với SQL Server thử nghiệm.
- 96 test hiện có giữ nguyên ý nghĩa. Test mới kiểm tra chiều phụ thuộc, DTO/tracking, khóa sinh sau ghi, phân quyền, vòng đời component con, lỗi validation, JSON/template cũ và tương thích SQL Server.
- SQL tạo schema khớp baseline trước refactor: 43 entity. Hai migration giữ ID; migration/snapshot giữ nguyên nội dung. EF xác nhận không có pending model changes.
- Test SQL tạo/xóa database IRM_RefactorTest_* riêng: đọc/ghi kiểu client cũ, đồng bộ sự kiện lặp, rollback cũ, khởi động và lặp bootstrap không đổi schema.
- Đăng nhập, health và render HTTP của 22 route thành công trên database UI thử nghiệm riêng.
- Chế độ --self-test hoàn tất trên database UI thử nghiệm: database, dữ liệu seed, login, báo cáo Excel, ghi/quét file bằng antivirus.
- Publish self-contained Windows x64 và Linux x64 thành công; có đủ bốn assembly và runtime, không kèm C# hoặc PDB.
- Docker runtime build thành công. Docker target test chạy trên Linux; test SQL được bỏ qua khi không cung cấp kết nối test.

## Giới hạn nghiệm thu

Host điều khiển trình duyệt không khả dụng: chưa thao tác tương tác toàn bộ màn hình bằng trình duyệt; đã kiểm tra render HTTP và chuyển sự kiện component bằng test.

Test tương thích WPF mô phỏng SQL/XML event theo hợp đồng hiện tại, chưa chạy client WPF thực tế. Chưa cài bộ cài vào máy Windows sạch hoặc thực hiện backup/restore end-to-end. Không gọi bảo trì vào database IRM đang dùng, không thay cấu hình SQL Server và không triển khai production.

Chỉ phát hành sau khi nghiệm thu bộ cài, hai client WPF/Web chạy chung và backup/restore trong môi trường cô lập.
