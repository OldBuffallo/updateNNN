# Báo cáo hotfix và triển khai IRM v0.1.1

**Ngày thực hiện:** 17/08/2026

**Môi trường:** demo/staging VPS `180.93.103.206`

**URL:** <https://irm.180.93.103.206.nip.io/>

**Verdict:** **PASS cho demo/staging; chưa được phê duyệt production khách hàng**

## 1. Sự cố và nguyên nhân gốc

Triệu chứng người dùng ghi nhận là trang có thể hiện khung giao diện nhưng không có dữ liệu và không tương tác được. Log container xác nhận Blazor Server circuit bị kết thúc bởi lỗi JS interop:

```text
Could not find 'irmInterop.registerShortcutHandler'
CircuitHost: Unhandled exception in circuit
```

File JavaScript đang phục vụ trên VPS có hàm này, trong khi trình duyệt bị lỗi đang dùng tài nguyên tĩnh đã cache từ deployment cũ. URL script không có version nên trình duyệt có thể giữ bản cũ sau khi image được thay. `MainLayout.OnAfterRenderAsync` đồng thời coi phím tắt toàn cục là bắt buộc; lỗi của tiện ích phụ vì vậy làm sập toàn bộ circuit, kéo theo dữ liệu và sự kiện UI không hoạt động.

## 2. Thay đổi trong v0.1.1

- Cache-bust tài nguyên interop bằng `irm-interop.js?v=0.1.1`.
- Cô lập lỗi đăng ký phím tắt và khôi phục theme bằng `try/catch JSException`; lỗi tiện ích không còn được phép kết thúc circuit.
- Đồng bộ version assembly, file, UI, page title, CSS/JS header và cấu hình image thành `0.1.1`.
- Đổi Data Protection application discriminator thành tên ổn định `IRM`, không gắn với version hiển thị.
- Đổi tên project Openship từ `IRM v0.1.0` thành `IRM Demo` để không gây nhầm lẫn khi ứng dụng được nâng patch version.
- Không có thay đổi schema. `SchemaVersions.RequiredVersion`, baseline và tên file SQLite vẫn là `0.1.0` theo chủ đích.

## 3. Provenance và artifact

| Mục | Giá trị |
|---|---|
| Release tag | `v0.1.1` |
| Source commit | `7f0bb51f5e8d3ef50060369930670c83de2e5d89` |
| Candidate image | `irm:0.1.1` |
| OCI image ID | `sha256:4a873bbf149e6faa93db2da29bbacffc432a0d0a3b88090426929357ff416f10` |
| Image archive | `/opt/irm/releases/irm-0.1.1-7f0bb51.tar` |
| Archive SHA-256 | `8f83d092af286705220b118abc248139284cb334a6640c444f3ba60a8e776262` |
| Openship project | `IRM Demo` / `proj_ktNeuK8V-TgRA58h` |
| Deployment | `dep_K7mcqFSJCIRJaFsE`, version 5, `ready` |
| Runtime volume | `openship-irm-v010-irm-v010-data:/app/data` |

Openship tạo wrapper image cho service. Image ID của container vì vậy khác candidate, nhưng danh sách RootFS layer đã được đối chiếu và khớp hoàn toàn với `irm:0.1.1`.

## 4. Backup, restore rehearsal và rollback

Backup trước cutover nằm tại:

```text
/opt/irm/backups/20260817T094805Z-hotfix-v011/
```

| Artifact/kiểm tra | Kết quả |
|---|---|
| SQLite backup | `IRM-v0.1.0-demo.db` |
| SQLite SHA-256 | `60fbd3892524d363f98971a28ea807db026ca1c72d9be22557781ba2ae26e735` |
| Runtime archive SHA-256 | `7b69a184517a3224cdf16035aa2b34d69e60f151aa324f7af9e2f7c970ec6e19` |
| Backup integrity | `ok` |
| Backup table/row count | 43 bảng / 294 dòng |
| Restore rehearsal | `ok`, 43 bảng / 294 dòng, cùng SHA-256 |
| Restore evidence | `/opt/irm/restore-tests/20260817T094805Z-hotfix-v011/IRM-restored.db` |

Rollback ưu tiên redeploy image v0.1.0 trước đó và giữ nguyên volume. Chỉ restore đè SQLite khi được phê duyệt riêng vì thao tác đó có thể loại bỏ dữ liệu phát sinh sau thời điểm backup.

## 5. Kết quả kiểm thử

| Kiểm tra | Kết quả |
|---|---:|
| `dotnet build IRM/IRM.csproj -c Release` | PASS — 0 warning, 0 error |
| `dotnet test IRM.Tests/IRM.Tests.csproj -c Release` | PASS — 17/17 |
| `node --check IRM/wwwroot/irm-interop.js` | PASS |
| Local candidate container `/login` | PASS — 200, hiển thị v0.1.1 |
| Chrome sạch nạp live app | PASS — script URL có `?v=0.1.1` |
| Public login bằng credential VPS hiện hành | PASS — cookie auth được cấp, redirect `/` |
| Authenticated `/`, `/employees`, `/companies`, `/foreign-persons`, `/students`, `/statistics` | PASS — tất cả 200, không trả màn hình login |
| Dữ liệu sau cutover | 42 hồ sơ hợp nhất, 30 lao động, 10 doanh nghiệp, 12 du học sinh |
| SQLite runtime | PASS — `integrity=ok` |
| Container | PASS — running, 0 restart, đúng persistent volume |
| Log sau browser smoke | PASS — không còn `CircuitHost`, `registerShortcutHandler`, unhandled exception hoặc `fail:` |

HTML render sau đăng nhập có 12 dòng bảng lao động, 12 dòng bảng doanh nghiệp, 44 dòng bảng hồ sơ hợp nhất và 12 dòng bảng du học sinh. Đây là bằng chứng dữ liệu demo vẫn tồn tại; deployment không seed lại hoặc thay database.

## 6. Vì sao admin local và VPS khác nhau

Local và VPS là hai database độc lập. Database local rỗng được `DatabaseSeeder` tạo tài khoản demo mặc định. Sau deployment đầu, toàn bộ mật khẩu trên VPS đã được xoay và lưu trong SQLite persistent volume; web dùng hash trong `WebCredentials`, trong khi cột mật khẩu legacy vẫn được giữ để tương thích WPF.

Seeder chỉ tạo tài khoản khi bảng `Accounts` rỗng. Vì deployment v0.1.1 gắn lại volume hiện hành, nó không thay đổi hoặc reset admin VPS. Sự khác nhau này là chính sách tách môi trường, không phải version drift. Report không ghi mật khẩu thật.

## 7. Kết luận

Lỗi “không có dữ liệu, không tương tác được” đã được xử lý ở hotfix v0.1.1 và deployment mới đã qua hậu kiểm. Môi trường hiện phù hợp để tiếp tục demo/UAT có kiểm soát.

Các gate production từ báo cáo v0.1.0 vẫn giữ nguyên: WPF binary/manual smoke, UAT nghiệp vụ, scanner tương thích Linux, domain/TLS chính thức, giám sát và backup/restore trên bản sao database khách hàng. Không có thao tác nào trong hotfix này chạm database production của khách hàng.
