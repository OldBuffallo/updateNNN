# Kiểm chứng import Excel — 13/11/2026

## Phạm vi và nguồn

Thực hiện theo `excel-import-spec-and-plan.md` tại `C:/Users/Windows/.gemini/antigravity/brain/925afd89-a5ed-45ea-b1df-9c7e4799c8fd/`. Phần A là đặc tả ràng buộc; file `db-specification.md` cùng thư mục cung cấp ý nghĩa danh mục và công thức số liệu công ty. Hướng dẫn người dùng: [IMPORT-EXCEL-SPEC.md](IMPORT-EXCEL-SPEC.md).

Giữ nguyên Git root và nhánh `develop`. Không sửa source WPF, không chạy migration hay ghi vào database nghiệp vụ, không tạo hoặc ghi đè bộ cài.

| Task | Kết quả source |
|---|---|
| 1 — Ngày, giới tính, GPLĐ | Khôi phục từ `1427aa1`; chạy lại 34 test |
| 2 — Kết quả giải quyết, địa chỉ, nghề | Khôi phục từ `be42d7e`; chạy lại 43 test |
| 3 — Sheet, vùng dòng, ô ngày | Khôi phục từ `09ef910`; chạy lại 11 test |
| 4 — Parse/preview theo WPF | `b8086e5`; thêm kiểm tra bắt buộc, nhóm nghề, nhà đầu tư và lựa chọn giao diện |
| 5 — Phiên bản, số liệu, rollback, automap | `0ee8fa3`; kiểm chứng trên SQLite và SQL Server |
| 6 — Hướng dẫn, workbook vàng | `d58f52d`; thêm golden tests, đối chiếu file cũ, thử nhập file xuất thực tế |
| Sửa kết quả review cuối | Cùng commit chứa báo cáo này; xem các test hồi quy bên dưới |

Task 1–3 đã có trước phiên thực thi này. Đã chạy lại test và kiểm tra commit; không có log RED lịch sử để chứng nhận chuỗi TDD ban đầu. Các phần triển khai/sửa mới đều có lần test thất bại trước khi sửa và lần đạt sau sửa.

## Kiểm tra cuối

| Kiểm tra | Kết quả |
|---|---|
| `dotnet build IRM.sln -c Release --nologo` | Đạt; 0 warning, 0 error |
| `dotnet test IRM.Tests/IRM.Tests.csproj -c Release --no-build --nologo` | 286 đạt, 0 lỗi, 0 bỏ qua |
| Node test: responsive-table, theme-chart, theme-preference | 31 đạt, 0 lỗi |
| File golden xuất thực tế | Chạy qua cả hai golden tests SQL Server/SQLite trong suite cuối |
| SQL cleanup | Không còn database `IRM_RefactorTest_*` trên instance test sau suite |
| Staged diff | Kiểm tra whitespace và rà soát nội dung trước commit |

SQL tests dùng Windows authentication tới instance cục bộ `SQLEXPRESS`. Test chỉ tạo/xóa database với tên tự sinh `IRM_RefactorTest_<guid>`. Các test SQL chạy với compatibility level 120 khi kiểm tra luồng import; test migration compatibility hiện có cũng được chạy. Đây không phải nghiệm thu trên SQL Server hoặc dữ liệu khách hàng.

Kiểm tra giao diện gồm build Razor và render HTML của bước preview có công tắc cho phép tạo công ty. Chưa thực hiện nghiệm thu thao tác đầy đủ trong trình duyệt trên database khách hàng. Các test JavaScript dùng bộ test hiện có; không coi chúng là thay thế nghiệm thu giao diện trực tiếp.

## Review độc lập và lượt sửa

Reviewer đầu (`gpt-6-astra`) dừng do giới hạn sử dụng trước khi có kết luận. Reviewer thay thế (`gpt-6.1-terra`) hoàn tất cùng phạm vi `a987f1a..d58f52d`, phát hiện 2 lỗi Critical và 5 lỗi Important, không có Minor. Sau khi phân loại, đã sửa tất cả trong một lượt TDD; không gọi review lại sau sửa, theo `executing-plans`.

| Phát hiện | Test tái hiện và kiểm chứng sau sửa |
|---|---|
| Rollback phiên cũ làm sai phiên mới/công ty phát sinh dữ liệu | `Rollback_RequiresReverseOrder_AndPreservesLaterVersions`, `SqlServer_Rollback_RequiresReverseOrder`, `Rollback_DoesNotDeleteACreatedCompanyUsedByLaterRecords` |
| Trạng thái EF còn sót sau transaction thất bại | `FailedTransaction_DoesNotLeakTrackerChanges_AndSameServiceCanRetry`, `FailedRollback_DoesNotDeleteTheVersionOnALaterSave` |
| Thay đổi cột mở rộng bị bỏ qua | `ExtensionOnlyChange_IsPreviewedSavedAndRolledBack`: VisaSymbol, người thân, ngày cấp thẻ, giấy tờ V010 |
| Cột Số GPLĐ riêng bị mất | `SeparatePermitNumber_IsConsumed`, `ConflictingPermitColumns_AreRejected` |
| Gender 2 bị đọc thành Nam từ DB cũ | `LegacyDatabaseImport_PreservesFemaleTwo` trên hai database SQL test riêng |
| Ngày thăm thân ISO bị mất | `FamilyExtensionDate_PreservesIsoAndDayMonthYear`, `InvalidFamilyExtensionDate_IsAnExplicitRowError` |
| Preview cũ ghi đè chỉnh sửa mới | `StalePreview_RejectsAnInPlaceEditToTheSameEmployee`, `SqlServer_StalePreviewAndRollback_ReadFreshStateFromAnotherContext` |

