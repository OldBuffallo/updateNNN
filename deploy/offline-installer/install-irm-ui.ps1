<# ============================================================
   IRM v1.0.2 — Bộ cài đặt với Giao diện (Windows Forms)
   Khách hàng chỉ cần chạy file BAT kèm theo
   ============================================================ #>

# Tự relaunch với -STA nếu cần (WinForms yêu cầu STA thread)
if ([Threading.Thread]::CurrentThread.GetApartmentState() -ne 'STA') {
    Start-Process powershell -ArgumentList "-STA -NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Wait
    return
}

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
[System.Windows.Forms.Application]::EnableVisualStyles()

# ── Constants ──
$IRM_VERSION = "1.0.2"
$INSTALL_DIR = "C:\IRM"
$SQL_INSTANCE = "SQLEXPRESS"
$SQL_DB_NAME = "IRM"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

# ══════════════════════════════════════════════════
# FORM SETUP
# ══════════════════════════════════════════════════
$form = New-Object System.Windows.Forms.Form
$form.Text = "IRM v$IRM_VERSION — Cài đặt"
$form.Size = New-Object System.Drawing.Size(780, 580)
$form.StartPosition = "CenterScreen"
$form.FormBorderStyle = "FixedSingle"
$form.MaximizeBox = $false
$form.BackColor = [System.Drawing.Color]::FromArgb(18, 28, 40)
$form.ForeColor = [System.Drawing.Color]::FromArgb(200, 215, 230)
$form.Font = New-Object System.Drawing.Font("Segoe UI", 10)

# ── Header Panel ──
$headerPanel = New-Object System.Windows.Forms.Panel
$headerPanel.Dock = "Top"
$headerPanel.Height = 70
$headerPanel.BackColor = [System.Drawing.Color]::FromArgb(12, 20, 32)
$form.Controls.Add($headerPanel)

$lblTitle = New-Object System.Windows.Forms.Label
$lblTitle.Text = "🏛️  IRM v$IRM_VERSION — Cài đặt Hệ thống Quản lý Người nước ngoài"
$lblTitle.Font = New-Object System.Drawing.Font("Segoe UI", 14, [System.Drawing.FontStyle]::Bold)
$lblTitle.ForeColor = [System.Drawing.Color]::FromArgb(79, 195, 247)
$lblTitle.Location = New-Object System.Drawing.Point(20, 12)
$lblTitle.AutoSize = $true
$headerPanel.Controls.Add($lblTitle)

$lblSubtitle = New-Object System.Windows.Forms.Label
$lblSubtitle.Text = "Trình cài đặt tự động — Dành cho Windows 10/11/Server"
$lblSubtitle.Font = New-Object System.Drawing.Font("Segoe UI", 9)
$lblSubtitle.ForeColor = [System.Drawing.Color]::FromArgb(100, 140, 170)
$lblSubtitle.Location = New-Object System.Drawing.Point(48, 42)
$lblSubtitle.AutoSize = $true
$headerPanel.Controls.Add($lblSubtitle)

# ── Step indicator ──
$lblStep = New-Object System.Windows.Forms.Label
$lblStep.Text = "BƯỚC 1/3 — QUÉT MÔI TRƯỜNG"
$lblStep.Font = New-Object System.Drawing.Font("Segoe UI", 9, [System.Drawing.FontStyle]::Bold)
$lblStep.ForeColor = [System.Drawing.Color]::FromArgb(79, 195, 247)
$lblStep.Location = New-Object System.Drawing.Point(20, 80)
$lblStep.Size = New-Object System.Drawing.Size(730, 22)
$form.Controls.Add($lblStep)

# ── Results ListView ──
$listView = New-Object System.Windows.Forms.ListView
$listView.View = "Details"
$listView.FullRowSelect = $true
$listView.GridLines = $false
$listView.Location = New-Object System.Drawing.Point(20, 108)
$listView.Size = New-Object System.Drawing.Size(730, 260)
$listView.BackColor = [System.Drawing.Color]::FromArgb(22, 34, 50)
$listView.ForeColor = [System.Drawing.Color]::FromArgb(200, 215, 230)
$listView.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$listView.HeaderStyle = "Nonclickable"
$listView.BorderStyle = "None"

$listView.Columns.Add("Trạng thái", 80) | Out-Null
$listView.Columns.Add("Kiểm tra", 320) | Out-Null
$listView.Columns.Add("Kết quả", 320) | Out-Null
$form.Controls.Add($listView)

