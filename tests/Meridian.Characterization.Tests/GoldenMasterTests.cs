using System.Globalization;
using System.Text.Json;

namespace Meridian.Characterization.Tests;

/// <summary>
/// Golden master for the pricing rules.
///   1. The legacy engine must still produce the approved output (guards the harness).
///   2. The modern engine must produce identical output for every case.
/// To re-record after an APPROVED behaviour change:
///   GOLDEN_RECORD=1 dotnet test --filter GoldenMaster
/// then review the diff of golden/pricing.approved.json in the pull request.
/// </summary>
public sealed class GoldenMasterTests
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private static readonly IReadOnlyList<CorpusCase> Corpus = CorpusGenerator.Build();

    private static string ApprovedPath([System.Runtime.CompilerServices.CallerFilePath] string source = "") =>
        Path.Combine(Path.GetDirectoryName(source)!, "golden", "pricing.approved.json");

    [Fact]
    public void GoldenMaster_LegacyEngine_MatchesApprovedFile()
    {
        var actual = Corpus.Select(c => Runners.RunLegacy(c)).ToList();
        var path = ApprovedPath();

        if (Environment.GetEnvironmentVariable("GOLDEN_RECORD") == "1")
        {
            File.WriteAllText(path, JsonSerializer.Serialize(actual, Json));
            Assert.Fail($"Recorded {actual.Count} cases to {path}. Review the diff, then run again without GOLDEN_RECORD.");
        }

        var approved = JsonSerializer.Deserialize<List<QuoteSnapshot>>(File.ReadAllText(path))!;
        Assert.Equal(approved.Count, actual.Count);
        var mismatches = approved.Zip(actual).Where(p => !p.First.SameAs(p.Second)).ToList();
        Assert.True(mismatches.Count == 0,
            "Legacy output drifted from the approved file:\n" +
            string.Join("\n", mismatches.Take(10).Select(m => $"approved {m.First}\nactual   {m.Second}")));
    }

    [Fact]
    public void GoldenMaster_ModernEngine_MatchesLegacyForEveryCase()
    {
        var mismatches = Corpus
            .Select(c => (Legacy: Runners.RunLegacy(c), Modern: Runners.RunModern(c)))
            .Where(p => !p.Legacy.SameAs(p.Modern))
            .ToList();

        Assert.True(mismatches.Count == 0,
            $"{mismatches.Count} of {Corpus.Count} cases differ:\n" +
            string.Join("\n", mismatches.Take(10).Select(m => $"legacy {m.Legacy}\nmodern {m.Modern}")));
    }

    [Fact]
    public void ModernEngine_IgnoresThreadCulture()
    {
        var c = Corpus.Single(x => x.Id == "edge-net-500.01");
        var expected = Runners.RunModern(c);

        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            Assert.True(expected.SameAs(Runners.RunModern(c)));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void LegacyEngine_DisplayTotal_DependsOnServerCulture()
    {
        // Documents WHY the culture is pinned: the same legacy code on a server
        // configured for de-DE would have produced a different contract string.
        var c = Corpus.Single(x => x.Id == "edge-net-500.01");
        var enUs = Runners.RunLegacy(c, "en-US");
        var deDe = Runners.RunLegacy(c, "de-DE");

        Assert.Equal(enUs.Total, deDe.Total);
        Assert.NotEqual(enUs.DisplayTotal, deDe.DisplayTotal);
    }
}
