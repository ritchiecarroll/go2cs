#!/usr/bin/env bash
# coord-train12-assemble.sh -- merge train 12 after train 11 is pushed, then run its battery. Seats (remote tips; an unset SHA skips):
#   WORDSIZE_SHA claude/g-mathbits-wordsize            (G: one-level word-size Mul/Add/Sub hand-own + AggressiveInlining on Add; RSA 68.5->22.8 ms; converter registry + bits_impl.cs + two-file hunk footprint)
#   C2POS_SHA    claude/c2-nil-array-dims-positions    (C2: item-4 follow-up, assign/argument/result cargo; 21-line footprint committed at 312f5faf6e)
#   LTZ_SHA      claude/c2-localtimezone-exclusive     (C2: LocalTimeZone marked windows + "cannot MEASURE" wording; stacked on F8)
#   C1BOARD_SHA  claude/c1-board-environblockwalk      (C1: board -- EnvironBlockWalk per-GOOS golden finding + the five-row Linux residual; docs only)
# Battery (converter class + reflect-bridge-adjacent): converter suite + full CNR; slnx; GolibTests + alone; reflect build; sweeps: math/bits,
# math/big, crypto/rsa, crypto/x509, crypto/tls, archive/tar, encoding/json, encoding/xml, go/types, unicode/utf8, sort, strings; nistec COST PAIR in both
# directions (coord-nistec-pair.ps1 vs the train-12 head as control) -- the cut is a beneficiary as well as the canary; reflect RUN.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
WORDSIZE_SHA="${WORDSIZE_SHA:-7be0ddeae}"; C2POS_SHA="${C2POS_SHA:-0673fcf64c}"; C1BOARD_SHA="${C1BOARD_SHA:-91510a837}"; C1EXEC_SHA="${C1EXEC_SHA:-4deaeeee6}"; C1ROOTS_SHA="${C1ROOTS_SHA:-1fb37f437}"; C1KEY_SHA="${C1KEY_SHA:-32e7ba27e}"; SENDTO_SHA="${SENDTO_SHA:-f1c95f0d61}"; C2KEY_SHA="${C2KEY_SHA:-17d11832fd}"; C1HF_SHA="${C1HF_SHA:-a243ebc1d}"; C1MINT_SHA="${C1MINT_SHA:-d8218c42a}"; ROSTER_SHA="${ROSTER_SHA:-67e163e3c}"; C2LAYOUT_SHA="${C2LAYOUT_SHA:-8a7b668b75}"; DEDUP_SHA="${DEDUP_SHA:-f38c2ae01}"; LTZ_SHA="${LTZ_SHA:-f89e522e17}"; SENDMSGN_SHA="${SENDMSGN_SHA:-cc38f0082e}"
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train12-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 12 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
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
    elif [ -n "$unmerged" ] && ! echo "$unmerged" | tr ' ' '
