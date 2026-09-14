#!/usr/bin/env bash
# coord-t30-setsockopt-bisect.sh -- attribute the union's SECOND root (WSAEFAULT from setsockopt on every Windows dial).
# The A/B has already established the two endpoints with the SAME three fix files applied: the landed master DIALS, the
# assembly head does not. So the defect is one of the sixteen seat merges. Each probe: detach at the merge, copy in the
# two CORPUS files of the rtlGetVersion fix (the registry file is deliberately NOT copied -- the behavioral runner
# transpiles only src/tests, so the corpus files are what the guard compiles against, and leaving the converter alone
# keeps other seats' registry rows out of the arm), build and run the guard, classify by the KIND of failure.
# Q44's merge is probed FIRST because it is the prime suspect and a single arm can settle it; the rest binary-search.
set -u; W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; D=/c/Projects/go2cs/.claude/worktrees/coord-t25-dryrun
S=/c/Projects/go2cs/.claude/coord-scripts; ts=$(date +%H%M%S); BR=claude/musing-moser-d4552c
FIX="src/core/internal/syscall/windows/windows/zsyscall_windows.cs src/core/internal/syscall/windows/windows/zsyscall_windows_version_impl.cs"
cd "$W" || exit 1
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "train worktree dirty -- not bisecting"; exit 1; }
export GOROOT='$HOME\sdk\go1.23.12' DOTNET_ROOT='$HOME\dotnet10' MSBUILDDISABLENODEREUSE=1
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
probe() { # commitish label
  git checkout -q --detach "$1" 2>/dev/null || { echo "  $2 $1: CHECKOUT FAILED"; return 2; }
  for f in $FIX; do cp "$D/$f" "$W/$f"; done
  local L="$S/coord-t30-ssbisect-$2-$ts.log"
  (cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -File ./run-behavioral.ps1 --filter TcpLoopbackRoundTrip > "$L" 2>&1)
  local v; v=$(grep -aE '^(FAIL|PASS)' "$L" | tail -1 | tr -d '\r')
  local d; d=$(grep -aE 'exit code mismatch|stdout mismatch' "$L" | head -1 | tr -d '\r' | sed 's/^ *//')
  local cls="UNKNOWN"
  case "$v$d" in *"PASS"*) cls="DIALS" ;; *"stdout mismatch"*) cls="SETSOCKOPT-BROKEN" ;; *"exit code mismatch"*) cls="CRASH-OR-OTHER" ;; esac
  echo "  $2 $(git rev-parse --short=9 HEAD): $cls :: ${v:-no verdict} :: ${d:-}"
  git checkout -q -- . ; git clean -qfd src/core 2>/dev/null
  [ "$cls" = "DIALS" ] && return 0 || return 1
}
echo "=== SETSOCKOPT BISECT ($(date +%H:%M)); endpoints already measured: master DIALS, assembly head does not"
probe 0aed0a459 q44 ; q44=$?
if [ "$q44" != "0" ]; then
  echo "=> the FIRST seat merge (the pointer-token cut) already shows it: one arm settles the attribution"
else
  echo "=> the token cut is EXONERATED by its own merge point; binary-searching the remaining fifteen"
  for c in 3a85cd16b 899984e1c 01d1bd326; do probe $c mid-$c || { echo "=> first failing probe: $c"; break; }; done
fi
echo "=== RESTORE"; git checkout -q "$BR" && git checkout -q -- . && git clean -qfd src/core 2>/dev/null
echo "tree: branch $(git rev-parse --abbrev-ref HEAD) head $(git rev-parse --short=9 HEAD) dirty $(git status --porcelain | wc -l)"
