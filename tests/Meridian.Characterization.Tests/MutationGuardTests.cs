namespace Meridian.Characterization.Tests;

/// <summary>
/// Proves the corpus is strong enough to catch the "harmless clean-ups" an
/// AI assistant (or a person) is most likely to make. If one of these tests
/// starts failing, the corpus has lost coverage, not the code.
/// </summary>
public sealed class MutationGuardTests
{
    private static readonly IReadOnlyList<CorpusCase> Corpus = CorpusGenerator.Build();

    [Fact]
    public void Corpus_Detects_SwitchToAwayFromZeroRounding()
    {
        int differing = Corpus.Count(c =>
            Runners.RunLegacy(c).Total != TotalWith(c, MidpointRounding.AwayFromZero, freeFreightInclusive: false));
        Assert.True(differing > 0, "No case distinguishes ToEven from AwayFromZero rounding.");
    }

    [Fact]
    public void Corpus_Detects_InclusiveFreeFreightThreshold()
    {
        int differing = Corpus.Count(c =>
            Runners.RunLegacy(c).Total != TotalWith(c, MidpointRounding.ToEven, freeFreightInclusive: true));
        Assert.True(differing > 0, "No case sits exactly on the 500.00 free-freight boundary.");
    }

    // A deliberately "tidied" re-implementation with two plausible mistakes as switches.
    private static decimal TotalWith(CorpusCase c, MidpointRounding mode, bool freeFreightInclusive)
    {
        decimal subtotal = 0m;
        double weight = 0;
        foreach (var l in c.Lines.Where(l => l.Quantity > 0))
        {
            decimal lineTotal = l.UnitPrice * l.Quantity * (l.Quantity >= 100 ? 0.90m : l.Quantity >= 50 ? 0.95m : 1m);
            subtotal += Math.Round(lineTotal, 2, mode);
            weight += l.WeightKg * l.Quantity;
        }

        decimal rate = c.Tier switch { "GOLD" => 0.075m, "SILVER" => 0.03m, _ => 0m };
        decimal discount = Math.Round(subtotal * rate, 2, mode);
        decimal net = subtotal - discount;
        bool free = freeFreightInclusive ? net >= 500m : net > 500m;
        decimal freight = free ? 0m : Math.Round(25m + (decimal)(weight * 0.35), 2, mode);
        decimal taxRate = c.Region switch { "SOUTH" => 0.0725m, "EXPORT" => 0m, _ => 0.08m };
        decimal tax = Math.Round((net + freight) * taxRate, 2, mode);
        return net + freight + tax;
    }
}
