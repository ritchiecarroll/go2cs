#!/bin/bash
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-drop
T=$HOME/AppData/Local/Temp/claude
export DOTNET_ROOT=$HOME/dotnet10
export GOROOT=$HOME/sdk/go1.23.12
export PATH="$GOROOT/bin:$DOTNET_ROOT:$PATH"
export CGO_ENABLED=0 MSBUILDDISABLENODEREUSE=1
V=$(go version 2>&1); case "$V" in *go1.23.12*) echo "PIN OK: $V";; *) echo "ABORT pin: $V"; exit 9;; esac
cd "$W" || exit 8
echo "=== TREE $(git log -1 --format=%h)"
git merge-base --is-ancestor eed11b550 HEAD 2>/dev/null && { echo "  ANCESTRY: token seat PRESENT -- WRONG TREE"; exit 8; } || echo "  ANCESTRY: token seat absent"
echo "=== LEG: GolibTests, BOTH configurations (declared-count check per doctrine)"
DECL=$(grep -rho '\[TestMethod\]' src/tests/GolibTests/*.cs 2>/dev/null | wc -l)
echo "  declared [TestMethod] on disk: $DECL (compile-set exclusions may reduce it)"
for cfg in Debug Release; do
  if [ "$cfg" = Release ]; then EXTRA="-e DOTNET_TieredCompilation=0"; else EXTRA=""; fi
  DOTNET_TieredCompilation=$([ "$cfg" = Release ] && echo 0 || echo 1) \
    dotnet test src/tests/GolibTests/GolibTests.csproj -c $cfg --nologo > "$T/drop-golib-$cfg.log" 2>&1
  rc=$?
  ab=$(grep -c 'Test Run Aborted' "$T/drop-golib-$cfg.log" 2>/dev/null)
  line=$(grep -oE 'Failed: *[0-9]+, *Passed: *[0-9]+|Passed! *- *Failed: *[0-9]+, *Passed: *[0-9]+|total: *[0-9]+' "$T/drop-golib-$cfg.log" 2>/dev/null | tail -1)
  tot=$(grep -oE 'Total: *[0-9]+' "$T/drop-golib-$cfg.log" 2>/dev/null | tail -1)
  echo "  $cfg: exit $rc | aborted=$ab | $line | $tot"
done
echo "=== LEG: reflect and internal/reflectlite -tests BUILDS (route #7's neighbour)"
for p in reflect internal/reflectlite; do
  ./src/go2cs/bin/go2cs.exe -tests -test-action build -test-timeout 30m -test-config Release \
    -go2cspath "$W/src" "$GOROOT/src/$p" "$W/src/core/$p" > "$T/drop-tests-$(echo $p | tr / -).log" 2>&1
  echo "  $p: build exit $?"
done
echo "=== BATTERY PART 3 COMPLETE"
