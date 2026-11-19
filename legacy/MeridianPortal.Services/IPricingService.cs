using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace MeridianPortal.Services
{
    [ServiceContract(Namespace = "http://meridianparts.example/pricing/2014")]
    public interface IPricingService
    {
        [OperationContract]
        QuoteResponse GetQuote(QuoteRequestDto request);
    }

    [DataContract(Namespace = "http://meridianparts.example/pricing/2014")]
    public class QuoteRequestDto
    {
        [DataMember] public string CustomerAccount { get; set; }
        [DataMember] public List<QuoteLineDto> Lines { get; set; }
    }

    [DataContract(Namespace = "http://meridianparts.example/pricing/2014")]
    public class QuoteLineDto
    {
        [DataMember] public string ProductCode { get; set; }
        [DataMember] public int Quantity { get; set; }
    }

    [DataContract(Namespace = "http://meridianparts.example/pricing/2014")]
    public class QuoteResponse
    {
        [DataMember] public string QuoteNumber { get; set; }
        [DataMember] public decimal Subtotal { get; set; }
        [DataMember] public decimal Discount { get; set; }
        [DataMember] public decimal Freight { get; set; }
        [DataMember] public decimal Tax { get; set; }
        [DataMember] public decimal Total { get; set; }
        [DataMember] public string[] Warnings { get; set; }
    }
}
