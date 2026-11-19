# legacy/ — the "before" system (ASP.NET Web Forms, WCF, EF6 on .NET Framework 4.8)

This folder is a deliberately small but realistic slice of a legacy line-of-business
application: the Meridian Parts order portal. It is here so you can read it, run an
inventory over it, and see what the modernized code in `src/` replaces.

**It does not build on Linux or macOS.** Web Forms, WCF service hosting, and
`System.Web` exist only on .NET Framework, which needs Windows and Visual Studio
(or MSBuild from the .NET Framework developer pack). The project files are omitted
on purpose; the companion repository does not pretend to compile code it cannot test.

One part *is* compiled and tested everywhere: the pricing rules in
`MeridianPortal.Core/Pricing/*.cs`. They are written in C# 7.3 with no `System.Web`
dependency, so `src/Meridian.Legacy.Pricing` links those exact source files into a
`netstandard2.0` library. The characterization tests in
`tests/Meridian.Characterization.Tests` run the legacy rules and the modern rules
side by side over the same input corpus.

| Folder | Legacy technology | Modern replacement in `src/` |
|---|---|---|
| `MeridianPortal.Web` | ASP.NET Web Forms, Forms authentication, SqlMembershipProvider, Session | `Meridian.Web` (Blazor Web App, ASP.NET Core Identity) |
| `MeridianPortal.Services` | WCF service (`.svc`, BasicHttpBinding) | `Meridian.PricingService` (CoreWCF) and `/api/quotes` (REST) |
| `MeridianPortal.Data` | Entity Framework 6 code-first, `DbModelBuilder` | `Meridian.Data` (EF Core 10) |
| `MeridianPortal.Core` | Business rules | `Meridian.Domain` |
