#!/usr/bin/env bash
# coord-train9-assemble.sh -- merge train 9 into the coordinator worktree after train 8 is pushed, then run its battery.
# Seats (each taken from the REMOTE tip, a NOTE stamped if it differs from the SHA the merge message names):
#   1. C2 item 4   claude/c2-typed-nil-array-dims        (converter + golib; message names 6e1c20229)
#   2. R VALID arm claude/reflect-tail-r-structmarshal  (golib; message names 839351aac)
#   3. i9 funcInfo claude/i9-funcinfo-bridge            (converter + runtime hand-own; REBASED tip expected -- set I9_SHA before running)
# Battery: converter suite + full CNR; go2cs.slnx; GolibTests count-matched + each-class-alone; reflect -tests build + RUN;
# runtime -tests build + runtime.csproj on 3 targets (funcInfo); sweeps: nistec (cost), encoding/binary (item 4's canary),
# encoding/json, encoding/xml, crypto/x509, go/types (reflect importers), unicode/utf8, sort, strings, os/exec.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
I9_SHA="${I9_SHA:-f5ca2621e}"
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train9-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 9 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master claude/c2-typed-nil-array-dims claude/reflect-tail-r-structmarshal claude/i9-funcinfo-bridge || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "HEAD is not on top of origin/master -- ABORT"; exit 1; }
[ "$(git rev-parse origin/claude/c2-typed-nil-array-dims)" = "$(git rev-parse 6e1c20229 2>/dev/null)" ] || stamp "NOTE: c2-typed-nil-array-dims tip moved from 6e1c20229 (fix the message file before use)"
[ "$(git rev-parse origin/claude/reflect-tail-r-structmarshal)" = "$(git rev-parse 839351aac 2>/dev/null)" ] || stamp "NOTE: reflect-tail-r-structmarshal tip moved from 839351aac"
if [ -n "$I9_SHA" ]; then [ "$(git rev-parse origin/claude/i9-funcinfo-bridge)" = "$(git rev-parse $I9_SHA 2>/dev/null)" ] || stamp "NOTE: i9-funcinfo-bridge tip is not $I9_SHA"; else stamp "I9_SHA not set -- the funcInfo seat is SKIPPED this train"; fi
merge_one() { # ref msg label
  if git merge --no-ff -S -q -F "$2" "$1"; then stamp "merged $3 -> $(git rev-parse --short HEAD)"; else
    unmerged=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
    if [ "$unmerged" = "src/go2cs/manualTypeOperations.go " ]; then
      python - <<'PY'
import io,re
p='src/go2cs/manualTypeOperations.go'; s=io.open(p,encoding='utf-8',newline='').read(); n=s.count('<<<<<<<')
s2=re.sub(r'<<<<<<< [^\r\n]*\r?\n(.*?)=======\r?\n(.*?)>>>>>>> [^\r\n]*\r?\n', lambda m: m.group(1)+m.group(2), s, flags=re.S)
assert '<<<<<<<' not in s2 and '>>>>>>>' not in s2, 'residual markers'
io.open(p,'w',encoding='utf-8',newline='').write(s2); print('manualTypeOperations.go: resolved', n, 'block(s) by keeping both sides')
PY
      (cd src/go2cs && gofmt -w manualTypeOperations.go && go build ./...) || { stamp "go build after union FAILED -- ABORT"; git merge --abort 2>/dev/null; exit 1; }
      git add src/go2cs/manualTypeOperations.go && git commit -S -q -F "$2" || { stamp "$3 merge commit FAILED"; git merge --abort 2>/dev/null; exit 1; }
      stamp "merged $3 (registration union) -> $(git rev-parse --short HEAD)"
    else stamp "$3 merge CONFLICTS: $unmerged -- ABORT for hand resolution"; git merge --abort 2>/dev/null; exit 1; fi
  fi
}
merge_one origin/claude/c2-typed-nil-array-dims "$SP/coord-merge-c2-item4.txt" "C2 item 4"
merge_one origin/claude/reflect-tail-r-structmarshal "$SP/coord-merge-r-structmarshal.txt" "R VALID arm"
[ -n "$I9_SHA" ] && merge_one origin/claude/i9-funcinfo-bridge "$SP/coord-merge-i9-funcinfo.txt" "i9 funcInfo"
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B" || { stamp "converter build FAILED -- ABORT"; exit 1; }
stamp "TRAIN 9 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"

stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train9-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train9-suite-cnr-$ts.log"
stamp "BATTERY: slnx"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train9-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train9-slnx-$ts.log"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train9-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train9-golibtests-$ts.log"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train9-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train9-alone-$ts.log"
stamp "BATTERY: reflect -tests build; runtime -tests build; runtime x3 targets"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train9-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train9-reflect-$ts.log"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-pkg-tests-build.ps1" -Package runtime > "$SP/coord-train9-runtime-$ts.log" 2>&1; echo "runtime exit=$?" >> "$SP/coord-train9-runtime-$ts.log"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-runtime-3targets.ps1" > "$SP/coord-train9-3targets-$ts.log" 2>&1; echo "3targets exit=$?" >> "$SP/coord-train9-3targets-$ts.log"
stamp "BATTERY: sweeps"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps crypto/internal/nistec,encoding/binary,encoding/json,encoding/xml,crypto/x509,go/types,unicode/utf8,sort,strings,os/exec -SweepTimeout 10m > "$SP/coord-train9-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train9-sweeps-$ts.log"
stamp "BATTERY: reflect RUN (unbanked row -- tail and empty count)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label coord-train9 > "$SP/coord-train9-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train9-reflectrun-$ts.log"
stamp "TRAIN 9 CHAIN DONE"
