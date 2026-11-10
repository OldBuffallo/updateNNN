using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class CompanyLegalDocumentConfiguration : IEntityTypeConfiguration<CompanyLegalDocument>
{
    public void Configure(EntityTypeBuilder<CompanyLegalDocument> e)
    {
            e.ToTable("CompanyLegalDocuments"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.CompanyId, x.TypeCode, x.DocumentNumber });
            e.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.StoredFile).WithMany().HasForeignKey(x => x.StoredFileId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LegacyAttach).WithMany().HasForeignKey(x => x.LegacyAttachId).OnDelete(DeleteBehavior.Restrict);
        }
}
