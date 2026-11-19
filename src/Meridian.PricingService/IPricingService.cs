using System.Runtime.Serialization;
using CoreWCF;

namespace Meridian.PricingService;

// Wire contract copied from legacy/MeridianPortal.Services/IPricingService.cs.
// Namespace, contract name, operation name, and data member names must not
// change: existing SOAP clients (ERP integration, two partner portals) depend
// on them. Operation is async on the server; the WSDL is unchanged because
// Name = "GetQuote" pins the operation name and SOAP action.
[ServiceContract(Namespace = Ns.Pricing, Name = "IPricingService")]
public interface IPricingService
{
    [OperationContract(Name = "GetQuote")]
    Task<QuoteResponse> GetQuoteAsync(QuoteRequestDto request);
}

internal static class Ns
{
    public const string Pricing = "http://meridianparts.example/pricing/2014";
}

[DataContract(Namespace = Ns.Pricing)]
public sealed class QuoteRequestDto
{
    [DataMember] public string? CustomerAccount { get; set; }
    [DataMember] public List<QuoteLineDto>? Lines { get; set; }
}

[DataContract(Namespace = Ns.Pricing)]
public sealed class QuoteLineDto
{
    [DataMember] public string? ProductCode { get; set; }
    [DataMember] public int Quantity { get; set; }
}

[DataContract(Namespace = Ns.Pricing)]
public sealed class QuoteResponse
{
    [DataMember] public string? QuoteNumber { get; set; }
    [DataMember] public decimal Subtotal { get; set; }
    [DataMember] public decimal Discount { get; set; }
    [DataMember] public decimal Freight { get; set; }
    [DataMember] public decimal Tax { get; set; }
    [DataMember] public decimal Total { get; set; }
#pragma warning disable CA1819 // Arrays are part of the legacy wire contract (string[] Warnings).
    [DataMember] public string[]? Warnings { get; set; }
#pragma warning restore CA1819
}
