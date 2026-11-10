using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class PhoneNumberConfiguration : IEntityTypeConfiguration<PhoneNumber>
{
    public void Configure(EntityTypeBuilder<PhoneNumber> e)
    {
            e.ToTable("PhoneNumbers");
            e.HasKey(p => p.IDPhoneNumber);
            e.HasOne(p => p.Company)
                .WithMany(c => c.PhoneNumbers)
                .HasForeignKey(p => p.IDCompany);
        }
}
