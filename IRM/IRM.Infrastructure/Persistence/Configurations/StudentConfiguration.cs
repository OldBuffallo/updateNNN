using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> e)
    {
            e.ToTable("Students");
            e.HasKey(s => s.IDStudent);
            e.HasIndex(s => s.Nationality).HasDatabaseName("IX_Students_Nationality");
            e.HasIndex(s => new { s.Status, s.Hidden_flag }).HasDatabaseName("IX_Students_StatusHidden");
            e.HasIndex(s => s.VisaExpiry).HasDatabaseName("IX_Students_VisaExpiry");
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
            e.Property(s => s.Status).HasDefaultValue(StudentStatus.Studying);
            e.Property(s => s.ScholarshipType).HasDefaultValue(ScholarshipType.SelfFunded);
            e.Property(s => s.EducationLevel).HasDefaultValue(EducationLevel.University);
        }
}
