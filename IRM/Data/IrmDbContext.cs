using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Data;

/// <summary>
/// DbContext chính — map tới database ReportManagerDB hiện có.
/// Sử dụng Fluent API để map chính xác tên bảng/cột theo schema cũ.
/// v0.1.0 dùng các bảng mở rộng riêng, không thêm cột vào schema legacy.
/// </summary>
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

        // ── Accounts ──
        modelBuilder.Entity<Account>(e =>
        {
            e.ToTable("Accounts");
            e.HasKey(a => a.IDUser);
        });

        // ── Companies ──
        modelBuilder.Entity<Company>(e =>
        {
            e.ToTable("Companies");
            e.HasKey(c => c.IDCompany);
            e.HasOne(c => c.Field)
                .WithMany(f => f.Companies)
                .HasForeignKey(c => c.IDField);
            e.HasOne(c => c.Tracker)
                .WithMany()
                .HasForeignKey(c => c.TrackerID);
        });

        // ── Employees ──
        modelBuilder.Entity<Employee>(e =>
        {
            e.ToTable("Employees");
            e.HasKey(emp => emp.IDEmployee);
            e.HasOne(emp => emp.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(emp => emp.IDCompany);
            e.HasOne(emp => emp.Career)
                .WithMany()
                .HasForeignKey(emp => emp.IDCareer);
            e.HasOne(emp => emp.NationalityNav)
                .WithMany()
                .HasForeignKey(emp => emp.Nationality)
                .HasPrincipalKey(n => n.NationalityCode);
            // Dates lưu dạng datetime/varchar trong DB cũ
            e.Property(emp => emp.Birthday).HasColumnName("Birthday");
            e.Property(emp => emp.TemporaryStay).HasColumnName("TemporaryStay");
            e.Property(emp => emp.DateCreated).HasColumnName("DateCreated");
            e.Property(emp => emp.DateOfJoin).HasColumnName("DateOfJoin");
            e.Property(emp => emp.DateOfLeave).HasColumnName("DateOfLeave");
            // Thăm thân
            e.Property(emp => emp.FamilyVisit).HasColumnName("FamilyVisit").HasDefaultValue(0);
            e.Property(emp => emp.FamilyVisitRelativeName).HasColumnName("FamilyVisitRelativeName");
            e.Property(emp => emp.FamilyVisitRelationship).HasColumnName("FamilyVisitRelationship");
            e.Property(emp => emp.FamilyVisitRelativeIdCard).HasColumnName("FamilyVisitRelativeIdCard");
            e.Property(emp => emp.FamilyVisitStartDate).HasColumnName("FamilyVisitStartDate");
            e.Property(emp => emp.FamilyVisitEndDate).HasColumnName("FamilyVisitEndDate");
            e.Property(emp => emp.FamilyVisitNote).HasColumnName("FamilyVisitNote");
        });

        // ── Students (Du học sinh) ──
        modelBuilder.Entity<Student>(e =>
        {
            e.ToTable("Students");
            e.HasKey(s => s.IDStudent);
            e.HasOne(s => s.NationalityNav)
                .WithMany()
                .HasForeignKey(s => s.Nationality)
                .HasPrincipalKey(n => n.NationalityCode);
            e.Property(s => s.Birthday).HasColumnName("Birthday");
            e.Property(s => s.TemporaryStay).HasColumnName("TemporaryStay");
            e.Property(s => s.DateCreated).HasColumnName("DateCreated");
            e.Property(s => s.EnrollmentDate).HasColumnName("EnrollmentDate");
            e.Property(s => s.ExpectedGraduation).HasColumnName("ExpectedGraduation");
            e.Property(s => s.VisaExpiry).HasColumnName("VisaExpiry");
            e.Property(s => s.Hidden_flag).HasDefaultValue(0);
            e.Property(s => s.Status).HasDefaultValue(0);
            e.Property(s => s.ScholarshipType).HasDefaultValue(0);
            e.Property(s => s.EducationLevel).HasDefaultValue(0);
        });

        ConfigureV010(modelBuilder);

        // ── Fields ──
        modelBuilder.Entity<Field>(e =>
        {
            e.ToTable("Fields");
            e.HasKey(f => f.IDField);
        });

        // ── Careers ──
        modelBuilder.Entity<Career>(e =>
        {
            e.ToTable("Careers");
            e.HasKey(c => c.IDCareer);
            e.HasOne(c => c.CareerGroup)
                .WithMany(cg => cg.Careers)
                .HasForeignKey(c => c.IDCG);
        });

        // ── CareerGroups ──
        modelBuilder.Entity<CareerGroup>(e =>
        {
            e.ToTable("CareerGroups");
            e.HasKey(cg => cg.IDCG);
        });

        // ── Nationality ──
        modelBuilder.Entity<NationalityEntity>(e =>
        {
            e.ToTable("Nationality");
            e.HasKey(n => n.IDNationality);
            e.HasAlternateKey(n => n.NationalityCode);
        });

        // ── Investment ──
        modelBuilder.Entity<Investment>(e =>
        {
            e.ToTable("Investment");
            e.HasKey(i => i.IDInvestment);
            e.Property(i => i.AmountOfMoney).HasPrecision(18, 2);
            e.HasOne(i => i.Company)
                .WithMany(c => c.Investments)
                .HasForeignKey(i => i.IDCompany);
        });

        // ── PhoneNumbers ──
        modelBuilder.Entity<PhoneNumber>(e =>
        {
            e.ToTable("PhoneNumbers");
            e.HasKey(p => p.IDPhoneNumber);
            e.HasOne(p => p.Company)
                .WithMany(c => c.PhoneNumbers)
                .HasForeignKey(p => p.IDCompany);
        });

        // ── Emails ──
        modelBuilder.Entity<Email>(e =>
        {
            e.ToTable("Emails");
            e.HasKey(em => em.IDEmail);
            e.HasOne(em => em.Company)
                .WithMany(c => c.Emails)
                .HasForeignKey(em => em.IDCompany);
        });

        // ── Districts ──
        modelBuilder.Entity<District>(e =>
        {
            e.ToTable("Districts");
            e.HasKey(d => d.IDDistrict);
        });

        // ── Wards ──
        modelBuilder.Entity<Ward>(e =>
        {
            e.ToTable("Wards");
            e.HasKey(w => w.IDWard);
        });

        // ── Attach ──
        modelBuilder.Entity<Attach>(e =>
        {
            e.ToTable("Attach");
            e.HasKey(a => a.IDAttach);
            e.HasOne(a => a.Company)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.IDCompany);
        });

        // ── AuditLog (bảng mới) ──
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs");
            e.HasKey(a => a.Id);
        });

        // ── ImportHistory (mở rộng) ──
        modelBuilder.Entity<ImportHistory>(e =>
        {
            e.ToTable("ImportHistories");
            e.HasKey(i => i.Id);
            e.HasIndex(i => i.SessionId);
        });

        // ── ImportBackup (bảng mới) ──
        modelBuilder.Entity<ImportBackup>(e =>
        {
            e.ToTable("ImportBackups");
            e.HasKey(b => b.Id);
            e.HasIndex(b => b.ImportSessionId);
        });

        // ── ColumnMappingTemplate (bảng mới) ──
        modelBuilder.Entity<ColumnMappingTemplate>(e =>
        {
            e.ToTable("ColumnMappingTemplates");
            e.HasKey(t => t.Id);
        });

        // ── ArchivedEmployee (bảng lưu trữ) ──
        modelBuilder.Entity<ArchivedEmployee>(e =>
        {
            e.ToTable("ArchivedEmployees");
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.OriginalId);
            e.HasIndex(a => a.ArchiveReason);
        });
    }

    private static void ConfigureV010(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SchemaVersion>(e => { e.ToTable("SchemaVersions"); e.HasKey(x => x.Id); e.HasIndex(x => x.Version).IsUnique(); });
        modelBuilder.Entity<ForeignPerson>(e =>
        {
            e.ToTable("ForeignPersons"); e.HasKey(x => x.Id); e.HasIndex(x => x.PassportSearchKey);
            e.HasOne(x => x.Nationality).WithMany().HasForeignKey(x => x.NationalityCode).HasPrincipalKey(x => x.NationalityCode).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ForeignPersonSourceLink>(e =>
        {
            e.ToTable("ForeignPersonSourceLinks"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.SourceType, x.SourceId }).IsUnique();
            e.HasOne(x => x.ForeignPerson).WithMany(x => x.SourceLinks).HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<StayCase>(e =>
        {
            e.ToTable("StayCases"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.ForeignPersonId, x.ValidFrom, x.ValidTo });
            e.HasOne(x => x.ForeignPerson).WithMany(x => x.StayCases).HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.SponsorCompany).WithMany().HasForeignKey(x => x.SponsorCompanyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SponsorForeignPerson).WithMany().HasForeignKey(x => x.SponsorForeignPersonId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<FamilyVisitDetail>(e =>
        {
            e.ToTable("FamilyVisitDetails"); e.HasKey(x => x.StayCaseId);
            e.HasOne(x => x.StayCase).WithOne(x => x.FamilyVisitDetail).HasForeignKey<FamilyVisitDetail>(x => x.StayCaseId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<AdministrativeUnit>(e =>
        {
            e.ToTable("AdministrativeUnits"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.Code, x.ValidFrom }).IsUnique();
            e.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Predecessor).WithMany().HasForeignKey(x => x.PredecessorId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ResidencePeriod>(e =>
        {
            e.ToTable("ResidencePeriods"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.AdministrativeUnitId, x.ValidFrom, x.ValidTo });
            e.HasOne(x => x.ForeignPerson).WithMany(x => x.ResidencePeriods).HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Accommodation).WithMany(x => x.ResidencePeriods).HasForeignKey(x => x.AccommodationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AdministrativeUnit).WithMany().HasForeignKey(x => x.AdministrativeUnitId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ResponsibleCompany).WithMany().HasForeignKey(x => x.ResponsibleCompanyId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ImmigrationDocument>(e =>
        {
            e.ToTable("ImmigrationDocuments"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.ForeignPersonId, x.TypeCode, x.ValidTo });
            e.HasOne(x => x.ForeignPerson).WithMany(x => x.ImmigrationDocuments).HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ElectronicIdentity>(e =>
        {
            e.ToTable("ElectronicIdentities"); e.HasKey(x => x.Id); e.HasIndex(x => x.IdentitySearchKey);
            e.HasOne(x => x.ForeignPerson).WithMany(x => x.ElectronicIdentities).HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<CompanyProfile>(e =>
        {
            e.ToTable("CompanyProfiles"); e.HasKey(x => x.CompanyId);
            e.HasOne(x => x.Company).WithOne().HasForeignKey<CompanyProfile>(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<CompanySite>(e =>
        {
            e.ToTable("CompanySites"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.CompanyId, x.ValidFrom, x.ValidTo });
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AdministrativeUnit).WithMany().HasForeignKey(x => x.AdministrativeUnitId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<EconomicZone>(e => { e.ToTable("EconomicZones"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.Code, x.ValidFrom }).IsUnique(); });
        modelBuilder.Entity<SiteZoneMembership>(e =>
        {
            e.ToTable("SiteZoneMemberships"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.CompanySiteId, x.EconomicZoneId, x.ValidFrom }).IsUnique();
            e.HasOne(x => x.CompanySite).WithMany(x => x.ZoneMemberships).HasForeignKey(x => x.CompanySiteId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.EconomicZone).WithMany(x => x.SiteMemberships).HasForeignKey(x => x.EconomicZoneId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Accommodation>(e =>
        {
            e.ToTable("AccommodationsV010"); e.HasKey(x => x.Id); e.HasIndex(x => x.AdministrativeUnitId);
            e.HasOne(x => x.AdministrativeUnit).WithMany().HasForeignKey(x => x.AdministrativeUnitId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<CompanyAccommodationAgreement>(e =>
        {
            e.ToTable("CompanyAccommodationAgreements"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.CompanyId, x.AccommodationId, x.ValidFrom });
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Accommodation).WithMany(x => x.CompanyAgreements).HasForeignKey(x => x.AccommodationId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<CompanyRepresentative>(e =>
        {
            e.ToTable("CompanyRepresentatives"); e.HasKey(x => x.Id);
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<StoredFile>(e => { e.ToTable("StoredFiles"); e.HasKey(x => x.Id); e.HasIndex(x => x.Sha256); });
        modelBuilder.Entity<CompanyLegalDocument>(e =>
        {
            e.ToTable("CompanyLegalDocuments"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.CompanyId, x.TypeCode, x.DocumentNumber });
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.StoredFile).WithMany().HasForeignKey(x => x.StoredFileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LegacyAttach).WithMany().HasForeignKey(x => x.LegacyAttachId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Inspection>(e =>
        {
            e.ToTable("InspectionsV010"); e.HasKey(x => x.Id); e.HasIndex(x => x.InspectedAt);
            e.HasOne(x => x.AdministrativeUnit).WithMany().HasForeignKey(x => x.AdministrativeUnitId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Accommodation).WithMany().HasForeignKey(x => x.AccommodationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CompanySite).WithMany().HasForeignKey(x => x.CompanySiteId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<InspectionSubject>(e =>
        {
            e.ToTable("InspectionSubjects"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.ForeignPersonId, x.InspectionId }).IsUnique();
            e.HasOne(x => x.Inspection).WithMany(x => x.Subjects).HasForeignKey(x => x.InspectionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ForeignPerson).WithMany().HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<WebCredential>(e =>
        {
            e.ToTable("WebCredentials"); e.HasKey(x => x.AccountId);
            e.HasOne(x => x.Account).WithOne().HasForeignKey<WebCredential>(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<WebRoleAssignment>(e =>
        {
            e.ToTable("WebRoleAssignments"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.AccountId, x.RoleCode, x.AdministrativeUnitId }).IsUnique();
            e.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AdministrativeUnit).WithMany().HasForeignKey(x => x.AdministrativeUnitId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<LegacyChangeEvent>(e => { e.ToTable("LegacyChangeEvents"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.StatusCode, x.OccurredAt }); });
        modelBuilder.Entity<MigrationIssue>(e => { e.ToTable("MigrationIssues"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.StatusCode, x.SourceType }); });
    }
}
