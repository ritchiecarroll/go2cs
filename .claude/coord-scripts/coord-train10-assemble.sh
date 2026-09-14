#!/usr/bin/env bash
# coord-train10-assemble.sh -- merge train 10 into the coordinator worktree after train 9 is pushed, then run its battery.
# Seats (remote tips; set the SHA env vars to the ANNOUNCED tips so a moved tip is NOTEd; an unset var skips the seat):
#   RECVMSG_SHA  claude/c2-syscall-recvmsg          (converter registrations + linux hand-owns + ScmRightsSeam guard)
#   CHANDIR_SHA  $CHANDIR_BRANCH (R's one-train chanDir cut: narrowing stamp + marshalling direction check)
#   CPUID_SHA    claude/g-cpuid-x86-detection       (internal/cpu hand-own + ModuleInitializer)
# Battery: converter suite + full CNR; syscall.csproj linux; go2cs.slnx; GolibTests count-matched + each-class-alone;
# reflect -tests build + RUN; sweeps: nistec (cost), encoding/json, encoding/xml, crypto/x509, go/types (shared-golib
# canaries), crypto/aes, crypto/cipher, crypto/tls (cpuid), unicode/utf8, sort, strings, os/exec.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
RECVMSG_SHA="${RECVMSG_SHA:-e20510be9}"; CHANDIR_SHA="${CHANDIR_SHA:-}"; CHANDIR_BRANCH="${CHANDIR_BRANCH:-}"; CPUID_SHA="${CPUID_SHA:-acc79ab48}"
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train10-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 10 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "HEAD is not on top of origin/master -- ABORT"; exit 1; }
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
seat() { # branch sha msg label
  [ -n "$2" ] || { stamp "$4: SHA unset -- seat SKIPPED"; return; }
  git fetch -q origin "$1" || { stamp "fetch $1 failed -- ABORT"; exit 1; }
  [ "$(git rev-parse origin/$1)" = "$(git rev-parse $2 2>/dev/null)" ] || stamp "NOTE: $1 tip is not $2 (announced) -- merging the remote tip; fix the message file"
  merge_one "origin/$1" "$3" "$4"
}
seat claude/c2-syscall-recvmsg "$RECVMSG_SHA" "$SP/coord-merge-c2-recvmsg.txt" "C2 Recvmsg seam"
[ -n "$CHANDIR_BRANCH" ] && seat "$CHANDIR_BRANCH" "$CHANDIR_SHA" "$SP/coord-merge-r-chandir.txt" "R chanDir" || stamp "R chanDir: branch unset -- seat SKIPPED"
seat claude/g-cpuid-x86-detection "$CPUID_SHA" "$SP/coord-merge-g-cpuid.txt" "G cpuid"
BOARD_SHA="${BOARD_SHA:-128e7042f}"; seat claude/g-board-mathbits-null "$BOARD_SHA" "$SP/coord-merge-g-board-mathbits.txt" "G board (math/bits null)"
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B" || { stamp "converter build FAILED -- ABORT"; exit 1; }
stamp "TRAIN 10 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"

stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train10-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train10-suite-cnr-$ts.log"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train10-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train10-syscall-linux-$ts.log"
stamp "BATTERY: slnx"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train10-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train10-slnx-$ts.log"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train10-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train10-golibtests-$ts.log"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train10-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train10-alone-$ts.log"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train10-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train10-reflect-$ts.log"
stamp "BATTERY: sweeps"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps crypto/internal/nistec,encoding/json,encoding/xml,crypto/x509,go/types,crypto/aes,crypto/cipher,unicode/utf8,sort,strings,os/exec -SweepTimeout 10m > "$SP/coord-train10-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train10-sweeps-$ts.log"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label coord-train10 > "$SP/coord-train10-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train10-reflectrun-$ts.log"
stamp "TRAIN 10 CHAIN DONE"
