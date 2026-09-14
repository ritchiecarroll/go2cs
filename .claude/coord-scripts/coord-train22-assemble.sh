#!/usr/bin/env bash
# coord-train22-assemble.sh -- merge train 22 on top of the train-21 landed master (set CONTROL_SHA to it), then run its battery. Seats (remote tips; an unset SHA skips):
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
RR1_SHA="${RR1_SHA:-18d03f7f1}"; GC0_SHA="${GC0_SHA:-1065e8b39}"; GLINUX_SHA="${GLINUX_SHA:-a16df3995}"; C2DARWIN3_SHA="${C2DARWIN3_SHA:-787850c7b5}"; C2ROUTE_SHA="${C2ROUTE_SHA:-56a045e50b}"; C2XPKG3C_SHA="${C2XPKG3C_SHA:-ebc450fe69}"; GSYSCALLCLS_SHA="${GSYSCALLCLS_SHA:-875ac7c1e}"; GSYSCALLCLS_BRANCH="${GSYSCALLCLS_BRANCH:-claude/g-cgo-class-admission}"; C2PIN_SHA="${C2PIN_SHA:-}"; C2PIN_BRANCH="${C2PIN_BRANCH:-claude/c2-syscall-pin}"; C2STDERR_SHA="${C2STDERR_SHA:-ebf6148df5}"
ts=$(date +%Y%m%d-%H%M%S)
log="$SP/coord-train22-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 22 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
if [ "${SKIP_ASSEMBLE:-0}" != "1" ]; then
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
seat claude/reflect-cargo-r1-1 "$RR1_SHA" "$SP/coord-merge-r-cargo-r1.txt" "R descriptor-cargo R1 + R1.1 (rebased onto master 22d2bd9dc as 86fbb07bb; seat condition BROKEN {} on reflect)"
seat claude/g-c0-contract "$GC0_SHA" "$SP/coord-merge-g-c0-contract.txt" "G C0: the cross-package lowering contract, guard-only zero-reduction increment (a3b389b98 cut; SHA set at the seat post after the post-sweep gates)"
# COUPLED: the refresh carries os/signal on the cgo-configuration class, which fails by construction until the admission seat lands -> ONE train; seat the refresh only when the admission seat is present.
if [ -n "${GLINUX_SHA:-}" ] && [ -z "${GSYSCALLCLS_SHA:-}" ]; then stamp "GLINUX seat HELD: refresh requires the cgo-configuration admission seat on the same train (GSYSCALLCLS_SHA unset)"; GLINUX_SHA=""; fi
seat claude/g-linux-annotation-refresh "$GLINUX_SHA" "$SP/coord-merge-g-linux-annotations.txt" "G Linux annotation refresh under the cgo state-of-record ruling (438728de0 on d188e89ed; coupled to the cgo-configuration admission seat)"
seat claude/c2-darwin-stderr "$C2STDERR_SHA" "$SP/coord-merge-c2-darwin-stderr.txt" "C2 darwin full-stderr instrument: os-matrix behavioral-stderr stage (workflow only, 2e86101d70)"
seat claude/c2-darwin-netpoll "$C2DARWIN3_SHA" "$SP/coord-merge-c2-darwin-netpoll.txt" "C2 darwin run-layer increment 3: the darwin readiness poller (census read 9fcd14a2b)"
seat claude/c2-route-endian "$C2ROUTE_SHA" "$SP/coord-merge-c2-route-endian.txt" "C2 darwin increment 3b: route.init endian probe hand-owned darwin-first (rebased SHA at the post)"
seat claude/c2-xpkg-box-bind "$C2XPKG3C_SHA" "$SP/coord-merge-c2-xpkg-box-bind.txt" "C2 darwin increment 3c: the cross-package box-bind converter fix + guard (ebc450fe69 on d188e89ed)"
seat "$GSYSCALLCLS_BRANCH" "$GSYSCALLCLS_SHA" "$SP/coord-merge-g-cgo-configuration-class.txt" "G: matchTerminalStatuses admits the cgo-configuration disclosure class for the pass/skip shape + guard + TestUseCgroupFD entry (branch/SHA at the post; seat 22 if before assembly, else 23)"
seat "$C2PIN_BRANCH" "$C2PIN_SHA" "$SP/coord-merge-c2-syscall-pin.txt" "C2: the syscall buffer-pin fix -- Pointer minted through the retaining door + funnel KeepAlive for the two-step uintptr(<Pointer var>) form, 77 sites / 3 targets (branch/SHA at the post; seat 22 if before assembly, else 23)"
# Re-derive the four behavioral test classes with the (now ordinal) UpdateTestTargets -- a pure function of the project set,
# so hand-inserted +3 lines from R/C2 and i9's reorder reconcile mechanically. No --createTargetFiles (that would re-baseline goldens).
NEED_UTT="${NEED_UTT:-1}"
if [ "$NEED_UTT" = "1" ]; then
  (cd src/utilities/UpdateTestTargets && dotnet build -c Debug -p:UseSharedCompilation=false -clp:ErrorsOnly >/dev/null 2>&1) || { stamp "UpdateTestTargets build FAILED -- ABORT"; exit 1; }
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1) || { stamp "UpdateTestTargets run FAILED -- ABORT"; exit 1; }
  changed=$(git status --porcelain | wc -l); other=$(git status --porcelain | grep -vE 'BehavioralTests/(Compile|OutputComparison|TargetComparison|Transpile)Tests\.cs$' | wc -l)
  [ "$other" = "0" ] || { stamp "UTT touched files outside the four classes: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
  if [ "$changed" != "0" ]; then git add src/tests/Behavioral/BehavioralTests/*Tests.cs && git commit -S -q -m "tests: re-derive the four behavioral test classes (ordinal UpdateTestTargets) after the train-22 seats

The seats carry hand-inserted +3 registrations beside earlier ordinal reorders of the same blocks; the utility is a pure function of the project set, so one run reconciles them. numstat: $(git diff --cached --numstat | tr '
' ' ')

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" && stamp "UTT re-derived the four classes -> $(git rev-parse --short HEAD)"; else stamp "UTT: four classes already canonical (0 0)"; fi
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1); [ "$(git status --porcelain | wc -l)" = "0" ] && stamp "UTT idempotency: 0 0" || { stamp "UTT NOT idempotent: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
fi
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B" || { stamp "converter build FAILED -- ABORT"; exit 1; }
stamp "TRAIN 22 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
else
  stamp "SKIP_ASSEMBLE=1: seats not re-run; head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"; [ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
  (cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B"
fi

# Roster header guard-as-calculator: two C1 seats (poll-bank, keystone) and the bcache bank each move header lines that git folds
# without a conflict; the union's numbers are whatever the guard computes from the merged table, never either branch's. RED here stops
# the chain BEFORE the battery so the header is recomposed by hand from the guard's printed values and the battery relaunched.
powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$SP/coord-train22-roster-guard-$ts.log" 2>&1; rg=$?
# Keep-alive census guard (src/syscall-keepalive-census.ps1): its arm 2 is green only with the train-16 Windows pin-holders seat in the tree.
powershell -NoProfile -ExecutionPolicy Bypass -File src/syscall-keepalive-census.ps1 > "$SP/coord-train22-keepalive-guard-$ts.log" 2>&1; kg=$?
stamp "KEEPALIVE GUARD exit=$kg :: $(tail -c 300 "$SP/coord-train22-keepalive-guard-$ts.log" | tr -d '' | tail -2 | tr '
' ' ')"
stamp "ROSTER GUARD exit=$rg ($(grep -aciE 'FAIL|violation' "$SP/coord-train22-roster-guard-$ts.log") fail lines)"
if [ "$rg" != "0" ]; then stamp "ROSTER HEADER NEEDS RECOMPOSITION -- battery NOT started; fix the header from the guard's output, commit, then relaunch with SKIP_ASSEMBLE=1"; exit 2; fi
stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train22-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train22-suite-cnr-$ts.log"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train22-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train22-syscall-linux-$ts.log"
stamp "BATTERY: slnx (the behavioral COMPILE gate)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train22-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train22-slnx-$ts.log"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train22-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train22-golibtests-$ts.log"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train22-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train22-alone-$ts.log"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train22-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train22-reflect-$ts.log"
stamp "BATTERY: FULL behavioral suite, all phases -- the reflect/golib behavioral guard (a byte-identical golib behavioral regression is invisible to CNR and to a filtered runner). KNOWN RED, allowed by NAME: ReflectArrayOf -- ROOTED by R (3850ac433) as the cargo arc's empty-container boundary, unfixable at runtime, fix = Increment C (dims carried on the slice header, +8 B/slice bar measured and stated), R cutting it after the seat verdicts for train 23; the leg FAILS this train on any OTHER failing project."
(cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -Command "\$ErrorActionPreference='Continue'; & './run-behavioral.ps1' --build-timeout 10800 --build-one-timeout 900"; echo "runner exit=$?") > "$SP/coord-train22-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
other_red=$(tr -d "\000" < "$SP/coord-train22-runner-$ts.log" | grep -aE "^\s{2}[A-Za-z0-9_]+ \[" | grep -av "ReflectArrayOf" | wc -l); stamp "FULL BEHAVIORAL: failing projects other than the known red ReflectArrayOf = $other_red (must be 0)"
stamp "BATTERY: sweeps"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Sweeps net/http,errors,math/bits,math/big,crypto/rsa,crypto/x509,crypto/tls,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec,crypto/md5,crypto/internal/boring/bcache,internal/cpu,time,internal/abi,sync,crypto/internal/alias,slices -SweepTimeout 30m > "$SP/coord-train22-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train22-sweeps-$ts.log"
stamp "BATTERY: nistec COST PAIR vs the train-21 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train22-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train22-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train22-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label coord-train11 > "$SP/coord-train22-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train22-reflectrun-$ts.log"
stamp "TRAIN 22 CHAIN DONE"
