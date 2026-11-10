using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class ResidencePeriodConfiguration : IEntityTypeConfiguration<ResidencePeriod>
{
    public void Configure(EntityTypeBuilder<ResidencePeriod> e)
    {
            e.ToTable("ResidencePeriods"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.AdministrativeUnitId, x.ValidFrom, x.ValidTo });
            e.HasOne(x => x.ForeignPerson).WithMany(x => x.ResidencePeriods).HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Accommodation).WithMany(x => x.ResidencePeriods).HasForeignKey(x => x.AccommodationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AdministrativeUnit).WithMany().HasForeignKey(x => x.AdministrativeUnitId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ResponsibleCompany).WithMany().HasForeignKey(x => x.ResponsibleCompanyId).OnDelete(DeleteBehavior.Restrict);
        }
}
