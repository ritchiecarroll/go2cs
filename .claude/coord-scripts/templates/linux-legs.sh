#!/usr/bin/env bash
# TEMPLATE: linux legs (P1 / P2) for the next train. Derived from trainL/tL-linux-legs.sh at claude/coord-handover
# 51c5ab0e42 (the file TRAIN L's P1 ran) with exactly three changes, each marked "TEMPLATE FIX n" below:
#   1. LB passes GoTargetOS=linux to the behavioral runner (without it the C# side is the windows flavour).
#   2. The clone check also refuses UNTRACKED files under any path the union tracks (LEGS_CHECK_ONLY=1 runs just it).
#   3. The "HANG/ABORT/CRASH" flag matches the host's crash banner, not Blame's normal completion line.
# The seat list, fixup gate, GolibTests class filter and row list below are still TRAIN L's: edit them per train.
# TRAIN L linux legs (P1 / P2) at the TRAIN L union. Brief: tL-lane-brief-linux.md (same folder).
# DRAFT 2026-10-01 from tK-linux-legs.sh (env, leg/buildconv/purge/summ/tleg/gt VERBATIM). NOT RUN by its author.
# DRAFT 2 (CHANGES.md): P1 adds LPB (cmp -test-publish-binlog, polled; critic 0 #3); every fresh record's stderr tails
# are counted for deadlock lines (critic 0 #16); LB's restore reads NUL-separated paths and its rc (critic 1 #11).
# Save this file OUTSIDE your build clone and run it IN THE BACKGROUND (one reading at a time on the box):
#   LANE=P1 UNION=<full union SHA from COORD> W=<your build clone> GOROOT=<go1.24.13 root> [FIXUP=<fixup SHA>] bash ~/linux-legs.sh
# Dry check of the clone only (no checkout, no legs): LEGS_CHECK_ONLY=1 on the same command line.
# Legs run SEQUENTIALLY. Every leg's rc is captured BEFORE any pipe (floor 7). Evidence lands in $R, outside the clone.
# The clone is only READ except for what the -tests pipeline and the behavioral runner write (restored after each row)
# and build output (purged between heavy legs). Nothing is committed or pushed by this script.
# P1: GolibTests x2 + the L classes; LCn/LCf the converter suite (r-m2/m3/m6: no linux reading exists); rows reflect fmt
#     internal/fmtsort encoding/json sync net/rpc (c1-token's linux re-read at the union; g-deadlock's net/rpc); LB the
#     g-deadlock + r-d6 + i9-crosspkg behavioral projects on linux (exit 2 for the main-alone pair).
# P2: GolibTests Release + the L classes (second box); runtime/pprof once; the FULL runtime row; TBS-LOOP x10 under load.
set -u
LANE=${LANE:?set LANE=P1 or LANE=P2}
UNION=${UNION:?set UNION to the full union SHA COORD posted}
W=${W:?set W to your build clone}
GOROOT=${GOROOT:?set GOROOT to the go1.24.13 root, spelled exactly as go env GOROOT prints it}
R=${R:-$HOME/tL-linux-$LANE}
FLOOR_GB=${FLOOR_GB:-15}
TBS_N=${TBS_N:-10}               # P2: TestTracebackSystem iterations
TBS_LOAD=${TBS_LOAD:-1}          # P2: 1 = run the iterations beside nproc busy loops (K's red appeared only under load)
case "$LANE" in P1|P2) ;; *) echo "LANE must be P1 or P2"; exit 2;; esac
mkdir -p "$R" || exit 2
SUM="$R/SUMMARY.txt"
stamp(){ echo "$(date -u '+%Y-%m-%d %H:%M:%SZ') $*" | tee -a "$SUM"; }
abort(){ stamp "ABORT: $*"; exit 2; }
freegb(){ df -BG --output=avail "$W" | tail -n 1 | tr -dc '0-9'; }

