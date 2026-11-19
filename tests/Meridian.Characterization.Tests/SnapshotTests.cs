namespace Meridian.Characterization.Tests;

/// <summary>
/// A human-readable approval snapshot of one representative quote. The
/// .approved.txt file is committed and reviewed like code: a reviewer can see
/// at a glance what changed when a pull request touches it.
/// </summary>
public sealed class SnapshotTests
{
    [Fact]
    public void RepresentativeQuote_MatchesApprovedSnapshot()
    {
        var c = new CorpusCase("snapshot-mixed-order", "GOLD", "SOUTH",
        [
            new("BLT-M8-40", 250, 0.42m, 0.021),
            new("VLV-200", 2, 129.90m, 6.4),
            new("HAZ-SOLV-5", 1, 38.15m, 5.2),
            new("GSK-12", 0, 3.00m, 0.1),
        ]);

        Approvals.AssertMatches("representative-quote", Render(Runners.RunModern(c)));
    }

    private static string Render(QuoteSnapshot s) => string.Join('\n',
        $"Subtotal:      {s.Subtotal}",
        $"Discount:      {s.Discount}",
        $"Freight:       {s.Freight}",
        $"Tax:           {s.Tax}",
        $"Total:         {s.Total}",
        $"Display total: {s.DisplayTotal}",
        "Warnings:",
        string.Join('\n', s.Warnings.Select(w => "  - " + w))) + "\n";
}
