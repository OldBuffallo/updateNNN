using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> e)
    {
            e.ToTable("Employees");
            e.HasKey(emp => emp.IDEmployee);
            e.HasIndex(emp => emp.IDCompany).HasDatabaseName("IX_Employees_IDCompany");
            e.HasIndex(emp => new { emp.WorkingStatus, emp.Hidden_flag }).HasDatabaseName("IX_Employees_WorkStatus");
            e.HasIndex(emp => emp.Nationality).HasDatabaseName("IX_Employees_Nationality");
            e.HasIndex(emp => emp.TemporaryStay).HasDatabaseName("IX_Employees_TemporaryStay");
            e.HasIndex(emp => emp.FamilyVisit).HasDatabaseName("IX_Employees_FamilyVisit");
            e.HasOne(emp => emp.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(emp => emp.IDCompany)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
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
        }
}
