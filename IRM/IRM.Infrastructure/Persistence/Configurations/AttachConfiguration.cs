using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class AttachConfiguration : IEntityTypeConfiguration<Attach>
{
    public void Configure(EntityTypeBuilder<Attach> e)
    {
            e.ToTable("Attach");
            e.HasKey(a => a.IDAttach);
            e.HasOne(a => a.Company)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.IDCompany);
        }
}
