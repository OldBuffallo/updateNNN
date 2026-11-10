using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IRM.Data;

public sealed class DesignTimeIrmDbContextFactory : IDesignTimeDbContextFactory<IrmDbContext>
{
    public IrmDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<IrmDbContext>()
            .UseSqlServer("Server=127.0.0.1,14331;Database=IRM;User Id=irm_app;Password=DesignTimeOnly-NotUsed-1!;Encrypt=True;TrustServerCertificate=True")
            .Options;
        return new IrmDbContext(options);
    }
}
