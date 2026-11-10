[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$PayloadRoot,
    [string]$AdminUser,
    [string]$AdminPassword,
    [string]$DbaPassword,
    [switch]$AcceptSqlLicense,
    [switch]$EnableLan,
    [switch]$NonInteractive
)
$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot 'windows-environment.ps1')
$ProgressPreference = "SilentlyContinue"
[Console]::OutputEncoding = New-Object Text.UTF8Encoding($false)
$OutputEncoding = [Console]::OutputEncoding
$minimumAvailableBytes = 2GB
$minimumDiskBytes = 10GB
$minimumPostInstallBytes = 4GB
$installRoot = Join-Path $env:ProgramData "IRM"
$appRoot = Join-Path $installRoot "app"
$dataRoot = Join-Path $installRoot "data"
$backupRoot = Join-Path $installRoot "backups"
$logRoot = Join-Path $installRoot "logs"
$toolsRoot = Join-Path $installRoot "tools"
$script:currentStep = 'Khởi động trình cài đặt'

trap {
    $failure = $_
    [Console]::Error.WriteLine("IRM_ERROR_STEP=$script:currentStep")
    [Console]::Error.WriteLine("IRM_ERROR_MESSAGE=$($failure.Exception.Message)")
    if (-not [string]::IsNullOrWhiteSpace($failure.ScriptStackTrace)) {
        [Console]::Error.WriteLine("IRM_ERROR_LOCATION=$($failure.ScriptStackTrace)")
    }
    exit 1
}

function Write-InstallStep([string]$Name) {
    $script:currentStep = $Name
    Write-Output ("[{0}] {1}" -f (Get-Date -Format 'HH:mm:ss'), $Name)
}

function Get-LatestSqlSetupLog {
    $bootstrapRoot = Join-Path ${env:ProgramFiles} 'Microsoft SQL Server\160\Setup Bootstrap\Log'
    if (-not (Test-Path -LiteralPath $bootstrapRoot -PathType Container)) { return $null }
    return Get-ChildItem -LiteralPath $bootstrapRoot -Filter Summary.txt -File -Recurse -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
}

function Invoke-CheckedInstaller {
    param([string]$FilePath, [string[]]$Arguments, [string]$DisplayName, [int[]]$AdditionalSuccessExitCodes = @())
    $process = Start-Process -FilePath $FilePath -ArgumentList $Arguments -Wait -PassThru
    $successExitCodes = @(0,3010) + $AdditionalSuccessExitCodes
    if ($process.ExitCode -notin $successExitCodes) {
        $sqlLog = Get-LatestSqlSetupLog
        $logHint = if ($sqlLog) { " Nhật ký SQL: $sqlLog" } else { '' }
        throw "$DisplayName thất bại (mã $($process.ExitCode)).$logHint"
    }
    if ($process.ExitCode -eq 3010) { Write-Output "$DisplayName yêu cầu khởi động lại; bộ cài sẽ hoàn tất cấu hình trước khi thông báo." }
    if ($process.ExitCode -in $AdditionalSuccessExitCodes) { Write-Output "$DisplayName không cần thực hiện vì instance đã ở đúng hoặc cao hơn mức cập nhật yêu cầu." }
}

function Get-InstalledSqlVersion([string]$InstanceId) {
    $setup = Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$InstanceId\Setup"
    $versionText = if (-not [string]::IsNullOrWhiteSpace($setup.PatchLevel)) { $setup.PatchLevel } else { $setup.Version }
    return [version]$versionText
}

