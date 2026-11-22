#!/bin/bash
# ============================================================
#  IRM v1.0.2 — Bo cai dat Offline cho Ubuntu/Debian
#  Chay: sudo bash install-irm.sh
# ============================================================

set -e

IRM_VERSION="1.0.2"
INSTALL_DIR="/opt/irm"
DATA_DIR="/opt/irm/data"
SQL_SA_PASSWORD=""
IRM_PORT=5050
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# ── Mau sac ──
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
GRAY='\033[0;37m'
NC='\033[0m'

# ── Ham tien ich ──
print_banner() {
    clear 2>/dev/null || true
    echo ""
    echo -e "${CYAN}  ╔══════════════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}  ║                                                  ║${NC}"
    echo -e "${CYAN}  ║   IRM v${IRM_VERSION} — Bo cai dat Offline (Ubuntu)       ║${NC}"
    echo -e "${CYAN}  ║   He thong Quan ly Nguoi nuoc ngoai              ║${NC}"
    echo -e "${CYAN}  ║                                                  ║${NC}"
    echo -e "${CYAN}  ╚══════════════════════════════════════════════════╝${NC}"
    echo ""
}

print_step() {
    echo ""
    echo -e "${CYAN}  ┌──────────────────────────────────────────────────${NC}"
    echo -e "${YELLOW}  │ BUOC $1: $2${NC}"
    echo -e "${CYAN}  └──────────────────────────────────────────────────${NC}"
    echo ""
}

check_pass() {
    echo -e "    ${GREEN}✅  $1${NC} ${GRAY}— $2${NC}"
}

check_fail() {
    echo -e "    ${RED}❌  $1${NC} ${GRAY}— $2${NC}"
}

check_warn() {
    echo -e "    ${YELLOW}⚠️   $1${NC} ${GRAY}— $2${NC}"
}

check_info() {
    echo -e "    ${CYAN}ℹ️   $1${NC}"
}