# ── Progress Bar ──
$progressBar = New-Object System.Windows.Forms.ProgressBar
$progressBar.Location = New-Object System.Drawing.Point(20, 378)
$progressBar.Size = New-Object System.Drawing.Size(730, 24)
$progressBar.Style = "Continuous"
$progressBar.BackColor = [System.Drawing.Color]::FromArgb(22, 34, 50)
$progressBar.ForeColor = [System.Drawing.Color]::FromArgb(79, 195, 247)
$form.Controls.Add($progressBar)

$lblProgress = New-Object System.Windows.Forms.Label
$lblProgress.Text = "Nhấn 'Bắt đầu' để quét môi trường máy tính..."
$lblProgress.Location = New-Object System.Drawing.Point(20, 408)
$lblProgress.Size = New-Object System.Drawing.Size(730, 20)
$lblProgress.Font = New-Object System.Drawing.Font("Segoe UI", 9)
$lblProgress.ForeColor = [System.Drawing.Color]::FromArgb(120, 160, 190)
$form.Controls.Add($lblProgress)

# ── Log TextBox ──
$logBox = New-Object System.Windows.Forms.TextBox
$logBox.Multiline = $true
$logBox.ScrollBars = "Vertical"
$logBox.ReadOnly = $true
$logBox.Location = New-Object System.Drawing.Point(20, 434)
$logBox.Size = New-Object System.Drawing.Size(730, 60)
$logBox.BackColor = [System.Drawing.Color]::FromArgb(14, 22, 34)
$logBox.ForeColor = [System.Drawing.Color]::FromArgb(100, 160, 200)
$logBox.Font = New-Object System.Drawing.Font("Consolas", 9)
$logBox.BorderStyle = "None"
$form.Controls.Add($logBox)

# ── Buttons ──
$btnStart = New-Object System.Windows.Forms.Button
$btnStart.Text = "▶  Bắt đầu"
$btnStart.Location = New-Object System.Drawing.Point(530, 502)
$btnStart.Size = New-Object System.Drawing.Size(220, 38)
$btnStart.BackColor = [System.Drawing.Color]::FromArgb(21, 101, 192)
$btnStart.ForeColor = [System.Drawing.Color]::White
$btnStart.FlatStyle = "Flat"
$btnStart.Font = New-Object System.Drawing.Font("Segoe UI", 11, [System.Drawing.FontStyle]::Bold)
$btnStart.FlatAppearance.BorderSize = 0
$btnStart.Cursor = "Hand"
$form.Controls.Add($btnStart)

$btnClose = New-Object System.Windows.Forms.Button
$btnClose.Text = "Đóng"
$btnClose.Location = New-Object System.Drawing.Point(20, 502)
$btnClose.Size = New-Object System.Drawing.Size(120, 38)
$btnClose.BackColor = [System.Drawing.Color]::FromArgb(40, 55, 75)
$btnClose.ForeColor = [System.Drawing.Color]::FromArgb(150, 170, 190)
$btnClose.FlatStyle = "Flat"
$btnClose.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$btnClose.FlatAppearance.BorderSize = 0
$btnClose.Cursor = "Hand"
$form.Controls.Add($btnClose)
$btnClose.Add_Click({ $form.Close() })

# ══════════════════════════════════════════════════
# HELPER FUNCTIONS
# ══════════════════════════════════════════════════
function Add-Check {
    param([string]$Name, [string]$Status, [string]$Detail)
    $item = New-Object System.Windows.Forms.ListViewItem($Status)
    $item.SubItems.Add($Name) | Out-Null
    $item.SubItems.Add($Detail) | Out-Null
    switch ($Status) {
        "✅" { $item.ForeColor = [System.Drawing.Color]::FromArgb(76, 175, 80) }
        "❌" { $item.ForeColor = [System.Drawing.Color]::FromArgb(244, 67, 54) }
        "⚠️" { $item.ForeColor = [System.Drawing.Color]::FromArgb(255, 152, 0) }
        "🔄" { $item.ForeColor = [System.Drawing.Color]::FromArgb(79, 195, 247) }
    }
    $listView.Items.Add($item) | Out-Null
    $listView.EnsureVisible($listView.Items.Count - 1)
    $form.Refresh()
    return $item
}

