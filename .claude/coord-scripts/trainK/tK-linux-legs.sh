#!/usr/bin/env bash
# TRAIN K linux legs (P1 / P2) at the TRAIN K union. Brief: tK-lane-brief-linux.md (same folder).
# Save this file OUTSIDE your build clone and run it IN THE BACKGROUND (one reading at a time on the box):
#   LANE=P1 UNION=<full union SHA from COORD> W=<your build clone> GOROOT=<go1.24.13 root> [FIXUP=<fixup SHA>] bash ~/tK-linux-legs.sh
# Legs run SEQUENTIALLY. Every leg's rc is captured BEFORE any pipe (floor 7). Evidence lands in $R, outside the clone.
# The clone is only READ except for what the -tests pipeline writes under src/core/<pkg> (restored after each row)
# and build output (purged between heavy legs). Nothing is committed or pushed by this script.
set -u
LANE=${LANE:?set LANE=P1 or LANE=P2}
UNION=${UNION:?set UNION to the full union SHA COORD posted}
W=${W:?set W to your build clone}
GOROOT=${GOROOT:?set GOROOT to the go1.24.13 root, spelled exactly as go env GOROOT prints it}
R=${R:-$HOME/tK-linux-$LANE}
FLOOR_GB=${FLOOR_GB:-15}
case "$LANE" in P1|P2) ;; *) echo "LANE must be P1 or P2"; exit 2;; esac
mkdir -p "$R" || exit 2
SUM="$R/SUMMARY.txt"
stamp(){ echo "$(date -u '+%Y-%m-%d %H:%M:%SZ') $*" | tee -a "$SUM"; }
abort(){ stamp "ABORT: $*"; exit 2; }
freegb(){ df -BG --output=avail "$W" | tail -n 1 | tr -dc '0-9'; }

# --- 0. the clone: clean, then the union, detached -------------------------------------------------------------
cd "$W" || abort "no clone at $W"
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || abort "tracked changes in $W before checkout (stash or reset them yourself first)"
git fetch -q origin claude/coord-trainK-union || abort "fetch of claude/coord-trainK-union failed"
[ "$(git rev-parse FETCH_HEAD)" = "$UNION" ] || abort "origin claude/coord-trainK-union is $(git rev-parse --short=10 FETCH_HEAD), not $UNION (ask COORD before reading anything)"
git checkout -q --detach "$UNION" || abort "checkout $UNION failed"
[ "$(git rev-parse HEAD)" = "$UNION" ] || abort "HEAD is not $UNION"
# TRAIN J's landed master plus every seat this brief reads (the full list is tK-seats.txt on claude/coord-handover)
for s in f819887fa3 272afeef93 1176230dec 38afcb5a4b 6c1213399e a01a8d958f 05bdd5abf0 f0d257686f a11e95f46f 346b26c81f \
         3b8a93ddb3 2051786a8b 7c0fca34d7 1d09813887 0aef70f524 437e6a8f57 ed7859d20e b5db4b2279 b9c8948630 5cf6a0ea56; do
  git merge-base --is-ancestor "$s" HEAD || abort "seat $s is not an ancestor of the union"
done
# The TRAIN K FIXUP must be in the union: without it every -tests row re-emits its committed test csproj from the S1
# template, and 'csproj rewritten' would read nonzero on every row. Optional FIXUP=<sha>, plus two independent checks.
if [ -n "${FIXUP:-}" ]; then
  git merge-base --is-ancestor "$FIXUP" HEAD || abort "FIXUP $FIXUP is not HEAD or an ancestor of the union"
fi
nfix=$(git log --format=%s f819887fa3..HEAD | grep -c '^fixup: TRAIN K')
[ "$nfix" -ge 1 ] || abort "no 'fixup: TRAIN K' commit between f819887fa3 and the union (the union lacks the S1 fixup)"
git grep -l -F '<LangVersion>latest</LangVersion>' HEAD -- 'src/core/**/*.csproj' 'src/tests/Behavioral/**/*.csproj' \
    'src/tests/Performance/**/*.csproj' > "$R/langversion-latest.txt"; rc=$?
