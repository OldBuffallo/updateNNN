#!/usr/bin/env bash
set -euo pipefail

[[ "${EUID:-$(id -u)}" -eq 0 ]] || { echo "Cần quyền sudo/root." >&2; exit 1; }
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
LOG=/var/log/irm-install.log
exec > >(tee -a "$LOG") 2>&1

MIN_RAM_KB=2097152
MIN_DISK_KB=10485760
MIN_POST_DISK_KB=4194304
[[ "$(uname -m)" == "x86_64" ]] || { echo "Chỉ hỗ trợ amd64/x64." >&2; exit 1; }
[[ "$(. /etc/os-release; echo "$VERSION_ID")" == "22.04" ]] || { echo "Chỉ hỗ trợ Ubuntu 22.04 LTS." >&2; exit 1; }
[[ "$(nproc)" -ge 2 ]] || { echo "Cần tối thiểu 2 CPU core." >&2; exit 1; }
cpu_mhz="$(awk -F: '/cpu MHz/ {gsub(/ /,"",$2); print int($2); exit}' /proc/cpuinfo)"
[[ "${cpu_mhz:-0}" -ge 2000 ]] || { echo "CPU phải đạt tối thiểu 2 GHz." >&2; exit 1; }
available_kb="$(awk '/MemAvailable/ {print $2}' /proc/meminfo)"
[[ "$available_kb" -ge "$MIN_RAM_KB" ]] || { echo "Cần tối thiểu 2 GB RAM khả dụng." >&2; exit 1; }
disk_kb="$(df -Pk /opt | awk 'NR==2 {print $4}')"
[[ "$disk_kb" -ge "$MIN_DISK_KB" ]] || { echo "Cần tối thiểu 10 GB ổ đĩa trống." >&2; exit 1; }
filesystem="$(findmnt -no FSTYPE /opt)"
[[ "$filesystem" == "ext4" || "$filesystem" == "xfs" ]] || { echo "Phân vùng /opt phải dùng ext4 hoặc XFS." >&2; exit 1; }
for port in 14331 5050 5443; do
  ! ss -ltn "sport = :$port" | grep -q LISTEN || { echo "Cổng $port đang được sử dụng." >&2; exit 1; }
done