function Update-Check {
    param([System.Windows.Forms.ListViewItem]$Item, [string]$Status, [string]$Detail)
    $Item.Text = $Status
    $Item.SubItems[2].Text = $Detail
    switch ($Status) {
        "✅" { $Item.ForeColor = [System.Drawing.Color]::FromArgb(76, 175, 80) }
        "❌" { $Item.ForeColor = [System.Drawing.Color]::FromArgb(244, 67, 54) }
        "⚠️" { $Item.ForeColor = [System.Drawing.Color]::FromArgb(255, 152, 0) }
    }
    $form.Refresh()
}

function Write-Log {
    param([string]$Message)
    $logBox.AppendText("$Message`r`n")
    $logBox.SelectionStart = $logBox.TextLength
    $logBox.ScrollToCaret()
    $form.Refresh()
}

function Set-Progress {
    param([int]$Value, [string]$Text)
    $progressBar.Value = [Math]::Min($Value, 100)
    $lblProgress.Text = $Text
    $form.Refresh()
}

# ══════════════════════════════════════════════════
# SCAN + INSTALL LOGIC
# ══════════════════════════════════════════════════
$script:scanPassed = $false
$script:sqlNeeded = $false
$script:phase = 0  # 0=ready, 1=scanned, 2=installing, 3=done

$btnStart.Add_Click({
    switch ($script:phase) {
        0 { Start-Scan }
        1 { Start-Install }
        3 { Start-Process "http://localhost:5050"; $form.Close() }
    }
})

