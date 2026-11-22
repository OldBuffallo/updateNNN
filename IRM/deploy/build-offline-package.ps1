<# ============================================================
   build-offline-package.ps1 — Đóng gói bộ cài Offline IRM v1.0.2
   Chạy trên máy DEV, tạo ra file ZIP chứa toàn bộ bộ cài
   ============================================================ #>

param(
    [string]$OutputDir = "",
    [switch]$IncludeSqlSetup   # Bật nếu muốn bundle SQL Server setup
)

$ErrorActionPreference = "Stop"
$IRM_VERSION = "1.0.2"
$RepoRoot = Join-Path $PSScriptRoot "..\.." | Resolve-Path
$IrmProject = Join-Path $RepoRoot "IRM"

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "IRM-v$IRM_VERSION-offline-installer"
}

Write-Host ""
Write-Host "  ╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "  ║  Build Offline Package — IRM v$IRM_VERSION              ║" -ForegroundColor Cyan
Write-Host "  ╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# ── Clean output ──
if (Test-Path $OutputDir) { Remove-Item $OutputDir -Recurse -Force }
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# ── Step 1: Publish self-contained ──
Write-Host "[1/5] Publishing self-contained app (win-x64)..." -ForegroundColor Yellow
$appOut = Join-Path $OutputDir "app"
dotnet publish $IrmProject -c Release -r win-x64 --self-contained -o $appOut /p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) { throw "Build failed!" }
Write-Host "  ✅ App published" -ForegroundColor Green

# ── Step 2: Generate EF migration script ──
Write-Host "[2/5] Generating idempotent migration script..." -ForegroundColor Yellow
$migrationFile = Join-Path $OutputDir "migration.sql"
dotnet ef migrations script --project $IrmProject --idempotent --output $migrationFile 2>$null
if (Test-Path $migrationFile) {
    Write-Host "  ✅ migration.sql generated" -ForegroundColor Green
} else {
    Write-Host "  ⚠️ Migration script not generated — app will auto-migrate" -ForegroundColor Yellow
}

# ── Step 3: Copy installer scripts ──
Write-Host "[3/5] Copying installer scripts..." -ForegroundColor Yellow
$installerDir = Join-Path $PSScriptRoot "offline-installer"
Copy-Item "$installerDir\install-irm.ps1" $OutputDir -Force
Copy-Item "$installerDir\CÀI ĐẶT IRM.bat" $OutputDir -Force
Write-Host "  ✅ Scripts copied" -ForegroundColor Green

# ── Step 4: Create prerequisites directory ──
Write-Host "[4/5] Setting up prerequisites..." -ForegroundColor Yellow
$prereqDir = Join-Path $OutputDir "prerequisites"
New-Item -ItemType Directory -Path $prereqDir -Force | Out-Null

if ($IncludeSqlSetup) {
    # Copy SQL Server Express setup nếu có
    $sqlSource = Join-Path $PSScriptRoot "SQLEXPRWT_x64_2014"
    if (Test-Path $sqlSource) {
        $sqlDest = Join-Path $prereqDir "SQLEXPRESS"
        Copy-Item $sqlSource $sqlDest -Recurse -Force
        Write-Host "  ✅ SQL Server Express bundled" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ SQL Server setup not found at $sqlSource" -ForegroundColor Yellow
        Write-Host "    Đặt bộ cài SQL Express vào: $prereqDir\SQLEXPRESS\" -ForegroundColor DarkGray
    }
} else {
    Write-Host "  ℹ️ SQL Server setup NOT bundled (use -IncludeSqlSetup to include)" -ForegroundColor DarkGray
    # Tạo file hướng dẫn
    @"
Đặt bộ cài SQL Server 2022 Express vào thư mục này.
Download tại: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

Cấu trúc:
  prerequisites\
    SQLEXPRESS\
      setup.exe
      ...
"@ | Set-Content (Join-Path $prereqDir "README-SQL-SETUP.txt") -Encoding UTF8
}

# ── Step 5: Create ZIP ──
Write-Host "[5/5] Creating ZIP package..." -ForegroundColor Yellow
$zipPath = Join-Path (Split-Path $OutputDir) "IRM-v$IRM_VERSION-offline-installer.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$OutputDir\*" -DestinationPath $zipPath -CompressionLevel Optimal
$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)
Write-Host "  ✅ ZIP created: $zipPath ($zipSize MB)" -ForegroundColor Green

# ── Summary ──
Write-Host ""
Write-Host "  ╔══════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "  ║  ✅ Offline Package Ready!                       ║" -ForegroundColor Green
Write-Host "  ╠══════════════════════════════════════════════════╣" -ForegroundColor Green
Write-Host "  ║  Folder: $OutputDir" -ForegroundColor Green
Write-Host "  ║  ZIP:    $zipPath" -ForegroundColor Green
Write-Host "  ╚══════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "  Cấu trúc bộ cài:" -ForegroundColor Cyan
Write-Host "    📁 IRM-v$IRM_VERSION-offline-installer\" -ForegroundColor White
Write-Host "    ├── 📄 CÀI ĐẶT IRM.bat          ← Khách hàng click đây" -ForegroundColor White
Write-Host "    ├── 📄 install-irm.ps1           ← Script cài đặt chính" -ForegroundColor White
Write-Host "    ├── 📄 migration.sql             ← SQL migration" -ForegroundColor White
Write-Host "    ├── 📁 app\                      ← Ứng dụng self-contained" -ForegroundColor White
Write-Host "    └── 📁 prerequisites\            ← SQL Server setup" -ForegroundColor White
