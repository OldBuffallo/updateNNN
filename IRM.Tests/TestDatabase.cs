using IRM.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IRM.Tests;

internal sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    public IrmDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        var options = new DbContextOptionsBuilder<IrmDbContext>().UseSqlite(_connection).Options;
        Context = new IrmDbContext(options);
        await Context.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Context is not null) await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
