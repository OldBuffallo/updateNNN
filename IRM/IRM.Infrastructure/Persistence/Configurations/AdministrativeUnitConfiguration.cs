using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class AdministrativeUnitConfiguration : IEntityTypeConfiguration<AdministrativeUnit>
{
    public void Configure(EntityTypeBuilder<AdministrativeUnit> e)
    {
            e.ToTable("AdministrativeUnits"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.Code, x.ValidFrom }).IsUnique();
            e.HasOne(x => x.Parent).WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Predecessor).WithMany().HasForeignKey(x => x.PredecessorId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.LegacyDistrictId).HasDatabaseName("IX_AU_LegacyDistrictId");
            e.HasIndex(x => x.LegacyWardId).HasDatabaseName("IX_AU_LegacyWardId");
        }
}
