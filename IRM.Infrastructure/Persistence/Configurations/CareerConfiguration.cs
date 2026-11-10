using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class CareerConfiguration : IEntityTypeConfiguration<Career>
{
    public void Configure(EntityTypeBuilder<Career> e)
    {
            e.ToTable("Careers");
            e.HasKey(c => c.IDCareer);
            e.HasOne(c => c.CareerGroup)
                .WithMany(cg => cg.Careers)
                .HasForeignKey(c => c.IDCG);
        }
}
