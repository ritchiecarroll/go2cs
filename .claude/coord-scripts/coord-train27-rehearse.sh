#!/usr/bin/env bash
# coord-train27-assemble.sh -- merge train 26 on top of the train-25 landed master (set CONTROL_SHA to it), then run its battery. Seats (remote tips; an unset SHA skips):
#   SUBQ32 _SHA claude/sub-q32                      (SUB-Q32: the os want-zero ladder at I1 on SUB-Q5's converged instrument -- 744.25 -> 744.25, count 10 -> 8, the eight survivors segmented, docs only (SHA at the post))
#   RINC2  _SHA claude/reflect-cargo-inc-2          (R: the ChanElemDims Printf restoration + descriptor-cargo increment 2 (4.2 unexported method qualification) and 2b (the per-level direction chain) as sized, golib + converter (SHA at the post))
#   GB     _SHA claude/g-b-defer-finally            (G: capability 4 remedy B -- finally-lowering of qualifying deferred receiver calls, six gates, guard DeferFinallyLowering, 170 sites (SHA at the post))
# Battery: converter suite + full CNR (CHANGED set stamped by name); syscall-linux; slnx; GolibTests + alone; reflect build; FULL behavioral; sweeps; nistec pair; reflect RUN.
# Every leg stamps `LEG <name> exit=<code> :: <verdict>` into THIS log (the Monitor tails it); per-leg logs keep the detail. Pre-resolved conflicts apply from coord-t27-resolutions/<branch>/<path>.
set -uo pipefail
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
GB2_SHA="${GB2_SHA:-39624e080}"; GB2_BRANCH="${GB2_BRANCH:-claude/g-b2-widenings}"   # G B2: the two defer-finally widenings (conditional defers + receiver-method) wit
GQ48_SHA="${GQ48_SHA:-c5e552949}"; GQ48_BRANCH="${GQ48_BRANCH:-claude/g-q48-trace-header}"   # G Q48: one header for both runtime trace_impl.cs copies -- the L3 merge refusal 
C1RT3_SHA="${C1RT3_SHA:-c3279299b}"; C1RT3_BRANCH="${C1RT3_BRANCH:-claude/c1-runtime-inc3-sliceheader}"   # C1 RT3: runtime increment 3, option A slice half -- SliceHeaderBox adapter in Re
C2Q44_SHA="${C2Q44_SHA:-}"; C2Q44_BRANCH="${C2Q44_BRANCH:-claude/c2-q44-cut}"   # C2 Q44: the managed pointer token for reference-bearing boxes + the syncTimer di
C2Q49_SHA="${C2Q49_SHA:-}"; C2Q49_BRANCH="${C2Q49_BRANCH:-claude/c2-q49-cut}"   # C2 Q49: the KeepAlive predicate widened -- the bridged-wrapper arm + the darwin 
RD_SHA="${RD_SHA:-a7b3e4a6a}"; RD_BRANCH="${RD_BRANCH:-claude/reflect-cargo-inc-d}"   # R D: the channel-value cargo (direction chain + element dims) on the header, the
SUBQ43_SHA="${SUBQ43_SHA:-5b58d49ea}"; SUBQ43_BRANCH="${SUBQ43_BRANCH:-claude/sub-q43}"   # SUB-Q43: runtime/pprof host-killer gated by capability disclosure + the 180-row 
SUBDOC10_SHA="${SUBDOC10_SHA:-674982db9}"; SUBDOC10_BRANCH="${SUBDOC10_BRANCH:-claude/sub-doc10}"   # SUB-DOC10: DOCTRINE BATCH 10 (accumulator items 448-492) landed in CLAUDE.md, +3
SUBQ45_SHA="${SUBQ45_SHA:-49ccad282}"; SUBQ45_BRANCH="${SUBQ45_BRANCH:-claude/sub-q45}"   # SUB-Q45: DESIGN-runtime-pinner.md -- the Pinner over the CLR heap sized (pin cou
C2CEN25_SHA="${C2CEN25_SHA:-3752b7495}"; C2CEN25_BRANCH="${C2CEN25_BRANCH:-claude/c2-darwin-board-t25}"   # C2: the train-25 darwin census scored on both legs (increment 5 moved x64 from s
ts=$(date +%Y%m%d-%H%M%S)
# The label a non-PASS sweep row's PRESERVED comparison record carries (CLAUDE.md rule 4). DERIVED
# from this script's own name, never written out: a train script is made by copying the previous one,
# and a hand-written label survives that copy and mislabels the NEXT train's evidence.
TRAIN_LABEL="$(basename "$0" | sed -n 's/^coord-\(train[0-9][0-9]*\).*/\1/p')"; TRAIN_LABEL="${TRAIN_LABEL:-battery}"
log="$SP/coord-train27-rehearsal-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 27 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
if [ "${SKIP_ASSEMBLE:-0}" != "1" ]; then
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "HEAD is not on top of origin/master -- ABORT"; exit 1; }
merge_one() { # ref msg label
  if git merge --no-ff -S -q -F "$2" "$1"; then stamp "merged $3 -> $(git rev-parse --short HEAD)"; else
    unmerged=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
    RES="$SP/coord-t27-resolutions/$(echo "$1" | sed 's#^origin/##' | tr '/' '_')"
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
seat "$GB2_BRANCH" "$GB2_SHA" "$SP/coord-merge-g-b2.txt" "G B2: the two defer-finally widenings (conditional defers + receiver-method) with the 35-file footprint, nss.cs pair applied after Q35; os row 744.25/8 -> 552.25/7"
seat "$GQ48_BRANCH" "$GQ48_SHA" "$SP/coord-merge-g-q48.txt" "G Q48: one header for both runtime trace_impl.cs copies -- the L3 merge refusal at master cleared, comment lines only"
seat "$C1RT3_BRANCH" "$C1RT3_SHA" "$SP/coord-merge-c1-rt3.txt" "C1 RT3: runtime increment 3, option A slice half -- SliceHeaderBox adapter in Reinterpret, golib only"
seat "$C2Q44_BRANCH" "$C2Q44_SHA" "$SP/coord-merge-c2-q44-cut.txt" "C2 Q44: the managed pointer token for reference-bearing boxes + the syncTimer displacement + the registry-growth arm"
seat "$C2Q49_BRANCH" "$C2Q49_SHA" "$SP/coord-merge-c2-q49.txt" "C2 Q49: the KeepAlive predicate widened -- the bridged-wrapper arm + the darwin funnel arm + the census guard darwin arm"
seat "$RD_BRANCH" "$RD_SHA" "$SP/coord-merge-r-inc-d.txt" "R D: the channel-value cargo (direction chain + element dims) on the header, the value route through the bridge, TestChanOf/TestTypes"
seat "$SUBQ43_BRANCH" "$SUBQ43_SHA" "$SP/coord-merge-sub-q43.txt" "SUB-Q43: runtime/pprof host-killer gated by capability disclosure + the 180-row census by door"
seat "$SUBDOC10_BRANCH" "$SUBDOC10_SHA" "$SP/coord-merge-sub-doc10.txt" "SUB-DOC10: DOCTRINE BATCH 10 (accumulator items 448-492) landed in CLAUDE.md, +345/-0 pure inserts, docs only"
seat "$SUBQ45_BRANCH" "$SUBQ45_SHA" "$SP/coord-merge-sub-q45.txt" "SUB-Q45: DESIGN-runtime-pinner.md -- the Pinner over the CLR heap sized (pin count by referent, +0 B, no token write), 19 rows classified, prediction 17/1/1; docs only"
seat "$C2CEN25_BRANCH" "$C2CEN25_SHA" "$SP/coord-merge-c2-census25.txt" "C2: the train-25 darwin census scored on both legs (increment 5 moved x64 from sigprocmask to setsig FuncPCABI0(sigtramp)); board +64, docs only"
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
stamp "TRAIN 27 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
else
  stamp "SKIP_ASSEMBLE=1: seats not re-run; head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"; [ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
  (cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B"
fi

# Roster header guard-as-calculator: two C1 seats (poll-bank, keystone) and the bcache bank each move header lines that git folds
# without a conflict; the union's numbers are whatever the guard computes from the merged table, never either branch's. RED here stops
# the chain BEFORE the battery so the header is recomposed by hand from the guard's printed values and the battery relaunched.
powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$SP/coord-train27-roster-guard-$ts.log" 2>&1; rg=$?
# Keep-alive census guard (src/syscall-keepalive-census.ps1): its arm 2 is green only with the train-16 Windows pin-holders seat in the tree.
powershell -NoProfile -ExecutionPolicy Bypass -File src/syscall-keepalive-census.ps1 > "$SP/coord-train27-keepalive-guard-$ts.log" 2>&1; kg=$?
stamp "KEEPALIVE GUARD exit=$kg :: $(tail -c 300 "$SP/coord-train27-keepalive-guard-$ts.log" | tr -d '' | tail -2 | tr '
' ' ')"
stamp "ROSTER GUARD exit=$rg ($(grep -aciE 'FAIL|violation' "$SP/coord-train27-roster-guard-$ts.log") fail lines)"
if [ "$rg" != "0" ]; then stamp "ROSTER HEADER NEEDS RECOMPOSITION -- battery NOT started; fix the header from the guard's output, commit, then relaunch with SKIP_ASSEMBLE=1"; exit 2; fi
[ "${REHEARSAL:-0}" = "1" ] && { stamp "REHEARSAL DONE: merges + UTT + converter rebuild + guards in $(pwd); battery NOT started; head=$(git rev-parse --short=9 HEAD) clean=$(git status --porcelain | wc -l)"; exit 0; }
stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train27-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train27-suite-cnr-$ts.log"
stamp "LEG suite+cnr $(tail -1 "$SP/coord-train27-suite-cnr-$ts.log") :: $(grep -aE '^ok|LEG1 END|LEG2 END' "$SP/coord-train27-suite-cnr-$ts.log" | tr '\n' ' ') :: CHANGED: $(sed -n '/CHANGED converter output/,/^\[/p' "$SP/coord-train27-suite-cnr-$ts.log" | grep -aE '^ +[AM?] ' | tr '\n' ' ')"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train27-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train27-syscall-linux-$ts.log"
stamp "BATTERY: slnx (the behavioral COMPILE gate)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train27-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train27-slnx-$ts.log"
stamp "LEG slnx $(tail -1 "$SP/coord-train27-slnx-$ts.log") :: $(grep -aE 'SLNX BUILD END' "$SP/coord-train27-slnx-$ts.log" | tail -1)"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train27-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train27-golibtests-$ts.log"
stamp "LEG golibtests $(tail -1 "$SP/coord-train27-golibtests-$ts.log") :: $(grep -aE 'count-matched|Aborted|Failed:' "$SP/coord-train27-golibtests-$ts.log" | tail -2 | tr '\n' ' ')"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train27-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train27-alone-$ts.log"
stamp "LEG alone $(tail -1 "$SP/coord-train27-alone-$ts.log") :: $(grep -a 'EACH-CLASS-ALONE END' "$SP/coord-train27-alone-$ts.log" | tail -1)"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train27-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train27-reflect-$ts.log"
stamp "LEG reflect $(tail -1 "$SP/coord-train27-reflect-$ts.log") :: $(grep -aE 'exit=' "$SP/coord-train27-reflect-$ts.log" | tr '\n' ' ')"
KNOWN_RED=""  # Increment C landed with train 23; no known red is allowed by name on this train
stamp "BATTERY: FULL behavioral suite, all phases (known red allowed by name: ${KNOWN_RED:-none} -- ReflectArrayOf only while Increment C is NOT seated on this train) -- the reflect/golib behavioral guard (a byte-identical golib behavioral regression is invisible to CNR and to a filtered runner). KNOWN RED, allowed by NAME: ReflectArrayOf -- ROOTED by R (3850ac433) as the cargo arc's empty-container boundary, unfixable at runtime, fix = Increment C (dims carried on the slice header, +8 B/slice bar measured and stated), R cutting it after the seat verdicts for train 23; the leg FAILS this train on any OTHER failing project."
(cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -Command "\$ErrorActionPreference='Continue'; & './run-behavioral.ps1' --build-timeout 10800 --build-one-timeout 900"; echo "runner exit=$?") > "$SP/coord-train27-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
other_red=$(tr -d "\000" < "$SP/coord-train27-runner-$ts.log" | grep -aE "^\s{2}[A-Za-z0-9_]+ \[" | grep -av "${KNOWN_RED:-__none__}" | wc -l); stamp "FULL BEHAVIORAL: failing projects other than the known red ${KNOWN_RED:-none} = $other_red (must be 0)"
stamp "BATTERY: sweeps (a non-PASS row's comparison record is PRESERVED to $SP/coord-pkg-run-record-<pkg>-$TRAIN_LABEL-* BEFORE the leg's restore -- CLAUDE.md rule 4)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Label "$TRAIN_LABEL" -Sweeps net/http,errors,math/bits,math/big,crypto/rsa,crypto/x509,crypto/tls,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec,crypto/md5,crypto/internal/boring/bcache,internal/cpu,time,internal/abi,sync,crypto/internal/alias,slices -SweepTimeout 30m > "$SP/coord-train27-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train27-sweeps-$ts.log"
stamp "SWEEP EVIDENCE: $(grep -a 'RECORD PRESERVED' "$SP/coord-train27-sweeps-$ts.log" | wc -l) record(s) preserved :: $(grep -a 'kept: ' "$SP/coord-train27-sweeps-$ts.log" | sed 's/.*kept: //' | tr '\n' ' ')"
stamp "LEG sweeps :: $(grep -aE 'PASS|FAIL|NOT MEASURED' "$SP/coord-train27-sweeps-$ts.log" | grep -av 'RECORD PRESERVED' | cut -c1-100 | tail -24 | tr '
' ' ; ')"
stamp "BATTERY: nistec COST PAIR vs the train-24 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train27-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train27-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train27-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label "$TRAIN_LABEL" > "$SP/coord-train27-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train27-reflectrun-$ts.log"
stamp "LEG reflectrun $(tail -1 "$SP/coord-train27-reflectrun-$ts.log") :: $(grep -aiE 'moved|PASS|FAIL|matched' "$SP/coord-train27-reflectrun-$ts.log" | tail -3 | cut -c1-120 | tr '
' ' ; ')"
stamp "TRAIN 27 CHAIN DONE"
