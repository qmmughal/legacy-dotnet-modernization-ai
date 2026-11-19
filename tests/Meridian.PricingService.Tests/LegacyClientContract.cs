using System.Runtime.Serialization;
using System.ServiceModel;

namespace Meridian.PricingService.Tests;

// What a legacy consumer's svcutil-generated proxy looks like: synchronous,
// System.ServiceModel attributes, same namespace and names as the 2014 WSDL.
[ServiceContract(Namespace = "http://meridianparts.example/pricing/2014", Name = "IPricingService")]
public interface ILegacyPricingClient
{
    [OperationContract]
    LegacyQuoteResponse GetQuote(LegacyQuoteRequest request);
}

[DataContract(Name = "QuoteRequestDto", Namespace = "http://meridianparts.example/pricing/2014")]
public sealed class LegacyQuoteRequest
{
    [DataMember] public string? CustomerAccount { get; set; }
    [DataMember] public List<LegacyQuoteLine>? Lines { get; set; }
}

[DataContract(Name = "QuoteLineDto", Namespace = "http://meridianparts.example/pricing/2014")]
public sealed class LegacyQuoteLine
{
    [DataMember] public string? ProductCode { get; set; }
    [DataMember] public int Quantity { get; set; }
}

[DataContract(Name = "QuoteResponse", Namespace = "http://meridianparts.example/pricing/2014")]
public sealed class LegacyQuoteResponse
{
    [DataMember] public string? QuoteNumber { get; set; }
    [DataMember] public decimal Subtotal { get; set; }
    [DataMember] public decimal Discount { get; set; }
    [DataMember] public decimal Freight { get; set; }
    [DataMember] public decimal Tax { get; set; }
    [DataMember] public decimal Total { get; set; }
#pragma warning disable CA1819
    [DataMember] public string[]? Warnings { get; set; }
#pragma warning restore CA1819
}
