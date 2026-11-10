param(
    [Parameter(Mandatory = $true)][string]$SqlInstance,
    [Parameter(Mandatory = $true)][ValidatePattern('^[A-Za-z0-9_-]+$')][string]$Database,
    [Parameter(Mandatory = $true)][string]$EvidenceDirectory,
    [string]$BackupDirectory,
    [ValidatePattern('^[A-Za-z0-9_-]+$')][string]$RestoreTestDatabase,
    [switch]$ApproveBackupRestore
)

$ErrorActionPreference = 'Stop'
Import-Module SqlServer -ErrorAction Stop

$evidenceRoot = [System.IO.Path]::GetFullPath($EvidenceDirectory)
[System.IO.Directory]::CreateDirectory($evidenceRoot) | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'

function Invoke-IrmQuery([string]$DbName, [string]$Query) {
    Invoke-Sqlcmd -ServerInstance $SqlInstance -Database $DbName -Query $Query -QueryTimeout 0 -ErrorAction Stop
}

$inventorySql = @'
SELECT DB_NAME() DatabaseName, @@SERVERNAME ServerName, CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(100)) ProductVersion,
       CAST(SERVERPROPERTY('Edition') AS nvarchar(100)) Edition, GETDATE() CapturedAt;
SELECT t.name TableName, SUM(p.rows) RowCount
FROM sys.tables t LEFT JOIN sys.partitions p ON p.object_id=t.object_id AND p.index_id IN (0,1)
GROUP BY t.name ORDER BY t.name;
SELECT t.name TableName, c.column_id, c.name ColumnName, ty.name SqlType, c.max_length, c.is_nullable
FROM sys.tables t JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
ORDER BY t.name,c.column_id;
'@

$baseline = Invoke-IrmQuery $Database $inventorySql
$baseline | Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $evidenceRoot "baseline-$Database-$stamp.csv")

$checksumSql = @'
DECLARE @sql nvarchar(max)=N'';
SELECT @sql=@sql+N'SELECT N'''+REPLACE(t.name,'''','''''')+N''' TableName, COUNT_BIG(*) RowCount, CHECKSUM_AGG(BINARY_CHECKSUM(*)) DataChecksum FROM dbo.'+QUOTENAME(t.name)+N';'+CHAR(13)
FROM sys.tables t WHERE t.name IN (N'Accounts',N'Employees',N'Companies',N'Wards',N'Districts',N'Attach',N'Students');
EXEC sp_executesql @sql;
'@
$checksums = Invoke-IrmQuery $Database $checksumSql
$checksums | Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $evidenceRoot "checksums-before-$Database-$stamp.csv")

if (-not $ApproveBackupRestore) {
    Write-Host "Read-only baseline captured. Re-run with -ApproveBackupRestore, -BackupDirectory and -RestoreTestDatabase for the production gate."
    return
}
if ([string]::IsNullOrWhiteSpace($BackupDirectory) -or [string]::IsNullOrWhiteSpace($RestoreTestDatabase)) {
    throw 'BackupDirectory and RestoreTestDatabase are mandatory with -ApproveBackupRestore.'
}
if ($RestoreTestDatabase -eq $Database) { throw 'RestoreTestDatabase must differ from the source database.' }

$backupRoot = [System.IO.Path]::GetFullPath($BackupDirectory)
[System.IO.Directory]::CreateDirectory($backupRoot) | Out-Null
$backupFile = Join-Path $backupRoot "$Database-$stamp.bak"
$escapedBackup = $backupFile.Replace("'", "''")
$escapedDatabase = $Database.Replace(']', ']]')

Invoke-Sqlcmd -ServerInstance $SqlInstance -Database master -Query "BACKUP DATABASE [$escapedDatabase] TO DISK=N'$escapedBackup' WITH COPY_ONLY, CHECKSUM, INIT, STATS=10; RESTORE VERIFYONLY FROM DISK=N'$escapedBackup' WITH CHECKSUM;" -QueryTimeout 0 -ErrorAction Stop

$fileList = Invoke-Sqlcmd -ServerInstance $SqlInstance -Database master -Query "RESTORE FILELISTONLY FROM DISK=N'$escapedBackup';" -QueryTimeout 0
$dataRoot = (Invoke-Sqlcmd -ServerInstance $SqlInstance -Database master -Query "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000)) DataPath, CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(4000)) LogPath;").DataPath
$logRoot = (Invoke-Sqlcmd -ServerInstance $SqlInstance -Database master -Query "SELECT CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(4000)) LogPath;").LogPath
$moveClauses = @()
$dataIndex = 0
$logIndex = 0
foreach ($file in $fileList) {
    $logical = ([string]$file.LogicalName).Replace("'", "''")
    if ($file.Type -eq 'L') {
        $logIndex++
        $target = Join-Path $logRoot "$RestoreTestDatabase-$logIndex.ldf"
    } else {
        $dataIndex++
        $target = Join-Path $dataRoot "$RestoreTestDatabase-$dataIndex.mdf"
    }
    $moveClauses += "MOVE N'$logical' TO N'$($target.Replace("'", "''"))'"
}
$restoreSql = "RESTORE DATABASE [$($RestoreTestDatabase.Replace(']', ']]'))] FROM DISK=N'$escapedBackup' WITH " + ($moveClauses -join ', ') + ', RECOVERY, CHECKSUM, STATS=10;'
Invoke-Sqlcmd -ServerInstance $SqlInstance -Database master -Query $restoreSql -QueryTimeout 0 -ErrorAction Stop

$restoredChecksums = Invoke-IrmQuery $RestoreTestDatabase $checksumSql
$restoredChecksums | Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $evidenceRoot "checksums-restored-$RestoreTestDatabase-$stamp.csv")
$comparison = foreach ($source in $checksums) {
    $restored = $restoredChecksums | Where-Object TableName -eq $source.TableName | Select-Object -First 1
    [pscustomobject]@{ TableName=$source.TableName; SourceRows=$source.RowCount; RestoredRows=$restored.RowCount;
        SourceChecksum=$source.DataChecksum; RestoredChecksum=$restored.DataChecksum;
        Match=($source.RowCount -eq $restored.RowCount -and $source.DataChecksum -eq $restored.DataChecksum) }
}
$comparison | Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $evidenceRoot "restore-verification-$stamp.csv")
if ($comparison.Match -contains $false) { throw 'Restore verification failed: row count or checksum differs.' }
Get-FileHash -Algorithm SHA256 $backupFile | Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $evidenceRoot "backup-hash-$stamp.csv")
Write-Host "Gate passed on restored database [$RestoreTestDatabase]. The source database was not migrated."
