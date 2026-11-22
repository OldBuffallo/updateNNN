using IRM.Services;
using IRM.Application.Features.ImportExport;
using static IRM.Application.Features.ImportExport.ImportMapping;
using System.Text.Json;
using IRM.Data;
using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Persistence.Stores.ImportExport;

/// <summary>
/// Service xử lý nghiệp vụ Import Excel vào CSDL
/// </summary>
public partial class SqlImportStore : IImportStore
{
    private static readonly HashSet<string> V010ImportFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "StayPurposeCode", "StayValidFrom", "StayValidTo", "ElectronicIdentityNumber",
        "DocumentTypeCode", "DocumentNumber", "DocumentValidFrom", "DocumentValidTo",
        "AdministrativeUnitCode", "ResidenceAddress", "ResidenceValidFrom", "ResidenceValidTo"
    };
    private readonly IrmDbContext _db;
    private readonly SqlAuditStore _audit;
    private readonly IServiceAuthorizationGuard? _guard;

    public SqlImportStore(IrmDbContext db, SqlAuditStore audit)
    {
        _db = db;
        _audit = audit;
    }
    public SqlImportStore(IrmDbContext db, SqlAuditStore audit, IServiceAuthorizationGuard guard) : this(db, audit) => _guard = guard;
}
