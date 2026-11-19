using Meridian.Data;
using Meridian.Web.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Web.Tests;

/// <summary>
/// Boots the real Meridian.Web pipeline (auth, antiforgery, endpoints) against
/// a private in-memory SQLite database seeded with one migrated legacy user.
/// </summary>
public sealed class PortalFactory : WebApplicationFactory<Program>
{
    // Independently computed (Python hashlib) SqlMembershipProvider SHA1 hash of
    // "Spring2014!" with salt bytes 0x01..0x10. Not produced by the code under test.
    public const string LegacySalt = "AQIDBAUGBwgJCgsMDQ4PEA==";
    public const string LegacyHash = "bEb6Nxz6EcmX7Tu37td2FqYyZz0=";
    public const string UserName = "s.khan";
    public const string Password = "Spring2014!";

    private readonly string _dbName = "web-tests-" + Guid.NewGuid().ToString("N");
    private SqliteConnection? _keepAlive;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var cs = $"Data Source=file:{_dbName}?mode=memory&cache=shared";
        _keepAlive = new SqliteConnection(cs);
        _keepAlive.Open();

        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Meridian", cs);
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("Database:EnsureCreated", "true");
        builder.UseSetting("RemoteApp:Uri", "");
        builder.UseSetting("Logging:LogLevel:Default", "Warning");
        builder.UseSetting("Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning");
    }

    public async Task SeedLegacyUserAsync()
    {
        using var scope = Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<PortalUser>>();
        if (await users.FindByNameAsync(UserName) is not null)
        {
            return;
        }

        var user = new PortalUser
        {
            UserName = UserName,
            Email = "s.khan@meridianparts.example",
            Region = "SOUTH",
            SecurityStamp = Guid.NewGuid().ToString(),
            PasswordHash = $"{LegacyMembershipPasswordHasher<PortalUser>.Prefix}{LegacySalt}:{LegacyHash}",
        };
        var result = await users.CreateAsync(user); // no password: keeps the migrated hash as-is
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public HttpClient CreatePortalClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost"),
    });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _keepAlive?.Dispose();
        }
    }
}
