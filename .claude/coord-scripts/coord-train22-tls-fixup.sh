#!/usr/bin/env bash
# Train-22 fixup: restore crypto/tls's TestCertCache disclosure entry (removed by the Linux refresh a16df3995 on
# Linux evidence; the manifest is ONE file per package shared by every platform, and Windows still fires it).
# Run ONLY after the train-22 chain has finished (its sweeps leg restores src/core at its end). Then re-run the
# crypto/tls row with its record preserved, confirm 400 + 2, and land with coord-train22-land.sh.
set -euo pipefail
W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; SP=/c/Projects/go2cs/.claude/coord-scripts
cd "$W"; export DOTNET_ROOT='$HOME\dotnet10' GOROOT='$HOME\sdk\go1.23.12' PATH="$HOME/dotnet10:$HOME/sdk/go1.23.12/bin:$PATH" MSYS_NO_PATHCONV=1 MSBUILDDISABLENODEREUSE=1
ts=$(date +%Y%m%d-%H%M%S); log="$SP/coord-train22-tls-fixup-$ts.log"; stamp(){ echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "ABORT: worktree dirty (chain still running?)"; exit 1; }
stamp "FIXUP START head=$(git rev-parse --short HEAD)"
# G's precondition: the d188e89ed blob equals the intended post-fixup blob ONLY if no other train-22 seat touched the file.
touchers=$(git log --oneline d188e89ed..HEAD -- src/core/crypto/tls/go2cs_test_disclosures.json | wc -l); stamp "commits touching the tls manifest since d188e89ed: $touchers (must be exactly 1 -- the Linux refresh)"
[ "$touchers" = "1" ] || { stamp "ABORT: another seat touched the tls manifest -- restore the six-line entry as a HUNK instead"; exit 1; }
cp "$SP/coord-tls-manifest-d188e89ed.json" src/core/crypto/tls/go2cs_test_disclosures.json
n=$(git diff --numstat -- src/core/crypto/tls/go2cs_test_disclosures.json | awk '{print $1"/"$2}'); stamp "manifest numstat vs head: $n (want 6/0 -- the six-line TestCertCache object added, nothing removed)"
[ "$n" = "6/0" ] || { stamp "ABORT: numstat is not 6/0"; git checkout -- src/core/crypto/tls/go2cs_test_disclosures.json; exit 1; }
python -m json.tool src/core/crypto/tls/go2cs_test_disclosures.json > /dev/null && stamp "manifest parses as JSON" || { stamp "ABORT: restored manifest does NOT parse -- a manifest that will not parse reads as NO disclosures"; git checkout -- src/core/crypto/tls/go2cs_test_disclosures.json; exit 1; }
[ "$(git diff --name-only | wc -l)" = "1" ] || { stamp "ABORT: more than one file changed"; git checkout -- .; exit 1; }
git add src/core/crypto/tls/go2cs_test_disclosures.json
git commit -S -q -F "$SP/coord-tls-fixup-msg.txt"; stamp "fixup committed $(git rev-parse --short HEAD)"
stamp "re-running crypto/tls row (record preserved before restore)"
powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter crypto/tls -Exact -TestTimeout 30m -SkipBuild > "$SP/coord-train22-tls-rerun-$ts.log" 2>&1 || true
keep="$SP/coord-pkg-run-record-crypto.tls-train22-fixup-$ts"; mkdir -p "$keep"; for f in go2cs_test_comparison.json go2cs_test_results.json go2cs_test_results.xml; do [ -f "src/core/crypto/tls/$f" ] && cp "src/core/crypto/tls/$f" "$keep/"; done
tr -d '\000' < "$SP/coord-train22-tls-rerun-$ts.log" | grep -aE '^\s*(PASS|FAIL|COUNT)\s' | tee -a "$log"
git checkout -q HEAD -- src/core docs/validation/current 2>/dev/null; git clean -fdq src/core/crypto/tls docs/validation 2>/dev/null
stamp "post-rerun dirty=$(git status --porcelain | wc -l) (must be 0); FIXUP END -- if the row reads PASS 400 (+2 disclosed), run coord-train22-land.sh"