[ $rc = 0 ] || [ $rc = 1 ] || abort "git grep (LangVersion latest) rc=$rc"
nlv=$(wc -l < "$R/langversion-latest.txt")
[ "$nlv" = 0 ] || abort "union lacks the S1 fixup: $nlv csproj still carry <LangVersion>latest</LangVersion> (langversion-latest.txt)"

# --- toolchain: go1.24.13 pinned, Microsoft .NET 10.0.12, GoTargetOS NOT exported --------------------------------
# floor 6, on the value itself before it is exported (go env GOROOT after the export only echoes it back):
case "$GOROOT" in /*) ;; *) abort "GOROOT is not an absolute path: $GOROOT";; esac
case "$GOROOT" in *\\*|*/) abort "GOROOT has a backslash or a trailing slash: $GOROOT";; esac
[ -f "$GOROOT/VERSION" ] || abort "no VERSION file under GOROOT ($GOROOT)"
head -n 1 "$GOROOT/VERSION" | grep -qx 'go1\.24\.13' || abort "GOROOT's VERSION reads '$(head -n 1 "$GOROOT/VERSION")', not go1.24.13"
export GOROOT GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export PATH="$GOROOT/bin:$PATH"
unset GoTargetOS
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
stamp "PRE lane=$LANE uid=$(id -u) head=$(git rev-parse --short=10 HEAD) tree=$(git rev-parse --short=10 'HEAD^{tree}') $(go version | cut -d' ' -f3-4) sdk=$SDKV DOTNET_ROOT=$DOTNET_ROOT free=$(freegb)G"

