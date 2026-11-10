using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class MigrationIssueConfiguration : IEntityTypeConfiguration<MigrationIssue>
{
    public void Configure(EntityTypeBuilder<MigrationIssue> e)
    { e.ToTable("MigrationIssues"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.StatusCode, x.SourceType }); }
}
