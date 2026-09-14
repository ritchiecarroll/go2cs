#!/usr/bin/env bash
# coord-train20-b1-fixup.sh -- after the train-20 chain ends: merge R's B.1 (golib fix for the ISlice-as-array-dim regression) ON TOP of the
# train-20 head a283331d4, then re-run the legs B/B.1 can reach. Run from the musing-moser worktree, SOLO, after `TRAIN 20 CHAIN DONE`.
#   B1_SHA  = R's announced fresh SHA on claude/reflect-cargo-inc-b (REQUIRED)
# Re-run set (golib moved, no converter/emission change): reflect -tests build; GolibTests + each-class-alone; sweeps net/http, gob,
# encoding/json, encoding/xml, crypto/x509, go/types, crypto/tls, reflect canaries; nistec COST PAIR vs the control at 93a131a3f; reflect RUN.
# CNR is NOT re-owed (no emission change); go2cs.slnx only if R's post says a public signature moved (set SLNX=1).
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
B1_SHA="${B1_SHA:-ab7ce0534}"; [ -n "$B1_SHA" ] || { echo "B1_SHA unset -- nothing to do"; exit 1; }
ts=$(date +%Y%m%d-%H%M%S); log="$SP/coord-train20-b1-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "B1 FIXUP START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
[ "$(git rev-parse --short HEAD)" = "a283331d4" ] || { stamp "head is not the train-20 assembled head a283331d4 -- ABORT"; exit 1; }
git fetch -q origin claude/reflect-cargo-inc-b || { stamp "fetch failed -- ABORT"; exit 1; }
[ "$(git rev-parse origin/claude/reflect-cargo-inc-b)" = "$(git rev-parse $B1_SHA 2>/dev/null)" ] || stamp "NOTE: branch tip is not $B1_SHA (announced) -- merging $B1_SHA as announced, not the tip"
git merge --no-ff -S -q -F "$SP/coord-merge-r-cargo-b1.txt" "$B1_SHA" || { stamp "B.1 merge CONFLICTS -- ABORT"; git merge --abort 2>/dev/null; exit 1; }
stamp "merged B.1 -> $(git rev-parse --short HEAD); converter unchanged (golib only): $(git diff --name-only a283331d4 HEAD -- 'src/go2cs/*.go' | wc -l) converter files"
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD) (no source change expected)"
if [ "${SLNX:-0}" = "1" ]; then stamp "LEG: slnx (public signature moved)"; powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train20-b1-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train20-b1-slnx-$ts.log"; fi
stamp "LEG: filtered behavioral run (CanonicalTypeIdentity -- the guard whose golden B.1 re-baselined; 4 phases)"
(cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -File ./run-behavioral.ps1 --filter CanonicalTypeIdentity; echo "runner CanonicalTypeIdentity exit=$?") > "$SP/coord-train20-b1-runner-$ts.log" 2>&1; git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null
stamp "LEG: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train20-b1-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train20-b1-reflect-$ts.log"
stamp "LEG: GolibTests (build) + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" > "$SP/coord-train20-b1-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train20-b1-golibtests-$ts.log"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train20-b1-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train20-b1-alone-$ts.log"
stamp "LEG: sweeps (net/http first, then the reflect canaries + gob)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps net/http,encoding/gob,encoding/json,encoding/xml,crypto/x509,go/types,crypto/tls -SweepTimeout 60m > "$SP/coord-train20-b1-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train20-b1-sweeps-$ts.log"
stamp "LEG: nistec COST PAIR vs control (control worktree at 93a131a3f)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "93a131a3f" -SkipWarmup -Rounds 2 > "$SP/coord-train20-b1-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train20-b1-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train20-b1-pair-$ts.log" | cut -c23-120 | tr '\n' ';')"
stamp "LEG: reflect RUN (moved-set vs train 19's preserved record)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label coord-train20-b1 > "$SP/coord-train20-b1-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train20-b1-reflectrun-$ts.log"
stamp "B1 FIXUP CHAIN DONE head=$(git rev-parse --short HEAD)"
