# Bố cục thư mục bàn giao (`BAN-GIAO/`)

Tất cả gói cài đặt, dữ liệu mẫu kiểm thử và tài liệu bàn giao được tập trung tại thư mục `BAN-GIAO/` ở gốc workspace (thư mục này được `.gitignore` loại trừ khỏi Git để không lưu trữ các tệp nhị phân dung lượng lớn).

## Cấu trúc chuẩn

- `BAN-GIAO/01-HIEN-HANH/`: Chỉ chứa gói bàn giao mới nhất đang hiệu lực.
  - Hiện hành: `v1.0.2-R6-UNSIGNED-TEST/`
    - `IRM-v1.0.2-windows-x64-offline-setup-R6.exe` (`790.854.254` byte, SHA-256: `2D6BA7B6E2FFEB8E6CE411C473965556F2FCEFADCEE5F77F3F2FDBD77CECF585`, commit nguồn: `706f74e44e3b97b1c50d6e3c116d4f969eb5389c`)
    - `IRM-v1.0.2-windows-x64-offline-setup-R6.exe.sha256`
    - `IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` (10 hồ sơ mẫu chuẩn hóa 3 chữ cái + Nhóm nghề `Kỹ thuật`, nạp đạt 10/10 trên DB cài mới R6)
    - `PHIEU-KHAO-SAT-GOP-Y.html` (Phiếu khảo sát & góp ý tương tác offline, xuất CSV/JSON, in PDF)
    - `PHIEU-KHAO-SAT-GOP-Y.xlsx` (Phiếu khảo sát & góp ý Excel 3 sheet có Data Validation & công thức tổng hợp)
    - `HUONG-DAN-CAI-DAT.html` & `HUONG-DAN-CAI-DAT.md`
    - `DOC-TRUOC-KHI-CAI.txt`, `SHA256SUMS.txt`, `SOURCE-REVISION.json`, `VERIFICATION.json`
- `BAN-GIAO/02-LUU-TRU/`: Lưu trữ nguyên vẹn các gói đã phát hành trước đó (không sửa đổi hoặc đổi nhãn):
  - `v1.0.2-R5-UNSIGNED-TEST/` (SHA-256 EXE: `C10A1915646381331DB3481BCA0D2F546CAF3B54440721830DC38A8FB9125D45`, commit `b42a277894b963d3a38ccc50ac924d0756f0a741`)
  - `v1.0.2-DARK-R4-UNSIGNED-TEST/`
  - `v1.0.2-R2/`, `v1.0.2-R3/`
- `BAN-GIAO/03-DU-LIEU-TEST/`: Chứa các bộ dữ liệu Excel mẫu theo mốc ngày:
  - `2026-11-15/IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` (chuẩn hóa cho R6)
  - `2026-11-10/IRM-MAU-IMPORT-TEST-DA-SUA.xlsx` (bản lưu cũ)
- `BAN-GIAO/04-CHO-NGHIEM-THU/`: Lưu biên bản build candidate (`20261115-v1.0.2-R6-UNSIGNED-TEST/`, `20261115-v1.0.2-R5-LANDING-UNSIGNED-TEST/`).
- `BAN-GIAO/05-HO-TRO/`: Lưu các công cụ và dữ liệu đối chiếu hỗ trợ trước đây.

## Quy tắc bảo toàn

1. Không bao giờ ghi đè hoặc gán lại nhãn commit mới cho bộ cài cũ đã bàn giao.
2. Khi phát hành bản mới, di chuyển nguyên vẹn gói cũ từ `01-HIEN-HANH/` sang `02-LUU-TRU/` trước khi đặt gói mới vào `01-HIEN-HANH/`.
3. Mọi tệp trong gói hiện hành phải có mã băm khớp với `SHA256SUMS.txt` và `IRM/docs/DELIVERY-INVENTORY.json`.
