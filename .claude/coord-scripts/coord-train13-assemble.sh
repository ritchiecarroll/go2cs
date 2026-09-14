#!/usr/bin/env bash
# coord-train13-assemble.sh -- merge train 12 after train 11 is pushed, then run its battery. Seats (remote tips; an unset SHA skips):
#   WORDSIZE_SHA claude/g-mathbits-wordsize            (G: one-level word-size Mul/Add/Sub hand-own + AggressiveInlining on Add; RSA 68.5->22.8 ms; converter registry + bits_impl.cs + two-file hunk footprint)
#   C2POS_SHA    claude/c2-nil-array-dims-positions    (C2: item-4 follow-up, assign/argument/result cargo; 21-line footprint committed at 312f5faf6e)
#   LTZ_SHA      claude/c2-localtimezone-exclusive     (C2: LocalTimeZone marked windows + "cannot MEASURE" wording; stacked on F8)
#   C1BOARD_SHA  claude/c1-board-environblockwalk      (C1: board -- EnvironBlockWalk per-GOOS golden finding + the five-row Linux residual; docs only)
# Battery (converter class + reflect-bridge-adjacent): converter suite + full CNR; slnx; GolibTests + alone; reflect build; sweeps: math/bits,
# math/big, crypto/rsa, crypto/x509, crypto/tls, archive/tar, encoding/json, encoding/xml, go/types, unicode/utf8, sort, strings; nistec COST PAIR in both
# directions (coord-nistec-pair.ps1 vs the train-13 head as control) -- the cut is a beneficiary as well as the canary; reflect RUN.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
ASSIGN_SHA="${ASSIGN_SHA:-0b780440e}"; ALIAS_SHA="${ALIAS_SHA:-9629e4867c}"; GUNTYPED_SHA="${GUNTYPED_SHA:-94cfb6149}"; GINLINE_SHA="${GINLINE_SHA:-}"; GDUAL_SHA="${GDUAL_SHA:-910d3a7bd}"; C2KEY2_SHA="${C2KEY2_SHA:-2916aa2b80}"; C1DEFER_SHA="${C1DEFER_SHA:-c37f76c4c}"; CGOPIN_SHA="${CGOPIN_SHA:-ee532b9b15}"; RGETTYPE_SHA="${RGETTYPE_SHA:-3bb3f068c}"; I9CENSUS_SHA="${I9CENSUS_SHA:-0f8bb337f}"; SLICE_SHA="${SLICE_SHA:-09f576cbc0}"; RECON_SHA="${RECON_SHA:-3df136d93}"; ROSTERFIX_SHA="${ROSTERFIX_SHA:-75283c973}"; BACKLOG_SHA="${BACKLOG_SHA:-0c8543c13}"
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train13-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 13 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
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
    elif [ "$unmerged" = "docs/phase4/BOARD-next-validation-candidates.md " ]; then
      git merge --abort 2>/dev/null; python "$SP/coord-board-merge.py" "$1" "$2" || { stamp "$3 board append-append resolution FAILED -- ABORT"; git merge --abort 2>/dev/null; exit 1; }
      stamp "merged $3 (board append-append, both sides kept) -> $(git rev-parse --short HEAD)"
    else stamp "$3 merge CONFLICTS: $unmerged -- ABORT for hand resolution"; git merge --abort 2>/dev/null; exit 1; fi
  fi
}
seat() { # branch sha msg label
  [ -n "$2" ] || { stamp "$4: SHA unset -- seat SKIPPED"; return; }
  git fetch -q origin "$1" || { stamp "fetch $1 failed -- ABORT"; exit 1; }
  [ "$(git rev-parse origin/$1)" = "$(git rev-parse $2 2>/dev/null)" ] || stamp "NOTE: $1 tip is not $2 (announced) -- merging the remote tip; fix the message file"
  merge_one "origin/$1" "$3" "$4"
}
seat claude/reflect-tail-r-assignmode "$ASSIGN_SHA" "$SP/coord-merge-r-assignmode.txt" "R assignability mode at both arms"
seat claude/c2-alias-drift-check "$ALIAS_SHA" "$SP/coord-merge-c2-alias-drift.txt" "C2 alias-drift standing check"
seat claude/c2-keystone-darwin "$C2KEY2_SHA" "$SP/coord-merge-c2-keystone-census.txt" "C2 darwin keystone pointer call-site census (s3.4)"
seat claude/g-untyped-const-design "$GUNTYPED_SHA" "$SP/coord-merge-g-untyped-design.txt" "G DESIGN untyped-const emission"
seat claude/g-untypedint-inline "$GINLINE_SHA" "$SP/coord-merge-g-untypedint-inline.txt" "G UntypedInt operators inlineable (candidate 3)"
seat claude/g-dualembed-witness "$GDUAL_SHA" "$SP/coord-merge-g-dualembed.txt" "G dual-embed witness (net DNS cluster root)"
seat claude/c1-defer-multivalue "$C1DEFER_SHA" "$SP/coord-merge-c1-defer.txt" "C1 deferred multi-value discard (CS0839)"
seat claude/c2-syscall-cgo-pin "$CGOPIN_SHA" "$SP/coord-merge-c2-cgo-pin.txt" "C2 syscall joins cgoOffPackages (widened predicate)"
seat claude/reflect-tail-r-gettype "$RGETTYPE_SHA" "$SP/coord-merge-r-gettype.txt" "R atomic.Value type-word comparisons via the bridge hook"
seat claude/i9-release-tc0-census "$I9CENSUS_SHA" "$SP/coord-merge-i9-census.txt" "i9 CENSUS release-tc0 delta (201 rows)"
seat claude/c2-behavioral-slice "$SLICE_SHA" "$SP/coord-merge-c2-slice.txt" "C2 BehavioralRunner --slice + behavioral-full leg"
seat claude/sub-recon124-delta "$RECON_SHA" "$SP/coord-merge-sub-recon124.txt" "sub-agent 1.24 recon delta (docs)"
seat claude/sub-backlog-strike "$BACKLOG_SHA" "$SP/coord-merge-sub-backlog.txt" "sub-agent backlog strike + renumber (docs)"
seat claude/sub-roster-record-fixes "$ROSTERFIX_SHA" "$SP/coord-merge-sub-rosterfix.txt" "sub-agent roster/board record reconciliation (docs; board append-append)"
# Re-derive the four behavioral test classes with the (now ordinal) UpdateTestTargets -- a pure function of the project set,
# so hand-inserted +3 lines from R/C2 and i9's reorder reconcile mechanically. No --createTargetFiles (that would re-baseline goldens).
NEED_UTT="${NEED_UTT:-1}"
if [ "$NEED_UTT" = "1" ]; then
  (cd src/utilities/UpdateTestTargets && dotnet build -c Debug -p:UseSharedCompilation=false -clp:ErrorsOnly >/dev/null 2>&1) || { stamp "UpdateTestTargets build FAILED -- ABORT"; exit 1; }
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1) || { stamp "UpdateTestTargets run FAILED -- ABORT"; exit 1; }
  changed=$(git status --porcelain | wc -l); other=$(git status --porcelain | grep -vE 'BehavioralTests/(Compile|OutputComparison|TargetComparison|Transpile)Tests\.cs$' | wc -l)
  [ "$other" = "0" ] || { stamp "UTT touched files outside the four classes: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
  if [ "$changed" != "0" ]; then git add src/tests/Behavioral/BehavioralTests/*Tests.cs && git commit -S -q -m "tests: re-derive the four behavioral test classes (ordinal UpdateTestTargets) after the train-13 seats

The seats carry hand-inserted +3 registrations (R's ReflectChanNarrowing) beside i9's ordinal reorder of the same blocks; the utility is a pure function of the project set, so one run reconciles them. numstat: $(git diff --cached --numstat | tr '
' ' ')

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" && stamp "UTT re-derived the four classes -> $(git rev-parse --short HEAD)"; else stamp "UTT: four classes already canonical (0 0)"; fi
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1); [ "$(git status --porcelain | wc -l)" = "0" ] && stamp "UTT idempotency: 0 0" || { stamp "UTT NOT idempotent: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
fi
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B" || { stamp "converter build FAILED -- ABORT"; exit 1; }
stamp "TRAIN 13 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"

stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train13-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train13-suite-cnr-$ts.log"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train13-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train13-syscall-linux-$ts.log"
stamp "BATTERY: slnx (the behavioral COMPILE gate)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train13-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train13-slnx-$ts.log"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train13-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train13-golibtests-$ts.log"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train13-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train13-alone-$ts.log"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train13-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train13-reflect-$ts.log"
stamp "BATTERY: filtered behavioral run (UdpWriteMsgAddrPort, SendtoSeam, TypedNilPtrArrayDims)"
(cd src/tests/Behavioral && for f in TypedNilPtrArrayDims TypedNilPtrArrayPositions; do powershell -NoProfile -ExecutionPolicy Bypass -File ./run-behavioral.ps1 --filter $f; echo "runner $f exit=$?"; done) > "$SP/coord-train13-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
stamp "BATTERY: sweeps"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps errors,math/bits,math/big,crypto/rsa,crypto/x509,crypto/tls,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec -SweepTimeout 30m > "$SP/coord-train13-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train13-sweeps-$ts.log"
stamp "BATTERY: nistec COST PAIR vs the train-11 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train13-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train13-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train13-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label coord-train11 > "$SP/coord-train13-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train13-reflectrun-$ts.log"
stamp "TRAIN 13 CHAIN DONE"
