using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class NationalityEntityConfiguration : IEntityTypeConfiguration<NationalityEntity>
{
    public void Configure(EntityTypeBuilder<NationalityEntity> e)
    {
            e.ToTable("Nationality");
            e.HasKey(n => n.IDNationality);
            e.HasAlternateKey(n => n.NationalityCode);
        }
}
