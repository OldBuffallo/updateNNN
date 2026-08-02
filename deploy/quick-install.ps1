#Requires -RunAsAdministrator
<#
.SYNOPSIS
    IRM v2.0 — Cài đặt nhanh (Quick Install)
.DESCRIPTION
    Dành cho máy chủ ĐÃ CÓ SQL Server + database ReportManagerDB đang chạy.
    Không cần restore .bak, không cần USB phức tạp.

    Quy trình:
      1. Detect SQL Server instance
      2. Copy ứng dụng vào C:\IRM
      3. Chạy SQL migration (thêm bảng/cột mới)
      4. Cấu hình appsettings.json
      5. Tạo Windows Service
      6. Mở Firewall + Khởi động
.USAGE
    .\quick-install.ps1
    .\quick-install.ps1 -SqlInstance ".\BIRDIEPO" -SqlUser "sa" -SqlPassword "<your-password>"
    .\quick-install.ps1 -SqlInstance ".\SQLEXPRESS" -UseWindowsAuth
    .\quick-install.ps1 -Port 8080
#>

param(
    [string]$InstallDir = "C:\IRM",
    [int]$Port = 5050,
    [string]$SqlInstance = "",
    [string]$DbName = "ReportManagerDB",
    [string]$SqlUser = "",
    [string]$SqlPassword = "",
    [switch]$UseWindowsAuth,
    [switch]$SkipMigration,
    [switch]$SkipService,
    [switch]$SkipFirewall
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
Write-Host "    IRM v2.0 — Cai dat nhanh (Quick Install)" -ForegroundColor White
Write-Host "    Ket noi voi database co san" -ForegroundColor Gray
Write-Host "  ================================================================" -ForegroundColor DarkCyan
Write-Host ""

$ErrorActionPreference = "Stop"
$totalSteps = 6
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$appSourceDir = Join-Path $scriptDir "app"
$sqlScriptsDir = Join-Path $scriptDir "sql"

# Kiểm tra thư mục app/
if (-not (Test-Path (Join-Path $appSourceDir "IRM.exe"))) {
    # Thử tìm trong publish-temp
    $altAppDir = Join-Path (Split-Path $scriptDir) "IRM\publish-temp"
    if (Test-Path (Join-Path $altAppDir "IRM.exe")) {
        $appSourceDir = $altAppDir
        Write-Info "Su dung ban build tu: $altAppDir"
    } else {
        Write-Err "Khong tim thay 'app\IRM.exe' hoac 'IRM\publish-temp\IRM.exe'"
        Write-Err "Can build truoc: cd IRM && dotnet publish -c Release -o ..\deploy\app"
        exit 1
    }
}

# ============================================================
#  STEP 1: Detect SQL Server
# ============================================================
Write-Step "1" $totalSteps "Detect SQL Server"

if ([string]::IsNullOrEmpty($SqlInstance)) {
    Write-Info "Dang tim SQL Server tren may nay..."

    # Tìm tất cả SQL Server services đang chạy
    $sqlServices = Get-Service | Where-Object {
        $_.Name -like "MSSQL`$*" -or $_.Name -eq "MSSQLSERVER"
    } | Where-Object { $_.Status -eq "Running" }

    if ($sqlServices.Count -eq 0) {
        Write-Err "Khong tim thay SQL Server dang chay!"
        Write-Info "Chi dinh thu cong: .\quick-install.ps1 -SqlInstance '.\BIRDIEPO'"
        exit 1
    }

    # Liệt kê instances tìm được
    $instances = @()
    foreach ($svc in $sqlServices) {
        if ($svc.Name -eq "MSSQLSERVER") {
            $instances += "."
        } else {
            $instanceName = $svc.Name -replace "^MSSQL\`\$", ""
            $instances += ".\$instanceName"
        }
    }

    Write-Info "Tim thay $($instances.Count) SQL Server instance(s):"
    for ($i = 0; $i -lt $instances.Count; $i++) {
        Write-Host "      [$($i+1)] $($instances[$i])" -ForegroundColor White
    }

    if ($instances.Count -eq 1) {
        $SqlInstance = $instances[0]
        Write-Ok "Tu dong chon: $SqlInstance"
    } else {
        $choice = Read-Host "  Chon instance (1-$($instances.Count))"
        $idx = [int]$choice - 1
        if ($idx -ge 0 -and $idx -lt $instances.Count) {
            $SqlInstance = $instances[$idx]
        } else {
            $SqlInstance = $instances[0]
        }
        Write-Ok "Da chon: $SqlInstance"
    }
}

# Xác định connection string
if ($UseWindowsAuth -or ([string]::IsNullOrEmpty($SqlUser))) {
    # Thử Windows Authentication trước
    $sqlcmdAuth = "-E"
    $connString = "Server=$SqlInstance;Database=$DbName;Trusted_Connection=True;TrustServerCertificate=True;"
    Write-Info "Su dung Windows Authentication"
} else {
    $sqlcmdAuth = "-U `"$SqlUser`" -P `"$SqlPassword`""
    $connString = "Server=$SqlInstance;Database=$DbName;User Id=$SqlUser;Password=$SqlPassword;TrustServerCertificate=True;"
    Write-Info "Su dung SQL Server Authentication (User: $SqlUser)"
}

# Kiểm tra kết nối + database tồn tại
Write-Info "Kiem tra ket noi..."
$testCmd = "sqlcmd -S `"$SqlInstance`" $sqlcmdAuth -Q `"SELECT name FROM sys.databases WHERE name = '$DbName'`" -h -1 -W 2>&1"
$dbCheck = Invoke-Expression $testCmd

if ($LASTEXITCODE -ne 0) {
    # Thử lại với SQL Auth nếu Windows Auth thất bại
    if ($sqlcmdAuth -eq "-E" -and -not [string]::IsNullOrEmpty($SqlUser)) {
        Write-Warn "Windows Auth that bai, thu SQL Auth..."
        $sqlcmdAuth = "-U `"$SqlUser`" -P `"$SqlPassword`""
        $connString = "Server=$SqlInstance;Database=$DbName;User Id=$SqlUser;Password=$SqlPassword;TrustServerCertificate=True;"
        $testCmd = "sqlcmd -S `"$SqlInstance`" $sqlcmdAuth -Q `"SELECT name FROM sys.databases WHERE name = '$DbName'`" -h -1 -W 2>&1"
        $dbCheck = Invoke-Expression $testCmd
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Err "Khong ket noi duoc SQL Server '$SqlInstance'!"
        Write-Info "Thu lai voi: .\quick-install.ps1 -SqlInstance '.\INSTANCE' -SqlUser 'sa' -SqlPassword 'xxx'"
        exit 1
    }
}

if ($dbCheck -and $dbCheck.ToString().Trim() -eq $DbName) {
    # Đếm số record
    $countCmd = "sqlcmd -S `"$SqlInstance`" $sqlcmdAuth -d `"$DbName`" -Q `"SELECT COUNT(*) FROM Companies WHERE Delete_flag = 0`" -h -1 -W 2>&1"
    $companyCount = Invoke-Expression $countCmd 2>$null
    $empCountCmd = "sqlcmd -S `"$SqlInstance`" $sqlcmdAuth -d `"$DbName`" -Q `"SELECT COUNT(*) FROM Employees WHERE Hidden_flag = 0`" -h -1 -W 2>&1"
    $empCount = Invoke-Expression $empCountCmd 2>$null

    Write-Ok "Database '$DbName' — $($companyCount.ToString().Trim()) cong ty, $($empCount.ToString().Trim()) nhan vien"
} else {
    Write-Err "Database '$DbName' khong ton tai tren $SqlInstance!"
    exit 1
}

# ============================================================
#  STEP 2: Copy ứng dụng
# ============================================================
Write-Step "2" $totalSteps "Copy ung dung vao $InstallDir"

# Tạo thư mục
@($InstallDir, "$InstallDir\logs", "$InstallDir\backups") | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
        Write-Ok "Tao: $_"
    }
}

