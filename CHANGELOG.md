# Changelog

Tất cả thay đổi đáng chú ý của dự án IRM được ghi lại tại đây.
Format theo [Keep a Changelog](https://keepachangelog.com/).
Versioning theo [Semantic Versioning](https://semver.org/).

## [Unreleased]

_(Tính năng đang phát triển cho version tiếp theo)_

---

## [0.1.0] - 2026-08-17

### Added — Yêu cầu khách hàng

- **YC1:** Quản lý người nước ngoài thăm thân — `FamilyVisitDetails`, `FamilyVisitors.razor`, phân loại thân nhân (công dân VN / gốc VN / VN định cư nước ngoài), liên kết sponsor công ty/người
- **YC2:** Danh mục cơ sở lưu trú & hợp đồng thuê — `AccommodationsV010`, `CompanyAccommodationAgreements`, `Accommodations.razor`; phạm vi thuê toàn bộ/căn/phòng; theo dõi CSLT → DN → NNN
- **YC3:** Kiểm tra hiện trường — `InspectionsV010`, `InspectionSubjects` với snapshot diện/giấy tờ/công việc/địa chỉ; lookback loại trừ cho lần kiểm tra sau
- **YC5:** Tra cứu & thống kê quá khứ — `TemporalReportFilter` (AsOfDate / From-To); effective dating trên tất cả entity v0.1.0
- **YC6:** Số định danh điện tử & thống kê giấy tờ — `ElectronicIdentities`, `ImmigrationDocuments` (Visa / Gia hạn / Thẻ / Miễn thị thực); thống kê đã/chưa cấp định danh
- **YC7:** Thống kê theo KCN/KKT/cụm CN — `EconomicZones`, `SiteZoneMemberships` (many-to-many temporal); `CompanyProfiles` phân loại FDI/trong nước
- **YC8:** Thống kê theo 54 địa bàn — `AdministrativeUnits` seed Quảng Ninh; territory overview + detail + export Excel
- **YC9:** Pháp nhân DN & đại diện — `CompanyRepresentatives`, `CompanyLegalDocuments`, `StoredFiles`; upload bảo mật (MIME/magic bytes/SHA-256/Defender)

### Added — Kiến trúc & bảo mật

- Hồ sơ NNN hợp nhất (`ForeignPersons`) — identity trung tâm, passport hash SHA-256
- RBAC 5 roles: Admin, DataEditor, Inspector, Reporter, Viewer
- Cookie auth HttpOnly + SameSite Strict + timeout 8h
- Password hash upgrade tự động khi đăng nhập web lần đầu
- Brute force protection: 5 lần sai → khóa 15 phút
- Formula injection protection cho Excel export
- Audit logging toàn hệ thống (CRUD/import/export/login)
- Legacy sync: XML triggers → `LegacyChangeEvents` outbox → idempotent worker
- WPF SQL compatibility: explicit column + parameter cho 6 insert trọng yếu

### Added — Hạ tầng

- 24 bảng v0.1.0 mới (tổng 43 bảng)
- 17 xUnit tests (normalization, overlap, distinct, as-of, inspection lookback, auth, import, MIME, malware cleanup)
- 12 SQL migration scripts (schema, triggers, backfill, smoke test)
- Preflight script (`preflight-v0.1.0.ps1`) — baseline/checksum/backup/verify
- Docker deployment (demo/staging trên VPS)
- Deploy demo/staging: HTTPS trên `irm.180.93.103.206.nip.io`

### Deferred

- **YC4:** NNN miễn thị thực cửa khẩu → Phase 2 (ADR-007). Foundation: mã diện `BORDER_VISA_EXEMPT`, blueprint 3 bảng (`BorderGates`, `BorderMovements`, `BorderStayEpisodes`)

### Security

- Cookie HttpOnly, SameSite Strict, HTTPS redirect
- Login lockout sau 5 lần sai (15 phút)
- Formula injection prefix cho Excel export
- SQL parameterized cho WPF inserts (6 ViewModels)
- File upload: whitelist MIME/extension, magic bytes, 10MB limit, GUID name, SHA-256, Windows Defender scan
- Service authorization guard cho tất cả CRUD/import/export

### Known Limitations

- WPF binary build bị BLOCKED (thiếu .NET Framework 4.5.2 Developer Pack)
- Manual browser UAT chưa chạy (cần người dùng nghiệp vụ)
- Production database chưa được test (cần backup/restore rehearsal từ khách hàng)
- Data Protection key trên volume chưa mã hóa bằng certificate/KMS
- Windows Defender không có trên Linux; upload trên demo fail-closed

---

## [0.0.1] - 2026-07-xx

### Added

- Phiên bản initial: WPF legacy migration lên Blazor Server
- Dashboard với Chart.js, KPI cards
- Import/Export Excel (ClosedXML)
- Quản lý Công ty, Lao động nước ngoài, Du học sinh
- Tìm kiếm toàn văn
- Login + đổi mật khẩu
- Admin CRUD (accounts, fields, careers, nationalities, districts, wards)
- Audit log viewer
- Responsive UI với MudBlazor + glassmorphism design
- Docker deployment cơ bản
- 13 bảng legacy + 6 bảng mở rộng
