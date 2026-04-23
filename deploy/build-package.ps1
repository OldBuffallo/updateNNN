<#
.SYNOPSIS
    IRM v2.0 — Đóng gói toàn bộ vào USB
.DESCRIPTION
    Chạy trên máy DEV. Tạo thư mục deploy-package/ sẵn sàng copy vào USB.
    Bao gồm: app + scripts + SQL migrations + thư mục data (cho file .bak)
.USAGE
    .\deploy\build-package.ps1
    .\deploy\build-package.ps1 -Zip
#>

param(
    [switch]$Zip,
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$projectDir = Join-Path $repoRoot "IRM"
$deployScriptsDir = Join-Path $repoRoot "deploy"
$outputDir = Join-Path $repoRoot "deploy-package"
$appDir = Join-Path $outputDir "deploy\app"
$sqlDir = Join-Path $outputDir "deploy\sql"
$dataDir = Join-Path $outputDir "deploy\data"
$scriptsOut = Join-Path $outputDir "deploy"

Clear-Host
Write-Host ""
Write-Host "  ================================================================" -ForegroundColor DarkCyan
Write-Host "    IRM v2.0 — Dong goi USB" -ForegroundColor White
Write-Host "  ================================================================" -ForegroundColor DarkCyan
Write-Host ""

# ── 1. Build ──
Write-Host "  [1/4] Build self-contained ($Runtime)..." -ForegroundColor Cyan

if (Test-Path $outputDir) { Remove-Item -Path $outputDir -Recurse -Force }

dotnet publish $projectDir -c Release --self-contained -r $Runtime -o $appDir
if ($LASTEXITCODE -ne 0) { Write-Host "  [XX] Build that bai!" -ForegroundColor Red; exit 1 }

$appSize = [math]::Round((Get-ChildItem $appDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB, 1)
Write-Host "    [OK] Build thanh cong — $appSize MB" -ForegroundColor Green

# ── 2. Copy scripts ──
Write-Host "  [2/4] Copy scripts..." -ForegroundColor Cyan

Copy-Item (Join-Path $deployScriptsDir "install.ps1") $scriptsOut
Copy-Item (Join-Path $deployScriptsDir "uninstall.ps1") $scriptsOut
Copy-Item (Join-Path $deployScriptsDir "backup-old-server.ps1") $scriptsOut
Write-Host "    [OK] install.ps1, uninstall.ps1, backup-old-server.ps1" -ForegroundColor Green

# ── 3. Copy SQL scripts + tạo thư mục data ──
Write-Host "  [3/4] Copy SQL migrations + tao thu muc data..." -ForegroundColor Cyan

$sqlSrcDir = Join-Path $deployScriptsDir "sql"
if (Test-Path $sqlSrcDir) {
    New-Item -ItemType Directory -Path $sqlDir -Force | Out-Null
    Copy-Item "$sqlSrcDir\*" $sqlDir -Recurse -Force
    $sqlCount = (Get-ChildItem $sqlDir -Filter "*.sql").Count
    Write-Host "    [OK] $sqlCount SQL scripts" -ForegroundColor Green
}

# Tạo thư mục data rỗng (để user bỏ file .bak vào)
New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
$dataReadme = @"
=== THU MUC DATA ===

Dat file backup database (.bak) vao day truoc khi chay install.ps1

Cach tao file .bak:
  1. Tren may cu, chay: .\backup-old-server.ps1
  2. Copy file ReportManagerDB_xxxxx.bak vao thu muc nay

Hoac backup bang tay (SSMS):
  - Mo SQL Server Management Studio
  - Click phai database 'ReportManagerDB' > Tasks > Back Up...
  - Chon 'Full' backup > OK
  - Copy file .bak vao day
"@
Set-Content -Path (Join-Path $dataDir "DAT-FILE-BAK-VAO-DAY.txt") -Value $dataReadme
Write-Host "    [OK] Thu muc data\ (dat file .bak vao day)" -ForegroundColor Green

# ── 4. Tạo README ──
Write-Host "  [4/4] Tao huong dan..." -ForegroundColor Cyan

$readme = @"
==========================================================
  IRM v2.0 — HUONG DAN CAI DAT
==========================================================

TRUOC KHI CAI DAT:
  1. BACKUP du lieu tren may cu:
     - Chay: backup-old-server.ps1
     - HOAC: Dung SSMS backup thu cong
  2. Copy file .bak vao: deploy\data\

CAI DAT (chay tren may chu moi):
  1. Mo PowerShell (Admin)
  2. Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
  3. cd <USB>:\deploy
  4. .\install.ps1

Script se tu dong:
  - Restore database tu file .bak
  - Chay migration (tao bang moi)
  - Cai ung dung + Windows Service
  - Mo Firewall cho 3 client

SAU KHI CAI DAT:
  - May chu : http://localhost:5050
  - Client  : http://<IP-may-chu>:5050

==========================================================
  CAU TRUC USB
==========================================================

  deploy\
  +-- install.ps1              Chay file nay = cai dat xong
  +-- uninstall.ps1            Go bo
  +-- backup-old-server.ps1    Chay tren may CU de backup DB
  +-- DOC-TRUOC.txt            File nay
  +-- data\                    <== DAT FILE .bak VAO DAY
  +-- sql\                     Migration scripts
  |   +-- 01-add-new-tables.sql
  |   +-- 02-add-indexes.sql
  +-- app\                     Ung dung IRM v2.0
      +-- IRM.exe
      +-- appsettings.json
      +-- wwwroot\
      +-- *.dll

==========================================================
"@
Set-Content -Path (Join-Path $scriptsOut "DOC-TRUOC.txt") -Value $readme
Write-Host "    [OK] DOC-TRUOC.txt" -ForegroundColor Green

# ── ZIP (nếu cần) ──
if ($Zip) {
    $zipPath = Join-Path $repoRoot "IRM-v2.0-deploy.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path "$outputDir\*" -DestinationPath $zipPath
    $zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)
    Write-Host "    [OK] ZIP: $zipPath ($zipSize MB)" -ForegroundColor Green
}

$totalSize = [math]::Round((Get-ChildItem $outputDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB, 1)

Write-Host ""
Write-Host "  ================================================================" -ForegroundColor Green
Write-Host "    DONG GOI HOAN TAT — $totalSize MB" -ForegroundColor Green
Write-Host "  ================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Output: $outputDir" -ForegroundColor White
Write-Host ""
Write-Host "  QUY TRINH TRIEN KHAI:" -ForegroundColor Yellow
Write-Host "    1. [May cu]  Chay backup-old-server.ps1" -ForegroundColor Gray
Write-Host "    2. [May cu]  Copy file .bak vao deploy-package\deploy\data\" -ForegroundColor Gray
Write-Host "    3.           Copy toan bo 'deploy-package\' vao USB" -ForegroundColor Gray
Write-Host "    4. [May moi] Cam USB, mo PowerShell (Admin)" -ForegroundColor Gray
Write-Host "    5. [May moi] cd E:\deploy && .\install.ps1" -ForegroundColor Gray
Write-Host "    6.           Xong! Mo browser: http://localhost:5050" -ForegroundColor Gray
Write-Host ""