# Backup bản cũ nếu có
if (Test-Path (Join-Path $InstallDir "IRM.exe")) {
    $ts = Get-Date -Format "yyyyMMdd_HHmmss"
    $bkDir = Join-Path $InstallDir "backups\IRM_$ts"
    Write-Warn "Phat hien ban cu — backup tai: backups\IRM_$ts"
    New-Item -ItemType Directory -Path $bkDir -Force | Out-Null
    Copy-Item (Join-Path $InstallDir "IRM.exe") $bkDir -ErrorAction SilentlyContinue
    Copy-Item (Join-Path $InstallDir "appsettings.json") $bkDir -ErrorAction SilentlyContinue
}

# Dừng service cũ nếu có
$svcRunning = sc.exe query IRM 2>&1 | Select-String "RUNNING"
if ($svcRunning) {
    Write-Info "Dung service IRM dang chay..."
    sc.exe stop IRM 2>$null | Out-Null
    Start-Sleep -Seconds 5
}

Copy-Item -Path "$appSourceDir\*" -Destination $InstallDir -Recurse -Force
Write-Ok "Da copy toan bo ung dung"

# ============================================================
#  STEP 3: Chạy SQL migration
# ============================================================
Write-Step "3" $totalSteps "Chay SQL migration (them bang + cot moi)"

