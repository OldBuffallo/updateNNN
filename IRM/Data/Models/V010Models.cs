namespace IRM.Data.Models;

public static class StayPurposeCodes
{
    public const string Work = "WORK";
    public const string FamilyVisit = "FAMILY_VISIT";
    public const string Study = "STUDY";
    public const string Tourism = "TOURISM";
    public const string BorderVisaExempt = "BORDER_VISA_EXEMPT";
    public const string Other = "OTHER";

    public static readonly string[] All = [Work, FamilyVisit, Study, Tourism, BorderVisaExempt, Other];
}

public static class AdministrativeUnitTypeCodes
{
    public const string Province = "PROVINCE";
    public const string Commune = "COMMUNE";
    public const string Ward = "WARD";
    public const string SpecialZone = "SPECIAL_ZONE";
}

public static class ImmigrationDocumentTypeCodes
{
    public const string Visa = "VISA";
    public const string VisaExtension = "VISA_EXTENSION";
    public const string TemporaryResidenceCard = "TEMPORARY_RESIDENCE_CARD";
    public const string VisaExemption = "VISA_EXEMPTION";
    public const string Other = "OTHER";
}

public class SchemaVersion
{
    public int Id { get; set; }
    public string Version { get; set; } = "";
    public string Description { get; set; } = "";
    public string Checksum { get; set; } = "";
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public string AppliedBy { get; set; } = "";
}

public class ForeignPerson
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? NationalityCode { get; set; }
    public string? PassportNumber { get; set; }
    public string? PassportSearchKey { get; set; }
    public bool IsDataIncomplete { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public NationalityEntity? Nationality { get; set; }
    public ICollection<ForeignPersonSourceLink> SourceLinks { get; set; } = new List<ForeignPersonSourceLink>();
    public ICollection<StayCase> StayCases { get; set; } = new List<StayCase>();
    public ICollection<ResidencePeriod> ResidencePeriods { get; set; } = new List<ResidencePeriod>();
    public ICollection<ImmigrationDocument> ImmigrationDocuments { get; set; } = new List<ImmigrationDocument>();
    public ICollection<ElectronicIdentity> ElectronicIdentities { get; set; } = new List<ElectronicIdentity>();
}

public class ForeignPersonSourceLink
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public string SourceType { get; set; } = "";
    public string SourceId { get; set; } = "";
    public string? SourceFingerprint { get; set; }
    public DateTime LastSynchronizedAt { get; set; } = DateTime.UtcNow;

    public ForeignPerson? ForeignPerson { get; set; }
}

public class StayCase
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public string PurposeCode { get; set; } = StayPurposeCodes.Other;
    public bool IsPrimary { get; set; } = true;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int? SponsorCompanyId { get; set; }
    public int? SponsorForeignPersonId { get; set; }
    public string StatusCode { get; set; } = "ACTIVE";
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ForeignPerson? ForeignPerson { get; set; }
    public Company? SponsorCompany { get; set; }
    public ForeignPerson? SponsorForeignPerson { get; set; }
    public FamilyVisitDetail? FamilyVisitDetail { get; set; }
}

public class FamilyVisitDetail
{
    public int StayCaseId { get; set; }
    public string RelativeName { get; set; } = "";
    public string? RelativeIdNumber { get; set; }
    public string? RelativePhone { get; set; }
    public string? RelativeAddress { get; set; }
    public string Relationship { get; set; } = "";
    public string RelativeTypeCode { get; set; } = "VIETNAMESE_CITIZEN";
    public string? RelativeNationalityCode { get; set; }

    public StayCase? StayCase { get; set; }
}

public class AdministrativeUnit
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string TypeCode { get; set; } = AdministrativeUnitTypeCodes.Commune;
    public int? ParentId { get; set; }
    public int? PredecessorId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsDeleted { get; set; }
    public int? LegacyDistrictId { get; set; }   // Cross-ref District legacy
    public int? LegacyWardId { get; set; }       // Cross-ref Ward legacy

    public AdministrativeUnit? Parent { get; set; }
    public AdministrativeUnit? Predecessor { get; set; }
}

