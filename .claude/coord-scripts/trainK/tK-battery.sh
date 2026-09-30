#!/usr/bin/env bash
# TRAIN K battery on the i7 -- the union of record (COORD), adapted from tJ-battery.sh (+ tJ-regate2.sh's lessons).
# Launch from a PER-RUN COPY (floor 4): copy this directory (tK-battery.sh, tK-emitcheck.sh, emitdrift.py, tK-helpers.py,
# tK-seats-draft.txt or the frozen tK-seats.txt, tK-i7-sweeps.txt, tK-i9-shard.txt) to a run folder and run the copy:
#   EXPECT_HEAD=<fixup sha, 10 chars> MASTER=f819887fa3 bash tK-battery.sh
# Logs land beside the copy in tK-logs/. The union is /h/go2cs-tmp-coord/tK: the 35 seat merges onto TRAIN J's landed
# master, then the TRAIN K FIXUP commit (tK-fixup.sh, a SEPARATE script: S1 sed + opt-in insert per FIXUP ORDER, the
# PRERES-K6 file_unix.cs refresh, the K1/K2/K3 asserts). The fixup does NOT refresh review siblings: ruling
# 857afcb47e's refresh is COORD's OPEN ruling for K (options in tK-fixup.sh's header), and leg E below lists the U-arm
# siblings it would owe. No map regeneration is owed: PRERES-K9 is a by-line resolution made at assembly.
# This battery REFUSES to run unless that fixup is HEAD. The i9 sweeps the complement shard (tK-i9-shard.txt). Legs
# run SEQUENTIALLY; the assembly worktree is FROZEN while this runs; every leg's rc is captured before any pipe
# (floor 7). One battery at a time on this box.
W=/h/go2cs-tmp-coord/tK
WB='H:\go2cs-tmp-coord\tK'
EXPECT_HEAD=${EXPECT_HEAD:?the fixup commit, 10 chars}
MASTER=${MASTER:?TRAIN J landed master, f819887fa3}
SD=$(cd "$(dirname "$0")" && pwd)
SEATS=${SEATS:-$SD/tK-seats.txt}; [ -f "$SEATS" ] || SEATS=$SD/tK-seats-draft.txt
I7LIST=$SD/tK-i7-sweeps.txt
I9LIST=${I9LIST:-$SD/tK-i9-shard.txt}
PRECHECK_MODE=${PRECHECK_MODE:-abort}   # 'warn' runs on past a failed PRE assert (COORD's explicit call, stated in SUMMARY)
LOGDIR=$SD/tK-logs
mkdir -p "$LOGDIR"
SUM="$LOGDIR/SUMMARY.txt"
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }
hp(){ python "$(cygpath -w "$SD/tK-helpers.py")" "$@"; }

ORIGPATH="$PATH"
export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
# GOROOT stays on C: in its BACKSLASH spelling (floor 6): GOROOT on ReFS H: deadlocks runtime's TestTracebackSystem
# panic child (2026-09-30), and a forward-slash spelling misroutes the emission.
export GOROOT="$(cygpath -w "$HOME/sdk/go1.24.13")" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
# Keep the system drive out of the battery (2026-09-27: C: filled to 0 bytes mid-run): temp, Go cache, NuGet on H:.
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tmp /h/go-cache/go-build /h/nuget/packages
export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"
PWSH=$HOME/.dotnet/tools/pwsh
LOGW=$(cygpath -w "$LOGDIR")
WW=$(cygpath -w "$W")

# ================================================================================================= PRE (no builds)
cd "$W" || { stamp "ABORT: no worktree $W"; exit 2; }
h=$(git rev-parse --short=10 HEAD)
[ "$h" = "$EXPECT_HEAD" ] || { stamp "ABORT: HEAD $h != $EXPECT_HEAD"; exit 2; }
subj=$(git log -1 --format=%s HEAD)
case "$subj" in "fixup: TRAIN K"*) ;; *) stamp "ABORT: HEAD is not the TRAIN K fixup (subject: ${subj:0:120})"; exit 2 ;; esac
np=$(git rev-list --parents -n 1 HEAD | wc -w)
[ "$np" = 2 ] || { stamp "ABORT: the fixup HEAD has $((np - 1)) parents (a single-parent commit on the assembled union is ruled)"; exit 2; }
[ "$(go env GOROOT)" = "$(cygpath -w "$HOME/sdk/go1.24.13")" ] || { stamp "ABORT: GOROOT pin $(go env GOROOT)"; exit 2; }
git merge-base --is-ancestor "$MASTER" HEAD || { stamp "ABORT: master $MASTER is not an ancestor"; exit 2; }
ns=0
while IFS='|' read -r b sha msg; do
  ns=$((ns + 1)); git merge-base --is-ancestor "$sha" HEAD || { stamp "ABORT: seat $b $sha not an ancestor"; exit 2; }
