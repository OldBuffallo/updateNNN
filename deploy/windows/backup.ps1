$ErrorActionPreference = 'Stop'
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$appRoot = Join-Path $env:ProgramData 'IRM\app'
& (Join-Path $appRoot 'IRM.exe') --backup
if ($LASTEXITCODE -ne 0) { throw "Backup IRM thất bại (mã $LASTEXITCODE)." }
