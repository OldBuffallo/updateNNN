using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class WebCredentialConfiguration : IEntityTypeConfiguration<WebCredential>
{
    public void Configure(EntityTypeBuilder<WebCredential> e)
    {
            e.ToTable("WebCredentials"); e.HasKey(x => x.AccountId);
            e.HasOne(x => x.Account).WithOne().HasForeignKey<WebCredential>(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
        }
}
