---
applyTo: "tests/**/*.cs"
---
# Tests

- xUnit v3 on Microsoft.Testing.Platform. Use `TestContext.Current.CancellationToken`.
- Never edit files under `golden/` or ending in `.approved.txt` / `.approved.json`.
- New behaviour tests describe legacy behaviour in their names, for example
  `NullRegionUser_SeesNullRegionCustomers_LegacyNullSemantics`.
- Do not mock what you can run: prefer in-memory SQLite, WebApplicationFactory,
  and real Kestrel ports over mocks of framework types.
