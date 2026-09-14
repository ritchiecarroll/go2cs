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
git merge-base --is-ancestor eed11b550 HEAD 2>/dev/null && { echo "  ANCESTRY: token seat PRESENT"; exit 8; } || echo "  ANCESTRY: token seat absent"
for p in reflect internal/reflectlite; do
  tag=$(echo $p | tr / -)
  echo "=== $p : convert then build (build alone refuses a stale manifest)"
  ./src/go2cs/bin/go2cs.exe -tests -test-action convert -test-config Release \
    -go2cspath "$W/src" "$GOROOT/src/$p" "$W/src/core/$p" > "$T/drop-conv-$tag.log" 2>&1
  echo "  convert exit $?"
  ./src/go2cs/bin/go2cs.exe -tests -test-action build -test-timeout 30m -test-config Release \
    -go2cspath "$W/src" "$GOROOT/src/$p" "$W/src/core/$p" > "$T/drop-build-$tag.log" 2>&1
  rc=$?
  echo "  BUILD exit $rc | strict errors: $(grep -cE 'error (CS|MSB|NETSDK)[0-9]+' "$T/drop-build-$tag.log" 2>/dev/null)"
  [ $rc -ne 0 ] && { grep -E 'error (CS|MSB|NETSDK)[0-9]+' "$T/drop-build-$tag.log" 2>/dev/null | head -3 | sed 's/^/    /'; tail -2 "$T/drop-build-$tag.log" | sed 's/^/    /'; }
done
echo "=== TESTS-BUILD LEG COMPLETE"