done < <(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS")
# The union holds ONLY master + the seat merges + the fixup: first-parent count == seats + 1, every first-parent commit
# below HEAD is a two-parent merge whose ^2 IS a listed seat (or contains one that its first parent lacks), and the
# fixup is signed (tK-fixup.sh rules ONE signed commit). A stray commit on the union branch is never gated unseen.
fpc=$(git rev-list --first-parent --count "$MASTER..HEAD")
[ "$fpc" = $((ns + 1)) ] || { stamp "ABORT: union first-parent count $fpc != seats $ns + 1 (fixup) -- a stray or missing commit on the union"; exit 2; }
fsig=$(git log -1 --format=%G? HEAD)
case "$fsig" in G|U) ;; *) stamp "ABORT: the fixup HEAD signature reads '$fsig' (want G or U)"; exit 2 ;; esac
SEATFULL=$(grep -E '^[A-Za-z0-9._-]+\|' "$SEATS" | cut -d'|' -f2 | while read -r s; do git rev-parse "$s^{commit}"; done)
mbad=0; mcontain=0; munsigned=0
for c in $(git rev-list --first-parent "$MASTER..HEAD^"); do
  [ "$(git rev-list --parents -n 1 "$c" | wc -w)" = 3 ] || { stamp "  PRE union: $(git rev-parse --short=10 "$c") is not a two-parent merge"; mbad=$((mbad + 1)); continue; }
  p2=$(git rev-parse "$c^2")
  if echo "$SEATFULL" | grep -qxF "$p2"; then :
  elif echo "$SEATFULL" | while read -r s; do git merge-base --is-ancestor "$s" "$p2" && ! git merge-base --is-ancestor "$s" "$c^1" && echo y; done | grep -q y; then
    mcontain=$((mcontain + 1)); stamp "  PRE union: merge $(git rev-parse --short=10 "$c") ^2=$(git rev-parse --short=10 "$p2") CONTAINS a listed seat (not the seat sha itself)"
  else
    stamp "  PRE union: merge $(git rev-parse --short=10 "$c") ^2=$(git rev-parse --short=10 "$p2") is NOT a listed seat"; mbad=$((mbad + 1))
  fi
  case "$(git log -1 --format=%G? "$c")" in G|U) ;; *) munsigned=$((munsigned + 1)) ;; esac
done
[ "$mbad" = 0 ] || { stamp "ABORT: $mbad first-parent commit(s) on the union are not seat merges"; exit 2; }
stamp "PRE union shape OK: first-parent=$fpc (= $ns seat merges + fixup), ^2-contains-seat=$mcontain, fixup signed=$fsig, unsigned seat merges=$munsigned (stated, not gated)"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "ABORT: tracked changes before the battery"; exit 2; }
# global.json (p1-s1-dotnet-pin): 10.0.100 latestFeature. The SDK this tree resolves must be a 10.0 band, and a refusal
# (an SDK global.json does not admit) fails every dotnet leg with the same message -- read it here, once.
dv=$(dotnet --version 2>&1); dvrc=$?
case "$dvrc:$dv" in 0:10.0.*) ;; *) stamp "ABORT: dotnet under global.json rc=$dvrc: $(echo "$dv" | tr '\r\n' '  ' | cut -c1-200)"; exit 2 ;; esac
stamp "PRE head=$h (fixup: ${subj:0:90}) parent=$(git rev-parse --short=10 HEAD^) tree=$(git rev-parse --short=10 HEAD^{tree}) signed=$(git log -1 --format=%G? HEAD) master=$MASTER seats=$ns ($(basename "$SEATS")) $(go version) dotnet=$(echo "$dv" | tr -d '\r') free=$(freegb)G C:=$(cfree)G"

# PRE-1: PRERES asserts on the assembled tree (tK-helpers.py precheck): K1/K3 registration counts == base + each seat's
# own net inserts; K8's decisive Goroutine.cs counts; K9 census rows + map lines x3; K6 StackTraceHidden x2; no conflict
# markers under src/; S1's (1,0,1) completeness census; optin.py check == CHECK PASS (FIXUP ORDER steps 3 and 4).
hp precheck "$WW" "$MASTER" "$(cygpath -w "$SEATS")" > "$LOGDIR/precheck.log" 2>&1; prc=$?
stamp "PRE-1 precheck rc=$prc :: $(grep -a '^PRECHECK' "$LOGDIR/precheck.log") :: $(grep -a '^FAIL' "$LOGDIR/precheck.log" | head -n 4 | tr '\n' ' ' | cut -c1-500)"
grep -a '^note' "$LOGDIR/precheck.log" | head -n 5 | while IFS= read -r l; do stamp "  PRE-1 $l"; done
# PRE-1b: tK-postasserts.sh (the fixup author's K8/K9 checker, the contract says the battery's PRE calls it too): a
# second, independent derivation of K8/K9 beside precheck's. Exit 0 = PASS, 4 = a FAIL.
if [ -f "$SD/tK-postasserts.sh" ]; then
  bash "$SD/tK-postasserts.sh" "$W" > "$LOGDIR/postasserts.log" 2>&1; parc=$?
  stamp "PRE-1b postasserts rc=$parc :: $(grep -aE '^\s+FAIL' "$LOGDIR/postasserts.log" | head -n 3 | tr '\n' ' ' | cut -c1-300)$(tail -n 1 "$LOGDIR/postasserts.log" | cut -c1-120)"
  [ "$parc" = 0 ] || prc=$((prc + 10))
else
  stamp "PRE-1b postasserts: tK-postasserts.sh NOT in the run copy -- K8/K9 read by precheck alone"
fi
if [ "$prc" != 0 ]; then
  [ "$PRECHECK_MODE" = warn ] || { stamp "ABORT: a PRE assert failed (precheck.log); PRECHECK_MODE=warn runs past it by COORD's explicit call"; exit 2; }
  stamp "PRE-1 FAILED and PRECHECK_MODE=warn: running on by COORD's explicit call"
fi

