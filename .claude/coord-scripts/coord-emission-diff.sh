#!/bin/bash
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-drop
export DOTNET_ROOT=$HOME/dotnet10
export GOROOT=$HOME/sdk/go1.23.12
export PATH="$GOROOT/bin:$DOTNET_ROOT:$PATH"
export CGO_ENABLED=0
T=$HOME/AppData/Local/Temp/claude
cd "$W" || exit 8
for tree in 8693aa5ba 0778bb914; do
  git checkout -q --detach $tree || exit 8
  git clean -qfd src/core/reflect 2>/dev/null
  ( cd src/go2cs && go build -o bin/go2cs.exe . ) || exit 8
  ./src/go2cs/bin/go2cs.exe -tests -test-action convert -go2cspath "$W/src" \
    "$GOROOT/src/reflect" "$W/src/core/reflect" > "$T/coord-conv-$tree.log" 2>&1
  echo "  $tree convert exit $?"
  rm -rf "$T/emit-$tree"; mkdir -p "$T/emit-$tree"
  cp "$W"/src/core/reflect/*_test.cs "$T/emit-$tree/" 2>/dev/null
  cp "$W"/src/core/reflect/package_test_info.cs "$T/emit-$tree/" 2>/dev/null
  echo "    files: $(ls "$T/emit-$tree" | wc -l)"
done
echo "=== DIFF of reflect's TEST emission, 15 seats vs 16:"
diff -rq "$T/emit-8693aa5ba" "$T/emit-0778bb914" 2>/dev/null | sed 's/^/  /' | head -10
echo "=== line delta per differing file:"
for f in $(diff -rq "$T/emit-8693aa5ba" "$T/emit-0778bb914" 2>/dev/null | sed -n 's/^Files .*emit-8693aa5ba\/\([^ ]*\) and.*/\1/p'); do
  echo "  $f: $(diff "$T/emit-8693aa5ba/$f" "$T/emit-0778bb914/$f" | grep -c '^[<>]') changed lines"
done
git checkout -q --detach 0778bb914 2>/dev/null
