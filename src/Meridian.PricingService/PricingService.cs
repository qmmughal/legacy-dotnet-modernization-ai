using CoreWCF;
using Meridian.Data;

namespace Meridian.PricingService;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
public sealed partial class PricingService(QuoteService quotes, ILogger<PricingService> logger) : IPricingService
{
    public async Task<QuoteResponse> GetQuoteAsync(QuoteRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            var input = new QuoteInput(
                request.CustomerAccount ?? string.Empty,
                (request.Lines ?? []).Select(l => new QuoteLineInput(l.ProductCode ?? string.Empty, l.Quantity)).ToList());

            var output = await quotes.CreateQuoteAsync(input);
            var r = output.Result;

            // Replaces the App_Data/quotes.log file write (HttpContext.Current.Server.MapPath).
            LogQuote(logger, output.QuoteNumber, request.CustomerAccount, r.Total);

            return new QuoteResponse
            {
                QuoteNumber = output.QuoteNumber,
                Subtotal = r.Subtotal,
                Discount = r.Discount,
                Freight = r.Freight,
                Tax = r.Tax,
                Total = r.Total,
                Warnings = [.. r.Warnings],
            };
        }
        catch (QuoteValidationException ex)
        {
            // Legacy threw FaultException with the same messages; clients parse them.
            throw new FaultException(ex.Message);
        }
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information,
        Message = "Quote {QuoteNumber} for {Account}: total {Total}")]
    private static partial void LogQuote(ILogger logger, string quoteNumber, string? account, decimal total);
}
