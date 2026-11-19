using System.Globalization;
using Legacy = MeridianPortal.Core.Pricing;

namespace Meridian.Characterization.Tests;

public static class Runners
{
    /// <summary>
    /// Runs the UNCHANGED legacy engine. The legacy site ran with
    /// &lt;globalization culture="en-US" /&gt;, so the thread culture is pinned
    /// for the call, exactly as IIS did for every request.
    /// </summary>
    public static QuoteSnapshot RunLegacy(CorpusCase c, string culture = "en-US")
    {
        var request = new Legacy.QuoteRequest { CustomerTier = c.Tier, Region = c.Region };
        foreach (var l in c.Lines)
        {
            request.Lines.Add(new Legacy.QuoteLine
            {
                ProductCode = l.Code, Quantity = l.Quantity, UnitPrice = l.UnitPrice, WeightKg = l.WeightKg,
            });
        }

        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        try
        {
            var r = Legacy.PricingEngine.Calculate(request);
            return new QuoteSnapshot(c.Id, r.Subtotal, r.Discount, r.Freight, r.Tax, r.Total, r.DisplayTotal, r.Warnings);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }


    public static QuoteSnapshot RunModern(CorpusCase c)
    {
        var request = new Meridian.Domain.Pricing.QuoteRequest(
            c.Tier, c.Region,
            c.Lines.Select(l => new Meridian.Domain.Pricing.QuoteLine(l.Code, l.Quantity, l.UnitPrice, l.WeightKg)).ToList());
        var r = Meridian.Domain.Pricing.PricingRules.Calculate(request);
        return new QuoteSnapshot(c.Id, r.Subtotal, r.Discount, r.Freight, r.Tax, r.Total, r.DisplayTotal, r.Warnings);
    }

}
