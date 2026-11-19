using System.Collections.Generic;

namespace MeridianPortal.Core.Pricing
{
    // Legacy DTOs: public fields, mutable, no nullability. Kept as-is so the
    // characterization tests exercise the real legacy shapes.
    public class QuoteLine
    {
        public string ProductCode;
        public int Quantity;
        public decimal UnitPrice;
        public double WeightKg;
    }

    public class QuoteRequest
    {
        public string CustomerTier;
        public string Region;
        public List<QuoteLine> Lines = new List<QuoteLine>();
    }

    public class QuoteResult
    {
        public decimal Subtotal;
        public decimal Discount;
        public decimal Freight;
        public decimal Tax;
        public decimal Total;
        public string DisplayTotal;
        public List<string> Warnings = new List<string>();
    }
}
