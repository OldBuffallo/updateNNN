using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> e)
    {
            e.ToTable("InspectionsV010"); e.HasKey(x => x.Id); e.HasIndex(x => x.InspectedAt);
            e.HasOne(x => x.AdministrativeUnit).WithMany().HasForeignKey(x => x.AdministrativeUnitId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Accommodation).WithMany().HasForeignKey(x => x.AccommodationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CompanySite).WithMany().HasForeignKey(x => x.CompanySiteId).OnDelete(DeleteBehavior.Restrict);
        }
}