' | grep -v '^$' | grep -vqE '^src/tests/Behavioral/BehavioralTests/(Compile|OutputComparison|TargetComparison|Transpile)Tests\.cs$'; then
      git checkout --ours -- $unmerged && git add $unmerged && git commit -S -q -F "$2" || { stamp "$3 test-class resolution FAILED"; git merge --abort 2>/dev/null; exit 1; }
      NEED_UTT=1; stamp "merged $3 (four test classes taken from ours; UTT re-derives them after the seats) -> $(git rev-parse --short HEAD)"
    else stamp "$3 merge CONFLICTS: $unmerged -- ABORT for hand resolution"; git merge --abort 2>/dev/null; exit 1; fi
  fi
}
seat() { # branch sha msg label
  [ -n "$2" ] || { stamp "$4: SHA unset -- seat SKIPPED"; return; }
  git fetch -q origin "$1" || { stamp "fetch $1 failed -- ABORT"; exit 1; }
  [ "$(git rev-parse origin/$1)" = "$(git rev-parse $2 2>/dev/null)" ] || stamp "NOTE: $1 tip is not $2 (announced) -- merging the remote tip; fix the message file"
  merge_one "origin/$1" "$3" "$4"
}
seat claude/coord-dedup-cross-assembly "$DEDUP_SHA" "$SP/coord-merge-dedup-fix.txt" "COORD dedup cross-assembly fix"
seat claude/g-mathbits-wordsize "$WORDSIZE_SHA" "$SP/coord-merge-g-mathbits-wordsize.txt" "G math/bits word-size"
seat claude/c2-nil-array-dims-positions "$C2POS_SHA" "$SP/coord-merge-c2-nil-array-positions.txt" "C2 nil-array dims positions"
seat claude/c2-localtimezone-exclusive "$LTZ_SHA" "$SP/coord-merge-c2-localtimezone.txt" "C2 LocalTimeZone marker"
seat claude/c2-sendmsgn-zero-n "$SENDMSGN_SHA" "$SP/coord-merge-c2-sendmsgn.txt" "C2 SendmsgN zero-n + guard phase 2"
seat claude/c1-syscall-exec-hooks "$C1EXEC_SHA" "$SP/coord-merge-c1-exec-hooks.txt" "C1 Exec marshalling + BeforeExec bodies"
seat claude/c1-board-environblockwalk "$C1BOARD_SHA" "$SP/coord-merge-c1-board-environ.txt" "C1 board (EnvironBlockWalk + Linux residual)"
seat claude/c1-board-syscall-roots "$C1ROOTS_SHA" "$SP/coord-merge-c1-board-roots.txt" "C1 board (syscall roots)"
seat claude/c1-design-cgocaller "$C1KEY_SHA" "$SP/coord-merge-c1-design-cgocaller.txt" "C1 DESIGN cgocaller keystone (sizing)"
seat claude/c2-keystone-darwin "$C2KEY_SHA" "$SP/coord-merge-c2-keystone-darwin.txt" "C2 DESIGN cgocaller keystone (darwin s3)"
seat claude/c2-sendtoseam-marker "$SENDTO_SHA" "$SP/coord-merge-c2-sendtoseam.txt" "C2 SendtoSeam marker"
seat claude/c1-host-fatal "$C1HF_SHA" "$SP/coord-merge-c1-host-fatal.txt" "C1 exclusion arc increment 2 (host-fatal, runtime/debug 4+6)"
seat claude/c1-syscall-mint14 "$C1MINT_SHA" "$SP/coord-merge-c1-mint14.txt" "C1 syscall 14th mint (TestExecPtrace host-limit)"
seat claude/c2-board-managed-struct-layout "$C2LAYOUT_SHA" "$SP/coord-merge-c2-board-layout.txt" "C2 board (managed struct layout)"
seat claude/i9-roster-guard-testconfig "$ROSTER_SHA" "$SP/coord-merge-roster-guard.txt" "i9 roster guard release-tc0 expectation"
# Re-derive the four behavioral test classes with the (now ordinal) UpdateTestTargets -- a pure function of the project set,
# so hand-inserted +3 lines from R/C2 and i9's reorder reconcile mechanically. No --createTargetFiles (that would re-baseline goldens).
NEED_UTT="${NEED_UTT:-1}"
if [ "$NEED_UTT" = "1" ]; then
  (cd src/utilities/UpdateTestTargets && dotnet build -c Debug -p:UseSharedCompilation=false -clp:ErrorsOnly >/dev/null 2>&1) || { stamp "UpdateTestTargets build FAILED -- ABORT"; exit 1; }
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1) || { stamp "UpdateTestTargets run FAILED -- ABORT"; exit 1; }
  changed=$(git status --porcelain | wc -l); other=$(git status --porcelain | grep -vE 'BehavioralTests/(Compile|OutputComparison|TargetComparison|Transpile)Tests\.cs$' | wc -l)
  [ "$other" = "0" ] || { stamp "UTT touched files outside the four classes: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
  if [ "$changed" != "0" ]; then git add src/tests/Behavioral/BehavioralTests/*Tests.cs && git commit -S -q -m "tests: re-derive the four behavioral test classes (ordinal UpdateTestTargets) after the train-12 seats

The seats carry hand-inserted +3 registrations (R's ReflectChanNarrowing) beside i9's ordinal reorder of the same blocks; the utility is a pure function of the project set, so one run reconciles them. numstat: $(git diff --cached --numstat | tr '
' ' ')

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" && stamp "UTT re-derived the four classes -> $(git rev-parse --short HEAD)"; else stamp "UTT: four classes already canonical (0 0)"; fi
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1); [ "$(git status --porcelain | wc -l)" = "0" ] && stamp "UTT idempotency: 0 0" || { stamp "UTT NOT idempotent: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
fi
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B" || { stamp "converter build FAILED -- ABORT"; exit 1; }
stamp "TRAIN 12 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"

stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train12-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train12-suite-cnr-$ts.log"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train12-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train12-syscall-linux-$ts.log"
stamp "BATTERY: slnx (the behavioral COMPILE gate)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train12-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train12-slnx-$ts.log"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train12-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train12-golibtests-$ts.log"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train12-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train12-alone-$ts.log"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train12-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train12-reflect-$ts.log"
stamp "BATTERY: filtered behavioral run (UdpWriteMsgAddrPort, SendtoSeam, TypedNilPtrArrayDims)"
(cd src/tests/Behavioral && for f in TypedNilPtrArrayDims TypedNilPtrArrayPositions; do powershell -NoProfile -ExecutionPolicy Bypass -File ./run-behavioral.ps1 --filter $f; echo "runner $f exit=$?"; done) > "$SP/coord-train12-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
stamp "BATTERY: sweeps"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps errors,math/bits,math/big,crypto/rsa,crypto/x509,crypto/tls,archive/tar,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings -SweepTimeout 30m > "$SP/coord-train12-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train12-sweeps-$ts.log"
stamp "BATTERY: nistec COST PAIR vs the train-11 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train12-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train12-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train12-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label coord-train11 > "$SP/coord-train12-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train12-reflectrun-$ts.log"
stamp "TRAIN 12 CHAIN DONE"
