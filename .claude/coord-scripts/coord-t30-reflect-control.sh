#!/usr/bin/env bash
# Is the reflect host death NEW at the train-30 head, or pre-existing? The five-minute control: run the SAME row at the
# train-29 landed master in the free dry-run worktree, and compare the empty span. The head run died with an
# AccessViolationException inside the test's own byte-offset write helper, taking 221 rows with it.
set -u; W=/c/Projects/go2cs/.claude/worktrees/coord-t25-dryrun; S=/c/Projects/go2cs/.claude/coord-scripts
cd "$W" || exit 1
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "dry-run worktree dirty -- not running"; git status --porcelain | head -3; exit 1; }
git fetch -q origin master && git checkout -q --detach b91684991 || { echo "checkout failed"; exit 1; }
echo "control tree: $(git rev-parse --short=9 HEAD) (the train-29 landed master)"
export GOROOT='$HOME\sdk\go1.23.12' DOTNET_ROOT='$HOME\dotnet10' MSBUILDDISABLENODEREUSE=1 CGO_ENABLED=0
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
go version | grep -q go1.23.12 || { echo "GO PIN FAILED: $(go version)"; exit 1; }
(cd src/go2cs && go build -o bin/go2cs.exe . ) || { echo "converter build failed"; exit 1; }
L="$S/coord-t30-reflect-control-$(date +%H%M%S).log"
./src/go2cs/bin/go2cs.exe -tests -test-action all -test-timeout 30m -test-config Release -go2cspath "$(pwd -W)/src" "$GOROOT/src/reflect" "$(pwd -W)/src/core/reflect" > "$L" 2>&1
echo "pipeline exit=$? log=$L"
echo "=== does the control die at the same place?"; grep -aE 'Fatal error|AccessViolation|setField|"test":"TestIsZero"' "$L" | head -6 | cut -c1-170
echo "=== empty span:"; python - "$W" <<'PY'
import json,sys,os
f=os.path.join(sys.argv[1],'src','core','reflect','go2cs_test_comparison.json')
if os.path.exists(f):
    js=json.load(open(f,encoding='utf-8-sig')); go=js.get('go') or {}; cs=js.get('csharp') or {}
    names=sorted(go); miss=[n for n in names if not cs.get(n)]
    print("go",len(go),"cs-nonempty",len([n for n in names if cs.get(n)]),"empty",len(miss))
    if miss: print("empty span:",miss[0],"..",miss[-1])
else: print("no comparison record")
PY
git checkout -q claude/coord-t30-rtlgetversion 2>/dev/null; git checkout -q -- . ; git clean -qfd src/core 2>/dev/null; echo "restored: $(git rev-parse --abbrev-ref HEAD) dirty $(git status --porcelain | wc -l)"
