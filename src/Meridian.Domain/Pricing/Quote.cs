namespace Meridian.Domain.Pricing;

public sealed record QuoteLine(string? ProductCode, int Quantity, decimal UnitPrice, double WeightKg);

public sealed record QuoteRequest(string? CustomerTier, string? Region, IReadOnlyList<QuoteLine> Lines);

public sealed record QuoteResult(
    decimal Subtotal,
    decimal Discount,
    decimal Freight,
    decimal Tax,
    decimal Total,
    string DisplayTotal,
    IReadOnlyList<string> Warnings);
