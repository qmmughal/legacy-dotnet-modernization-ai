---
name: preserving-legacy-behavior
description: >
  Use when porting any method from legacy/ into src/, or when asked to "port",
  "migrate", "convert", or "modernize" pricing, quoting, order, or customer
  logic. Ensures characterization tests exist and pass before and after.
---

# Preserving legacy behaviour while porting

## Workflow
1. Find the legacy method and list every input, output, side effect, and
   default it relies on (culture, rounding, time zone, null handling, casing).
2. Check `tests/Meridian.Characterization.Tests` for coverage of that method.
   If none exists, STOP and propose characterization tests first (separate PR).
3. Port the method into `src/` with explicit choices for every default you listed.
   Mark intentional oddities with `// QUIRK-n:` and add them to `docs/pricing-quirks.md`.
4. Add or extend an equivalence test that runs legacy and modern code on the
   same corpus.
5. Run `dotnet test --solution Meridian.slnx`.

## Success criteria
- [ ] Characterization tests existed before the port (link the PR).
- [ ] Equivalence test passes for every corpus case.
- [ ] No golden/approved file changed.
- [ ] Every implicit default is now explicit in code.

## If something fails
- A golden mismatch means behaviour changed: report the first 10 differing
  cases and stop. Do not "fix" the expected values.
