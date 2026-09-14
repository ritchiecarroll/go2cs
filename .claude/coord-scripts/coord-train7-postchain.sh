#!/usr/bin/env bash
# coord-train7-postchain.sh -- after train 7's battery chain has ended: (1) the SOLO net/http re-run with its record
# preserved (the battery's own run failed and discarded the evidence), (2) the nistec A B A B cost pair against a
# control worktree at 092329148. Lands NOTHING; the coordinator reads both logs and rules. Run from the worktree root.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train7-postchain-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "POSTCHAIN START head=$(git rev-parse --short HEAD) dirty=$(git status --porcelain | wc -l)"
alive=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.Name -match '^(go2cs|BehavioralRunner|testhost|vstest\.console)\.exe$' } | Measure-Object).Count")
[ "${alive//[[:space:]]/}" = "0" ] || { stamp "processes alive: $alive -- chain not done, ABORT"; exit 1; }
stamp "LEG A: net/http solo re-run, record kept"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-sweep-keep.ps1" -Package net/http -SweepTimeout 40m -Label train7 > "$SP/coord-train7-nethttp-rerun-$ts.log" 2>&1; echo "nethttp-rerun exit=$?" >> "$SP/coord-train7-nethttp-rerun-$ts.log"
stamp "LEG A END: $(grep -E 'sweep exit=' "$SP/coord-train7-nethttp-rerun-$ts.log" | tail -1 | cut -c1-160)"
stamp "LEG B: nistec pair (control 092329148 vs head), A B A B"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" > "$SP/coord-train7-nistec-pair-$ts.log" 2>&1; echo "nistec-pair exit=$?" >> "$SP/coord-train7-nistec-pair-$ts.log"
stamp "LEG B END: $(grep -E 'mean control' "$SP/coord-train7-nistec-pair-$ts.log" | tail -1 | cut -c1-160)"
stamp "POSTCHAIN DONE dirty=$(git status --porcelain | wc -l)"