echo "IRM sử dụng Microsoft SQL Server 2022 Express CU27."
read -r -p "Nhập YES để chấp nhận điều khoản Microsoft SQL Server: " accept
[[ "$accept" == "YES" ]] || { echo "Chưa chấp nhận điều khoản SQL Server." >&2; exit 1; }
read -r -p "Tên tài khoản quản trị IRM [admin]: " admin_user
admin_user="${admin_user:-admin}"
read -r -s -p "Mật khẩu quản trị IRM (ít nhất 12 ký tự): " admin_password; echo
read -r -s -p "Mật khẩu irm_dba phục hồi database (ít nhất 16 ký tự): " dba_password; echo
[[ ${#admin_password} -ge 12 && ${#dba_password} -ge 16 ]] || { echo "Mật khẩu không đạt độ dài tối thiểu." >&2; exit 1; }
[[ "$dba_password" != *";"* && "$dba_password" != *"\""* ]] || { echo "Mật khẩu irm_dba không được chứa dấu chấm phẩy hoặc dấu nháy kép." >&2; exit 1; }
read -r -p "Cho phép máy khác truy cập qua HTTPS? [y/N]: " enable_lan
sa_password="$(openssl rand -base64 32)"
app_password="$(openssl rand -base64 32)"
certificate_password="$(openssl rand -base64 32)"
lan_url="https://127.0.0.1:5443"
[[ "$enable_lan" =~ ^[Yy]$ ]] && lan_url="https://0.0.0.0:5443"

source_list=/etc/apt/sources.list.d/irm-offline.list
printf 'deb [trusted=yes] file:%s/packages ./\n' "$(dirname "$SCRIPT_DIR")" > "$source_list"
apt-get -o Dir::Etc::sourcelist="$source_list" -o Dir::Etc::sourceparts='-' update
ACCEPT_EULA=Y apt-get -o Dir::Etc::sourcelist="$source_list" -o Dir::Etc::sourceparts='-' \
  --no-download --no-install-recommends install -y mssql-server=16.0.4295.3-1 mssql-tools18 msodbcsql18 clamav curl openssl
systemctl disable --now clamav-freshclam.service clamav-daemon.service 2>/dev/null || true

MSSQL_PID=Express MSSQL_SA_PASSWORD="$sa_password" ACCEPT_EULA=Y /opt/mssql/bin/mssql-conf -n setup
/opt/mssql/bin/mssql-conf set network.tcpport 14331
systemctl enable --now mssql-server
for _ in {1..60}; do timeout 1 bash -c '</dev/tcp/127.0.0.1/14331' 2>/dev/null && break; sleep 1; done
timeout 1 bash -c '</dev/tcp/127.0.0.1/14331' 2>/dev/null || { echo "SQL Server không khởi động." >&2; exit 1; }

id irm >/dev/null 2>&1 || useradd --system --home /var/lib/irm --shell /usr/sbin/nologin irm
install -d -o root -g irm -m 0750 /opt/irm /etc/irm
install -d -o root -g root -m 0755 /opt/irm/tools
install -d -o irm -g irm -m 0750 /var/lib/irm/keys /var/lib/irm/private-files
install -d -o mssql -g mssql -m 0770 /var/opt/mssql/backup
cp -a "$SCRIPT_DIR/app/." /opt/irm/
install -m 0750 "$SCRIPT_DIR/restore.sh" /opt/irm/tools/restore-irm
install -m 0750 "$SCRIPT_DIR/repair.sh" /opt/irm/tools/repair-irm
install -m 0644 "$SCRIPT_DIR/release-manifest.json" /opt/irm/release-manifest.json
install -m 0644 "$SCRIPT_DIR/SBOM-dotnet.json" /opt/irm/SBOM-dotnet.json
install -m 0644 "$SCRIPT_DIR/SBOM-deb.txt" /opt/irm/SBOM-deb.txt
install -m 0644 "$SCRIPT_DIR/IRM-v1.0.1-schema.sql" /opt/irm/IRM-v1.0.1-schema.sql
install -m 0644 "$(dirname "$SCRIPT_DIR")/SHA256SUMS.txt" /opt/irm/SHA256SUMS.txt
ln -sfn /opt/irm/tools/restore-irm /usr/local/sbin/restore-irm
ln -sfn /opt/irm/tools/repair-irm /usr/local/sbin/repair-irm
chmod 0755 /opt/irm/IRM

shopt -s nullglob
signature_files=("$SCRIPT_DIR"/clamav-signatures/*.cvd "$SCRIPT_DIR"/clamav-signatures/*.cld)
[[ ${#signature_files[@]} -gt 0 ]] || { echo "ISO thiếu chữ ký ClamAV ngoại tuyến." >&2; exit 1; }
cp -f "${signature_files[@]}" /var/lib/clamav/
chown clamav:clamav /var/lib/clamav/*.{cvd,cld} 2>/dev/null || true
shopt -u nullglob

openssl req -x509 -newkey rsa:3072 -sha256 -days 1095 -nodes -subj "/CN=$(hostname)" \
  -keyout /var/lib/irm/https.key -out /var/lib/irm/https.crt
openssl pkcs12 -export -out /var/lib/irm/irm-https.pfx -inkey /var/lib/irm/https.key \
  -in /var/lib/irm/https.crt -passout "pass:$certificate_password"
chown irm:irm /var/lib/irm/irm-https.pfx; chmod 0600 /var/lib/irm/irm-https.pfx

cat > /etc/irm/appsettings.Production.json <<JSON
{
  "ConnectionStrings": { "DefaultConnection": "Server=127.0.0.1,14331;Database=IRM;User Id=irm_app;Password=$app_password;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True" },
  "DataProtection": { "KeysPath": "/var/lib/irm/keys" },
  "FileStorage": { "Root": "/var/lib/irm/private-files" },
  "Backup": { "Directory": "/var/opt/mssql/backup" },
  "Security": { "RequireHttpsForRemoteClients": true },
  "Storage": { "StopUploadsBelowFreeBytes": 1073741824, "WarnBelowFreeBytes": 2147483648 },
  "AllowedHosts": "*",
  "Kestrel": { "Endpoints": {
    "Loopback": { "Url": "http://127.0.0.1:5050" },
    "LanHttps": { "Url": "$lan_url", "Certificate": { "Path": "/var/lib/irm/irm-https.pfx", "Password": "$certificate_password" } }
  } }
}
JSON
chown root:irm /etc/irm/appsettings.Production.json; chmod 0640 /etc/irm/appsettings.Production.json
ln -sfn /etc/irm/appsettings.Production.json /opt/irm/appsettings.Production.json

cd /opt/irm
ASPNETCORE_ENVIRONMENT=Production DOTNET_GCHeapHardLimit=0x20000000 \
ConnectionStrings__DefaultConnection="Server=127.0.0.1,14331;Database=IRM;User Id=irm_app;Password=$app_password;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True" \
Provisioning__AdminConnectionString="Server=127.0.0.1,14331;Database=master;User Id=sa;Password=$sa_password;Encrypt=True;TrustServerCertificate=True" \
Provisioning__AppPassword="$app_password" Provisioning__DbaPassword="$dba_password" \
Bootstrap__AdminUsername="$admin_user" Bootstrap__AdminPassword="$admin_password" \
./IRM --bootstrap --provision-database

ASPNETCORE_ENVIRONMENT=Production DOTNET_GCHeapHardLimit=0x20000000 \
ConnectionStrings__DefaultConnection="Server=127.0.0.1,14331;Database=IRM;User Id=irm_app;Password=$app_password;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True" \
SelfTest__AdminUsername="$admin_user" SelfTest__AdminPassword="$admin_password" \
./IRM --self-test

cp "$SCRIPT_DIR/irm.service" /etc/systemd/system/irm.service
cp "$SCRIPT_DIR/irm-backup.service" /etc/systemd/system/irm-backup.service
cp "$SCRIPT_DIR/irm-backup.timer" /etc/systemd/system/irm-backup.timer
cp "$SCRIPT_DIR/irm.desktop" /usr/share/applications/irm.desktop
systemctl daemon-reload
systemctl enable --now irm.service irm-backup.timer
if [[ "$enable_lan" =~ ^[Yy]$ ]] && command -v ufw >/dev/null; then ufw allow 5443/tcp; fi

for _ in {1..30}; do curl -fsS http://127.0.0.1:5050/health/ready >/dev/null && break; sleep 1; done
curl -fsS http://127.0.0.1:5050/health/ready >/dev/null || { echo "Health check IRM thất bại." >&2; exit 1; }
post_disk_kb="$(df -Pk /opt | awk 'NR==2 {print $4}')"
[[ "$post_disk_kb" -ge "$MIN_POST_DISK_KB" ]] || { echo "Sau cài đặt còn dưới 4 GB ổ đĩa." >&2; exit 1; }
rm -f "$source_list"
echo "Cài đặt IRM v1.0.1 hoàn tất. Mở http://localhost:5050"
