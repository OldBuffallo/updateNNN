using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class ArchivedEmployeeConfiguration : IEntityTypeConfiguration<ArchivedEmployee>
{
    public void Configure(EntityTypeBuilder<ArchivedEmployee> e)
    {
            e.ToTable("ArchivedEmployees");
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.OriginalId);
            e.HasIndex(a => a.ArchiveReason);
        }
}
