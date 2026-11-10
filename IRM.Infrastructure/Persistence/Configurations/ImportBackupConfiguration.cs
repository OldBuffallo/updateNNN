using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class ImportBackupConfiguration : IEntityTypeConfiguration<ImportBackup>
{
    public void Configure(EntityTypeBuilder<ImportBackup> e)
    {
            e.ToTable("ImportBackups");
            e.HasKey(b => b.Id);
            e.HasIndex(b => b.ImportSessionId);
            e.HasIndex(b => new { b.EntityType, b.EntityId }).HasDatabaseName("IX_ImportBackups_EntityTypeId");
        }
}
