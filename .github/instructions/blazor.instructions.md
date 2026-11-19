---
applyTo: "src/Meridian.Web/Components/**/*.razor,src/Meridian.Web/Components/**/*.cs"
---
# Blazor components (replacing Web Forms pages)

- One Web Forms page becomes one routable component with the same URL meaning;
  record the old and new URL in `docs/migration-log.md`.
- Interactive pages use `@rendermode InteractiveServer`. Login and other
  cookie-writing pages stay static SSR and post to minimal API endpoints.
- Never use a scoped `MeridianDbContext` in an interactive component; inject
  `IDbContextFactory<MeridianDbContext>` and dispose each context.
- Replace `Session[...]` with query string, component state, or claims. Do not
  add `AddSession()` without an approved task card.
- `GridView` maps to `QuickGrid` + `Paginator`; keep column order and formats
  (`{0:d}` → `Format="d"`, `{0:C}` → `Format="C"`).
- Keep `@attribute [Authorize]` on every page that the legacy app protected.
