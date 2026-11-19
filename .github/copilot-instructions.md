# Copilot repository instructions

Follow `AGENTS.md` at the repository root; it is the source of truth for every
coding agent in this repo. Path-specific rules live in `.github/instructions/`.

In short: `legacy/` is read-only; golden/approved test files are human-owned;
behaviour must not change unless the issue says so; versions come only from
`Directory.Packages.props`; build and test must pass:

    dotnet build Meridian.slnx -warnaserror
    dotnet test --solution Meridian.slnx
