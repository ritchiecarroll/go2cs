#!/bin/bash
# REHEARSAL ONLY -- merges every seat in a throwaway worktree to surface conflicts. Lands nothing.
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-t25-dryrun
S=/c/Projects/go2cs/.claude/coord-scripts
cd "$W" || exit 8
LIVE=$(powershell -NoProfile -Command "@(Get-CimInstance Win32_Process -Filter \"Name='go2cs.exe' OR Name='BehavioralRunner.exe'\").Count" 2>/dev/null | tr -d '\r')
[ "${LIVE:-0}" != "0" ] && { echo "REFUSING: $LIVE measurement process(es) live"; exit 9; }
git fetch -q origin master 2>/dev/null
ORIG=$(git rev-parse --abbrev-ref HEAD); [ "$ORIG" = HEAD ] && ORIG=$(git rev-parse HEAD)
git checkout -q --detach origin/master || exit 8
git clean -qfd 2>/dev/null
echo "=== REHEARSAL on master $(git log -1 --format=%h) -- lands nothing"
python "$S/coord-train31-seatlist.py" || exit 8
OK=0; CONF=0
while IFS='|' read -r key br sha msg; do
  git fetch -q origin "$br" </dev/null 2>/dev/null
  if git merge --no-ff --no-edit -q -F "$S/$msg" "$sha" </dev/null >/dev/null 2>&1; then
    OK=$((OK+1)); echo "  ok   $key"
  else
    CONF=$((CONF+1)); echo "  CONFLICT $key:"; git diff --name-only --diff-filter=U | sed 's/^/        /'
    git merge --abort </dev/null 2>/dev/null
  fi
done < $HOME/AppData/Local/Temp/dropseats31.txt
echo "=== REHEARSAL RESULT: $OK merged clean, $CONF conflicted"
echo "=== restoring"; git checkout -q "$ORIG" 2>/dev/null; git clean -qfd 2>/dev/null
echo "  back at $(git log -1 --format=%h), dirty=$(git status --porcelain | wc -l)"
