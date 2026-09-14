#!/bin/bash
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-drop
export DOTNET_ROOT=$HOME/dotnet10
export GOROOT=$HOME/sdk/go1.23.12
export PATH="$GOROOT/bin:$DOTNET_ROOT:$PATH"
export CGO_ENABLED=0 MSBUILDDISABLENODEREUSE=1
V=$(go version 2>&1); case "$V" in *go1.23.12*) echo "PIN OK: $V";; *) echo "ABORT pin: $V"; exit 9;; esac
cd "$W" || exit 8
git checkout -q --detach ${BISECT:?} || exit 8
git clean -qfd 2>/dev/null
echo "=== tree $(git log -1 --format=%h) (BISECT probe)"
# doctrine 692: the arm PRINTS what it holds, so the check cannot be skipped
for accused in ${ACCUSED:-eed11b550}; do
  if git merge-base --is-ancestor "$accused" HEAD 2>/dev/null; then echo "  ANCESTRY: $accused IS PRESENT in this arm"; else echo "  ANCESTRY: $accused absent"; fi
done
( cd src/go2cs && go build -o bin/go2cs.exe . ) || { echo "ABORT converter build"; exit 8; }
echo "=== reflect -tests -test-action all, Release, 40m"
./src/go2cs/bin/go2cs.exe -tests -test-action all -test-timeout 40m -test-config Release \
  -go2cspath "$W/src" "$GOROOT/src/reflect" "$W/src/core/reflect" \
  > $HOME/AppData/Local/Temp/claude/coord-drop-reflect.log 2>&1
echo "  pipeline exit: $?"
R="$W/src/core/reflect/go2cs_test_comparison.json"
RW=$(cygpath -w "$R" 2>/dev/null || echo "$R")
python - "$RW" <<'PY'
import json,io,sys
try:
    d=json.load(io.open(sys.argv[1],encoding='utf-8'))
except Exception as e:
    print("  record unreadable:",e); raise SystemExit
g=d.get('go') or {}; c=d.get('csharp') or {}
empt=[n for n in g if not c.get(n)]
print("  status",d.get('status'),"go",len(g),"cs",len(c),"EMPTY",len(empt))
diff=[n for n in sorted(set(list(g)+list(c))) if g.get(n)!=c.get(n)]
print("  differing",len(diff))
for n in diff[:8]: print("    ",n,"go=",g.get(n),"cs=",c.get(n))
PY
cp "$W/src/core/reflect/go2cs_test_results.json" "$HOME/AppData/Local/Temp/claude/coord-preserved-results-$BISECT.json" 2>/dev/null
cp "$W/src/core/reflect/go2cs_test_comparison.json" "$HOME/AppData/Local/Temp/claude/coord-preserved-comparison-$BISECT.json" 2>/dev/null
echo "  records PRESERVED to distinct paths for $BISECT"
echo "=== results tail (deadline/crash statement):"
tail -c 900 "$W/src/core/reflect/go2cs_test_results.json" 2>/dev/null | tr ',' '\n' | grep -iE 'timeout|action|exit status' | tail -4
