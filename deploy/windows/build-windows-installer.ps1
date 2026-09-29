[CmdletBinding()]
param(
    [string]$VendorDirectory = (Join-Path $PSScriptRoot "..\vendor\windows"),
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "..\..\build-output\v1.0.2"),
    [string]$InnoCompiler = ""
)
$ErrorActionPreference = "Stop"
$repo = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
$VendorDirectory = [IO.Path]::GetFullPath($VendorDirectory)
$manifest = Get-Content (Join-Path $repo "deploy\release-manifest.json") -Raw | ConvertFrom-Json
$stage = Join-Path $OutputDirectory "windows-stage"
$appStage = Join-Path $stage "app"
$sqlBase = Join-Path $VendorDirectory $manifest.windows.sqlBaseMedia
$sqlCu = Join-Path $VendorDirectory $manifest.windows.sqlCuMedia

if ([string]::IsNullOrWhiteSpace($InnoCompiler)) {
    $InnoCompiler = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
    ) | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
}

foreach ($file in @($sqlBase, $sqlCu)) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw "Thiếu bộ cài Microsoft đã khóa: $file" }
    $signature = Get-AuthenticodeSignature -LiteralPath $file
    if ($signature.Status -ne "Valid" -or $signature.SignerCertificate.Subject -notmatch "Microsoft") {
        throw "Chữ ký Microsoft không hợp lệ: $file"
    }
}
$actualBaseHash = (Get-FileHash -LiteralPath $sqlBase -Algorithm SHA256).Hash
if ($actualBaseHash -ne $manifest.windows.sqlBaseSha256) {
    throw "SHA-256 của SQL Server Express base media không đúng: $actualBaseHash"
}
$actualCuHash = (Get-FileHash -LiteralPath $sqlCu -Algorithm SHA256).Hash
if ($actualCuHash -ne $manifest.windows.sqlCuSha256) {
    throw "SHA-256 của SQL Server CU27 không đúng: $actualCuHash"
}
if (-not (Test-Path -LiteralPath $InnoCompiler -PathType Leaf)) { throw "Không tìm thấy Inno Setup 6: $InnoCompiler" }

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$resolvedOutput = [IO.Path]::GetFullPath($OutputDirectory).TrimEnd('\')
$resolvedStage = [IO.Path]::GetFullPath($stage)
if (-not $resolvedStage.StartsWith($resolvedOutput + '\',[StringComparison]::OrdinalIgnoreCase)) {
    throw "Thư mục stage không nằm trong output: $resolvedStage"
}
if (Test-Path -LiteralPath $resolvedStage) { Remove-Item -LiteralPath $resolvedStage -Recurse -Force }
New-Item -ItemType Directory -Path $appStage -Force | Out-Null
dotnet publish (Join-Path $repo "IRM\IRM.csproj") -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=false -p:DebugType=None -p:DebugSymbols=false -o $appStage
if ($LASTEXITCODE -ne 0) { throw "dotnet publish thất bại" }

Copy-Item -LiteralPath $sqlBase -Destination (Join-Path $stage "sql-base.exe") -Force
Copy-Item -LiteralPath $sqlCu -Destination (Join-Path $stage "sql-cu.exe") -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "install.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "installer-ui.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "uninstall.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "restore.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "repair.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "backup.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $repo "deploy\release-manifest.json") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $repo "deploy\database\IRM-v1.0.1-schema.sql") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $repo "docs\INSTALLATION-v1.0.2.md") -Destination (Join-Path $stage "HUONG-DAN-CAI-DAT.md") -Force

dotnet list (Join-Path $repo "IRM\IRM.csproj") package --include-transitive --format json | `
    Set-Content -LiteralPath (Join-Path $stage "SBOM-dotnet.json") -Encoding UTF8
Get-ChildItem -LiteralPath $stage -Recurse -File | Get-FileHash -Algorithm SHA256 | ForEach-Object {
    "{0}  {1}" -f $_.Hash, $_.Path.Substring($stage.Length + 1)
} | Set-Content -LiteralPath (Join-Path $stage "SHA256SUMS.txt") -Encoding ASCII

& $InnoCompiler "/DStageDir=$stage" "/DOutputDir=$OutputDirectory" (Join-Path $PSScriptRoot "irm.iss")
if ($LASTEXITCODE -ne 0) { throw "Biên dịch Inno Setup thất bại" }

$artifact = Join-Path $OutputDirectory $manifest.windows.artifact
$artifactHash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash
"$artifactHash  $($manifest.windows.artifact)" | Set-Content -LiteralPath "$artifact.sha256" -Encoding ASCII
Get-FileHash -LiteralPath $artifact -Algorithm SHA256 | Format-List
