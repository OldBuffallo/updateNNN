using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class ForeignPersonConfiguration : IEntityTypeConfiguration<ForeignPerson>
{
    public void Configure(EntityTypeBuilder<ForeignPerson> e)
    {
            e.ToTable("ForeignPersons"); e.HasKey(x => x.Id); e.HasIndex(x => x.PassportSearchKey);
            e.HasOne(x => x.Nationality).WithMany().HasForeignKey(x => x.NationalityCode).HasPrincipalKey(x => x.NationalityCode).OnDelete(DeleteBehavior.Restrict);
        }
}
