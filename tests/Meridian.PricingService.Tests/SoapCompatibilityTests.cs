using System.ServiceModel;
using System.Text;
using System.Xml.Linq;

namespace Meridian.PricingService.Tests;

public sealed class SoapCompatibilityTests(PricingServiceFixture service) : IClassFixture<PricingServiceFixture>
{
    private static readonly XNamespace Pricing = "http://meridianparts.example/pricing/2014";
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ILegacyPricingClient CreateLegacyClient()
    {
        var factory = new ChannelFactory<ILegacyPricingClient>(
            new BasicHttpBinding(BasicHttpSecurityMode.None), new EndpointAddress(service.ServiceUri));
        return factory.CreateChannel();
    }

    [Fact]
    public void LegacyProxy_CanCallCoreWcfService()
    {
        var client = CreateLegacyClient();
        var response = client.GetQuote(new LegacyQuoteRequest
        {
            CustomerAccount = "C-1001",
            Lines = [new() { ProductCode = "VLV-200", Quantity = 2 }, new() { ProductCode = "HAZ-SOLV-5", Quantity = 1 }],
        });
        ((IClientChannel)client).Close();

        Assert.StartsWith("Q-20261119-", response.QuoteNumber, StringComparison.Ordinal);
        Assert.Equal(297.95m, response.Subtotal);          // 259.80 + 38.15
        Assert.Equal(22.35m, response.Discount);           // GOLD 7.5% = 22.34625 -> 22.35
        Assert.Equal(["Hazardous item HAZ-SOLV-5 requires manual freight approval"], response.Warnings!);
    }

    [Fact]
    public void UnknownCustomer_ReturnsSoapFault_WithLegacyMessage()
    {
        var client = CreateLegacyClient();
        var fault = Assert.Throws<FaultException>(() => client.GetQuote(new LegacyQuoteRequest
        {
            CustomerAccount = "C-0000",
            Lines = [new() { ProductCode = "VLV-200", Quantity = 1 }],
        }));
        ((IClientChannel)client).Abort();

        Assert.Equal("Unknown customer account C-0000", fault.Message);
    }

    [Fact]
    public async Task RecordedLegacyRequest_GetsWireCompatibleResponse()
    {
        using var http = new HttpClient();
        var body = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "recorded", "GetQuote.request.xml"), Ct);
        using var content = new StringContent(body, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", "\"http://meridianparts.example/pricing/2014/IPricingService/GetQuote\"");

        var response = await http.PostAsync(service.ServiceUri, content, Ct);
        var xml = XDocument.Parse(await response.Content.ReadAsStringAsync(Ct));

        Assert.True(response.IsSuccessStatusCode, xml.ToString());
        var result = xml.Descendants(Pricing + "GetQuoteResult").Single();
        Assert.NotNull(xml.Descendants(Pricing + "GetQuoteResponse").SingleOrDefault());
        Assert.Equal("297.95", result.Elements().Single(e => e.Name.LocalName == "Subtotal").Value);
    }

    [Fact]
    public async Task Wsdl_IsPublished_WithLegacyOperationName()
    {
        using var http = new HttpClient();
        var wsdl = await http.GetStringAsync(service.ServiceUri + "?wsdl", Ct);
        Assert.Contains("name=\"GetQuote\"", wsdl, StringComparison.Ordinal);
        Assert.DoesNotContain("GetQuoteAsync", wsdl, StringComparison.Ordinal);
    }
}