leg(){ # name, log-suffix, outer-timeout, command...
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
buildconv(){ # label -- the purge removes every src/**/bin, the converter's own included (TRAIN J run 2's rc=127)
  local rc sha
  (cd "$W/src/go2cs" && go build -o bin/go2cs .) > "$R/conv-build-$1.log" 2>&1; rc=$?
  [ $rc = 0 ] && [ -x "$EXE" ] || abort "converter build ($1) rc=$rc"
  sha=$(sha256sum "$EXE" | cut -c1-16)
  [ -z "$CONV0" ] && CONV0=$sha
  stamp "BUILD converter ($1) sha=$sha$( [ "$sha" = "$CONV0" ] || echo " DIFFERS from the first build $CONV0")"
}
purge(){ # label
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
summ(){ # comparison.json manifest.json -- one line + the first errors, from the FRESH record. Exits 1, printing one
  # 'FRESHNESS FAIL: <field>' line each, when the record was not read under the pinned toolchain: oracleGoVersion
  # without go1.24.13, configuration != Release, tiered true, targetGOOS != linux, or a converterRevision that differs
  # from the first row's (kept in $R/converter-revision.txt).
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
tleg(){ # pkg, per-child -test-timeout, outer timeout (both children + convert/build), suffix, extra converter args...
  local pkg=$1 to=$2 otm=$3 suf=$4; shift 4
  local n out C M L changed src
  n=$(echo "$pkg" | tr '/' '.'); out="$W/src/core/$pkg"; C="$out/go2cs_test_comparison.json"; M="$out/go2cs_test_manifest.json"
  rm -f "$C" "$out/go2cs_test_results.json" "$out/go2cs_test_results.xml"
  touch "$R/.t0-$n-$suf"
  # floor 3: the output directory is the SECOND positional. -platforms defaults to this host (linux/amd64).
  leg "T:$pkg:$suf" "tests-$n-$suf" "$otm" "$EXE" -tests -test-action all -test-timeout "$to" -test-config Release "$@" \
      -go2cspath "$W/src" "$GOROOT/src/$pkg" "$out"
  L="$R/tests-$n-$suf.log"
  stamp "  T:$pkg:$suf: $(grep -aE 'Validated [0-9]+ tests|comparison failed' "$L" | tail -n 1 | cut -c1-400)"
  if [ -f "$C" ] && [ "$C" -nt "$R/.t0-$n-$suf" ]; then
    cp "$C" "$R/tests-$n-$suf.comparison.json"; [ -f "$M" ] && cp "$M" "$R/tests-$n-$suf.manifest.json"
    summ "$C" "$M" > "$R/.summ-$n-$suf" 2>&1; src=$?     # floor 7: rc before any pipe
    cat "$R/.summ-$n-$suf" >> "$SUM"; cat "$R/.summ-$n-$suf"
    if [ $src != 0 ]; then FRESH_FAILS=$((FRESH_FAILS + 1)); stamp "      FRESHNESS FAIL on $pkg:$suf: this record is NOT a reading at the pinned toolchain"; fi
  else
    stamp "      NO FRESH COMPARISON RECORD (the run did not reach its compare; read the log tail)"
  fi
  git -C "$W" diff --stat -- "src/core/$pkg" > "$R/tests-$n-$suf.srcdiff.txt"
  git -C "$W" diff -- "src/core/$pkg" > "$R/tests-$n-$suf.srcdiff.patch"
  changed=$(git -C "$W" status --porcelain -- "src/core/$pkg" | grep -vc '^??')
  stamp "      tracked files the run rewrote under src/core/$pkg: $changed (csproj among them: $(grep -c 'csproj' "$R/tests-$n-$suf.srcdiff.txt"))"
  git -C "$W" checkout -q -- "src/core/$pkg"
  [ "$(git -C "$W" status --porcelain | grep -c '^ D')" = 0 ] || abort "tracked deletions after restoring src/core/$pkg"
}
GTK='FullyQualifiedName~FieldPointerEqualityTests|FullyQualifiedName~SliceBoundsR1aTests|FullyQualifiedName~FrameSymbolNameTests|FullyQualifiedName~GoTestOrderTests|FullyQualifiedName~UnsafeLengthLimitTests|FullyQualifiedName~ReflectHashTokenBandTests|FullyQualifiedName~RuntimeCallerPCSpanTests|FullyQualifiedName~SyntheticPCRegistryTests|FullyQualifiedName~EventLineTerminatorTests|FullyQualifiedName~CreatedByPositionTests|FullyQualifiedName~TestGoroutineCreatorTests|FullyQualifiedName~LinuxDescriptorLimitTests|FullyQualifiedName~ExecutionTracerOracleResolutionTests|FullyQualifiedName~ExecutionTracerParserTests|FullyQualifiedName~PrintThroughRuntimeTests|FullyQualifiedName~ThreadStateCensusTests|FullyQualifiedName~GoroutineProfileInstantTests|FullyQualifiedName~RuntimeLockProfileTests|FullyQualifiedName~TracebackDecorationTests'
GP="$W/src/tests/GolibTests/GolibTests.csproj"
gtbuild(){ # cfg
  leg "GT-build-$1" "gt-build-$1" 60m env GoTargetOS=linux dotnet build "$GP" -c "$1" -p:GoTargetOS=linux -p:UseSharedCompilation=false
}
gt(){ # cfg, tag, extra dotnet-test args... -- PLAIN (never under strace: it perturbs the CPU-time arm)
  local cfg=$1 tag=$2 L; shift 2
  leg "GT-$cfg-$tag" "gt-$cfg-$tag" 90m env GoTargetOS=linux dotnet test "$GP" -c "$cfg" --no-build -p:GoTargetOS=linux \
      --blame-hang-timeout 20m --blame-hang-dump-type none --logger 'console;verbosity=normal' "$@"
  L="$R/gt-$cfg-$tag.log"
  stamp "  GT-$cfg-$tag: $(grep -aE '(Passed|Failed)!' "$L" | tail -n 1 | cut -c1-200)"
  stamp "      failed: $(grep -aE '^\s+Failed [A-Za-z]' "$L" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-400)"
  stamp "      skipped: $(grep -aE '^\s+Skipped [A-Za-z]' "$L" | sed 's/^ *Skipped //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-400)"
  grep -aqE 'Blame|The active test run was aborted|Test host process crashed' "$L" && stamp "      HANG/ABORT/CRASH text present in the log: read it"
}

buildconv first
if [ "$LANE" = P1 ]; then
  # L1 -- p1-tests-goos: a linux row with GoTargetOS NOT exported must build the LINUX flavour and validate (14 + 1).
  tleg unicode/utf8 10m 240m plain
  # the witness reads the DLL: type names live in its metadata string heap, not in the portable PDB
  dll=$(find "$W/src/core/unicode/utf8" -path '*bin*' -name syscall.dll 2>/dev/null | head -n 1)
  stamp "      flavour witness: syscall.dll=${dll:-none} PtraceRegs=$( [ -n "$dll" ] && grep -ac PtraceRegs "$dll" || echo n/a)"
  grep -aqE 'Validated 14 tests' "$R/tests-unicode.utf8-plain.log" || abort "L1 unicode/utf8 did not validate 14: STOP and post (p1-tests-goos or the S1 pin)"
  [ "$FRESH_FAILS" = 0 ] || abort "L1 unicode/utf8 record failed its freshness checks (targetGOOS/oracle/config): STOP and post"
  # L1v -- the runtime proof on a -tests host (its self-contained bin bundles the root's libcoreclr)
  touch "$R/.t0-verify"
  leg "V:unicode/utf8" verify-utf8 60m bash "$W/docs/phase4/recipes/microsoft-dotnet/verify-ms-dotnet.sh" \
      "$EXE" -tests -test-action all -test-timeout 10m -test-config Release -go2cspath "$W/src" "$GOROOT/src/unicode/utf8" "$W/src/core/unicode/utf8"
  stamp "  V: $(grep -aE 'VERIFY (PASS|FAIL)|MICROSOFT|NOT MICROSOFT' "$R/verify-utf8.log" | tr '\n' ' ' | cut -c1-500)"
  git -C "$W" checkout -q -- src/core/unicode/utf8
  purge after-L1
  # L2 -- GolibTests x2 (Release, Debug), the FULL suite, nothing excluded (p2-descriptor-guard retires the known hang)
  for cfg in Release Debug; do
    gtbuild "$cfg" && gt "$cfg" full
  done
  gt Release kseats --filter "$GTK"
  purge after-GT
  # L3 -- os (host identity: this box runs as uid 0; the banked linux annotation 912 + 2 is a NON-ROOT reading)
  tleg os 30m 240m row
  purge after-os
  # L4 -- the linux rows K seats name
  for pkg in runtime/debug testing sync net/http/pprof reflect fmt internal/fmtsort encoding/json; do
    tleg "$pkg" 30m 240m row
  done
  purge after-L4
else
  # L0v -- the runtime proof (verifier), on the descriptor guard's own class
  gtbuild Release || abort "GolibTests Release build failed"
  leg "V:GolibTests" verify-gt 30m bash "$W/docs/phase4/recipes/microsoft-dotnet/verify-ms-dotnet.sh" \
      env GoTargetOS=linux dotnet test "$GP" -c Release --no-build -p:GoTargetOS=linux --filter 'FullyQualifiedName~LinuxDescriptorLimitTests'
  stamp "  V: $(grep -aE 'VERIFY (PASS|FAIL)|MICROSOFT|NOT MICROSOFT|(Passed|Failed)!' "$R/verify-gt.log" | tr '\n' ' ' | cut -c1-500)"
  # L2 -- GolibTests Release, FULL and PLAIN, on the box where the descriptor guard failed order-sensitively
  gt Release full
  gt Release kseats --filter "$GTK"
  purge after-GT
  # L5 -- runtime/pprof (the bank candidate's linux annotation; G's gate seat's owed linux read), twice
  tleg runtime/pprof 30m 240m row1
  tleg runtime/pprof 30m 240m row2
  purge after-pprof
  # L6 -- the FULL linux runtime row
  tleg runtime 150m 330m row
  purge after-runtime
fi
stamp "END freshness-fails=$FRESH_FAILS head=$(git -C "$W" rev-parse --short=10 HEAD) tracked-changes=$(git -C "$W" status --porcelain | grep -vc '^??') tracked-deletions=$(git -C "$W" status --porcelain | grep -c '^ D')"
stamp "LINUX LEGS DONE ($LANE)"
