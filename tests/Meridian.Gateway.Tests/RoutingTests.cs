using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Meridian.Gateway.Tests;

/// <summary>
/// Starts two stub back ends on real ports ("legacy" and "modern") and checks
/// that the gateway's checked-in route table sends each URL to the right one.
/// This is the test that must change in the same PR as any route move.
/// </summary>
public sealed class RoutingTests : IAsyncLifetime
{
    private WebApplication? _legacy;
    private WebApplication? _modern;
    private WebApplicationFactory<Program>? _gateway;

    public async ValueTask InitializeAsync()
    {
        _legacy = await StartStubAsync("legacy");
        _modern = await StartStubAsync("modern");

        _gateway = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ReverseProxy:Clusters:legacy:Destinations:iis:Address", AddressOf(_legacy));
            b.UseSetting("ReverseProxy:Clusters:modern:Destinations:web:Address", AddressOf(_modern));
            b.UseSetting("Logging:LogLevel:Default", "Warning");
        });
    }

    [Theory]
    [InlineData("/orders", "modern")]
    [InlineData("/orders/42", "modern")]
    [InlineData("/api/quotes", "modern")]
    [InlineData("/account/login", "modern")]
    [InlineData("/account/logout", "modern")]
    [InlineData("/_framework/blazor.web.js", "modern")]
    [InlineData("/Orders.aspx", "legacy")]
    [InlineData("/OrderDetail.aspx?id=42", "legacy")]
    // Routing is case-INSENSITIVE: a "/account/{**rest}" route would have stolen
    // the legacy login page. This case is the regression test for that bug.
    [InlineData("/Account/Login.aspx", "legacy")]
    [InlineData("/Account/ResetPassword.aspx", "legacy")]
    [InlineData("/", "legacy")]
    public async Task Route_GoesToExpectedBackend(string path, string expected)
    {
        using var client = _gateway!.CreateClient();
        var body = await client.GetStringAsync(path, TestContext.Current.CancellationToken);
        Assert.StartsWith(expected + ":", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Gateway_ForwardsOriginalHost()
    {
        using var client = _gateway!.CreateClient();
        var body = await client.GetStringAsync("/orders", TestContext.Current.CancellationToken);
        Assert.Contains("xfh=localhost", body, StringComparison.Ordinal);
    }

    private static async Task<WebApplication> StartStubAsync(string name)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        var app = builder.Build();
        app.Run(ctx => ctx.Response.WriteAsync(
            $"{name}:{ctx.Request.Path}{ctx.Request.QueryString} xfh={ctx.Request.Headers["X-Forwarded-Host"]}"));
        await app.StartAsync();
        return app;
    }

    private static string AddressOf(WebApplication app) =>
        app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First() + "/";

    public async ValueTask DisposeAsync()
    {
        if (_gateway is not null)
        {
            await _gateway.DisposeAsync();
        }

        foreach (var app in new[] { _legacy, _modern })
        {
            if (app is not null)
            {
                await app.StopAsync();
                await app.DisposeAsync();
            }
        }
    }
}
