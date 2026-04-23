#Requires -RunAsAdministrator
<#
.SYNOPSIS
    IRM v2.0 — Cài đặt 1 lần (One-Click Deploy)
.DESCRIPTION
    Quy trình:
      1. Tạo thư mục
      2. Copy ứng dụng
      3. Restore database từ file .bak (nếu có)
      4. Chạy SQL migration scripts
      5. Cấu hình appsettings.json
      6. Tạo Windows Service
      7. Mở Firewall
      8. Khởi động
.USAGE
    .\install.ps1
    .\install.ps1 -Port 8080
    .\install.ps1 -SqlInstance ".\SQLEXPRESS" -SkipRestore
#>

param(
    [string]$InstallDir = "C:\IRM",
    [int]$Port = 5050,
    [string]$SqlInstance = ".\SQLEXPRESS",
    [string]$DbName = "ReportManagerDB",
    [switch]$SkipFirewall,
    [switch]$SkipService,
    [switch]$SkipRestore,
    [switch]$SkipMigration,
    [switch]$SkipBackupTask
)

# ============================================================
#  HELPERS
# ============================================================
function Write-Step { param([string]$N, [string]$T, [string]$M)
    Write-Host ""; Write-Host "  [$N/$T] $M" -ForegroundColor Cyan; Write-Host "  $('-' * 50)" -ForegroundColor DarkGray }
function Write-Ok { param([string]$M) Write-Host "    [OK] $M" -ForegroundColor Green }
function Write-Warn { param([string]$M) Write-Host "    [!!] $M" -ForegroundColor Yellow }
function Write-Err { param([string]$M) Write-Host "    [XX] $M" -ForegroundColor Red }
function Write-Info { param([string]$M) Write-Host "    $M" -ForegroundColor Gray }

# ============================================================
#  BANNER
# ============================================================
Clear-Host
Write-Host ""
Write-Host "  ================================================================" -ForegroundColor DarkCyan
Write-Host "    IRM v2.0 — Cai dat tu dong (One-Click Deploy)" -ForegroundColor White
Write-Host "    He thong Quan ly Bao cao Lao dong Nuoc ngoai" -ForegroundColor Gray
Write-Host "  ================================================================" -ForegroundColor DarkCyan
Write-Host ""
Write-Host "  Cau hinh:" -ForegroundColor Yellow
Write-Host "    Thu muc cai dat : $InstallDir"
Write-Host "    Port            : $Port"
Write-Host "    SQL Server      : $SqlInstance"
Write-Host "    Database        : $DbName"
Write-Host ""

$confirm = Read-Host "  Ban co muon tiep tuc? (Y/N)"
if ($confirm -notin @('Y', 'y', 'Yes', 'yes')) { Write-Host "`n  Da huy." -ForegroundColor Yellow; exit 0 }

$ErrorActionPreference = "Stop"
$totalSteps = 8
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$appSourceDir = Join-Path $scriptDir "app"
$sqlScriptsDir = Join-Path $scriptDir "sql"
$dataDir = Join-Path $scriptDir "data"

# Kiểm tra thư mục app/
if (-not (Test-Path (Join-Path $appSourceDir "IRM.exe"))) {
    Write-Err "Khong tim thay 'app\IRM.exe'. Kiem tra cau truc USB."
    exit 1
}

# ============================================================
#  STEP 1: Tạo thư mục
# ============================================================
Write-Step "1" $totalSteps "Tao thu muc cai dat"

@($InstallDir, "$InstallDir\logs", "$InstallDir\backups") | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
        Write-Ok "Tao: $_"
    } else { Write-Info "Da ton tai: $_" }
}

# ============================================================
#  STEP 2: Copy ứng dụng
# ============================================================
Write-Step "2" $totalSteps "Copy ung dung vao $InstallDir"

# Backup bản cũ nếu có
if (Test-Path (Join-Path $InstallDir "IRM.exe")) {
    $ts = Get-Date -Format "yyyyMMdd_HHmmss"
    $bkDir = Join-Path $InstallDir "backups\IRM_$ts"
    Write-Warn "Phat hien ban cu — backup tai: backups\IRM_$ts"
    New-Item -ItemType Directory -Path $bkDir -Force | Out-Null
    Copy-Item (Join-Path $InstallDir "IRM.exe") $bkDir -ErrorAction SilentlyContinue
    Copy-Item (Join-Path $InstallDir "appsettings.json") $bkDir -ErrorAction SilentlyContinue
}

