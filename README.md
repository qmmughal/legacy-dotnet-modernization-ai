# legacy-dotnet-modernization-ai

Companion repository for **Modernizing Legacy .NET with AI Agents** by Qaiser Mehmood
(2026). It contains a small legacy ASP.NET Web Forms / WCF / EF6 portal ("before"),
its modernized .NET 10 replacement ("after"), and the tests, agent contracts, and
scripts the book uses to move from one to the other safely.

> Fictional company, fictional data. Patterns, not production code.

## What is here

```
legacy/                       .NET Framework 4.8 "before" (read-only; does not build on Linux)
src/
  Meridian.Legacy.Pricing     links legacy pricing source, unchanged, into netstandard2.0
  Meridian.Domain             modern pricing rules + quote numbers (TimeProvider)
  Meridian.Data               EF Core 10 onto the legacy schema + Identity tables
  Meridian.Web                Blazor Web App, minimal APIs, ASP.NET Core Identity
  Meridian.PricingService     CoreWCF host, wire-compatible with the 2014 SOAP contract
  Meridian.Gateway            YARP strangler-fig front door
tests/
  Meridian.Characterization.Tests   golden master (614 cases), mutation guards, snapshot
  Meridian.Data.Tests               legacy schema mapping, query semantics, N+1 removal
  Meridian.Web.Tests                auth, antiforgery, legacy password rehash, API parity
  Meridian.PricingService.Tests     legacy SOAP proxy, recorded envelope, WSDL
  Meridian.Gateway.Tests            route table: which URLs go legacy vs modern
benchmarks/
  Meridian.Benchmarks               BenchmarkDotNet: legacy vs modern pricing over the corpus
scripts/
  inventory.cs                      .NET 10 file-based app: legacy estate inventory
  check-golden-unchanged.sh         CI guard for human-owned golden files
  check-diff-budget.sh              CI guard for pull request size
AGENTS.md, CLAUDE.md, .github/, .cursor/, .claude/   agent contracts and review gates
docs/                               inventory, decisions, quirks, migration log, AI-use policy
deploy/web.Dockerfile               chiseled container image
```

## Requirements

- .NET SDK 10.0.404 or a later 10.0 feature band (`global.json`).
- Nothing else for build and tests: SQLite is in-memory, SOAP and proxy tests use
  local Kestrel ports.

## Build, test, run

```bash
dotnet build Meridian.slnx -warnaserror
dotnet test --solution Meridian.slnx          # 47 tests (Microsoft.Testing.Platform)
dotnet run scripts/inventory.cs -- legacy docs/inventory.md
dotnet run --project src/Meridian.Web          # https://localhost:7180, SQLite dev DB
dotnet run -c Release --project benchmarks/Meridian.Benchmarks -- --filter '*'
```

The web app creates `meridian-dev.db` with three fictional customers and orders.
To sign in during development, create a user with `UserManager` or use the test
`PortalFactory` pattern in `tests/Meridian.Web.Tests`.

## Verified versions (19 November 2026)

| Component | Version |
|---|---|
| .NET SDK / runtime | 10.0.404 / 10.0.15 (LTS, supported to 14 Nov 2028) |
| EF Core, ASP.NET Core packages | 10.0.15 |
| CoreWCF | 1.9.1 |
| YARP (Yarp.ReverseProxy) | 2.3.0 |
| System.Web adapters | 2.3.0 |
| System.ServiceModel.* (client) | 10.1.0 |
| xUnit v3 | 4.0.3 |
| BenchmarkDotNet | 0.15.8 |

## What is NOT tested here

- The `legacy/` projects themselves (they need Windows + .NET Framework 4.8).
  On a Windows machine you can add a `net48` target to the characterization
  test project to capture the golden master on the original runtime.
- SQL Server: tests use SQLite. The mapping tests check the SQL Server model
  (column types) without a server. Run the data tests against a real SQL Server
  copy of production schema before cut-over.
- The System.Web adapters remote-app path (`RemoteApp:Uri`) compiles and is
  wired, but needs a running ASP.NET Framework app to exercise.

## License

MIT for the code. The book text is copyright © 2026 Qaiser Mehmood.
