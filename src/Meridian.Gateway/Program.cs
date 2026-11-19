// Strangler-fig front door. Every request for the portal arrives here.
// Routes listed in configuration go to the modern ASP.NET Core app; everything
// else falls through to the legacy Web Forms app. Moving a feature = adding a route.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/gateway/healthz");
app.MapReverseProxy();

app.Run();

public partial class Program;
