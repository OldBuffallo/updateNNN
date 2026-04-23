#Requires -RunAsAdministrator
<#
.SYNOPSIS
    IRM v2.0 — Go bo hoan toan
.DESCRIPTION
    Dung service, xoa service, xoa Firewall rule, xoa Scheduled Task.
    Mac dinh GIU LAI thu muc cai dat (de bao toan data/backup).
    Dung flag -RemoveFiles de xoa sach.
#>

param(
    [string]$InstallDir = "C:\IRM",
    [int]$Port = 5050,
    [switch]$RemoveFiles
)

Clear-Host
Write-Host ""
Write-Host "  ================================================================" -ForegroundColor Red
Write-Host "    IRM v2.0 — Go bo he thong" -ForegroundColor White
Write-Host "  ================================================================" -ForegroundColor Red
Write-Host ""

if ($RemoveFiles) {
    Write-Host "  CHE DO: Xoa TOAN BO (bao gom thu muc $InstallDir)" -ForegroundColor Red
} else {
    Write-Host "  CHE DO: Chi xoa Service/Firewall — GIU LAI file va database" -ForegroundColor Yellow
}
Write-Host ""

$confirm = Read-Host "  Ban co chac chan? (Y/N)"
if ($confirm -notin @('Y', 'y')) {
    Write-Host "`n  Da huy." -ForegroundColor Yellow
    exit 0
}

Write-Host ""

# 1. Dừng & xóa Service
Write-Host "  [1/4] Dung va xoa Windows Service..." -ForegroundColor Cyan
$svc = Get-Service -Name "IRM" -ErrorAction SilentlyContinue
if ($svc) {
    if ($svc.Status -eq 'Running') {
        sc.exe stop IRM | Out-Null
        Start-Sleep -Seconds 5
        Write-Host "    [OK] Da dung service" -ForegroundColor Green
    }
    sc.exe delete IRM | Out-Null
    Start-Sleep -Seconds 2
    Write-Host "    [OK] Da xoa service" -ForegroundColor Green
} else {
    Write-Host "    Service khong ton tai — bo qua" -ForegroundColor Gray
}

# 2. Xóa Firewall rule
Write-Host "  [2/4] Xoa Firewall rule..." -ForegroundColor Cyan
$fwResult = netsh advfirewall firewall delete rule name="IRM Web Application" 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "    [OK] Da xoa rule Firewall (port $Port)" -ForegroundColor Green
} else {
    Write-Host "    Rule khong ton tai — bo qua" -ForegroundColor Gray
}

# 3. Xóa Scheduled Tasks
Write-Host "  [3/4] Xoa Scheduled Tasks..." -ForegroundColor Cyan
$tasks = @("IRM-DailyBackup", "IRM-HealthCheck")
foreach ($task in $tasks) {
    try {
        Unregister-ScheduledTask -TaskName $task -Confirm:$false -ErrorAction Stop
        Write-Host "    [OK] Da xoa task: $task" -ForegroundColor Green
    } catch {
        Write-Host "    Task '$task' khong ton tai — bo qua" -ForegroundColor Gray
    }
}

# 4. Xóa thư mục (nếu có flag)
Write-Host "  [4/4] Thu muc cai dat..." -ForegroundColor Cyan
if ($RemoveFiles) {
    if (Test-Path $InstallDir) {
        Remove-Item -Path $InstallDir -Recurse -Force
        Write-Host "    [OK] Da xoa: $InstallDir" -ForegroundColor Green
    }
} else {
    Write-Host "    GIU LAI: $InstallDir" -ForegroundColor Yellow
    Write-Host "    (Dung -RemoveFiles de xoa sach)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "  ================================================================" -ForegroundColor Green
Write-Host "    GO BO HOAN TAT" -ForegroundColor Green
Write-Host "  ================================================================" -ForegroundColor Green
Write-Host ""
