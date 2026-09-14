#!/usr/bin/env bash
# coord-cle-rebaseline.sh -- after the train-23 chain ends: re-baseline CompositeLiteralElements under the MERGED converter
# (the union of SUB-Q1's guard and R's Increment C), commit with the prepared message, then the two gates the landing owes:
# a full CNR at the new head (expect 714/714 byte-identical) and the filtered four-phase run of the project. Run from the
# coordinator worktree cwd. Refuses on a dirty tree or a live runner in this root.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
ts=$(date +%Y%m%d-%H%M%S); log="$SP/coord-cle-rebaseline-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
[ "$(basename "$(pwd)")" = "musing-moser-d4552c" ] || { stamp "cwd is not the coordinator worktree -- ABORT"; exit 1; }
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
live=$(powershell -NoProfile -Command "@(Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*musing-moser-d4552c*' -and (\$_.Name -in 'BehavioralRunner.exe','go2cs.exe','powershell.exe','pwsh.exe','dotnet.exe') -and \$_.CommandLine -notlike '*Get-CimInstance*' }).Count" 2>/dev/null | tr -d '\r')
[ "${live:-0}" = "0" ] || { stamp "live process(es) in this root: $live -- the chain has not ended; ABORT"; exit 1; }
head0=$(git rev-parse --short HEAD); stamp "REBASELINE START head=$head0"
(cd src/go2cs && go build -o bin/go2cs.exe .) || { stamp "converter build FAILED -- ABORT"; exit 1; }
stamp "converter built at $head0: $(stat -c %s src/go2cs/bin/go2cs.exe) B"
# the utility re-transpiles unconditionally and refuses on best-effort (SUB-Q11), and --only confines it to one project
(cd src/utilities/UpdateTestTargets && dotnet build -c Debug -p:UseSharedCompilation=false -clp:ErrorsOnly >/dev/null 2>&1) || { stamp "UpdateTestTargets build FAILED -- ABORT"; exit 1; }
(cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe --createTargetFiles --only CompositeLiteralElements > "$SP/coord-cle-utt-$ts.log" 2>&1) || { stamp "UpdateTestTargets --only CompositeLiteralElements FAILED (a refusal is a best-effort conversion) -- ABORT: $(tail -3 "$SP/coord-cle-utt-$ts.log" | tr '\n' ' ')"; exit 1; }
changed=$(git status --porcelain | wc -l); other=$(git status --porcelain | grep -v 'src/tests/Behavioral/CompositeLiteralElements/' | wc -l)
[ "$other" = "0" ] || { stamp "re-baseline touched files outside the project -- ABORT: $(git status --porcelain | tr '\n' ' ')"; exit 1; }
[ "$changed" != "0" ] || { stamp "NOTHING CHANGED -- the golden already matches the merged emission; nothing to commit (was the wrong binary run?) -- ABORT"; exit 1; }
cmp -s src/tests/Behavioral/CompositeLiteralElements/main.cs src/tests/Behavioral/CompositeLiteralElements/main.cs.target || { stamp ".cs and .cs.target DIFFER after the re-baseline -- ABORT"; exit 1; }
diffkinds=$(git diff --numstat -- src/tests/Behavioral/CompositeLiteralElements | tr '\n' ' '); stamp "numstat: $diffkinds"
n_dims=$(git diff -- src/tests/Behavioral/CompositeLiteralElements/main.cs | grep -c '^+.*WithElemDims'); stamp "added WithElemDims lines in main.cs: $n_dims (expect 5)"
[ "$n_dims" = "5" ] || { stamp "unexpected shape -- ABORT for a read"; exit 1; }
git add src/tests/Behavioral/CompositeLiteralElements && git commit -S -q -F "$SP/coord-cle-rebaseline-msg.txt" || { stamp "commit FAILED -- ABORT"; exit 1; }
stamp "REBASELINED -> $(git rev-parse --short HEAD) ($(git show --stat --format= HEAD | tail -1))"
stamp "GATE: filtered four-phase run of CompositeLiteralElements"
(cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -Command "\$ErrorActionPreference='Continue'; & './run-behavioral.ps1' --filter CompositeLiteralElements") > "$SP/coord-cle-filtered-$ts.log" 2>&1; echo "filtered exit=$?" >> "$SP/coord-cle-filtered-$ts.log"
stamp "filtered: $(tail -3 "$SP/coord-cle-filtered-$ts.log" | tr -d '\000' | tr '\n' ' ' | cut -c1-200)"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null
stamp "GATE: full CNR at $(git rev-parse --short HEAD)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite > "$SP/coord-cle-cnr-$ts.log" 2>&1; echo "cnr exit=$?" >> "$SP/coord-cle-cnr-$ts.log"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null
stamp "CNR: $(grep -aE 'NO REGRESSION|CHANGED converter output|NOT MEASURED|LEG2 END|cnr exit' "$SP/coord-cle-cnr-$ts.log" | tr '\n' ' ' | cut -c1-300)"
stamp "GATE: GolibTests at Release + DOTNET_TieredCompilation=0 (the union proof of SUB-Q14's seat)"
(cd src/tests/GolibTests && dotnet build -c Release -p:UseSharedCompilation=false -p:go2csPath="$(pwd -W | sed "s#/tests/GolibTests##")/" -clp:ErrorsOnly > "$SP/coord-cle-golibtc0-build-$ts.log" 2>&1; echo "build exit=$?" >> "$SP/coord-cle-golibtc0-build-$ts.log"; DOTNET_TieredCompilation=0 dotnet test --no-build -c Release -p:go2csPath="$(pwd -W | sed "s#/tests/GolibTests##")/" --logger "console;verbosity=minimal" > "$SP/coord-cle-golibtc0-$ts.log" 2>&1; echo "tc0 exit=$?" >> "$SP/coord-cle-golibtc0-$ts.log")
stamp "GolibTests TC0 build: $(tail -1 "$SP/coord-cle-golibtc0-build-$ts.log") strict errors $(grep -ciE "error (CS|MSB|NETSDK)[0-9]+" "$SP/coord-cle-golibtc0-build-$ts.log") :: $(grep -aE 'Passed!|Failed!|Aborted|tc0 exit' "$SP/coord-cle-golibtc0-$ts.log" | tail -3 | tr '\n' ' ' | cut -c1-240)"
stamp "REBASELINE CHAIN DONE head=$(git rev-parse --short HEAD) dirty=$(git status --porcelain | wc -l)"