# ══════════════════════════════════════════════════
# BUOC 1: QUET MOI TRUONG
# ══════════════════════════════════════════════════
scan_environment() {
    print_banner
    print_step "1/3" "QUET MOI TRUONG"

    local critical_fail=0

    # 1. Root
    if [ "$EUID" -eq 0 ]; then
        check_pass "Quyen root/sudo" "Dang chay voi quyen root"
    else
        check_fail "Quyen root/sudo" "Can chay: sudo bash install-irm.sh"
        critical_fail=1
    fi

    # 2. OS
    if [ -f /etc/os-release ]; then
        . /etc/os-release
        os_name="$PRETTY_NAME"
        if echo "$ID" | grep -qiE "ubuntu|debian"; then
            check_pass "He dieu hanh" "$os_name"
        else
            check_warn "He dieu hanh" "$os_name (khong phai Ubuntu/Debian, co the gap loi)"
        fi
    else
        check_fail "He dieu hanh" "Khong xac dinh duoc OS"
        critical_fail=1
    fi

    # 3. Architecture
    arch=$(uname -m)
    if [ "$arch" = "x86_64" ]; then
        check_pass "Kien truc" "$arch (64-bit)"
    else
        check_fail "Kien truc" "$arch — Can x86_64"
        critical_fail=1
    fi

    # 4. CPU
    cpu_cores=$(nproc 2>/dev/null || echo 1)
    cpu_model=$(grep -m1 "model name" /proc/cpuinfo 2>/dev/null | cut -d: -f2 | xargs || echo "Unknown")
    if [ "$cpu_cores" -ge 2 ]; then
        check_pass "CPU" "$cpu_model — $cpu_cores cores"
    elif [ "$cpu_cores" -ge 1 ]; then
        check_warn "CPU" "$cpu_cores core (khuyen nghi >= 2 cores)"
    else
        check_fail "CPU" "Khong xac dinh duoc CPU"
        critical_fail=1
    fi

    # 5. RAM
    total_ram_kb=$(grep MemTotal /proc/meminfo | awk '{print $2}')
    total_ram_gb=$(echo "scale=1; $total_ram_kb / 1048576" | bc 2>/dev/null || echo "?")
    free_ram_kb=$(grep MemAvailable /proc/meminfo | awk '{print $2}')
    free_ram_gb=$(echo "scale=1; $free_ram_kb / 1048576" | bc 2>/dev/null || echo "?")
    if [ "$total_ram_kb" -ge 2000000 ]; then
        check_pass "RAM (>= 2 GB)" "${total_ram_gb} GB tong, ${free_ram_gb} GB trong"
    else
        check_fail "RAM (>= 2 GB)" "${total_ram_gb} GB"
        critical_fail=1
    fi

    # 6. Disk
    free_disk_gb=$(df -BG / | tail -1 | awk '{print $4}' | tr -d 'G')
    if [ "$free_disk_gb" -ge 10 ]; then
        check_pass "O dia (>= 10 GB trong)" "${free_disk_gb} GB trong"
    else
        check_fail "O dia (>= 10 GB trong)" "${free_disk_gb} GB"
        critical_fail=1
    fi

    # 7. Docker
    if command -v docker &>/dev/null; then
        docker_version=$(docker --version 2>/dev/null | head -1)
        check_pass "Docker" "$docker_version"
        DOCKER_INSTALLED=1
    else
        check_warn "Docker" "Chua cai — Se cai o buoc 2"
        DOCKER_INSTALLED=0
    fi

    # 8. Docker Compose
    if command -v docker &>/dev/null && docker compose version &>/dev/null; then
        compose_version=$(docker compose version 2>/dev/null | head -1)
        check_pass "Docker Compose" "$compose_version"
    else
        check_warn "Docker Compose" "Chua cai — Se cai cung Docker"
    fi

    # 9. Port 5050
    if ss -tlnp 2>/dev/null | grep -q ":${IRM_PORT} "; then
        check_warn "Cong $IRM_PORT" "DANG SU DUNG — co the la IRM cu"
    else
        check_pass "Cong $IRM_PORT" "Dang trong"
    fi

    # Summary
    echo ""
    if [ $critical_fail -eq 0 ]; then
        echo -e "    ${GREEN}══════════════════════════════════════════${NC}"
        echo -e "    ${GREEN}  ✅ MAY TINH DAT YEU CAU — San sang cai${NC}"
        echo -e "    ${GREEN}══════════════════════════════════════════${NC}"
    else
        echo -e "    ${RED}══════════════════════════════════════════${NC}"
        echo -e "    ${RED}  ❌ CHUA DAT YEU CAU${NC}"
        echo -e "    ${RED}══════════════════════════════════════════${NC}"
        echo ""
        echo -e "    ${RED}Vui long khac phuc cac muc ❌ roi chay lai.${NC}"
        exit 1
    fi

    if [ -t 0 ]; then
        read -p "    Nhan Enter de tiep tuc cai dat... " _
    fi
}

# ══════════════════════════════════════════════════
# BUOC 2: CAI THANH PHAN THIEU
# ══════════════════════════════════════════════════
install_prerequisites() {
    print_step "2/3" "CAI THANH PHAN THIEU"

    # Docker
    if [ "$DOCKER_INSTALLED" -eq 0 ]; then
        echo -e "    ${YELLOW}🔄 Dang cai Docker...${NC}"

        # Check if offline docker packages exist
        if [ -d "$SCRIPT_DIR/prerequisites/docker-debs" ]; then
            echo -e "    ${CYAN}   Tim thay goi cai offline${NC}"
            dpkg -i "$SCRIPT_DIR/prerequisites/docker-debs/"*.deb 2>/dev/null || true
            apt-get install -f -y 2>/dev/null || true
        else
            # Online install
            echo -e "    ${CYAN}   Cai tu Internet...${NC}"
            apt-get update -qq
            apt-get install -y -qq ca-certificates curl gnupg lsb-release

            install -m 0755 -d /etc/apt/keyrings
            curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg 2>/dev/null
            chmod a+r /etc/apt/keyrings/docker.gpg

            echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null

            apt-get update -qq
            apt-get install -y -qq docker-ce docker-ce-cli containerd.io docker-compose-plugin
        fi

        systemctl enable docker
        systemctl start docker
        echo -e "    ${GREEN}✅ Docker da cai thanh cong${NC}"
    else
        echo -e "    ${GREEN}✅ Docker da co san${NC}"
    fi
}

