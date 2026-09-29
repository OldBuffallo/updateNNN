#!/usr/bin/env bash
set -euo pipefail
[[ $EUID -eq 0 ]] || { echo "Cần chạy bằng sudo." >&2; exit 1; }
systemctl restart mssql-server.service
systemctl restart irm.service
sleep 5
curl -fsS http://127.0.0.1:5050/health/ready >/dev/null
echo "IRM service và kết nối database hoạt động bình thường."
