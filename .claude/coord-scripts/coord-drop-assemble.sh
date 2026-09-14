#!/bin/bash
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-drop
S=/c/Projects/go2cs/.claude/coord-scripts
cd "$W" || exit 8
git checkout -q --detach b91684991 || exit 8
git clean -qfd 2>/dev/null
# the seat list is written BY PYTHON with explicit LF -- no shell in the loop, no CR to strip
python "$S/coord-drop-seatlist.py" || exit 8
echo "=== seats to merge: $(wc -l < /tmp/dropseats.txt) (Q44 dropped)"
FAIL=0
while IFS='|' read -r key br sha msg; do
  git fetch -q origin "$br" </dev/null 2>/dev/null
  if ! git merge --no-ff --no-edit -q -F "$S/$msg" "$sha" </dev/null >/dev/null 2>&1; then
    echo "  MERGE FAILED at $key ($br) -- unmerged paths follow; NONE means it was not a conflict"
    git diff --name-only --diff-filter=U | sed 's/^/      /'
    git merge --abort </dev/null 2>/dev/null
    FAIL=1; break
  fi
  echo "  ok $key -> $(git log -1 --format=%h)"
done < /tmp/dropseats.txt
echo "=== assembly head: $(git log -1 --format=%h)  failures: $FAIL"
