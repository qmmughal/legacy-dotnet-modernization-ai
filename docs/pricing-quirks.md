# Pricing quirks preserved on purpose

Each quirk is legacy behaviour that the modern code keeps because changing it
would change prices customers see. Each has a characterization test. Changing
any of them needs a decision record and Sales Operations sign-off.

| ID | Behaviour | Where | Test that pins it |
|---|---|---|---|
| QUIRK-1 | `Math.Round(x, 2)` uses banker's rounding (`MidpointRounding.ToEven`), per line and for discount/freight/tax | `PricingRules.Calculate` | `Corpus_Detects_SwitchToAwayFromZeroRounding` |
| QUIRK-2 | Weight is accumulated as `double`; freight converts `(decimal)(weight * 0.35)` | `PricingRules.Calculate` | golden master |
| QUIRK-3 | Free freight only when net is strictly greater than 500.00 | `PricingRules.Calculate` | `Corpus_Detects_InclusiveFreeFreightThreshold`, case `edge-net-exactly-500` |
| QUIRK-4 | Tier codes are case-sensitive: `gold` gets no discount | `TierRate` | case `edge-gold-lowercase` |
| QUIRK-5 | Unknown/missing region is taxed at the NORTH rate (8%) | `TaxRate` | cases `edge-region-null`, `edge-region-lowercase` |
| QUIRK-6 | Display total is formatted with en-US currency format regardless of server culture | `DisplayCulture` | `ModernEngine_IgnoresThreadCulture` |
| QUIRK-7 | Non-positive quantity lines are skipped with a warning, not rejected | `PricingRules.Calculate` | case `edge-negative-qty` |
