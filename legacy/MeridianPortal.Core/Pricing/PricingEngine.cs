using System;

namespace MeridianPortal.Core.Pricing
{
    // Written 2014, amended 2017 (export region) and 2019 (hazardous warning).
    // Business owner: Sales Operations. Do not change without sign-off.
    public static class PricingEngine
    {
        public static QuoteResult Calculate(QuoteRequest request)
        {
            var result = new QuoteResult();
            decimal subtotal = 0m;
            double totalWeight = 0;

            foreach (var line in request.Lines)
            {
                if (line.Quantity <= 0)
                {
                    result.Warnings.Add("Ignored line " + line.ProductCode + ": quantity " + line.Quantity);
                    continue;
                }

                decimal lineTotal = line.UnitPrice * line.Quantity;
                if (line.Quantity >= 100)
                    lineTotal = lineTotal * 0.90m;
                else if (line.Quantity >= 50)
                    lineTotal = lineTotal * 0.95m;

                subtotal += Math.Round(lineTotal, 2);
                totalWeight += line.WeightKg * line.Quantity;

                if (line.ProductCode != null && line.ProductCode.ToUpper().StartsWith("HAZ"))
                    result.Warnings.Add("Hazardous item " + line.ProductCode + " requires manual freight approval");
            }

            decimal tierRate = 0m;
            if (request.CustomerTier == "GOLD") tierRate = 0.075m;
            else if (request.CustomerTier == "SILVER") tierRate = 0.03m;

            decimal discount = Math.Round(subtotal * tierRate, 2);
            decimal net = subtotal - discount;

            decimal freight;
            if (net > 500m)
                freight = 0m;
            else
                freight = Math.Round(25m + (decimal)(totalWeight * 0.35), 2);

            decimal taxRate;
            switch (request.Region)
            {
                case "NORTH": taxRate = 0.08m; break;
                case "SOUTH": taxRate = 0.0725m; break;
                case "EXPORT": taxRate = 0m; break;
                default: taxRate = 0.08m; break;
            }

            decimal tax = Math.Round((net + freight) * taxRate, 2);

            result.Subtotal = subtotal;
            result.Discount = discount;
            result.Freight = freight;
            result.Tax = tax;
            result.Total = net + freight + tax;
            result.DisplayTotal = result.Total.ToString("C");
            return result;
        }
    }
}
