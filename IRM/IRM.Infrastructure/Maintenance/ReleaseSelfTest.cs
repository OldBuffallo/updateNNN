using IRM.Services;
using IRM.Data;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Maintenance;

public static class ReleaseSelfTest
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var scoped = scope.ServiceProvider;
        var username = configuration["SelfTest:AdminUsername"]
            ?? throw new InvalidOperationException("Thiếu SelfTest:AdminUsername.");
        var password = configuration["SelfTest:AdminPassword"]
            ?? throw new InvalidOperationException("Thiếu SelfTest:AdminPassword.");

        var db = scoped.GetRequiredService<IrmDbContext>();
        if (!await db.Database.CanConnectAsync(cancellationToken))
            throw new InvalidOperationException("Self-test không thể đọc database IRM.");
        if (!await db.Accounts.AnyAsync(account => account.Delete_flag == 0, cancellationToken)
            || !await db.Fields.AnyAsync(field => field.Delete_flag == 0, cancellationToken)
            || !await db.Careers.AnyAsync(career => career.Delete_flag == 0, cancellationToken)
            || !await db.Nationality.AnyAsync(nationality => nationality.Delete_flag == 0, cancellationToken)
            || !await db.Companies.AnyAsync(company => company.Delete_flag == 0, cancellationToken))
        {
            throw new InvalidOperationException("Self-test phát hiện database chưa có đủ dữ liệu khởi tạo bắt buộc.");
        }

        var principal = await scoped.GetRequiredService<SqlAuthStore>()
            .AuthenticateWebAsync(username, password, "127.0.0.1/self-test", cancellationToken);
        if (principal?.Identity?.IsAuthenticated != true || !principal.IsInRole(IrmRoles.Admin))
            throw new InvalidOperationException("Self-test đăng nhập quản trị thất bại.");

        var factory = scoped.GetRequiredService<IDbContextFactory<IrmDbContext>>();
        var report = await new SqlExportStore(factory).ExportCompaniesAsync();
        if (report.Length < 100)
            throw new InvalidOperationException("Self-test xuất báo cáo Excel thất bại.");

        var storageRoot = configuration["FileStorage:Root"]
            ?? throw new InvalidOperationException("Thiếu FileStorage:Root.");
        Directory.CreateDirectory(storageRoot);
        scoped.GetRequiredService<IStorageCapacityGuard>().EnsureCanWrite(storageRoot);
        var testFile = Path.Combine(storageRoot, $".irm-self-test-{Guid.NewGuid():N}.txt");
        try
        {
            await File.WriteAllTextAsync(testFile, "IRM antivirus self-test", cancellationToken);
            await scoped.GetRequiredService<IMalwareScanner>().ScanAsync(testFile, cancellationToken);
        }
        finally
        {
            File.Delete(testFile);
        }
    }
}
