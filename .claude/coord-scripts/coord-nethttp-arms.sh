#!/usr/bin/env bash
# coord-nethttp-arms.sh -- attribution arms for the net/http TestServerUndeclaredTrailers pass->fail at the train-20 head a283331d4.
# Runs in the CONTROL worktree (coord-nistec-ctrl), SOLO on the i7 (never while a battery's time row or the nistec pair runs).
#   arm 1 = master 93a131a3f + cherry-pick of the remedy 8a8e229a8f (C2's measurement shape)     -> net/http sweep, record preserved
#   arm 2 = arm 1 + merge of B fb51d8730                                                        -> net/http sweep, record preserved (only if arm 1 PASSES the family)
# The sweep is run WITHOUT -TestConfig so the row's own release-tiered annotation is honoured (the default respects annotations).
# Afterwards the control worktree is returned to origin/master with its exe removed (the nistec pair's precondition).
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
CTRL="/c/Projects/go2cs/.claude/worktrees/coord-nistec-ctrl"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
ts=$(date +%Y%m%d-%H%M%S); log="$SP/coord-nethttp-arms-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
cd "$CTRL" || exit 1
stamp "ARMS START ctrl=$(git rev-parse --short HEAD) dirty=$(git status --porcelain | wc -l) go=$(go version)"
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "control worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master claude/c2-alias-overlap-race claude/reflect-cargo-inc-b 2>/dev/null
git checkout -q --detach 93a131a3f || { stamp "checkout 93a131a3f failed -- ABORT"; exit 1; }
run_row() { # label
  local label="$1"
  (cd src/go2cs && go build -o bin/go2cs.exe . ) || { stamp "$label: converter build FAILED"; return 1; }
  stamp "$label: head=$(git rev-parse --short HEAD) converter $(stat -c %s src/go2cs/bin/go2cs.exe) B; sweep net/http (60m floor, row annotation honoured)"
  powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter net/http -Exact -TestTimeout 60m -SkipBuild > "$SP/coord-nethttp-arm-$label-$ts.log" 2>&1; rc=$?
  local rec="src/core/net/http/go2cs_test_comparison.json"; local keep="$SP/coord-pkg-run-record-net.http-arm-$label-$ts"; mkdir -p "$keep"
  [ -e "$rec" ] && cp -p "$rec" "$keep/" ; [ -e src/core/net/http/go2cs_test_results.json ] && cp -p src/core/net/http/go2cs_test_results.json "$keep/"
  stamp "$label: sweep exit=$rc; verdict: $(tr -d '\000' < "$SP/coord-nethttp-arm-$label-$ts.log" | grep -aE '^\s*(PASS|FAIL|COUNT)\s+net/http' | head -1 | tr -s ' ')"
  python - "$keep/go2cs_test_comparison.json" "$label" <<'PY' | tee -a "$log"
import json, io, sys, os
p, label = sys.argv[1], sys.argv[2]
if not os.path.exists(p): print("  %s: NO comparison record" % label); sys.exit(0)
d = json.load(io.open(p, encoding='utf-8-sig')); go = d.get('go') or {}; cs = d.get('csharp') or {}
fam = {n: (go.get(n), cs.get(n)) for n in sorted(set(go) | set(cs)) if 'UndeclaredTrailers' in n}
from collections import Counter
print("  %s: rows go=%d cs=%d cs-verdicts=%s errors=%d env=%s" % (label, len(go), len(cs), dict(Counter(cs.values())), len(d.get('errors') or []), d.get('environment') or d.get('testEnvironment')))
print("  %s: TestServerUndeclaredTrailers family (go, cs): %s" % (label, fam))
PY
  git checkout -q -- src docs 2>/dev/null; git clean -fdq -- src/core docs/validation 2>/dev/null
  stamp "$label: post-run restore dirty=$(git status --porcelain | wc -l)"
}
# ---- arm 1: master + remedy only ----
git -c commit.gpgsign=false cherry-pick 8a8e229a8f >/dev/null 2>&1 || { stamp "cherry-pick of the remedy CONFLICTED -- ABORT"; git cherry-pick --abort 2>/dev/null; exit 1; }
stamp "arm1 tree: $(git rev-parse --short HEAD) = 93a131a3f + remedy 8a8e229a8f (cherry-pick)"
run_row arm1-remedy-only
# ---- arm 2: + B (only meaningful if arm 1 passed the family; run regardless unless ARM2=0) ----
if [ "${ARM2:-1}" = "1" ]; then
  git -c commit.gpgsign=false merge --no-ff -q -m "arm2: + B fb51d8730" fb51d8730 >/dev/null 2>&1 || { stamp "merge of B CONFLICTED -- arm 2 skipped"; git merge --abort 2>/dev/null; }
  stamp "arm2 tree: $(git rev-parse --short HEAD) = arm1 + B fb51d8730"
  run_row arm2-remedy-plus-B
fi
# ---- return the control worktree to master, exe removed ----
git checkout -q --detach origin/master; rm -f src/go2cs/bin/go2cs.exe
stamp "ARMS DONE control at $(git rev-parse --short HEAD), exe removed, dirty=$(git status --porcelain | wc -l)"
