#!/usr/bin/env bash
# coord-train29-assemble.sh -- merge train 26 on top of the train-25 landed master (set CONTROL_SHA to it), then run its battery. Seats (remote tips; an unset SHA skips):
#   SUBQ32 _SHA claude/sub-q32                      (SUB-Q32: the os want-zero ladder at I1 on SUB-Q5's converged instrument -- 744.25 -> 744.25, count 10 -> 8, the eight survivors segmented, docs only (SHA at the post))
#   RINC2  _SHA claude/reflect-cargo-inc-2          (R: the ChanElemDims Printf restoration + descriptor-cargo increment 2 (4.2 unexported method qualification) and 2b (the per-level direction chain) as sized, golib + converter (SHA at the post))
#   GB     _SHA claude/g-b-defer-finally            (G: capability 4 remedy B -- finally-lowering of qualifying deferred receiver calls, six gates, guard DeferFinallyLowering, 170 sites (SHA at the post))
# Battery: converter suite + full CNR (CHANGED set stamped by name); syscall-linux; slnx; GolibTests + alone; reflect build; FULL behavioral; sweeps; nistec pair; reflect RUN.
# Every leg stamps `LEG <name> exit=<code> :: <verdict>` into THIS log (the Monitor tails it); per-leg logs keep the detail. Pre-resolved conflicts apply from coord-t29-resolutions/<branch>/<path>.
set -uo pipefail
cd /c/Projects/go2cs/.claude/worktrees/coord-t25-dryrun || exit 2
[ "$(git rev-parse --show-toplevel)" = "C:/Projects/go2cs/.claude/worktrees/coord-t25-dryrun" ] || { echo "WRONG WORKTREE: $(pwd)"; exit 2; }
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
C2Q44_SHA="${C2Q44_SHA:-}"; C2Q44_BRANCH="${C2Q44_BRANCH:-claude/c2-q44-cut}"   # C2 Q44: the managed pointer token for reference-bearing boxes + the syncTimer di
C2Q49_SHA="${C2Q49_SHA:-d5645ab97}"; C2Q49_BRANCH="${C2Q49_BRANCH:-claude/c2-q49-cut}"   # C2 Q49: the KeepAlive predicate widened -- the bridged-wrapper arm + the darwin 
GFVC_SHA="${GFVC_SHA:-a5d40fdfc}"; GFVC_BRANCH="${GFVC_BRANCH:-claude/g-field-view-cut}"   # G: the field-view cache CUT (arm 3, SlottedStandardBox + per-T weak table + the 
GA_SHA="${GA_SHA:-955e271c0}"; GA_BRANCH="${GA_BRANCH:-claude/g-elem-take-concrete}"   # G candidate A: the concrete-header element-take overloads (golib; -1 object per 
SUBQ57_SHA="${SUBQ57_SHA:-588a01aaa}"; SUBQ57_BRANCH="${SUBQ57_BRANCH:-claude/sub-q57}"   # SUB-Q57 named-array empty literal constructs its elements (layer A)
C1RT6_SHA="${C1RT6_SHA:-d8cecc3ce}"; C1RT6_BRANCH="${C1RT6_BRANCH:-claude/c1-runtime-inc6-mem}"   # C1 increment 6 = the memory family: sysMmap/sysMunmap/madvise/usleep over libc (
C1RT7_SHA="${C1RT7_SHA:-846c36e1e}"; C1RT7_BRANCH="${C1RT7_BRANCH:-claude/c1-runtime-inc7-w2a}"   # C1 increment 7: W2a (addrRanges writers displaced over OverNativeMemory(base,len
RE3B_SHA="${RE3B_SHA:-6a7ea30be}"; RE3B_BRANCH="${RE3B_BRANCH:-claude/reflect-value-singles-inc-e3}"   # R E3 roots 4-5 + the order-token amendment (ccf4776b8) + Convert 7b/7c (3eff1e1c
C2Q56D_SHA="${C2Q56D_SHA:-8c1d2d506}"; C2Q56D_BRANCH="${C2Q56D_BRANCH:-claude/c2-q56-design}"   # C2 Q56 DESIGN: the whole cgo_unsafe_args parameter block lifted for darwin libcC
C2Q52D_SHA="${C2Q52D_SHA:-15968370d}"; C2Q52D_BRANCH="${C2Q52D_BRANCH:-claude/c2-q52-design}"   # C2 Q52 DESIGN: the os/signal posix bridge darwin flavour under its distinct base
C2INC7_SHA="${C2INC7_SHA:-48291283b}"; C2INC7_BRANCH="${C2INC7_BRANCH:-claude/c2-darwin-inc7}"   # C2 darwin increment 7: libcCall returns the C result (20 readers) and a null box
C2INC8_SHA="${C2INC8_SHA:-261f3e1e4}"; C2INC8_BRANCH="${C2INC8_BRANCH:-claude/c2-darwin-inc8}"   # C2 darwin increment 8: the sockaddr TWIN over the BSD layout + the datagram comp
C2Q52B_SHA="${C2Q52B_SHA:-554620235}"; C2Q52B_BRANCH="${C2Q52B_BRANCH:-claude/c2-q52-bridge}"   # C2 Q52 BRIDGE: the os/signal posix bridge darwin flavour under its distinct base
SUBQ62_SHA="${SUBQ62_SHA:-d97193e1f}"; SUBQ62_BRANCH="${SUBQ62_BRANCH:-claude/sub-q62}"   # SUB-Q62: the registration ledger guard gains the FLAVOUR axis (census 290 x 3 = 
SUBDOC11_SHA="${SUBDOC11_SHA:-5b407a952}"; SUBDOC11_BRANCH="${SUBDOC11_BRANCH:-claude/sub-doc11}"   # SUB-DOC11: CLAUDE.md doctrine batch 11 (accumulator items 493-516), docs only
ts=$(date +%Y%m%d-%H%M%S)
# The label a non-PASS sweep row's PRESERVED comparison record carries (CLAUDE.md rule 4). DERIVED
# from this script's own name, never written out: a train script is made by copying the previous one,
# and a hand-written label survives that copy and mislabels the NEXT train's evidence.
TRAIN_LABEL="$(basename "$0" | sed -n 's/^coord-\(train[0-9][0-9]*\).*/\1/p')"; TRAIN_LABEL="${TRAIN_LABEL:-battery}"
log="$SP/coord-train29-rehearsal-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 29 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
if [ "${SKIP_ASSEMBLE:-0}" != "1" ]; then
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "HEAD is not on top of origin/master -- ABORT"; exit 1; }
merge_one() { # ref msg label
  if git merge --no-ff -S -q -F "$2" "$1"; then stamp "merged $3 -> $(git rev-parse --short HEAD)"; else
    unmerged=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
    RES="$SP/coord-t29-resolutions/$(echo "$1" | sed 's#^origin/##' | tr '/' '_')"
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
seat "$C2Q44_BRANCH" "$C2Q44_SHA" "$SP/coord-merge-c2-q44-cut.txt" "C2 Q44: the managed pointer token for reference-bearing boxes + the syncTimer displacement + the registry-growth arm + SUB-Q27 labels re-entry"
seat "$C2Q49_BRANCH" "$C2Q49_SHA" "$SP/coord-merge-c2-q49.txt" "C2 Q49: the KeepAlive predicate widened -- the bridged-wrapper arm + the darwin funnel arm (third package) + the census guard darwin arm + C1 arm-7 twin"
seat "$GFVC_BRANCH" "$GFVC_SHA" "$SP/coord-merge-g-field-view-cut.txt" "G: the field-view cache CUT (arm 3, SlottedStandardBox + per-T weak table + the seven-arm guard) -- branch name set at the announce"
seat "$GA_BRANCH" "$GA_SHA" "$SP/coord-merge-g-elem-take.txt" "G candidate A: the concrete-header element-take overloads (golib; -1 object per &s[i] on 520 sites; the os row 488.25/6 -> 376.25/4 MET)"
seat "$SUBQ57_BRANCH" "$SUBQ57_SHA" "$SP/coord-merge-sub-q57.txt" "SUB-Q57 named-array empty literal constructs its elements (layer A)"
seat "$C1RT6_BRANCH" "$C1RT6_SHA" "$SP/coord-merge-c1-rt6.txt" "C1 increment 6 = the memory family: sysMmap/sysMunmap/madvise/usleep over libc (linux), persistentalloc1/inPersistentAlloc displaced (W1 retired on every row)"
seat "$C1RT7_BRANCH" "$C1RT7_SHA" "$SP/coord-merge-c1-rt7.txt" "C1 increment 7: W2a (addrRanges writers displaced over OverNativeMemory(base,len,cap) + HeaderSliceBox) + linux bootstrap constants + the promoted-selection arm gate; SHA at the announce"
seat "$RE3B_BRANCH" "$RE3B_SHA" "$SP/coord-merge-r-e3b.txt" "R E3 roots 4-5 + the order-token amendment (ccf4776b8) + Convert 7b/7c (3eff1e1ca) -- the branch tip behind the train-28 seat 10eecadb9, filled at the announce"
seat "$C2Q56D_BRANCH" "$C2Q56D_SHA" "$SP/coord-merge-c2-q56-design.txt" "C2 Q56 DESIGN: the whole cgo_unsafe_args parameter block lifted for darwin libcCall (docs only; findings: shape (d) result discarded, the darwin sockaddr door)"
seat "$C2Q52D_BRANCH" "$C2Q52D_SHA" "$SP/coord-merge-c2-q52-design.txt" "C2 Q52 DESIGN: the os/signal posix bridge darwin flavour under its distinct basename (docs only)"
seat "$C2INC7_BRANCH" "$C2INC7_SHA" "$SP/coord-merge-c2-inc7.txt" "C2 darwin increment 7: libcCall returns the C result (20 readers) and a null box is the zero-argument trampoline; getpid guards both arms"
seat "$C2INC8_BRANCH" "$C2INC8_SHA" "$SP/coord-merge-c2-inc8.txt" "C2 darwin increment 8: the sockaddr TWIN over the BSD layout + the datagram companion; twelve names widened; the five net rows predicted to move to runtime: kevent failed"
seat "$C2Q52B_BRANCH" "$C2Q52B_SHA" "$SP/coord-merge-c2-q52-bridge.txt" "C2 Q52 BRIDGE: the os/signal posix bridge darwin flavour under its distinct basename; SignalPrimitives predicted exit 0 / stderr 0 / stdout 6 on both mac legs"
seat "$SUBQ62_BRANCH" "$SUBQ62_SHA" "$SP/coord-merge-sub-q62.txt" "SUB-Q62: the registration ledger guard gains the FLAVOUR axis (census 290 x 3 = 0 fires; guard-only)"
seat "$SUBDOC11_BRANCH" "$SUBDOC11_SHA" "$SP/coord-merge-sub-doc11.txt" "SUB-DOC11: CLAUDE.md doctrine batch 11 (accumulator items 493-516), docs only"
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
stamp "TRAIN 29 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
else
  stamp "SKIP_ASSEMBLE=1: seats not re-run; head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"; [ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
  (cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B"
fi

# Roster header guard-as-calculator: two C1 seats (poll-bank, keystone) and the bcache bank each move header lines that git folds
# without a conflict; the union's numbers are whatever the guard computes from the merged table, never either branch's. RED here stops
# the chain BEFORE the battery so the header is recomposed by hand from the guard's printed values and the battery relaunched.
powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$SP/coord-train29-roster-guard-$ts.log" 2>&1; rg=$?
# Keep-alive census guard (src/syscall-keepalive-census.ps1): its arm 2 is green only with the train-16 Windows pin-holders seat in the tree.
powershell -NoProfile -ExecutionPolicy Bypass -File src/syscall-keepalive-census.ps1 > "$SP/coord-train29-keepalive-guard-$ts.log" 2>&1; kg=$?
stamp "KEEPALIVE GUARD exit=$kg :: $(tail -c 300 "$SP/coord-train29-keepalive-guard-$ts.log" | tr -d '' | tail -2 | tr '
' ' ')"
stamp "ROSTER GUARD exit=$rg ($(grep -aciE 'FAIL|violation' "$SP/coord-train29-roster-guard-$ts.log") fail lines)"
if [ "$rg" != "0" ]; then stamp "ROSTER HEADER NEEDS RECOMPOSITION -- battery NOT started; fix the header from the guard's output, commit, then relaunch with SKIP_ASSEMBLE=1"; exit 2; fi
[ "${REHEARSAL:-0}" = "1" ] && { stamp "REHEARSAL DONE: merges + UTT + converter rebuild + guards in $(pwd); battery NOT started; head=$(git rev-parse --short=9 HEAD) clean=$(git status --porcelain | wc -l)"; exit 0; }
stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train29-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train29-suite-cnr-$ts.log"
stamp "LEG suite+cnr $(tail -1 "$SP/coord-train29-suite-cnr-$ts.log") :: $(grep -aE '^ok|LEG1 END|LEG2 END' "$SP/coord-train29-suite-cnr-$ts.log" | tr '\n' ' ') :: CHANGED: $(sed -n '/CHANGED converter output/,/^\[/p' "$SP/coord-train29-suite-cnr-$ts.log" | grep -aE '^ +[AM?] ' | tr '\n' ' ')"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train29-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train29-syscall-linux-$ts.log"
stamp "BATTERY: slnx (the behavioral COMPILE gate)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train29-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train29-slnx-$ts.log"
stamp "LEG slnx $(tail -1 "$SP/coord-train29-slnx-$ts.log") :: $(grep -aE 'SLNX BUILD END' "$SP/coord-train29-slnx-$ts.log" | tail -1)"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train29-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train29-golibtests-$ts.log"
stamp "LEG golibtests $(tail -1 "$SP/coord-train29-golibtests-$ts.log") :: $(grep -aE 'count-matched|Aborted|Failed:' "$SP/coord-train29-golibtests-$ts.log" | tail -2 | tr '\n' ' ')"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train29-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train29-alone-$ts.log"
stamp "LEG alone $(tail -1 "$SP/coord-train29-alone-$ts.log") :: $(grep -a 'EACH-CLASS-ALONE END' "$SP/coord-train29-alone-$ts.log" | tail -1)"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train29-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train29-reflect-$ts.log"
stamp "LEG reflect $(tail -1 "$SP/coord-train29-reflect-$ts.log") :: $(grep -aE 'exit=' "$SP/coord-train29-reflect-$ts.log" | tr '\n' ' ')"
KNOWN_RED=""  # Increment C landed with train 23; no known red is allowed by name on this train
stamp "BATTERY: FULL behavioral suite, all phases (known red allowed by name: ${KNOWN_RED:-none} -- ReflectArrayOf only while Increment C is NOT seated on this train) -- the reflect/golib behavioral guard (a byte-identical golib behavioral regression is invisible to CNR and to a filtered runner). KNOWN RED, allowed by NAME: ReflectArrayOf -- ROOTED by R (3850ac433) as the cargo arc's empty-container boundary, unfixable at runtime, fix = Increment C (dims carried on the slice header, +8 B/slice bar measured and stated), R cutting it after the seat verdicts for train 23; the leg FAILS this train on any OTHER failing project."
(cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -Command "\$ErrorActionPreference='Continue'; & './run-behavioral.ps1' --build-timeout 10800 --build-one-timeout 900"; echo "runner exit=$?") > "$SP/coord-train29-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
other_red=$(tr -d "\000" < "$SP/coord-train29-runner-$ts.log" | grep -aE "^\s{2}[A-Za-z0-9_]+ \[" | grep -av "${KNOWN_RED:-__none__}" | wc -l); stamp "FULL BEHAVIORAL: failing projects other than the known red ${KNOWN_RED:-none} = $other_red (must be 0)"
stamp "BATTERY: sweeps (a non-PASS row's comparison record is PRESERVED to $SP/coord-pkg-run-record-<pkg>-$TRAIN_LABEL-* BEFORE the leg's restore -- CLAUDE.md rule 4)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Label "$TRAIN_LABEL" -Sweeps net/http,errors,math/bits,math/big,crypto/rsa,crypto/x509,crypto/tls,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec,crypto/md5,crypto/internal/boring/bcache,internal/cpu,time,internal/abi,sync,crypto/internal/alias,slices -SweepTimeout 30m > "$SP/coord-train29-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train29-sweeps-$ts.log"
stamp "SWEEP EVIDENCE: $(grep -a 'RECORD PRESERVED' "$SP/coord-train29-sweeps-$ts.log" | wc -l) record(s) preserved :: $(grep -a 'kept: ' "$SP/coord-train29-sweeps-$ts.log" | sed 's/.*kept: //' | tr '\n' ' ')"
stamp "LEG sweeps :: $(grep -aE 'PASS|FAIL|NOT MEASURED' "$SP/coord-train29-sweeps-$ts.log" | grep -av 'RECORD PRESERVED' | cut -c1-100 | tail -24 | tr '
' ' ; ')"
stamp "BATTERY: nistec COST PAIR vs the train-24 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train29-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train29-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train29-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label "$TRAIN_LABEL" > "$SP/coord-train29-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train29-reflectrun-$ts.log"
stamp "LEG reflectrun $(tail -1 "$SP/coord-train29-reflectrun-$ts.log") :: $(grep -aiE 'moved|PASS|FAIL|matched' "$SP/coord-train29-reflectrun-$ts.log" | tail -3 | cut -c1-120 | tr '
' ' ; ')"
stamp "TRAIN 29 CHAIN DONE"
