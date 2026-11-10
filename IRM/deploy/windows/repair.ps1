$ErrorActionPreference = 'Stop'
$service = Get-Service IRM -ErrorAction Stop
if ($service.Status -ne 'Running') { Start-Service IRM }
Start-Sleep -Seconds 3
$health = Invoke-RestMethod 'http://127.0.0.1:5050/health/ready' -TimeoutSec 15
if ($health.status -ne 'ready') { throw 'IRM chưa đạt health check.' }
Write-Host 'IRM service và kết nối database hoạt động bình thường.' -ForegroundColor Green
