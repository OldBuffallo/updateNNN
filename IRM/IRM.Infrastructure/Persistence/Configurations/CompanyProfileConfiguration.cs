using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class CompanyProfileConfiguration : IEntityTypeConfiguration<CompanyProfile>
{
    public void Configure(EntityTypeBuilder<CompanyProfile> e)
    {
            e.ToTable("CompanyProfiles"); e.HasKey(x => x.CompanyId);
            e.HasOne(x => x.Company).WithOne().HasForeignKey<CompanyProfile>(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
        }
}
