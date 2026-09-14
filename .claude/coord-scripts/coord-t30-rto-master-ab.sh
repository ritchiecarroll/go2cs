#!/usr/bin/env bash
# coord-t30-rto-master-ab.sh -- THE decisive one-axis A/B for train 30's second root.
# The dial fails with WSAEFAULT from the SIO_TCP_INITIAL_RTO WSAIoctl (Go names that error "setsockopt"), a path that is
# DEAD at master because version() there silently answers zeros and the gate is false. The rtlGetVersion fix makes the path
# REACHABLE for the first time, so the question is not "did the union break it" but "has it ever worked":
#   ARM MASTER  = the train-29 landed master b91684991 + the same three fix files  -> if the dial FAILS here too, the RTO
#                 path is a PRE-EXISTING defect unmasked by the fix, and the union is innocent of it.
#   ARM UNION   = the assembly head 75758cf06 + the same three fix files           -> the reading we already have.
# Runs in the TRAIN worktree (must be clean and idle); returns it to the assembly branch at the end, verified.
set -u; W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; D=/c/Projects/go2cs/.claude/worktrees/coord-t25-dryrun
S=/c/Projects/go2cs/.claude/coord-scripts; ts=$(date +%H%M%S); BR=claude/musing-moser-d4552c
FIX="src/core/internal/syscall/windows/windows/zsyscall_windows.cs src/go2cs/manualTypeOperations.go src/core/internal/syscall/windows/windows/zsyscall_windows_version_impl.cs"
cd "$W" || exit 1
live=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*musing-moser*' -and \$_.Name -match 'go2cs|dotnet|BehavioralRunner' }).Count" 2>/dev/null | tr -d '\r ')
[ "${live:-0}" = "0" ] || { echo "train worktree busy ($live) -- not running"; exit 1; }
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "train worktree dirty -- not running"; exit 1; }
[ "$(git rev-parse --abbrev-ref HEAD)" = "$BR" ] || { echo "not on $BR"; exit 1; }
for f in $FIX; do [ -f "$D/$f" ] || { echo "fix file missing in the cut's worktree: $f"; exit 1; }; done
export GOROOT='$HOME\sdk\go1.23.12' DOTNET_ROOT='$HOME\dotnet10' MSBUILDDISABLENODEREUSE=1
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
go version | grep -q go1.23.12 || { echo "GO PIN FAILED"; exit 1; }
arm() { # label commitish
  echo "=== ARM $1 at $2 ($(date +%H:%M))"
  git checkout -q --detach "$2" || { echo "checkout failed"; return 1; }
  for f in $FIX; do cp "$D/$f" "$W/$f"; done
  local L="$S/coord-t30-rtoab-$1-$ts.log"
  (cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -File ./run-behavioral.ps1 --filter TcpLoopbackRoundTrip > "$L" 2>&1)
  echo "  rc=$? :: $(grep -aE '^(FAIL|PASS)' "$L" | tail -1 | tr -d '\r') :: $(grep -aE 'exit code mismatch|stdout mismatch' "$L" | head -1 | tr -d '\r' | sed 's/^ *//')"
  local cs; cs=$(ls -t "$W/src/tests/Behavioral/TcpLoopbackRoundTrip/bin/"*/net10.0/TcpLoopbackRoundTrip.exe 2>/dev/null | head -1)
  if [ -n "$cs" ]; then (cd "$(dirname "$cs")" && timeout 60 ./TcpLoopbackRoundTrip.exe > "$S/coord-t30-rtoab-$1-stdout-$ts.txt" 2> "$S/coord-t30-rtoab-$1-stderr-$ts.txt"); echo "  program rc=$? stdout: $(head -1 "$S/coord-t30-rtoab-$1-stdout-$ts.txt" 2>/dev/null | tr -d '\r')"; fi
  git checkout -q -- . ; git clean -qfd src/core src/go2cs 2>/dev/null
}
arm master b91684991
arm union 75758cf06
echo "=== RESTORE"; git checkout -q "$BR" && git checkout -q -- . && git clean -qfd src/core src/go2cs 2>/dev/null
echo "tree: branch $(git rev-parse --abbrev-ref HEAD) head $(git rev-parse --short=9 HEAD) dirty $(git status --porcelain | wc -l)"
