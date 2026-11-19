# AGENTS.md — Meridian Parts modernization

You are helping migrate a .NET Framework 4.8 portal (Web Forms, WCF, EF6) to
.NET 10. Humans review and merge every change. Your job is small, verifiable
steps, not rewrites.

## Repository map
- `legacy/` — the running production system. READ-ONLY. Never edit, format, or
  "fix" anything here. It does not build on Linux; do not try.
- `src/Meridian.Legacy.Pricing` — compiles legacy pricing source unchanged for tests.
- `src/Meridian.Domain` — modern business rules (pricing, quote numbers).
- `src/Meridian.Data` — EF Core 10 mapping onto the EXISTING legacy schema.
- `src/Meridian.Web` — Blazor Web App + minimal APIs + ASP.NET Core Identity.
- `src/Meridian.PricingService` — CoreWCF host; SOAP contract must stay wire-compatible.
- `src/Meridian.Gateway` — YARP front door; route table decides legacy vs modern.
- `tests/` — characterization, data, web, SOAP, and gateway tests.

## Build and test (must pass before you say a task is done)
```bash
dotnet build Meridian.slnx -warnaserror
dotnet test --solution Meridian.slnx
```
SDK is pinned in `global.json` (10.0.404, rollForward latestFeature). Tests run on
Microsoft.Testing.Platform (`dotnet test --project <path>` for one project).

## Hard rules
1. Behaviour first. Do not change observable behaviour (numbers, strings, status
   codes, SOAP names, routes, redirects, rounding, culture, null handling) unless
   the task card says so explicitly and names the approving person.
2. Never edit `tests/**/golden/*` or `*.approved.*` files. If a golden test
   fails, stop and report the diff. Re-recording is a human decision.
3. Never delete, skip, or weaken a test to make a build pass. No `[Fact(Skip=...)]`,
   no loosened assertions, no `#pragma warning disable` without a written reason.
4. Package versions come from `Directory.Packages.props` only. Do not add a
   package or bump a version unless the task card asks for it. Never invent a
   version number: if unsure, say so.
5. Do not touch auth, antiforgery, cookie, CORS, or authorization code unless
   the task is about auth. Never remove `[Authorize]`, `RequireAuthorization()`,
   or `<AntiforgeryToken />`.
6. No secrets in code or config. Connection strings and keys come from
   configuration/user-secrets/environment.
7. Keep diffs small: one task card = one pull request, ideally under 400 changed
   lines excluding generated files. If it grows, stop and propose a split.

## Conventions
- C# latest, nullable enabled, warnings are errors, file-scoped namespaces.
- Money is `decimal`. Use explicit `MidpointRounding` and explicit `CultureInfo`
  wherever the legacy code relied on defaults; add a `QUIRK-n` comment and an
  entry in `docs/pricing-quirks.md` when preserving odd behaviour on purpose.
- Time comes from `TimeProvider`; never call `DateTime.Now` in new code.
- Data access: async EF Core, `AsNoTracking()` for reads, projections over
  lazy loading. Blazor components use `IDbContextFactory<MeridianDbContext>`.
- Logging: `ILogger<T>` with `[LoggerMessage]` source-generated methods.

## When you finish a task
Write in the PR description: what changed, which tests prove behaviour is
unchanged, anything you were unsure about, and any API you used for the first
time with a link to its documentation.
