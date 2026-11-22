using IRM.Data.Models;
using IRM.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace IRM.Data;

/// <summary>SQL mapping for the shared WPF/web database. Entity names and schema are compatibility contracts.</summary>
public class IrmDbContext : DbContext
{
    public IrmDbContext(DbContextOptions<IrmDbContext> options) : base(options) { }

    // === Bảng cũ (giữ nguyên schema) ===
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Field> Fields { get; set; }
    public DbSet<Career> Careers { get; set; }
    public DbSet<CareerGroup> CareerGroups { get; set; }
    public DbSet<NationalityEntity> Nationality { get; set; }
    public DbSet<Investment> Investments { get; set; }
    public DbSet<PhoneNumber> PhoneNumbers { get; set; }
    public DbSet<Email> Emails { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Ward> Wards { get; set; }
    public DbSet<Attach> Attachments { get; set; }

    // === Du học sinh ===
    public DbSet<Student> Students { get; set; }

    // === Bảng mở rộng trước v0.1.0 ===
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ImportHistory> ImportHistories { get; set; }
    public DbSet<ImportBackup> ImportBackups { get; set; }
    public DbSet<ArchivedEmployee> ArchivedEmployees { get; set; }
    public DbSet<ColumnMappingTemplate> ColumnMappingTemplates { get; set; }

    // === Mô hình mở rộng v0.1.0 ===
    public DbSet<SchemaVersion> SchemaVersions { get; set; }
    public DbSet<ForeignPerson> ForeignPersons { get; set; }
    public DbSet<ForeignPersonSourceLink> ForeignPersonSourceLinks { get; set; }
    public DbSet<StayCase> StayCases { get; set; }
    public DbSet<FamilyVisitDetail> FamilyVisitDetails { get; set; }
    public DbSet<AdministrativeUnit> AdministrativeUnits { get; set; }
    public DbSet<ResidencePeriod> ResidencePeriods { get; set; }
    public DbSet<ImmigrationDocument> ImmigrationDocuments { get; set; }
    public DbSet<ElectronicIdentity> ElectronicIdentities { get; set; }
    public DbSet<CompanyProfile> CompanyProfiles { get; set; }
    public DbSet<CompanySite> CompanySites { get; set; }
    public DbSet<Accommodation> Accommodations { get; set; }
    public DbSet<CompanyAccommodationAgreement> CompanyAccommodationAgreements { get; set; }
    public DbSet<EconomicZone> EconomicZones { get; set; }
    public DbSet<SiteZoneMembership> SiteZoneMemberships { get; set; }
    public DbSet<CompanyRepresentative> CompanyRepresentatives { get; set; }
    public DbSet<CompanyLegalDocument> CompanyLegalDocuments { get; set; }
    public DbSet<StoredFile> StoredFiles { get; set; }
    public DbSet<Inspection> Inspections { get; set; }
    public DbSet<InspectionSubject> InspectionSubjects { get; set; }
    public DbSet<WebCredential> WebCredentials { get; set; }
    public DbSet<WebRoleAssignment> WebRoleAssignments { get; set; }
    public DbSet<LegacyChangeEvent> LegacyChangeEvents { get; set; }
    public DbSet<MigrationIssue> MigrationIssues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new FieldConfiguration());
        modelBuilder.ApplyConfiguration(new CareerConfiguration());
        modelBuilder.ApplyConfiguration(new CareerGroupConfiguration());
        modelBuilder.ApplyConfiguration(new NationalityEntityConfiguration());
        modelBuilder.ApplyConfiguration(new InvestmentConfiguration());
        modelBuilder.ApplyConfiguration(new PhoneNumberConfiguration());
        modelBuilder.ApplyConfiguration(new EmailConfiguration());
        modelBuilder.ApplyConfiguration(new DistrictConfiguration());
        modelBuilder.ApplyConfiguration(new WardConfiguration());
        modelBuilder.ApplyConfiguration(new AttachConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new ImportHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new ImportBackupConfiguration());
        modelBuilder.ApplyConfiguration(new ColumnMappingTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new ArchivedEmployeeConfiguration());
        modelBuilder.ApplyConfiguration(new SchemaVersionConfiguration());
        modelBuilder.ApplyConfiguration(new ForeignPersonConfiguration());
        modelBuilder.ApplyConfiguration(new ForeignPersonSourceLinkConfiguration());
        modelBuilder.ApplyConfiguration(new StayCaseConfiguration());
        modelBuilder.ApplyConfiguration(new FamilyVisitDetailConfiguration());
        modelBuilder.ApplyConfiguration(new AdministrativeUnitConfiguration());
        modelBuilder.ApplyConfiguration(new ResidencePeriodConfiguration());
        modelBuilder.ApplyConfiguration(new ImmigrationDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new ElectronicIdentityConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyProfileConfiguration());
        modelBuilder.ApplyConfiguration(new CompanySiteConfiguration());
        modelBuilder.ApplyConfiguration(new EconomicZoneConfiguration());
        modelBuilder.ApplyConfiguration(new SiteZoneMembershipConfiguration());
        modelBuilder.ApplyConfiguration(new AccommodationConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyAccommodationAgreementConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyRepresentativeConfiguration());
        modelBuilder.ApplyConfiguration(new StoredFileConfiguration());
        modelBuilder.ApplyConfiguration(new CompanyLegalDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new InspectionConfiguration());
        modelBuilder.ApplyConfiguration(new InspectionSubjectConfiguration());
        modelBuilder.ApplyConfiguration(new WebCredentialConfiguration());
        modelBuilder.ApplyConfiguration(new WebRoleAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new LegacyChangeEventConfiguration());
        modelBuilder.ApplyConfiguration(new MigrationIssueConfiguration());
    }
}
