using System.Data.Common;
using IRM.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace IRM.Tests;

public class ImportRetryTests
{
    [Fact]
    public async Task RetryAfterLostCommitAcknowledgement_RecordsOneSessionAndCorrectCounts()
    {
        await using var connection=new SqliteConnection("Data Source=:memory:");await connection.OpenAsync();
        var interceptor=new LostAcknowledgement();
        await using var db=new IrmDbContext(new DbContextOptionsBuilder<IrmDbContext>().UseSqlite(connection)
            .ReplaceService<IExecutionStrategyFactory,TestRetryFactory>().AddInterceptors(interceptor).Options);
        await db.Database.EnsureCreatedAsync();await LegacyImportPersistenceTests.SeedAsync(db);
        var preview=await LegacyImportPersistenceTests.Preview(db,LegacyImportEndToEndTests.ValidRow());
        interceptor.Enabled=true;
        var result=await LegacyImportPersistenceTests.Store(db).ExecuteImportAsync(preview,[],1,"retry.xlsx");
        Assert.True(result.Success,string.Join(";",result.Errors));
        Assert.Equal(1,result.AddedCount);Assert.Equal(0,result.SkippedCount);
        Assert.Equal(1,await db.Employees.CountAsync());Assert.Equal(1,await db.ImportHistories.CountAsync());
        Assert.Equal(result.SessionId,(await db.ImportHistories.SingleAsync()).SessionId);
        Assert.True(preview[0].ParsedEmployee!.IDEmployee>0);
        Assert.True(interceptor.Fired);
    }

    [Fact]
    public async Task RollbackRetryAfterLostAcknowledgement_ConfirmsDurableRollback()
    {
        await using var connection=new SqliteConnection("Data Source=:memory:");await connection.OpenAsync();
        var interceptor=new LostAcknowledgement();
        await using var db=new IrmDbContext(new DbContextOptionsBuilder<IrmDbContext>().UseSqlite(connection)
            .ReplaceService<IExecutionStrategyFactory,TestRetryFactory>().AddInterceptors(interceptor).Options);
        await db.Database.EnsureCreatedAsync();await LegacyImportPersistenceTests.SeedAsync(db);
        var store=LegacyImportPersistenceTests.Store(db);
        var preview=await LegacyImportPersistenceTests.Preview(db,LegacyImportEndToEndTests.ValidRow());
        var result=await store.ExecuteImportAsync(preview,[],1,"rollback-retry.xlsx");Assert.True(result.Success);
        interceptor.Enabled=true;
        Assert.True(await store.RollbackImportAsync(result.SessionId));
        db.ChangeTracker.Clear();Assert.Equal("rolledback",(await db.ImportHistories.SingleAsync()).Status);
        Assert.Equal(1,(await db.Employees.SingleAsync()).Hidden_flag);
        Assert.False(await store.RollbackImportAsync(result.SessionId));
    }
    private sealed class LostAcknowledgement : DbTransactionInterceptor
    {
        public bool Enabled { get; set; }
        public bool Fired { get; private set; }
        public override Task TransactionCommittedAsync(DbTransaction transaction,TransactionEndEventData eventData,CancellationToken cancellationToken=default)
        {
            if(Enabled&&!Fired) {Fired=true;throw new LostCommitException();}
            return Task.CompletedTask;
        }
    }
    public sealed class LostCommitException : Exception;
    public sealed class TestRetryFactory(ExecutionStrategyDependencies dependencies) : IExecutionStrategyFactory
    {
        public IExecutionStrategy Create()=>new TestRetryStrategy(dependencies);
    }
    private sealed class TestRetryStrategy(ExecutionStrategyDependencies dependencies) : ExecutionStrategy(dependencies,1,TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception)=>exception is LostCommitException;
    }
}