# PRE-2: sweep coverage. Every roster row at the union is swept by the i7 or the i9. The roster must be EXACTLY the two
# lists' disjoint union, so a MISSING, EXTRA, EXTRA-I9 or DUP row ABORTS below (update the lists). Only under
# PRECHECK_MODE=warn does the run go on: a MISSING row is then added to the i7 run and an EXTRA row dropped (the sweep
# throws on an unmatched -Exact filter).
hp coverage "$WW" "$(cygpath -w "$I7LIST")" "$(cygpath -w "$I9LIST")" > "$LOGDIR/coverage.log" 2>&1; cvrc=$?
# PRE-3: the reflect-bridge canaries (validation-bank: the FIVE largest banked reflect importers by verdict count, DERIVED
# now, never carried; controls encoding/json IN, cmp OUT, go/doc/comment OUT). r-field-ptr-equality,
# r-named-slice-reflect-dims and c1-reflect-hash-band are bridge-touching. A derived canary absent from the i7 list is
# added to the run (it is read on the union-of-record box even when the i9's shard also carries it).
hp canaries "$WW" > "$LOGDIR/canaries.log" 2>&1; carc=$?
RUNLIST="$LOGDIR/sweep-runlist.txt"
{ grep -vE '^\s*(#|$)' "$I7LIST"; grep -a '^MISSING ' "$LOGDIR/coverage.log" | cut -d' ' -f2; grep -a '^CANARY ' "$LOGDIR/canaries.log" | cut -d' ' -f2; } \
  | LC_ALL=C sort -u | grep -vxF -f <(grep -a '^EXTRA ' "$LOGDIR/coverage.log" | cut -d' ' -f2; echo '#none#') > "$RUNLIST"
stamp "PRE-2 coverage rc=$cvrc :: $(grep -a '^COVERAGE' "$LOGDIR/coverage.log") :: missing->i7: $(grep -a '^MISSING ' "$LOGDIR/coverage.log" | cut -d' ' -f2 | tr '\n' ' ') extra(dropped): $(grep -a '^EXTRA ' "$LOGDIR/coverage.log" | cut -d' ' -f2 | tr '\n' ' ') i9-shard-not-in-roster: $(grep -a '^EXTRA-I9 ' "$LOGDIR/coverage.log" | cut -d' ' -f2 | tr '\n' ' ') dup(i7+i9): $(grep -ac '^DUP ' "$LOGDIR/coverage.log")"
stamp "PRE-3 canaries rc=$carc :: $(grep -a '^CANARY' "$LOGDIR/canaries.log" | tr '\n' ' ' | cut -c1-300) :: run list $(wc -l < "$RUNLIST") rows"
# coverage exits non-zero on roster=0, any EXTRA/EXTRA-I9/DUP, or roster != i7+i9 (a roster-format change would read
# 0 rows, drop every i7 row as EXTRA and sweep NOTHING -- a vacuous green); canaries exits non-zero on a failed control
# or fewer than five canaries. Either aborts here unless PRECHECK_MODE=warn.
if [ "$cvrc" != 0 ] || [ "$carc" != 0 ]; then
  [ "$PRECHECK_MODE" = warn ] || { stamp "ABORT: PRE-2 coverage rc=$cvrc / PRE-3 canaries rc=$carc (coverage.log, canaries.log); PRECHECK_MODE=warn runs past it by COORD's explicit call"; exit 2; }
  stamp "PRE-2/PRE-3 FAILED (coverage rc=$cvrc canaries rc=$carc) and PRECHECK_MODE=warn: running on by COORD's explicit call"
fi
[ -s "$RUNLIST" ] || { stamp "ABORT: the sweep run list is EMPTY"; exit 2; }

leg(){ # name, logfile-suffix, command...
  local name=$1 suf=$2; shift 2
  local L="$LOGDIR/$suf.log" t0 f c
  t0=$(date +%s); LEG_T0=$t0
  f=$(freegb); [ "${f:-0}" -ge 30 ] || { stamp "LEG $name ABORT: free disk ${f}G < 30G"; exit 3; }
  c=$(cfree); [ "${c:-0}" -ge 8 ] || { stamp "LEG $name ABORT: C: free ${c}G < 8G"; exit 3; }
  "$@" > "$L" 2>&1 < /dev/null; local rc=$?
  LEG_RC=$rc
  stamp "LEG $name rc=$rc wall=$(( $(date +%s) - t0 ))s free=$(freegb)G log=$suf.log"
  return $rc
}
purge(){ # label
  local P left td
  P=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
  left=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  td=$(git status --porcelain | grep -c '^ D')
  stamp "PURGE($1) purged=$P remaining=$left tracked-deletions=$td free=$(freegb)G"
  [ "$left" = 0 ] && [ "$td" = 0 ] || { stamp "ABORT: purge incomplete or tracked deletions"; exit 3; }
}

# ================================================================================================= LEGS
# LEG C -- the full converter suite. Gates every converter seat by its own test: r-s2-cs15-escape (csReservedTypeNames),
# r-hop-population-gen (internal/genpopulation), r-m1-module-proof-pages (moduleProofPages), i9-r1a-commit2
# (sliceBoundsMethodEmission), r-d1/d1b/d2/d3 (their converter halves), p1-s3-cgo-refusal (cgoSelectedRefusal),
# p1-tests-goos (testHostGoTargetOS), p1-os-withdrawal-gate (testConversion_test), c2-osr-optin (csprojTemplate),
# p1-s1 (csprojMetadata), c2-gen-guard + p1-plan-nugetgo-rulings (repoguard), i9-a3 (callerInliningAnalysis), and
# PRERES-K1's projitemsIntegrity (SEVEN seats register a test file in go2cs-src.projitems).
leg C conv-suite bash -c 'cd src/go2cs && go test -count=1 -timeout 40m ./...'
stamp "  C: $(grep -aE '^(ok|FAIL|---)' "$LOGDIR/conv-suite.log" | tr '\n' ' ' | cut -c1-400) :: projitems: $(grep -aE 'TestProjitems[A-Za-z]*' "$LOGDIR/conv-suite.log" | grep -ac FAIL) FAIL line(s)"

