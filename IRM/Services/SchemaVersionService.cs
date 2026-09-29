using IRM.Data;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class SchemaVersionService : ISchemaVersionService
{
    public const string RequiredVersion = "1.0.1";
    public const string RequiredChecksum = "B737B48CE90D924B78303E3E22C9B5D465E880E37B0F6C93A7FA15809CA46CA2";
    private readonly IrmDbContext _db;
    public SchemaVersionService(IrmDbContext db) => _db = db;

    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _db.SchemaVersions.AsNoTracking().AnyAsync(
                    x => x.Version == RequiredVersion && x.Checksum == RequiredChecksum, cancellationToken))
                throw new InvalidOperationException($"Database chưa có schema {RequiredVersion} với checksum được phê duyệt. Hãy chạy migration trước khi khởi động.");
        }
        catch (Exception exception) when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Không đọc được SchemaVersions. Ứng dụng không tự thay đổi database: {exception.Message}", exception);
        }
    }
}
