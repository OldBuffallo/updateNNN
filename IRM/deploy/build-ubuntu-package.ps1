<# ============================================================
   build-ubuntu-package.ps1 — Đóng gói bộ cài Offline cho Ubuntu 20.04/22.04 LTS
   Tạo file nén IRM-v1.0.2-ubuntu-installer.tar.gz hoặc .zip
   ============================================================ #>

param(
    [string]$OutputDir = "",
    [switch]$BuildImageTar
)

$ErrorActionPreference = "Stop"
$IRM_VERSION = "1.0.2"
$RepoRoot = Join-Path $PSScriptRoot ".." | Resolve-Path
$InstallerSrc = Join-Path $PSScriptRoot "ubuntu-installer"

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $RepoRoot "IRM-v$IRM_VERSION-ubuntu-installer"
}

Write-Host ""
Write-Host "  ╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "  ║  Build Ubuntu Package — IRM v$IRM_VERSION               ║" -ForegroundColor Cyan
Write-Host "  ╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

if (Test-Path $OutputDir) { Remove-Item $OutputDir -Recurse -Force }
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# ── Copy scripts and configs ──
Write-Host "[1/4] Copying installer scripts and compose configs..." -ForegroundColor Yellow
Copy-Item "$InstallerSrc\install-irm.sh" $OutputDir -Force
Copy-Item "$InstallerSrc\docker-compose.yml" $OutputDir -Force
Copy-Item "$InstallerSrc\HUONG-DAN-CAI-DAT.md" $OutputDir -Force
Copy-Item "$RepoRoot\Dockerfile" $OutputDir -Force

# Create convenient run script
@"
#!/bin/bash
sudo bash install-irm.sh
"@ | Set-Content (Join-Path $OutputDir "CAI-DAT.sh") -NoNewline

Write-Host "  ✅ Scripts and configs copied" -ForegroundColor Green

# ── Publish app for Linux if image not bundled ──
Write-Host "[2/4] Publishing application binaries for Linux..." -ForegroundColor Yellow
$appDir = Join-Path $OutputDir "app"
dotnet publish "$RepoRoot\IRM\IRM.csproj" -c Release -o "$appDir/publish" /p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) { throw "Dotnet publish failed!" }
Copy-Item "$RepoRoot\IRM\IRM.csproj" "$appDir/" -Force
Write-Host "  ✅ Application ready for offline container build" -ForegroundColor Green

# ── Check for prebuilt Docker image ──
Write-Host "[3/4] Checking offline Docker image..." -ForegroundColor Yellow
$cachedImage = Join-Path $RepoRoot "build-output\irm-1.0.2.tar.gz"
if (Test-Path $cachedImage) {
    Copy-Item $cachedImage (Join-Path $OutputDir "irm-image.tar.gz") -Force
    Write-Host "  ✅ Offline Docker image bundled (irm-image.tar.gz)" -ForegroundColor Green
} else {
    Write-Host "  ℹ️ Image tar not found in build-output, installer will use local Dockerfile & app folder" -ForegroundColor DarkGray
}

# ── Package into ZIP / TAR ──
Write-Host "[4/4] Creating distribution archive..." -ForegroundColor Yellow
$zipPath = Join-Path (Split-Path $OutputDir) "IRM-v$IRM_VERSION-ubuntu-installer.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$OutputDir\*" -DestinationPath $zipPath -CompressionLevel Optimal
$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)

Write-Host ""
Write-Host "  ╔══════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "  ║  ✅ Ubuntu Offline Package Ready!                ║" -ForegroundColor Green
Write-Host "  ╠══════════════════════════════════════════════════╣" -ForegroundColor Green
Write-Host "  ║  Folder: $OutputDir" -ForegroundColor Green
Write-Host "  ║  ZIP:    $zipPath ($zipSize MB)" -ForegroundColor Green
Write-Host "  ╚══════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
