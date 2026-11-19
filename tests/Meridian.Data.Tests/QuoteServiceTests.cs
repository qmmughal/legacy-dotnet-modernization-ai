using Meridian.Domain.Pricing;
using Microsoft.Extensions.Time.Testing;

namespace Meridian.Data.Tests;

public sealed class QuoteServiceTests : IDisposable
{
    private readonly SqliteDatabase _db = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 19, 15, 0, 0, TimeSpan.Zero));

    public void Dispose() => _db.Dispose();

    private QuoteService CreateService(MeridianDbContext db) => new(db,
        new QuoteNumberGenerator(_clock, new InMemorySequenceSource(), TimeZoneInfo.FindSystemTimeZoneById("America/Chicago")));

    [Fact]
    public async Task CreateQuote_UsesCustomerTierAndRegion()
    {
        await using var db = _db.CreateContext();
        var output = await CreateService(db).CreateQuoteAsync(
            new QuoteInput("C-1001", [new("VLV-200", 2), new("BLT-M8-40", 250)]), TestContext.Current.CancellationToken);

        Assert.Equal("Q-20261119-00001", output.QuoteNumber);
        Assert.Equal(354.30m, output.Result.Subtotal);   // 259.80 + 94.50
        Assert.Equal(26.57m, output.Result.Discount);    // GOLD 7.5% of 354.30 = 26.5725, rounded to 2 dp
    }

    [Fact]
    public void QuoteNumber_UsesBusinessDate_NotUtcDate()
    {
        // 03:00 UTC on 19 Nov is still 18 Nov in Chicago. The legacy code used the
        // server's local clock; the modern code names the business time zone.
        var early = new FakeTimeProvider(new DateTimeOffset(2026, 11, 19, 3, 0, 0, TimeSpan.Zero));
        var generator = new QuoteNumberGenerator(early, new InMemorySequenceSource(),
            TimeZoneInfo.FindSystemTimeZoneById("America/Chicago"));

        Assert.Equal("Q-20261118-00001", generator.NextQuoteNumber());
    }

    [Fact]
    public async Task CreateQuote_LoadsProductsInOneQuery()
    {
        await using var db = _db.CreateContext();
        _db.Counter.Reset();
        _ = await CreateService(db).CreateQuoteAsync(
            new QuoteInput("C-2002", [new("VLV-200", 1), new("BLT-M8-40", 10), new("HAZ-SOLV-5", 1)]), TestContext.Current.CancellationToken);

        Assert.Equal(2, _db.Counter.Count); // customer + products; legacy issued 1 + N
    }

    [Fact]
    public async Task UnknownProduct_IsRejectedWithLegacyMessage()
    {
        await using var db = _db.CreateContext();
        var ex = await Assert.ThrowsAsync<QuoteValidationException>(() =>
            CreateService(db).CreateQuoteAsync(new QuoteInput("C-1001", [new("NOPE-1", 1)]), TestContext.Current.CancellationToken));
        Assert.Equal("Unknown product NOPE-1", ex.Message);
    }
}
