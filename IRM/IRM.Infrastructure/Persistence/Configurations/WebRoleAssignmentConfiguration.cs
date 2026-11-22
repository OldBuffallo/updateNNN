using IRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Infrastructure.Persistence.Configurations;

internal sealed class WebRoleAssignmentConfiguration : IEntityTypeConfiguration<WebRoleAssignment>
{
    public void Configure(EntityTypeBuilder<WebRoleAssignment> e)
    {
            e.ToTable("WebRoleAssignments"); e.HasKey(x => x.Id); e.HasIndex(x => new { x.AccountId, x.RoleCode, x.AdministrativeUnitId }).IsUnique();
            e.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AdministrativeUnit).WithMany().HasForeignKey(x => x.AdministrativeUnitId).OnDelete(DeleteBehavior.Restrict);
        }
}
