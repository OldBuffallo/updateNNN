#!/usr/bin/env bash
set -euo pipefail

if [[ "$(dpkg --print-architecture)" != "amd64" ]] || [[ "$(. /etc/os-release; echo "$VERSION_ID")" != "22.04" ]]; then
  echo "Build phải chạy trên Ubuntu 22.04 amd64." >&2; exit 1
fi

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
OUTPUT_DIR="${1:-$REPO_ROOT/build-output/v1.0.1}"
STAGE="$OUTPUT_DIR/ubuntu-iso"
PACKAGES="$STAGE/packages"
APP="$STAGE/payload/app"
SQL_VERSION="16.0.4295.3-1"

rm -rf -- "$STAGE"
mkdir -p "$PACKAGES" "$APP" "$STAGE/payload/clamav-signatures"
dotnet publish "$REPO_ROOT/IRM/IRM.csproj" -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false -o "$APP"

sudo apt-get update
sudo apt-get install -y --no-install-recommends apt-rdepends dpkg-dev xorriso curl ca-certificates gnupg clamav-freshclam

candidate="$(apt-cache policy mssql-server | awk '/Candidate:/ {print $2}')"
if [[ "$candidate" != "$SQL_VERSION" ]]; then
  echo "Repository không cung cấp mssql-server=$SQL_VERSION (candidate=$candidate)." >&2; exit 1
fi

mapfile -t package_names < <(
  apt-rdepends --follow=DEPENDS mssql-server mssql-tools18 msodbcsql18 clamav curl openssl 2>/dev/null |
    awk '/^[a-zA-Z0-9][a-zA-Z0-9+.-]*$/ {print}' | sort -u
)
(
  cd "$PACKAGES"
  for package in "${package_names[@]}"; do
    [[ "$(apt-cache policy "$package" | awk '/Candidate:/ {print $2}')" != "(none)" ]] && apt-get download "$package"
  done
  apt-get download "mssql-server=$SQL_VERSION"
  dpkg-scanpackages . /dev/null | gzip -9c > Packages.gz
)

temporary_clamav="$(mktemp -d)"
freshclam --datadir="$temporary_clamav"
shopt -s nullglob
signature_files=("$temporary_clamav"/*.cvd "$temporary_clamav"/*.cld)
[[ ${#signature_files[@]} -gt 0 ]] || { echo "Không tải được chữ ký ClamAV ngoại tuyến." >&2; exit 1; }
cp -a "${signature_files[@]}" "$STAGE/payload/clamav-signatures/"
shopt -u nullglob
rm -rf -- "$temporary_clamav"

cp "$SCRIPT_DIR/install.sh" "$STAGE/payload/install.sh"
cp "$SCRIPT_DIR/irm.service" "$STAGE/payload/irm.service"
cp "$SCRIPT_DIR/irm-backup.service" "$STAGE/payload/irm-backup.service"
cp "$SCRIPT_DIR/irm-backup.timer" "$STAGE/payload/irm-backup.timer"
cp "$SCRIPT_DIR/irm.desktop" "$STAGE/payload/irm.desktop"
cp "$SCRIPT_DIR/restore.sh" "$STAGE/payload/restore.sh"
cp "$SCRIPT_DIR/repair.sh" "$STAGE/payload/repair.sh"
cp "$REPO_ROOT/deploy/release-manifest.json" "$STAGE/payload/release-manifest.json"
cp "$REPO_ROOT/deploy/database/IRM-v1.0.1-schema.sql" "$STAGE/payload/IRM-v1.0.1-schema.sql"
chmod +x "$STAGE/payload/install.sh" "$STAGE/payload/restore.sh" "$STAGE/payload/repair.sh" "$APP/IRM"

dotnet list "$REPO_ROOT/IRM/IRM.csproj" package --include-transitive --format json > "$STAGE/payload/SBOM-dotnet.json"
dpkg-scanpackages "$PACKAGES" /dev/null > "$STAGE/payload/SBOM-deb.txt"

cat > "$STAGE/Install-IRM.desktop" <<'DESKTOP'
[Desktop Entry]
Type=Application
Name=Install IRM v1.0.1
Comment=Cài đặt IRM và SQL Server 2022 Express ngoại tuyến
Terminal=true
Exec=sh -c 'base=$(dirname "$1"); exec pkexec "$base/payload/install.sh"' sh %k
Icon=system-software-install
Categories=System;
DESKTOP
chmod +x "$STAGE/Install-IRM.desktop"

(
  cd "$STAGE"
  find . -type f ! -name SHA256SUMS.txt -print0 | sort -z | xargs -0 sha256sum > SHA256SUMS.txt
)

mkdir -p "$OUTPUT_DIR"
xorriso -as mkisofs -V "IRM_1_0_1" -J -joliet-long -r \
  -o "$OUTPUT_DIR/IRM-v1.0.1-ubuntu22.04-amd64-offline.iso" "$STAGE"
sha256sum "$OUTPUT_DIR/IRM-v1.0.1-ubuntu22.04-amd64-offline.iso" > \
  "$OUTPUT_DIR/IRM-v1.0.1-ubuntu22.04-amd64-offline.iso.sha256"
