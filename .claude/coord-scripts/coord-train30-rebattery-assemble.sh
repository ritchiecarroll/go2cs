#!/usr/bin/env bash
# coord-train30-assemble.sh -- merge train 26 on top of the train-25 landed master (set CONTROL_SHA to it), then run its battery. Seats (remote tips; an unset SHA skips):
#   SUBQ32 _SHA claude/sub-q32                      (SUB-Q32: the os want-zero ladder at I1 on SUB-Q5's converged instrument -- 744.25 -> 744.25, count 10 -> 8, the eight survivors segmented, docs only (SHA at the post))
#   RINC2  _SHA claude/reflect-cargo-inc-2          (R: the ChanElemDims Printf restoration + descriptor-cargo increment 2 (4.2 unexported method qualification) and 2b (the per-level direction chain) as sized, golib + converter (SHA at the post))
#   GB     _SHA claude/g-b-defer-finally            (G: capability 4 remedy B -- finally-lowering of qualifying deferred receiver calls, six gates, guard DeferFinallyLowering, 170 sites (SHA at the post))
# Battery: converter suite + full CNR (CHANGED set stamped by name); syscall-linux; slnx; GolibTests + alone; reflect build; FULL behavioral; sweeps; nistec pair; reflect RUN.
# Every leg stamps `LEG <name> exit=<code> :: <verdict>` into THIS log (the Monitor tails it); per-leg logs keep the detail. Pre-resolved conflicts apply from coord-t30-resolutions/<branch>/<path>.
set -uo pipefail
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 2
[ "$(git rev-parse --show-toplevel)" = "C:/Projects/go2cs/.claude/worktrees/musing-moser-d4552c" ] || { echo "WRONG WORKTREE: $(pwd)"; exit 2; }
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
C2Q44_SHA="${C2Q44_SHA:-eed11b550}"; C2Q44_BRANCH="${C2Q44_BRANCH:-claude/c2-q44-cut}"   # C2 Q44: the managed pointer token for reference-bearing boxes (take 2: the void*
C2INC9_SHA="${C2INC9_SHA:-d185e28b8}"; C2INC9_BRANCH="${C2INC9_BRANCH:-claude/c2-darwin-inc9}"   # C2 darwin increment 9: the darwin bridge sigignore installs the kernel SIG_IGN +
C2Q56L_SHA="${C2Q56L_SHA:-0ac8a607c}"; C2Q56L_BRANCH="${C2Q56L_BRANCH:-claude/c2-q56-lift}"   # C2 Q56 LIFT: the //go:cgo_unsafe_args block lift (converter rule + fixture guard
C2INC10_SHA="${C2INC10_SHA:-4efd81cf5}"; C2INC10_BRANCH="${C2INC10_BRANCH:-claude/c2-darwin-inc10}"   # C2 darwin INCREMENT 10 (a): the five keystone pull-linknames bodied in internal/
SUBQ63_SHA="${SUBQ63_SHA:-66a73ab03}"; SUBQ63_BRANCH="${SUBQ63_BRANCH:-claude/sub-q63}"   # SUB-Q63: the unique row Blocker A -- the companion type parameter at reflect.Typ
SUBQ60_SHA="${SUBQ60_SHA:-16d1943ac}"; SUBQ60_BRANCH="${SUBQ60_BRANCH:-claude/sub-q60}"   # SUB-Q60: the named-array wrapper zero value with a needy element (layer B; carri
SUBQ59_SHA="${SUBQ59_SHA:-1dd5bf492}"; SUBQ59_BRANCH="${SUBQ59_BRANCH:-claude/sub-q59}"   # SUB-Q59: the -tests comparison record carries the host stderr tail (additive fie
GBD_SHA="${GBD_SHA:-58e83c419}"; GBD_BRANCH="${GBD_BRANCH:-claude/g-design-b-outparam}"   # G DESIGN B: the syscall out-parameter (ref-lowering + fixed over the ref through
GED_SHA="${GED_SHA:-b4337813a}"; GED_BRANCH="${GED_BRANCH:-claude/g-design-e-elemaddr}"   # G DESIGN E: the syscall buffer element address (fixed-scope from the caller side
GCD_SHA="${GCD_SHA:-6db8d95a2}"; GCD_BRANCH="${GCD_BRANCH:-claude/g-design-c-strwindow}"   # G DESIGN C: the string byte-window (the element box between two already-zero-cop
GFVCR_SHA="${GFVCR_SHA:-2f43ef7b3}"; GFVCR_BRANCH="${GFVCR_BRANCH:-claude/g-fvc-record-measured}"   # G: the field-view-cache record section 9, the cut MEASURED at the train-29 landi
RE2B_SHA="${RE2B_SHA:-ca74dd433}"; RE2B_BRANCH="${RE2B_BRANCH:-claude/reflect-embedded-inc-e2b}"   # R E2b + 7e-b + 7g: the embedded-BUILTIN [GoEmbedded] marker, the [GoChanDir] and
C1Q61_SHA="${C1Q61_SHA:-e33e14ccf}"; C1Q61_BRANCH="${C1Q61_BRANCH:-claude/c1-runtime-q61-parkhook}"   # C1 Q61: the park hook -- golib ParkTransition slot at the outermost park boundar
C1Q64_SHA="${C1Q64_SHA:-7ab3d6fa6}"; C1Q64_BRANCH="${C1Q64_BRANCH:-claude/c1-runtime-q64-sigign}"   # C1 Q64: Ignore becomes the KERNEL disposition in the linux signal bridge for the
C1Q58D_SHA="${C1Q58D_SHA:-44fba8cf6}"; C1Q58D_BRANCH="${C1Q58D_BRANCH:-claude/c1-q58-design}"   # C1 Q58 DESIGN: the native-backed array pointer (a virtual consultation inside ar
GDC_SHA="${GDC_SHA:-67eba534f}"; GDC_BRANCH="${GDC_BRANCH:-claude/g-deferred-class}"   # G: the DEFERRED / STRUCTURAL disclosure classes in the schema and the roster gua
ts=$(date +%Y%m%d-%H%M%S)
# The label a non-PASS sweep row's PRESERVED comparison record carries (CLAUDE.md rule 4). DERIVED
# from this script's own name, never written out: a train script is made by copying the previous one,
# and a hand-written label survives that copy and mislabels the NEXT train's evidence.
TRAIN_LABEL="$(basename "$0" | sed -n 's/^coord-\(train[0-9][0-9]*\).*/\1/p')"; TRAIN_LABEL="${TRAIN_LABEL:-battery}"
log="$SP/coord-train30-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 30 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
if [ "${SKIP_ASSEMBLE:-0}" != "1" ]; then
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "HEAD is not on top of origin/master -- ABORT"; exit 1; }
merge_one() { # ref msg label
  if git merge --no-ff -S -q -F "$2" "$1"; then stamp "merged $3 -> $(git rev-parse --short HEAD)"; else
    unmerged=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
    RES="$SP/coord-t30-resolutions/$(echo "$1" | sed 's#^origin/##' | tr '/' '_')"
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
seat "$C2Q44_BRANCH" "$C2Q44_SHA" "$SP/coord-merge-c2-q44-cut.txt" "C2 Q44: the managed pointer token for reference-bearing boxes (take 2: the void* in-operator line; the F13 row demoted to compile-shape) -- SHA at the announce"
seat "$C2INC9_BRANCH" "$C2INC9_SHA" "$SP/coord-merge-c2-inc9.txt" "C2 darwin increment 9: the darwin bridge sigignore installs the kernel SIG_IGN + the job-control trio mapped (acceptance OWED to increment 10)"
seat "$C2Q56L_BRANCH" "$C2Q56L_SHA" "$SP/coord-merge-c2-q56-lift.txt" "C2 Q56 LIFT: the //go:cgo_unsafe_args block lift (converter rule + fixture guard, golib forms, darwin table; 27 lifted bodies in sys_darwin.cs; darwin census predicted to move 0 rows)"
seat "$C2INC10_BRANCH" "$C2INC10_SHA" "$SP/coord-merge-c2-inc10.txt" "C2 darwin INCREMENT 10 (a): the five keystone pull-linknames bodied in internal/syscall/unix darwin companion, two public doors in the keystone hand-own, the SyscallKeystonePulls guard row; on increment 9"
seat "$SUBQ63_BRANCH" "$SUBQ63_SHA" "$SP/coord-merge-sub-q63.txt" "SUB-Q63: the unique row Blocker A -- the companion type parameter at reflect.TypeFor[T] use sites (16 -> 19 predicted)"
seat "$SUBQ60_BRANCH" "$SUBQ60_SHA" "$SP/coord-merge-sub-q60.txt" "SUB-Q60: the named-array wrapper zero value with a needy element (layer B; carrier sized first)"
seat "$SUBQ59_BRANCH" "$SUBQ59_SHA" "$SP/coord-merge-sub-q59.txt" "SUB-Q59: the -tests comparison record carries the host stderr tail (additive field)"
seat "$GBD_BRANCH" "$GBD_SHA" "$SP/coord-merge-g-design-b.txt" "G DESIGN B: the syscall out-parameter (ref-lowering + fixed over the ref through the funnel; population 56/25/34; os row 376.25/4 -> 184.25/2 predicted) -- docs only"
seat "$GED_BRANCH" "$GED_SHA" "$SP/coord-merge-g-design-e.txt" "G DESIGN E: the syscall buffer element address (fixed-scope from the caller side; population 41/26/28 + 4/20/14; os row 184.25/2 -> 64.25/1 predicted) -- docs only"
seat "$GCD_BRANCH" "$GCD_SHA" "$SP/coord-merge-g-design-c.txt" "G DESIGN C: the string byte-window (the element box between two already-zero-copy halves; converter recognition emitting s.Slice(0,len(s)); population 2 production sites; os row 64.25/1 -> 0.25/0 predicted) -- docs only"
seat "$GFVCR_BRANCH" "$GFVCR_SHA" "$SP/coord-merge-g-fvc-record.txt" "G: the field-view-cache record section 9, the cut MEASURED at the train-29 landing (552.25/7 -> 488.25/6 -> 376.25/4 by A); docs only, off the landed master"
seat "$RE2B_BRANCH" "$RE2B_SHA" "$SP/coord-merge-r-e2b.txt" "R E2b + 7e-b + 7g: the embedded-BUILTIN [GoEmbedded] marker, the [GoChanDir] and widened [GoArrayDims] sibling attributes (TestConvert GREEN, 66 -> 65) + the reflectlite test-companion fix; stacked on RE3B"
seat "$C1Q61_BRANCH" "$C1Q61_SHA" "$SP/coord-merge-c1-q61.txt" "C1 Q61: the park hook -- golib ParkTransition slot at the outermost park boundary, runtime installs gopark/ready halves (casgstatus _Grunning <-> _Gwaiting); TestMutexWaitTimeMetric PASS; stacked on C1RT7"
seat "$C1Q64_BRANCH" "$C1Q64_SHA" "$SP/coord-merge-c1-q64.txt" "C1 Q64: Ignore becomes the KERNEL disposition in the linux signal bridge for the CLR-free class (USR1/USR2 + TSTP/TTIN/TTOU); TestForeground pair PASSES under a real tty where master hangs; on 9c44a6d6a"
seat "$C1Q58D_BRANCH" "$C1Q58D_SHA" "$SP/coord-merge-c1-q58-design.txt" "C1 Q58 DESIGN: the native-backed array pointer (a virtual consultation inside arrayView answered by the native box with OverNativeMemory; NativeBox<array<T>> refuted as the crash itself; increment 8 = the W1 write + W2b read pair) -- docs only"
seat "$GDC_BRANCH" "$GDC_SHA" "$SP/coord-merge-g-deferred-class.txt" "G: the DEFERRED / STRUCTURAL disclosure classes in the schema and the roster guard (41 manifests, 190 entries), refused without their plan, both PowerShell editions; the owner-ratified ruling made mechanical"
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
stamp "TRAIN 30 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
else
  stamp "SKIP_ASSEMBLE=1: seats not re-run; head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"; [ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
  (cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B"
fi

# Roster header guard-as-calculator: two C1 seats (poll-bank, keystone) and the bcache bank each move header lines that git folds
# without a conflict; the union's numbers are whatever the guard computes from the merged table, never either branch's. RED here stops
# the chain BEFORE the battery so the header is recomposed by hand from the guard's printed values and the battery relaunched.
powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$SP/coord-train30-roster-guard-$ts.log" 2>&1; rg=$?
# Keep-alive census guard (src/syscall-keepalive-census.ps1): its arm 2 is green only with the train-16 Windows pin-holders seat in the tree.
powershell -NoProfile -ExecutionPolicy Bypass -File src/syscall-keepalive-census.ps1 > "$SP/coord-train30-keepalive-guard-$ts.log" 2>&1; kg=$?
stamp "KEEPALIVE GUARD exit=$kg :: $(tail -c 300 "$SP/coord-train30-keepalive-guard-$ts.log" | tr -d '' | tail -2 | tr '
' ' ')"
stamp "ROSTER GUARD exit=$rg ($(grep -aciE 'FAIL|violation' "$SP/coord-train30-roster-guard-$ts.log") fail lines)"
if [ "$rg" != "0" ]; then stamp "ROSTER HEADER NEEDS RECOMPOSITION -- battery NOT started; fix the header from the guard's output, commit, then relaunch with SKIP_ASSEMBLE=1"; exit 2; fi
stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train30-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train30-suite-cnr-$ts.log"
stamp "LEG suite+cnr $(tail -1 "$SP/coord-train30-suite-cnr-$ts.log") :: $(grep -aE '^ok|LEG1 END|LEG2 END' "$SP/coord-train30-suite-cnr-$ts.log" | tr '\n' ' ') :: CHANGED: $(sed -n '/CHANGED converter output/,/^\[/p' "$SP/coord-train30-suite-cnr-$ts.log" | grep -aE '^ +[AM?] ' | tr '\n' ' ')"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train30-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train30-syscall-linux-$ts.log"
stamp "LEG syscall-linux $(tail -1 "$SP/coord-train30-syscall-linux-$ts.log") :: $(grep -aE 'Error\(s\)|error (CS|MSB|NETSDK)[0-9]+' "$SP/coord-train30-syscall-linux-$ts.log" | tail -2 | tr '
' ' ')"
stamp "BATTERY: slnx (the behavioral COMPILE gate)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train30-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train30-slnx-$ts.log"
stamp "LEG slnx $(tail -1 "$SP/coord-train30-slnx-$ts.log") :: $(grep -aE 'SLNX BUILD END' "$SP/coord-train30-slnx-$ts.log" | tail -1)"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train30-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train30-golibtests-$ts.log"
stamp "LEG golibtests $(tail -1 "$SP/coord-train30-golibtests-$ts.log") :: $(grep -aE 'count-matched|Aborted|Failed:' "$SP/coord-train30-golibtests-$ts.log" | tail -2 | tr '\n' ' ')"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train30-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train30-alone-$ts.log"
stamp "LEG alone $(tail -1 "$SP/coord-train30-alone-$ts.log") :: $(grep -a 'EACH-CLASS-ALONE END' "$SP/coord-train30-alone-$ts.log" | tail -1)"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train30-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train30-reflect-$ts.log"
stamp "LEG reflect $(tail -1 "$SP/coord-train30-reflect-$ts.log") :: $(grep -aE 'exit=' "$SP/coord-train30-reflect-$ts.log" | tr '\n' ' ')"
stamp "BATTERY: internal/reflectlite -tests build (doctrine 542: a banked row with a *_impl_test.cs companion)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" -Package internal/reflectlite > "$SP/coord-train30-reflectlite-$ts.log" 2>&1; echo "reflectlite exit=$?" >> "$SP/coord-train30-reflectlite-$ts.log"
stamp "LEG reflectlite $(tail -1 "$SP/coord-train30-reflectlite-$ts.log") :: $(grep -aE 'exit=' "$SP/coord-train30-reflectlite-$ts.log" | tr '\n' ' ')"
KNOWN_RED=""  # Increment C landed with train 23; no known red is allowed by name on this train
stamp "BATTERY: FULL behavioral suite, all phases (known red allowed by name: ${KNOWN_RED:-none} -- ReflectArrayOf only while Increment C is NOT seated on this train) -- the reflect/golib behavioral guard (a byte-identical golib behavioral regression is invisible to CNR and to a filtered runner). KNOWN RED, allowed by NAME: ReflectArrayOf -- ROOTED by R (3850ac433) as the cargo arc's empty-container boundary, unfixable at runtime, fix = Increment C (dims carried on the slice header, +8 B/slice bar measured and stated), R cutting it after the seat verdicts for train 23; the leg FAILS this train on any OTHER failing project."
(cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -Command "\$ErrorActionPreference='Continue'; & './run-behavioral.ps1' --build-timeout 10800 --build-one-timeout 900"; echo "runner exit=$?") > "$SP/coord-train30-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
other_red=$(tr -d "\000" < "$SP/coord-train30-runner-$ts.log" | grep -aE "^\s{2}[A-Za-z0-9_]+ \[" | grep -av "${KNOWN_RED:-__none__}" | wc -l); stamp "FULL BEHAVIORAL: failing projects other than the known red ${KNOWN_RED:-none} = $other_red (must be 0)"
SUITE_RED=$(grep -a "FULL BEHAVIORAL: failing projects" "$log" | tail -1 | sed -n "s/.*= \([0-9][0-9]*\) (must be 0).*/\1/p"); SUITE_RED="${SUITE_RED:-unknown}"; stamp "SUITE RED COUNT=$SUITE_RED (doctrine 583: a non-zero count skips the solo pair and the reflect run after the sweeps)"
stamp "BATTERY: sweeps (a non-PASS row's comparison record is PRESERVED to $SP/coord-pkg-run-record-<pkg>-$TRAIN_LABEL-* BEFORE the leg's restore -- CLAUDE.md rule 4)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Label "$TRAIN_LABEL" -Sweeps net/http,internal/poll,net,crypto/x509,crypto/tls,encoding/json,errors,math/bits,math/big,crypto/rsa,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec,crypto/md5,crypto/internal/boring/bcache,internal/cpu,time,internal/abi,sync,crypto/internal/alias,slices,internal/reflectlite -SweepTimeout 30m > "$SP/coord-train30-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train30-sweeps-$ts.log"
if grep -aq "DISK PREFLIGHT" "$SP/coord-train30-sweeps-$ts.log"; then stamp "LEG sweeps UNMEASURED: the sweep refused on its disk preflight ($(grep -aoE "[0-9.]+ GB free" "$SP/coord-train30-sweeps-$ts.log" | head -1)) -- battery STOPPED before nistec/reflect; free space, then relaunch the three tail legs"; exit 3; fi
stamp "SWEEP EVIDENCE: $(grep -a 'RECORD PRESERVED' "$SP/coord-train30-sweeps-$ts.log" | wc -l) record(s) preserved :: $(grep -a 'kept: ' "$SP/coord-train30-sweeps-$ts.log" | sed 's/.*kept: //' | tr '\n' ' ')"
stamp "LEG sweeps :: $(grep -aE 'PASS|FAIL|NOT MEASURED' "$SP/coord-train30-sweeps-$ts.log" | grep -av 'RECORD PRESERVED' | cut -c1-100 | tail -24 | tr '
' ' ; ')"
if [ "${SUITE_RED:-0}" != "0" ]; then stamp "SUITE RED ($SUITE_RED): nistec pair + reflect run SKIPPED -- TRAIN 30 CHAIN DONE (RED)"; exit 3; fi
stamp "BATTERY: nistec COST PAIR vs the train-24 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train30-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train30-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train30-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label "$TRAIN_LABEL" > "$SP/coord-train30-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train30-reflectrun-$ts.log"
stamp "LEG reflectrun $(tail -1 "$SP/coord-train30-reflectrun-$ts.log") :: $(grep -aiE 'moved|PASS|FAIL|matched' "$SP/coord-train30-reflectrun-$ts.log" | tail -3 | cut -c1-120 | tr '
' ' ; ')"
stamp "TRAIN 30 CHAIN DONE"
