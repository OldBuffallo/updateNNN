# Giai đoạn 6 — Test log IRM v0.1.0

**Ngày chạy:** 16/08/2026

## Automated

| Suite | Kết quả |
|---|---|
| Web full build | PASS, 0 warnings / 0 errors |
| xUnit | PASS — 17/17 |
| SQLite startup/login | PASS |
| SQL Server migration first/second run | PASS/PASS |
| SQL Server backfill first/second run | PASS/PASS |
| Seed 54 địa bàn | PASS |
| 5 legacy trigger | PASS |
| WPF explicit-column insert SQL smoke | PASS, rolled back |

Phạm vi xUnit: passport normalization, duplicate/overlap, distinct/as-of, inspection lookback, temporal filter, password hash upgrade/lockout, RBAC/account claim, export formula injection, import mở rộng nhiều giấy tờ/lịch sử và rollback sạch, seed idempotency, MIME/signature và simulated malware failure cleanup.

## Blocked/not run

| Hạng mục | Trạng thái | Lý do |
|---|---|---|
| WPF binary build | BLOCKED | Thiếu .NET Framework 4.5.2 Developer Pack (`MSB3644`) |
| WPF manual UI smoke | NOT RUN | Phụ thuộc binary build và dữ liệu restore |
| Web nghiệp vụ UAT | NOT RUN | Cần tester nghiệp vụ và dữ liệu đại diện của khách hàng |
| Defender real-file UAT | NOT RUN | Chỉ test failure bằng scanner giả lập |
| Customer backup/restore | NOT RUN | Chưa có phê duyệt/connection/evidence |
| Customer production deploy | NOT RUN | Production gate chưa đạt |

## Demo/staging VPS — Openship

| Kiểm tra | Kết quả |
|---|---|
| Deployment `dep_xrAPbwO0_JlJhLog` | PASS — active/ready |
| HTTPS login + nhãn v0.1.0 | PASS |
| Login cookie Secure/HttpOnly | PASS |
| Authenticated home/family/accommodation/inspection/statistics/company/admin | PASS — 200 |
| Restart và giữ session/Data Protection key/SQLite | PASS |
| Port app chỉ bind `127.0.0.1:5050` | PASS |
| Rotate 4 demo credentials; old admin password rejected/new accepted | PASS |
| SQLite post-rotation backup → restore → integrity/row-count | PASS — 43 bảng, 284 dòng |
| GH-600 regression health | PASS — 200 |

## Quality gate

Full build `--no-incremental` đạt 0 warning/0 error. Reviewer verdict là `APPROVED FOR DEMO/STAGING ONLY`; production gate vẫn phụ thuộc WPF build, UAT và bản restore database khách hàng như bảng trên.
