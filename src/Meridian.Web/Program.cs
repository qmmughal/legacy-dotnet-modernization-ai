using Meridian.Data;
using Meridian.Domain.Pricing;
using Meridian.Web.Components;
using Meridian.Web.Endpoints;
using Meridian.Web.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---- Data -------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("Meridian")
    ?? throw new InvalidOperationException("Connection string 'Meridian' is missing.");
var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";

// Blazor Server components get short-lived contexts from the factory; request-
// scoped code (Identity, minimal APIs) gets a scoped context built by the factory.
builder.Services.AddDbContextFactory<MeridianDbContext>(options =>
{
    if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<MeridianDbContext>>().CreateDbContext());

// ---- Domain services --------------------------------------------------------
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ISequenceSource, InMemorySequenceSource>(); // swap for a DB sequence in production
builder.Services.AddSingleton(_ => TimeZoneInfo.FindSystemTimeZoneById(
    builder.Configuration["Business:TimeZone"] ?? "America/Chicago"));
builder.Services.AddSingleton<IQuoteNumberGenerator, QuoteNumberGenerator>();
builder.Services.AddScoped<QuoteService>();

// ---- Identity (replaces Forms auth + SqlMembershipProvider) -----------------
builder.Services.AddScoped<IPasswordHasher<PortalUser>, LegacyMembershipPasswordHasher<PortalUser>>();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
builder.Services.AddIdentityCore<PortalUser>(options =>
    {
        options.Password.RequiredLength = 8;              // legacy minRequiredPasswordLength
        options.Password.RequireNonAlphanumeric = false;  // legacy minRequiredNonalphanumericCharacters=0
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MeridianDbContext>()
    .AddSignInManager()
    .AddClaimsPrincipalFactory<PortalClaimsFactory>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);   // legacy forms timeout
    options.SlidingExpiration = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // legacy requireSSL="true"

    // APIs answer 401/403; only pages redirect to the login form.
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<MeridianDbContext>();

// ---- Optional: coexistence with the legacy app (strangler fig) ---------------
// When RemoteApp:Uri is set, unmatched routes are forwarded to the Web Forms app.
var remoteAppUri = builder.Configuration["RemoteApp:Uri"];
if (!string.IsNullOrEmpty(remoteAppUri))
{
    builder.Services.AddSystemWebAdapters()
        .AddRemoteAppClient(options =>
        {
            options.RemoteAppUrl = new Uri(remoteAppUri);
            options.ApiKey = builder.Configuration["RemoteApp:ApiKey"]
                ?? throw new InvalidOperationException("RemoteApp:ApiKey is required when RemoteApp:Uri is set.");
        });
    builder.Services.AddHttpForwarder();
}

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:EnsureCreated"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MeridianDbContext>();
    db.Database.EnsureCreated();
    SeedData.Populate(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Legacy web.config: <globalization culture="en-US" uiCulture="en-US" />. Pages
// format dates and money ({0:d}, {0:C}) with the request culture, so pin it.
// Without this, a container's default culture would change what users see.
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures("en-US")
    .AddSupportedUICultures("en-US"));

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
if (!string.IsNullOrEmpty(remoteAppUri))
{
    app.UseSystemWebAdapters();
}

app.MapStaticAssets();
app.MapHealthChecks("/healthz");
app.MapQuoteEndpoints();
app.MapAccountEndpoints();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

if (!string.IsNullOrEmpty(remoteAppUri))
{
    app.MapForwarder("/{**catch-all}", remoteAppUri)
        .WithOrder(int.MaxValue)  // lowest priority: only routes nobody else claimed
        .ShortCircuit();
}

app.Run();

public partial class Program;
