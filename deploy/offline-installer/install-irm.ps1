<# ============================================================
   IRM v1.0.2 — Bộ cài đặt Offline cho Windows
   Dành cho khách hàng không có kiến thức kỹ thuật
   ============================================================ #>

#Requires -RunAsAdministrator
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$Host.UI.RawUI.WindowTitle = "IRM v1.0.2 — Cài đặt"

# ── Hằng số ──
$IRM_VERSION     = "1.0.2"
$INSTALL_DIR     = "C:\IRM"
$DATA_DIR        = "C:\IRM\data"
$SQL_INSTANCE    = "SQLEXPRESS"
$SQL_DB_NAME     = "IRM"
$IRM_PORT        = 5050
$IRM_HTTPS_PORT  = 5443
$MIN_RAM_GB      = 2
$MIN_DISK_GB     = 10
$ScriptDir       = Split-Path -Parent $MyInvocation.MyCommand.Definition
$PREREQS_DIR     = Join-Path $ScriptDir "prerequisites"
$APP_DIR         = Join-Path $ScriptDir "app"

# ── Hàm tiện ích ──
function Write-Banner {
    Clear-Host
    Write-Host ""
    Write-Host "  ╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "  ║                                                  ║" -ForegroundColor Cyan
    Write-Host "  ║   🏛️  IRM v$IRM_VERSION — Bộ cài đặt Offline          ║" -ForegroundColor Cyan
    Write-Host "  ║   Quản lý người nước ngoài                      ║" -ForegroundColor Cyan
    Write-Host "  ║                                                  ║" -ForegroundColor Cyan
    Write-Host "  ╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step {
    param([string]$Step, [string]$Title)
    Write-Host ""
    Write-Host "  ┌──────────────────────────────────────────────────" -ForegroundColor DarkCyan
    Write-Host "  │ BƯỚC $Step: $Title" -ForegroundColor Yellow
    Write-Host "  └──────────────────────────────────────────────────" -ForegroundColor DarkCyan
    Write-Host ""
}

function Write-Check {
    param([string]$Name, [bool]$Pass, [string]$Detail = "")
    if ($Pass) {
        Write-Host "    ✅  $Name" -ForegroundColor Green -NoNewline
    } else {
        Write-Host "    ❌  $Name" -ForegroundColor Red -NoNewline
    }
    if ($Detail) { Write-Host " — $Detail" -ForegroundColor Gray } else { Write-Host "" }
}

function Write-Info {
    param([string]$Message)
    Write-Host "    ℹ️  $Message" -ForegroundColor DarkGray
}

function Confirm-Continue {
    param([string]$Message = "Nhấn Enter để tiếp tục hoặc Ctrl+C để hủy...")
    Write-Host ""
    Write-Host "  $Message" -ForegroundColor Yellow
    Read-Host
}

