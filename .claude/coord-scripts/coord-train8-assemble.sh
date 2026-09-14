#!/usr/bin/env bash
# coord-train8-assemble.sh -- merge train 8 (C2's host fix + the per-flavor visibility invariant) into the
# coordinator worktree after train 7 is pushed, then run its battery: go2cs.slnx, GolibTests --no-build, the
# each-class-ALONE ordering control, and the five-row filtered sweep. Run from the worktree root.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train8-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }

stamp "TRAIN 8 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master claude/c2-golibtests-abort claude/c2-tz-pin-invariant claude/g-structof-embedded-methods || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "HEAD is not on top of origin/master -- ABORT"; exit 1; }
[ "$(git rev-parse origin/claude/c2-golibtests-abort)" = "$(git rev-parse f21ff7866)" ] || stamp "NOTE: c2-golibtests-abort tip moved from f21ff7866"
[ "$(git rev-parse origin/claude/c2-tz-pin-invariant)" = "$(git rev-parse f7cf8124c)" ] || stamp "NOTE: c2-tz-pin-invariant tip moved from f7cf8124c"
# G's row 2 (e57fe22c7) is OFF train 8 by ruling 2026-09-02 04:30 -- DynamicMethod forwarders replaced by (b), one branch for rows 1-3
for pair in "origin/claude/c2-golibtests-abort|$SP/coord-merge-c2-abort-fix.txt|C2 host fix" "origin/claude/c2-tz-pin-invariant|$SP/coord-merge-c2-tz-invariant.txt|C2 tz-pin invariant"; do
  ref=${pair%%|*}; rest=${pair#*|}; msg=${rest%%|*}; label=${rest#*|}
  if git merge --no-ff -S -q -F "$msg" "$ref"; then stamp "merged $label -> $(git rev-parse --short HEAD)"; else stamp "MERGE FAILED $label"; git merge --abort 2>/dev/null; exit 1; fi
done
# R's NewAt branch: the reflect disclosure manifest is an append-append against train 7's unsafeslice entry -- union it
git fetch -q origin claude/reflect-tail-r-newat || { stamp "fetch newat failed -- ABORT"; exit 1; }
[ "$(git rev-parse origin/claude/reflect-tail-r-newat)" = "$(git rev-parse 700ec2060 2>/dev/null)" ] || stamp "NOTE: reflect-tail-r-newat tip moved from 700ec2060 (message file names 700ec2060 -- fix before use)"
if git merge --no-ff -S -q -F "$SP/coord-merge-r-newat.txt" origin/claude/reflect-tail-r-newat; then
  stamp "merged R NewAt -> $(git rev-parse --short HEAD)"
else
  unmerged=$(git diff --name-only --diff-filter=U)
  if [ "$unmerged" = "src/core/reflect/go2cs_test_disclosures.json" ]; then
    python "$SP/coord-merge-disclosures.py" src/core/reflect/go2cs_test_disclosures.json | tee -a "$log" && git add src/core/reflect/go2cs_test_disclosures.json && git commit -S -q -F "$SP/coord-merge-r-newat.txt" || { stamp "NewAt merge commit FAILED"; git merge --abort 2>/dev/null; exit 1; }
    stamp "merged R NewAt (manifest union) -> $(git rev-parse --short HEAD); conflict markers in tree: $(git grep -c '^<<<<<<<' -- src/core/reflect/go2cs_test_disclosures.json | wc -l)"
  else
    stamp "NewAt merge conflicts outside the manifest: $unmerged -- ABORT"; git merge --abort 2>/dev/null; exit 1
  fi
fi
python -c "import json;d=json.load(open('src/core/reflect/go2cs_test_disclosures.json',encoding='utf-8-sig'));print('manifest entries at head:',len(d['disclosures']))" | tee -a "$log"
# C2's Sendto (converter class): registrations in manualTypeOperations.go may collide line-adjacent with NewAt's -- keep BOTH sides, gofmt, go build
git fetch -q origin claude/c2-syscall-sendto || { stamp "fetch sendto failed -- ABORT"; exit 1; }
[ "$(git rev-parse origin/claude/c2-syscall-sendto)" = "$(git rev-parse f7cd10ede 2>/dev/null)" ] || stamp "NOTE: c2-syscall-sendto tip moved from f7cd10ede"
if git merge --no-ff -S -q -F "$SP/coord-merge-c2-sendto.txt" origin/claude/c2-syscall-sendto; then
  stamp "merged C2 Sendto -> $(git rev-parse --short HEAD)"
else
  unmerged=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
  if [ "$unmerged" = "src/go2cs/manualTypeOperations.go " ]; then
    python - <<'PY'
import io,re
p='src/go2cs/manualTypeOperations.go'; s=io.open(p,encoding='utf-8',newline='').read()
n=s.count('<<<<<<<')
# keep ours then theirs for every conflict block (append-append registrations)
s2=re.sub(r'<<<<<<< [^\r\n]*\r?\n(.*?)=======\r?\n(.*?)>>>>>>> [^\r\n]*\r?\n', lambda m: m.group(1)+m.group(2), s, flags=re.S)
assert '<<<<<<<' not in s2 and '>>>>>>>' not in s2 and '=======' not in s2, 'residual markers'
io.open(p,'w',encoding='utf-8',newline='').write(s2); print('manualTypeOperations.go: resolved', n, 'conflict block(s) by keeping both sides')
PY
    (cd src/go2cs && gofmt -l manualTypeOperations.go >/dev/null; gofmt -w manualTypeOperations.go && go build ./... ) || { stamp "go build after union FAILED -- ABORT"; git merge --abort 2>/dev/null; exit 1; }
    git add src/go2cs/manualTypeOperations.go && git commit -S -q -F "$SP/coord-merge-c2-sendto.txt" || { stamp "Sendto merge commit FAILED"; git merge --abort 2>/dev/null; exit 1; }
    stamp "merged C2 Sendto (registration union) -> $(git rev-parse --short HEAD)"
  else
    stamp "Sendto merge conflicts outside manualTypeOperations.go: $unmerged -- ABORT"; git merge --abort 2>/dev/null; exit 1
  fi
fi
# G's trio (option (b) promoted methods, 840b85543): shares value_impl.cs with NewAt -- three-way merge expected clean; anything else aborts for a hand resolution
git fetch -q origin claude/g-structof-promoted-methods || { stamp "fetch trio failed -- ABORT"; exit 1; }
[ "$(git rev-parse origin/claude/g-structof-promoted-methods)" = "$(git rev-parse 840b85543 2>/dev/null)" ] || stamp "NOTE: g-structof-promoted-methods tip moved from 840b85543"
if git merge --no-ff -S -q -F "$SP/coord-merge-g-trio.txt" origin/claude/g-structof-promoted-methods; then
  stamp "merged G trio -> $(git rev-parse --short HEAD)"
else
  stamp "G trio merge CONFLICTS: $(git diff --name-only --diff-filter=U | tr '\n' ' ') -- ABORT for hand resolution"; git merge --abort 2>/dev/null; exit 1
fi
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B" || { stamp "converter build FAILED -- ABORT"; exit 1; }
stamp "TRAIN 8 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"

stamp "BATTERY: converter suite + full CNR (converter class: Sendto's registration + NewAt's)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train8-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train8-suite-cnr-$ts.log"
stamp "BATTERY: syscall.csproj on linux (Sendto's flavor) -- obj purged first"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train8-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train8-syscall-linux-$ts.log"
stamp "BATTERY: slnx"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train8-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train8-slnx-$ts.log"
stamp "BATTERY: GolibTests --no-build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train8-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train8-golibtests-$ts.log"
stamp "BATTERY: each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train8-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train8-alone-$ts.log"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train8-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train8-reflect-$ts.log"
stamp "BATTERY: sweeps -- five-row host-fix set + reflect-importer canaries + nistec cost canary"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps crypto/internal/nistec,unicode/utf8,sort,strings,encoding/json,encoding/xml,crypto/x509,go/types,os/exec -SweepTimeout 10m > "$SP/coord-train8-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train8-sweeps-$ts.log"
stamp "BATTERY: reflect RUN (unbanked row -- read the tail and the empty count, not the verdict)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps reflect -SweepTimeout 30m > "$SP/coord-train8-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train8-reflectrun-$ts.log"
stamp "TRAIN 8 CHAIN DONE"
