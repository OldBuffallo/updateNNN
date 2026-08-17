using IRM.Data.Models;

namespace IRM.Services;

public static class IrmRoles
{
    public const string Admin = "ADMIN";
    public const string DataEditor = "DATA_EDITOR";
    public const string Inspector = "INSPECTOR";
    public const string Reporter = "REPORTER";
    public const string Viewer = "VIEWER";
    public const string AnyUser = Admin + "," + DataEditor + "," + Inspector + "," + Reporter + "," + Viewer;
}

public sealed class TemporalReportFilter
{
    public DateTime? AsOfDate { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? AdministrativeUnitId { get; set; }
    public string? StayPurposeCode { get; set; }
    public int? EconomicZoneId { get; set; }
    public string? CompanyOwnershipTypeCode { get; set; }
    public string? DocumentTypeCode { get; set; }
    public bool? HasElectronicIdentity { get; set; }

    public void Validate()
    {
        if (AsOfDate.HasValue && (From.HasValue || To.HasValue))
            throw new ArgumentException("AsOfDate không được dùng đồng thời với From/To.");
        if (From.HasValue && To.HasValue && From.Value.Date > To.Value.Date)
            throw new ArgumentException("From phải nhỏ hơn hoặc bằng To.");
    }
}

public sealed class FamilyVisitInput
{
    public int? ForeignPersonId { get; set; }
    public int? StayCaseId { get; set; }
    public string FullName { get; set; } = "";
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? NationalityCode { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime ValidFrom { get; set; } = DateTime.Today;
    public DateTime? ValidTo { get; set; }
    public string RelativeName { get; set; } = "";
    public string Relationship { get; set; } = "";
    public string RelativeTypeCode { get; set; } = "VIETNAMESE_CITIZEN";
    public string? RelativeIdNumber { get; set; }
    public string? RelativePhone { get; set; }
    public string? RelativeAddress { get; set; }
    public int? SponsorForeignPersonId { get; set; }
    public int? SponsorCompanyId { get; set; }
    public int? AdministrativeUnitId { get; set; }
    public string? ResidenceAddress { get; set; }
    public string? ElectronicIdentityNumber { get; set; }
    public string? DocumentTypeCode { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? DocumentValidTo { get; set; }
    public string? Note { get; set; }
}

public sealed class ForeignerSearchFilter
{
    public string? Search { get; set; }
    public string? PurposeCode { get; set; }
    public int? AdministrativeUnitId { get; set; }
    public DateTime AsOfDate { get; set; } = DateTime.Today;
}

public interface IForeignerRegistryService
{
    Task<IReadOnlyList<ForeignPerson>> SearchAsync(string? search, DateTime asOfDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FamilyVisitor>> SearchFamilyVisitsAsync(ForeignerSearchFilter filter, CancellationToken cancellationToken = default);
    Task<int> SaveFamilyVisitAsync(FamilyVisitInput input, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int foreignPersonId, CancellationToken cancellationToken = default);
    Task<ForeignPerson> FindOrCreateMinimalAsync(string fullName, string? passportNumber, string? nationalityCode, CancellationToken cancellationToken = default);
    Task<BackfillResult> BackfillLegacyAsync(CancellationToken cancellationToken = default);
    Task<int> SaveDocumentAsync(ImmigrationDocument document, CancellationToken cancellationToken = default);
    Task<int> SaveElectronicIdentityAsync(ElectronicIdentity identity, CancellationToken cancellationToken = default);
}

public sealed record BackfillResult(int Linked, int Created, int Issues);

public interface IResidenceService
{
    Task<IReadOnlyList<ResidencePeriod>> GetHistoryAsync(int foreignPersonId, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(ResidencePeriod residence, CancellationToken cancellationToken = default);
}

public interface ICompanyProfileService
{
    Task<CompanyProfileAggregate?> GetAsync(int companyId, CancellationToken cancellationToken = default);
    Task SaveProfileAsync(CompanyProfile profile, CancellationToken cancellationToken = default);
    Task<int> SaveSiteAsync(CompanySite site, IEnumerable<int> economicZoneIds, CancellationToken cancellationToken = default);
    Task<int> SaveRepresentativeAsync(CompanyRepresentative representative, CancellationToken cancellationToken = default);
}

public sealed class CompanyProfileAggregate
{
    public Company? Company { get; init; }
    public CompanyProfile? Profile { get; init; }
    public IReadOnlyList<CompanySite> Sites { get; init; } = [];
    public IReadOnlyList<CompanyAccommodationAgreement> AccommodationAgreements { get; init; } = [];
    public IReadOnlyList<CompanyRepresentative> Representatives { get; init; } = [];
    public IReadOnlyList<CompanyLegalDocument> LegalDocuments { get; init; } = [];
    public IReadOnlyList<FamilyVisitor> SponsoredFamilyVisitors { get; init; } = [];
}

public interface IAccommodationService
{
    Task<IReadOnlyList<Accommodation>> SearchAsync(string? search, int? administrativeUnitId, int? companyId, DateTime? asOfDate, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(Accommodation accommodation, CancellationToken cancellationToken = default);
    Task<int> SaveAgreementAsync(CompanyAccommodationAgreement agreement, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface IInspectionService
{
    Task<IReadOnlyList<Inspection>> SearchAsync(DateTime? from, DateTime? to, int? administrativeUnitId, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(Inspection inspection, CancellationToken cancellationToken = default);
    Task<int> AddSubjectAsync(int inspectionId, FamilyVisitInput person, InspectionSubject subject, CancellationToken cancellationToken = default);
    Task<IReadOnlySet<int>> GetCheckedPersonIdsAsync(DateTime lookbackFrom, DateTime lookbackTo, int? administrativeUnitId, CancellationToken cancellationToken = default);
}

public interface IStatisticsService
{
    Task<TerritoryStatistics> GetTerritoryAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TerritoryOverview>> GetTerritoryOverviewAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ZoneStatistics>> GetZoneStatisticsAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
    Task<DocumentIdentityStatistics> GetDocumentIdentityStatisticsAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
}

public interface IStatisticsExportService
{
    Task<byte[]> ExportTerritoryAsync(TemporalReportFilter filter, CancellationToken cancellationToken = default);
}

public sealed class TerritoryStatistics
{
    public string AdministrativeUnitName { get; set; } = "";
    public int TotalForeigners { get; set; }
    public Dictionary<string, int> PeopleByPurpose { get; set; } = new();
    public int CompanyCount { get; set; }
    public int AccommodationCount { get; set; }
    public List<CompanySummary> Companies { get; set; } = [];
    public List<AccommodationSummary> Accommodations { get; set; } = [];
}

public sealed class TerritoryOverview
{
    public int AdministrativeUnitId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public int TotalForeigners { get; set; }
    public Dictionary<string, int> PeopleByPurpose { get; set; } = new();
}

public sealed class CompanySummary
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = "";
    public string OwnershipTypeCode { get; set; } = "";
    public int CurrentWorkerCount { get; set; }
}

public sealed class AccommodationSummary
{
    public int AccommodationId { get; set; }
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public int CurrentResidentCount { get; set; }
    public string Companies { get; set; } = "";
}

public sealed class ZoneStatistics
{
    public int EconomicZoneId { get; set; }
    public string ZoneName { get; set; } = "";
    public string TypeCode { get; set; } = "";
    public int CompanyCount { get; set; }
    public int ForeignPersonCount { get; set; }
}

public sealed class DocumentIdentityStatistics
{
    public Dictionary<string, int> DocumentsByType { get; set; } = new();
    public int WithElectronicIdentity { get; set; }
    public int WithoutElectronicIdentity { get; set; }
}

public interface ILegalDocumentService
{
    Task<int> SaveMetadataAsync(CompanyLegalDocument document, CancellationToken cancellationToken = default);
    Task<StoredFile> StoreAsync(Stream content, string originalName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(int storedFileId, CancellationToken cancellationToken = default);
}

public interface IMalwareScanner
{
    Task ScanAsync(string path, CancellationToken cancellationToken = default);
}

public interface ILegacySyncService
{
    Task<int> ProcessPendingAsync(int batchSize, CancellationToken cancellationToken = default);
}

public interface ISchemaVersionService
{
    Task ValidateAsync(CancellationToken cancellationToken = default);
}

public interface IServiceAuthorizationGuard
{
    Task RequireAnyRoleAsync(params string[] roles);
    Task<int> GetRequiredAccountIdAsync();
}
