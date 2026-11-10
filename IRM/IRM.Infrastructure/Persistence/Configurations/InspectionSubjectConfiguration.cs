using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class InspectionSubjectConfiguration : IEntityTypeConfiguration<InspectionSubject>
{
    public void Configure(EntityTypeBuilder<InspectionSubject> e)
    {
            e.ToTable("InspectionSubjects"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.ForeignPersonId, x.InspectionId }).IsUnique();
            e.HasOne(x => x.Inspection).WithMany(x => x.Subjects).HasForeignKey(x => x.InspectionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ForeignPerson).WithMany().HasForeignKey(x => x.ForeignPersonId).OnDelete(DeleteBehavior.Restrict);
        }
}
