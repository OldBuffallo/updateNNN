[CmdletBinding()]
param([Parameter(Mandatory)][string]$ArchivePath)

$ErrorActionPreference = 'Stop'
$appRoot = Join-Path $env:ProgramData 'IRM\app'
if (-not (Test-Path -LiteralPath $ArchivePath -PathType Leaf)) { throw "Không tìm thấy bản sao lưu: $ArchivePath" }
if ($ArchivePath -notlike '*.bak.gz') { throw 'Chỉ chấp nhận tệp IRM_*.bak.gz.' }

$securePassword = Read-Host 'Mật khẩu irm_dba' -AsSecureString
$credential = New-Object System.Management.Automation.PSCredential('irm_dba', $securePassword)
$password = $credential.GetNetworkCredential().Password
if ($password -match '[;"\r\n]') { throw 'Mật khẩu irm_dba không hợp lệ với chuỗi kết nối.' }
$processInfo = New-Object Diagnostics.ProcessStartInfo
$processInfo.FileName = Join-Path $appRoot 'IRM.exe'
$processInfo.Arguments = '--restore'
$processInfo.UseShellExecute = $false
$processInfo.WorkingDirectory = $appRoot
$processInfo.EnvironmentVariables['ASPNETCORE_ENVIRONMENT'] = 'Production'
$processInfo.EnvironmentVariables['Restore__ArchivePath'] = (Resolve-Path -LiteralPath $ArchivePath).Path
$processInfo.EnvironmentVariables['Restore__AdminConnectionString'] = "Server=127.0.0.1,14331;Database=master;User Id=irm_dba;Password=$password;Encrypt=True;TrustServerCertificate=True"

Stop-Service IRM -ErrorAction SilentlyContinue
try {
    $process = [Diagnostics.Process]::Start($processInfo)
    $process.WaitForExit()
    if ($process.ExitCode -ne 0) { throw "Phục hồi database thất bại (mã $($process.ExitCode))." }
}
finally {
    $password = $null
    Start-Service IRM
}
$ready = $false
for ($attempt = 1; $attempt -le 15; $attempt++) {
    Start-Sleep -Seconds 2
    try {
        $health = Invoke-RestMethod 'http://127.0.0.1:5050/health/ready' -TimeoutSec 10
        if ($health.status -eq 'ready') {
            $ready = $true
            break
        }
    } catch { }
}
if (-not $ready) { throw 'IRM chưa đạt health check sau khi phục hồi.' }
Write-Host 'Phục hồi IRM hoàn tất.' -ForegroundColor Green
