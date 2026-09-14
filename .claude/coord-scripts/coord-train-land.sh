#!/usr/bin/env bash
# coord-train-land.sh <train-label> <branch-to-prune>... -- land the coordinator worktree's HEAD onto master: assert the
# box is solo and the tree clean (pipeline record files deleted first), assert origin/master is an ancestor, scan every
# merge blob for conflict markers, push HEAD:master, verify, then prune the named lane branches (only those whose tip is
# in HEAD, unless the name is suffixed with '!' which forces the prune for a branch ruled redundant). Run from the worktree root.
set -uo pipefail
label="${1:?train label}"; shift
stamp() { echo "[$(date '+%F %T')] $*"; }
head=$(git rev-parse --short HEAD)
stamp "LAND $label head=$head"
alive=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.Name -match '^(go2cs|BehavioralRunner|testhost|vstest\.console)\.exe$' } | Measure-Object).Count")
[ "${alive//[[:space:]]/}" = "0" ] || { stamp "processes alive: $alive -- battery not done, ABORT"; exit 1; }
recs=$(find src/core -maxdepth 4 \( -name go2cs_test_manifest.json -o -name go2cs_test_comparison.json -o -name go2cs_test_results.json -o -name go2cs_test_results.xml -o \( -name go2cs_test_comparison -type d \) \) 2>/dev/null)
if [ -n "$recs" ]; then stamp "deleting pipeline record files:"; echo "$recs" | sed 's/^/  /'; echo "$recs" | while read -r r; do rm -rf "$r"; done; fi
dirty=$(git status --porcelain | wc -l)
[ "$dirty" = "0" ] || { stamp "tree dirty ($dirty entries) -- classify before landing, ABORT"; git status --porcelain | head -20; exit 1; }
git fetch -q origin master || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "origin/master ($(git rev-parse --short origin/master)) is not an ancestor of HEAD -- master moved, ABORT"; exit 1; }
stamp "origin/master=$(git rev-parse --short origin/master) -> HEAD=$head ($(git rev-list --count origin/master..HEAD) commits, $(git rev-list --count --merges origin/master..HEAD) merges)"
for m in $(git rev-list --merges origin/master..HEAD); do n=$(git grep -c '^<<<<<<<' $m -- . 2>/dev/null | wc -l); [ "$n" = "0" ] || { stamp "conflict markers in merge $m -- ABORT"; exit 1; }; done
stamp "no conflict markers in any merge blob"
git push -q origin HEAD:master || { stamp "PUSH FAILED"; exit 1; }
git fetch -q origin master
[ "$(git rev-parse origin/master)" = "$(git rev-parse HEAD)" ] && stamp "PUSH VERIFIED master=$(git rev-parse --short origin/master)" || { stamp "push not verified"; exit 1; }
for spec in "$@"; do
  b="${spec%!}"; force=0; [ "$spec" != "$b" ] && force=1
  if git ls-remote --exit-code --heads origin "claude/$b" >/dev/null 2>&1; then
    git fetch -q origin "claude/$b" 2>/dev/null
    if [ $force = 1 ] || git merge-base --is-ancestor "origin/claude/$b" HEAD 2>/dev/null; then git push -q origin --delete "claude/$b" && stamp "pruned claude/$b" || stamp "prune FAILED claude/$b"; else stamp "NOT pruned claude/$b (tip not in HEAD)"; fi
  else stamp "absent already: claude/$b"; fi
done
stamp "LAND $label DONE"
