using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class EmailConfiguration : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> e)
    {
            e.ToTable("Emails");
            e.HasKey(em => em.IDEmail);
            e.HasOne(em => em.Company)
                .WithMany(c => c.Emails)
                .HasForeignKey(em => em.IDCompany);
        }
}
