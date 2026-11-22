using Microsoft.EntityFrameworkCore;

namespace IRM.Infrastructure.Persistence;

internal static class EntityUpdates
{
    public static void UpdateValues<TEntity>(this DbSet<TEntity> set, DbContext context, TEntity incoming) where TEntity : class
    {
        var entry = context.Entry(incoming);
        var key = entry.Metadata.FindPrimaryKey()!;
        var values = key.Properties.Select(p => entry.Property(p.Name).CurrentValue).ToArray();
        var existing = set.Find(values);
        if (existing is null)
        {
            set.Update(incoming);
            return;
        }
        context.Entry(existing).CurrentValues.SetValues(incoming);
    }
}
