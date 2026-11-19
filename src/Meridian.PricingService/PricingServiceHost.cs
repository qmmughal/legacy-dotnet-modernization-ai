using CoreWCF.Configuration;
using CoreWCF.Description;
using Meridian.Data;
using Meridian.Domain.Pricing;
using Microsoft.EntityFrameworkCore;

namespace Meridian.PricingService;

public static class PricingServiceHost
{
    public static WebApplication Build(string[] args, Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddServiceModelServices();
        builder.Services.AddServiceModelMetadata();
        builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

        var cs = builder.Configuration.GetConnectionString("Meridian") ?? "Data Source=meridian-dev.db";
        var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
        builder.Services.AddDbContext<MeridianDbContext>(o =>
        {
            if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                o.UseSqlServer(cs);
            }
            else
            {
                o.UseSqlite(cs);
            }
        });

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<ISequenceSource, InMemorySequenceSource>();
        builder.Services.AddSingleton(_ => TimeZoneInfo.FindSystemTimeZoneById(
            builder.Configuration["Business:TimeZone"] ?? "America/Chicago"));
        builder.Services.AddSingleton<IQuoteNumberGenerator, QuoteNumberGenerator>();
        builder.Services.AddScoped<QuoteService>();
        builder.Services.AddScoped<PricingService>();

        configure?.Invoke(builder);

        var app = builder.Build();
        var transportSecurity = app.Configuration.GetValue("Pricing:RequireTransportSecurity", true);

        app.UseServiceModel(serviceBuilder =>
        {
            serviceBuilder.AddService<PricingService>(options =>
                options.DebugBehavior.IncludeExceptionDetailInFaults = app.Environment.IsDevelopment());

            var binding = new CoreWCF.BasicHttpBinding(transportSecurity
                ? CoreWCF.Channels.BasicHttpSecurityMode.Transport
                : CoreWCF.Channels.BasicHttpSecurityMode.None)
            {
                MaxReceivedMessageSize = 1_048_576, // legacy maxReceivedMessageSize
            };

            // Same relative address as IIS hosting gave the .svc file.
            serviceBuilder.AddServiceEndpoint<PricingService, IPricingService>(binding, "/PricingService.svc");
        });

        var metadata = app.Services.GetRequiredService<ServiceMetadataBehavior>();
        metadata.HttpGetEnabled = !transportSecurity;
        metadata.HttpsGetEnabled = transportSecurity;

        return app;
    }
}