# --- 0. the clone: clean, then the union, detached -------------------------------------------------------------
cd "$W" || abort "no clone at $W"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || abort "tracked changes in $W before checkout (stash or reset them yourself first)"
git fetch -q origin claude/coord-trainL-union || abort "fetch of claude/coord-trainL-union failed"
[ "$(git rev-parse FETCH_HEAD)" = "$UNION" ] || abort "origin claude/coord-trainL-union is $(git rev-parse --short=10 FETCH_HEAD), not $UNION (ask COORD before reading anything)"
# TEMPLATE FIX 2: the check above counts TRACKED changes only. Earlier -tests runs leave UNTRACKED files under tracked
# paths (P2's first TRAIN L launch failed its checkout on 33 of them), and a leftover that does not collide still
# pollutes the tree the reading is taken in. Refuse any untracked, non-ignored file whose path the union tracks or that
# sits under a directory the union tracks, naming each (build output is ignored and is not counted).
UTLIST=$(python3 - "$UNION" <<'PY'
import subprocess, sys
def git(*a): return subprocess.run(["git", *a], check=True, capture_output=True).stdout.decode("utf-8", "surrogateescape")
tracked = [p for p in git("ls-tree", "-r", "--name-only", "-z", sys.argv[1]).split("\0") if p]
untracked = [p for p in git("ls-files", "--others", "--exclude-standard", "-z").split("\0") if p]
files, dirs = set(tracked), set()
for p in tracked:
    d = p
    while "/" in d:
        d = d.rsplit("/", 1)[0]
        if d in dirs: break
        dirs.add(d)
bad = []
for p in untracked:
    if p in files:
        bad.append((p, "a path the union tracks: the checkout would fail"))
        continue
    d = p
    while "/" in d:
        d = d.rsplit("/", 1)[0]
        if d in dirs:
            bad.append((p, "under the directory " + d + ", which the union tracks"))
            break
for p, why in bad[:40]: print("  untracked " + p + " (" + why + ")")
if len(bad) > 40: print("  ... and %d more" % (len(bad) - 40))
sys.exit(1 if bad else 0)
PY
); utrc=$?
[ $utrc = 0 ] || abort "untracked files under paths the union tracks in $W (clean them yourself first, by exact path):
$UTLIST"
if [ "${LEGS_CHECK_ONLY:-}" = 1 ]; then stamp "CHECK-ONLY ok: $W has no tracked changes and no untracked files under the union's tracked paths; stopping before the checkout"; exit 0; fi
git checkout -q --detach "$UNION" || abort "checkout $UNION failed"
[ "$(git rev-parse HEAD)" = "$UNION" ] || abort "HEAD is not $UNION"
# TRAIN K's landed master plus every L seat this brief reads (the full list is tL-seats.txt on claude/coord-handover):
# c1-token-ids, g-deadlock-checkdead, r-m2-onK, r-m3, r-m4, r-m6, r-d6, i9-crosspkg
for s in 75648a022b 667d052869 0377dbff39 27f2199b30 c5e934c464 e9009f2945 6a079a9675 33fb0565f6 8213221317; do
  git merge-base --is-ancestor "$s" HEAD || abort "seat $s is not an ancestor of the union"
done
# The TRAIN L FIXUP must be in the union (two behavioral csproj cut at J's template, + the two D4 rows).
if [ -n "${FIXUP:-}" ]; then
  git merge-base --is-ancestor "$FIXUP" HEAD || abort "FIXUP $FIXUP is not HEAD or an ancestor of the union"
fi
nfix=$(git log --format=%s 75648a022b..HEAD | grep -c '^fixup: TRAIN L')
[ "$nfix" -ge 1 ] || abort "no 'fixup: TRAIN L' commit between 75648a022b and the union (the union lacks the L fixup)"
git grep -l -F '<LangVersion>latest</LangVersion>' HEAD -- 'src/core/**/*.csproj' 'src/tests/Behavioral/**/*.csproj' \
    'src/tests/Performance/**/*.csproj' > "$R/langversion-latest.txt"; rc=$?
[ $rc = 0 ] || [ $rc = 1 ] || abort "git grep (LangVersion latest) rc=$rc"
nlv=$(wc -l < "$R/langversion-latest.txt")
[ "$nlv" = 0 ] || abort "union lacks the L fixup: $nlv csproj still carry <LangVersion>latest</LangVersion> (langversion-latest.txt)"

