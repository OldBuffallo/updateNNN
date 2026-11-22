using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class CompanySiteConfiguration : IEntityTypeConfiguration<CompanySite>
{
    public void Configure(EntityTypeBuilder<CompanySite> e)
    {
            e.ToTable("CompanySites"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.CompanyId, x.ValidFrom, x.ValidTo });
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AdministrativeUnit).WithMany().HasForeignKey(x => x.AdministrativeUnitId).OnDelete(DeleteBehavior.Restrict);
        }
}
