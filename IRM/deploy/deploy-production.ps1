# ===================================================
# IRM Production Deploy Script
# Build self-contained package + SQL migration script
# ===================================================

param(
    [string]$OutputDir = "",
    [string]$Runtime = "win-x64",
    [switch]$CreateZip
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $PSScriptRoot "..\deploy-package"
}
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
$ProjectDir = Join-Path $PSScriptRoot "..\IRM"
$ProjectDir = [System.IO.Path]::GetFullPath($ProjectDir)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  IRM Production Deploy Builder         " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# -- Step 1: Clean output directory --
if (Test-Path $OutputDir) {
    Write-Host "[1/5] Cleaning output directory..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $OutputDir
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $OutputDir "app") -Force | Out-Null

# -- Step 2: Build self-contained publish --
Write-Host "[2/5] Building self-contained publish ($Runtime)..." -ForegroundColor Yellow
$appOut = Join-Path $OutputDir "app"
dotnet publish $ProjectDir -c Release -r $Runtime --self-contained -o $appOut /p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) { throw "Build failed!" }
Write-Host "  [OK] Build successful" -ForegroundColor Green

# -- Step 3: Copy / Generate SQL migration script --
Write-Host "[3/5] Setting up SQL migration script..." -ForegroundColor Yellow
$sqlMigrationTarget = Join-Path $OutputDir "migration.sql"
$sqlFallback = Join-Path $PSScriptRoot "sql\00-full-setup.sql"

if (Test-Path $sqlFallback) {
    Copy-Item $sqlFallback $sqlMigrationTarget -Force
    Write-Host "  [OK] Migration script copied from sql\00-full-setup.sql" -ForegroundColor Green
} else {
    Write-Host "  [!] sql\00-full-setup.sql not found, attempting ef migrations script..." -ForegroundColor Yellow
    dotnet ef migrations script --project $ProjectDir --idempotent --output $sqlMigrationTarget
}

# Also copy sql directory if present
$sqlSourceDir = Join-Path $PSScriptRoot "sql"
if (Test-Path $sqlSourceDir) {
    $sqlTargetDir = Join-Path $OutputDir "sql"
    New-Item -ItemType Directory -Path $sqlTargetDir -Force | Out-Null
    Copy-Item "$sqlSourceDir\*" $sqlTargetDir -Recurse -Force
    Write-Host "  [OK] SQL scripts folder copied" -ForegroundColor Green
}

# -- Step 4: Copy configuration and documentation --
Write-Host "[4/5] Copying configuration and guides..." -ForegroundColor Yellow
Copy-Item (Join-Path $ProjectDir "appsettings.Production.json") (Join-Path $appOut "appsettings.json") -Force

$guideDoc = Join-Path $PSScriptRoot "DEPLOY-GUIDE-PRODUCTION.md"
if (Test-Path $guideDoc) {
    Copy-Item $guideDoc $OutputDir -Force
}
$installScript = Join-Path $PSScriptRoot "install.ps1"
if (Test-Path $installScript) {
    Copy-Item $installScript $OutputDir -Force
}
$quickInstallScript = Join-Path $PSScriptRoot "quick-install.ps1"
if (Test-Path $quickInstallScript) {
    Copy-Item $quickInstallScript $OutputDir -Force
}
Write-Host "  [OK] Configuration and guides copied" -ForegroundColor Green

# -- Step 5: Create ZIP if requested --
if ($CreateZip) {
    Write-Host "[5/5] Creating ZIP package..." -ForegroundColor Yellow
    $zipPath = Join-Path (Split-Path $OutputDir) "IRM-production-deploy.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Compress-Archive -Path "$OutputDir\*" -DestinationPath $zipPath
    Write-Host "  [OK] ZIP created: $zipPath" -ForegroundColor Green
} else {
    Write-Host "[5/5] Skipped ZIP (use -CreateZip to enable)" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " Deploy package ready at: $OutputDir" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Edit $appOut\appsettings.json to configure SQL Server connection" -ForegroundColor White
Write-Host "  2. Run migration.sql or sql\00-full-setup.sql on SQL Server" -ForegroundColor White
Write-Host "  3. Launch $appOut\IRM.exe" -ForegroundColor White
