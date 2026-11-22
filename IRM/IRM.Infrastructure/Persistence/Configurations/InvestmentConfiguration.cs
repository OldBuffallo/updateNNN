using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class InvestmentConfiguration : IEntityTypeConfiguration<Investment>
{
    public void Configure(EntityTypeBuilder<Investment> e)
    {
            e.ToTable("Investment");
            e.HasKey(i => i.IDInvestment);
            e.Property(i => i.AmountOfMoney).HasPrecision(18, 2);
            e.HasOne(i => i.Company)
                .WithMany(c => c.Investments)
                .HasForeignKey(i => i.IDCompany);
        }
}
