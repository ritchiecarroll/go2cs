#!/usr/bin/env bash
# coord-train20-b1-golib.sh -- the fixup's GolibTests leg re-measured in the PROVEN shape (slnx build, then GolibTests --no-build + each-class-alone),
# because the standalone GolibTests build in the fixup chain died in a CS0234/CS0246 storm on golib attribute types (unmeasured, not red).
# Run from the musing-moser worktree at 22d2bd9dc after the fixup chain ends. SOLO.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
ts=$(date +%Y%m%d-%H%M%S); log="$SP/coord-train20-b1golib-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "B1 GOLIB RE-MEASURE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
[ "$(git rev-parse --short HEAD)" = "22d2bd9dc" ] || { stamp "head is not 22d2bd9dc -- ABORT"; exit 1; }
stamp "LEG: slnx (the behavioral COMPILE gate; builds GolibTests' closure in solution context)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train20-b1golib-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train20-b1golib-slnx-$ts.log"
stamp "slnx: $(tr -d '\000' < "$SP/coord-train20-b1golib-slnx-$ts.log" | grep -aoE 'SLNX BUILD END exit=[0-9]+ strictErrors=[0-9]+ elapsed=[0-9]+s' | head -1)"
stamp "LEG: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train20-b1golib-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train20-b1golib-golibtests-$ts.log"
stamp "golibtests: $(tr -d '\000' < "$SP/coord-train20-b1golib-golibtests-$ts.log" | grep -aoE 'count-matched: [0-9]+ / [0-9]+|COUNT MISMATCH[^\r]*|Test Run Aborted' | head -1)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train20-b1golib-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train20-b1golib-alone-$ts.log"
stamp "alone: $(tr -d '\000' < "$SP/coord-train20-b1golib-alone-$ts.log" | grep -acE 'alone: exit=0 aborted=False') classes exit 0 / $(tr -d '\000' < "$SP/coord-train20-b1golib-alone-$ts.log" | grep -acE 'alone: exit=') run"
stamp "B1 GOLIB RE-MEASURE DONE"