# Dừng service cũ
$svcRunning = sc.exe query IRM 2>&1 | Select-String "RUNNING"
if ($svcRunning) {
    Write-Info "Dung service IRM dang chay..."
    sc.exe stop IRM 2>$null | Out-Null
    Start-Sleep -Seconds 5
}

Copy-Item -Path "$appSourceDir\*" -Destination $InstallDir -Recurse -Force
Write-Ok "Da copy toan bo ung dung"

# ============================================================
#  STEP 3: Restore database từ .bak
# ============================================================
Write-Step "3" $totalSteps "Restore database"

if ($SkipRestore) {
    Write-Warn "Bo qua restore (flag -SkipRestore)"
} else {
    # Tìm file .bak trong thư mục data/
    $bakFile = $null
    if (Test-Path $dataDir) {
        $bakFile = Get-ChildItem -Path $dataDir -Filter "*.bak" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    }

    if ($bakFile) {
        $bakPath = $bakFile.FullName
        $bakSize = [math]::Round($bakFile.Length / 1MB, 1)
        Write-Info "Tim thay: $($bakFile.Name) ($bakSize MB)"

        # Kiểm tra DB đã tồn tại chưa
        $dbExists = sqlcmd -S "$SqlInstance" -E -Q "SELECT name FROM sys.databases WHERE name = '$DbName'" -h -1 -W 2>$null
        if ($dbExists -and $dbExists.Trim() -eq $DbName) {
            Write-Warn "Database '$DbName' da ton tai — backup truoc khi restore"
            $existingBackup = "$InstallDir\backups\${DbName}_existing_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
            sqlcmd -S "$SqlInstance" -E -Q "BACKUP DATABASE [$DbName] TO DISK = N'$existingBackup' WITH FORMAT" 2>$null
            Write-Ok "Backup DB cu tai: $existingBackup"
        }

        Write-Info "Dang restore database tu file backup..."

        # Lấy logical names từ backup file
        $fileListCmd = "RESTORE FILELISTONLY FROM DISK = N'$bakPath'"
        $fileListResult = sqlcmd -S "$SqlInstance" -E -Q "$fileListCmd" -W -s "|" 2>&1

        # Xác định đường dẫn mặc định SQL Server data
        $sqlDataPath = sqlcmd -S "$SqlInstance" -E -Q "SELECT SERVERPROPERTY('InstanceDefaultDataPath')" -h -1 -W 2>$null
        $sqlLogPath = sqlcmd -S "$SqlInstance" -E -Q "SELECT SERVERPROPERTY('InstanceDefaultLogPath')" -h -1 -W 2>$null

        if ([string]::IsNullOrWhiteSpace($sqlDataPath)) { $sqlDataPath = "C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA\" }
        if ([string]::IsNullOrWhiteSpace($sqlLogPath)) { $sqlLogPath = $sqlDataPath }

        $sqlDataPath = $sqlDataPath.Trim()
        $sqlLogPath = $sqlLogPath.Trim()

        $restoreSQL = @"
USE [master];
IF EXISTS (SELECT name FROM sys.databases WHERE name = '$DbName')
BEGIN
    ALTER DATABASE [$DbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
END

RESTORE DATABASE [$DbName]
FROM DISK = N'$bakPath'
WITH REPLACE,
     MOVE N'ReportManagerDB' TO N'${sqlDataPath}${DbName}.mdf',
     MOVE N'ReportManagerDB_log' TO N'${sqlLogPath}${DbName}_log.ldf',
     STATS = 10;

ALTER DATABASE [$DbName] SET MULTI_USER;
"@

        sqlcmd -S "$SqlInstance" -E -Q "$restoreSQL" 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Ok "Restore database thanh cong!"
        } else {
            # Thử restore không có MOVE (nếu logical names khác)
            Write-Warn "Thu restore don gian (khong MOVE)..."
            $simpleRestore = @"
USE [master];
IF EXISTS (SELECT name FROM sys.databases WHERE name = '$DbName')
    ALTER DATABASE [$DbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [$DbName] FROM DISK = N'$bakPath' WITH REPLACE, STATS = 10;
ALTER DATABASE [$DbName] SET MULTI_USER;
"@
            sqlcmd -S "$SqlInstance" -E -Q "$simpleRestore" 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Ok "Restore database thanh cong (simple mode)!"
            } else {
                Write-Err "Restore that bai! Kiem tra SQL Server va file .bak"
                Write-Info "Ban co the restore thu cong bang SSMS"
            }
        }
    } else {
        Write-Warn "Khong tim thay file .bak trong thu muc 'data\'"
        Write-Info "Neu database da co san tren server nay, bo qua buoc nay."
        Write-Info "Neu ban can restore, dat file .bak vao: $dataDir"
    }
}

# ============================================================
#  STEP 4: Chạy SQL migration scripts
# ============================================================
Write-Step "4" $totalSteps "Chay SQL migration scripts"

if ($SkipMigration) {
    Write-Warn "Bo qua migration (flag -SkipMigration)"
} else {
    # Kiểm tra database tồn tại
    $dbCheck = sqlcmd -S "$SqlInstance" -E -Q "SELECT name FROM sys.databases WHERE name = '$DbName'" -h -1 -W 2>$null

    if ($dbCheck -and $dbCheck.Trim() -eq $DbName) {
        if (Test-Path $sqlScriptsDir) {
            $scripts = Get-ChildItem -Path $sqlScriptsDir -Filter "*.sql" | Sort-Object Name
            foreach ($script in $scripts) {
                Write-Info "Chay: $($script.Name)..."
                sqlcmd -S "$SqlInstance" -E -d "$DbName" -i "$($script.FullName)" 2>&1 | Out-Null
                if ($LASTEXITCODE -eq 0) {
                    Write-Ok "$($script.Name)"
                } else {
                    Write-Warn "$($script.Name) — co the da chay truoc do"
                }
            }
        } else {
            Write-Info "Khong tim thay thu muc sql\ — bo qua"
        }
    } else {
        Write-Err "Database '$DbName' khong ton tai! Can restore truoc."
    }
}

# ============================================================
#  STEP 5: Cấu hình
# ============================================================
Write-Step "5" $totalSteps "Cau hinh ung dung"

$connString = "Server=$SqlInstance;Database=$DbName;Trusted_Connection=True;TrustServerCertificate=True;"

$appSettings = @{
    Urls = "http://0.0.0.0:$Port"
    ConnectionStrings = @{ DefaultConnection = $connString }
    Logging = @{
        LogLevel = @{
            Default = "Information"
            "Microsoft.AspNetCore" = "Warning"
            "Microsoft.EntityFrameworkCore" = "Warning"
        }
    }
    AllowedHosts = "*"
} | ConvertTo-Json -Depth 4

Set-Content -Path (Join-Path $InstallDir "appsettings.json") -Value $appSettings -Encoding UTF8
Write-Ok "appsettings.json — Port: $Port, DB: $DbName"

# ============================================================
#  STEP 6: Tạo Windows Service
# ============================================================
Write-Step "6" $totalSteps "Tao Windows Service"

if ($SkipService) {
    Write-Warn "Bo qua (flag -SkipService)"
} else {
    $svcCheck = sc.exe query IRM 2>&1
    if ($LASTEXITCODE -eq 0 -or ($svcCheck | Select-String "IRM")) {
        sc.exe stop IRM 2>$null | Out-Null; Start-Sleep 3
        sc.exe delete IRM 2>$null | Out-Null; Start-Sleep 2
    }

    $exePath = Join-Path $InstallDir "IRM.exe"
    sc.exe create IRM binPath="`"$exePath`" --urls http://0.0.0.0:$Port" start=auto displayname="IRM - Immigration Report Manager" | Out-Null
    sc.exe description IRM "He thong Quan ly Bao cao Lao dong Nuoc ngoai v2.0" | Out-Null
    sc.exe failure IRM reset=86400 actions=restart/60000/restart/60000/restart/60000 | Out-Null
    Write-Ok "Service 'IRM' — auto start, auto recovery"
}

# ============================================================
#  STEP 7: Mở Firewall
# ============================================================
Write-Step "7" $totalSteps "Cau hinh Firewall"

if ($SkipFirewall) {
    Write-Warn "Bo qua (flag -SkipFirewall)"
} else {
    netsh advfirewall firewall delete rule name="IRM Web Application" 2>$null | Out-Null
    netsh advfirewall firewall add rule name="IRM Web Application" dir=in action=allow protocol=tcp localport=$Port | Out-Null
    Write-Ok "Da mo port $Port — 3 may client co the truy cap"
}

# ============================================================
#  STEP 8: Khởi động
# ============================================================
Write-Step "8" $totalSteps "Khoi dong ung dung"

# Tạo scripts hỗ trợ
@{
    "status.ps1" = @"
Write-Host "`n=== IRM v2.0 ===" -ForegroundColor Cyan
`$svc = Get-Service IRM -EA SilentlyContinue
Write-Host "  Service: `$(`$svc.Status)" -ForegroundColor `$(if(`$svc.Status -eq 'Running'){'Green'}else{'Red'})
`$p = netstat -an | Select-String ":$Port.*LISTEN"
Write-Host "  Port   : `$(if(`$p){'LISTENING'}else{'DOWN'})" -ForegroundColor `$(if(`$p){'Green'}else{'Red'})
try { `$r = Invoke-WebRequest http://localhost:$Port -TimeoutSec 5 -UseBasicParsing; Write-Host "  Web    : OK" -ForegroundColor Green } catch { Write-Host "  Web    : FAIL" -ForegroundColor Red }
Write-Host ""
"@
    "backup-db.ps1" = @"
`$f = "$InstallDir\backups\${DbName}_`$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
sqlcmd -S "$SqlInstance" -E -Q "BACKUP DATABASE [$DbName] TO DISK = N'`$f' WITH FORMAT, COMPRESSION"
Write-Host "Backup: `$f" -ForegroundColor Green
Get-ChildItem "$InstallDir\backups\${DbName}_*.bak" | Where-Object { `$_.LastWriteTime -lt (Get-Date).AddDays(-30) } | Remove-Item -Force -EA SilentlyContinue
"@
} | ForEach-Object {
    $_.GetEnumerator() | ForEach-Object {
        Set-Content -Path (Join-Path $InstallDir $_.Key) -Value $_.Value
        Write-Ok $_.Key
    }
}

# Backup task hàng ngày
if (-not $SkipBackupTask) {
    try {
        Unregister-ScheduledTask -TaskName "IRM-DailyBackup" -Confirm:$false -EA SilentlyContinue
        $act = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-ExecutionPolicy Bypass -File `"$InstallDir\backup-db.ps1`""
        $trg = New-ScheduledTaskTrigger -Daily -At "02:00"
        Register-ScheduledTask -TaskName "IRM-DailyBackup" -Action $act -Trigger $trg -User "SYSTEM" -RunLevel Highest -Description "Backup DB IRM" | Out-Null
        Write-Ok "Auto backup luc 2:00 hang ngay"
    } catch { Write-Warn "Khong tao duoc scheduled task" }
}

# Khởi động
if (-not $SkipService) {
    sc.exe start IRM | Out-Null
    Start-Sleep 5
    $running = sc.exe query IRM | Select-String "RUNNING"
    if ($running) { Write-Ok "Service IRM dang chay!" }
    else { Write-Err "Service khong start — xem log: $InstallDir\logs\" }

    try {
        $web = Invoke-WebRequest "http://localhost:$Port" -TimeoutSec 10 -UseBasicParsing
        Write-Ok "Web OK — Status $($web.StatusCode)"
    } catch { Write-Warn "Web chua phan hoi — doi vai giay" }
}

# ============================================================
#  HOÀN TẤT
# ============================================================
$ip = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -ne "127.0.0.1" -and $_.PrefixOrigin -ne "WellKnown" } | Select-Object -First 1).IPAddress

Write-Host ""
Write-Host "  ================================================================" -ForegroundColor Green
Write-Host "    CAI DAT HOAN TAT!" -ForegroundColor Green
Write-Host "  ================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Truy cap:" -ForegroundColor White
Write-Host "    May chu  : http://localhost:$Port" -ForegroundColor Cyan
if ($ip) {
Write-Host "    Client 1 : http://${ip}:$Port" -ForegroundColor Cyan
Write-Host "    Client 2 : http://${ip}:$Port" -ForegroundColor Cyan
Write-Host "    Client 3 : http://${ip}:$Port" -ForegroundColor Cyan
}
Write-Host ""
Write-Host "  Quan ly:" -ForegroundColor White
Write-Host "    Kiem tra  : powershell $InstallDir\status.ps1" -ForegroundColor Gray
Write-Host "    Backup DB : powershell $InstallDir\backup-db.ps1" -ForegroundColor Gray
Write-Host "    Dung      : sc.exe stop IRM" -ForegroundColor Gray
Write-Host "    Khoi dong : sc.exe start IRM" -ForegroundColor Gray
Write-Host "    Go bo     : chay deploy\uninstall.ps1" -ForegroundColor Gray
Write-Host ""