# LEG GP -- r-hop-population-gen's OWN gate: internal/genpopulation has no _test file (leg C only compiles it), so the
# generator is run here over the pinned GOROOT and its output diffed, CR-stripped, against the committed
# docs/phase4/data/population-go1.24.13.txt. Read-only: go run builds into GOCACHE and writes only into the log folder.
gp_run(){ ( cd src/go2cs && MSYS_NO_PATHCONV=1 go run ./internal/genpopulation -goroot "$GOROOT" -out "$LOGW\\population-go1.24.13.txt" ); }
leg GP genpopulation gp_run
gprc=$LEG_RC
if [ "$gprc" = 0 ] && [ -f "$LOGDIR/population-go1.24.13.txt" ]; then
  diff <(tr -d '\r' < "$LOGDIR/population-go1.24.13.txt") <(tr -d '\r' < docs/phase4/data/population-go1.24.13.txt) > "$LOGDIR/population.diff" 2>&1; gpd=$?
else
  gpd=NA
fi
stamp "  GP: generator rc=$gprc diff rc=$gpd ($([ "$gpd" = 0 ] && echo 'REPRODUCES the committed population' || echo 'DOES NOT reproduce the committed population -- population.diff / genpopulation.log')) :: $(head -n 1 "$LOGDIR/population-go1.24.13.txt" 2>/dev/null | tr -d '\r' | cut -c1-120)"

# LEG E -- whole-corpus emission check: two arms (master vs the fixup HEAD), three targets, planted controls, S3 refusal
# count, K6/K9 named readings, committed-csproj M-vs-U (FIXUP ORDER step 5). Gates the corpus of i9-r1a-commit2
# (610 files), i9-a3 (6 NoInlining sites + maps), c1-print-fidelity (maps), g-pprof-symbol-name, p2-created-by, and the
# footprint-0 claims of r-s2, r-d1, r-d1b, r-d2, r-d3, r-m1, p1-s3, p1-tests-goos, c2-osr-optin.
leg E emitcheck env M="$MASTER" U="$EXPECT_HEAD" LOGDIR="$LOGDIR" bash "$SD/tK-emitcheck.sh"
stamp "  E: $(grep -aE '^(EMIT |PLANT|windows:|linux:|darwin:|UNION-ATTRIBUTABLE TOTAL|K6|K9|CSPROJ|EMITCHECK VERDICT|EVIDENCE|ABORT)' "$LOGDIR/emitcheck.log" | tr '\r\n' '  ' | cut -c1-1600)"

# LEG G1/G2 -- the roster guard under pwsh 7 and Windows PowerShell 5.1 (DOTNET_ROOT unset, dotnet10 off PATH).
# Gates coord-roster-cleanup (the roster rewrite), r-hop-population-gen (section 2b reads data/population-go1.24.13.txt)
# and every _roster.ps1 edit (c2-gen-guard's Test-GeneratedTypeMissingFailure; PRERES-K7).
leg G1 roster env -u DOTNET_ROOT PATH="$ORIGPATH" "$PWSH" -NoProfile -File src/check-roster-format.ps1
stamp "  G1: $(tail -n 3 "$LOGDIR/roster.log" | tr '\r\n' '  ' | cut -c1-300)"
leg G2 roster51 env -u DOTNET_ROOT PATH="$ORIGPATH" powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1
stamp "  G2: $(tail -n 1 "$LOGDIR/roster51.log" | tr '\r\n' '  ' | cut -c1-300)"

# LEG SY -- symbol sync (converter seats that touch naming: r-s2-cs15-escape's identifierNaming).
leg SY symbol-sync env -u DOTNET_ROOT PATH="$HOME/sdk/go1.24.13/bin:$ORIGPATH" "$PWSH" -NoProfile -File src/check-symbol-sync.ps1
stamp "  SY: $(tail -n 3 "$LOGDIR/symbol-sync.log" | tr '\r\n' '  ' | cut -c1-300)"

# LEG SI -- PRERES-K3: check-solution-integrity at the union (six new behavioral projects registered in go2cs.slnx by
# i9-r1a-commit2, r-d3, r-d1, r-d2, r-d1b, r-named-slice-reflect-dims; the per-GOOS project graph acyclic), then its
# own POSITIVE CONTROL (floor 13): an injected runtime -> internal/syscall/windows edge must print W1's six cycles, exit 1.
leg SI solution-integrity powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-solution-integrity.ps1
stamp "  SI: $(tail -n 3 "$LOGDIR/solution-integrity.log" | tr '\r\n' '  ' | cut -c1-300)"
leg SIc solution-integrity-control env MSYS_NO_PATHCONV=1 powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-solution-integrity.ps1 -TargetOS windows -InjectReference 'runtime=internal/syscall/windows'
sic=$LEG_RC
# The control FIRES only on rc=1 AND exactly W1's six cycle lines AND no unhandled-error abort: the script also exits 1
# on a bad -InjectReference, a parse throw or a missing file ("ABORTED on an unhandled error"), which is not a firing.
sicl=$(grep -ac 'runtime -> internal/syscall/windows' "$LOGDIR/solution-integrity-control.log")
sica=$(grep -ac 'ABORTED on an unhandled error' "$LOGDIR/solution-integrity-control.log")
if [ "$sic" = 1 ] && [ "$sicl" = 6 ] && [ "$sica" = 0 ]; then SICV='control FIRED (six cycles)'
elif [ "$sic" = 1 ] && [ "$sicl" -ge 1 ] && [ "$sica" = 0 ]; then SICV="CONTROL FIRED WITH THE WRONG CYCLE SET ($sicl, want 6) -- read the log before trusting SI"
else SICV='CONTROL DID NOT FIRE -- SI proves nothing'; fi
stamp "  SIc (EXPECT rc=1, six cycles, no abort): rc=$sic cycle-lines=$sicl aborted=$sica :: $SICV"