# --- toolchain: go1.24.13 pinned, Microsoft .NET 10.0.12, GoTargetOS NOT exported (K verbatim) ----------------
case "$GOROOT" in /*) ;; *) abort "GOROOT is not an absolute path: $GOROOT";; esac
case "$GOROOT" in *\\*|*/) abort "GOROOT has a backslash or a trailing slash: $GOROOT";; esac
[ -f "$GOROOT/VERSION" ] || abort "no VERSION file under GOROOT ($GOROOT)"
head -n 1 "$GOROOT/VERSION" | grep -qx 'go1\.24\.13' || abort "GOROOT's VERSION reads '$(head -n 1 "$GOROOT/VERSION")', not go1.24.13"
export GOROOT GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export PATH="$GOROOT/bin:$PATH"
unset GoTargetOS
# r-m4: the converter sets GO2CS_MODULE_ROOT per host child for module packages only; never inherit one.
unset GO2CS_MODULE_ROOT
[ "$(go env GOROOT)" = "$GOROOT" ] || abort "go env GOROOT reads $(go env GOROOT), not $GOROOT (floor 6)"
go version | grep -q 'go1\.24\.13 ' || abort "go is $(go version), not go1.24.13"
ENVL=$(bash "$W/docs/phase4/recipes/microsoft-dotnet/use-ms-dotnet.sh") || abort "use-ms-dotnet.sh failed"
eval "$ENVL"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 MSBUILDDISABLENODEREUSE=1
[ "$(command -v dotnet)" = "$DOTNET_ROOT/dotnet" ] || abort "dotnet resolves to $(command -v dotnet), not $DOTNET_ROOT/dotnet"
dotnet build-server shutdown > "$R/build-server-shutdown.log" 2>&1
SDKV=$(cd "$W" && dotnet --version 2>&1); rc=$?
[ $rc = 0 ] || abort "dotnet --version in the clone failed under the union's global.json (S1 pin): $SDKV"
[ "$(env | grep -c '^GoTargetOS=')" = 0 ] || abort "GoTargetOS is still exported"
stamp "PRE lane=$LANE uid=$(id -u) head=$(git rev-parse --short=10 HEAD) tree=$(git rev-parse --short=10 'HEAD^{tree}') $(go version | cut -d' ' -f3-4) sdk=$SDKV DOTNET_ROOT=$DOTNET_ROOT nproc=$(nproc) free=$(freegb)G"

