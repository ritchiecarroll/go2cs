#!/usr/bin/env bash
# coord-train26-assemble.sh -- merge train 26 on top of the train-25 landed master (set CONTROL_SHA to it), then run its battery. Seats (remote tips; an unset SHA skips):
#   SUBQ32 _SHA claude/sub-q32                      (SUB-Q32: the os want-zero ladder at I1 on SUB-Q5's converged instrument -- 744.25 -> 744.25, count 10 -> 8, the eight survivors segmented, docs only (SHA at the post))
#   RINC2  _SHA claude/reflect-cargo-inc-2          (R: the ChanElemDims Printf restoration + descriptor-cargo increment 2 (4.2 unexported method qualification) and 2b (the per-level direction chain) as sized, golib + converter (SHA at the post))
#   GB     _SHA claude/g-b-defer-finally            (G: capability 4 remedy B -- finally-lowering of qualifying deferred receiver calls, six gates, guard DeferFinallyLowering, 170 sites (SHA at the post))
# Battery: converter suite + full CNR (CHANGED set stamped by name); syscall-linux; slnx; GolibTests + alone; reflect build; FULL behavioral; sweeps; nistec pair; reflect RUN.
# Every leg stamps `LEG <name> exit=<code> :: <verdict>` into THIS log (the Monitor tails it); per-leg logs keep the detail. Pre-resolved conflicts apply from coord-t26-resolutions/<branch>/<path>.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
SUBQ32_SHA="${SUBQ32_SHA:-}"; SUBQ32_BRANCH="${SUBQ32_BRANCH:-claude/sub-q32}"; RINC2_SHA="${RINC2_SHA:-}"; RINC2_BRANCH="${RINC2_BRANCH:-claude/reflect-cargo-inc-2}"; GB_SHA="${GB_SHA:-}"; GB_BRANCH="${GB_BRANCH:-claude/g-b-defer-finally}"
C1Q34_SHA="${C1Q34_SHA:-}"   # C1 Q34 docs: the init-hook relocation regen-debt census (board block + PLAN-rebank-wave line); set when C1 announces
C2CENSUS_SHA="${C2CENSUS_SHA:-}"   # C2: the train-23 darwin census board block (docs); set C2CENSUS_BRANCH + SHA when C2 announces
C1Q31_SHA="${C1Q31_SHA:-}"   # C1 Q31: the fourth absorption arm (host-conditional-disclosure) -- shared .ps1 change, 5.1 parse owed at the rehearsal; set when C1 announces
SUBQ36_SHA="${SUBQ36_SHA:-}"   # SUB-Q36: net/http re-bank 1343 -> 1345; RE-POINT to the pin-deletion commit; set when final
SUBQ27_SHA="${SUBQ27_SHA:-}"   # SUB-Q27: the goroutine profile (golib API change -> slnx leg owed); set when the row runs are posted
C1RT1_SHA="${C1RT1_SHA:-}"   # C1: runtime Linux increment 1 -- the rtsigprocmask hand-own body + linux GolibTests guard (bodyless partial displaced by writing it); set when C1 announces
GQ35_SHA="${GQ35_SHA:-8c9ee1907}"   # G Q35: I1 + I3 linux/darwin footprint (per-GOOS hunks byte-identical to the emission, remainder routed) + board block + PLAN line; set when G announces
C2INC5_SHA="${C2INC5_SHA:-}"   # C2: darwin run-layer increment 5 -- runtime.sigprocmask over pthread_sigmask via the registry (converter + hand-own + linux contract tests + 2-file footprint); set when verified
SUBQ39_SHA="${SUBQ39_SHA:-278c10a9a}"
C1RT2_SHA="${C1RT2_SHA:-88fe8965b}"   # C1 runtime increment 2 first half: hash_impl.cs hand-own (memhash/32/64/strhash) + RuntimeHashFamilyTests (7 arms); stacked on inc 1 (train 25)   # SUB-Q39: the external-variant lift-dedup fix (productionLiftReuseReachable reads the test-project MODEL); set when verified
SUBQ29_SHA="${SUBQ29_SHA:-}"   # SUB-Q29: the test.v tri-state retirement via golib AdapterBinder; testing row 35+17 -> 37+15 (HEADER COLLIDES with SUBQ36: both write 27,774/167, the union is 27,776/165 -- the roster guard recomposes); set when verified
C2CRASH_SHA="${C2CRASH_SHA:-27d87f31b}"   # C2 Q41 instrument: darwin crash-report collection in os-matrix.yml (workflow only); train 26; set when seated
SUBQ42_SHA="${SUBQ42_SHA:-d0b43965f}"   # SUB-Q42: the pinned-box staleness witness as a gated GolibTests guard (RED under GO2CS_PIN_STALENESS_STRICT=1 by design); set at launch
SUBQ40_SHA="${SUBQ40_SHA:-b8e69dd61}"   # SUB-Q40: DESIGN-managed-getg.md (docs only)
C1BILL_SHA="${C1BILL_SHA:-a70e99c1a}"   # C1: the runtime Linux bill as a board block + tracker line (docs)
C2Q44D_SHA="${C2Q44D_SHA:-657bf8baa}"   # C2: DESIGN-managed-pointer-token.md (docs only)
C2MIR_SHA="${C2MIR_SHA:-6d3cca8ef}"   # C2: the struct-passing tracker block -- the twenty reference-bearing FromPinnedBox sites as the mirror arc worklist (docs)
ts=$(date +%Y%m%d-%H%M%S)
# The label a non-PASS sweep row's PRESERVED comparison record carries (CLAUDE.md rule 4). DERIVED
# from this script's own name, never written out: a train script is made by copying the previous one,
# and a hand-written label survives that copy and mislabels the NEXT train's evidence.
TRAIN_LABEL="$(basename "$0" | sed -n 's/^coord-\(train[0-9][0-9]*\).*/\1/p')"; TRAIN_LABEL="${TRAIN_LABEL:-battery}"
log="$SP/coord-train26-rehearsal-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 26 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
if [ "${SKIP_ASSEMBLE:-0}" != "1" ]; then
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "HEAD is not on top of origin/master -- ABORT"; exit 1; }
merge_one() { # ref msg label
  if git merge --no-ff -S -q -F "$2" "$1"; then stamp "merged $3 -> $(git rev-parse --short HEAD)"; else
    unmerged=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
    RES="$SP/coord-t26-resolutions/$(echo "$1" | sed 's#^origin/##' | tr '/' '_')"
    if [ -n "$unmerged" ] && [ -d "$RES" ] && { allres=1; for f in $unmerged; do [ -f "$RES/$f" ] || allres=0; done; [ "$allres" = "1" ]; }; then
      for f in $unmerged; do cp "$RES/$f" "$f"; grep -q '^<<<<<<< ' "$f" && { stamp "$3 resolution artifact for $f still carries markers -- ABORT"; git merge --abort 2>/dev/null; exit 1; }; git add "$f"; done
      git commit -S -q -F "$2" || { stamp "$3 merge commit FAILED after applying resolutions"; git merge --abort 2>/dev/null; exit 1; }
      stamp "merged $3 (PRE-RESOLVED from the dry run: $(echo $unmerged | tr '
' ' ')) -> $(git rev-parse --short HEAD)"
    elif [ "$unmerged" = "src/go2cs/manualTypeOperations.go " ]; then
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
seat "$SUBQ32_BRANCH" "$SUBQ32_SHA" "$SP/coord-merge-sub-q32.txt" "SUB-Q32: the os want-zero ladder at I1 on SUB-Q5's converged instrument -- 744.25 -> 744.25, count 10 -> 8, the eight survivors segmented, docs only (SHA at the post)"
seat "${C1Q34_BRANCH:-claude/c1-q34-census}" "$C1Q34_SHA" "$SP/coord-merge-c1-q34.txt" "C1 Q34: the syscall/linux six-line staleness rooted as the init-hook relocation arc 289cc6c33 regen debt -- 768 flat + 171 per-GOOS production carriers at 22237fcbc, ruled to the deliberate regen; census block + PLAN-rebank-wave line, docs only (SHA at the post)"
seat "${C2CENSUS_BRANCH:-claude/c2-darwin-board-t24}" "$C2CENSUS_SHA" "$SP/coord-merge-c2-census.txt" "C2: the train-23 darwin census read row by row on 22237fcbc and scored (8 of 9 held; arm64 668/15 vs x64 669/14), the dated board block beside train 22 (SHA at the post)"
seat "${C1Q31_BRANCH:-claude/c1-q31-host-condition}" "$C1Q31_SHA" "$SP/coord-merge-c1-q31.txt" "C1 Q31: the os/exec host-conditional-disclosure absorption arm -- _roster.ps1 rule + sweep reader + roster annotation, both editions (SHA at the post)"
seat "${SUBQ36_BRANCH:-claude/sub-q36}" "$SUBQ36_SHA" "$SP/coord-merge-sub-q36.txt" "SUB-Q36: net/http re-banked at 1345, the performance-margin disclosure retired, header by the guard (SHA at the post)"
seat "${SUBQ27_BRANCH:-claude/sub-q27}" "$SUBQ27_SHA" "$SP/coord-merge-sub-q27.txt" "SUB-Q27: the goroutine profile over the managed registry, golib + runtime hand-own (SHA at the post)"
seat "${C1RT1_BRANCH:-claude/c1-runtime-inc1-sigprocmask}" "$C1RT1_SHA" "$SP/coord-merge-c1-rt1.txt" "C1: runtime Linux increment 1 -- rtsigprocmask over rt_sigprocmask(2), the init door answered; linux-only guard (SHA at the post)"
seat "${GQ35_BRANCH:-claude/g-q35-i1-l3-footprint}" "$GQ35_SHA" "$SP/coord-merge-g-q35.txt" "G Q35: the I1 + I3 per-GOOS corpus footprint measured by the three-target two-seeded A/B and applied as hunks (SHA at the post)"
seat "${C2INC5_BRANCH:-claude/c2-darwin-sigprocmask}" "$C2INC5_SHA" "$SP/coord-merge-c2-inc5.txt" "C2: darwin increment 5 -- runtime.sigprocmask bodied over pthread_sigmask, displaced through the registry, the seam guard red-then-green (SHA at the post)"
seat "${SUBQ39_BRANCH:-claude/sub-q39}" "$SUBQ39_SHA" "$SP/coord-merge-sub-q39.txt" "SUB-Q39: the external test variant adopts a production lift when the test-project MODEL makes internals visible -- runtime hash_test.cs CS1503 on every target (SHA at the post)"
seat "${C1RT2_BRANCH:-claude/c1-runtime-inc2-hash}" "$C1RT2_SHA" "$SP/coord-merge-c1-rt2.txt" "C1 RT2: runtime increment 2 first half -- the flat hash-stub family bodied (hash_impl.cs hand-own) + 7-arm GolibTests guard; no converter change"
seat "${SUBQ29_BRANCH:-claude/sub-q29}" "$SUBQ29_SHA" "$SP/coord-merge-sub-q29.txt" "SUB-Q29: the test.v tri-state retires through golib AdapterBinder -- no assembly, no reference, no codegen; testing 37 + 15 (SHA at the post)"
seat "${C2CRASH_BRANCH:-claude/c2-darwin-crashreport}" "$C2CRASH_SHA" "$SP/coord-merge-c2-crashreport.txt" "C2 Q41: crash-report collection for the darwin diagnostic stage, the null reported (SHA at the post)"
seat "${SUBQ42_BRANCH:-claude/sub-q42}" "$SUBQ42_SHA" "$SP/coord-merge-sub-q42.txt" "SUB-Q42: the reference-bearing box address-staleness witness, RED 10 of 10 under its gate, the mechanism bisected (SHA at the post)"
seat "${SUBQ40_BRANCH:-claude/sub-q40}" "$SUBQ40_SHA" "$SP/coord-merge-sub-q40.txt" "SUB-Q40: DESIGN-managed-getg -- the reader census reconciled from two derivations, a g AND its m, every acceptance row a door that moves (SHA at the post)"
seat "${C1BILL_BRANCH:-claude/c1-runtime-bill-docs}" "$C1BILL_SHA" "$SP/coord-merge-c1-bill.txt" "C1: the runtime Linux 378-row bill behind position 57 by door, with the parallel-phase attribution corrected (SHA at the post)"
seat "${C2Q44D_BRANCH:-claude/c2-q44-design}" "$C2Q44D_SHA" "$SP/coord-merge-c2-q44-design.txt" "C2: the managed pointer TOKEN design for reference-bearing boxes -- one arm in three address-take paths, the falsifier populated by twenty syscall sites (SHA at the post)"
seat "${C2MIR_BRANCH:-claude/c2-q44-mirror-population}" "$C2MIR_SHA" "$SP/coord-merge-c2-mirror-population.txt" "C2: the explicit-layout mirror arc population -- twenty reference-bearing syscall sites by name and shape on the struct-passing tracker (SHA at the post)"
seat "$RINC2_BRANCH" "$RINC2_SHA" "$SP/coord-merge-r-inc2.txt" "R: the ChanElemDims Printf restoration + descriptor-cargo increment 2 (4.2 unexported method qualification) and 2b (the per-level direction chain) as sized, golib + converter (SHA at the post)"
seat "$GB_BRANCH" "$GB_SHA" "$SP/coord-merge-g-b-defer-finally.txt" "G: capability 4 remedy B -- finally-lowering of qualifying deferred receiver calls, six gates, guard DeferFinallyLowering, 170 sites (SHA at the post)"
# Re-derive the four behavioral test classes with the (now ordinal) UpdateTestTargets -- a pure function of the project set,
# so hand-inserted +3 lines from R/C2 and i9's reorder reconcile mechanically. No --createTargetFiles (that would re-baseline goldens).
NEED_UTT="${NEED_UTT:-1}"
if [ "$NEED_UTT" = "1" ]; then
  (cd src/utilities/UpdateTestTargets && dotnet build -c Debug -p:UseSharedCompilation=false -clp:ErrorsOnly >/dev/null 2>&1) || { stamp "UpdateTestTargets build FAILED -- ABORT"; exit 1; }
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1) || { stamp "UpdateTestTargets run FAILED -- ABORT"; exit 1; }
  changed=$(git status --porcelain | wc -l); other=$(git status --porcelain | grep -vE 'BehavioralTests/(Compile|OutputComparison|TargetComparison|Transpile)Tests\.cs$' | wc -l)
  [ "$other" = "0" ] || { stamp "UTT touched files outside the four classes: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
  if [ "$changed" != "0" ]; then git add src/tests/Behavioral/BehavioralTests/*Tests.cs && git commit -S -q -m "tests: re-derive the four behavioral test classes (ordinal UpdateTestTargets) after the train-25 seats

The seats carry hand-inserted +3 registrations beside earlier ordinal reorders of the same blocks; the utility is a pure function of the project set, so one run reconciles them. numstat: $(git diff --cached --numstat | tr '
' ' ')

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" && stamp "UTT re-derived the four classes -> $(git rev-parse --short HEAD)"; else stamp "UTT: four classes already canonical (0 0)"; fi
  (cd src/utilities/UpdateTestTargets/bin/Debug/net10.0 && ./UpdateTestTargets.exe >/dev/null 2>&1); [ "$(git status --porcelain | wc -l)" = "0" ] && stamp "UTT idempotency: 0 0" || { stamp "UTT NOT idempotent: $(git status --porcelain | tr '
' ' ') -- ABORT"; exit 1; }
fi
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B" || { stamp "converter build FAILED -- ABORT"; exit 1; }
stamp "TRAIN 26 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
else
  stamp "SKIP_ASSEMBLE=1: seats not re-run; head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"; [ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
  (cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B"
fi

# Roster header guard-as-calculator: two C1 seats (poll-bank, keystone) and the bcache bank each move header lines that git folds
# without a conflict; the union's numbers are whatever the guard computes from the merged table, never either branch's. RED here stops
# the chain BEFORE the battery so the header is recomposed by hand from the guard's printed values and the battery relaunched.
powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$SP/coord-train26-roster-guard-$ts.log" 2>&1; rg=$?
# Keep-alive census guard (src/syscall-keepalive-census.ps1): its arm 2 is green only with the train-16 Windows pin-holders seat in the tree.
powershell -NoProfile -ExecutionPolicy Bypass -File src/syscall-keepalive-census.ps1 > "$SP/coord-train26-keepalive-guard-$ts.log" 2>&1; kg=$?
stamp "KEEPALIVE GUARD exit=$kg :: $(tail -c 300 "$SP/coord-train26-keepalive-guard-$ts.log" | tr -d '' | tail -2 | tr '
' ' ')"
stamp "ROSTER GUARD exit=$rg ($(grep -aciE 'FAIL|violation' "$SP/coord-train26-roster-guard-$ts.log") fail lines)"
if [ "$rg" != "0" ]; then stamp "ROSTER HEADER NEEDS RECOMPOSITION -- battery NOT started; fix the header from the guard's output, commit, then relaunch with SKIP_ASSEMBLE=1"; exit 2; fi
[ "${REHEARSAL:-0}" = "1" ] && { stamp "REHEARSAL DONE: merges + UTT + converter rebuild + guards in $(pwd); battery NOT started; head=$(git rev-parse --short=9 HEAD) clean=$(git status --porcelain | wc -l)"; exit 0; }
stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train26-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train26-suite-cnr-$ts.log"
stamp "LEG suite+cnr $(tail -1 "$SP/coord-train26-suite-cnr-$ts.log") :: $(grep -aE '^ok|LEG1 END|LEG2 END' "$SP/coord-train26-suite-cnr-$ts.log" | tr '\n' ' ') :: CHANGED: $(sed -n '/CHANGED converter output/,/^\[/p' "$SP/coord-train26-suite-cnr-$ts.log" | grep -aE '^ +[AM?] ' | tr '\n' ' ')"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train26-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train26-syscall-linux-$ts.log"
stamp "BATTERY: slnx (the behavioral COMPILE gate)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train26-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train26-slnx-$ts.log"
stamp "LEG slnx $(tail -1 "$SP/coord-train26-slnx-$ts.log") :: $(grep -aE 'SLNX BUILD END' "$SP/coord-train26-slnx-$ts.log" | tail -1)"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train26-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train26-golibtests-$ts.log"
stamp "LEG golibtests $(tail -1 "$SP/coord-train26-golibtests-$ts.log") :: $(grep -aE 'count-matched|Aborted|Failed:' "$SP/coord-train26-golibtests-$ts.log" | tail -2 | tr '\n' ' ')"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train26-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train26-alone-$ts.log"
stamp "LEG alone $(tail -1 "$SP/coord-train26-alone-$ts.log") :: $(grep -a 'EACH-CLASS-ALONE END' "$SP/coord-train26-alone-$ts.log" | tail -1)"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train26-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train26-reflect-$ts.log"
stamp "LEG reflect $(tail -1 "$SP/coord-train26-reflect-$ts.log") :: $(grep -aE 'exit=' "$SP/coord-train26-reflect-$ts.log" | tr '\n' ' ')"
KNOWN_RED=""  # Increment C landed with train 23; no known red is allowed by name on this train
stamp "BATTERY: FULL behavioral suite, all phases (known red allowed by name: ${KNOWN_RED:-none} -- ReflectArrayOf only while Increment C is NOT seated on this train) -- the reflect/golib behavioral guard (a byte-identical golib behavioral regression is invisible to CNR and to a filtered runner). KNOWN RED, allowed by NAME: ReflectArrayOf -- ROOTED by R (3850ac433) as the cargo arc's empty-container boundary, unfixable at runtime, fix = Increment C (dims carried on the slice header, +8 B/slice bar measured and stated), R cutting it after the seat verdicts for train 23; the leg FAILS this train on any OTHER failing project."
(cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -Command "\$ErrorActionPreference='Continue'; & './run-behavioral.ps1' --build-timeout 10800 --build-one-timeout 900"; echo "runner exit=$?") > "$SP/coord-train26-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
other_red=$(tr -d "\000" < "$SP/coord-train26-runner-$ts.log" | grep -aE "^\s{2}[A-Za-z0-9_]+ \[" | grep -av "${KNOWN_RED:-__none__}" | wc -l); stamp "FULL BEHAVIORAL: failing projects other than the known red ${KNOWN_RED:-none} = $other_red (must be 0)"
stamp "BATTERY: sweeps (a non-PASS row's comparison record is PRESERVED to $SP/coord-pkg-run-record-<pkg>-$TRAIN_LABEL-* BEFORE the leg's restore -- CLAUDE.md rule 4)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Label "$TRAIN_LABEL" -Sweeps net/http,errors,math/bits,math/big,crypto/rsa,crypto/x509,crypto/tls,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec,crypto/md5,crypto/internal/boring/bcache,internal/cpu,time,internal/abi,sync,crypto/internal/alias,slices -SweepTimeout 30m > "$SP/coord-train26-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train26-sweeps-$ts.log"
stamp "SWEEP EVIDENCE: $(grep -a 'RECORD PRESERVED' "$SP/coord-train26-sweeps-$ts.log" | wc -l) record(s) preserved :: $(grep -a 'kept: ' "$SP/coord-train26-sweeps-$ts.log" | sed 's/.*kept: //' | tr '\n' ' ')"
stamp "LEG sweeps :: $(grep -aE 'PASS|FAIL|NOT MEASURED' "$SP/coord-train26-sweeps-$ts.log" | grep -av 'RECORD PRESERVED' | cut -c1-100 | tail -24 | tr '
' ' ; ')"
stamp "BATTERY: nistec COST PAIR vs the train-24 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train26-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train26-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train26-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label "$TRAIN_LABEL" > "$SP/coord-train26-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train26-reflectrun-$ts.log"
stamp "LEG reflectrun $(tail -1 "$SP/coord-train26-reflectrun-$ts.log") :: $(grep -aiE 'moved|PASS|FAIL|matched' "$SP/coord-train26-reflectrun-$ts.log" | tail -3 | cut -c1-120 | tr '
' ' ; ')"
stamp "TRAIN 26 CHAIN DONE"
