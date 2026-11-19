namespace Meridian.Characterization.Tests;

public sealed record CorpusLine(string? Code, int Quantity, decimal UnitPrice, double WeightKg);

public sealed record CorpusCase(string Id, string? Tier, string? Region, IReadOnlyList<CorpusLine> Lines);

/// <summary>
/// Deterministic input corpus. Uses its own linear congruential generator so the
/// sequence never depends on System.Random's implementation or the runtime.
/// Hand-picked edge cases come first; generated cases fill the space between.
/// </summary>
public static class CorpusGenerator
{
    private static readonly string?[] Tiers = ["GOLD", "SILVER", null, "gold", "BRONZE"];
    private static readonly string?[] Regions = ["NORTH", "SOUTH", "EXPORT", null, "north", "WEST"];
    private static readonly string?[] Codes = ["BLT-M8-40", "VLV-200", "HAZ-SOLV-5", "haz-glue", "GSK-12", null];
    private static readonly int[] Quantities = [-1, 0, 1, 2, 7, 49, 50, 51, 99, 100, 101, 250];

    public static IReadOnlyList<CorpusCase> Build(int generated = 600)
    {
        var cases = new List<CorpusCase>
        {
            new("edge-empty", "GOLD", "NORTH", []),
            new("edge-qty-49", null, "NORTH", [new("BLT-M8-40", 49, 1.10m, 0.02)]),
            new("edge-qty-50", null, "NORTH", [new("BLT-M8-40", 50, 1.10m, 0.02)]),
            new("edge-qty-99", null, "NORTH", [new("BLT-M8-40", 99, 1.10m, 0.02)]),
            new("edge-qty-100", null, "NORTH", [new("BLT-M8-40", 100, 1.10m, 0.02)]),
            new("edge-net-exactly-500", null, "SOUTH", [new("VLV-200", 4, 125.00m, 6.4)]),
            new("edge-net-500.01", null, "SOUTH", [new("VLV-200", 1, 500.01m, 6.4)]),
            new("edge-gold-lowercase", "gold", "NORTH", [new("VLV-200", 2, 129.90m, 6.4)]),
            new("edge-region-null", "SILVER", null, [new("VLV-200", 1, 129.90m, 6.4)]),
            new("edge-region-lowercase", "SILVER", "south", [new("VLV-200", 1, 129.90m, 6.4)]),
            new("edge-hazard-lowercase", null, "EXPORT", [new("haz-glue", 3, 12.50m, 0.4)]),
            new("edge-negative-qty", "GOLD", "NORTH", [new("GSK-12", -1, 3.00m, 0.1), new("GSK-12", 2, 3.00m, 0.1)]),
            new("edge-midpoint-discount", "GOLD", "EXPORT", [new("GSK-12", 1, 0.60m, 0.0)]),
            new("edge-midpoint-tax", null, "NORTH", [new("GSK-12", 1, 0.0625m, 0.0)]),
        };

        uint state = 20141103;
        uint NextUInt()
        {
            state = unchecked((state * 1664525u) + 1013904223u);
            return state;
        }

        int Pick(int n) => (int)(NextUInt() % (uint)n);

        for (int i = 0; i < generated; i++)
        {
            int lineCount = 1 + Pick(4);
            var lines = new List<CorpusLine>(lineCount);
            for (int j = 0; j < lineCount; j++)
            {
                // Prices with 2-4 decimal places produce many rounding midpoints.
                decimal price = (NextUInt() % 2_000_000) / 10_000m;
                double weight = (NextUInt() % 5000) / 1000.0;
                lines.Add(new(Codes[Pick(Codes.Length)], Quantities[Pick(Quantities.Length)], price, weight));
            }

            cases.Add(new($"gen-{i:D4}", Tiers[Pick(Tiers.Length)], Regions[Pick(Regions.Length)], lines));
        }

        return cases;
    }
}
