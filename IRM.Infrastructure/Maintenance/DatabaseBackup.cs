using System.IO.Compression;
using Microsoft.Data.SqlClient;

namespace IRM.Data;

public static class DatabaseBackup
{
    private const int MaximumBackupCount = 7;
    private const long MaximumBackupBytes = 2L * 1024 * 1024 * 1024;

    public static async Task<string> CreateAndRotateAsync(string connectionString, string backupDirectory,
        CancellationToken cancellationToken = default)
    {
        var fullDirectory = Path.GetFullPath(backupDirectory);
        Directory.CreateDirectory(fullDirectory);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(fullDirectory, $"IRM_{timestamp}.bak");
        var archivePath = backupPath + ".gz";

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        var escapedPath = backupPath.Replace("'", "''", StringComparison.Ordinal);
        await using var command = new SqlCommand(
            $"BACKUP DATABASE [IRM] TO DISK = N'{escapedPath}' WITH COPY_ONLY, CHECKSUM, INIT;",
            connection) { CommandTimeout = 600 };
        await command.ExecuteNonQueryAsync(cancellationToken);

        try
        {
            await using var input = File.OpenRead(backupPath);
            await using var output = File.Create(archivePath);
            await using var gzip = new GZipStream(output, CompressionLevel.SmallestSize);
            await input.CopyToAsync(gzip, cancellationToken);
        }
        catch
        {
            File.Delete(archivePath);
            throw;
        }
        finally
        {
            File.Delete(backupPath);
        }

        Rotate(fullDirectory);
        return archivePath;
    }

    public static async Task RestoreAsync(string connectionString, string archivePath, string backupDirectory,
        CancellationToken cancellationToken = default)
    {
        var fullArchivePath = Path.GetFullPath(archivePath);
        if (!File.Exists(fullArchivePath) || !fullArchivePath.EndsWith(".bak.gz", StringComparison.OrdinalIgnoreCase))
            throw new FileNotFoundException("Bản sao lưu phải là tệp IRM_*.bak.gz hợp lệ.", fullArchivePath);

        var fullDirectory = Path.GetFullPath(backupDirectory);
        Directory.CreateDirectory(fullDirectory);
        var restorePath = Path.Combine(fullDirectory, $"IRM_restore_{Guid.NewGuid():N}.bak");
        try
        {
            await using (var input = File.OpenRead(fullArchivePath))
            await using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            await using (var output = File.Create(restorePath))
                await gzip.CopyToAsync(output, cancellationToken);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            var escapedPath = restorePath.Replace("'", "''", StringComparison.Ordinal);
            await using var command = new SqlCommand($"""
                ALTER DATABASE [IRM] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [IRM] FROM DISK = N'{escapedPath}' WITH REPLACE, CHECKSUM;
                ALTER DATABASE [IRM] SET MULTI_USER;
                """, connection) { CommandTimeout = 900 };
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch
        {
            await TryReturnDatabaseToMultiUserAsync(connectionString, cancellationToken);
            throw;
        }
        finally
        {
            File.Delete(restorePath);
        }
    }

    internal static void Rotate(string backupDirectory)
    {
        var backups = new DirectoryInfo(backupDirectory)
            .GetFiles("IRM_*.bak.gz", SearchOption.TopDirectoryOnly)
            .OrderByDescending(x => x.CreationTimeUtc)
            .ToList();
        foreach (var extra in backups.Skip(MaximumBackupCount).ToList())
        {
            extra.Delete();
            backups.Remove(extra);
        }

        while (backups.Count > 1 && backups.Sum(x => x.Exists ? x.Length : 0) > MaximumBackupBytes)
        {
            var oldest = backups[^1];
            oldest.Delete();
            backups.RemoveAt(backups.Count - 1);
        }
    }

    private static async Task TryReturnDatabaseToMultiUserAsync(string connectionString,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(
                "IF DB_ID(N'IRM') IS NOT NULL ALTER DATABASE [IRM] SET MULTI_USER;", connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch
        {
            // Preserve the original restore error. The repair tool can retry SET MULTI_USER.
        }
    }
}
