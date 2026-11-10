using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class ColumnMappingTemplateConfiguration : IEntityTypeConfiguration<ColumnMappingTemplate>
{
    public void Configure(EntityTypeBuilder<ColumnMappingTemplate> e)
    {
            e.ToTable("ColumnMappingTemplates");
            e.HasKey(t => t.Id);
        }
}
