$ErrorActionPreference = 'Stop'
$service = Get-Service IRM -ErrorAction Stop
if ($service.Status -ne 'Running') { Start-Service IRM }

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
if (-not $ready) { throw 'IRM chưa đạt health check sau 30 giây.' }
Write-Host 'IRM service và kết nối database hoạt động bình thường.' -ForegroundColor Green
