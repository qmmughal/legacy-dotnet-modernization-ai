using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Meridian.Data;
using Meridian.Web.Endpoints;
using Meridian.Web.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Legacy = MeridianPortal.Core.Pricing;

namespace Meridian.Web.Tests;

public sealed partial class PortalTests(PortalFactory factory) : IClassFixture<PortalFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Orders_RequiresSignIn_RedirectsToLogin()
    {
        using var client = factory.CreatePortalClient();
        var response = await client.GetAsync("/orders", Ct);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("https://localhost/account/login", response.Headers.Location!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Api_WithoutSignIn_Returns401_NotARedirect()
    {
        using var client = factory.CreatePortalClient();
        var response = await client.PostAsJsonAsync("/api/quotes",
            new CreateQuoteRequest("C-1001", [new("VLV-200", 1)]), Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithoutAntiforgeryToken_IsRejected()
    {
        await factory.SeedLegacyUserAsync();
        using var client = factory.CreatePortalClient();
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["userName"] = PortalFactory.UserName, ["password"] = PortalFactory.Password,
        });

        var response = await client.PostAsync("/account/login", form, Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithMigratedMembershipPassword_Succeeds_AndRehashes()
    {
        await factory.SeedLegacyUserAsync();
        using var client = factory.CreatePortalClient();

        var response = await SignInAsync(client, returnUrl: "/orders");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/orders", response.Headers.Location!.ToString());

        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<PortalUser>>();
        var user = await users.FindByNameAsync(PortalFactory.UserName);
        Assert.False(user!.PasswordHash!.StartsWith(LegacyMembershipPasswordHasher<PortalUser>.Prefix, StringComparison.Ordinal),
            "Legacy hash should be replaced by an Identity v3 hash after first sign-in.");
    }

    [Fact]
    public async Task Login_RejectsOffSiteReturnUrl()
    {
        await factory.SeedLegacyUserAsync();
        using var client = factory.CreatePortalClient();

        var response = await SignInAsync(client, returnUrl: "//evil.example/phish");
        Assert.Equal("/orders", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task QuoteApi_MatchesLegacyEngine_ForSameInputs()
    {
        await factory.SeedLegacyUserAsync();
        using var client = factory.CreatePortalClient();
        await SignInAsync(client, "/orders");

        var response = await client.PostAsJsonAsync("/api/quotes",
            new CreateQuoteRequest("C-2002", [new("VLV-200", 2), new("HAZ-SOLV-5", 60)]), Ct);
        response.EnsureSuccessStatusCode();
        var quote = (await response.Content.ReadFromJsonAsync<QuoteResponse>(Ct))!;

        // Southbank Pumps: SILVER, SOUTH. Same inputs through the untouched legacy engine.
        var legacyRequest = new Legacy.QuoteRequest { CustomerTier = "SILVER", Region = "SOUTH" };
        legacyRequest.Lines.Add(new Legacy.QuoteLine { ProductCode = "VLV-200", Quantity = 2, UnitPrice = 129.90m, WeightKg = 6.4 });
        legacyRequest.Lines.Add(new Legacy.QuoteLine { ProductCode = "HAZ-SOLV-5", Quantity = 60, UnitPrice = 38.15m, WeightKg = 5.2 });
        var expected = Legacy.PricingEngine.Calculate(legacyRequest);

        Assert.Equal(expected.Total, quote.Total);
        Assert.Equal(expected.Tax, quote.Tax);
        Assert.Equal(expected.Warnings, quote.Warnings);
    }

    [Fact]
    public async Task OrdersPage_Prerenders_OnlyTheUsersRegion()
    {
        await factory.SeedLegacyUserAsync();
        using var client = factory.CreatePortalClient();
        await SignInAsync(client, "/orders");

        var html = await client.GetStringAsync("/orders", Ct);
        Assert.Contains("Southbank Pumps", html, StringComparison.Ordinal);       // SOUTH
        Assert.DoesNotContain("Northwind Fabrication", html, StringComparison.Ordinal); // NORTH
        Assert.Contains("$172.04", html, StringComparison.Ordinal);               // en-US {0:C}, as legacy
        Assert.Contains("/OrderDetail.aspx?id=", html, StringComparison.Ordinal); // detail still legacy
    }

    [Fact]
    public async Task Health_IsReported()
    {
        using var client = factory.CreatePortalClient();
        var response = await client.GetAsync("/healthz", Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> SignInAsync(HttpClient client, string returnUrl)
    {
        var page = await client.GetStringAsync("/account/login", Ct);
        var token = AntiforgeryField().Match(page).Groups[1].Value;
        Assert.False(string.IsNullOrEmpty(token), "Login page must render an antiforgery token.");

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["userName"] = PortalFactory.UserName,
            ["password"] = PortalFactory.Password,
            ["returnUrl"] = returnUrl,
        });
        return await client.PostAsync("/account/login", form, Ct);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryField();
}