# LEG ST -- PRERES-K7: the sweep's classifier self-test (Test-OracleOnlyFailure + c2-gen-guard's
# Test-GeneratedTypeMissingFailure, read through _roster.ps1 as the sweep reads it) under both editions.
leg ST51 sweep-selftest51 powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/sweep-oracle-rerun-selftest.ps1
stamp "  ST51: $(grep -a 'sweep-oracle-rerun-selftest:' "$LOGDIR/sweep-selftest51.log" | tail -n 1 | cut -c1-200)"
leg ST7 sweep-selftest7 env -u DOTNET_ROOT PATH="$ORIGPATH" "$PWSH" -NoProfile -File src/tests/sweep-oracle-rerun-selftest.ps1
stamp "  ST7: $(grep -a 'sweep-oracle-rerun-selftest:' "$LOGDIR/sweep-selftest7.log" | tail -n 1 | cut -c1-200)"

# LEG NV -- PRERES-K2: push-nuget's release pre-flight at the union (r-hop-population-gen's data/ read beside
# i9-j0-uuid-rehearsal's -VersionSuffix hunks), BEFORE any sweep or -tests leg rewrites proof pages or badges. Read-only
# (-VerifyOnly: nothing bumped, tagged, frozen, packed or pushed; its git calls are tag --list and ls-remote). Both
# editions. Then the REFUSAL ARM: -VersionSuffix with -VerifyOnly must throw by name (exit 1) -- the one refusal arm
# that stays harmless if the refusal were broken (-VerifyOnly still packs nothing), unlike -Push or -BumpBuild.
leg NV51 push-nuget-verify51 powershell -NoProfile -ExecutionPolicy Bypass -File src/push-nuget.ps1 -VerifyOnly
stamp "  NV51: $(grep -aiE 'Pre-flight clean|problem|backed by nothing|Cannot find|refus' "$LOGDIR/push-nuget-verify51.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
leg NV7 push-nuget-verify7 env -u DOTNET_ROOT PATH="$ORIGPATH" "$PWSH" -NoProfile -File src/push-nuget.ps1 -VerifyOnly
stamp "  NV7: $(grep -aiE 'Pre-flight clean|problem|backed by nothing|Cannot find|refus' "$LOGDIR/push-nuget-verify7.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
leg NVR push-nuget-suffix-refusal powershell -NoProfile -ExecutionPolicy Bypass -File src/push-nuget.ps1 -VerifyOnly -VersionSuffix local.1
nvr=$LEG_RC; nvm=$(grep -ac 'VersionSuffix is a local-rehearsal pack and is refused' "$LOGDIR/push-nuget-suffix-refusal.log")
stamp "  NVR (EXPECT rc!=0 + the refusal by name): rc=$nvr named=$nvm :: $([ "$nvr" != 0 ] && [ "$nvm" -ge 1 ] && echo 'REFUSED BY NAME' || echo 'REFUSAL ARM FAILED')"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || stamp "  NOTE: tracked changes after the read-only legs: $(git status --porcelain | grep -v '^??' | head -n 5 | tr '\n' ' ')"

# LEG 2b -- go2cs.slnx (golib/gen API changes: r-field-ptr-equality, c1-reflect-hash-band, p1-spanlength-fix,
# c1-print-fidelity's IPrintSink, g-gpc-torn-snapshot, p2-created-by, g-pprof-symbol-name; the generator-load codes are
# now errors (c2-gen-guard); every behavioral/performance csproj at LangVersion 14 after the S1 fixup).
leg 2b go2cs-slnx dotnet build src/go2cs.slnx -c Debug -m -p:UseSharedCompilation=false --no-incremental
stamp "  2b: errors=$(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/go2cs-slnx.log") gen-load=$(grep -acE '(error|warning) CS(8032|8034|8784|8785)' "$LOGDIR/go2cs-slnx.log") warnings-line=$(grep -aE 'Warning\(s\)|Error\(s\)' "$LOGDIR/go2cs-slnx.log" | tr '\n' ' ' | cut -c1-120)"

# LEG GN -- GenTests (src/gen changes: p1-s1's unconditional LangVersion 14 in go2cs-gen.csproj, c2-gen-guard's props).
leg GN gentests bash -c "dotnet build src/tests/GenTests/GenTests.csproj -c Debug -p:UseSharedCompilation=false && dotnet test src/tests/GenTests/GenTests.csproj -c Debug --no-build"
stamp "  GN: $(grep -aE '(Passed|Failed)!' "$LOGDIR/gentests.log" | tail -n 1 | cut -c1-200) :: failed-names: $(grep -aE '^\s+Failed [A-Za-z]' "$LOGDIR/gentests.log" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-300)"

# LEG TR -- BehavioralTests filtered to TestingRuntimeTests (the testing host moves: i9-d4-go-test-order's TestRunner
# order, i9-d5-host-lf's bare-LF event lines, p2-created-by's testing half).
leg TR testing-runtime bash -c "dotnet build src/tests/Behavioral/BehavioralTests/BehavioralTests.csproj -c Debug -p:UseSharedCompilation=false -p:go2csPath=H:/go2cs-tmp-coord/tK/src/ && dotnet test src/tests/Behavioral/BehavioralTests/BehavioralTests.csproj -c Debug --no-build -p:go2csPath=H:/go2cs-tmp-coord/tK/src/ --filter FullyQualifiedName~TestingRuntimeTests"
stamp "  TR: $(grep -aE '(Passed|Failed)!' "$LOGDIR/testing-runtime.log" | tail -n 1 | cut -c1-200)"