function Read-RequiredSecret([string]$Prompt, [int]$MinimumLength) {
    $secure = Read-Host $Prompt -AsSecureString
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try { $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
    if ([string]::IsNullOrWhiteSpace($plain) -or $plain.Length -lt $MinimumLength) { throw "$Prompt phải có ít nhất $MinimumLength ký tự." }
    return $plain
}
function New-RandomSecret {
    $bytes = New-Object byte[] 32
    [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    return [Convert]::ToBase64String($bytes)
}
function Assert-PayloadIntegrity {
    $checksumFile = Join-Path $PayloadRoot 'SHA256SUMS.txt'
    if (-not (Test-Path -LiteralPath $checksumFile -PathType Leaf)) { throw 'Thiếu SHA256SUMS.txt trong bộ cài.' }
    foreach ($line in Get-Content -LiteralPath $checksumFile) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        if ($line -notmatch '^([A-Fa-f0-9]{64})\s{2}(.+)$') { throw "Dòng checksum không hợp lệ: $line" }
        $expected = $matches[1].ToUpperInvariant()
        $relativePath = $matches[2]
        $file = Join-Path $PayloadRoot $relativePath
        if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw "Thiếu tệp trong bộ cài: $relativePath" }
        $actual = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash
        if ($actual -ne $expected) { throw "Checksum không đúng: $relativePath" }
    }
}
function Assert-Preflight {
    $os = Get-CimInstance Win32_OperatingSystem
    $windows = Get-IrmWindowsCompatibility -OperatingSystem $os
    if (-not $windows.ArchitecturePassed) { throw "IRM chỉ hỗ trợ Windows 64-bit. $($windows.ArchitectureDetail)" }
    if (-not $windows.VersionPassed) { throw $windows.VersionDetail }
    if (-not $windows.Is64BitProcess) { throw 'Windows là 64-bit nhưng tiến trình PowerShell đang là 32-bit. Hãy chạy bộ cài EXE hoặc Windows PowerShell 64-bit.' }
    $framework = Get-IrmNetFrameworkCompatibility
    if (-not $framework.Passed) { throw $framework.Detail }

    $cpu = Get-CimInstance Win32_Processor | Select-Object -First 1
    if ($cpu.NumberOfLogicalProcessors -lt 2 -or $cpu.MaxClockSpeed -lt 2000) { throw "Cần tối thiểu 2 CPU core, 2 GHz." }
    $available = [long]$os.FreePhysicalMemory * 1KB
    if ($available -lt $minimumAvailableBytes) { throw "Cần tối thiểu 2 GB RAM khả dụng." }
    $drive = Get-PSDrive -Name ([IO.Path]::GetPathRoot($installRoot).TrimEnd(':\'))
    if ($drive.Free -lt $minimumDiskBytes) { throw "Cần tối thiểu 10 GB ổ đĩa trống." }
    $existingIrmService = Get-Service IRM -ErrorAction SilentlyContinue
    $existingSqlService = Get-Service 'MSSQL$IRMEXPRESS' -ErrorAction SilentlyContinue
    foreach ($port in 14331,5050,5443) {
        $listener = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue
        $ownedByExistingIrm = $existingIrmService -and $port -in 5050,5443
        $ownedByExistingSql = $existingSqlService -and $port -eq 14331
        if ($listener -and -not $ownedByExistingIrm -and -not $ownedByExistingSql) {
            throw "Cổng $port đang được ứng dụng khác sử dụng."
        }
    }
}

Write-InstallStep 'Kiểm tra tính toàn vẹn của bộ cài'
Assert-PayloadIntegrity
Write-InstallStep 'Kiểm tra điều kiện máy tính'
Assert-Preflight
Write-Host "IRM sử dụng Microsoft SQL Server 2022 Express."
if (-not $AcceptSqlLicense -and (Read-Host "Nhập YES để chấp nhận điều khoản Microsoft SQL Server") -ne "YES") {
    throw "Chưa chấp nhận điều khoản SQL Server."
}
if ([string]::IsNullOrWhiteSpace($AdminUser)) { $AdminUser = $env:IRM_INSTALL_ADMIN_USER }
if ([string]::IsNullOrWhiteSpace($AdminUser) -and -not $NonInteractive) { $AdminUser = Read-Host "Tên tài khoản quản trị IRM [admin]" }
if ([string]::IsNullOrWhiteSpace($AdminUser)) { $AdminUser = "admin" }
if ([string]::IsNullOrWhiteSpace($AdminPassword)) { $AdminPassword = $env:IRM_INSTALL_ADMIN_PASSWORD }
if ([string]::IsNullOrWhiteSpace($DbaPassword)) { $DbaPassword = $env:IRM_INSTALL_DBA_PASSWORD }
if ([string]::IsNullOrWhiteSpace($AdminPassword)) { $AdminPassword = Read-RequiredSecret "Mật khẩu quản trị IRM" 12 }
if ([string]::IsNullOrWhiteSpace($DbaPassword)) { $DbaPassword = Read-RequiredSecret "Mật khẩu irm_dba phục hồi database" 16 }
if ($AdminPassword.Length -lt 12) { throw "Mật khẩu quản trị IRM phải có ít nhất 12 ký tự." }
if ($DbaPassword.Length -lt 16) { throw "Mật khẩu irm_dba phải có ít nhất 16 ký tự." }
$saPassword = New-RandomSecret
$appPassword = New-RandomSecret
$certificatePassword = New-RandomSecret
if ($DbaPassword -match '[;"\r\n]') { throw 'Mật khẩu irm_dba không được chứa dấu chấm phẩy, dấu nháy kép hoặc ký tự xuống dòng.' }
$enableLanValue = $EnableLan.IsPresent
if (-not $NonInteractive -and -not $PSBoundParameters.ContainsKey('EnableLan')) {
    $enableLanValue = (Read-Host "Cho phép máy khác truy cập qua HTTPS? [y/N]") -match '^[Yy]'
}
$lanUrl = if ($enableLanValue) { 'https://0.0.0.0:5443' } else { 'https://127.0.0.1:5443' }

Write-InstallStep 'Sao chép ứng dụng IRM'
New-Item -ItemType Directory -Force -Path $appRoot,$dataRoot,$backupRoot,$logRoot,$toolsRoot | Out-Null
$existingIrmService = Get-Service IRM -ErrorAction SilentlyContinue
if ($existingIrmService -and $existingIrmService.Status -ne 'Stopped') {
    Stop-Service IRM -Force
    $existingIrmService.WaitForStatus('Stopped',[TimeSpan]::FromSeconds(30))
}
Copy-Item -Path (Join-Path $PayloadRoot "app\*") -Destination $appRoot -Recurse -Force
Copy-Item -LiteralPath (Join-Path $PayloadRoot "uninstall.ps1") -Destination $toolsRoot -Force
Copy-Item -LiteralPath (Join-Path $PayloadRoot "restore.ps1") -Destination $toolsRoot -Force
Copy-Item -LiteralPath (Join-Path $PayloadRoot "repair.ps1") -Destination $toolsRoot -Force
Copy-Item -LiteralPath (Join-Path $PayloadRoot "backup.ps1") -Destination $toolsRoot -Force
Copy-Item -LiteralPath (Join-Path $PayloadRoot "windows-environment.ps1") -Destination $toolsRoot -Force
Copy-Item -LiteralPath (Join-Path $PayloadRoot "diagnose-environment.ps1") -Destination $toolsRoot -Force
Copy-Item -LiteralPath (Join-Path $PayloadRoot "release-manifest.json") -Destination $installRoot -Force
Copy-Item -LiteralPath (Join-Path $PayloadRoot "SBOM-dotnet.json") -Destination $installRoot -Force
Copy-Item -LiteralPath (Join-Path $PayloadRoot "IRM-v1.0.1-schema.sql") -Destination $installRoot -Force
Copy-Item -LiteralPath (Join-Path $PayloadRoot "SHA256SUMS.txt") -Destination $installRoot -Force

$installedSqlNow = $false
$sqlService = Get-Service 'MSSQL$IRMEXPRESS' -ErrorAction SilentlyContinue
if (-not $sqlService) {
    Write-InstallStep 'Cài đặt SQL Server 2022 Express'
    $baseArgs = @(
        '/Q','/ACTION=Install','/FEATURES=SQLENGINE','/INSTANCENAME=IRMEXPRESS','/SECURITYMODE=SQL',
        "/SAPWD=$saPassword",'/TCPENABLED=1','/NPENABLED=0','/SQLSVCSTARTUPTYPE=Automatic',
        '/SQLSYSADMINACCOUNTS="BUILTIN\Administrators"','/IACCEPTSQLSERVERLICENSETERMS','/UPDATEENABLED=FALSE',
        '/USEMICROSOFTUPDATE=FALSE','/ERRORREPORTING=FALSE','/SUPPRESSPRIVACYSTATEMENTNOTICE=TRUE','/ENU'
    )
    Invoke-CheckedInstaller -FilePath (Join-Path $PayloadRoot "sql-base.exe") -Arguments $baseArgs -DisplayName 'Cài SQL Server'
    $installedSqlNow = $true
}

Write-InstallStep 'Kiểm tra phiên bản SQL Server'
$instanceId = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL').IRMEXPRESS
if ([string]::IsNullOrWhiteSpace($instanceId)) { throw 'Không tìm thấy SQL Server instance IRMEXPRESS sau khi cài.' }
$currentSqlVersion = Get-InstalledSqlVersion $instanceId
$requiredSqlVersion = [version]'16.0.4295.3'
if ($currentSqlVersion -lt $requiredSqlVersion) {
    Write-InstallStep 'Cập nhật SQL Server lên CU27'
    Invoke-CheckedInstaller -FilePath (Join-Path $PayloadRoot "sql-cu.exe") -Arguments @('/quiet','/action=patch','/instancename=IRMEXPRESS','/IAcceptSQLServerLicenseTerms') -DisplayName 'Cài SQL Server CU27' -AdditionalSuccessExitCodes @(-2042429437)
    $currentSqlVersion = Get-InstalledSqlVersion $instanceId
    if ($currentSqlVersion -lt $requiredSqlVersion) { throw "SQL Server vẫn ở phiên bản $currentSqlVersion sau khi cập nhật; yêu cầu tối thiểu $requiredSqlVersion." }
}

Write-InstallStep 'Cấu hình cổng SQL Server'
$tcpPath = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\$instanceId\MSSQLServer\SuperSocketNetLib\Tcp\IPAll"
Set-ItemProperty $tcpPath TcpDynamicPorts ''
Set-ItemProperty $tcpPath TcpPort '14331'
Restart-Service 'MSSQL$IRMEXPRESS'

Write-InstallStep 'Tạo chứng thư HTTPS nội bộ'
$cert = New-SelfSignedCertificate -DnsName @('localhost',$env:COMPUTERNAME) -CertStoreLocation 'Cert:\LocalMachine\My' -NotAfter (Get-Date).AddYears(3)
$pfxPath = Join-Path $dataRoot 'irm-https.pfx'
$secureCertPassword = ConvertTo-SecureString $certificatePassword -AsPlainText -Force
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $secureCertPassword | Out-Null

$connectionString = "Server=127.0.0.1,14331;Database=IRM;User Id=irm_app;Password=$appPassword;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
$settings = @{
    ConnectionStrings = @{ DefaultConnection = $connectionString }
    DataProtection = @{ KeysPath = (Join-Path $dataRoot 'keys') }
    FileStorage = @{ Root = (Join-Path $dataRoot 'private-files') }
    Backup = @{ Directory = $backupRoot }
    Security = @{ RequireHttpsForRemoteClients = $true }
    Storage = @{ StopUploadsBelowFreeBytes = 1073741824; WarnBelowFreeBytes = 2147483648 }
    AllowedHosts = '*'
    Kestrel = @{ Endpoints = @{
        Loopback = @{ Url = 'http://127.0.0.1:5050' }
        LanHttps = @{ Url = $lanUrl; Certificate = @{ Path = $pfxPath; Password = $certificatePassword } }
    }}
} | ConvertTo-Json -Depth 8
$settingsPath = Join-Path $appRoot 'appsettings.Production.json'
[IO.File]::WriteAllText($settingsPath, $settings, (New-Object Text.UTF8Encoding($false)))

Write-InstallStep 'Khởi tạo database và tài khoản quản trị'
$bootstrap = New-Object Diagnostics.ProcessStartInfo
$bootstrap.FileName = Join-Path $appRoot 'IRM.exe'
$bootstrap.Arguments = '--bootstrap --provision-database'
$bootstrap.UseShellExecute = $false
$bootstrap.EnvironmentVariables['ASPNETCORE_ENVIRONMENT'] = 'Production'
$bootstrap.EnvironmentVariables['ConnectionStrings__DefaultConnection'] = $connectionString
$provisioningAdminConnectionString = if ($installedSqlNow) {
    "Server=127.0.0.1,14331;Database=master;User Id=sa;Password=$saPassword;Encrypt=True;TrustServerCertificate=True"
} else {
    "Server=.\IRMEXPRESS;Database=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
}
$bootstrap.EnvironmentVariables['Provisioning__AdminConnectionString'] = $provisioningAdminConnectionString
$bootstrap.EnvironmentVariables['Provisioning__AppPassword'] = $appPassword
$bootstrap.EnvironmentVariables['Provisioning__DbaPassword'] = $DbaPassword
$bootstrap.EnvironmentVariables['Bootstrap__AdminUsername'] = $AdminUser
$bootstrap.EnvironmentVariables['Bootstrap__AdminPassword'] = $AdminPassword
$bootstrap.EnvironmentVariables['DOTNET_GCHeapHardLimit'] = '0x20000000'
$bootstrap.WorkingDirectory = $appRoot
$bootstrapProcess = [Diagnostics.Process]::Start($bootstrap)
$bootstrapProcess.WaitForExit()
if ($bootstrapProcess.ExitCode -ne 0) { throw "Khởi tạo database thất bại." }

Write-InstallStep 'Kiểm tra database, đăng nhập, upload và báo cáo'
$selfTest = New-Object Diagnostics.ProcessStartInfo
$selfTest.FileName = Join-Path $appRoot 'IRM.exe'
$selfTest.Arguments = '--self-test'
$selfTest.UseShellExecute = $false
$selfTest.EnvironmentVariables['ASPNETCORE_ENVIRONMENT'] = 'Production'
$selfTest.EnvironmentVariables['ConnectionStrings__DefaultConnection'] = $connectionString
$selfTest.EnvironmentVariables['SelfTest__AdminUsername'] = $AdminUser
$selfTest.EnvironmentVariables['SelfTest__AdminPassword'] = $AdminPassword
$selfTest.EnvironmentVariables['DOTNET_GCHeapHardLimit'] = '0x20000000'
$selfTest.WorkingDirectory = $appRoot
$selfTestProcess = [Diagnostics.Process]::Start($selfTest)
$selfTestProcess.WaitForExit()
if ($selfTestProcess.ExitCode -ne 0) { throw "Kiểm tra đăng nhập/database/upload/quét file/xuất báo cáo thất bại." }

Write-InstallStep 'Đăng ký dịch vụ Windows IRM'
$existingIrmService = Get-Service IRM -ErrorAction SilentlyContinue
if ($existingIrmService) {
    Stop-Service IRM -Force -ErrorAction SilentlyContinue
    sc.exe config IRM binPath= "`"$(Join-Path $appRoot 'IRM.exe')`"" start= auto obj= 'NT AUTHORITY\LocalService' DisplayName= 'IRM v1.0.2' | Out-Null
} else {
    sc.exe create IRM binPath= "`"$(Join-Path $appRoot 'IRM.exe')`"" start= auto obj= 'NT AUTHORITY\LocalService' DisplayName= 'IRM v1.0.2' | Out-Null
}
New-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\IRM' -Name Environment -PropertyType MultiString -Value @('ASPNETCORE_ENVIRONMENT=Production','DOTNET_GCHeapHardLimit=0x20000000') -Force | Out-Null
sc.exe failure IRM reset= 86400 actions= restart/60000/restart/60000 | Out-Null
icacls $installRoot /inheritance:r /grant:r 'SYSTEM:(OI)(CI)F' 'Administrators:(OI)(CI)F' 'LOCAL SERVICE:(OI)(CI)RX' | Out-Null
icacls $dataRoot /grant:r 'LOCAL SERVICE:(OI)(CI)M' | Out-Null
icacls $backupRoot /grant:r 'NT SERVICE\MSSQL$IRMEXPRESS:(OI)(CI)M' | Out-Null

Write-InstallStep 'Cấu hình sao lưu và tường lửa'
$action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$(Join-Path $toolsRoot 'backup.ps1')`"" -WorkingDirectory $appRoot
$trigger = New-ScheduledTaskTrigger -Daily -At '02:00'
Register-ScheduledTask -TaskName 'IRM-DailyBackup' -Action $action -Trigger $trigger -User 'SYSTEM' -RunLevel Highest -Force | Out-Null
Remove-NetFirewallRule -DisplayName 'IRM HTTPS LAN' -ErrorAction SilentlyContinue
if ($enableLanValue) { New-NetFirewallRule -DisplayName 'IRM HTTPS LAN' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 5443 -Profile Domain,Private | Out-Null }

$shell = New-Object -ComObject WScript.Shell
foreach ($shortcutPath in @((Join-Path ([Environment]::GetFolderPath('CommonDesktopDirectory')) 'IRM.lnk'), (Join-Path ([Environment]::GetFolderPath('CommonPrograms')) 'IRM.lnk'))) {
    $shortcut = $shell.CreateShortcut($shortcutPath); $shortcut.TargetPath = 'http://localhost:5050'; $shortcut.Save()
}
Write-InstallStep 'Khởi động và kiểm tra IRM'
Start-Service IRM
Start-Sleep -Seconds 5
$health = Invoke-RestMethod 'http://127.0.0.1:5050/health/ready' -TimeoutSec 15
if ($health.status -ne 'ready') { throw "Health check IRM thất bại." }
if ((Get-PSDrive -Name ([IO.Path]::GetPathRoot($installRoot).TrimEnd(':\'))).Free -lt $minimumPostInstallBytes) { throw "Sau cài đặt còn dưới 4 GB ổ đĩa." }
Start-Process 'http://localhost:5050'
Write-Host 'Cài đặt IRM v1.0.2 hoàn tất.' -ForegroundColor Green
