using Microsoft.Data.SqlClient;

namespace IRM.Data;

public static class DatabaseProvisioner
{
    public const string DatabaseName = "IRM";
    public const string ApplicationLogin = "irm_app";
    public const string DbaLogin = "irm_dba";

    public static string ForIrmDatabase(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = DatabaseName,
            MultipleActiveResultSets = true
        };
        return builder.ConnectionString;
    }

    public static async Task PrepareAsync(string adminConnectionString, string? appPassword, string? dbaPassword,
        CancellationToken cancellationToken = default)
    {
        ValidateProvisioningPassword(appPassword, "ứng dụng");
        ValidateProvisioningPassword(dbaPassword, "phục hồi database");

        var masterBuilder = new SqlConnectionStringBuilder(adminConnectionString) { InitialCatalog = "master" };
        await using var connection = new SqlConnection(masterBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var versionCommand = new SqlCommand("SELECT CONVERT(varchar(30), SERVERPROPERTY('ProductVersion'))", connection))
        {
            var version = Convert.ToString(await versionCommand.ExecuteScalarAsync(cancellationToken));
            if (!string.Equals(version, "16.0.4295.3", StringComparison.Ordinal))
                throw new InvalidOperationException($"SQL Server phải đúng CU27 build 16.0.4295.3; máy hiện tại là {version ?? "không xác định"}.");
        }
        await using (var editionCommand = new SqlCommand("SELECT CONVERT(nvarchar(128), SERVERPROPERTY('Edition'))", connection))
        {
            var edition = Convert.ToString(await editionCommand.ExecuteScalarAsync(cancellationToken));
            if (edition?.Contains("Express", StringComparison.OrdinalIgnoreCase) != true)
                throw new InvalidOperationException($"SQL Server phải là Express Edition; máy hiện tại là {edition ?? "không xác định"}.");
        }

        var sql = $"""
            IF DB_ID(N'{DatabaseName}') IS NULL CREATE DATABASE [{DatabaseName}];
            ALTER DATABASE [{DatabaseName}] SET AUTO_CLOSE OFF;
            ALTER DATABASE [{DatabaseName}] SET RECOVERY SIMPLE;

            IF SUSER_ID(N'{ApplicationLogin}') IS NULL
                CREATE LOGIN [{ApplicationLogin}] WITH PASSWORD = N'{EscapeSqlLiteral(appPassword!)}', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;
            ELSE
                ALTER LOGIN [{ApplicationLogin}] WITH PASSWORD = N'{EscapeSqlLiteral(appPassword!)}';

            IF SUSER_ID(N'{DbaLogin}') IS NULL
                CREATE LOGIN [{DbaLogin}] WITH PASSWORD = N'{EscapeSqlLiteral(dbaPassword!)}', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;
            ELSE
                ALTER LOGIN [{DbaLogin}] WITH PASSWORD = N'{EscapeSqlLiteral(dbaPassword!)}';

            EXEC sp_configure 'show advanced options', 1;
            RECONFIGURE;
            EXEC sp_configure 'max server memory (MB)', 1024;
            RECONFIGURE;
            """;
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = 120 };
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public static async Task FinalizeAsync(string adminConnectionString, CancellationToken cancellationToken = default)
    {
        var masterBuilder = new SqlConnectionStringBuilder(adminConnectionString) { InitialCatalog = "master" };
        await using var connection = new SqlConnection(masterBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        var sql = $"""
            USE [{DatabaseName}];
            IF DATABASE_PRINCIPAL_ID(N'{ApplicationLogin}') IS NULL CREATE USER [{ApplicationLogin}] FOR LOGIN [{ApplicationLogin}];
            IF DATABASE_PRINCIPAL_ID(N'{DbaLogin}') IS NULL CREATE USER [{DbaLogin}] FOR LOGIN [{DbaLogin}];
            IF IS_ROLEMEMBER(N'db_datareader', N'{ApplicationLogin}') <> 1 ALTER ROLE [db_datareader] ADD MEMBER [{ApplicationLogin}];
            IF IS_ROLEMEMBER(N'db_datawriter', N'{ApplicationLogin}') <> 1 ALTER ROLE [db_datawriter] ADD MEMBER [{ApplicationLogin}];
            IF IS_ROLEMEMBER(N'db_backupoperator', N'{ApplicationLogin}') <> 1 ALTER ROLE [db_backupoperator] ADD MEMBER [{ApplicationLogin}];
            GRANT EXECUTE TO [{ApplicationLogin}];
            IF IS_ROLEMEMBER(N'db_owner', N'{DbaLogin}') <> 1 ALTER ROLE [db_owner] ADD MEMBER [{DbaLogin}];
            USE [master];
            IF IS_SRVROLEMEMBER(N'sysadmin', N'{DbaLogin}') <> 1 ALTER SERVER ROLE [sysadmin] ADD MEMBER [{DbaLogin}];
            IF SUSER_ID(N'sa') IS NOT NULL ALTER LOGIN [sa] DISABLE;
            """;
        await using var command = new SqlCommand(sql, connection) { CommandTimeout = 120 };
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void ValidateProvisioningPassword(string? password, string label)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 16)
            throw new InvalidOperationException($"Mật khẩu {label} phải có ít nhất 16 ký tự.");
    }

    private static string EscapeSqlLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}
