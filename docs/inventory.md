# Legacy inventory: `legacy`

| Area | Signal | Risk | Files | Occurrences |
|---|---|---|---|---|
| Auth | Forms auth / Membership | High | 2 | 3 |
| Auth | Roles | Medium | 2 | 2 |
| Auth | machineKey | High | 1 | 1 |
| Config | appSettings | Low | 1 | 1 |
| EF6 | EF6 namespaces | High | 1 | 3 |
| EF6 | DbModelBuilder / EntityTypeConfiguration | Medium | 1 | 3 |
| Logging | log4net / Trace | Low | 3 | 10 |
| Semantics | Culture-sensitive casing | Medium | 1 | 1 |
| Semantics | Culture-sensitive formatting | Medium | 2 | 2 |
| Semantics | Midpoint rounding (default ToEven) | Medium | 1 | 2 |
| Semantics | DateTime.Now (local time) | Medium | 2 | 2 |
| System.Web | HttpContext.Current | High | 1 | 1 |
| System.Web | Server.MapPath | Medium | 1 | 1 |
| System.Web | Session state | High | 2 | 4 |
| System.Web | Global.asax events | Medium | 1 | 2 |
| WCF | Service contracts | High | 1 | 2 |
| WCF | ASP.NET compatibility mode | High | 2 | 3 |
| WCF | system.serviceModel config | High | 1 | 1 |
| Web Forms | Page / code-behind | High | 2 | 2 |
| Web Forms | Server controls | High | 1 | 1 |
| Web Forms | ViewState / postback | Medium | 2 | 2 |
| Web Forms | AjaxControlToolkit | High | 1 | 1 |

## Files by signal

- `MeridianPortal.Core/Pricing/PricingEngine.cs`: Culture-sensitive casing ×1
- `MeridianPortal.Core/Pricing/PricingEngine.cs`: Culture-sensitive formatting ×1
- `MeridianPortal.Core/Pricing/PricingEngine.cs`: Midpoint rounding (default ToEven) ×2
- `MeridianPortal.Core/Pricing/QuoteNumberGenerator.cs`: DateTime.Now (local time) ×1
- `MeridianPortal.Data/MeridianContext.cs`: EF6 namespaces ×3
- `MeridianPortal.Data/MeridianContext.cs`: DbModelBuilder / EntityTypeConfiguration ×3
- `MeridianPortal.Services/IPricingService.cs`: Service contracts ×2
- `MeridianPortal.Services/PricingService.svc.cs`: HttpContext.Current ×1
- `MeridianPortal.Services/PricingService.svc.cs`: Server.MapPath ×1
- `MeridianPortal.Services/PricingService.svc.cs`: ASP.NET compatibility mode ×2
- `MeridianPortal.Services/PricingService.svc.cs`: DateTime.Now (local time) ×1
- `MeridianPortal.Services/Web.config`: system.serviceModel config ×1
- `MeridianPortal.Services/Web.config`: ASP.NET compatibility mode ×1
- `MeridianPortal.Web/Account/Login.aspx.cs`: Session state ×1
- `MeridianPortal.Web/Account/Login.aspx.cs`: Page / code-behind ×1
- `MeridianPortal.Web/Account/Login.aspx.cs`: Forms auth / Membership ×2
- `MeridianPortal.Web/Global.asax.cs`: Global.asax events ×2
- `MeridianPortal.Web/Global.asax.cs`: log4net / Trace ×2
- `MeridianPortal.Web/Orders.aspx`: Server controls ×1
- `MeridianPortal.Web/Orders.aspx`: ViewState / postback ×1
- `MeridianPortal.Web/Orders.aspx`: Culture-sensitive formatting ×1
- `MeridianPortal.Web/Orders.aspx.cs`: Session state ×3
- `MeridianPortal.Web/Orders.aspx.cs`: Page / code-behind ×1
- `MeridianPortal.Web/Orders.aspx.cs`: ViewState / postback ×1
- `MeridianPortal.Web/Orders.aspx.cs`: Roles ×1
- `MeridianPortal.Web/Web.config`: Forms auth / Membership ×1
- `MeridianPortal.Web/Web.config`: Roles ×1
- `MeridianPortal.Web/Web.config`: machineKey ×1
- `MeridianPortal.Web/Web.config`: appSettings ×1
- `MeridianPortal.Web/Web.config`: log4net / Trace ×7
- `MeridianPortal.Web/packages.config`: AjaxControlToolkit ×1
- `MeridianPortal.Web/packages.config`: log4net / Trace ×1

## packages.config

- EntityFramework 6.2.0 (MeridianPortal.Web/packages.config)
- log4net 2.0.8 (MeridianPortal.Web/packages.config)
- Microsoft.AspNet.Web.Optimization 1.1.3 (MeridianPortal.Web/packages.config)
- Newtonsoft.Json 9.0.1 (MeridianPortal.Web/packages.config)
- AjaxControlToolkit 17.1.1 (MeridianPortal.Web/packages.config)
