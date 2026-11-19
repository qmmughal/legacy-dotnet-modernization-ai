using Meridian.Domain.Pricing;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Data;

public sealed record QuoteLineInput(string ProductCode, int Quantity);

public sealed record QuoteInput(string CustomerAccount, IReadOnlyList<QuoteLineInput> Lines);

public sealed record QuoteOutput(string QuoteNumber, QuoteResult Result);

public sealed class QuoteValidationException(string message) : Exception(message);

/// <summary>
/// Shared by the REST endpoint (/api/quotes) and the CoreWCF PricingService,
/// replacing the body of the legacy PricingService.GetQuote. Products are
/// loaded in one query instead of one query per line.
/// </summary>
public sealed class QuoteService(MeridianDbContext db, IQuoteNumberGenerator numbers)
{
    public async Task<QuoteOutput> CreateQuoteAsync(QuoteInput input, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var customer = await db.Customers.AsNoTracking()
            .SingleOrDefaultAsync(c => c.AccountCode == input.CustomerAccount, ct)
            ?? throw new QuoteValidationException($"Unknown customer account {input.CustomerAccount}");

        var codes = input.Lines.Select(l => l.ProductCode).Distinct().ToList();
        var products = await db.Products.AsNoTracking()
            .Where(p => codes.Contains(p.Code))
            // SQL Server's default collation matched codes case-insensitively in the
            // legacy per-line query, so the in-memory lookup must do the same.
            .ToDictionaryAsync(p => p.Code, StringComparer.OrdinalIgnoreCase, ct);

        var lines = new List<QuoteLine>(input.Lines.Count);
        foreach (var l in input.Lines)
        {
            if (!products.TryGetValue(l.ProductCode, out var p))
            {
                throw new QuoteValidationException($"Unknown product {l.ProductCode}");
            }

            lines.Add(new QuoteLine(p.Code, l.Quantity, p.ListPrice, p.WeightKg));
        }

        var result = PricingRules.Calculate(new QuoteRequest(customer.Tier, customer.Region, lines));
        return new QuoteOutput(numbers.NextQuoteNumber(), result);
    }
}
