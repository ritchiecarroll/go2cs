#!/usr/bin/env bash
# coord-train9-bisect.sh <sha> [label] -- contingency for a RED train-9 cost pair: pair the train-8 head (3c745e0d9,
# the warm control worktree) against a scratch worktree checked out at <sha> -- a linear PREFIX of train 9's merges
# (8dea1cc03 = item 4 only; 7ddf848fd = item 4 + R's VALID arm; 263a12685 = all three) -- so each seat's cost is
# isolated by subtraction. The scratch tree is cold, so its first row is a warm-up: run Rounds=3 and DISCARD the
# first main reading by hand when reading the log. Lands nothing. Run from the coordinator worktree root, SOLO.
set -uo pipefail
sha="${1:?sha (a train-9 merge prefix)}"; label="${2:-bisect}"
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
WT="C:/Projects/go2cs/.claude/worktrees/coord-bisect-t9"
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train9-bisect-$label-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "BISECT START sha=$sha label=$label"
alive=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.Name -match '^(go2cs|BehavioralRunner|testhost|vstest\.console)\.exe$' } | Measure-Object).Count")
[ "${alive//[[:space:]]/}" = "0" ] || { stamp "processes alive: $alive -- ABORT"; exit 1; }
git worktree remove --force "$WT" 2>/dev/null; git worktree prune
git worktree add --detach "$WT" "$sha" >/dev/null 2>&1 || { stamp "worktree add failed -- ABORT"; exit 1; }
stamp "scratch worktree at $(git -C "$WT" rev-parse --short HEAD)"
(cd "$WT/src/go2cs" && go build -o bin/go2cs.exe . ) || { stamp "converter build FAILED -- ABORT"; exit 1; }
[ -f "$WT/src/go2cs/bin/go2cs.exe" ] || { stamp "converter not at the expected path -- ABORT"; exit 1; }
stamp "scratch converter: $(stat -c %s "$WT/src/go2cs/bin/go2cs.exe") B"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(cygpath -w "$WT")" -ControlSha 3c745e0d9 -SkipWarmup -Rounds 3 > "$SP/coord-train9-bisect-pair-$label-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train9-bisect-pair-$label-$ts.log"
stamp "PAIR: $(grep -E 'round|mean' "$SP/coord-train9-bisect-pair-$label-$ts.log" | cut -c23-120 | tr '\n' ';')"
stamp "BISECT DONE (discard the first main round: the scratch tree was cold)"