Bổ sung trong cùng lượt sửa: ngày bắt đầu tự động theo tiêu đề đã chọn; công tắc tạo công ty ngay ở preview; chặn rollback sau chỉnh sửa thủ công vào hồ sơ/giấy tờ; báo xung đột thời hạn giấy tờ đã có. Hai test retry giả lập mất xác nhận sau COMMIT thực tế để chứng minh một phiên chỉ được ghi một lần và rollback được xác nhận bằng trạng thái đã lưu. Retry được kích hoạt qua execution strategy và transaction interceptor thật của EF trong database test, không chỉ kiểm tra mock.

## Workbook bàn giao

`BAN-GIAO/03-DU-LIEU-TEST/2026-11-12/IRM-IMPORT-WPF-GOLDEN-20261112.xlsx`

- SHA-256: `D9A5C839E99AD48990684B475739C2C74CDCEF5E9E8E066A086C35F66E967576`.
- Dữ liệu giả: 12 dòng hợp lệ, 15 dòng lỗi với kết quả viết tay, thân nhân và nhà đầu tư; test thêm nhánh trùng/thay đổi và rollback.
- Đã render các vùng dữ liệu, hướng dẫn và kết quả; ngày xuất dạng số Excel với định dạng `dd/mm/yyyy` để tương thích ClosedXML.
- File có ngày tạo trong tên là 12/11; kiểm chứng cuối ngày 13/11. Không đổi nhãn ngày của file đã tạo.
- File mẫu cũ ngày 10/11 giữ nguyên SHA-256 `5DC0518B9FA3086253B4BC7372D849C5A987409B858368E477084567DFBEE3CA`.
- `SOURCE-REVISION.json` và `SHA256.txt` cạnh workbook ghi commit nguồn cuối và checksum sau khi commit. Bộ cài R4 hiện hành không chứa các commit mới này.

## Quyết định trong ledger

1. Làm tại checkout `develop` hiện có theo hướng dẫn workspace; rủi ro nếu có chỉnh sửa đồng thời phải đối chiếu lại.
2. Khôi phục Task 1–3 bằng commit và chạy lại test, không viết lại; không chứng nhận được RED lịch sử.
3. Quy tắc A3 loại ISO khỏi ngày lõi, ghi đè ví dụ Task 1 mâu thuẫn; ngày mở rộng nhận ISO riêng. Cần giữ phân biệt hai loại cột.
4. Không thay toàn bộ danh mục, đổi số quyền hay migrate DB thật trong kế hoạch import. Giữ Nữ 0 cũ, nhận thêm Nữ 2 và GPLĐ 6. Danh mục đích vẫn là tiền đề nghiệm thu.
5. Bổ sung điều khiển sheet/vùng dòng/nhóm nghề và opt-in tạo công ty để A1/A6 có luồng sử dụng; tăng phạm vi kiểm tra giao diện.
6. Test SQL chỉ dùng DB tự sinh qua Windows authentication; khác biệt môi trường khách hàng còn phải nghiệm thu.
7. Bổ sung cột bắt buộc và bản ghi Investment vào fixture cũ, giữ mục tiêu kiểm tra gốc; workbook thiếu trường nay bị từ chối đúng đặc tả.
8. Rollback xóa vật lý phiên bản thay thế mới, nhưng giữ cách ẩn đối với INSERT thông thường/lịch sử cũ; các dòng thêm mới vẫn tồn tại ở trạng thái ẩn.
9. Dòng cuối thiếu cả họ tên/hộ chiếu được bỏ dù có ghi chú, dòng trống giữa vùng vẫn báo lỗi; footer không có định danh cố ý không được nhập.
10. Thay reviewer bị giới hạn bằng reviewer khác để có kết luận độc lập; chi phí thêm một lần nạp ngữ cảnh.
11. Các mục reviewer không đánh giá (kế hoạch danh mục/quyền và bộ cài) giữ ngoài phạm vi; kiểm chứng source/workbook không xác nhận triển khai khách hàng.
12. Cột mở rộng được ánh xạ thay đổi phải đi vào nhánh thay đổi, cột không ánh xạ giữ nguyên; có thể phát sinh phiên bản chỉ vì thay đổi phần mở rộng.
13. Số GPLĐ riêng được dùng khi ô kết hợp trống; hai cột có nội dung mâu thuẫn bị báo lỗi. Mẫu cũ mâu thuẫn phải sửa trước khi nhập.
14. Thời hạn giấy tờ/diện cư trú V010 đã có nhưng khác dữ liệu nhập bị báo lỗi yêu cầu sửa hồ sơ; phải thêm bước chỉnh sửa thủ công thay vì ghi đè dữ liệu dùng chung.
15. Reload đúng bản ghi EF trước khi so sánh; xóa trạng thái tracker của giao dịch thất bại, reset kết quả mỗi retry và đọc mã phiên đã lưu khi mất xác nhận COMMIT. Sau giao dịch thất bại, bên gọi cần truy vấn lại dữ liệu.

Minor hoãn lại: không có.

## Giới hạn bàn giao

Kế hoạch DB riêng (nạp đúng ID/mã danh mục cũ, chiến lược chuẩn hóa dữ liệu/enum/quyền và migration) chưa được thực thi lên DB thật. Không suy ra rằng IRM mới khởi tạo đã có đầy đủ danh mục WPF. Không push, merge, triển khai hoặc tạo bộ cài trong lần thực hiện này.
