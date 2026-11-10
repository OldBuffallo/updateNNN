using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class ForeignPersonSourceLinkConfiguration : IEntityTypeConfiguration<ForeignPersonSourceLink>
{
    public void Configure(EntityTypeBuilder<ForeignPersonSourceLink> e)
    {
            e.ToTable("ForeignPersonSourceLinks"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.SourceType, x.SourceId }).IsUnique();
            e.HasOne(x => x.ForeignPerson).WithMany(x => x.SourceLinks).HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Cascade);
        }
}
