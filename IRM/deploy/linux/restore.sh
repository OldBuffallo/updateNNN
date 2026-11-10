#!/usr/bin/env bash
set -euo pipefail

[[ $EUID -eq 0 ]] || { echo "Cần chạy bằng sudo." >&2; exit 1; }
archive="${1:-}"
[[ -f "$archive" && "$archive" == *.bak.gz ]] || { echo "Cách dùng: sudo restore-irm /đường/dẫn/IRM_*.bak.gz" >&2; exit 1; }
read -r -s -p "Mật khẩu irm_dba: " dba_password; echo
[[ "$dba_password" != *";"* && "$dba_password" != *"\""* ]] || { echo "Mật khẩu irm_dba không hợp lệ với chuỗi kết nối." >&2; exit 1; }
systemctl stop irm.service
trap 'systemctl start irm.service >/dev/null 2>&1 || true; unset dba_password' EXIT
cd /opt/irm
ASPNETCORE_ENVIRONMENT=Production \
Restore__ArchivePath="$(readlink -f "$archive")" \
Restore__AdminConnectionString="Server=127.0.0.1,14331;Database=master;User Id=irm_dba;Password=$dba_password;Encrypt=True;TrustServerCertificate=True" \
./IRM --restore
systemctl start irm.service
sleep 3
curl -fsS http://127.0.0.1:5050/health/ready >/dev/null
trap - EXIT
unset dba_password
echo "Phục hồi IRM hoàn tất."
