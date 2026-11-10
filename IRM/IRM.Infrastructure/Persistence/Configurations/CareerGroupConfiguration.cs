using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class CareerGroupConfiguration : IEntityTypeConfiguration<CareerGroup>
{
    public void Configure(EntityTypeBuilder<CareerGroup> e)
    {
            e.ToTable("CareerGroups");
            e.HasKey(cg => cg.IDCG);
        }
}
