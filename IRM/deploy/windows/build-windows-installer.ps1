[CmdletBinding()]
param(
    [string]$VendorDirectory = "",
    [string]$OutputDirectory = "",
    [string]$InnoCompiler = "",
    [string]$SignTool = "",
    [string]$SigningCertificateThumbprint = $env:IRM_SIGNING_CERT_THUMBPRINT,
    [ValidateSet('CurrentUser','LocalMachine')][string]$SigningCertificateStore = 'CurrentUser',
    [string]$TimestampUrl = $env:IRM_SIGNING_TIMESTAMP_URL,
    [switch]$AllowUnsigned
)
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($VendorDirectory)) {
    $VendorDirectory = Join-Path $PSScriptRoot "..\vendor\windows"
}
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $buildStamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
    $OutputDirectory = Join-Path $PSScriptRoot "..\..\..\BAN-GIAO\04-CHO-NGHIEM-THU\$buildStamp-v1.0.2"
}
$repo = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$sourceCommit = & git -C $repo rev-parse --verify HEAD
if ($LASTEXITCODE -ne 0) { throw 'Khong xac dinh duoc Git commit cua ma nguon.' }
$sourceChanges = & git -C $repo status --porcelain --untracked-files=normal
if ($LASTEXITCODE -ne 0) { throw 'Khong kiem tra duoc trang thai Git.' }
if ($sourceChanges) { throw 'Hay commit day du cac thay doi ma nguon truoc khi dong goi ban giao.' }
$sourceCommit = $sourceCommit.Trim()
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

if ([string]::IsNullOrWhiteSpace($SigningCertificateThumbprint)) {
    if (-not $AllowUnsigned) {
        throw 'Bản phát hành Windows bắt buộc phải ký số. Cấu hình IRM_SIGNING_CERT_THUMBPRINT và IRM_SIGNING_TIMESTAMP_URL, hoặc chỉ dùng -AllowUnsigned cho build thử nội bộ.'
    }
} else {
    if ([string]::IsNullOrWhiteSpace($TimestampUrl)) {
        throw 'Thiếu URL máy chủ RFC 3161 timestamp trong IRM_SIGNING_TIMESTAMP_URL hoặc -TimestampUrl.'
    }
    if ([string]::IsNullOrWhiteSpace($SignTool)) {
        $SignTool = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\*\x64\signtool.exe" -File -ErrorAction SilentlyContinue |
            Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
    }
    if (-not (Test-Path -LiteralPath $SignTool -PathType Leaf)) { throw "Không tìm thấy signtool.exe: $SignTool" }

    $certificatePath = "Cert:\$SigningCertificateStore\My\$($SigningCertificateThumbprint.Replace(' ',''))"
    $certificate = Get-Item -LiteralPath $certificatePath -ErrorAction SilentlyContinue
    if (-not $certificate -or -not $certificate.HasPrivateKey) {
        throw "Không tìm thấy chứng thư ký mã có private key: $certificatePath"
    }
    $codeSigningOid = '1.3.6.1.5.5.7.3.3'
    if (-not ($certificate.EnhancedKeyUsageList | Where-Object { $_.ObjectId.Value -eq $codeSigningOid })) {
        throw "Chứng thư không có mục đích Code Signing: $certificatePath"
    }
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
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "windows-environment.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "diagnose-environment.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "installer-ui.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "installer-theme.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "uninstall.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "restore.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "repair.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "backup.ps1") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $repo "deploy\release-manifest.json") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $repo "deploy\database\IRM-v1.0.1-schema.sql") -Destination $stage -Force
Copy-Item -LiteralPath (Join-Path $repo "docs\INSTALLATION-v1.0.2.md") -Destination (Join-Path $stage "HUONG-DAN-CAI-DAT.md") -Force

[ordered]@{
    commit = $sourceCommit
    dirty = $false
    builtAtUtc = [DateTime]::UtcNow.ToString('o')
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $stage 'SOURCE-REVISION.json') -Encoding UTF8

dotnet list (Join-Path $repo "IRM\IRM.csproj") package --include-transitive --format json | `
    Set-Content -LiteralPath (Join-Path $stage "SBOM-dotnet.json") -Encoding UTF8
Get-ChildItem -LiteralPath $stage -Recurse -File | Get-FileHash -Algorithm SHA256 | ForEach-Object {
    "{0}  {1}" -f $_.Hash, $_.Path.Substring($stage.Length + 1)
} | Set-Content -LiteralPath (Join-Path $stage "SHA256SUMS.txt") -Encoding ASCII

& $InnoCompiler "/DStageDir=$stage" "/DOutputDir=$OutputDirectory" (Join-Path $PSScriptRoot "irm.iss")
if ($LASTEXITCODE -ne 0) { throw "Biên dịch Inno Setup thất bại" }

$artifact = Join-Path $OutputDirectory $manifest.windows.artifact
if ([string]::IsNullOrWhiteSpace($SigningCertificateThumbprint)) {
    Write-Warning 'Artifact chưa ký số, chỉ dùng để kiểm thử nội bộ; Windows SmartScreen có thể chặn file này.'
} else {
    $signArguments = @('sign','/sha1',$SigningCertificateThumbprint.Replace(' ',''),'/s','My','/fd','SHA256','/tr',$TimestampUrl,'/td','SHA256','/v')
    if ($SigningCertificateStore -eq 'LocalMachine') { $signArguments += '/sm' }
    $signArguments += $artifact
    $signProcess = Start-Process -FilePath $SignTool -ArgumentList $signArguments -Wait -PassThru -NoNewWindow
    if ($signProcess.ExitCode -ne 0) { throw "Ký số bộ cài thất bại: $($signProcess.ExitCode)" }

    $verifyProcess = Start-Process -FilePath $SignTool -ArgumentList @('verify','/pa','/all','/v',$artifact) -Wait -PassThru -NoNewWindow
    if ($verifyProcess.ExitCode -ne 0) { throw "Xác minh chữ ký bộ cài thất bại: $($verifyProcess.ExitCode)" }
    $artifactSignature = Get-AuthenticodeSignature -LiteralPath $artifact
    if ($artifactSignature.Status -ne 'Valid') { throw "Chữ ký bộ cài không hợp lệ: $($artifactSignature.StatusMessage)" }
    Write-Host "Đã ký bộ cài bởi $($artifactSignature.SignerCertificate.Subject)" -ForegroundColor Green
}
$artifactHash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash
"$artifactHash  $($manifest.windows.artifact)" | Set-Content -LiteralPath "$artifact.sha256" -Encoding ASCII
Get-FileHash -LiteralPath $artifact -Algorithm SHA256 | Format-List
