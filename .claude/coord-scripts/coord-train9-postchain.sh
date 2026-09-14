#!/usr/bin/env bash
# coord-train9-postchain.sh -- after train 9's battery: the nistec COST pair with the control at the train-8 head
# (3c745e0d9 = master) versus the train-9 head. Item 4 adds an arm to PointeeArrayDims (consulted on every pointer
# descriptor synthesis) and R's VALID arm adds a struct-kind check to TryMarshalAssignable (every marshal), so the
# descriptor-synthesis cost rule applies; the battery's in-battery reading (354 s after three build legs) is not a
# cost reading. Control worktree recreated at the control SHA; warm-up kept (cold closure). Lands NOTHING.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
CTRL_SHA="${CTRL_SHA:-3c745e0d9}"
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train9-postchain-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "POSTCHAIN START head=$(git rev-parse --short HEAD) dirty=$(git status --porcelain | wc -l) control=$CTRL_SHA"
alive=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.Name -match '^(go2cs|BehavioralRunner|testhost|vstest\.console)\.exe$' } | Measure-Object).Count")
[ "${alive//[[:space:]]/}" = "0" ] || { stamp "processes alive: $alive -- battery not done, ABORT"; exit 1; }
git worktree remove --force C:/Projects/go2cs/.claude/worktrees/coord-nistec-ctrl 2>/dev/null; git worktree prune
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -ControlSha "$CTRL_SHA" > "$SP/coord-train9-nistec-pair-$ts.log" 2>&1; echo "nistec-pair exit=$?" >> "$SP/coord-train9-nistec-pair-$ts.log"
stamp "PAIR END: $(grep -E 'mean control' "$SP/coord-train9-nistec-pair-$ts.log" | tail -1 | cut -c1-160)"
stamp "POSTCHAIN DONE dirty=$(git status --porcelain | wc -l)"
