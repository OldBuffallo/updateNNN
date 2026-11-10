using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class SiteZoneMembershipConfiguration : IEntityTypeConfiguration<SiteZoneMembership>
{
    public void Configure(EntityTypeBuilder<SiteZoneMembership> e)
    {
            e.ToTable("SiteZoneMemberships"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.CompanySiteId, x.EconomicZoneId, x.ValidFrom }).IsUnique();
            e.HasOne(x => x.CompanySite).WithMany(x => x.ZoneMemberships).HasForeignKey(x => x.CompanySiteId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.EconomicZone).WithMany(x => x.SiteMemberships).HasForeignKey(x => x.EconomicZoneId).OnDelete(DeleteBehavior.Cascade);
        }
}
