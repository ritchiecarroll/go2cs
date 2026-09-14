#!/usr/bin/env bash
# coord-t30-dial-ab.sh -- ONE-AXIS A/B on the union's SECOND root (every Windows dial fails once the rtlGetVersion fault is
# removed). Arm 1: the assembly head + the rtlGetVersion fix (the second root should REPRODUCE: no fault, stdout mismatch,
# "dial failed" lines). Arm 2: the same tree with ONLY golib's pointer-token files reverted to the train-29 landed master
# (b91684991) -- if the dials come back, the token cut is the root; if they do not, it is not, and the next suspect is the
# park hook. Runs in the TRAIN worktree, which must be clean and idle; restores it to the bare assembly head at the end.
set -u; W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; D=/c/Projects/go2cs/.claude/worktrees/coord-t25-dryrun
S=/c/Projects/go2cs/.claude/coord-scripts; ts=$(date +%H%M%S)
cd "$W" || exit 1
live=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*musing-moser*' -and \$_.Name -match 'go2cs|dotnet|BehavioralRunner' }).Count" 2>/dev/null | tr -d '\r ')
[ "${live:-0}" = "0" ] || { echo "train worktree busy ($live) -- not running the A/B"; exit 1; }
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "train worktree dirty -- not running the A/B"; exit 1; }
[ "$(git rev-parse --short=9 HEAD)" = "75758cf06" ] || { echo "head is $(git rev-parse --short=9 HEAD), expected the assembly head"; exit 1; }
export GOROOT='$HOME\sdk\go1.23.12' DOTNET_ROOT='$HOME\dotnet10' MSBUILDDISABLENODEREUSE=1
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
go version | grep -q go1.23.12 || { echo "GO PIN FAILED"; exit 1; }
run_guard() { # label
  local L="$S/coord-t30-dialab-$1-$ts.log"
  (cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -File ./run-behavioral.ps1 --filter TcpLoopbackRoundTrip > "$L" 2>&1)
  local rc=$?
  local verdict; verdict=$(grep -aE 'FAIL|PASS' "$L" | tail -1 | tr -d '\r')
  local detail; detail=$(grep -aE 'exit code mismatch|stdout mismatch' "$L" | head -1 | tr -d '\r' | sed 's/^ *//')
  echo "ARM $1: rc=$rc :: ${verdict:-no verdict line} :: ${detail:-no failure detail}"
}
echo "=== ARM 1: assembly head + the rtlGetVersion fix (copied from the cut's worktree, read-only)"
for f in src/core/internal/syscall/windows/windows/zsyscall_windows.cs src/go2cs/manualTypeOperations.go src/core/internal/syscall/windows/windows/zsyscall_windows_version_impl.cs; do
  [ -f "$D/$f" ] && cp "$D/$f" "$W/$f" && echo "  applied $(basename $f)"
done
run_guard fix
echo "=== ARM 2: the same tree, golib's pointer-token files reverted to the landed master b91684991"
for f in $(git diff --name-only b91684991..HEAD -- src/core/golib | grep -iE 'PointerTokens|/[^/]*\xd0\xb6[^/]*\.cs$'); do
  git checkout b91684991 -- "$f" 2>/dev/null && echo "  reverted $f"
done
git status --porcelain -- src/core/golib | cut -c1-90
run_guard notoken
echo "=== RESTORE"
git checkout -q HEAD -- . && git clean -qfd src/core src/go2cs 2>/dev/null
echo "tree: head $(git rev-parse --short=9 HEAD) dirty $(git status --porcelain | wc -l)"