leg(){ # name, log-suffix, outer-timeout, command...   (K verbatim)
  local name=$1 suf=$2 tmo=$3; shift 3
  local L="$R/$suf.log" t0 f rc
  f=$(freegb); [ "${f:-0}" -ge "$FLOOR_GB" ] || { stamp "LEG $name ABORT: free disk ${f}G < ${FLOOR_GB}G"; exit 3; }
  t0=$(date +%s)
  timeout -k 120 "$tmo" "$@" > "$L" 2>&1 < /dev/null; rc=$?
  stamp "LEG $name rc=$rc wall=$(( $(date +%s) - t0 ))s free=$(freegb)G log=$suf.log"
  return $rc
}
EXE="$W/src/go2cs/bin/go2cs"
CONV0=
buildconv(){ # label   (K verbatim)
  local rc sha
  (cd "$W/src/go2cs" && go build -o bin/go2cs .) > "$R/conv-build-$1.log" 2>&1; rc=$?
  [ $rc = 0 ] && [ -x "$EXE" ] || abort "converter build ($1) rc=$rc"
  sha=$(sha256sum "$EXE" | cut -c1-16)
  [ -z "$CONV0" ] && CONV0=$sha
  stamp "BUILD converter ($1) sha=$sha$( [ "$sha" = "$CONV0" ] || echo " DIFFERS from the first build $CONV0")"
}
purge(){ # label   (K verbatim)
  local P left td
  P=$(find "$W/src" -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  find "$W/src" -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
  left=$(find "$W/src" -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
  td=$(git -C "$W" status --porcelain | grep -c '^ D')
  stamp "PURGE($1) purged=$P remaining=$left tracked-deletions=$td free=$(freegb)G"
  [ "$left" = 0 ] && [ "$td" = 0 ] || abort "purge incomplete or tracked deletions (floor 8)"
  buildconv "after-$1"
}
FRESH_FAILS=0
summ(){ # comparison.json manifest.json   (K verbatim)
  python3 - "$1" "$2" "$R/converter-revision.txt" <<'PY'
import json, os, sys
c = json.load(open(sys.argv[1]))
try: m = json.load(open(sys.argv[2]))
except Exception: m = {}
e = c.get("environment", {})
fails = []
if "go1.24.13" not in (e.get("oracleGoVersion") or ""): fails.append("oracleGoVersion=%r" % e.get("oracleGoVersion"))
if e.get("configuration") != "Release": fails.append("configuration=%r" % e.get("configuration"))
if e.get("tiered") is not False: fails.append("tiered=%r" % e.get("tiered"))
if m.get("targetGOOS") != "linux": fails.append("targetGOOS=%r" % m.get("targetGOOS"))
rev, rf = m.get("converterRevision"), sys.argv[3]
if not rev: fails.append("converterRevision missing")
elif os.path.exists(rf):
    first = open(rf).read().strip()
    if rev != first: fails.append("converterRevision=%s differs from the first row's %s" % (rev, first))
else: open(rf, "w").write(rev + "\n")
print("      record: status=%s matched=%s go=%d cs=%d skipped=%d disclosed=%d excluded=%d errors=%d orphans=%d config=%s tiered=%s oracle=%s goos=%s conv=%s" % (
  c.get("status"), c.get("matched"), len(c.get("go") or {}), len(c.get("csharp") or {}), len(c.get("skipped") or []),
  len(c.get("disclosed") or []), len(c.get("excluded") or []), len(c.get("errors") or []), len(c.get("orphanedDisclosures") or []),
  e.get("configuration"), e.get("tiered"), (e.get("oracleGoVersion") or "?").replace("go version ", ""), m.get("targetGOOS"), m.get("converterRevision")))
for x in (c.get("errors") or [])[:12]: print("      ERR " + x[:180])
for o in (c.get("orphanedDisclosures") or [])[:6]: print("      ORPHAN " + json.dumps(o)[:180])
for f in fails: print("      FRESHNESS FAIL: " + f)
sys.exit(1 if fails else 0)
PY
}
named(){ # comparison.json name... -- each named test's verdicts and whether it is disclosed (L addition)
  python3 - "$@" <<'PY'
import json, sys
c = json.load(open(sys.argv[1])); g, s = c.get("go") or {}, c.get("csharp") or {}
disc = {d.split(" (")[0] for d in c.get("disclosed") or []}
for n in sys.argv[2:]:
    ks = sorted(k for k in set(g) | set(s) if k == n or k.startswith(n + "/"))
    if not ks: print("      NAMED %s: absent from both sides" % n); continue
    for k in ks[:8]: print("      NAMED %s: go=%s cs=%s%s" % (k, g.get(k), s.get(k), " disclosed" if k in disc else ""))
PY
}
recdl(){ # comparison.json -> deadlock lines in its stderr tails (go + csharp); critic 0 #16 (DRAFT 2)
  python3 - "$1" <<'PY'
import json, sys
try: st = json.load(open(sys.argv[1])).get("stderr") or {}
except Exception: print("unreadable"); sys.exit(0)
print(sum(((st.get(k) or {}).get("text") or "").count("all goroutines are asleep") for k in ("go", "csharp")))
PY
}
NAMED_NOW=''   # tleg reads these names from the fresh record when set (L addition)
tleg(){ # pkg, per-child -test-timeout, outer timeout, suffix, extra converter args...   (K verbatim + NAMED_NOW + deadlock count)
  local pkg=$1 to=$2 otm=$3 suf=$4; shift 4
  local n out C M L changed src
  n=$(echo "$pkg" | tr '/' '.'); out="$W/src/core/$pkg"; C="$out/go2cs_test_comparison.json"; M="$out/go2cs_test_manifest.json"
  rm -f "$C" "$out/go2cs_test_results.json" "$out/go2cs_test_results.xml"
  touch "$R/.t0-$n-$suf"
  # floor 3: the output directory is the SECOND positional. -platforms defaults to this host (linux/amd64).
  leg "T:$pkg:$suf" "tests-$n-$suf" "$otm" "$EXE" -tests -test-action all -test-timeout "$to" -test-config Release "$@" \
      -go2cspath "$W/src" "$GOROOT/src/$pkg" "$out"
  L="$R/tests-$n-$suf.log"
  stamp "  T:$pkg:$suf: $(grep -aE 'Validated [0-9]+ tests|comparison failed' "$L" | tail -n 1 | cut -c1-400) :: deadlock-lines=$(grep -ac 'all goroutines are asleep' "$L")"
  if [ -f "$C" ] && [ "$C" -nt "$R/.t0-$n-$suf" ]; then
    cp "$C" "$R/tests-$n-$suf.comparison.json"; [ -f "$M" ] && cp "$M" "$R/tests-$n-$suf.manifest.json"
    summ "$C" "$M" > "$R/.summ-$n-$suf" 2>&1; src=$?     # floor 7: rc before any pipe
    cat "$R/.summ-$n-$suf" >> "$SUM"; cat "$R/.summ-$n-$suf"
    if [ $src != 0 ]; then FRESH_FAILS=$((FRESH_FAILS + 1)); stamp "      FRESHNESS FAIL on $pkg:$suf: this record is NOT a reading at the pinned toolchain"; fi
    if [ -n "$NAMED_NOW" ]; then named "$C" $NAMED_NOW > "$R/.named-$n-$suf" 2>&1; cat "$R/.named-$n-$suf" >> "$SUM"; cat "$R/.named-$n-$suf"; fi
    stamp "      deadlock lines in the record's stderr tails (go + csharp): $(recdl "$C")"
  else
    stamp "      NO FRESH COMPARISON RECORD (the run did not reach its compare; read the log tail)"
  fi
  git -C "$W" diff --stat -- "src/core/$pkg" > "$R/tests-$n-$suf.srcdiff.txt"
  git -C "$W" diff -U0 -- "src/core/$pkg" > "$R/tests-$n-$suf.srcdiff.patch"
  changed=$(git -C "$W" status --porcelain -- "src/core/$pkg" | grep -vc '^??')
  stamp "      tracked files the run rewrote under src/core/$pkg: $changed (csproj among them: $(grep -c 'csproj' "$R/tests-$n-$suf.srcdiff.txt"))"
  git -C "$W" checkout -q -- "src/core/$pkg"
  [ "$(git -C "$W" status --porcelain | grep -c '^ D')" = 0 ] || abort "tracked deletions after restoring src/core/$pkg"
}
# GolibTests classes the L seats add or change (c1-token, g-deadlock, r-m4, r-m6 + the SyncMutexProfile pair):
GTL='FullyQualifiedName~PointerTokenUniquenessTests|FullyQualifiedName~AliasOverlapTests|FullyQualifiedName~ReflectHashTokenBandTests|FullyQualifiedName~RuntimePinnerTests|FullyQualifiedName~FieldPointerEqualityTests|FullyQualifiedName~FieldRefTokenTests|FullyQualifiedName~ForeverWaitDeadlockDecisionTests|FullyQualifiedName~GoroutineProfileInstantTests|FullyQualifiedName~GoroutineParkAccountingTests|FullyQualifiedName~RuntimeParkTransitionTests|FullyQualifiedName~BubbledChannelTests|FullyQualifiedName~ModuleAncestryTests|FullyQualifiedName~SyncMutexProfileTests|FullyQualifiedName~SkipCountedWalkerFrameTests|FullyQualifiedName~ParseDebugVarsAtStartTests|FullyQualifiedName~InternalCpuGodebugTests|FullyQualifiedName~NilPanicHookTests|FullyQualifiedName~JunctionGodebugToolchainChildTests'
GP="$W/src/tests/GolibTests/GolibTests.csproj"
gtbuild(){ # cfg   (K verbatim)
  leg "GT-build-$1" "gt-build-$1" 60m env GoTargetOS=linux dotnet build "$GP" -c "$1" -p:GoTargetOS=linux -p:UseSharedCompilation=false
}
gt(){ # cfg, tag, extra dotnet-test args...   (K verbatim)
  local cfg=$1 tag=$2 L; shift 2
  leg "GT-$cfg-$tag" "gt-$cfg-$tag" 90m env GoTargetOS=linux dotnet test "$GP" -c "$cfg" --no-build -p:GoTargetOS=linux \
      --blame-hang-timeout 20m --blame-hang-dump-type none --logger 'console;verbosity=normal' "$@"
  L="$R/gt-$cfg-$tag.log"
  stamp "  GT-$cfg-$tag: $(grep -aE '(Passed|Failed)!' "$L" | tail -n 1 | cut -c1-200)"
  stamp "      failed: $(grep -aE '^\s+Failed [A-Za-z]' "$L" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-400)"
  stamp "      skipped: $(grep -aE '^\s+Skipped [A-Za-z]' "$L" | sed 's/^ *Skipped //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-400)"
  crashbanner "$L" && stamp "      HANG/ABORT/CRASH text present in the log: read it"
}
crashbanner(){ # log -> rc 0 when the host's abort/crash/hang banner is present   (TEMPLATE FIX 3)
  # The old pattern matched the bare word Blame, which every passing run prints as "Data collector 'Blame' message: All
  # tests finished running, Sequence file will not be generated.": a false flag on 3 of 3 clean TRAIN L GolibTests logs.
  # Match the two banners and any Blame message that is not that normal completion line (a hang or a dump).
  { grep -aE 'The active test run was aborted|Test host process crashed' "$1"
    grep -a "Data collector 'Blame' message:" "$1" | grep -av 'All tests finished running'; } | grep -aq .
}

buildconv first
if [ "$LANE" = P1 ]; then
  # L1 -- the quick STOP gate (K's): a linux -tests row with GoTargetOS unset validates 14 under the union.
  tleg unicode/utf8 10m 240m plain
  grep -aqE 'Validated 14 tests' "$R/tests-unicode.utf8-plain.log" || abort "L1 unicode/utf8 did not validate 14: STOP and post"
  [ "$FRESH_FAILS" = 0 ] || abort "L1 unicode/utf8 record failed its freshness checks: STOP and post"
  purge after-L1
  # L2 -- GolibTests Release and Debug, FULL, then the L classes by name (c1-token, g-deadlock, r-m4, r-m6)
  for cfg in Release Debug; do
    gtbuild "$cfg" && gt "$cfg" full
  done
  gt Release lseats --filter "$GTL"
  purge after-GT
  # LCn -- r-m2/m3/m4/m6's converter tests BY NAME on linux (-v; '--- PASS' each, no SKIP): M2's non-windows file://
  # GOPROXY URL, M3's non-.exe driver binary, M6's go list oracle under the go1.24.13 pin. LCf -- the full suite, a
  # FIRST linux reading (no K linux leg ran go test in src/go2cs): a red outside the named set is reported, not chased.
  CM='TestModuleCacheEndToEndLockAndClash|TestModuleCachePreflightNamesMissingDependency|TestModuleCacheOutsideGopathResolvesAsCacheDependency|TestModulesLockRoundTripAndClash|TestTestsRecurseConvertsAModuleAgainstItsTests|TestTestsRecurseRefusesANewerGoLineAndAnInModuleRoot|TestTestClosureDiscoveryAddsNoOrderEdges|TestTestHostCarriesTheLogicalModulePathOnly|TestTestHostModuleEnvironmentIsForModulePackagesOnly|TestWriteTestHostUsesCSharpClassOverride|TestFixtureDirectoriesStagePackageShape|TestDefaultGODEBUGMatchesTheToolchain|TestRecurseMainProgramCarriesItsDefaultGODEBUG|TestModuleGodebugReadsTheGodebugBlockStrictly'
  leg LCn conv-named 45m bash -c "cd '$W/src/go2cs' && go test -count=1 -timeout 40m -v -run '^($CM)\$' ."
  miss=''; for t in $(echo "$CM" | tr '|' ' ') TestDefaultGODEBUGMatchesTheToolchain/go120 TestDefaultGODEBUGMatchesTheToolchain/go123 TestDefaultGODEBUGMatchesTheToolchain/go124 TestDefaultGODEBUGMatchesTheToolchain/block TestDefaultGODEBUGMatchesTheToolchain/directive; do
    grep -aqE -- "--- PASS: $t \(" "$R/conv-named.log" || miss="$miss $t"; done
  stamp "  LCn: not-PASS=[${miss# }] skip/fail-lines=$(grep -acE -- '--- (SKIP|FAIL):' "$R/conv-named.log") (19 names incl. 5 subtests)"
  leg LCf conv-full 60m bash -c "cd '$W/src/go2cs' && go test -count=1 -timeout 50m ./..."
  stamp "  LCf: $(grep -aE '^(ok|FAIL|---)' "$R/conv-full.log" | tr '\n' ' ' | cut -c1-500)"
  stamp "      tracked changes after LC: $(git -C "$W" status --porcelain | grep -vc '^??') (expect 0)"
  purge after-LC
  # L4 -- rows: c1-token-ids' linux re-read AT THE UNION (its '0 moved' was read at f0d257686f, before K's
  # r-field-ptr-equality joined ж.FieldRefBox.cs): reflect fmt internal/fmtsort encoding/json sync; g-deadlock: net/rpc.
  for pkg in reflect fmt internal/fmtsort encoding/json sync; do tleg "$pkg" 30m 240m row; done
  NAMED_NOW='TestSendDeadlock'; tleg net/rpc 30m 240m row; NAMED_NOW=''
  # log/slog AT TC0 (COORD 2026-10-01): G measured windows TC0 Validated 199 with TestCallDepth 10/10; green here retires
  # its 'execution: release-tiered' in L's roster commit.
  NAMED_NOW='TestCallDepth'; tleg log/slog 30m 240m row; NAMED_NOW=''
  # LPB (DRAFT 2, critic 0 #3) -- c2-publish-binlog-onK's composed claim on linux: the test host's publish carries BOTH
  # -p:GoTargetOS=linux (the record's targetGOOS, checked by summ) and -bl (the binlog EXISTS during the run: a 1 s
  # background poll; a passing publish then deletes it). EXPECT rc 0, the binlog SEEN, absent afterwards, record fresh.
  BL="$W/src/core/cmp/bin/tests/publish.binlog"; rm -f "$R/.lpb-seen"
  # Verifier (same fix as the i7's SPB/PB): a leftover binlog would read SEEN before this leg's publish starts, so record
  # it (pre=PRESENT means SEEN cannot be trusted; post it) and delete it before the poll starts.
  lpbpre=$([ -e "$BL" ] && echo PRESENT || echo absent); rm -f "$BL"
  ( while :; do [ -e "$BL" ] && : > "$R/.lpb-seen"; sleep 1; done ) > /dev/null 2>&1 & LPBPID=$!
  trap '[ -n "${LPBPID:-}" ] && kill "$LPBPID" 2>/dev/null' EXIT   # an abort inside tleg must not orphan the poll (killed by PID)
  tleg cmp 10m 60m binlog -test-publish-binlog
  lpbrc=$(grep -aoE 'LEG T:cmp:binlog rc=[0-9]+' "$SUM" | tail -n 1 | sed 's/.*rc=//')
  kill "$LPBPID" 2>/dev/null; wait "$LPBPID" 2>/dev/null; LPBPID=''; trap - EXIT
  stamp "  LPB (EXPECT rc 0, pre=absent, binlog SEEN during, absent after): rc=$lpbrc pre=$lpbpre during=$([ -e "$R/.lpb-seen" ] && echo SEEN || echo never-seen) after=$([ -e "$BL" ] && echo PRESENT || echo absent)"
  purge after-L4
  # LB -- the behavioral projects on linux through the runner run-behavioral.ps1 wraps (no pwsh here). g-deadlock: the
  # 3 new projects + ChannelReceiveFromNil/ChannelSendToNil (now Output-compared: stderr first line
  # 'fatal error: all goroutines are asleep - deadlock!', exit 2 on both sides); r-d6 ForClauseSpill; i9-crosspkg
  # CrossPackagePromotedValueMethod. The runner's output folder (bin/Debug/net*) is INFERRED from run-behavioral.ps1:
  # a missing executable is NOT MEASURED (false-green route #6), never a pass.
  BR="$W/src/tests/Behavioral/BehavioralRunner"
  leg LB-build behav-runner-build 30m env GoTargetOS=linux dotnet build "$BR/BehavioralRunner.csproj" -c Debug   # TEMPLATE FIX 1 (as the runs)
  BRX=$(ls "$BR"/bin/Debug/net*/BehavioralRunner 2>/dev/null | head -n 1)
  # TEMPLATE FIX 1: the runner must see GoTargetOS=linux. The driver unsets it for the -tests rows (p1-tests-goos passes it
  # itself), but the runner has no such seam: without it the C# side is the windows flavour (kernel32.dll DllNotFound,
  # exit 2 vs 0 on 6 of 7 projects at TRAIN L). Set per command with env, so GoTargetOS stays unexported in the driver.
  lbcmd(){ printf "cd '%s/src/tests/Behavioral' && env GoTargetOS=linux '%s' --filter '%s'" "$W" "$BRX" "$1"; }
  if [ -n "$BRX" ] && [ -x "$BRX" ]; then
    for p in ForeverWaitWorkersMainReturns MainSelectForeverWorkerExits MainSelectForeverAfterFunc ChannelReceiveFromNil ChannelSendToNil ForClauseSpill CrossPackagePromotedValueMethod; do
      stamp "  LB command: bash -c \"$(lbcmd "$p")\""
      leg "LB:$p" "behav-$p" 60m bash -c "$(lbcmd "$p")"
      stamp "  LB:$p: $(grep -aE '(Transpile|Compile|Target|Output).*(pass|fail)|compared|exit code' "$R/behav-$p.log" | tr -s ' ' | tr '\n' ' ' | cut -c1-300)"
    done
    # DRAFT 2 (critic 1 #11's parse fix, applied here too): NUL-separated names from git, the restore's rc read.
    git -C "$W" diff --name-only -z HEAD > "$R/lb-rewrites.paths.z"
    stamp "      tracked changes after LB: $(tr -cd '\0' < "$R/lb-rewrites.paths.z" | wc -c) (expect 0; any are saved to lb-rewrites.patch, then restored)"
    if [ -s "$R/lb-rewrites.paths.z" ]; then
      git -C "$W" diff HEAD > "$R/lb-rewrites.patch"
      git -C "$W" restore --source=HEAD --staged --worktree --pathspec-from-file="$R/lb-rewrites.paths.z" --pathspec-file-nul; lrc=$?
      [ "$lrc" = 0 ] && [ "$(git -C "$W" status --porcelain | grep -vc '^??')" = 0 ] || abort "LB restore rc=$lrc left tracked changes: STOP and post"
    fi
  else
    stamp "  LB: NOT MEASURED -- no BehavioralRunner executable under $BR/bin/Debug/net*/ (read behav-runner-build.log)"
  fi
  purge after-LB
else
  # L2 -- GolibTests Release, FULL and PLAIN, then the L classes by name (the second box)
  gtbuild Release || abort "GolibTests Release build failed"
  gt Release full
  gt Release lseats --filter "$GTL"
  purge after-GT
  # L5 -- runtime/pprof once (banked linux: 147 + 7): g-deadlock's AllGoroutinesForeverBlocked takes s_profileGate
  # EXCLUSIVE, the goroutine profile's gate; c1-token's token path.
  NAMED_NOW='TestGoroutineProfileConcurrency TestBlockProfile'; tleg runtime/pprof 30m 240m row; NAMED_NOW=''
  purge after-pprof
  # L6 -- the FULL linux runtime row (banked linux: 10810 + 73). g-deadlock: TestTracebackSystem/panic and /trap must read
  # DISCLOSED-divergent (not an undisclosed red), and the crash family pass/pass; c1-token: the row 'over 10,827 reached'.
  NAMED_NOW='TestTracebackSystem TestSimpleDeadlock TestInitDeadlock TestLockedDeadlock TestLockedDeadlock2 TestGoexitDeadlock TestGoNil TestMainGoroutineID TestNoHelperGoroutines TestPanicDeadlockGosched TestPanicDeadlockSyscall TestStopTheWorldDeadlock'
  tleg runtime 150m 330m row
  NAMED_NOW=''
  purge after-runtime
  # TBS-LOOP -- g-deadlock: K's linux final head read /panic RED on a loaded box (the 200 ms race on the child's
  # 'go child(); select {}'). TBS_N iterations of the filtered row (DIAGNOSTIC ONLY: a gated run never banks), each
  # reading /panic and /trap disclosed-divergent on the pinned signature and 0 deadlock lines. TBS_LOAD=1 runs nproc
  # busy loops beside them (INFERRED load shape; K's red came from real concurrent work), killed by PID (floor 5).
  LOADPIDS=''
  if [ "$TBS_LOAD" = 1 ]; then
    for i in $(seq "$(nproc)"); do ( while :; do :; done ) & LOADPIDS="$LOADPIDS $!"; done
    trap '[ -n "$LOADPIDS" ] && kill $LOADPIDS 2>/dev/null' EXIT
    stamp "TBS load: $(echo $LOADPIDS | wc -w) busy loops (PIDs$LOADPIDS)"
  fi
  NAMED_NOW='TestTracebackSystem'
  for i in $(seq "$TBS_N"); do tleg runtime 30m 90m "tbs$i" -test-filter '^TestTracebackSystem$'; done
  NAMED_NOW=''
  [ -n "$LOADPIDS" ] && kill $LOADPIDS 2>/dev/null; LOADPIDS=''
  stamp "TBS-LOOP: iterations=$TBS_N load=$TBS_LOAD; deadlock-lines per iteration (log+record-stderr): $(for i in $(seq "$TBS_N"); do printf '%s+%s ' "$(grep -ac 'all goroutines are asleep' "$R/tests-runtime-tbs$i.log")" "$([ -f "$R/tests-runtime-tbs$i.comparison.json" ] && recdl "$R/tests-runtime-tbs$i.comparison.json" || echo no-record)"; done)"
  purge after-tbs
fi
stamp "END freshness-fails=$FRESH_FAILS head=$(git -C "$W" rev-parse --short=10 HEAD) tracked-changes=$(git -C "$W" status --porcelain | grep -vc '^??') tracked-deletions=$(git -C "$W" status --porcelain | grep -c '^ D')"
stamp "LINUX LEGS DONE ($LANE, TRAIN L)"