if ($SkipMigration) {
    Write-Warn "Bo qua migration (flag -SkipMigration)"
} else {
    if (Test-Path $sqlScriptsDir) {
        # Chỉ chạy migration scripts (01-05), KHÔNG chạy 00-full-setup
        $scripts = Get-ChildItem -Path $sqlScriptsDir -Filter "*.sql" |
            Where-Object { $_.Name -notlike "00-*" } |
            Sort-Object Name

        foreach ($script in $scripts) {
            Write-Info "Chay: $($script.Name)..."
            $migCmd = "sqlcmd -S `"$SqlInstance`" $sqlcmdAuth -d `"$DbName`" -i `"$($script.FullName)`" 2>&1"
            Invoke-Expression $migCmd | Out-Null
            if ($LASTEXITCODE -eq 0) {
                Write-Ok "$($script.Name)"
            } else {
                Write-Warn "$($script.Name) — co the da chay truoc do"
            }
        }
    } else {
        Write-Warn "Khong tim thay thu muc sql\ — ung dung se tu tao bang khi khoi dong"
    }
}

# ============================================================
#  STEP 4: Cấu hình
# ============================================================
Write-Step "4" $totalSteps "Cau hinh ung dung"

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
Write-Ok "appsettings.json — Port: $Port"
Write-Ok "SQL Server: $SqlInstance / $DbName"

# ============================================================
#  STEP 5: Tạo Windows Service
# ============================================================
Write-Step "5" $totalSteps "Tao Windows Service"

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
#  STEP 6: Firewall + Khởi động
# ============================================================
Write-Step "6" $totalSteps "Khoi dong"

# Firewall
if (-not $SkipFirewall) {
    netsh advfirewall firewall delete rule name="IRM Web Application" 2>$null | Out-Null
    netsh advfirewall firewall add rule name="IRM Web Application" dir=in action=allow protocol=tcp localport=$Port | Out-Null
    Write-Ok "Firewall: da mo port $Port"
}

# Tạo scripts tiện ích
$statusScript = @"
Write-Host "`n=== IRM v2.0 ===" -ForegroundColor Cyan
`$svc = Get-Service IRM -EA SilentlyContinue
Write-Host "  Service: `$(`$svc.Status)" -ForegroundColor `$(if(`$svc.Status -eq 'Running'){'Green'}else{'Red'})
`$p = netstat -an | Select-String ":$Port.*LISTEN"
Write-Host "  Port   : `$(if(`$p){'LISTENING'}else{'DOWN'})" -ForegroundColor `$(if(`$p){'Green'}else{'Red'})
try { `$r = Invoke-WebRequest http://localhost:$Port -TimeoutSec 5 -UseBasicParsing; Write-Host "  Web    : OK" -ForegroundColor Green } catch { Write-Host "  Web    : FAIL" -ForegroundColor Red }
Write-Host ""
"@
Set-Content -Path (Join-Path $InstallDir "status.ps1") -Value $statusScript
Write-Ok "status.ps1"

$backupScript = @"
`$f = "$InstallDir\backups\${DbName}_`$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
sqlcmd -S "$SqlInstance" $sqlcmdAuth -Q "BACKUP DATABASE [$DbName] TO DISK = N'`$f' WITH FORMAT, COMPRESSION"
Write-Host "Backup: `$f" -ForegroundColor Green
Get-ChildItem "$InstallDir\backups\${DbName}_*.bak" | Where-Object { `$_.LastWriteTime -lt (Get-Date).AddDays(-30) } | Remove-Item -Force -EA SilentlyContinue
"@
Set-Content -Path (Join-Path $InstallDir "backup-db.ps1") -Value $backupScript
Write-Ok "backup-db.ps1"

# Khởi động service
if (-not $SkipService) {
    sc.exe start IRM | Out-Null
    Start-Sleep 5
    $running = sc.exe query IRM | Select-String "RUNNING"
    if ($running) { Write-Ok "Service IRM dang chay!" }
    else { Write-Err "Service khong start — xem log: $InstallDir\logs\" }

    Start-Sleep 3
    try {
        $web = Invoke-WebRequest "http://localhost:$Port" -TimeoutSec 10 -UseBasicParsing
        Write-Ok "Web OK — Status $($web.StatusCode)"
    } catch { Write-Warn "Web chua phan hoi — doi them vai giay" }
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
Write-Host "  Database: $SqlInstance / $DbName" -ForegroundColor White
Write-Host ""
Write-Host "  Truy cap:" -ForegroundColor White
Write-Host "    May chu  : http://localhost:$Port" -ForegroundColor Cyan
if ($ip) {
Write-Host "    Client   : http://${ip}:$Port" -ForegroundColor Cyan
}
Write-Host ""
Write-Host "  Quan ly:" -ForegroundColor White
Write-Host "    Kiem tra  : powershell $InstallDir\status.ps1" -ForegroundColor Gray
Write-Host "    Backup DB : powershell $InstallDir\backup-db.ps1" -ForegroundColor Gray
Write-Host "    Dung      : sc.exe stop IRM" -ForegroundColor Gray
Write-Host "    Khoi dong : sc.exe start IRM" -ForegroundColor Gray
Write-Host "    Go bo     : sc.exe stop IRM && sc.exe delete IRM" -ForegroundColor Gray
Write-Host ""
Write-Host "  Dang nhap: tai khoan cu van su dung duoc" -ForegroundColor Yellow
Write-Host ""