# ══════════════════════════════════════════════════
# BUOC 3: CAI DAT IRM
# ══════════════════════════════════════════════════
install_irm() {
    print_step "3/3" "CAI DAT IRM v${IRM_VERSION}"

    # 3.1 Tao thu muc
    echo -e "    ${YELLOW}🔄 Tao thu muc...${NC}"
    mkdir -p "$INSTALL_DIR" "$DATA_DIR/private-files" "$DATA_DIR/dataprotection-keys" "$DATA_DIR/backups" "$DATA_DIR/sql-data"
    chmod -R 777 "$DATA_DIR/private-files" "$DATA_DIR/dataprotection-keys" "$DATA_DIR/backups"
    echo -e "    ${GREEN}✅ Thu muc $INSTALL_DIR da tao${NC}"

    # 3.2 Dung container cu
    echo -e "    ${YELLOW}🔄 Kiem tra container cu...${NC}"
    cd "$INSTALL_DIR"
    docker compose down --timeout 30 2>/dev/null || true

    # 3.3 Copy docker-compose
    echo -e "    ${YELLOW}🔄 Sao chep cau hinh...${NC}"
    cp "$SCRIPT_DIR/docker-compose.yml" "$INSTALL_DIR/docker-compose.yml"

    # 3.4 Load Docker image
    if [ -f "$SCRIPT_DIR/irm-image.tar.gz" ]; then
        echo -e "    ${YELLOW}🔄 Nap Docker image (offline)...${NC}"
        docker load < "$SCRIPT_DIR/irm-image.tar.gz"
        echo -e "    ${GREEN}✅ Image da nap${NC}"
    elif [ -f "$SCRIPT_DIR/Dockerfile" ]; then
        echo -e "    ${YELLOW}🔄 Build Docker image...${NC}"
        cp "$SCRIPT_DIR/Dockerfile" "$INSTALL_DIR/"
        [ -d "$SCRIPT_DIR/app" ] && cp -r "$SCRIPT_DIR/app" "$INSTALL_DIR/"
        docker build -t "irm:${IRM_VERSION}" "$INSTALL_DIR/"
        echo -e "    ${GREEN}✅ Image da build${NC}"
    else
        echo -e "    ${RED}❌ Khong tim thay image hoac Dockerfile${NC}"
        exit 1
    fi

    # 3.5 Cau hinh mat khau SQL
    if [ -z "$SQL_SA_PASSWORD" ]; then
        echo ""
        echo -e "    ${CYAN}Nhap mat khau SQL Server (it nhat 12 ky tu):${NC}"
        read -s -p "    > " SQL_SA_PASSWORD
        echo ""
        if [ ${#SQL_SA_PASSWORD} -lt 12 ]; then
            SQL_SA_PASSWORD="IRM_Strong_P@ss_$(date +%s | tail -c 6)!"
            echo -e "    ${YELLOW}   Mat khau qua ngan — da tao tu dong${NC}"
        fi
    fi

    # Ghi vao .env
    cat > "$INSTALL_DIR/.env" << EOF
SQL_SA_PASSWORD=${SQL_SA_PASSWORD}
IRM_VERSION=${IRM_VERSION}
EOF
    chmod 600 "$INSTALL_DIR/.env"

    # 3.6 Khoi dong SQL Server va chuan bi database
    echo -e "    ${YELLOW}🚀 Dang khoi dong SQL Server...${NC}"
    cd "$INSTALL_DIR"
    docker compose up -d db

    echo -e "    ${YELLOW}⏳ Cho SQL Server san sang (khoang 15-30s)...${NC}"
    for i in $(seq 1 30); do
        if docker compose exec -T db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${SQL_SA_PASSWORD}" -Q "SELECT 1" -C -b >/dev/null 2>&1; then
            echo -e "    ${GREEN}✅ SQL Server da san sang${NC}"
            break
        fi
        sleep 2
        echo -n "."
    done
    echo ""

    echo -e "    ${YELLOW}🔄 Khoi tao database va he thong du lieu ban dau...${NC}"
    docker compose run --rm \
      -e ASPNETCORE_ENVIRONMENT=Production \
      -e "ConnectionStrings__DefaultConnection=Server=db;Database=IRM;User=sa;Password=${SQL_SA_PASSWORD};TrustServerCertificate=true;MultipleActiveResultSets=true" \
      -e "Provisioning__AdminConnectionString=Server=db;Database=IRM;User=sa;Password=${SQL_SA_PASSWORD};TrustServerCertificate=true;MultipleActiveResultSets=true" \
      -e "Bootstrap__AdminUsername=admin" \
      -e "Bootstrap__AdminPassword=${SQL_SA_PASSWORD}" \
      app dotnet IRM.dll --bootstrap >/dev/null 2>&1 || true

    echo -e "    ${YELLOW}🚀 Dang khoi dong ung dung IRM...${NC}"
    docker compose up -d app

    # 3.7 Cho health check
    echo -e "    ${YELLOW}⏳ Kiem tra trang thai hoat dong ung dung...${NC}"
    for i in $(seq 1 20); do
        sleep 2
        if curl -sf "http://localhost:${IRM_PORT}/health/ready" >/dev/null 2>&1; then
            echo -e "    ${GREEN}✅ Health check PASSED — IRM da hoat dong!${NC}"
            break
        fi
        echo -n "."
    done
    echo ""

    # 3.8 Tao systemd service cho auto-start
    echo -e "    ${YELLOW}🔄 Cau hinh tu khoi dong...${NC}"
    cat > /etc/systemd/system/irm.service << EOF
[Unit]
Description=IRM v${IRM_VERSION} - He thong Quan ly NNN
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=${INSTALL_DIR}
ExecStart=/usr/bin/docker compose up -d
ExecStop=/usr/bin/docker compose down
TimeoutStartSec=120

[Install]
WantedBy=multi-user.target
EOF
    systemctl daemon-reload
    systemctl enable irm.service 2>/dev/null
    echo -e "    ${GREEN}✅ Tu khoi dong da cau hinh${NC}"

    # 3.9 Firewall
    if command -v ufw &>/dev/null; then
        ufw allow ${IRM_PORT}/tcp 2>/dev/null || true
        echo -e "    ${GREEN}✅ Firewall da mo cong ${IRM_PORT}${NC}"
    fi
}

# ══════════════════════════════════════════════════
# MAIN
# ══════════════════════════════════════════════════
print_banner
scan_environment
install_prerequisites
install_irm

# Hoan tat
echo ""
echo -e "${GREEN}  ╔══════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}  ║                                                  ║${NC}"
echo -e "${GREEN}  ║   🎉 CAI DAT HOAN TAT!                          ║${NC}"
echo -e "${GREEN}  ║                                                  ║${NC}"
echo -e "${GREEN}  ║   Truy cap: http://localhost:${IRM_PORT}              ║${NC}"
echo -e "${GREEN}  ║   LAN:      http://$(hostname -I | awk '{print $1}'):${IRM_PORT}       ║${NC}"
echo -e "${GREEN}  ║                                                  ║${NC}"
echo -e "${GREEN}  ╚══════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "  ${GRAY}Lenh huu ich:${NC}"
echo -e "    docker compose -f ${INSTALL_DIR}/docker-compose.yml logs -f"
echo -e "    sudo systemctl status irm"
echo -e "    sudo systemctl restart irm"
echo ""
