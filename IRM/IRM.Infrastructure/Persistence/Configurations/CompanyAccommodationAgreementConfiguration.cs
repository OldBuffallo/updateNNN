using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class CompanyAccommodationAgreementConfiguration : IEntityTypeConfiguration<CompanyAccommodationAgreement>
{
    public void Configure(EntityTypeBuilder<CompanyAccommodationAgreement> e)
    {
            e.ToTable("CompanyAccommodationAgreements"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.CompanyId, x.AccommodationId, x.ValidFrom });
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Accommodation).WithMany(x => x.CompanyAgreements).HasForeignKey(x => x.AccommodationId).OnDelete(DeleteBehavior.Cascade);
        }
}
