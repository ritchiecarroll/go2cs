#!/usr/bin/env bash
# coord-t29-golibtc0-run.sh -- union GolibTests at Release + DOTNET_TieredCompilation=0 on train 29's assembled head. Env PINNED (the 03:30 copy without the pin built against the ambient 9.0.317 SDK: 77 x NETSDK1045, no assembly, and the wrapper exited 0 over it -- route #6, hand-typed). Exit code PROPAGATES.
set -u
SP='C:/Projects/go2cs/.claude/coord-scripts'; ts="t29-$(date +%H%M%S)"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 2
echo "sdk: $(dotnet --version)  head: $(git rev-parse --short=9 HEAD)"
ROOT="$(pwd -W)/src/"
cd src/tests/GolibTests || exit 2
dotnet build -c Release -p:UseSharedCompilation=false -p:go2csPath="$ROOT" -clp:ErrorsOnly > "$SP/coord-t29union-golibtc0-build-$ts.log" 2>&1; b=$?; echo "build exit=$b" >> "$SP/coord-t29union-golibtc0-build-$ts.log"
if [ $b -ne 0 ]; then echo "BUILD FAILED exit=$b (strict errors: $(grep -acE 'error (CS|MSB|NETSDK)[0-9]+' "$SP/coord-t29union-golibtc0-build-$ts.log"))"; exit 1; fi
DOTNET_TieredCompilation=0 dotnet test --no-build -c Release -p:go2csPath="$ROOT" --logger "console;verbosity=minimal" > "$SP/coord-t29union-golibtc0-$ts.log" 2>&1; t=$?; echo "tc0 exit=$t" >> "$SP/coord-t29union-golibtc0-$ts.log"
declared=$(grep -rc '\[TestMethod\]' --include='*.cs' . | awk -F: '{s+=$2} END{print s}')
echo "TC0 exit=$t declared(raw grep)=$declared :: $(grep -aE 'Passed!|Failed!|Aborted|Total tests' "$SP/coord-t29union-golibtc0-$ts.log" | tail -2 | tr '\n' ' ')"
exit $t
