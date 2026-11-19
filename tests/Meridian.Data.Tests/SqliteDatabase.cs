using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Meridian.Data.Tests;

/// <summary>In-memory SQLite database that lives as long as this object.</summary>
public sealed class SqliteDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public SqliteDatabase()
    {
        _connection.Open();
        using var db = CreateContext();
        db.Database.EnsureCreated();
        SeedData.Populate(db);
    }

    public CommandCounter Counter { get; } = new();

    public MeridianDbContext CreateContext() => new(
        new DbContextOptionsBuilder<MeridianDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(Counter)
            .Options);

    public void Dispose() => _connection.Dispose();
}

/// <summary>Counts SQL commands so tests can assert "one query, not N+1".</summary>
public sealed class CommandCounter : DbCommandInterceptor
{
    public int Count { get; private set; }

    public void Reset() => Count = 0;

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Count++;
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}
