using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace IRM.Data;

/// <summary>
/// DbContext chính — map tới database ReportManagerDB hiện có.
/// Sử dụng Fluent API để map chính xác tên bảng/cột theo schema cũ.
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

    // === Bảng mới ===
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ImportHistory> ImportHistories { get; set; }
    public DbSet<ImportBackup> ImportBackups { get; set; }
    public DbSet<ColumnMappingTemplate> ColumnMappingTemplates { get; set; }

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
    }
}
