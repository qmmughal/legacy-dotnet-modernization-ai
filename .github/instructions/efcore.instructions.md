---
applyTo: "src/Meridian.Data/**/*.cs"
---
# EF Core mapping onto the legacy schema

- Map to existing tables and columns (`tblOrder`, `tblCustomer`, ...). Never
  rename tables, change column types, or add migrations without a task card
  that says "schema change" and names the DBA reviewer.
- Money is `decimal(18,2)` (configured in `ConfigureConventions`).
- No lazy loading. Use projections; prove query counts with `CommandCounter`
  in `tests/Meridian.Data.Tests` when replacing an N+1 pattern.
- Preserve C# null semantics in comparisons (EF6 default). Do not "simplify"
  `x.Region == region` into something that changes null behaviour.
