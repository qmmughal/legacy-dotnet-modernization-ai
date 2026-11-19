# Claude Code instructions

@AGENTS.md

## Claude-specific
- Run `dotnet test --solution Meridian.slnx` before declaring a task complete and
  paste the summary lines into your final message.
- Permissions are enforced in `.claude/settings.json`; do not ask to bypass them.
