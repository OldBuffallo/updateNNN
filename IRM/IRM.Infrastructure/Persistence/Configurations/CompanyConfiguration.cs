using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> e)
    {
            e.ToTable("Companies");
            e.HasKey(c => c.IDCompany);
            e.HasIndex(c => c.IDField).HasDatabaseName("IX_Companies_IDField");
            e.HasIndex(c => c.Delete_flag).HasDatabaseName("IX_Companies_DeleteFlag");
            e.HasOne(c => c.Field)
                .WithMany(f => f.Companies)
                .HasForeignKey(c => c.IDField);
            e.HasOne(c => c.Tracker)
                .WithMany()
                .HasForeignKey(c => c.TrackerID);
        }
}