public class ResidencePeriod
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public int? AccommodationId { get; set; }
    public int AdministrativeUnitId { get; set; }
    public string AddressLine { get; set; } = "";
    public string? UnitLabel { get; set; }
    public string ArrangementCode { get; set; } = "SELF_RENTED";
    public int? ResponsibleCompanyId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Note { get; set; }

    public ForeignPerson? ForeignPerson { get; set; }
    public Accommodation? Accommodation { get; set; }
    public AdministrativeUnit? AdministrativeUnit { get; set; }
    public Company? ResponsibleCompany { get; set; }
}

public class ImmigrationDocument
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public string TypeCode { get; set; } = ImmigrationDocumentTypeCodes.Visa;
    public string Number { get; set; } = "";
    public DateTime? IssuedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? IssuedBy { get; set; }
    public string StatusCode { get; set; } = "VALID";
    public string? Note { get; set; }

    public ForeignPerson? ForeignPerson { get; set; }
}

public class ElectronicIdentity
{
    public int Id { get; set; }
    public int ForeignPersonId { get; set; }
    public string IdentityNumber { get; set; } = "";
    public string IdentitySearchKey { get; set; } = "";
    public DateTime? IssuedAt { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string StatusCode { get; set; } = "ACTIVE";

    public ForeignPerson? ForeignPerson { get; set; }
}

public class CompanyProfile
{
    public int CompanyId { get; set; }
    public string OwnershipTypeCode { get; set; } = "DOMESTIC";
    public string? TaxCode { get; set; }
    public string? BusinessRegistrationNumber { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public Company? Company { get; set; }
}

public class CompanySite
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = "";
    public string SiteTypeCode { get; set; } = "OPERATING_SITE";
    public string AddressLine { get; set; } = "";
    public int AdministrativeUnitId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsDeleted { get; set; }

    public Company? Company { get; set; }
    public AdministrativeUnit? AdministrativeUnit { get; set; }
    public ICollection<SiteZoneMembership> ZoneMemberships { get; set; } = new List<SiteZoneMembership>();
}

public class SiteZoneMembership
{
    public int Id { get; set; }
    public int CompanySiteId { get; set; }
    public int EconomicZoneId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public CompanySite? CompanySite { get; set; }
    public EconomicZone? EconomicZone { get; set; }
}

public class CompanyAccommodationAgreement
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int AccommodationId { get; set; }
    public string ScopeCode { get; set; } = "WHOLE_PROPERTY";
    public string? UnitLabel { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Note { get; set; }

    public Company? Company { get; set; }
    public Accommodation? Accommodation { get; set; }
}

public class CompanyRepresentative
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string FullName { get; set; } = "";
    public string? Title { get; set; }
    public string? IdNumber { get; set; }
    public string? Phone { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public Company? Company { get; set; }
}

public class StoredFile
{
    public int Id { get; set; }
    public string StorageName { get; set; } = "";
    public string OriginalName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long Size { get; set; }
    public string Sha256 { get; set; } = "";
    public string ScanStatusCode { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByAccountId { get; set; }
    public bool IsDeleted { get; set; }
}

public class CompanyLegalDocument
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string TypeCode { get; set; } = "BUSINESS_REGISTRATION";
    public string? DocumentNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public string? IssuedBy { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? StoredFileId { get; set; }
    public int? LegacyAttachId { get; set; }
    public string? Note { get; set; }
    public bool IsDeleted { get; set; }

    public Company? Company { get; set; }
    public StoredFile? StoredFile { get; set; }
    public Attach? LegacyAttach { get; set; }
}

public class WebCredential
{
    public int AccountId { get; set; }
    public string PasswordHash { get; set; } = "";
    public bool MustChangePassword { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Account? Account { get; set; }
}

public class WebRoleAssignment
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string RoleCode { get; set; } = "VIEWER";
    public int? AdministrativeUnitId { get; set; }
    public Account? Account { get; set; }
    public AdministrativeUnit? AdministrativeUnit { get; set; }
}

public class LegacyChangeEvent
{
    public long Id { get; set; }
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string Operation { get; set; } = "";
    public string? BeforeXml { get; set; }
    public string? AfterXml { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string StatusCode { get; set; } = "PENDING";
    public string? LastError { get; set; }
}

public class MigrationIssue
{
    public long Id { get; set; }
    public string SourceType { get; set; } = "";
    public string? SourceId { get; set; }
    public string IssueCode { get; set; } = "";
    public string Description { get; set; } = "";
    public string StatusCode { get; set; } = "OPEN";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
