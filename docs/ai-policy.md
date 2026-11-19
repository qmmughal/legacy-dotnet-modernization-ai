# AI-assisted development policy — Meridian modernization

Owner: [AI tooling owner]. Approved by: [security], [legal]. Review: quarterly.

## Approved tools
| Tool | Plan tier | Data terms confirmed | Allowed modes |
|---|---|---|---|
| [tool] | [business/enterprise] | [date, reference] | [local / cloud agent / review] |

## What may be shared
- Repository code under `src/`, `tests/`, `legacy/`, `docs/`: yes.
- Production data, customer records, credentials, production config: never.
- [Excluded components, e.g. licensed third-party source]: never.

## What agents may do
- Without approval: read the repository; run build and tests; edit files on a branch.
- With approval: add or change packages; change CI, gateway routes, or auth code.
- Never: push to protected branches; access production systems; edit golden files.
(Mirrored in .claude/settings.json, Copilot and Cursor settings, branch protection.)

## Review
Every AI-assisted change is reviewed by a person who reads the diff and runs
the tests. AI review comments are advice, not approval.

## Records
Pull requests state the tool used and include the agent's plan and
uncertainties. Decisions about behaviour are recorded in docs/decisions.md.

## Incidents
Suspected data exposure or a harmful change: stop the tool, tell [security],
record what happened, and review this policy.