# ══════════════════════════════════════════════════
# BƯỚC 1: QUÉT MÔI TRƯỜNG
# ══════════════════════════════════════════════════
function Invoke-EnvironmentScan {
    Write-Banner
    Write-Step "1/3" "QUÉT MÔI TRƯỜNG"
    Write-Host "    Đang kiểm tra máy tính của bạn..." -ForegroundColor White
    Write-Host ""

    $results = @{}

    # 1. Quyền Admin
    $isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    $results["Admin"] = $isAdmin
    Write-Check "Quyền Administrator" $isAdmin

    # 2. Hệ điều hành
    $os = Get-CimInstance Win32_OperatingSystem
    $osCaption = $os.Caption
    $osArch = $os.OSArchitecture
    $is64 = $osArch -match "64"
    $isWinOk = ($os.Version -ge "10.0") -and $is64
    $results["OS"] = $isWinOk
    Write-Check "Hệ điều hành" $isWinOk "$osCaption ($osArch)"

    # 3. CPU
    $cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
    $cpuCores = $cpu.NumberOfCores
    $cpuOk = $cpuCores -ge 2
    $results["CPU"] = $cpuOk
    Write-Check "CPU" $cpuOk "$($cpu.Name) — $cpuCores cores"

    # 4. RAM
    $totalRam = [math]::Round($os.TotalVisibleMemorySize / 1MB, 1)
    $freeRam = [math]::Round($os.FreePhysicalMemory / 1MB, 1)
    $ramOk = $totalRam -ge $MIN_RAM_GB
    $results["RAM"] = $ramOk
    Write-Check "RAM" $ramOk "${totalRam} GB tổng, ${freeRam} GB trống (yêu cầu ≥ ${MIN_RAM_GB} GB)"

    # 5. Ổ đĩa
    $drive = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='C:'"
    $freeGB = [math]::Round($drive.FreeSpace / 1GB, 1)
    $diskOk = $freeGB -ge $MIN_DISK_GB
    $results["Disk"] = $diskOk
    Write-Check "Ổ đĩa C:" $diskOk "${freeGB} GB trống (yêu cầu ≥ ${MIN_DISK_GB} GB)"

    # 6. SQL Server
    $sqlService = Get-Service -Name "MSSQL`$$SQL_INSTANCE" -ErrorAction SilentlyContinue
    $sqlInstalled = $null -ne $sqlService
    $sqlRunning = $sqlInstalled -and $sqlService.Status -eq "Running"
    $results["SQLInstalled"] = $sqlInstalled
    $results["SQLRunning"] = $sqlRunning
    if ($sqlInstalled) {
        Write-Check "SQL Server Express" $true "Đã cài đặt — Trạng thái: $($sqlService.Status)"
    } else {
        Write-Check "SQL Server Express" $false "Chưa cài đặt — sẽ được cài ở bước 2"
    }

    # 7. Port 5050
    $port5050 = Get-NetTCPConnection -LocalPort $IRM_PORT -ErrorAction SilentlyContinue
    $port5050Free = $null -eq $port5050
    $results["Port5050"] = $port5050Free
    Write-Check "Cổng $IRM_PORT" $port5050Free $(if ($port5050Free) { "Đang trống" } else { "ĐANG BỊ CHIẾM — cần giải phóng" })

    # 8. Port 5443
    $port5443 = Get-NetTCPConnection -LocalPort $IRM_HTTPS_PORT -ErrorAction SilentlyContinue
    $port5443Free = $null -eq $port5443
    $results["Port5443"] = $port5443Free
    Write-Check "Cổng $IRM_HTTPS_PORT (HTTPS)" $port5443Free $(if ($port5443Free) { "Đang trống" } else { "ĐANG BỊ CHIẾM" })

    # 9. Windows Defender
    try {
        $defender = Get-MpComputerStatus -ErrorAction SilentlyContinue
        $defenderOk = $defender.AntivirusEnabled
        $results["Defender"] = $defenderOk
        Write-Check "Windows Defender" $defenderOk $(if ($defenderOk) { "Đang hoạt động" } else { "Không hoạt động" })
    } catch {
        $results["Defender"] = $false
        Write-Check "Windows Defender" $false "Không xác định được"
    }

    # 10. IRM đã cài trước?
    $irmExists = Test-Path $INSTALL_DIR
    $results["IRMExists"] = $irmExists
    if ($irmExists) {
        Write-Check "IRM đã cài đặt trước" $false "Phát hiện bản cài cũ tại $INSTALL_DIR — sẽ được nâng cấp"
    } else {
        Write-Check "Thư mục cài đặt" $true "$INSTALL_DIR — Sẵn sàng"
    }

    # 11. Firewall
    $fwEnabled = (Get-NetFirewallProfile -Profile Domain,Public,Private | Where-Object { $_.Enabled -eq $true }).Count -gt 0
    $results["Firewall"] = $fwEnabled
    Write-Check "Windows Firewall" $fwEnabled $(if ($fwEnabled) { "Đang bật" } else { "Đã tắt — nên bật" })

    # ── Tổng kết ──
    Write-Host ""
    Write-Host "  ┌──────────────────────────────────────────────────" -ForegroundColor DarkCyan
    Write-Host "  │ KẾT QUẢ QUÉT MÔI TRƯỜNG" -ForegroundColor Yellow
    Write-Host "  └──────────────────────────────────────────────────" -ForegroundColor DarkCyan

    $critical = @("Admin", "OS", "CPU", "RAM", "Disk")
    $criticalPass = ($critical | Where-Object { $results[$_] }).Count
    $criticalTotal = $critical.Count

    if ($criticalPass -eq $criticalTotal) {
        Write-Host ""
        Write-Host "    🟢  MÁY TÍNH ĐẠT YÊU CẦU ($criticalPass/$criticalTotal tiêu chí bắt buộc)" -ForegroundColor Green
        if (-not $results["SQLInstalled"]) {
            Write-Host "    🟡  SQL Server chưa cài — sẽ được cài tự động ở bước tiếp theo" -ForegroundColor Yellow
        }
    } else {
        Write-Host ""
        Write-Host "    🔴  MÁY TÍNH CHƯA ĐẠT YÊU CẦU ($criticalPass/$criticalTotal)" -ForegroundColor Red
        Write-Host "    Vui lòng khắc phục các mục ❌ ở trên trước khi tiếp tục." -ForegroundColor Red
        Confirm-Continue "Nhấn Enter để thoát..."
        exit 1
    }

    return $results
}

