using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class ElectronicIdentityConfiguration : IEntityTypeConfiguration<ElectronicIdentity>
{
    public void Configure(EntityTypeBuilder<ElectronicIdentity> e)
    {
            e.ToTable("ElectronicIdentities"); e.HasKey(x => x.Id); e.HasIndex(x => x.IdentitySearchKey);
            e.HasOne(x => x.ForeignPerson).WithMany(x => x.ElectronicIdentities).HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Cascade);
        }
}