function Start-Scan {
    $script:phase = -1
    $btnStart.Enabled = $false
    $listView.Items.Clear()
    $lblStep.Text = "BƯỚC 1/3 — QUÉT MÔI TRƯỜNG"

    $criticalFail = $false

    # 1. Admin
    Set-Progress 10 "Kiểm tra quyền Administrator..."
    $item = Add-Check "🔄" "Quyền Administrator" "Đang kiểm tra..."
    $isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    if ($isAdmin) { Update-Check $item "✅" "Có quyền Admin" }
    else { Update-Check $item "❌" "KHÔNG CÓ QUYỀN — Chạy lại với Run as Administrator"; $criticalFail = $true }
    Start-Sleep -Milliseconds 200

    # 2. OS
    Set-Progress 20 "Kiểm tra hệ điều hành..."
    $item = Add-Check "🔄" "Hệ điều hành Windows" "Đang kiểm tra..."
    $os = Get-CimInstance Win32_OperatingSystem
    $osOk = $os.Caption -match "Windows" -and $os.OSArchitecture -match "64"
    if ($osOk) { Update-Check $item "✅" "$($os.Caption) ($($os.OSArchitecture))" }
    else { Update-Check $item "❌" "$($os.Caption) — Cần Windows 64-bit"; $criticalFail = $true }
    Start-Sleep -Milliseconds 200

    # 3. CPU
    Set-Progress 30 "Kiểm tra CPU..."
    $item = Add-Check "🔄" "CPU (yêu cầu ≥ 2 cores)" "Đang kiểm tra..."
    $cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
    $cores = $cpu.NumberOfCores
    if ($cores -ge 2) { Update-Check $item "✅" "$($cpu.Name) — $cores cores" }
    else { Update-Check $item "❌" "$cores cores — Cần ít nhất 2 cores"; $criticalFail = $true }
    Start-Sleep -Milliseconds 200

    # 4. RAM
    Set-Progress 40 "Kiểm tra RAM..."
    $item = Add-Check "🔄" "RAM (yêu cầu ≥ 2 GB)" "Đang kiểm tra..."
    $totalRam = [math]::Round($os.TotalVisibleMemorySize / 1MB, 1)
    $freeRam = [math]::Round($os.FreePhysicalMemory / 1MB, 1)
    if ($totalRam -ge 2) { Update-Check $item "✅" "${totalRam} GB tổng — ${freeRam} GB trống" }
    else { Update-Check $item "❌" "${totalRam} GB — Cần ít nhất 2 GB"; $criticalFail = $true }
    Start-Sleep -Milliseconds 200

    # 5. Disk
    Set-Progress 50 "Kiểm tra ổ đĩa..."
    $item = Add-Check "🔄" "Ổ đĩa C: (yêu cầu ≥ 10 GB trống)" "Đang kiểm tra..."
    $drive = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='C:'"
    $freeGB = [math]::Round($drive.FreeSpace / 1GB, 1)
    if ($freeGB -ge 10) { Update-Check $item "✅" "${freeGB} GB trống" }
    else { Update-Check $item "❌" "${freeGB} GB trống — Cần ít nhất 10 GB"; $criticalFail = $true }
    Start-Sleep -Milliseconds 200

    # 6. SQL Server
    Set-Progress 65 "Kiểm tra SQL Server..."
    $item = Add-Check "🔄" "SQL Server Express" "Đang kiểm tra..."
    $sqlService = Get-Service -Name "MSSQL`$$SQL_INSTANCE" -ErrorAction SilentlyContinue
    if ($sqlService) {
        Update-Check $item "✅" "Đã cài đặt — $($sqlService.Status)"
    } else {
        Update-Check $item "⚠️" "Chưa cài — Sẽ được cài ở bước 2"
        $script:sqlNeeded = $true
    }
    Start-Sleep -Milliseconds 200

    # 7. Port
    Set-Progress 75 "Kiểm tra cổng mạng..."
    $item = Add-Check "🔄" "Cổng 5050 (HTTP)" "Đang kiểm tra..."
    $portUsed = Get-NetTCPConnection -LocalPort 5050 -ErrorAction SilentlyContinue
    if ($portUsed) { Update-Check $item "⚠️" "Đang sử dụng — có thể là IRM cũ" }
    else { Update-Check $item "✅" "Đang trống — sẵn sàng" }
    Start-Sleep -Milliseconds 200

    # 8. Defender
    Set-Progress 85 "Kiểm tra Windows Defender..."
    $item = Add-Check "🔄" "Windows Defender" "Đang kiểm tra..."
    try {
        $def = Get-MpComputerStatus -ErrorAction SilentlyContinue
        if ($def.AntivirusEnabled) { Update-Check $item "✅" "Đang hoạt động" }
        else { Update-Check $item "⚠️" "Không hoạt động" }
    } catch { Update-Check $item "⚠️" "Không xác định" }
    Start-Sleep -Milliseconds 200

    # 9. Existing IRM
    Set-Progress 95 "Kiểm tra IRM cũ..."
    $item = Add-Check "🔄" "IRM đã cài trước" "Đang kiểm tra..."
    if (Test-Path $INSTALL_DIR) {
        Update-Check $item "⚠️" "Phát hiện bản cũ tại $INSTALL_DIR — Sẽ nâng cấp"
    } else {
        Update-Check $item "✅" "Cài đặt mới"
    }

    Set-Progress 100 "Quét hoàn tất!"

    # Summary
    if ($criticalFail) {
        $lblProgress.Text = "❌ MÁY TÍNH CHƯA ĐẠT YÊU CẦU — Vui lòng khắc phục các mục ❌ ở trên"
        $lblProgress.ForeColor = [System.Drawing.Color]::FromArgb(244, 67, 54)
        Write-Log "❌ Quét thất bại — Máy tính chưa đạt yêu cầu tối thiểu"
        $btnStart.Enabled = $true
        $btnStart.Text = "🔄  Quét lại"
        $script:phase = 0
    } else {
        $lblProgress.Text = "✅ MÁY TÍNH ĐẠT YÊU CẦU — Nhấn 'Cài đặt' để tiếp tục"
        $lblProgress.ForeColor = [System.Drawing.Color]::FromArgb(76, 175, 80)
        Write-Log "✅ Quét thành công — Máy tính đạt yêu cầu"
        $btnStart.Enabled = $true
        $btnStart.Text = "⬇  Cài đặt IRM"
        $btnStart.BackColor = [System.Drawing.Color]::FromArgb(46, 125, 50)
        $script:phase = 1
        $script:scanPassed = $true
    }
}