# ══════════════════════════════════════════════════
# BƯỚC 2: CÀI ĐẶT THÀNH PHẦN THIẾU
# ══════════════════════════════════════════════════
function Install-Prerequisites {
    param([hashtable]$ScanResults)

    Write-Step "2/3" "CÀI ĐẶT THÀNH PHẦN THIẾU"

    $needInstall = @()

    if (-not $ScanResults["SQLInstalled"]) {
        $needInstall += "SQL Server 2022 Express"
    }

    if ($needInstall.Count -eq 0) {
        Write-Host "    ✅  Tất cả thành phần đã sẵn sàng — không cần cài thêm" -ForegroundColor Green
        # Đảm bảo SQL đang chạy
        if ($ScanResults["SQLInstalled"] -and -not $ScanResults["SQLRunning"]) {
            Write-Host "    🔄  Đang khởi động SQL Server..." -ForegroundColor Yellow
            Start-Service "MSSQL`$$SQL_INSTANCE" -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 3
            Write-Host "    ✅  SQL Server đã khởi động" -ForegroundColor Green
        }
        return
    }

    Write-Host "    Các thành phần cần cài đặt:" -ForegroundColor White
    foreach ($item in $needInstall) {
        Write-Host "      • $item" -ForegroundColor Yellow
    }

    Confirm-Continue "Nhấn Enter để bắt đầu cài đặt..."

    # ── Cài SQL Server Express ──
    if (-not $ScanResults["SQLInstalled"]) {
        Write-Host ""
        Write-Host "    🔄  Đang cài đặt SQL Server 2022 Express..." -ForegroundColor Yellow
        Write-Host "    ⏳  Quá trình này mất khoảng 5-10 phút, vui lòng chờ..." -ForegroundColor DarkGray

        $sqlSetup = Join-Path $PREREQS_DIR "SQLEXPRESS\setup.exe"
        if (-not (Test-Path $sqlSetup)) {
            # Fallback: tìm tên file khác
            $sqlSetup = Get-ChildItem $PREREQS_DIR -Filter "SQL*EXPR*.exe" -Recurse | Select-Object -First 1 -ExpandProperty FullName
        }

        if (-not $sqlSetup -or -not (Test-Path $sqlSetup)) {
            Write-Host "    ❌  Không tìm thấy bộ cài SQL Server trong thư mục prerequisites\" -ForegroundColor Red
            Write-Host "    Vui lòng đặt file cài đặt SQL Server vào: $PREREQS_DIR" -ForegroundColor Red
            Confirm-Continue "Nhấn Enter để thoát..."
            exit 1
        }

        # Silent install SQL Server Express
        $sqlArgs = @(
            "/Q",                                # Quiet mode
            "/ACTION=Install",
            "/FEATURES=SQLEngine",
            "/INSTANCENAME=$SQL_INSTANCE",
            "/SQLSVCSTARTUPTYPE=Automatic",
            "/SQLSYSADMINACCOUNTS=`"BUILTIN\Administrators`"",
            "/SECURITYMODE=SQL",
            "/SAPWD=`"IRM_Temp_Sa_$(Get-Random -Maximum 99999)!`"",
            "/TCPENABLED=0",                     # Chỉ named pipe, không mở TCP
            "/NPENABLED=1",
            "/BROWSERSVCSTARTUPTYPE=Disabled",
            "/IACCEPTSQLSERVERLICENSETERMS"
        )

        $process = Start-Process -FilePath $sqlSetup -ArgumentList $sqlArgs -Wait -PassThru -NoNewWindow
        if ($process.ExitCode -ne 0 -and $process.ExitCode -ne 3010) {
            Write-Host "    ❌  Cài SQL Server thất bại (exit code: $($process.ExitCode))" -ForegroundColor Red
            Write-Host "    Kiểm tra log tại: C:\Program Files\Microsoft SQL Server\*\Setup Bootstrap\Log" -ForegroundColor Yellow
            Confirm-Continue "Nhấn Enter để thoát..."
            exit 1
        }

        # Chờ service khởi động
        Write-Host "    ⏳  Đang chờ SQL Server khởi động..." -ForegroundColor DarkGray
        $retries = 0
        while ($retries -lt 30) {
            $svc = Get-Service -Name "MSSQL`$$SQL_INSTANCE" -ErrorAction SilentlyContinue
            if ($svc -and $svc.Status -eq "Running") { break }
            Start-Sleep -Seconds 2
            $retries++
        }

        if ($retries -ge 30) {
            Write-Host "    ❌  SQL Server không khởi động được" -ForegroundColor Red
            exit 1
        }

        Write-Host "    ✅  SQL Server 2022 Express đã cài thành công" -ForegroundColor Green
    }
}

