using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class ImmigrationDocumentConfiguration : IEntityTypeConfiguration<ImmigrationDocument>
{
    public void Configure(EntityTypeBuilder<ImmigrationDocument> e)
    {
            e.ToTable("ImmigrationDocuments"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.ForeignPersonId, x.TypeCode, x.ValidTo });
            e.HasOne(x => x.ForeignPerson).WithMany(x => x.ImmigrationDocuments).HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Cascade);
        }
}
