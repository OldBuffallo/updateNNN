using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class EconomicZoneConfiguration : IEntityTypeConfiguration<EconomicZone>
{
    public void Configure(EntityTypeBuilder<EconomicZone> e)
    { e.ToTable("EconomicZones"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.Code, x.ValidFrom }).IsUnique(); }
}
