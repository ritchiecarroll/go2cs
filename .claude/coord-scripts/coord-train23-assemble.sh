#!/usr/bin/env bash
# coord-train23-assemble.sh -- merge train 23 on top of the train-21 landed master (set CONTROL_SHA to it), then run its battery. Seats (remote tips; an unset SHA skips):
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
GREC8_SHA="${GREC8_SHA:-011abc8b4}"; C2PIN_SHA="${C2PIN_SHA:-f349b3499a}"; C2PIN_BRANCH="${C2PIN_BRANCH:-claude/c2-syscall-pin}"; RINCC_SHA="${RINCC_SHA:-268a6d4b2}"; RINCC_BRANCH="${RINCC_BRANCH:-claude/reflect-cargo-inc-c}"; GI3_SHA="${GI3_SHA:-6a7688c88}"; GI3_BRANCH="${GI3_BRANCH:-claude/g-i3-callsite-rule}"; GSEMA_SHA="${GSEMA_SHA:-ad0ed9a2a}"; GSEMA_BRANCH="${GSEMA_BRANCH:-claude/g-bprime-inline-gates}"; SUBQ14_SHA="${SUBQ14_SHA:-4b8e19ee6}"; SUBQ10_SHA="${SUBQ10_SHA:-5f1490009}"; SUBDOC8_SHA="${SUBDOC8_SHA:-86e16e9f5}"; SUBQ11_SHA="${SUBQ11_SHA:-bc5acdaf8}"; SUBQ1_SHA="${SUBQ1_SHA:-54c7ecb85}"; SUBQ2_SHA="${SUBQ2_SHA:-46c13d703}"; SUBQ9_SHA="${SUBQ9_SHA:-dc7667683}"; C1ZR_SHA="${C1ZR_SHA:-5fdd7ebeb}"; SUBQ20_SHA="${SUBQ20_SHA:-5d8d69744}"; SUBSEC_SHA="${SUBSEC_SHA:-60ba23404}"; SUBQ17_SHA="${SUBQ17_SHA:-667bf9c71}"; SUBQ18_SHA="${SUBQ18_SHA:-}"; SUBSECG_SHA="${SUBSECG_SHA:-fda427e4e}"; SUBQ26_SHA="${SUBQ26_SHA:-40619d109}"
ts=$(date +%Y%m%d-%H%M%S)
# The label a non-PASS sweep row's PRESERVED comparison record carries (CLAUDE.md rule 4). DERIVED
# from this script's own name, never written out: a train script is made by copying the previous one,
# and a hand-written label survives that copy and mislabels the NEXT train's evidence.
TRAIN_LABEL="$(basename "$0" | sed -n 's/^coord-\(train[0-9][0-9]*\).*/\1/p')"; TRAIN_LABEL="${TRAIN_LABEL:-battery}"
log="$SP/coord-train23-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 23 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
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
seat claude/g-record-i1-retired "$GREC8_SHA" "$SP/coord-merge-g-record-i1-retired.txt" "G: design record section 8 amendment -- I1 retired with its measurement, section 4 corrected 9/2 -> 6/5 (docs only, 011abc8b4 on d188e89ed)"
seat "$C2PIN_BRANCH" "$C2PIN_SHA" "$SP/coord-merge-c2-syscall-pin.txt" "C2: the syscall buffer-pin fix -- FromPinnedBox mint + the compiler-transcribed funnel KeepAlive predicate, [500,730] + 77 sites / 3 targets (SHA at the post)"
seat "$RINCC_BRANCH" "$RINCC_SHA" "$SP/coord-merge-r-increment-c.txt" "R: Increment C as the ZERO-BYTE side table (ConditionalWeakTable on the backing array, 130 creation sites, read before observation) + the 14-row guard (SHA at the post)"
seat "$GSEMA_BRANCH" "$GSEMA_SHA" "$SP/coord-merge-g-sema-inline-gate.txt" "G: design (b-prime) -- inline SemaphoreSlim gate fields rgate/wgate/cgate via displaced rwlock/rwunlock ref-receiver hand-owns, no table; dissolves the identity boundary (SHA at the post)"
seat "$GI3_BRANCH" "$GI3_SHA" "$SP/coord-merge-g-i3.txt" "G: I3 cross-package receiver aliasing on C0 contract -- 1 box / 64 B on os, first live exercise of refPrimaryHandOwns (SHA at the post)"
seat claude/sub-q14 "$SUBQ14_SHA" "$SP/coord-merge-sub-q14.txt" "SUB-Q14: the six GolibTests guards under Release+TC0 -- escaping self-control bodies, NoInlining on the named frames, the inlining-dependence recorded (SHA at the post)"
seat claude/sub-q10 "$SUBQ10_SHA" "$SP/coord-merge-sub-q10.txt" "SUB-Q10: BehavioralRunner + MSTest classify a best-effort conversion as NOT MEASURED, never PASS (SHA at the post)"
seat claude/sub-q11 "$SUBQ11_SHA" "$SP/coord-merge-sub-q11.txt" "SUB-Q11: golden re-baseline paths re-transpile unconditionally + refuse on best-effort; BehavioralPackages.cs shared; based on SUB-Q10 predicate (SHA at the post; seats AFTER sub-q10)"
seat claude/sub-q1 "$SUBQ1_SHA" "$SP/coord-merge-sub-q1.txt" "SUB-Q1: composite-literal elements (the owner chip) (SHA at the post)"
seat claude/sub-q2 "$SUBQ2_SHA" "$SP/coord-merge-sub-q2.txt" "SUB-Q2: Printf comma-in-parens MEASURED as no defect -- the 24-row PrintfFormatCommaParen guard + a dated design-record block (SHA at the post)"
seat claude/sub-q9 "$SUBQ9_SHA" "$SP/coord-merge-sub-q9.txt" "SUB-Q9: [GoArchExclusive(amd64)] for StdLibInternalAbi -- F8 one axis over, shared predicate + CNR twin, no converter change (SHA at the post)"
seat claude/c1-zero-readers "$C1ZR_SHA" "$SP/coord-merge-c1-zero-readers.txt" "C1: item (2) ladder -- TestFakeMapping gated runtime-capability, the key-pinning guard widened per package clause, memory rows honest, goroutine-counts finding routed (SHA at the post)"
seat claude/sub-q20 "$SUBQ20_SHA" "$SP/coord-merge-sub-q20.txt" "SUB-Q20: the os want-zero row floor 1,320.00 on the record -- board dated block, design amendment, the sampling rule beside the comparability law (docs only)"
seat claude/sub-sec "$SUBSEC_SHA" "$SP/coord-merge-sub-sec.txt" "SUB-SEC: tip-wide scrub of profile-root usernames reintroduced in tracked docs since the 09-01 scrub (SHA at the post)"
seat claude/sub-q17 "$SUBQ17_SHA" "$SP/coord-merge-sub-q17.txt" "SUB-Q17: BoringCacheRegistryTests -- the six-arm GolibTests guard for the banked bcache mechanism, the wiring arm paying for the file (SHA at the post)"
seat claude/sub-q18 "$SUBQ18_SHA" "$SP/coord-merge-sub-q18.txt" "SUB-Q18: the testing row admitted colocated -- production half suppressed, internal variant the S1 exclusion, bucket B ruled per kind, denominator 59/156 (SHA at the post)"
seat claude/sub-sec-guard "$SUBSECG_SHA" "$SP/coord-merge-sub-sec-guard.txt" "SUB-SEC: the standing identifier-census guard in the converter suite, allowlist and positive control inside it (SHA at the post)"
seat claude/sub-q26 "$SUBQ26_SHA" "$SP/coord-merge-sub-q26.txt" "SUB-Q26: clean-bin.ps1 refuses an unattended deletion instead of exiting 0 having deleted nothing -- -Force, truthful exit codes, both editions controlled"
seat claude/sub-doc8 "$SUBDOC8_SHA" "$SP/coord-merge-sub-doc8.txt" "SUB-DOC8: doctrine batch 8 (items 231..386+) landed into CLAUDE.md, docs only (SHA at the post)"
# Re-derive the four behavioral test classes with the (now ordinal) UpdateTestTargets -- a pure function of the project set,
# so hand-inserted +3 lines from R/C2 and i9's reorder reconcile mechanically. No --createTargetFiles (that would re-baseline goldens).
NEED_UTT="${NEED_UTT:-1}"
if [ "$NEED_UTT" = "1" ]; then
  (cd src/utilities/UpdateTestTargets && dotnet build -c Debug -p:UseSharedCompilation=false -clp:ErrorsOnly >/dev/null 2>&1) || { stamp "UpdateTestTargets build FAILED -- ABORT"; exit 1; }
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1) || { stamp "UpdateTestTargets run FAILED -- ABORT"; exit 1; }
  changed=$(git status --porcelain | wc -l); other=$(git status --porcelain | grep -vE 'BehavioralTests/(Compile|OutputComparison|TargetComparison|Transpile)Tests\.cs$' | wc -l)
  [ "$other" = "0" ] || { stamp "UTT touched files outside the four classes: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
  if [ "$changed" != "0" ]; then git add src/tests/Behavioral/BehavioralTests/*Tests.cs && git commit -S -q -m "tests: re-derive the four behavioral test classes (ordinal UpdateTestTargets) after the train-23 seats

The seats carry hand-inserted +3 registrations beside earlier ordinal reorders of the same blocks; the utility is a pure function of the project set, so one run reconciles them. numstat: $(git diff --cached --numstat | tr '
' ' ')

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" && stamp "UTT re-derived the four classes -> $(git rev-parse --short HEAD)"; else stamp "UTT: four classes already canonical (0 0)"; fi
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1); [ "$(git status --porcelain | wc -l)" = "0" ] && stamp "UTT idempotency: 0 0" || { stamp "UTT NOT idempotent: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
fi
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B" || { stamp "converter build FAILED -- ABORT"; exit 1; }
# C2 pin finding 3: the F8-skipped windows-native PointerOutParameter carries two old-form mints its Linux cutter could not regenerate;
# regenerate it under the MERGED converter on this Windows host so the union CNR reads byte-identical rather than reporting the project changed.
stamp "regen PointerOutParameter under the merged converter (C2 pin finding 3)"
(cd src/go2cs && go build -o bin/go2cs.exe . ) || { stamp "go build of the merged converter FAILED -- ABORT"; exit 1; }
(cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe --createTargetFiles --only PointerOutParameter >/dev/null 2>&1) || { stamp "UpdateTestTargets --only PointerOutParameter FAILED (refusal = a best-effort or failed transpile) -- ABORT"; exit 1; }
pop=$(git status --porcelain | wc -l); popother=$(git status --porcelain | grep -v 'src/tests/Behavioral/PointerOutParameter/' | wc -l)
[ "$popother" = "0" ] || { stamp "regen touched files outside PointerOutParameter -- ABORT: $(git status --porcelain | tr '
' ' ')"; exit 1; }
if [ "$pop" != "0" ]; then git add src/tests/Behavioral/PointerOutParameter && git commit -S -q -m "tests: regenerate PointerOutParameter under the merged converter -- the F8-skipped windows-native project C2's Linux-cut syscall buffer-pin footprint could not reach (finding 3), so its two old-form mints become the retaining door on the host that can measure it

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" && stamp "PointerOutParameter regenerated -> $(git rev-parse --short HEAD) ($(git show --stat --format= HEAD | tail -1))"; else stamp "PointerOutParameter byte-identical under the merged converter -- C2's finding 3 did NOT reproduce here (NOTE for the landing)"; fi
stamp "TRAIN 23 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
else
  stamp "SKIP_ASSEMBLE=1: seats not re-run; head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"; [ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
  (cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B"
fi

# Roster header guard-as-calculator: two C1 seats (poll-bank, keystone) and the bcache bank each move header lines that git folds
# without a conflict; the union's numbers are whatever the guard computes from the merged table, never either branch's. RED here stops
# the chain BEFORE the battery so the header is recomposed by hand from the guard's printed values and the battery relaunched.
powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$SP/coord-train23-roster-guard-$ts.log" 2>&1; rg=$?
# Keep-alive census guard (src/syscall-keepalive-census.ps1): its arm 2 is green only with the train-16 Windows pin-holders seat in the tree.
powershell -NoProfile -ExecutionPolicy Bypass -File src/syscall-keepalive-census.ps1 > "$SP/coord-train23-keepalive-guard-$ts.log" 2>&1; kg=$?
stamp "KEEPALIVE GUARD exit=$kg :: $(tail -c 300 "$SP/coord-train23-keepalive-guard-$ts.log" | tr -d '' | tail -2 | tr '
' ' ')"
stamp "ROSTER GUARD exit=$rg ($(grep -aciE 'FAIL|violation' "$SP/coord-train23-roster-guard-$ts.log") fail lines)"
if [ "$rg" != "0" ]; then stamp "ROSTER HEADER NEEDS RECOMPOSITION -- battery NOT started; fix the header from the guard's output, commit, then relaunch with SKIP_ASSEMBLE=1"; exit 2; fi
stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train23-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train23-suite-cnr-$ts.log"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train23-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train23-syscall-linux-$ts.log"
stamp "BATTERY: slnx (the behavioral COMPILE gate)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train23-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train23-slnx-$ts.log"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train23-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train23-golibtests-$ts.log"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train23-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train23-alone-$ts.log"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train23-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train23-reflect-$ts.log"
if [ -n "${RINCC_SHA:-}" ]; then KNOWN_RED=""; else KNOWN_RED="ReflectArrayOf"; fi
stamp "BATTERY: FULL behavioral suite, all phases (known red allowed by name: ${KNOWN_RED:-none} -- ReflectArrayOf only while Increment C is NOT seated on this train) -- the reflect/golib behavioral guard (a byte-identical golib behavioral regression is invisible to CNR and to a filtered runner). KNOWN RED, allowed by NAME: ReflectArrayOf -- ROOTED by R (3850ac433) as the cargo arc's empty-container boundary, unfixable at runtime, fix = Increment C (dims carried on the slice header, +8 B/slice bar measured and stated), R cutting it after the seat verdicts for train 23; the leg FAILS this train on any OTHER failing project."
(cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -Command "\$ErrorActionPreference='Continue'; & './run-behavioral.ps1' --build-timeout 10800 --build-one-timeout 900"; echo "runner exit=$?") > "$SP/coord-train23-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
other_red=$(tr -d "\000" < "$SP/coord-train23-runner-$ts.log" | grep -aE "^\s{2}[A-Za-z0-9_]+ \[" | grep -av "${KNOWN_RED:-__none__}" | wc -l); stamp "FULL BEHAVIORAL: failing projects other than the known red ${KNOWN_RED:-none} = $other_red (must be 0)"
stamp "BATTERY: sweeps (a non-PASS row's comparison record is PRESERVED to $SP/coord-pkg-run-record-<pkg>-$TRAIN_LABEL-* BEFORE the leg's restore -- CLAUDE.md rule 4)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Label "$TRAIN_LABEL" -Sweeps net/http,errors,math/bits,math/big,crypto/rsa,crypto/x509,crypto/tls,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec,crypto/md5,crypto/internal/boring/bcache,internal/cpu,time,internal/abi,sync,crypto/internal/alias,slices -SweepTimeout 30m > "$SP/coord-train23-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train23-sweeps-$ts.log"
stamp "SWEEP EVIDENCE: $(grep -a 'RECORD PRESERVED' "$SP/coord-train23-sweeps-$ts.log" | wc -l) record(s) preserved :: $(grep -a 'kept: ' "$SP/coord-train23-sweeps-$ts.log" | sed 's/.*kept: //' | tr '\n' ' ')"
stamp "BATTERY: nistec COST PAIR vs the train-22 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train23-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train23-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train23-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label "$TRAIN_LABEL" > "$SP/coord-train23-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train23-reflectrun-$ts.log"
stamp "TRAIN 23 CHAIN DONE"
