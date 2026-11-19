#!/usr/bin/env bash
# Fails if golden/approved files changed without the "golden-approved" marker
# in the latest commit message. Re-recording behaviour is a human decision.
set -euo pipefail
base="${1:-origin/main}"
git fetch --quiet origin "${base#origin/}" || true
changed=$(git diff --name-only "$base"...HEAD -- 'tests/**/golden/**' '*.approved.*' || true)
if [[ -n "$changed" ]]; then
  if git log -1 --pretty=%B | grep -q '\[golden-approved\]'; then
    echo "Golden files changed with explicit approval marker:"; echo "$changed"; exit 0
  fi
  echo "::error::Golden/approved files changed without [golden-approved] in the commit message:"
  echo "$changed"; exit 1
fi
echo "Golden files unchanged."
