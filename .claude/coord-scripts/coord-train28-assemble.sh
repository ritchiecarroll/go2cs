#!/usr/bin/env bash
# coord-train28-assemble.sh -- merge train 26 on top of the train-25 landed master (set CONTROL_SHA to it), then run its battery. Seats (remote tips; an unset SHA skips):
#   SUBQ32 _SHA claude/sub-q32                      (SUB-Q32: the os want-zero ladder at I1 on SUB-Q5's converged instrument -- 744.25 -> 744.25, count 10 -> 8, the eight survivors segmented, docs only (SHA at the post))
#   RINC2  _SHA claude/reflect-cargo-inc-2          (R: the ChanElemDims Printf restoration + descriptor-cargo increment 2 (4.2 unexported method qualification) and 2b (the per-level direction chain) as sized, golib + converter (SHA at the post))
#   GB     _SHA claude/g-b-defer-finally            (G: capability 4 remedy B -- finally-lowering of qualifying deferred receiver calls, six gates, guard DeferFinallyLowering, 170 sites (SHA at the post))
# Battery: converter suite + full CNR (CHANGED set stamped by name); syscall-linux; slnx; GolibTests + alone; reflect build; FULL behavioral; sweeps; nistec pair; reflect RUN.
# Every leg stamps `LEG <name> exit=<code> :: <verdict>` into THIS log (the Monitor tails it); per-leg logs keep the detail. Pre-resolved conflicts apply from coord-t28-resolutions/<branch>/<path>.
set -uo pipefail
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 2
[ "$(git rev-parse --show-toplevel)" = "C:/Projects/go2cs/.claude/worktrees/musing-moser-d4552c" ] || { echo "WRONG WORKTREE: $(pwd)"; exit 2; }
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
export MSBUILDDISABLENODEREUSE=1
C2Q44_SHA="${C2Q44_SHA:-}"; C2Q44_BRANCH="${C2Q44_BRANCH:-claude/c2-q44-cut}"   # C2 Q44: the managed pointer token for reference-bearing boxes + the syncTimer di
C2Q49_SHA="${C2Q49_SHA:-}"; C2Q49_BRANCH="${C2Q49_BRANCH:-claude/c2-q49-cut}"   # C2 Q49: the KeepAlive predicate widened -- the bridged-wrapper arm + the darwin 
C1RT4_SHA="${C1RT4_SHA:-49ad67e32}"; C1RT4_BRANCH="${C1RT4_BRANCH:-claude/c1-runtime-inc4-getg}"   # C1 RT4: runtime increment 4, the managed getg (a g AND its m, ThreadStatic-cache
C1RT5_SHA="${C1RT5_SHA:-7f318ab29}"; C1RT5_BRANCH="${C1RT5_BRANCH:-claude/c1-runtime-inc5-q54}"   # C1 increment 5 = Q54 runtime lock abandoned on goroutine death
SUBQ45C_SHA="${SUBQ45C_SHA:-a38e638fe}"; SUBQ45C_BRANCH="${SUBQ45C_BRANCH:-claude/sub-q45}"   # SUB-Q45 cut: runtime.Pinner over the CLR heap -- pinner_impl.cs, 3 registry addi
SUBQ50_SHA="${SUBQ50_SHA:-e8fcb6703}"; SUBQ50_BRANCH="${SUBQ50_BRANCH:-claude/sub-q50}"   # SUB-Q50: golib unsafe.String aliases its source bytes where Go aliases (the 4-li
GFVC_SHA="${GFVC_SHA:-}"; GFVC_BRANCH="${GFVC_BRANCH:-claude/g-field-view-cut}"   # G: the field-view cache CUT (arm 3, SlottedStandardBox + per-T weak table + the 
RE2_SHA="${RE2_SHA:-f5df84f49}"; RE2_BRANCH="${RE2_BRANCH:-claude/reflect-field-metadata-inc-e2}"   # R E2: the reflect field-metadata cluster (Anonymous, equal-depth ambiguity, flag
C2Q41F_SHA="${C2Q41F_SHA:-eb2ffed3d}"; C2Q41F_BRANCH="${C2Q41F_BRANCH:-claude/c2-q41-frames}"   # C2 Q41: the darwin crash-report stage grows a frames-and-registers block (workfl
GFVD_SHA="${GFVD_SHA:-c4bc47917}"; GFVD_BRANCH="${GFVD_BRANCH:-claude/g-field-view-design}"   # G: DESIGN-field-view-cache.md -- the type-gated view slot (arm 3 of the seg-3 sp
C2INC6_SHA="${C2INC6_SHA:-cc16ab170}"; C2INC6_BRANCH="${C2INC6_BRANCH:-claude/c2-darwin-inc6}"   # C2: darwin run-layer increment 6 -- sigaction over a blittable mirror (encode ne
RE3_SHA="${RE3_SHA:-10eecadb9}"; RE3_BRANCH="${RE3_BRANCH:-claude/reflect-value-singles-inc-e3}"   # R E3: the reflect Value singles, one root per commit on one branch (root 1 SetCa
SUBQ22_SHA="${SUBQ22_SHA:-969cbaeae}"; SUBQ22_BRANCH="${SUBQ22_BRANCH:-claude/sub-q22}"   # SUB-Q22: the elided & composite element with a NAMED array/slice/map pointee rou
C1Q46_SHA="${C1Q46_SHA:-f99111123}"; C1Q46_BRANCH="${C1Q46_BRANCH:-claude/c1-q46-hostfatal}"   # C1 Q46: TestPanicSystemstack as a host-fatal HANG member of the runtime manifest
SUBQ55_SHA="${SUBQ55_SHA:-b0c9d1d8c}"; SUBQ55_BRANCH="${SUBQ55_BRANCH:-claude/sub-q55}"   # SUB-Q55: the -tests pipeline deadline kill takes the child PROCESS GROUP (unix S
SUBQ57_SHA="${SUBQ57_SHA:-}"; SUBQ57_BRANCH="${SUBQ57_BRANCH:-claude/sub-q57}"   # SUB-Q57 named-array empty literal constructs its elements (layer A)
ts=$(date +%Y%m%d-%H%M%S)
# The label a non-PASS sweep row's PRESERVED comparison record carries (CLAUDE.md rule 4). DERIVED
# from this script's own name, never written out: a train script is made by copying the previous one,
# and a hand-written label survives that copy and mislabels the NEXT train's evidence.
TRAIN_LABEL="$(basename "$0" | sed -n 's/^coord-\(train[0-9][0-9]*\).*/\1/p')"; TRAIN_LABEL="${TRAIN_LABEL:-battery}"
log="$SP/coord-train28-merge-$ts.log"
stamp() { echo "[$(date '+%F %T')] $*" | tee -a "$log"; }
stamp "TRAIN 28 ASSEMBLE START head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
if [ "${SKIP_ASSEMBLE:-0}" != "1" ]; then
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master || { stamp "fetch failed -- ABORT"; exit 1; }
git merge-base --is-ancestor origin/master HEAD || { stamp "HEAD is not on top of origin/master -- ABORT"; exit 1; }
merge_one() { # ref msg label
  if git merge --no-ff -S -q -F "$2" "$1"; then stamp "merged $3 -> $(git rev-parse --short HEAD)"; else
    unmerged=$(git diff --name-only --diff-filter=U | tr '\n' ' ')
    RES="$SP/coord-t28-resolutions/$(echo "$1" | sed 's#^origin/##' | tr '/' '_')"
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
seat "$C1RT4_BRANCH" "$C1RT4_SHA" "$SP/coord-merge-c1-rt4.txt" "C1 RT4: runtime increment 4, the managed getg (a g AND its m, ThreadStatic-cached) per DESIGN-managed-getg, stacked on RT3"
seat "$C1RT5_BRANCH" "$C1RT5_SHA" "$SP/coord-merge-c1-rt5.txt" "C1 increment 5 = Q54 runtime lock abandoned on goroutine death"
seat "$SUBQ45C_BRANCH" "$SUBQ45C_SHA" "$SP/coord-merge-sub-q45-cut.txt" "SUB-Q45 cut: runtime.Pinner over the CLR heap -- pinner_impl.cs, 3 registry additions with placeholders, manifest, Reference (second commit on the design branch)"
seat "$SUBQ50_BRANCH" "$SUBQ50_SHA" "$SP/coord-merge-sub-q50.txt" "SUB-Q50: golib unsafe.String aliases its source bytes where Go aliases (the 4-line arm) + guard"
seat "$GFVC_BRANCH" "$GFVC_SHA" "$SP/coord-merge-g-field-view-cut.txt" "G: the field-view cache CUT (arm 3, SlottedStandardBox + per-T weak table + the seven-arm guard) -- branch name set at the announce"
seat "$RE2_BRANCH" "$RE2_SHA" "$SP/coord-merge-r-e2.txt" "R E2: the reflect field-metadata cluster (Anonymous, equal-depth ambiguity, flagRO through unexported embedded) -- 3 rows"
seat "$C2Q41F_BRANCH" "$C2Q41F_SHA" "$SP/coord-merge-c2-q41-frames.txt" "C2 Q41: the darwin crash-report stage grows a frames-and-registers block (workflow only) -- the arm64 SIGBUS at 0x4200000000 placed by frames"
seat "$GFVD_BRANCH" "$GFVD_SHA" "$SP/coord-merge-g-field-view-design.txt" "G: DESIGN-field-view-cache.md -- the type-gated view slot (arm 3 of the seg-3 spike), the identity contract, the byte formula, the seven-arm guard; docs only"
seat "$C2INC6_BRANCH" "$C2INC6_SHA" "$SP/coord-merge-c2-inc6.txt" "C2: darwin run-layer increment 6 -- sigaction over a blittable mirror (encode new / decode old), registry entry, linux contract guard; the arm64 mute death cleared if the stale-register write was the cause"
seat "$RE3_BRANCH" "$RE3_SHA" "$SP/coord-merge-r-e3.txt" "R E3: the reflect Value singles, one root per commit on one branch (root 1 SetCap hand-own; root 2 Bytes; ...) -- the branch tip at assembly"
seat "$SUBQ22_BRANCH" "$SUBQ22_SHA" "$SP/coord-merge-sub-q22.txt" "SUB-Q22: the elided & composite element with a NAMED array/slice/map pointee routed through the typed renderer (CS0144 at master) + guard; SHA at the announce"
seat "$C1Q46_BRANCH" "$C1Q46_SHA" "$SP/coord-merge-c1-q46.txt" "C1 Q46: TestPanicSystemstack as a host-fatal HANG member of the runtime manifest + the board line (the init-started goroutine vs the type-initializer lock); no code"
seat "$SUBQ55_BRANCH" "$SUBQ55_SHA" "$SP/coord-merge-sub-q55.txt" "SUB-Q55: the -tests pipeline deadline kill takes the child PROCESS GROUP (unix Setpgid; Windows job object with kill-on-close), guard re-execs three deep with a heartbeat; converter-side only"
seat "$SUBQ57_BRANCH" "$SUBQ57_SHA" "$SP/coord-merge-sub-q57.txt" "SUB-Q57 named-array empty literal constructs its elements (layer A)"
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
stamp "TRAIN 28 ASSEMBLED head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"
else
  stamp "SKIP_ASSEMBLE=1: seats not re-run; head=$(git rev-parse --short HEAD) clean=$(git status --porcelain | wc -l)"; [ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
  (cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B"
fi

# Roster header guard-as-calculator: two C1 seats (poll-bank, keystone) and the bcache bank each move header lines that git folds
# without a conflict; the union's numbers are whatever the guard computes from the merged table, never either branch's. RED here stops
# the chain BEFORE the battery so the header is recomposed by hand from the guard's printed values and the battery relaunched.
powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1 > "$SP/coord-train28-roster-guard-$ts.log" 2>&1; rg=$?
# Keep-alive census guard (src/syscall-keepalive-census.ps1): its arm 2 is green only with the train-16 Windows pin-holders seat in the tree.
powershell -NoProfile -ExecutionPolicy Bypass -File src/syscall-keepalive-census.ps1 > "$SP/coord-train28-keepalive-guard-$ts.log" 2>&1; kg=$?
stamp "KEEPALIVE GUARD exit=$kg :: $(tail -c 300 "$SP/coord-train28-keepalive-guard-$ts.log" | tr -d '' | tail -2 | tr '
' ' ')"
stamp "ROSTER GUARD exit=$rg ($(grep -aciE 'FAIL|violation' "$SP/coord-train28-roster-guard-$ts.log") fail lines)"
if [ "$rg" != "0" ]; then stamp "ROSTER HEADER NEEDS RECOMPOSITION -- battery NOT started; fix the header from the guard's output, commit, then relaunch with SKIP_ASSEMBLE=1"; exit 2; fi
stamp "BATTERY: converter suite + full CNR"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" > "$SP/coord-train28-suite-cnr-$ts.log" 2>&1; echo "suite+cnr exit=$?" >> "$SP/coord-train28-suite-cnr-$ts.log"
stamp "LEG suite+cnr $(tail -1 "$SP/coord-train28-suite-cnr-$ts.log") :: $(grep -aE '^ok|LEG1 END|LEG2 END' "$SP/coord-train28-suite-cnr-$ts.log" | tr '\n' ' ') :: CHANGED: $(sed -n '/CHANGED converter output/,/^\[/p' "$SP/coord-train28-suite-cnr-$ts.log" | grep -aE '^ +[AM?] ' | tr '\n' ' ')"
git checkout -q HEAD -- src/tests/Behavioral 2>/dev/null; stamp "post-CNR restore: behavioral dirt reverted ($(git status --porcelain -- src/tests/Behavioral | wc -l) left)"
stamp "BATTERY: syscall.csproj on linux (obj purged first)"
powershell -NoProfile -ExecutionPolicy Bypass -Command "\$env:DOTNET_ROOT='$HOME\dotnet10'; \$env:PATH=\"\$env:DOTNET_ROOT;\$env:PATH\"; \$env:MSBUILDDISABLENODEREUSE='1'; Set-Location '$(pwd -W)/src/core/syscall'; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue; dotnet build syscall.csproj -c Debug -p:GoTargetOS=linux -p:UseSharedCompilation=false -clp:ErrorsOnly 2>&1 | Select-String -Pattern 'error (CS|MSB|NETSDK)[0-9]+|Build succeeded|error\(s\)' | Select-Object -First 10; Remove-Item -Recurse -Force bin,obj -ErrorAction SilentlyContinue" > "$SP/coord-train28-syscall-linux-$ts.log" 2>&1; echo "syscall-linux exit=$?" >> "$SP/coord-train28-syscall-linux-$ts.log"
stamp "LEG syscall-linux $(tail -1 "$SP/coord-train28-syscall-linux-$ts.log") :: $(grep -aE 'Error\(s\)|error (CS|MSB|NETSDK)[0-9]+' "$SP/coord-train28-syscall-linux-$ts.log" | tail -2 | tr '
' ' ')"
stamp "BATTERY: slnx (the behavioral COMPILE gate)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-slnx-build.ps1" > "$SP/coord-train28-slnx-$ts.log" 2>&1; echo "slnx exit=$?" >> "$SP/coord-train28-slnx-$ts.log"
stamp "LEG slnx $(tail -1 "$SP/coord-train28-slnx-$ts.log") :: $(grep -aE 'SLNX BUILD END' "$SP/coord-train28-slnx-$ts.log" | tail -1)"
stamp "BATTERY: GolibTests --no-build + each-class-alone"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests.ps1" -NoBuild > "$SP/coord-train28-golibtests-$ts.log" 2>&1; echo "golibtests exit=$?" >> "$SP/coord-train28-golibtests-$ts.log"
stamp "LEG golibtests $(tail -1 "$SP/coord-train28-golibtests-$ts.log") :: $(grep -aE 'count-matched|Aborted|Failed:' "$SP/coord-train28-golibtests-$ts.log" | tail -2 | tr '\n' ' ')"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-golibtests-alone.ps1" > "$SP/coord-train28-alone-$ts.log" 2>&1; echo "alone exit=$?" >> "$SP/coord-train28-alone-$ts.log"
stamp "LEG alone $(tail -1 "$SP/coord-train28-alone-$ts.log") :: $(grep -a 'EACH-CLASS-ALONE END' "$SP/coord-train28-alone-$ts.log" | tail -1)"
stamp "BATTERY: reflect -tests build"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" > "$SP/coord-train28-reflect-$ts.log" 2>&1; echo "reflect exit=$?" >> "$SP/coord-train28-reflect-$ts.log"
stamp "LEG reflect $(tail -1 "$SP/coord-train28-reflect-$ts.log") :: $(grep -aE 'exit=' "$SP/coord-train28-reflect-$ts.log" | tr '\n' ' ')"
KNOWN_RED=""  # Increment C landed with train 23; no known red is allowed by name on this train
stamp "BATTERY: FULL behavioral suite, all phases (known red allowed by name: ${KNOWN_RED:-none} -- ReflectArrayOf only while Increment C is NOT seated on this train) -- the reflect/golib behavioral guard (a byte-identical golib behavioral regression is invisible to CNR and to a filtered runner). KNOWN RED, allowed by NAME: ReflectArrayOf -- ROOTED by R (3850ac433) as the cargo arc's empty-container boundary, unfixable at runtime, fix = Increment C (dims carried on the slice header, +8 B/slice bar measured and stated), R cutting it after the seat verdicts for train 23; the leg FAILS this train on any OTHER failing project."
(cd src/tests/Behavioral && powershell -NoProfile -ExecutionPolicy Bypass -Command "\$ErrorActionPreference='Continue'; & './run-behavioral.ps1' --build-timeout 10800 --build-one-timeout 900"; echo "runner exit=$?") > "$SP/coord-train28-runner-$ts.log" 2>&1; git checkout HEAD -- src/tests/Behavioral 2>/dev/null
other_red=$(tr -d "\000" < "$SP/coord-train28-runner-$ts.log" | grep -aE "^\s{2}[A-Za-z0-9_]+ \[" | grep -av "${KNOWN_RED:-__none__}" | wc -l); stamp "FULL BEHAVIORAL: failing projects other than the known red ${KNOWN_RED:-none} = $other_red (must be 0)"
stamp "BATTERY: sweeps (a non-PASS row's comparison record is PRESERVED to $SP/coord-pkg-run-record-<pkg>-$TRAIN_LABEL-* BEFORE the leg's restore -- CLAUDE.md rule 4)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-union-battery.ps1" -SkipSuite -SkipCNR -Label "$TRAIN_LABEL" -Sweeps net/http,errors,math/bits,math/big,crypto/rsa,crypto/x509,crypto/tls,encoding/json,encoding/xml,go/types,unicode/utf8,sort,strings,os/exec,crypto/md5,crypto/internal/boring/bcache,internal/cpu,time,internal/abi,sync,crypto/internal/alias,slices -SweepTimeout 30m > "$SP/coord-train28-sweeps-$ts.log" 2>&1; echo "sweeps exit=$?" >> "$SP/coord-train28-sweeps-$ts.log"
if grep -aq "DISK PREFLIGHT" "$SP/coord-train28-sweeps-$ts.log"; then stamp "LEG sweeps UNMEASURED: the sweep refused on its disk preflight ($(grep -aoE "[0-9.]+ GB free" "$SP/coord-train28-sweeps-$ts.log" | head -1)) -- battery STOPPED before nistec/reflect; free space, then relaunch the three tail legs"; exit 3; fi
stamp "SWEEP EVIDENCE: $(grep -a 'RECORD PRESERVED' "$SP/coord-train28-sweeps-$ts.log" | wc -l) record(s) preserved :: $(grep -a 'kept: ' "$SP/coord-train28-sweeps-$ts.log" | sed 's/.*kept: //' | tr '\n' ' ')"
stamp "LEG sweeps :: $(grep -aE 'PASS|FAIL|NOT MEASURED' "$SP/coord-train28-sweeps-$ts.log" | grep -av 'RECORD PRESERVED' | cut -c1-100 | tail -24 | tr '
' ' ; ')"
stamp "BATTERY: nistec COST PAIR vs the train-24 head (control worktree must be at that head)"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-nistec-pair.ps1" -Main "$(pwd -W)" -ControlSha "${CONTROL_SHA:-origin/master}" -SkipWarmup -Rounds 2 > "$SP/coord-train28-nistec-pair-$ts.log" 2>&1; echo "pair exit=$?" >> "$SP/coord-train28-nistec-pair-$ts.log"; stamp "PAIR: $(grep -E 'mean' "$SP/coord-train28-nistec-pair-$ts.log" | cut -c23-120 | tr '
' ';')"
stamp "BATTERY: reflect RUN"
powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-run.ps1" -Package reflect -Timeout 30m -Label "$TRAIN_LABEL" > "$SP/coord-train28-reflectrun-$ts.log" 2>&1; echo "reflectrun exit=$?" >> "$SP/coord-train28-reflectrun-$ts.log"
stamp "LEG reflectrun $(tail -1 "$SP/coord-train28-reflectrun-$ts.log") :: $(grep -aiE 'moved|PASS|FAIL|matched' "$SP/coord-train28-reflectrun-$ts.log" | tail -3 | cut -c1-120 | tr '
' ' ; ')"
stamp "TRAIN 28 CHAIN DONE"
