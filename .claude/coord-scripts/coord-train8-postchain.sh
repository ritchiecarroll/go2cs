#!/usr/bin/env bash
# coord-train8-postchain.sh -- after train 8's battery: (1) the filtered behavioral runner for SendtoSeam (the Windows
# fix's own gate: transpile/compile/target/output), (2) the nistec COST pair with the control at the TRAIN-8-MINUS-TRIO
# commit (ba40264c4, the Sendto merge) versus the train-8 head -- the battery's single 372 s reading against the paired
# 272-278 s baseline is a suspect, not a verdict, and the trio's mint-time method emission is the prime candidate.
# Lands NOTHING. Run from the worktree root.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
CTRL_SHA="${CTRL_SHA:-ba40264c4}"
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train8-postchain-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "POSTCHAIN START head=$(git rev-parse --short HEAD) dirty=$(git status --porcelain | wc -l) control=$CTRL_SHA"
alive=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.Name -match '^(go2cs|BehavioralRunner|testhost|vstest\.console)\.exe$' } | Measure-Object).Count")
[ "${alive//[[:space:]]/}" = "0" ] || { stamp "processes alive: $alive -- battery not done, ABORT"; exit 1; }
stamp "LEG A: filtered behavioral runner -- SendtoSeam"
(cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -File ./run-behavioral.ps1 --filter SendtoSeam) > "$SP/coord-train8-sendto-runner-$ts.log" 2>&1; echo "runner exit=$?" >> "$SP/coord-train8-sendto-runner-$ts.log"
stamp "LEG A END: $(grep -E 'PASS|FAIL|Transpile|Compile|Target|Output|runner exit' "$SP/coord-train8-sendto-runner-$ts.log" | tail -3 | tr '\n' ' ' | cut -c1-200)"
stamp "LEG A restore: $(git checkout HEAD -- src/tests/Behavioral/SendtoSeam 2>&1 | tail -1) dirty=$(git status --porcelain | wc -l)"
stamp "LEG B: nistec pair, control $CTRL_SHA (train 8 minus the trio) vs head -- control worktree recreated, warm-up kept"
git worktree remove --force C:/Projects/go2cs/.claude/worktrees/coord-nistec-ctrl 2>/dev/null; git worktree prune
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -ControlSha "$CTRL_SHA" > "$SP/coord-train8-nistec-pair-$ts.log" 2>&1; echo "nistec-pair exit=$?" >> "$SP/coord-train8-nistec-pair-$ts.log"
stamp "LEG B END: $(grep -E 'mean control' "$SP/coord-train8-nistec-pair-$ts.log" | tail -1 | cut -c1-160)"
stamp "POSTCHAIN DONE dirty=$(git status --porcelain | wc -l)"
