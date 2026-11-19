using Meridian.Data;

namespace Meridian.Web.Endpoints;

public sealed record QuoteLineRequest(string ProductCode, int Quantity);

public sealed record CreateQuoteRequest(string CustomerAccount, IReadOnlyList<QuoteLineRequest> Lines);

public sealed record QuoteResponse(
    string QuoteNumber, decimal Subtotal, decimal Discount, decimal Freight,
    decimal Tax, decimal Total, string DisplayTotal, IReadOnlyList<string> Warnings);

public static class QuoteEndpoints
{
    public static IEndpointRouteBuilder MapQuoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quotes").RequireAuthorization();

        group.MapPost("/", async (CreateQuoteRequest request, QuoteService quotes, CancellationToken ct) =>
        {
            if (request.Lines is null || request.Lines.Count == 0)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["lines"] = ["At least one line is required."],
                });
            }

            try
            {
                var input = new QuoteInput(request.CustomerAccount,
                    request.Lines.Select(l => new QuoteLineInput(l.ProductCode, l.Quantity)).ToList());
                var output = await quotes.CreateQuoteAsync(input, ct);
                var r = output.Result;
                return Results.Ok(new QuoteResponse(output.QuoteNumber, r.Subtotal, r.Discount, r.Freight,
                    r.Tax, r.Total, r.DisplayTotal, r.Warnings));
            }
            catch (QuoteValidationException ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        });

        return app;
    }
}
