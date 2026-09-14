#!/usr/bin/env bash
# coord-train7-land.sh -- land train 7: assert the chain finished and the tree is clean of sweep dirt and pipeline
# record files, push the worktree head to master (must be a fast-forward of origin/master), verify, prune the merged
# branches. Run from the worktree root ONLY after the battery legs and the nistec pair have been read and ruled green.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
stamp() { echo "[$(date '+%F %T')] $*"; }
head=$(git rev-parse --short HEAD)
stamp "LAND TRAIN 7 head=$head"
# no converter / runner / host alive (a leg still running means the chain is not done)
alive=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.Name -match '^(go2cs|BehavioralRunner|testhost|vstest\.console)\.exe$' } | Measure-Object).Count")
[ "${alive//[[:space:]]/}" = "0" ] || { stamp "processes alive: $alive -- chain not done, ABORT"; exit 1; }
# pipeline record files that restore/clean never touch
recs=$(find src/core -maxdepth 4 \( -name go2cs_test_manifest.json -o -name go2cs_test_comparison.json -o -name go2cs_test_results.json -o -name go2cs_test_results.xml -o -name go2cs_test_comparison -type d \) 2>/dev/null)
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
# prune the branches this train merged (NOT reflect-tail-r-lite, R's live lane branch)
for b in c2-structof-gcbits c2-syscall-linux-nil-guard c1-gated-stamp c2-tz-pin i9-runtime-regen g-nethttp-ladder-text c1-syscall-platform-skip g-perf-tls-handshake c2-structof-gcbits-item3 c2-backlog-orphaned-comments; do
  if git ls-remote --exit-code --heads origin "claude/$b" >/dev/null 2>&1; then
    git fetch -q origin "claude/$b" 2>/dev/null
    if git merge-base --is-ancestor "origin/claude/$b" HEAD 2>/dev/null || [ "$b" = "c2-structof-gcbits-item3" ]; then git push -q origin --delete "claude/$b" && stamp "pruned claude/$b" || stamp "prune FAILED claude/$b"; else stamp "NOT pruned claude/$b (tip not in HEAD)"; fi
  else stamp "absent already: claude/$b"; fi
done
stamp "LAND TRAIN 7 DONE"
