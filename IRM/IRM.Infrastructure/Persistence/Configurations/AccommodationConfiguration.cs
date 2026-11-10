using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class AccommodationConfiguration : IEntityTypeConfiguration<Accommodation>
{
    public void Configure(EntityTypeBuilder<Accommodation> e)
    {
            e.ToTable("AccommodationsV010"); e.HasKey(x => x.Id); e.HasIndex(x => x.AdministrativeUnitId);
            e.HasOne(x => x.AdministrativeUnit).WithMany().HasForeignKey(x => x.AdministrativeUnitId).OnDelete(DeleteBehavior.Restrict);
        }
}
