using System.Globalization;

namespace Meridian.Domain.Pricing;

/// <summary>
/// Port of MeridianPortal.Core.Pricing.PricingEngine (legacy, 2014-2019).
/// Behaviour is locked by tests/Meridian.Characterization.Tests. Every
/// deliberate quirk is preserved and named; see docs/pricing-quirks.md.
/// </summary>
public static class PricingRules
{
    // Legacy servers ran with <globalization culture="en-US" />. The display
    // string is part of the contract, so the culture is explicit here.
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-US");

    public static QuoteResult Calculate(QuoteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var warnings = new List<string>();
        decimal subtotal = 0m;
        double totalWeight = 0;

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
            {
                warnings.Add($"Ignored line {line.ProductCode}: quantity {line.Quantity}");
                continue;
            }

            decimal lineTotal = line.UnitPrice * line.Quantity;
            lineTotal *= VolumeMultiplier(line.Quantity);

            // QUIRK-1: banker's rounding (MidpointRounding.ToEven) per line.
            subtotal += Math.Round(lineTotal, 2, MidpointRounding.ToEven);

            // QUIRK-2: weight is accumulated as double, as in the legacy code.
            totalWeight += line.WeightKg * line.Quantity;

            if (IsHazardous(line.ProductCode))
            {
                warnings.Add($"Hazardous item {line.ProductCode} requires manual freight approval");
            }
        }

        decimal discount = Math.Round(subtotal * TierRate(request.CustomerTier), 2, MidpointRounding.ToEven);
        decimal net = subtotal - discount;

        // QUIRK-3: free freight only when net is strictly greater than 500.
        decimal freight = net > 500m
            ? 0m
            : Math.Round(25m + (decimal)(totalWeight * 0.35), 2, MidpointRounding.ToEven);

        decimal tax = Math.Round((net + freight) * TaxRate(request.Region), 2, MidpointRounding.ToEven);
        decimal total = net + freight + tax;

        return new QuoteResult(subtotal, discount, freight, tax, total,
            total.ToString("C", DisplayCulture), warnings);
    }

    private static decimal VolumeMultiplier(int quantity) => quantity switch
    {
        >= 100 => 0.90m,
        >= 50 => 0.95m,
        _ => 1m,
    };

    // QUIRK-4: tier codes are case-sensitive; "gold" earns no discount.
    private static decimal TierRate(string? tier) => tier switch
    {
        "GOLD" => 0.075m,
        "SILVER" => 0.03m,
        _ => 0m,
    };

    // QUIRK-5: unknown or missing regions are taxed at the NORTH rate.
    private static decimal TaxRate(string? region) => region switch
    {
        "SOUTH" => 0.0725m,
        "EXPORT" => 0m,
        _ => 0.08m,
    };

    // Legacy used ToUpper().StartsWith("HAZ") under en-US. Ordinal, case-insensitive
    // comparison gives the same answer for ASCII codes and is culture-proof.
    private static bool IsHazardous(string? productCode) =>
        productCode is not null && productCode.StartsWith("HAZ", StringComparison.OrdinalIgnoreCase);
}
