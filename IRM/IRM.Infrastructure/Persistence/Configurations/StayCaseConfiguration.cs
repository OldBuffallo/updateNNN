using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class StayCaseConfiguration : IEntityTypeConfiguration<StayCase>
{
    public void Configure(EntityTypeBuilder<StayCase> e)
    {
            e.ToTable("StayCases"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.ForeignPersonId, x.ValidFrom, x.ValidTo });
            e.HasOne(x => x.ForeignPerson).WithMany(x => x.StayCases).HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.SponsorCompany).WithMany().HasForeignKey(x => x.SponsorCompanyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SponsorForeignPerson).WithMany().HasForeignKey(x => x.SponsorForeignPersonId).OnDelete(DeleteBehavior.Restrict);
        }
}