# ══════════════════════════════════════════════════
# BƯỚC 3: CÀI ĐẶT ỨNG DỤNG IRM
# ══════════════════════════════════════════════════
function Install-IRMApplication {
    Write-Step "3/3" "CÀI ĐẶT ỨNG DỤNG IRM v$IRM_VERSION"

    # ── 3.1 Tạo thư mục ──
    Write-Host "    🔄  Tạo thư mục cài đặt..." -ForegroundColor Yellow
    @($INSTALL_DIR, $DATA_DIR, "$DATA_DIR\private-files", "$DATA_DIR\dataprotection-keys", "$DATA_DIR\backups") | ForEach-Object {
        if (-not (Test-Path $_)) { New-Item -ItemType Directory -Path $_ -Force | Out-Null }
    }

    # ── 3.2 Dừng service cũ nếu có ──
    $existingSvc = Get-Service -Name "IRM" -ErrorAction SilentlyContinue
    if ($existingSvc) {
        Write-Host "    🔄  Dừng bản IRM cũ..." -ForegroundColor Yellow
        Stop-Service "IRM" -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    }

    # ── 3.3 Copy ứng dụng ──
    Write-Host "    🔄  Sao chép ứng dụng..." -ForegroundColor Yellow
    $appSource = $APP_DIR
    if (-not (Test-Path $appSource)) {
        Write-Host "    ❌  Không tìm thấy thư mục app\ trong bộ cài" -ForegroundColor Red
        exit 1
    }
    Copy-Item "$appSource\*" "$INSTALL_DIR\" -Recurse -Force

    # ── 3.4 Tạo connection string ──
    Write-Host "    🔄  Cấu hình kết nối database..." -ForegroundColor Yellow
    $connStr = "Server=.\$SQL_INSTANCE;Database=$SQL_DB_NAME;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
    $appSettingsPath = Join-Path $INSTALL_DIR "appsettings.json"
    if (Test-Path $appSettingsPath) {
        $settings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        if (-not $settings.ConnectionStrings) {
            $settings | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{} -Force
        }
        $settings.ConnectionStrings | Add-Member -Type NoteProperty -Name "DefaultConnection" -Value $connStr -Force
        $settings.ConnectionStrings | Add-Member -Type NoteProperty -Name "SqlServer" -Value $connStr -Force
        $settings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
    }

    Write-Host "    ✅  Đã cấu hình kết nối: .\$SQL_INSTANCE\$SQL_DB_NAME" -ForegroundColor Green

    # ── 3.5 Tạo database ──
    Write-Host "    🔄  Tạo database..." -ForegroundColor Yellow
    $sqlCmd = "IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = '$SQL_DB_NAME') CREATE DATABASE [$SQL_DB_NAME];"
    try {
        sqlcmd -S ".\$SQL_INSTANCE" -E -Q $sqlCmd -b 2>$null
        if ($LASTEXITCODE -ne 0) {
            # Fallback: dùng Invoke-Sqlcmd nếu có module
            Import-Module SqlServer -ErrorAction SilentlyContinue
            Invoke-Sqlcmd -ServerInstance ".\$SQL_INSTANCE" -Query $sqlCmd
        }
        Write-Host "    ✅  Database $SQL_DB_NAME đã sẵn sàng" -ForegroundColor Green
    } catch {
        Write-Host "    ⚠️  Không tạo được database tự động. IRM sẽ tự tạo khi khởi động lần đầu." -ForegroundColor Yellow
    }

    # ── 3.6 Chạy EF Migration ──
    Write-Host "    🔄  Chạy migration database (tạo bảng)..." -ForegroundColor Yellow
    $migrationSql = Join-Path $ScriptDir "migration.sql"
    if (Test-Path $migrationSql) {
        try {
            sqlcmd -S ".\$SQL_INSTANCE" -d $SQL_DB_NAME -E -i $migrationSql -b 2>$null
            Write-Host "    ✅  Migration hoàn tất" -ForegroundColor Green
        } catch {
            Write-Host "    ⚠️  Migration sẽ được IRM tự chạy khi khởi động" -ForegroundColor Yellow
        }
    } else {
        Write-Info "File migration.sql không có — IRM sẽ tự chạy EF migration khi khởi động"
    }

    # ── 3.7 Tạo HTTPS certificate (self-signed) ──
    Write-Host "    🔄  Tạo chứng thư HTTPS tự ký..." -ForegroundColor Yellow
    $certExists = Get-ChildItem Cert:\LocalMachine\My | Where-Object { $_.Subject -match "IRM" }
    if (-not $certExists) {
        try {
            $cert = New-SelfSignedCertificate -DnsName $env:COMPUTERNAME, "localhost" -CertStoreLocation Cert:\LocalMachine\My -FriendlyName "IRM v$IRM_VERSION" -NotAfter (Get-Date).AddYears(5)
            Write-Host "    ✅  Chứng thư HTTPS đã tạo (hết hạn: $((Get-Date).AddYears(5).ToString('dd/MM/yyyy')))" -ForegroundColor Green
        } catch {
            Write-Host "    ⚠️  Không tạo được chứng thư — HTTPS có thể không hoạt động" -ForegroundColor Yellow
        }
    } else {
        Write-Host "    ✅  Chứng thư HTTPS đã tồn tại" -ForegroundColor Green
    }

    # ── 3.8 Đăng ký Windows Service ──
    Write-Host "    🔄  Đăng ký Windows Service..." -ForegroundColor Yellow
    $irmExe = Join-Path $INSTALL_DIR "IRM.exe"

    if (-not $existingSvc) {
        New-Service -Name "IRM" -BinaryPathName "`"$irmExe`"" -DisplayName "IRM - Quản lý NNN" -Description "Hệ thống quản lý người nước ngoài IRM v$IRM_VERSION" -StartupType Automatic | Out-Null
    }

    # Cấu hình service environment
    $regPath = "HKLM:\SYSTEM\CurrentControlSet\Services\IRM"
    $envVars = @(
        "ASPNETCORE_ENVIRONMENT=Production"
        "ASPNETCORE_URLS=http://+:$IRM_PORT;https://+:$IRM_HTTPS_PORT"
        "FileStorage__Root=$DATA_DIR\private-files"
        "DataProtection__KeysPath=$DATA_DIR\dataprotection-keys"
    )
    Set-ItemProperty -Path $regPath -Name "Environment" -Value $envVars -Type MultiString

    Write-Host "    ✅  Windows Service 'IRM' đã đăng ký" -ForegroundColor Green

    # ── 3.9 Firewall rules ──
    Write-Host "    🔄  Cấu hình Firewall..." -ForegroundColor Yellow
    $fwRules = @(
        @{ Name = "IRM HTTP";  Port = $IRM_PORT;       Protocol = "TCP" },
        @{ Name = "IRM HTTPS"; Port = $IRM_HTTPS_PORT;  Protocol = "TCP" }
    )
    foreach ($rule in $fwRules) {
        $existing = Get-NetFirewallRule -DisplayName $rule.Name -ErrorAction SilentlyContinue
        if (-not $existing) {
            New-NetFirewallRule -DisplayName $rule.Name -Direction Inbound -Protocol $rule.Protocol -LocalPort $rule.Port -Action Allow | Out-Null
        }
    }
    Write-Host "    ✅  Firewall đã mở cổng $IRM_PORT (HTTP) và $IRM_HTTPS_PORT (HTTPS)" -ForegroundColor Green

    # ── 3.10 Khởi động ──
    Write-Host ""
    Write-Host "    🚀  Đang khởi động IRM..." -ForegroundColor Yellow
    Start-Service "IRM"
    Start-Sleep -Seconds 5

    # Kiểm tra service đã chạy
    $svc = Get-Service "IRM"
    if ($svc.Status -eq "Running") {
        Write-Host "    ✅  IRM đang chạy!" -ForegroundColor Green
    } else {
        Write-Host "    ⚠️  IRM chưa khởi động. Kiểm tra log tại $INSTALL_DIR\logs\" -ForegroundColor Yellow
    }

    # ── 3.11 Health check ──
    Write-Host "    🔄  Kiểm tra sức khỏe hệ thống..." -ForegroundColor Yellow
    $retries = 0
    $healthy = $false
    while ($retries -lt 10) {
        try {
            $resp = Invoke-WebRequest -Uri "http://localhost:$IRM_PORT/health/ready" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
            if ($resp.StatusCode -eq 200) {
                $healthy = $true
                break
            }
        } catch { }
        Start-Sleep -Seconds 3
        $retries++
    }

    if ($healthy) {
        Write-Host "    ✅  Health check PASSED — Hệ thống hoạt động bình thường" -ForegroundColor Green
    } else {
        Write-Host "    ⚠️  Health check chưa phản hồi. IRM có thể đang khởi tạo database lần đầu." -ForegroundColor Yellow
        Write-Host "    Vui lòng chờ 30 giây rồi truy cập http://localhost:$IRM_PORT" -ForegroundColor Yellow
    }

    # ── Tạo shortcut Desktop ──
    Write-Host "    🔄  Tạo shortcut trên Desktop..." -ForegroundColor Yellow
    try {
        $desktopPath = [Environment]::GetFolderPath("CommonDesktopDirectory")
        $shell = New-Object -ComObject WScript.Shell
        $shortcut = $shell.CreateShortcut("$desktopPath\IRM - Quản lý NNN.lnk")
        $shortcut.TargetPath = "http://localhost:$IRM_PORT"
        $shortcut.IconLocation = "shell32.dll,14"
        $shortcut.Description = "Hệ thống quản lý người nước ngoài IRM v$IRM_VERSION"
        $shortcut.Save()
        Write-Host "    ✅  Shortcut đã tạo trên Desktop" -ForegroundColor Green
    } catch {
        Write-Info "Không tạo được shortcut — truy cập trực tiếp http://localhost:$IRM_PORT"
    }
}

# ══════════════════════════════════════════════════
# MAIN
# ══════════════════════════════════════════════════

try {
    # BƯỚC 1
    $scanResults = Invoke-EnvironmentScan
    Confirm-Continue "Nhấn Enter để tiếp tục cài đặt..."

    # BƯỚC 2
    Write-Banner
    Install-Prerequisites -ScanResults $scanResults
    Start-Sleep -Seconds 1

    # BƯỚC 3
    Write-Banner
    Install-IRMApplication

    # ── HOÀN TẤT ──
    Write-Host ""
    Write-Host "  ╔══════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "  ║                                                  ║" -ForegroundColor Green
    Write-Host "  ║   🎉  CÀI ĐẶT HOÀN TẤT!                       ║" -ForegroundColor Green
    Write-Host "  ║                                                  ║" -ForegroundColor Green
    Write-Host "  ║   Truy cập: http://localhost:$IRM_PORT              ║" -ForegroundColor Green
    Write-Host "  ║   LAN:      https://$($env:COMPUTERNAME):$IRM_HTTPS_PORT      ║" -ForegroundColor Green
    Write-Host "  ║                                                  ║" -ForegroundColor Green
    Write-Host "  ║   Đăng nhập lần đầu → tạo tài khoản Admin      ║" -ForegroundColor Green
    Write-Host "  ║                                                  ║" -ForegroundColor Green
    Write-Host "  ╚══════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""

    # Mở trình duyệt
    Start-Process "http://localhost:$IRM_PORT"

    Confirm-Continue "Nhấn Enter để đóng cửa sổ cài đặt..."

} catch {
    Write-Host ""
    Write-Host "  ❌  LỖI: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Vui lòng chụp ảnh màn hình này và gửi cho bộ phận hỗ trợ." -ForegroundColor Yellow
    Confirm-Continue "Nhấn Enter để thoát..."
    exit 1
}