function Start-Install {
    $script:phase = -1
    $btnStart.Enabled = $false
    $listView.Items.Clear()
    $progressBar.Value = 0
    $lblStep.Text = "BƯỚC 2/3 — CÀI ĐẶT ỨNG DỤNG"
    $lblProgress.ForeColor = [System.Drawing.Color]::FromArgb(79, 195, 247)

    try {
        # ── SQL Server ──
        if ($script:sqlNeeded) {
            Set-Progress 5 "Kiểm tra bộ cài SQL Server..."
            $item = Add-Check "🔄" "Cài đặt SQL Server Express" "Đang tìm bộ cài..."
            Write-Log "> Tìm bộ cài SQL Server trong thư mục prerequisites\..."

            $sqlSetup = Get-ChildItem (Join-Path $ScriptDir "prerequisites") -Filter "*.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($sqlSetup) {
                Write-Log "> Tìm thấy: $($sqlSetup.FullName)"
                Write-Log "> Đang cài đặt SQL Server (5-10 phút, vui lòng chờ)..."
                Set-Progress 10 "Đang cài SQL Server Express (5-10 phút)..."

                $args = "/Q /ACTION=Install /FEATURES=SQLEngine /INSTANCENAME=$SQL_INSTANCE /SQLSVCSTARTUPTYPE=Automatic /SQLSYSADMINACCOUNTS=`"BUILTIN\Administrators`" /IACCEPTSQLSERVERLICENSETERMS /TCPENABLED=0 /NPENABLED=1"
                $proc = Start-Process -FilePath $sqlSetup.FullName -ArgumentList $args -Wait -PassThru -NoNewWindow
                if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010) {
                    Update-Check $item "✅" "SQL Server Express đã cài thành công!"
                    Write-Log "✅ SQL Server cài thành công"
                    # Wait for service
                    $retry = 0
                    while ($retry -lt 20) {
                        $svc = Get-Service "MSSQL`$$SQL_INSTANCE" -ErrorAction SilentlyContinue
                        if ($svc -and $svc.Status -eq "Running") { break }
                        Start-Sleep -Seconds 2; $retry++
                    }
                } else {
                    Update-Check $item "⚠️" "Lỗi (code: $($proc.ExitCode)) — IRM sẽ hướng dẫn khi khởi động"
                    Write-Log "⚠️ SQL Server exit code: $($proc.ExitCode)"
                }
            } else {
                Update-Check $item "⚠️" "Không tìm thấy bộ cài — Đặt vào prerequisites\"
                Write-Log "⚠️ Bộ cài SQL Server không có sẵn"
            }
        } else {
            $item = Add-Check "✅" "SQL Server Express" "Đã có sẵn"
            # Ensure running
            $svc = Get-Service "MSSQL`$$SQL_INSTANCE" -ErrorAction SilentlyContinue
            if ($svc -and $svc.Status -ne "Running") {
                Start-Service "MSSQL`$$SQL_INSTANCE" -ErrorAction SilentlyContinue
                Start-Sleep -Seconds 3
            }
        }

        # ── Tạo thư mục ──
        Set-Progress 25 "Tạo thư mục cài đặt..."
        $item = Add-Check "🔄" "Tạo thư mục C:\IRM" "Đang tạo..."
        @($INSTALL_DIR, "$INSTALL_DIR\data", "$INSTALL_DIR\data\private-files", "$INSTALL_DIR\data\dataprotection-keys", "$INSTALL_DIR\data\backups") | ForEach-Object {
            if (-not (Test-Path $_)) { New-Item -ItemType Directory -Path $_ -Force | Out-Null }
        }
        Update-Check $item "✅" "Đã tạo C:\IRM\"
        Write-Log "✅ Thư mục C:\IRM\ đã tạo"

        # ── Dừng service cũ ──
        Set-Progress 30 "Kiểm tra service cũ..."
        $existingSvc = Get-Service -Name "IRM" -ErrorAction SilentlyContinue
        if ($existingSvc) {
            Stop-Service "IRM" -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 2
            Write-Log "> Đã dừng service IRM cũ"
        }

        # ── Copy ứng dụng ──
        Set-Progress 35 "Sao chép ứng dụng (vui lòng chờ)..."
        $item = Add-Check "🔄" "Sao chép ứng dụng" "Đang sao chép ~180 MB..."
        Write-Log "> Đang sao chép ứng dụng vào C:\IRM\..."
        $appSource = Join-Path $ScriptDir "app"
        if (Test-Path $appSource) {
            Copy-Item "$appSource\*" "$INSTALL_DIR\" -Recurse -Force
            Update-Check $item "✅" "Đã sao chép thành công"
            Write-Log "✅ Ứng dụng đã sao chép"
        } else {
            Update-Check $item "❌" "Không tìm thấy thư mục app\"
            Write-Log "❌ Thư mục app\ không tồn tại!"
            throw "Thư mục app\ không tồn tại"
        }

        # ── Cấu hình ──
        Set-Progress 55 "Cấu hình kết nối database..."
        $item = Add-Check "🔄" "Cấu hình kết nối database" "Đang cấu hình..."
        $connStr = "Server=.\$SQL_INSTANCE;Database=$SQL_DB_NAME;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
        $settingsPath = Join-Path $INSTALL_DIR "appsettings.json"
        if (Test-Path $settingsPath) {
            $json = Get-Content $settingsPath -Raw | ConvertFrom-Json
            if (-not $json.ConnectionStrings) {
                $json | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{} -Force
            }
            $json.ConnectionStrings | Add-Member -Type NoteProperty -Name "DefaultConnection" -Value $connStr -Force
            $json.ConnectionStrings | Add-Member -Type NoteProperty -Name "SqlServer" -Value $connStr -Force
            $json | ConvertTo-Json -Depth 10 | Set-Content $settingsPath -Encoding UTF8
        }
        Update-Check $item "✅" ".\$SQL_INSTANCE → $SQL_DB_NAME"
        Write-Log "✅ Kết nối: .\$SQL_INSTANCE → $SQL_DB_NAME"

        # ── Tạo database ──
        Set-Progress 65 "Tạo database..."
        $item = Add-Check "🔄" "Tạo database $SQL_DB_NAME" "Đang tạo..."
        try {
            $sqlCmd = "IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = '$SQL_DB_NAME') CREATE DATABASE [$SQL_DB_NAME];"
            sqlcmd -S ".\$SQL_INSTANCE" -E -Q $sqlCmd -b 2>$null
            Update-Check $item "✅" "Database đã sẵn sàng"
            Write-Log "✅ Database $SQL_DB_NAME đã tạo"
        } catch {
            Update-Check $item "⚠️" "IRM sẽ tự tạo khi khởi động"
            Write-Log "⚠️ Không tạo DB tự động — IRM sẽ tự tạo"
        }

        # ── Chạy Migration ──
        $migrationFile = Join-Path $ScriptDir "migration.sql"
        if (Test-Path $migrationFile) {
            Set-Progress 70 "Khởi tạo bảng dữ liệu..."
            $item = Add-Check "🔄" "Bảng dữ liệu hệ thống" "Đang khởi tạo..."
            try {
                sqlcmd -S ".\$SQL_INSTANCE" -d $SQL_DB_NAME -E -i $migrationFile -b 2>$null
                Update-Check $item "✅" "Bảng dữ liệu đã sẵn sàng"
                Write-Log "✅ Khởi tạo cấu trúc bảng hoàn tất"
            } catch {
                Update-Check $item "⚠️" "IRM sẽ cập nhật khi khởi động"
                Write-Log "⚠️ Migration script có cảnh báo"
            }
        }

        # ── Đăng ký Service ──
        Set-Progress 75 "Đăng ký Windows Service..."
        $item = Add-Check "🔄" "Đăng ký Windows Service" "Đang đăng ký..."
        $irmExe = Join-Path $INSTALL_DIR "IRM.exe"
        if (-not $existingSvc) {
            New-Service -Name "IRM" -BinaryPathName "`"$irmExe`"" -DisplayName "IRM - Quản lý NNN" -Description "Hệ thống quản lý người nước ngoài v$IRM_VERSION" -StartupType Automatic | Out-Null
        }
        $regPath = "HKLM:\SYSTEM\CurrentControlSet\Services\IRM"
        Set-ItemProperty -Path $regPath -Name "Environment" -Value @("ASPNETCORE_ENVIRONMENT=Production", "ASPNETCORE_URLS=http://+:5050;https://+:5443", "FileStorage__Root=$INSTALL_DIR\data\private-files", "DataProtection__KeysPath=$INSTALL_DIR\data\dataprotection-keys") -Type MultiString
        Update-Check $item "✅" "Service 'IRM' đã đăng ký — Tự khởi động"
        Write-Log "✅ Windows Service 'IRM' đã đăng ký"

        # ── Firewall ──
        Set-Progress 82 "Cấu hình Firewall..."
        $item = Add-Check "🔄" "Cấu hình Firewall" "Đang mở cổng..."
        if (-not (Get-NetFirewallRule -DisplayName "IRM HTTP" -ErrorAction SilentlyContinue)) {
            New-NetFirewallRule -DisplayName "IRM HTTP" -Direction Inbound -Protocol TCP -LocalPort 5050 -Action Allow | Out-Null
        }
        if (-not (Get-NetFirewallRule -DisplayName "IRM HTTPS" -ErrorAction SilentlyContinue)) {
            New-NetFirewallRule -DisplayName "IRM HTTPS" -Direction Inbound -Protocol TCP -LocalPort 5443 -Action Allow | Out-Null
        }
        Update-Check $item "✅" "Đã mở cổng 5050 (HTTP) và 5443 (HTTPS)"
        Write-Log "✅ Firewall đã cấu hình"

        # ── Khởi động ──
        Set-Progress 90 "Khởi động IRM..."
        $item = Add-Check "🔄" "Khởi động IRM" "Đang khởi động..."
        Write-Log "> Đang khởi động IRM..."
        Start-Service "IRM" -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 5

        $svc = Get-Service "IRM" -ErrorAction SilentlyContinue
        if ($svc -and $svc.Status -eq "Running") {
            Update-Check $item "✅" "IRM đang chạy!"
            Write-Log "✅ IRM đang chạy!"
        } else {
            Update-Check $item "⚠️" "Service chưa phản hồi — Kiểm tra log tại C:\IRM\"
            Write-Log "⚠️ Service chưa phản hồi ngay"
        }

        # ── Shortcut ──
        Set-Progress 95 "Tạo shortcut Desktop..."
        try {
            $desktopPath = [Environment]::GetFolderPath("CommonDesktopDirectory")
            $shell = New-Object -ComObject WScript.Shell
            $shortcut = $shell.CreateShortcut("$desktopPath\IRM - Quản lý NNN.lnk")
            $shortcut.TargetPath = "http://localhost:5050"
            $shortcut.Description = "Hệ thống quản lý NNN v$IRM_VERSION"
            $shortcut.Save()
            Write-Log "✅ Shortcut đã tạo trên Desktop"
        } catch {
            Write-Log "⚠️ Không tạo được shortcut"
        }

        # ── HOÀN TẤT ──
        Set-Progress 100 "✅ CÀI ĐẶT HOÀN TẤT!"
        $lblStep.Text = "BƯỚC 3/3 — HOÀN TẤT"
        $lblStep.ForeColor = [System.Drawing.Color]::FromArgb(76, 175, 80)
        $lblProgress.Text = "✅ CÀI ĐẶT HOÀN TẤT!   Truy cập: http://localhost:5050   |   LAN: https://$($env:COMPUTERNAME):5443"
        $lblProgress.ForeColor = [System.Drawing.Color]::FromArgb(76, 175, 80)

        Write-Log ""
        Write-Log "════════════════════════════════════════════"
        Write-Log "  🎉 CÀI ĐẶT HOÀN TẤT!"
        Write-Log "  Truy cập: http://localhost:5050"
        Write-Log "  LAN:      https://$($env:COMPUTERNAME):5443"
        Write-Log "════════════════════════════════════════════"

        $btnStart.Text = "🌐  Mở IRM trong trình duyệt"
        $btnStart.BackColor = [System.Drawing.Color]::FromArgb(21, 101, 192)
        $btnStart.Enabled = $true
        $script:phase = 3

        [System.Windows.Forms.MessageBox]::Show(
            "Cài đặt IRM v$IRM_VERSION hoàn tất!`n`nTruy cập: http://localhost:5050`nTruy cập LAN: https://$($env:COMPUTERNAME):5443`n`nShortcut đã được tạo trên Desktop.",
            "Cài đặt thành công",
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Information
        )

    } catch {
        Set-Progress 0 "❌ LỖI: $($_.Exception.Message)"
        $lblProgress.ForeColor = [System.Drawing.Color]::FromArgb(244, 67, 54)
        Write-Log "❌ LỖI: $($_.Exception.Message)"
        $btnStart.Text = "🔄  Thử lại"
        $btnStart.BackColor = [System.Drawing.Color]::FromArgb(21, 101, 192)
        $btnStart.Enabled = $true
        $script:phase = 1

        [System.Windows.Forms.MessageBox]::Show(
            "Đã xảy ra lỗi: $($_.Exception.Message)`n`nVui lòng chụp ảnh màn hình và liên hệ hỗ trợ kỹ thuật.",
            "Lỗi cài đặt",
            [System.Windows.Forms.MessageBoxButtons]::OK,
            [System.Windows.Forms.MessageBoxIcon]::Error
        )
    }
}

# ── Show Form ──
[void]$form.ShowDialog()
