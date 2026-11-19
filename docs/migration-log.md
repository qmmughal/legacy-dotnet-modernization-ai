# Migration log (legacy URL → modern URL)

| Legacy | Modern | Gateway route | Status | PR |
|---|---|---|---|---|
| `PricingService.svc` (WCF) | `PricingService.svc` (CoreWCF) + `POST /api/quotes` | separate host | Shadow-tested | #14, #15 |
| `Orders.aspx` | `/orders` (Blazor, InteractiveServer) | `modern-orders` | Live | #21 |
| `Account/Login.aspx` | `/account/login` (static SSR + minimal API) | `modern-login` | Live | #24 |
| `OrderDetail.aspx?id=` | `/orders/{id}` (planned) | legacy fallback until moved | Not started; modern Orders page links to the legacy URL | — |
| `Default.aspx` | — | legacy fallback | Legacy | — |
