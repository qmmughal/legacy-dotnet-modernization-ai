using Microsoft.EntityFrameworkCore;

namespace Meridian.Data.Tests;

public sealed class OrderQueryTests : IDisposable
{
    private readonly SqliteDatabase _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Admin_SeesAllRegions_NewestFirst()
    {
        await using var db = _db.CreateContext();
        var rows = await db.RecentOrders(status: null, isAdmin: true, userRegion: null).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(["Q-20261104-00003", "Q-20261103-00002", "Q-20261102-00001"], rows.Select(r => r.QuoteNumber));
    }

    [Fact]
    public async Task SalesRep_SeesOnlyOwnRegion()
    {
        await using var db = _db.CreateContext();
        var rows = await db.RecentOrders(null, isAdmin: false, userRegion: "SOUTH").ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Southbank Pumps", Assert.Single(rows).CustomerName);
    }

    [Fact]
    public async Task StatusFilter_IsApplied()
    {
        await using var db = _db.CreateContext();
        var rows = await db.RecentOrders("O", isAdmin: true, userRegion: null).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Q-20261102-00001", Assert.Single(rows).QuoteNumber);
    }

    [Fact]
    public async Task NullRegionUser_SeesNullRegionCustomers_LegacyNullSemantics()
    {
        // Characterizes EF6's default C# null semantics (UseDatabaseNullSemantics = false):
        // Customer.Region == null matched customers whose region IS NULL.
        await using (var setup = _db.CreateContext())
        {
            var c = new Meridian.Domain.Orders.Customer { AccountCode = "C-9009", Name = "No Region Co", Region = null };
            setup.Orders.Add(new Meridian.Domain.Orders.Order { QuoteNumber = "Q-20261105-00004", Customer = c, CreatedOn = new DateTime(2026, 11, 5) });
            await setup.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var db = _db.CreateContext();
        var rows = await db.RecentOrders(null, isAdmin: false, userRegion: null).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal("No Region Co", Assert.Single(rows).CustomerName);
    }

    [Fact]
    public async Task RecentOrders_IsOneSqlQuery_NotNPlusOne()
    {
        await using var db = _db.CreateContext();
        _db.Counter.Reset();
        _ = await db.RecentOrders(null, isAdmin: true, userRegion: null).ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, _db.Counter.Count);
    }
}