# LEG GT -- GolibTests, Debug and Release, with a TRX so each K seat's own test class is read BY NAME (a class with 0
# results is NOT FOUND, never green). Gates: r-field-ptr-equality, g-pprof-symbol-name, i9-d4, p1-spanlength-fix,
# c1-reflect-hash-band (x3 classes), i9-d5, p2-created-by (x3), p2-descriptor-guard (linux-only: expect skipped/absent
# rows here), p1-tracer-oracle-pin (x2; the oracle is GOROOT's go = the pin, so it RUNS), c1-print-fidelity +
# i9-a3 (ThreadStateCensus, PRERES-K9's two rows), g-gpc-torn-snapshot, i9-a3, and TRAIN J's A11 CleanupDispatch.
# c1-print-fidelity's own gate is NoUncountedBackingAllocationsTests (7/7). A Class.Method entry reads that ONE test's
# outcome by name (p1-tracer-oracle-pin: GosOwnParserAcceptsAManagedProgramsTrace RUNS -- Passed, not NotExecuted).
GT_CLASSES="FieldPointerEqualityTests FrameSymbolNameTests GoTestOrderTests UnsafeLengthLimitTests ReflectHashTokenBandTests SyntheticPCRegistryTests RuntimeCallerPCSpanTests EventLineTerminatorTests TracebackDecorationTests TestGoroutineCreatorTests CreatedByPositionTests LinuxDescriptorLimitTests ExecutionTracerParserTests ExecutionTracerOracleResolutionTests PrintThroughRuntimeTests NoUncountedBackingAllocationsTests ThreadStateCensusTests GoroutineProfileInstantTests RuntimeLockProfileTests CleanupDispatchTests ExecutionTracerParserTests.GosOwnParserAcceptsAManagedProgramsTrace"
GT_NF=''   # NOT FOUND classes across both configs (LinuxDescriptorLimitTests, linux-only, excluded), carried to END
for cfg in Debug Release; do
  leg GT-$cfg golibtests-$cfg bash -c "dotnet build src/tests/GolibTests/GolibTests.csproj -c $cfg -p:UseSharedCompilation=false && dotnet test src/tests/GolibTests/GolibTests.csproj -c $cfg --no-build --logger 'trx;LogFileName=golibtests-$cfg.trx' --results-directory '$LOGW'"
  stamp "  GT-$cfg: $(grep -aE '(Passed|Failed)!' "$LOGDIR/golibtests-$cfg.log" | tail -n 1 | cut -c1-200) :: failed-names: $(grep -aE '^\s+Failed [A-Za-z]' "$LOGDIR/golibtests-$cfg.log" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-300)"
  if [ -f "$LOGDIR/golibtests-$cfg.trx" ]; then
    hp trx "$LOGW\\golibtests-$cfg.trx" $GT_CLASSES > "$LOGDIR/golibtests-$cfg.classes.txt" 2>&1
    nf=$(grep -a 'NOT FOUND' "$LOGDIR/golibtests-$cfg.classes.txt" | sed 's/^TRX \(class\|test\) //; s/:.*//' | grep -vxF LinuxDescriptorLimitTests | tr '\n' ' ')
    [ -n "$nf" ] && GT_NF="$GT_NF$cfg:[${nf% }] "
    stamp "  GT-$cfg classes: NOT-FOUND=[${nf% }] $(grep -a 'Failed=' "$LOGDIR/golibtests-$cfg.classes.txt" | sed 's/^TRX class //' | tr '\n' ' ' | cut -c1-300) :: $(grep -a '^TRX test ' "$LOGDIR/golibtests-$cfg.classes.txt" | sed 's/^TRX test //' | tr '\n' ' ' | cut -c1-200) (per-class table: golibtests-$cfg.classes.txt)"
  else
    GT_NF="$GT_NF$cfg:[NO TRX] "
    stamp "  GT-$cfg classes: NO TRX written -- the by-name reading is NOT MEASURED"
  fi
done
purge after-GT

# LEG 4 -- CNR (byte-identical behavioral C# AND csproj under the union converter). Gates the 33 behavioral
# re-baselines of i9-r1a-commit2, r-d1/d1b/d2/d3 + r-named-slice-reflect-dims goldens, and the fixup's S1 sed + opt-in
# insert over every behavioral csproj (the union template must reproduce them byte for byte).
leg 4 cnr powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-no-regression.ps1
stamp "  4: $(grep -aiE 'NO REGRESSION|REGRESSION|byte-identical|CHANGED|verdict' "$LOGDIR/cnr.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
purge after-CNR

