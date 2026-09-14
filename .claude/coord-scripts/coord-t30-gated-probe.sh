#!/bin/bash
set -uo pipefail
W=/c/Projects/go2cs/.claude/worktrees/coord-t25-dryrun
HEAD=9c33b95c0
export DOTNET_ROOT=$HOME/dotnet10
export GOROOT=$HOME/sdk/go1.23.12
export PATH="$GOROOT/bin:$DOTNET_ROOT:$PATH"
export CGO_ENABLED=0 MSBUILDDISABLENODEREUSE=1
V=$(go version 2>&1)
case "$V" in *go1.23.12*) echo "PIN OK: $V";; *) echo "ABORT -- toolchain pin mismatch: $V"; exit 9;; esac
cd "$W" || exit 8
[ -n "$(git status --porcelain)" ] && { echo "ABORT -- worktree dirty"; exit 8; }
ORIG=$(git rev-parse --abbrev-ref HEAD); [ "$ORIG" = HEAD ] && ORIG=$(git rev-parse HEAD)
echo "=== detaching at $HEAD (from $ORIG)"
git checkout -q --detach "$HEAD" || { echo "ABORT -- checkout failed"; exit 8; }
echo "=== building converter"
( cd src/go2cs && go build -o bin/go2cs.exe . ) || { echo "ABORT -- converter build failed"; git checkout -q "$ORIG"; exit 8; }
ls -l src/go2cs/bin/go2cs.exe | awk '{print "  binary:",$5,"bytes",$6,$7,$8}'
P=${PKG:?}
GP="$GOROOT/src/$P"
CP="$W/src/core/$P"
echo "=== goroot pkg: $(ls "$GP"/*.go 2>/dev/null | wc -l) go files, $(ls "$GP"/*_test.go 2>/dev/null | wc -l) test files"
echo "=== running pipeline (Release, 30m)"
./src/go2cs/bin/go2cs.exe -tests -test-action all -test-timeout 30m -test-config Release -test-filter "${FILTER:?}" \
  -go2cspath "$W/src" "$GP" "$CP" > $HOME/AppData/Local/Temp/claude/coord-row-probe.log 2>&1
echo "  pipeline exit: $?"
tail -25 $HOME/AppData/Local/Temp/claude/coord-row-probe.log
REC="$CP/go2cs_test_comparison.json"
RECW=$(cygpath -w "$REC" 2>/dev/null || echo "$REC")   # python is native: bash /c/ paths do not resolve for it
echo "=== comparison record: $([ -f "$REC" ] && echo present || echo ABSENT)"
[ -f "$REC" ] && python -c "
import json,io,sys
d=json.load(io.open(r'$RECW',encoding='utf-8'))
ts=d.get('tests') or d.get('Tests') or []
print('  entries:',len(ts))
for t in ts[:20]:
    print('   ',t.get('name') or t.get('Name'),'go=',t.get('goResult') or t.get('GoResult'),'cs=',t.get('csResult') or t.get('CsResult'))
" 2>&1 | head -25
echo "=== restoring"
git checkout -q "$ORIG"; git clean -qfd src/core 2>/dev/null
echo "  back at $(git log -1 --format=%h), dirty=$(git status --porcelain | wc -l)"
