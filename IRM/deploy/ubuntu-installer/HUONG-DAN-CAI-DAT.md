# HUONG DAN CAI DAT IRM v1.0.2 — Ubuntu/Debian

## Yeu cau

- Ubuntu 20.04+ hoac Debian 11+ (64-bit)
- RAM >= 2 GB
- O dia >= 10 GB trong
- Quyen root/sudo

## Cai dat

### Buoc 1: Giai nen

```bash
tar -xzf IRM-v1.0.2-ubuntu-installer.tar.gz
cd IRM-v1.0.2-ubuntu-installer
```

### Buoc 2: Chay trinh cai dat

```bash
sudo bash install-irm.sh
```

Trinh cai dat se:
1. Quet moi truong may tinh
2. Cai Docker (neu chua co)
3. Khoi dong IRM + SQL Server

### Buoc 3: Truy cap

- May cai: http://localhost:5050
- May khac: http://DIA-CHI-IP:5050

## Lenh huu ich

```bash
# Xem log
docker compose -f /opt/irm/docker-compose.yml logs -f

# Khoi dong lai
sudo systemctl restart irm

# Dung
sudo systemctl stop irm

# Trang thai
sudo systemctl status irm
```

## Backup

Du lieu luu tai: /opt/irm/data/
Database: SQL Server trong Docker container

```bash
# Backup database
docker exec -it $(docker ps -qf "ancestor=mcr.microsoft.com/mssql/server:2022-latest") \
  -Q "BACKUP DATABASE [IRM] TO DISK='/var/opt/mssql/backup.bak'" -C
```