# LEG 5 -- the full behavioral suite (D1, D1b, D2, D3, R1-A's SliceBoundsRecover, R's NamedSliceReflectDims; every
# golib/gen change reaches it), then EACH K guard ISOLATED by name, then every run-5 NOT MEASURED name ISOLATED
# (TRAIN J run 1 lesson: six Output timeouts in one load window; never waived by 'close enough').
leg 5 behavioral powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --build-timeout 10800 --build-one-timeout 900
stamp "  5: $(grep -aiE 'Transpile|Compile|Target|Output|compared|pass|fail|skip' "$LOGDIR/behavioral.log" | tail -n 8 | tr '\r\n' '  ' | cut -c1-600)"
# The runner joins several phases with commas ([Compile:timeout,Output:timeout]) and lists at most 20 names, then
# "... and N more." -- a truncated list is stamped, and COORD re-runs rather than reading the isolated set as complete.
NM=$(grep -aoE '^\s+[A-Za-z0-9_]+ \[[^]]*:timeout[^]]*\]' "$LOGDIR/behavioral.log" | awk '{print $1}' | LC_ALL=C sort -u | tr '\n' ' ')
NMBE=$(grep -aoE '^\s+[A-Za-z0-9_]+ \[[^]]*:best-effort[^]]*\]' "$LOGDIR/behavioral.log" | awk '{print $1}' | LC_ALL=C sort -u | tr '\n' ' ')
NMT=$(grep -aoE '^\s+\.\.\. and [0-9]+ more\.' "$LOGDIR/behavioral.log" | tail -n 1 | tr -s ' ')
[ -n "$NMT" ] && stamp "  5 NOT-MEASURED LIST TRUNCATED ($NMT) -- the isolated set below is NOT complete; COORD re-runs"
[ -n "$NMBE" ] && stamp "  5 BEST-EFFORT (not isolated: a bigger budget cannot help): [${NMBE% }]"
KGUARDS="SliceBoundsRecover PackageVarTupleInitOrder SelectSendTupleSpread PlainSendOperandOrder NamedSliceOfArrayDims NamedSliceReflectDims"
stamp "  5 named: NOT-MEASURED=[${NM}] :: K-guard mentions on non-pass lines: $(for g in $KGUARDS; do n=$(grep -aE "\b$g\b" "$LOGDIR/behavioral.log" | grep -aciE 'fail|timeout|mismatch|differ'); [ "$n" != 0 ] && printf '%s=%s ' "$g" "$n"; done)"
for p in $KGUARDS $NM; do
  leg "B:$p" "behav-$p" powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --filter "$p"
  stamp "  B:$p: $(grep -aE '^\s+(Transpile|Compile|Target|Output)\s+pass' "$LOGDIR/behav-$p.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-300) :: $(grep -aE '^(PASS|FAIL)' "$LOGDIR/behav-$p.log" | tail -n 1 | cut -c1-120)"
done
purge after-behavioral

# LEG H7 -- compile parity per flavour (every golib/gen/corpus change; the S1 LangVersion pin and the generator-load
# errors reach all three flavours).
export MSYS_NO_PATHCONV=1
for FL in windows linux darwin; do
  leg H7-$FL h7-$FL dotnet build src/go2cs-stdlib.slnx -c Debug -p:GoTargetOS=$FL --no-incremental -m -p:UseSharedCompilation=false
  L="$LOGDIR/h7-$FL.log"
  stamp "  H7-$FL: CS=$(grep -aoE 'error CS[0-9]+' "$L" | wc -l) MSB/NETSDK=$(grep -aoE 'error (MSB|NETSDK)[0-9]+' "$L" | wc -l) :: $(grep -aoE '[A-Za-z0-9_./\\-]+[.]cs[(][0-9]+,[0-9]+[)]: error CS[0-9]+' "$L" | sed 's#\\#/#g; s#.*/core/#core/#' | LC_ALL=C sort -u | head -n 5 | tr '\n' ' ')"
  purge after-H7-$FL
done
unset MSYS_NO_PATHCONV

# LEG HOP -- PRERES-K7's live arm: -Hop at the real go1.24.13 tree over ONE row (cmp, r-hop-population-gen's own
# non-hop control row) through the union's run-validated-sweep.ps1, where c2-gen-guard's re-run arm now also lives. It
# must read docs/phase4/data/population-go1.24.13.txt and give cmp a HOP verdict with rc=0 (the -Hop path and the
# generator re-run arm do not interact on a healthy row).
leg HOP hop-arm powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Hop -Filter cmp -Exact
stamp "  HOP: $(grep -aE 'hop population:|HOP re-derivation|^\s+HOP|HOPNONE|^(sweep|hop):|RERUN|refus' "$LOGDIR/hop-arm.log" | tr -s ' ' | tr '\r\n' '  ' | cut -c1-500) :: population-1.24.13=$(grep -ac 'population-go1.24.13.txt' "$LOGDIR/hop-arm.log")"

# LEG S -- the banked rows the union reaches on this box (sweep-runlist.txt: tK-i7-sweeps.txt = TRAIN J's i7 split +
# runtime/debug (banked by TRAIN J) + the derived canaries, plus any row PRE-2 found in neither list). One row at a
# time, exact match. i9-r1a-commit2 moves ~107 rows' test sources and D4/D5 change the host's test order and line
# endings for every row, so every banked row is reached; the i9 sweeps tK-i9-shard.txt. A failed row, and the three
# rows K names (os, testing, runtime/debug), get the comparison record read (freshness, error names, named verdicts):
#   os             p1-os-withdrawal-gate: TestRemoveAllWithExecutedProcess pass/pass -> os re-banks 1106 + 2 at the
#                  landing (the roster still reads 1105 + 2 until then). i7 ENVIRONMENT: TestFileReadDir/. diverges on
#                  ReFS (ledger 61f422cc23) -- a NAMED divergence, never absorbed.
#   testing        i7 ENVIRONMENT: TestChdir/relative (Go skips on this host) -- named, never absorbed.
#   runtime/debug  p2-created-by: 0 changes; TestStack must stay pass (TRAIN J's bank, 8 + 1).
# A for-loop over the list, never `while read < file`: freegb/cfree run powershell.exe inside every leg, and a console
# host reading the loop's stdin would silently eat rows.
for pkg in $(cat "$RUNLIST"); do
  n=$(echo "$pkg" | tr '/' '.')
  leg "S:$pkg" "sweep-$n" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact
  src=$LEG_RC; st0=$LEG_T0
  stamp "  S:$pkg: $(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT|RERUN)" "$LOGDIR/sweep-$n.log" | tail -n 1 | cut -c1-200)"
  case "$pkg" in
    os) env='TestFileReadDir/.'; named='TestRemoveAllWithExecutedProcess,TestFileReadDir' ;;
    testing) env='TestChdir/relative'; named='TestChdir' ;;
    runtime/debug) env=''; named='TestStack,TestFreeOSMemory,TestPanicOnFault' ;;
    *) env=''; named='' ;;
  esac
  if [ "$src" != 0 ] || [ -n "$named" ]; then
    hp treport --dir "$WW\\src\\core\\$(echo "$pkg" | tr '/' '\\')" --log "$LOGW\\sweep-$n.log" --t0 "$st0" --rc "$src" --label "S:$pkg" --named "$named" --env "$env" > "$LOGDIR/sweep-$n.reading.txt" 2>&1
    while IFS= read -r l; do stamp "    $l"; done < <(grep -aE 'FRESHNESS|ERROR-NAMES|ENV-CLASS|NAMED ' "$LOGDIR/sweep-$n.reading.txt" | cut -c1-400 | head -n 12)
    [ -f "src/core/$pkg/go2cs_test_comparison.json" ] && cp "src/core/$pkg/go2cs_test_comparison.json" "$LOGDIR/sweep-$n.go2cs_test_comparison.json"
  fi
