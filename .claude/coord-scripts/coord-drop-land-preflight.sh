#!/bin/bash
# LANDING PREFLIGHT for train 30 as the fifteen-seat drop. Verifies, never pushes.
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-drop
T=$HOME/AppData/Local/Temp/claude
TREE=3737ed9a6
FAIL=0
cd "$W" || exit 8
# ⚠ this script CHECKS OUT a tree in $W. If a sweep or converter is live there, a checkout
# would yank the tree out from under it -- refuse rather than disrupt a running measurement.
LIVE=$(powershell -NoProfile -Command "@(Get-CimInstance Win32_Process -Filter \"Name='go2cs.exe' OR Name='BehavioralRunner.exe'\").Count" 2>/dev/null | tr -d "")
if [ "${LIVE:-0}" != "0" ]; then echo "  REFUSING: $LIVE converter/runner process(es) live -- a checkout here would disrupt a running measurement"; exit 9; fi
say() { printf '  %-46s %s\n' "$1" "$2"; }
chk() { if [ "$2" = "$3" ]; then say "$1" "OK ($2)"; else say "$1" "FAIL (got $2, want $3)"; FAIL=1; fi; }
echo "=== TRAIN 30 LANDING PREFLIGHT (drop, $TREE)"
git checkout -q --detach $TREE 2>/dev/null
chk "tree is the intended assembly" "$(git rev-parse --short=9 HEAD)" "$TREE"
chk "worktree clean" "$(git status --porcelain | wc -l)" "0"
if git merge-base --is-ancestor eed11b550 HEAD 2>/dev/null; then say "token seat ABSENT" "FAIL -- it is present"; FAIL=1; else say "token seat ABSENT" "OK"; fi
if git merge-base --is-ancestor 687ac9e8d HEAD 2>/dev/null; then say "version hand-own branch absent" "FAIL -- present (drags the seat)"; FAIL=1; else say "version hand-own branch absent" "OK"; fi
chk "master is an ancestor (fast-forwardable)" "$(git merge-base --is-ancestor origin/master HEAD 2>/dev/null && echo yes || echo no)" "yes"
say "first-parent commits above master" "$(git log --first-parent --format=%h origin/master..HEAD | wc -l)"
say "conflict markers in tree" "$(git grep -l '^<<<<<<<' HEAD -- 2>/dev/null | wc -l)"
echo "=== GATE LINES (read from the preserved logs, not from memory)"
g() { if grep -qE "$2" "$T/$1" 2>/dev/null; then say "$3" "GREEN"; else say "$3" "NOT GREEN / not run"; FAIL=1; fi; }
g drop-leg-integrity.log 'SOLUTION INTEGRITY OK' "solution integrity"
g drop-leg-cnr.log 'NO REGRESSION' "CNR byte-identical"
g coord-drop-slnx-final.log 'errors \(strict pattern\): 0' "go2cs.slnx zero errors (at the FINAL tree)"
g drop-behavioral.log 'PASS|passed|0 failed' "behavioral suite"
g drop-sweep.log '0 fail' "validated sweep 203/0"
echo "=== VERDICT: $([ $FAIL -eq 0 ] && echo READY TO LAND || echo NOT READY)"
exit $FAIL
