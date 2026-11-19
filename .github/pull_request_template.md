## Task card
<!-- Link the issue / task card. One card per PR. -->

## What changed
<!-- Plain language. Which legacy file(s) does this replace or touch? -->

## Behaviour
- [ ] No observable behaviour change, OR the change is listed below with the approver's name
- [ ] Characterization / equivalence tests that prove it: <!-- names -->
- [ ] No golden or `*.approved.*` file changed (if one did, explain and tag the owner)

## Authoring
- [ ] AI-assisted (tool: ______ ) / [ ] written by hand
- [ ] I have read every changed line, not only the summary
- [ ] New APIs used, with documentation links:

## Risk
- [ ] Touches auth, antiforgery, cookies, or authorization (needs security reviewer)
- [ ] Touches data mapping or SQL (needs data reviewer)
- [ ] Changes gateway routes (update `tests/Meridian.Gateway.Tests` in this PR)
- Diff size (excluding generated files): ____ lines
