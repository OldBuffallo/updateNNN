[CmdletBinding()]
param([Parameter(Mandatory)][string]$PayloadRoot)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
[System.Windows.Forms.Application]::EnableVisualStyles()

$script:phase = 'scan'
$script:scanPassed = $false
$script:reportPath = $null
$script:scanRows = @()
$script:worker = $null
$script:generatedDbaPassword = $null
$script:exitCode = 1
$requiredSqlVersion = [version]'16.0.4295.3'

function Test-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($identity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function New-RandomSecret {
    $bytes = New-Object byte[] 32
    [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    return [Convert]::ToBase64String($bytes)
}

function Add-ScanRow {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Detail,
        [bool]$Required = $true,
        [bool]$WillInstall = $false
    )
    $status = if ($Passed) { 'Đạt' } elseif ($WillInstall) { 'Sẽ cài bổ sung' } elseif ($Required) { 'Không đạt' } else { 'Cảnh báo' }
    $item = New-Object Windows.Forms.ListViewItem($Name)
    $item.SubItems.Add($status) | Out-Null
    $item.SubItems.Add($Detail) | Out-Null
    $item.Checked = $Passed
    if ($Passed) { $item.ForeColor = [Drawing.Color]::FromArgb(35,125,70) }
    elseif ($WillInstall) { $item.ForeColor = [Drawing.Color]::FromArgb(180,105,0) }
    elseif ($Required) { $item.ForeColor = [Drawing.Color]::FromArgb(190,45,45) }
    else { $item.ForeColor = [Drawing.Color]::FromArgb(180,105,0) }
    $list.Items.Add($item) | Out-Null
    $script:scanRows += [pscustomobject]@{ Name=$Name; Passed=$Passed; Detail=$Detail; Required=$Required; WillInstall=$WillInstall; Status=$status }
}

function Save-EnvironmentReport {
    $reportDir = Join-Path $env:ProgramData 'IRM\logs'
    New-Item -ItemType Directory -Path $reportDir -Force | Out-Null
    $script:reportPath = Join-Path $reportDir ("Bao-cao-moi-truong-{0}.html" -f (Get-Date -Format 'yyyyMMdd-HHmmss'))
    $rows = foreach ($row in $script:scanRows) {
        $mark = if ($row.Passed) { '&#9745;' } else { '&#9744;' }
        $color = if ($row.Passed) { '#237d46' } elseif ($row.WillInstall) { '#b46900' } else { '#be2d2d' }
        '<tr><td style="font-size:22px">{0}</td><td>{1}</td><td style="color:{2};font-weight:600">{3}</td><td>{4}</td></tr>' -f $mark,[Net.WebUtility]::HtmlEncode($row.Name),$color,[Net.WebUtility]::HtmlEncode($row.Status),[Net.WebUtility]::HtmlEncode($row.Detail)
    }
    $html = @"
<!doctype html><html lang="vi"><head><meta charset="utf-8"><title>Báo cáo môi trường IRM</title>
<style>body{font:15px Segoe UI,Arial;margin:32px;color:#243447}h1{color:#1565c0}table{border-collapse:collapse;width:100%}th,td{border:1px solid #d8e0e8;padding:10px;text-align:left}th{background:#eef4fa}.meta{color:#607080}</style></head>
<body><h1>Báo cáo môi trường cài đặt IRM v1.0.2</h1><p class="meta">Máy: $([Net.WebUtility]::HtmlEncode($env:COMPUTERNAME)) — Thời gian: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')</p>
<table><thead><tr><th>Tick</th><th>Hạng mục</th><th>Trạng thái</th><th>Chi tiết</th></tr></thead><tbody>$($rows -join "`r`n")</tbody></table></body></html>
"@
    [IO.File]::WriteAllText($script:reportPath, $html, (New-Object Text.UTF8Encoding($true)))
}

function Invoke-EnvironmentScan {
    $list.Items.Clear()
    $script:scanRows = @()
    $script:scanPassed = $false
    $progress.Style = 'Continuous'
    $progress.Value = 10
    $labelStep.Text = 'BƯỚC 1/3 — QUÉT VÀ LẬP BÁO CÁO MÔI TRƯỜNG'
    $labelStatus.Text = 'Đang kiểm tra máy tính...'
    $form.Refresh()

    $admin = Test-Administrator
    Add-ScanRow 'Quyền Administrator' $admin $(if($admin){'Đã được cấp quyền quản trị'}else{'Hãy chạy lại bộ cài và chọn Yes khi Windows hỏi quyền'})

    $os = Get-CimInstance Win32_OperatingSystem
    $osOk = [Environment]::Is64BitOperatingSystem -and (($os.ProductType -eq 1 -and [Environment]::OSVersion.Version.Build -ge 19044) -or ($os.ProductType -ne 1 -and [Environment]::OSVersion.Version.Build -ge 17763))
    Add-ScanRow 'Hệ điều hành Windows 64-bit' $osOk "$($os.Caption), build $([Environment]::OSVersion.Version.Build)"

    $cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
    $cpuOk = $cpu.NumberOfLogicalProcessors -ge 2 -and $cpu.MaxClockSpeed -ge 2000
    Add-ScanRow 'CPU tối thiểu 2 lõi, 2 GHz' $cpuOk "$($cpu.Name) — $($cpu.NumberOfLogicalProcessors) luồng, $($cpu.MaxClockSpeed) MHz"

    $freeRamGb = [math]::Round(([long]$os.FreePhysicalMemory * 1KB) / 1GB, 1)
    Add-ScanRow 'RAM khả dụng tối thiểu 2 GB' ($freeRamGb -ge 2) "$freeRamGb GB khả dụng"

    $driveName = ([IO.Path]::GetPathRoot($env:ProgramData)).TrimEnd(':\')
    $drive = Get-PSDrive -Name $driveName
    $freeDiskGb = [math]::Round($drive.Free / 1GB, 1)
    Add-ScanRow 'Ổ đĩa trống tối thiểu 10 GB' ($freeDiskGb -ge 10) "$freeDiskGb GB còn trống trên ổ $driveName`:"

    $sqlService = Get-Service 'MSSQL$IRMEXPRESS' -ErrorAction SilentlyContinue
    $sqlVersion = $null
    if ($sqlService) {
        try {
            $instanceId = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL').IRMEXPRESS
            $sqlVersion = [version](Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$instanceId\Setup").Version
        } catch { $sqlVersion = [version]'0.0' }
    }
    $baseMedia = Test-Path -LiteralPath (Join-Path $PayloadRoot 'sql-base.exe') -PathType Leaf
    $cuMedia = Test-Path -LiteralPath (Join-Path $PayloadRoot 'sql-cu.exe') -PathType Leaf
    if (-not $sqlService) {
        Add-ScanRow 'SQL Server 2022 Express' $false $(if($baseMedia){'Chưa có — bộ cài sẽ tự cài từ gói offline'}else{'Thiếu bộ cài SQL Server trong gói'}) $true $baseMedia
    } else {
        Add-ScanRow 'SQL Server 2022 Express' $true "Đã cài — $($sqlService.Status)"
    }
    if ($sqlVersion -and $sqlVersion -ge $requiredSqlVersion) {
        Add-ScanRow 'SQL Server CU27' $true "Phiên bản $sqlVersion"
    } else {
        Add-ScanRow 'SQL Server CU27' $false $(if($cuMedia){'Chưa có — bộ cài sẽ tự cập nhật offline'}else{'Thiếu gói cập nhật CU27 trong bộ cài'}) $true $cuMedia
    }

    $appOk = Test-Path -LiteralPath (Join-Path $PayloadRoot 'app\IRM.exe') -PathType Leaf
    Add-ScanRow 'Ứng dụng IRM và .NET Runtime' $appOk $(if($appOk){'Đã đóng gói self-contained, không cần cài .NET'}else{'Thiếu app\IRM.exe'})

    $progress.Value = 80
    $labelStatus.Text = 'Đang kiểm tra checksum toàn bộ bộ cài offline...'
    $form.Refresh()
    $checksumOk = $true
    $checksumDetail = 'Toàn bộ tệp trong bộ cài có checksum hợp lệ'
    try {
        $checksumFile = Join-Path $PayloadRoot 'SHA256SUMS.txt'
        if (-not (Test-Path -LiteralPath $checksumFile -PathType Leaf)) { throw 'Thiếu SHA256SUMS.txt' }
        foreach ($line in Get-Content -LiteralPath $checksumFile) {
            if ([string]::IsNullOrWhiteSpace($line)) { continue }
            if ($line -notmatch '^([A-Fa-f0-9]{64})\s{2}(.+)$') { throw 'Danh sách checksum không hợp lệ' }
            $file = Join-Path $PayloadRoot $matches[2]
            if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw "Thiếu $($matches[2])" }
            if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $matches[1].ToUpperInvariant()) { throw "Sai checksum $($matches[2])" }
        }
    } catch { $checksumOk = $false; $checksumDetail = $_.Exception.Message }
    Add-ScanRow 'Tính toàn vẹn bộ cài offline' $checksumOk $checksumDetail

    $existingIrm = Get-Service IRM -ErrorAction SilentlyContinue
    foreach ($port in 14331,5050,5443) {
        $listener = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue
        $allowedExisting = ($port -eq 14331 -and $sqlService) -or ($port -in 5050,5443 -and $existingIrm)
        Add-ScanRow "Cổng mạng $port" (-not $listener -or $allowedExisting) $(if(-not $listener){'Đang trống'}elseif($allowedExisting){'Đang do bản IRM hiện tại sử dụng — sẽ nâng cấp'}else{'Đang bị ứng dụng khác sử dụng'})
    }

    $defender = $null
    try { $defender = Get-MpComputerStatus -ErrorAction Stop } catch { }
    Add-ScanRow 'Windows Defender' ($defender -and $defender.AntivirusEnabled) $(if($defender -and $defender.AntivirusEnabled){'Đang hoạt động và sẵn sàng quét tệp tải lên'}else{'Chưa bật; hãy bật Windows Defender rồi quét lại'})

    Save-EnvironmentReport
    $hardFailures = @($script:scanRows | Where-Object { $_.Required -and -not $_.Passed -and -not $_.WillInstall })
    $script:scanPassed = $hardFailures.Count -eq 0
    $progress.Value = 100
    if ($script:scanPassed) {
        $labelStatus.Text = 'Quét hoàn tất. Các thành phần còn thiếu sẽ được cài từ gói offline.'
        $buttonMain.Text = 'Tiếp tục cài đặt'
        $buttonMain.Enabled = $true
        $buttonReport.Enabled = $true
        $script:phase = 'configure'
    } else {
        $labelStatus.Text = 'Máy chưa đạt yêu cầu. Hãy xử lý các mục “Không đạt” rồi quét lại.'
        $buttonMain.Text = 'Quét lại'
        $buttonMain.Enabled = $true
        $buttonReport.Enabled = $true
        $script:phase = 'scan'
    }
}

function Show-ConfigurationStep {
    $labelStep.Text = 'BƯỚC 2/3 — XÁC NHẬN VÀ CÀI THÀNH PHẦN CÒN THIẾU'
    $list.Visible = $false
    $panelConfig.Visible = $true
    $progress.Value = 0
    $labelStatus.Text = 'Nhập mật khẩu quản trị, sau đó nhấn Cài đặt.'
    $buttonMain.Text = 'Cài đặt IRM'
    $script:phase = 'install'
}

function Start-IrmInstallation {
    if ($textPassword.Text.Length -lt 12) {
        [Windows.Forms.MessageBox]::Show('Mật khẩu quản trị phải có ít nhất 12 ký tự.','Mật khẩu chưa đạt','OK','Warning') | Out-Null
        return
    }
    if ($textPassword.Text -ne $textPasswordConfirm.Text) {
        [Windows.Forms.MessageBox]::Show('Hai lần nhập mật khẩu không giống nhau.','Kiểm tra mật khẩu','OK','Warning') | Out-Null
        return
    }
    if (-not $checkLicense.Checked) {
        [Windows.Forms.MessageBox]::Show('Cần chấp nhận điều khoản Microsoft SQL Server Express để tiếp tục.','Điều khoản sử dụng','OK','Warning') | Out-Null
        return
    }

    $script:generatedDbaPassword = New-RandomSecret
    $adminPassword = $textPassword.Text
    $adminUser = if([string]::IsNullOrWhiteSpace($textAdmin.Text)){'admin'}else{$textAdmin.Text.Trim()}
    $installLog = Join-Path (Join-Path $env:ProgramData 'IRM\logs') ("install-{0}.log" -f (Get-Date -Format 'yyyyMMdd-HHmmss'))
    New-Item -ItemType Directory -Path (Split-Path $installLog) -Force | Out-Null
    $panelConfig.Enabled = $false
    $buttonMain.Enabled = $false
    $buttonReport.Enabled = $false
    $buttonClose.Enabled = $false
    $progress.Style = 'Marquee'
    $progress.MarqueeAnimationSpeed = 25
    $labelStatus.Text = 'Đang cài SQL Server, database và IRM. Quá trình có thể mất 10–20 phút...'
    $form.Refresh()

    $worker = New-Object ComponentModel.BackgroundWorker
    $script:worker = $worker
    $worker.add_DoWork({
        param($sender,$eventArgs)
        $config = $eventArgs.Argument
        $psi = New-Object Diagnostics.ProcessStartInfo
        $psi.FileName = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
        $lanSwitch = if($config.EnableLan){' -EnableLan'}else{''}
        $psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -File `"$($config.PayloadRoot)\install.ps1`" -PayloadRoot `"$($config.PayloadRoot)`" -AdminUser `"$($config.AdminUser)`" -AcceptSqlLicense -NonInteractive$lanSwitch"
        $psi.UseShellExecute = $false
        $psi.CreateNoWindow = $true
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $psi.EnvironmentVariables['IRM_INSTALL_ADMIN_PASSWORD'] = $config.AdminPassword
        $psi.EnvironmentVariables['IRM_INSTALL_DBA_PASSWORD'] = $config.DbaPassword
        $process = [Diagnostics.Process]::Start($psi)
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $logText = $stdoutTask.Result + "`r`n" + $stderrTask.Result
        [IO.File]::WriteAllText($config.InstallLog,$logText,(New-Object Text.UTF8Encoding($true)))
        $eventArgs.Result = $process.ExitCode
    })
    $worker.add_RunWorkerCompleted({
        param($sender,$eventArgs)
        $progress.Style = 'Continuous'
        $progress.Value = 100
        $buttonClose.Enabled = $true
        if ($eventArgs.Error -or $eventArgs.Result -ne 0) {
            $labelStatus.Text = 'Cài đặt chưa hoàn tất. Xem log và gửi cho bộ phận hỗ trợ.'
            $buttonMain.Text = 'Thử lại'
            $buttonMain.Enabled = $true
            $panelConfig.Enabled = $true
            $script:phase = 'install'
            [Windows.Forms.MessageBox]::Show('Cài đặt thất bại. Vui lòng chụp màn hình và gửi bộ phận hỗ trợ.','IRM','OK','Error') | Out-Null
            return
        }
        $recoveryFile = Join-Path $env:ProgramData 'IRM\MA-KHOI-PHUC.txt'
        $recoveryText = "IRM v1.0.2`r`nMa khoi phuc database (irm_dba): $script:generatedDbaPassword`r`nHay luu tep nay o noi an toan."
        [IO.File]::WriteAllText($recoveryFile,$recoveryText,(New-Object Text.UTF8Encoding($true)))
        & icacls.exe $recoveryFile /inheritance:r /grant:r 'SYSTEM:F' 'Administrators:F' | Out-Null
        $panelConfig.Visible = $false
        $list.Visible = $true
        $list.Items.Clear()
        Add-ScanRow 'Cài thành phần thiếu' $true 'Hoàn tất'
        Add-ScanRow 'Khởi tạo database' $true 'Migration và tài khoản quản trị đã tạo'
        Add-ScanRow 'Windows Service IRM' $true 'Đang chạy và tự khởi động cùng Windows'
        Add-ScanRow 'Health check ứng dụng' $true 'Ứng dụng và database phản hồi bình thường'
        $labelStep.Text = 'BƯỚC 3/3 — HOÀN TẤT'
        $labelStatus.Text = 'Cài đặt thành công. IRM đã sẵn sàng sử dụng.'
        $script:exitCode = 0
        $buttonMain.Text = 'Mở IRM'
        $buttonMain.Enabled = $true
        $script:phase = 'done'
    })
    $worker.RunWorkerAsync([pscustomobject]@{PayloadRoot=$PayloadRoot;AdminUser=$adminUser;AdminPassword=$adminPassword;DbaPassword=$script:generatedDbaPassword;EnableLan=$checkLan.Checked;InstallLog=$installLog})
}

$form = New-Object Windows.Forms.Form
$form.Text = 'IRM v1.0.2 — Bộ cài đặt offline'
$form.StartPosition = 'CenterScreen'
$form.Size = New-Object Drawing.Size(920,650)
$form.MinimumSize = New-Object Drawing.Size(920,650)
$form.Font = New-Object Drawing.Font('Segoe UI',10)
$form.BackColor = [Drawing.Color]::White

$header = New-Object Windows.Forms.Panel
$header.Dock = 'Top'; $header.Height = 82; $header.BackColor = [Drawing.Color]::FromArgb(20,63,103)
$form.Controls.Add($header)
$title = New-Object Windows.Forms.Label
$title.Text = 'IRM — HỆ THỐNG QUẢN LÝ NGƯỜI NƯỚC NGOÀI'; $title.ForeColor = [Drawing.Color]::White
$title.Font = New-Object Drawing.Font('Segoe UI Semibold',17); $title.AutoSize = $true; $title.Location = New-Object Drawing.Point(24,16)
$header.Controls.Add($title)
$subtitle = New-Object Windows.Forms.Label
$subtitle.Text = 'Bộ cài đặt offline v1.0.2 • Không cần kết nối Internet'; $subtitle.ForeColor = [Drawing.Color]::FromArgb(205,225,242)
$subtitle.AutoSize = $true; $subtitle.Location = New-Object Drawing.Point(27,51); $header.Controls.Add($subtitle)

$labelStep = New-Object Windows.Forms.Label
$labelStep.Text = 'BƯỚC 1/3 — QUÉT VÀ LẬP BÁO CÁO MÔI TRƯỜNG'; $labelStep.Font = New-Object Drawing.Font('Segoe UI Semibold',11)
$labelStep.ForeColor = [Drawing.Color]::FromArgb(20,93,160); $labelStep.Location = New-Object Drawing.Point(24,98); $labelStep.Size = New-Object Drawing.Size(850,28)
$form.Controls.Add($labelStep)

$list = New-Object Windows.Forms.ListView
$list.Location = New-Object Drawing.Point(24,132); $list.Size = New-Object Drawing.Size(854,350); $list.View = 'Details'
$list.FullRowSelect = $true; $list.GridLines = $true; $list.CheckBoxes = $true; $list.HeaderStyle = 'Nonclickable'
$list.Columns.Add('Hạng mục kiểm tra',285) | Out-Null; $list.Columns.Add('Trạng thái',140) | Out-Null; $list.Columns.Add('Chi tiết',405) | Out-Null
$form.Controls.Add($list)

$panelConfig = New-Object Windows.Forms.Panel
$panelConfig.Location = $list.Location; $panelConfig.Size = $list.Size; $panelConfig.Visible = $false; $panelConfig.BorderStyle = 'FixedSingle'
$form.Controls.Add($panelConfig)
$configTitle = New-Object Windows.Forms.Label
$configTitle.Text = 'Thông tin đăng nhập lần đầu'; $configTitle.Font = New-Object Drawing.Font('Segoe UI Semibold',14); $configTitle.AutoSize=$true; $configTitle.Location=New-Object Drawing.Point(28,24); $panelConfig.Controls.Add($configTitle)
$labelAdmin = New-Object Windows.Forms.Label; $labelAdmin.Text='Tên tài khoản quản trị';$labelAdmin.Location=New-Object Drawing.Point(32,78);$labelAdmin.AutoSize=$true;$panelConfig.Controls.Add($labelAdmin)
$textAdmin = New-Object Windows.Forms.TextBox; $textAdmin.Text='admin';$textAdmin.Location=New-Object Drawing.Point(260,74);$textAdmin.Width=310;$panelConfig.Controls.Add($textAdmin)
$labelPassword = New-Object Windows.Forms.Label; $labelPassword.Text='Mật khẩu (ít nhất 12 ký tự)';$labelPassword.Location=New-Object Drawing.Point(32,124);$labelPassword.AutoSize=$true;$panelConfig.Controls.Add($labelPassword)
$textPassword = New-Object Windows.Forms.TextBox; $textPassword.UseSystemPasswordChar=$true;$textPassword.Location=New-Object Drawing.Point(260,120);$textPassword.Width=310;$panelConfig.Controls.Add($textPassword)
$labelPassword2 = New-Object Windows.Forms.Label; $labelPassword2.Text='Nhập lại mật khẩu';$labelPassword2.Location=New-Object Drawing.Point(32,170);$labelPassword2.AutoSize=$true;$panelConfig.Controls.Add($labelPassword2)
$textPasswordConfirm = New-Object Windows.Forms.TextBox; $textPasswordConfirm.UseSystemPasswordChar=$true;$textPasswordConfirm.Location=New-Object Drawing.Point(260,166);$textPasswordConfirm.Width=310;$panelConfig.Controls.Add($textPasswordConfirm)
$checkLan = New-Object Windows.Forms.CheckBox; $checkLan.Text='Cho phép các máy khác trong mạng LAN truy cập IRM qua HTTPS';$checkLan.Checked=$true;$checkLan.AutoSize=$true;$checkLan.Location=New-Object Drawing.Point(32,220);$panelConfig.Controls.Add($checkLan)
$checkLicense = New-Object Windows.Forms.CheckBox; $checkLicense.Text='Tôi đồng ý điều khoản sử dụng Microsoft SQL Server Express';$checkLicense.AutoSize=$true;$checkLicense.Location=New-Object Drawing.Point(32,258);$panelConfig.Controls.Add($checkLicense)
$note = New-Object Windows.Forms.Label; $note.Text='Bộ cài sẽ tự động cài SQL Server nếu máy chưa có, tạo database, đăng ký dịch vụ và kiểm tra hoạt động trước khi hoàn tất.';$note.ForeColor=[Drawing.Color]::FromArgb(80,95,110);$note.Location=New-Object Drawing.Point(32,300);$note.Size=New-Object Drawing.Size(770,42);$panelConfig.Controls.Add($note)

$progress = New-Object Windows.Forms.ProgressBar
$progress.Location = New-Object Drawing.Point(24,495); $progress.Size = New-Object Drawing.Size(854,20); $form.Controls.Add($progress)
$labelStatus = New-Object Windows.Forms.Label
$labelStatus.Text = 'Nhấn “Quét môi trường” để bắt đầu.'; $labelStatus.Location = New-Object Drawing.Point(24,522); $labelStatus.Size = New-Object Drawing.Size(850,25); $form.Controls.Add($labelStatus)

$buttonClose = New-Object Windows.Forms.Button
$buttonClose.Text='Đóng';$buttonClose.Location=New-Object Drawing.Point(24,558);$buttonClose.Size=New-Object Drawing.Size(110,38);$buttonClose.Add_Click({$form.Close()});$form.Controls.Add($buttonClose)
$buttonReport = New-Object Windows.Forms.Button
$buttonReport.Text='Mở báo cáo';$buttonReport.Enabled=$false;$buttonReport.Location=New-Object Drawing.Point(145,558);$buttonReport.Size=New-Object Drawing.Size(135,38);$buttonReport.Add_Click({if($script:reportPath){Start-Process $script:reportPath}});$form.Controls.Add($buttonReport)
$buttonMain = New-Object Windows.Forms.Button
$buttonMain.Text='Quét môi trường';$buttonMain.Location=New-Object Drawing.Point(678,558);$buttonMain.Size=New-Object Drawing.Size(200,38);$buttonMain.BackColor=[Drawing.Color]::FromArgb(21,101,192);$buttonMain.ForeColor=[Drawing.Color]::White;$buttonMain.FlatStyle='Flat';$buttonMain.Add_Click({switch($script:phase){'scan'{Invoke-EnvironmentScan}'configure'{Show-ConfigurationStep}'install'{Start-IrmInstallation}'done'{Start-Process 'http://localhost:5050';$form.Close()}}});$form.Controls.Add($buttonMain)

$form.Add_FormClosing({param($sender,$e) if($script:worker -and $script:worker.IsBusy){$e.Cancel=$true}})
$form.Add_Shown({ Invoke-EnvironmentScan })
[void]$form.ShowDialog()
exit $script:exitCode
