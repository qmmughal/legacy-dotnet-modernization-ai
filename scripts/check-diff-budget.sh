#!/usr/bin/env bash
# Warn-then-fail guard on pull request size, excluding generated and golden files.
set -euo pipefail
base="${1:-origin/main}"
budget="${2:-400}"
git fetch --quiet origin "${base#origin/}" || true
lines=$(git diff --numstat "$base"...HEAD -- . \
  ':(exclude)tests/**/golden/**' ':(exclude)**/*.approved.*' ':(exclude)docs/inventory.md' \
  ':(exclude).github/upgrades/**' \
  | awk '{ add += $1; del += $2 } END { print add + del + 0 }')
echo "Changed lines (excluding generated/golden): $lines (budget $budget)"
if (( lines > budget )); then
  echo "::error::Diff is over budget. Split the task card or get an explicit exception."
  exit 1
fi
