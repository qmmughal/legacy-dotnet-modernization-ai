using System.Globalization;
using BenchmarkDotNet.Attributes;
using Meridian.Characterization.Tests;
using Legacy = MeridianPortal.Core.Pricing;
using Modern = Meridian.Domain.Pricing;

namespace Meridian.Benchmarks;

/// <summary>
/// Prices the whole 614-case corpus with each engine. Requests are built once in
/// GlobalSetup so the benchmark measures pricing, not request construction.
/// </summary>
[MemoryDiagnoser]
public class PricingBenchmarks
{
    private List<Legacy.QuoteRequest> _legacy = [];
    private List<Modern.QuoteRequest> _modern = [];

    [GlobalSetup]
    public void Setup()
    {
        // The legacy engine formats with the thread culture, as IIS did.
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        var corpus = CorpusGenerator.Build();

        _legacy = corpus.Select(c =>
        {
            var r = new Legacy.QuoteRequest { CustomerTier = c.Tier, Region = c.Region };
            r.Lines.AddRange(c.Lines.Select(l => new Legacy.QuoteLine
            {
                ProductCode = l.Code, Quantity = l.Quantity, UnitPrice = l.UnitPrice, WeightKg = l.WeightKg,
            }));
            return r;
        }).ToList();

        _modern = corpus.Select(c => new Modern.QuoteRequest(
            c.Tier, c.Region,
            c.Lines.Select(l => new Modern.QuoteLine(l.Code, l.Quantity, l.UnitPrice, l.WeightKg)).ToList()))
            .ToList();
    }

    [Benchmark(Baseline = true)]
    public decimal LegacyPricingEngine()
    {
        decimal sum = 0m;
        foreach (var request in _legacy)
        {
            sum += Legacy.PricingEngine.Calculate(request).Total;
        }

        return sum;
    }

    [Benchmark]
    public decimal ModernPricingRules()
    {
        decimal sum = 0m;
        foreach (var request in _modern)
        {
            sum += Modern.PricingRules.Calculate(request).Total;
        }

        return sum;
    }
}
