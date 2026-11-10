[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$VpsHost,
    [string]$VpsUser = 'ubuntu',
    [string]$KeyPath,
    [string]$AdminUser = 'admin',
    [SecureString]$AdminPassword,
    [SecureString]$DbaPassword,
    [SecureString]$SqlPassword
)

$ErrorActionPreference = 'Stop'
$version = '1.0.2'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$outputRoot = Join-Path $repoRoot 'build-output'
$imageTar = Join-Path $outputRoot "irm-$version.tar"
$composePath = Join-Path $repoRoot 'docker-compose.openship.yml'
$remoteRoot = '/opt/irm'

function Convert-SecureStringToPlain([SecureString]$Value) {
    $ptr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try { return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr) }
    finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr) }
}

function New-RandomSecret {
    $bytes = New-Object byte[] 32
    [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    return [Convert]::ToBase64String($bytes)
}

function Invoke-Native([string]$FileName, [string[]]$Arguments) {
    & $FileName @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$FileName thất bại với mã $LASTEXITCODE." }
}

if (-not $AdminPassword) { $AdminPassword = Read-Host 'Mật khẩu quản trị IRM (ít nhất 12 ký tự)' -AsSecureString }
if (-not $DbaPassword) { $DbaPassword = ConvertTo-SecureString (New-RandomSecret) -AsPlainText -Force }
if (-not $SqlPassword) {
    if ($env:IRM_SQL_SA_PASSWORD) { $SqlPassword = ConvertTo-SecureString $env:IRM_SQL_SA_PASSWORD -AsPlainText -Force }
    else { $SqlPassword = ConvertTo-SecureString (New-RandomSecret) -AsPlainText -Force }
}

$adminPlain = Convert-SecureStringToPlain $AdminPassword
$dbaPlain = Convert-SecureStringToPlain $DbaPassword
$sqlPlain = Convert-SecureStringToPlain $SqlPassword
if ($adminPlain.Length -lt 12 -or $dbaPlain.Length -lt 16 -or $sqlPlain.Length -lt 16) {
    throw 'Một hoặc nhiều mật khẩu chưa đạt độ dài tối thiểu.'
}
if (($adminPlain + $dbaPlain + $sqlPlain) -match "[`r`n]") { throw 'Mật khẩu không được chứa ký tự xuống dòng.' }
$appPlain = New-RandomSecret
if ($sqlPlain -notmatch '^[A-Za-z0-9+/=._-]+$') {
    throw 'Mật khẩu SQL nội bộ chỉ được dùng chữ, số và các ký tự + / = . _ - để tương thích Docker Compose.'
}

$sshArgs = @()
if ($KeyPath) { $sshArgs += @('-i',(Resolve-Path -LiteralPath $KeyPath).Path) }
$target = "${VpsUser}@${VpsHost}"

New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
Write-Host '[1/6] Build và kiểm tra Docker image...' -ForegroundColor Cyan
Invoke-Native docker @('build','--pull','-t',"irm:$version",'-f',(Join-Path $repoRoot 'Dockerfile'),$repoRoot)
Invoke-Native docker @('image','inspect',"irm:$version")
Invoke-Native docker @('save','-o',$imageTar,"irm:$version")

$tempRoot = Join-Path ([IO.Path]::GetTempPath()) ("irm-vps-{0}" -f [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null
try {
    $envFile = Join-Path $tempRoot '.env'
    $bootstrapFile = Join-Path $tempRoot 'bootstrap.env'
    $lines = @(
        "SQL_SA_PASSWORD=$sqlPlain",
        "IRM_APP_PASSWORD=$appPlain"
    )
    [IO.File]::WriteAllLines($envFile,$lines,(New-Object Text.UTF8Encoding($false)))
    $bootstrapLines = @(
        'IRM_DBA_PASSWORD_B64=' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($dbaPlain)),
        'IRM_ADMIN_USERNAME_B64=' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($AdminUser)),
        'IRM_ADMIN_PASSWORD_B64=' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($adminPlain))
    )
    [IO.File]::WriteAllLines($bootstrapFile,$bootstrapLines,(New-Object Text.UTF8Encoding($false)))

    Write-Host '[2/6] Chuẩn bị thư mục và snapshot rollback trên VPS...' -ForegroundColor Cyan
    $prepare = @'
set -eu
sudo install -d -m 0750 /opt/irm /opt/irm/releases /opt/irm/backups
stamp=$(date -u +%Y%m%dT%H%M%SZ)
if [ -f /opt/irm/docker-compose.yml ]; then sudo cp /opt/irm/docker-compose.yml "/opt/irm/backups/docker-compose-$stamp.yml"; fi
if [ -f /opt/irm/.env ]; then sudo cp /opt/irm/.env "/opt/irm/backups/env-$stamp"; fi
if [ -f /opt/irm/bootstrap.env ]; then sudo cp /opt/irm/bootstrap.env "/opt/irm/backups/bootstrap-$stamp.env"; fi
if sudo docker image inspect irm:1.0.2 >/dev/null 2>&1; then sudo docker image tag irm:1.0.2 "irm:rollback-$stamp"; fi
echo "$stamp" | sudo tee /opt/irm/backups/latest-stamp >/dev/null
'@
    Invoke-Native ssh ($sshArgs + @($target,$prepare))

    Write-Host '[3/6] Chuyển image, compose và secrets...' -ForegroundColor Cyan
    Invoke-Native scp ($sshArgs + @($imageTar,"${target}:/tmp/irm-$version.tar"))
    Invoke-Native scp ($sshArgs + @($composePath,"${target}:/tmp/irm-compose.yml"))
    Invoke-Native scp ($sshArgs + @($envFile,"${target}:/tmp/irm.env"))
    Invoke-Native scp ($sshArgs + @($bootstrapFile,"${target}:/tmp/irm-bootstrap.env"))

    Write-Host '[4/6] Nạp image và bootstrap database...' -ForegroundColor Cyan
    $deploy = @'
set -eu
sudo docker load -i /tmp/irm-1.0.2.tar
sudo mv /tmp/irm-compose.yml /opt/irm/docker-compose.yml
sudo mv /tmp/irm.env /opt/irm/.env
sudo mv /tmp/irm-bootstrap.env /opt/irm/bootstrap.env
sudo chmod 0600 /opt/irm/.env /opt/irm/bootstrap.env
sudo rm -f /tmp/irm-1.0.2.tar
cd /opt/irm
sudo docker compose up -d db
for i in $(seq 1 60); do
  if sudo docker compose exec -T db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$(sudo sed -n 's/^SQL_SA_PASSWORD=//p' .env)" -Q 'SELECT 1' -C -b >/dev/null 2>&1; then break; fi
  [ "$i" -eq 60 ] && exit 31
  sleep 2
done
sql_sa=$(sudo sed -n 's/^SQL_SA_PASSWORD=//p' .env)
app_pwd=$(sudo sed -n 's/^IRM_APP_PASSWORD=//p' .env)
dba_pwd=$(sudo sed -n 's/^IRM_DBA_PASSWORD_B64=//p' bootstrap.env | base64 -d)
admin_user=$(sudo sed -n 's/^IRM_ADMIN_USERNAME_B64=//p' bootstrap.env | base64 -d)
admin_pwd=$(sudo sed -n 's/^IRM_ADMIN_PASSWORD_B64=//p' bootstrap.env | base64 -d)
sudo docker compose run --rm \
  -e "Provisioning__AdminConnectionString=Server=db;Database=master;User Id=sa;Password=$sql_sa;Encrypt=True;TrustServerCertificate=True" \
  -e "Provisioning__AppPassword=$app_pwd" \
  -e "Provisioning__DbaPassword=$dba_pwd" \
  -e "Bootstrap__AdminUsername=$admin_user" \
  -e "Bootstrap__AdminPassword=$admin_pwd" \
  app --bootstrap --provision-database
sudo docker compose up -d app
'@
    Invoke-Native ssh ($sshArgs + @($target,$deploy))

    Write-Host '[5/6] Chờ health check...' -ForegroundColor Cyan
    $health = @'
set -eu
for i in $(seq 1 60); do
  if curl -fsS http://127.0.0.1:5050/health/ready | grep -q '"status":"ready"'; then exit 0; fi
  sleep 2
done
exit 41
'@
    try { Invoke-Native ssh ($sshArgs + @($target,$health)) }
    catch {
        Write-Warning 'Health check thất bại; đang rollback container và cấu hình trước đó.'
        $rollback = @'
set -eu
cd /opt/irm
stamp=$(cat backups/latest-stamp)
sudo docker compose down --timeout 30 || true
if sudo docker image inspect "irm:rollback-$stamp" >/dev/null 2>&1; then sudo docker image tag "irm:rollback-$stamp" irm:1.0.2; fi
if [ -f "backups/docker-compose-$stamp.yml" ]; then sudo cp "backups/docker-compose-$stamp.yml" docker-compose.yml; fi
if [ -f "backups/env-$stamp" ]; then sudo cp "backups/env-$stamp" .env; sudo chmod 0600 .env; fi
if [ -f "backups/bootstrap-$stamp.env" ]; then sudo cp "backups/bootstrap-$stamp.env" bootstrap.env; sudo chmod 0600 bootstrap.env; fi
sudo docker compose up -d
'@
        Invoke-Native ssh ($sshArgs + @($target,$rollback))
        throw
    }

    Write-Host '[6/6] Deployment hoàn tất và health check đạt.' -ForegroundColor Green
}
finally {
    $adminPlain = $null; $dbaPlain = $null; $sqlPlain = $null; $appPlain = $null
    if (Test-Path -LiteralPath $tempRoot) { Remove-Item -LiteralPath $tempRoot -Recurse -Force }
}