done
purge after-sweep

# LEG T -- direct -tests. REBUILD the converter first: purge after-sweep removes every src/**/bin, the converter's own
# included (TRAIN J run 2 attempt 1 lost both T legs to rc=127 exactly this way, 2026-09-30 01:01).
#  T:runtime/pprof -- the BANK CANDIDATE (g-gpc-torn-snapshot's gate + g-pprof-symbol-name; i9-a3 reads pprof 7=7).
#    G's proof on its merge of the two: Validated 145, 7 disclosed-divergent, 2 unsupported, exit 0. This leg's
#    BANK-READING decides the bank: rc, the Validated line, the error names, and the FRESHNESS rule (gate-forensics:
#    the comparison AND the results file both written after the leg started; a stale results.json beside a fresh
#    comparison is never read as a deadline kill) plus the results tail's package timeout event.
#  T:runtime -- the FULL windows runtime row at the union: i9-a3 (the 3 TestRuntimeLockMetricsAndProfile names),
#    p2-created-by (TestTracebackParentChildGoroutines), i9-d5 (TestFinalizerRegisterABI pass/pass on windows),
#    D4's order, p1-spanlength-fix (TestMemmoveOverflow's pin), c1-print-fidelity, TRAIN J's A11/A15 rows.
export MSYS_NO_PATHCONV=1
( cd src/go2cs && go build -o bin/go2cs.exe . ) > "$LOGDIR/conv-build.log" 2>&1
EXE="$W/src/go2cs/bin/go2cs.exe"
[ -x "$EXE" ] || { stamp "LEG T ABORT: no converter at $EXE"; exit 3; }
stamp "BUILD converter sha=$(sha256sum "$EXE" | cut -c1-16)"
tleg(){ # pkg, timeout, suffix, named-prefixes, bank(yes|no), extra args...
  local pkg=$1 to=$2 suf=$3 named=$4 bank=$5; shift 5
  local pb n L trc tt0
  pb=$(echo "$pkg" | tr '/' '\\'); n=$(echo "$pkg" | tr '/' '.')
  L="$LOGDIR/tests-$n-$suf.log"
  leg "T:$pkg:$suf" "tests-$n-$suf" "$EXE" -tests -test-action all -test-timeout "$to" -test-config Release "$@" -go2cspath "$WB\\src" "$GOROOT\\src\\$pb" "$WB\\src\\core\\$pb"
  trc=$LEG_RC; tt0=$LEG_T0
  if [ "$bank" = yes ]; then
    hp treport --dir "$WB\\src\\core\\$pb" --log "$(cygpath -w "$L")" --t0 "$tt0" --rc "$trc" --label "T:$pkg" --named "$named" --bank > "$LOGDIR/tests-$n-$suf.reading.txt" 2>&1
  else
    hp treport --dir "$WB\\src\\core\\$pb" --log "$(cygpath -w "$L")" --t0 "$tt0" --rc "$trc" --label "T:$pkg" --named "$named" > "$LOGDIR/tests-$n-$suf.reading.txt" 2>&1
  fi
  while IFS= read -r l; do stamp "  $l"; done < <(grep -aE 'FRESHNESS|RESULTS-TAIL|VALIDATED-LINE|COMPARISON|ERROR-NAMES|NAMED |BANK-READING' "$LOGDIR/tests-$n-$suf.reading.txt" | cut -c1-500 | head -n 40)
  for f in go2cs_test_comparison.json go2cs_test_results.json go2cs_test_results.xml; do [ -f "src/core/$pkg/$f" ] && cp "src/core/$pkg/$f" "$LOGDIR/tests-$n-$suf.$f"; done
}
tleg runtime/pprof 30m all 'TestGoroutineProfileConcurrency,TestGenericsHashKeyInPprofBuilder,TestGenericsInlineLocations,TestHeapRuntimeFrames,TestBlockMutexProfileInlineExpansion,TestMutexProfile' yes
tleg runtime 150m all 'TestRuntimeLockMetricsAndProfile,TestTracebackParentChildGoroutines,TestFinalizerRegisterABI,TestMemmoveOverflow,TestTracebackGeneric,TestCleanupAfterFinalizer,TestPeriodicGC,TestCrashWhileTracing,TestNumCPU' no
unset MSYS_NO_PATHCONV

stamp "END tracked-changes=$(git status --porcelain | grep -vc '^??') tracked-deletions=$(git status --porcelain | grep -c '^ D') head=$(git rev-parse --short=10 HEAD) SIc=[$SICV] GP=[gen rc=$gprc diff rc=$gpd] GT-NOT-FOUND=[${GT_NF% }] 5-TRUNCATED=[${NMT:-no}]"
stamp "NONZERO LEGS (SIc and NVR are EXPECTED non-zero; T:runtime is a known-failing row): $(grep -aoE 'LEG [^ ]+ rc=[1-9][0-9]*' "$SUM" | sed 's/^LEG //' | tr '\n' ' ')"
stamp "BATTERY DONE"
