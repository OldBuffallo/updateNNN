using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class LegacyChangeEventConfiguration : IEntityTypeConfiguration<LegacyChangeEvent>
{
    public void Configure(EntityTypeBuilder<LegacyChangeEvent> e)
    { e.ToTable("LegacyChangeEvents"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.StatusCode, x.OccurredAt }); }
}
