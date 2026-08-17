using IRM.Data;
using Microsoft.EntityFrameworkCore;

namespace IRM.Services;

public sealed class SchemaVersionService : ISchemaVersionService
{
    public const string RequiredVersion = "0.1.0";
    private readonly IrmDbContext _db;
    public SchemaVersionService(IrmDbContext db) => _db = db;

    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _db.SchemaVersions.AsNoTracking().AnyAsync(x => x.Version == RequiredVersion, cancellationToken))
                throw new InvalidOperationException($"Database chưa có schema {RequiredVersion}. Hãy chạy migration được phê duyệt trước khi khởi động.");
        }
        catch (Exception exception) when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Không đọc được SchemaVersions. Ứng dụng không tự thay đổi database: {exception.Message}", exception);
        }
    }
}
