using Meridian.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace Meridian.PricingService.Tests;

/// <summary>Runs the CoreWCF host on a real Kestrel port so real SOAP clients can call it.</summary>
public sealed class PricingServiceFixture : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private WebApplication? _app;

    public Uri ServiceUri { get; private set; } = default!;

    public async ValueTask InitializeAsync()
    {
        _connection.Open();
        _app = PricingServiceHost.Build(
            ["--urls", "http://127.0.0.1:0", "--Pricing:RequireTransportSecurity", "false", "--environment", "Production"],
            builder =>
            {
                builder.Logging.SetMinimumLevel(LogLevel.Warning);
                builder.Services.RemoveAll<DbContextOptions<MeridianDbContext>>();
                builder.Services.AddDbContext<MeridianDbContext>(o => o.UseSqlite(_connection));
                builder.Services.RemoveAll<TimeProvider>();
                builder.Services.AddSingleton<TimeProvider>(
                    new FakeTimeProvider(new DateTimeOffset(2026, 11, 19, 15, 0, 0, TimeSpan.Zero)));
            });

        using (var scope = _app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MeridianDbContext>();
            db.Database.EnsureCreated();
            SeedData.Populate(db);
        }

        await _app.StartAsync();
        var address = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        ServiceUri = new Uri(address + "/PricingService.svc");
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }

        await _connection.DisposeAsync();
    }
}
