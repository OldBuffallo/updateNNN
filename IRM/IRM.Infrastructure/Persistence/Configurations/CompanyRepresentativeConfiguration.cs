using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class CompanyRepresentativeConfiguration : IEntityTypeConfiguration<CompanyRepresentative>
{
    public void Configure(EntityTypeBuilder<CompanyRepresentative> e)
    {
            e.ToTable("CompanyRepresentatives"); e.HasKey(x => x.Id);
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
        }
}
