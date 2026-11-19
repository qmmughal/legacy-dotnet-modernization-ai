using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using MeridianPortal.Core.Pricing;
using MeridianPortal.Data;

namespace MeridianPortal.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class PricingService : IPricingService
    {
        public QuoteResponse GetQuote(QuoteRequestDto request)
        {
            using (var db = new MeridianContext())
            {
                var customer = db.Customers.SingleOrDefault(c => c.AccountCode == request.CustomerAccount);
                if (customer == null)
                    throw new FaultException("Unknown customer account " + request.CustomerAccount);

                var quote = new QuoteRequest { CustomerTier = customer.Tier, Region = customer.Region };
                foreach (var l in request.Lines)
                {
                    // N+1 query: one round trip per line. Characterized, not "fixed", in the first slice.
                    var p = db.Products.SingleOrDefault(x => x.Code == l.ProductCode);
                    if (p == null) throw new FaultException("Unknown product " + l.ProductCode);
                    quote.Lines.Add(new QuoteLine { ProductCode = p.Code, Quantity = l.Quantity, UnitPrice = p.ListPrice, WeightKg = p.WeightKg });
                }

                var r = PricingEngine.Calculate(quote);
                // Audit trail written to a file under App_Data via HttpContext. Works only when
                // hosted in IIS with ASP.NET compatibility mode enabled.
                var path = HttpContext.Current.Server.MapPath("~/App_Data/quotes.log");
                System.IO.File.AppendAllText(path, DateTime.Now + " " + request.CustomerAccount + " " + r.Total + Environment.NewLine);

                return new QuoteResponse
                {
                    QuoteNumber = QuoteNumberGenerator.Next(),
                    Subtotal = r.Subtotal, Discount = r.Discount, Freight = r.Freight,
                    Tax = r.Tax, Total = r.Total, Warnings = r.Warnings.ToArray()
                };
            }
        }
    }
}
