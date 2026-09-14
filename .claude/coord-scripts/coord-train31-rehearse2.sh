#!/bin/bash
# REHEARSAL 2 -- resolves each conflict ARBITRARILY (union-of-sides where possible) purely to CONTINUE,
# so that conflicts created FOR LATER SEATS become visible. The resolutions here are NOT the landing
# resolutions and the tree it produces is NOT landable. Lands nothing; discards everything.
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-t25-dryrun
S=/c/Projects/go2cs/.claude/coord-scripts
cd "$W" || exit 8
LIVE=$(powershell -NoProfile -Command "@(Get-CimInstance Win32_Process -Filter \"Name='go2cs.exe' OR Name='BehavioralRunner.exe'\").Count" 2>/dev/null | tr -d '\r')
[ "${LIVE:-0}" != "0" ] && { echo "REFUSING: $LIVE measurement process(es) live"; exit 9; }
git fetch -q origin master 2>/dev/null
ORIG=$(git rev-parse --abbrev-ref HEAD); [ "$ORIG" = HEAD ] && ORIG=$(git rev-parse HEAD)
git checkout -q --detach origin/master || exit 8; git clean -qfd 2>/dev/null
echo "=== REHEARSAL 2 on master $(git log -1 --format=%h) -- RESOLVES to continue; tree NOT landable"
python "$S/coord-train31-seatlist.py" || exit 8
OK=0; RES=0
while IFS='|' read -r key br sha msg; do
  git fetch -q origin "$br" </dev/null 2>/dev/null
  if git merge --no-ff --no-edit -q -F "$S/$msg" "$sha" </dev/null >/dev/null 2>&1; then
    OK=$((OK+1)); echo "  ok        $key"
  else
    files=$(git diff --name-only --diff-filter=U)
    echo "  RESOLVED  $key  [$(echo "$files" | tr '\n' ' ')]"
    for f in $files; do git checkout --theirs -- "$f" 2>/dev/null || true; git add "$f" 2>/dev/null; done
    git -c core.editor=true commit -q --no-edit 2>/dev/null && RES=$((RES+1)) || { echo "        commit FAILED"; git merge --abort </dev/null 2>/dev/null; }
  fi
done < $HOME/AppData/Local/Temp/dropseats31.txt
echo "=== REHEARSAL 2: $OK clean, $RES resolved-to-continue, head $(git log -1 --format=%h)"
echo "=== conflict markers anywhere in the result: $(git grep -l '^<<<<<<<' HEAD -- 2>/dev/null | wc -l)"
echo "=== restoring"; git checkout -q "$ORIG" 2>/dev/null; git clean -qfd 2>/dev/null
echo "  back at $(git log -1 --format=%h), dirty=$(git status --porcelain | wc -l)"
