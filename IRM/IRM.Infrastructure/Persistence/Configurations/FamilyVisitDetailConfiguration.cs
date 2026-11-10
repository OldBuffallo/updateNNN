using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class FamilyVisitDetailConfiguration : IEntityTypeConfiguration<FamilyVisitDetail>
{
    public void Configure(EntityTypeBuilder<FamilyVisitDetail> e)
    {
            e.ToTable("FamilyVisitDetails"); e.HasKey(x => x.StayCaseId);
            e.HasOne(x => x.StayCase).WithOne(x => x.FamilyVisitDetail).HasForeignKey<FamilyVisitDetail>(x => x.StayCaseId).OnDelete(DeleteBehavior.Cascade);
        }
}
