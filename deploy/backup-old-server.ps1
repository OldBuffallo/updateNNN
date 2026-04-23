<#
.SYNOPSIS
    Backup database ReportManagerDB từ máy chủ cũ (SQL Server 2014)
.DESCRIPTION
    Chạy script này TRÊN MÁY CHỦ CŨ để tạo file backup .bak
    Sau đó copy file .bak vào USB cùng với gói deploy
.USAGE
    .\backup-old-server.ps1
    .\backup-old-server.ps1 -SqlInstance "DESKTOP-BJ63NDN\BIRDIEPO"
    .\backup-old-server.ps1 -SqlInstance ".\SQLEXPRESS" -OutputDir "E:\backup"
#>

param(
    [string]$SqlInstance = ".\SQLEXPRESS",
    [string]$DatabaseName = "ReportManagerDB",
    [string]$OutputDir = ".",
    [string]$SqlUser = "",
    [string]$SqlPassword = ""
)

Clear-Host
Write-Host ""
Write-Host "  ================================================================" -ForegroundColor DarkCyan
Write-Host "    Backup Database — $DatabaseName" -ForegroundColor White
Write-Host "    Tu SQL Server: $SqlInstance" -ForegroundColor Gray
Write-Host "  ================================================================" -ForegroundColor DarkCyan
Write-Host ""

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = Join-Path $OutputDir "${DatabaseName}_${timestamp}.bak"

# Xây dựng lệnh sqlcmd
if ($SqlUser -ne "" -and $SqlPassword -ne "") {
    # SQL Server Authentication
    $authParams = "-U `"$SqlUser`" -P `"$SqlPassword`""
} else {
    # Windows Authentication
    $authParams = "-E"
}

$sqlCommand = @"
BACKUP DATABASE [$DatabaseName]
TO DISK = N'$backupFile'
WITH FORMAT,
     NAME = N'$DatabaseName - Full Backup $timestamp',
     DESCRIPTION = N'Backup truoc khi chuyen sang IRM v2.0',
     STATS = 10;
"@

Write-Host "  [1/3] Kiem tra ket noi SQL Server..." -ForegroundColor Cyan

# Test connection
$testCmd = "sqlcmd -S `"$SqlInstance`" $authParams -Q `"SELECT @@VERSION`" -W -h -1"
$testResult = Invoke-Expression $testCmd 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "    [XX] Khong ket noi duoc toi SQL Server!" -ForegroundColor Red
    Write-Host "    Chi tiet: $testResult" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Thu lai voi tham so khac:" -ForegroundColor Yellow
    Write-Host "    .\backup-old-server.ps1 -SqlInstance `"TEN_MAY\TEN_INSTANCE`"" -ForegroundColor Gray
    Write-Host "    .\backup-old-server.ps1 -SqlInstance `".\BIRDIEPO`" -SqlUser `"sa`" -SqlPassword `"123456`"" -ForegroundColor Gray
    Write-Host ""
    exit 1
}
Write-Host "    [OK] Da ket noi" -ForegroundColor Green

Write-Host "  [2/3] Dang backup database..." -ForegroundColor Cyan
Write-Host "    File: $backupFile" -ForegroundColor Gray

$backupCmd = "sqlcmd -S `"$SqlInstance`" $authParams -Q `"$sqlCommand`""
$backupResult = Invoke-Expression $backupCmd 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "    [XX] Backup that bai!" -ForegroundColor Red
    Write-Host "    $backupResult" -ForegroundColor Gray
    exit 1
}

# Kiểm tra file
Write-Host "  [3/3] Kiem tra file backup..." -ForegroundColor Cyan

if (Test-Path $backupFile) {
    $fileSize = [math]::Round((Get-Item $backupFile).Length / 1MB, 1)
    Write-Host "    [OK] $backupFile ($fileSize MB)" -ForegroundColor Green
} else {
    Write-Host "    [!!] File khong tim thay tai $backupFile" -ForegroundColor Yellow
    Write-Host "    SQL Server co the luu tai thu muc mac dinh. Kiem tra:" -ForegroundColor Gray
    Write-Host "    C:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\Backup\" -ForegroundColor Gray
}

Write-Host ""
Write-Host "  ================================================================" -ForegroundColor Green
Write-Host "    BACKUP HOAN TAT" -ForegroundColor Green
Write-Host "  ================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Buoc tiep theo:" -ForegroundColor Yellow
Write-Host "    1. Copy file .bak nay vao USB:" -ForegroundColor Gray
Write-Host "       USB:\deploy\data\${DatabaseName}_${timestamp}.bak" -ForegroundColor White
Write-Host "    2. Mang USB sang may chu moi" -ForegroundColor Gray
Write-Host "    3. Chay: .\install.ps1" -ForegroundColor Gray
Write-Host ""
