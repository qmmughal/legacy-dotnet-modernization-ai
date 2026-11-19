namespace Meridian.Domain.Pricing;

public interface IQuoteNumberGenerator
{
    string NextQuoteNumber();
}

/// <summary>
/// Q-yyyyMMdd-nnnnn. The legacy version used DateTime.Now and a static counter
/// that reset on app-pool recycle; this one takes a TimeProvider and a
/// sequence source so tests are deterministic and numbers survive restarts.
/// </summary>
public sealed class QuoteNumberGenerator(TimeProvider clock, ISequenceSource sequence, TimeZoneInfo businessZone)
    : IQuoteNumberGenerator
{
    public string NextQuoteNumber()
    {
        var local = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), businessZone);
        return $"Q-{local:yyyyMMdd}-{sequence.NextValue():D5}";
    }
}

public interface ISequenceSource
{
    long NextValue();
}

public sealed class InMemorySequenceSource : ISequenceSource
{
    private long _value;
    public long NextValue() => Interlocked.Increment(ref _value);
}
