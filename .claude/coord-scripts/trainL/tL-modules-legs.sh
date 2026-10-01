#!/usr/bin/env bash
# TRAIN L -- the R module chain's MANUAL READINGS as runnable legs (M3 -tests -recurse e2e, M4 module ancestry e2e,
# M6 DefaultGODEBUG e2e + program arm), plus the converter suite's module tests READ BY NAME. Derived 2026-10-01 by
# the battery-prep workflow from the seats' own commits and R's inbox posts (20261001T004836Z, T025143Z, T071348Z).
# NOT RUN by its author (read-only derivation): COORD reviews, copies into a per-run folder (floor 4), and runs it AFTER
# (or as a tail of) the main battery, on the i7, one battery at a time:
#   EXPECT_HEAD=<union head, 10 chars> bash tL-modules-legs.sh            # main legs
#   EXPECT_HEAD=<...> CONTROLS=1 bash tL-modules-legs.sh                   # + the two discriminating controls
# Env, leg() and purge semantics are tK-reread.sh's (verbatim where marked). Fixtures and output roots live in the
# run folder ($SD/fx, $SD/out), OUTSIDE every worktree; the only tree write is the converter binary under
# src/go2cs/bin (as in K's T legs), purged at the end. rc captured per leg before any pipe (floor 7).
# DRAFT 2 (CHANGES.md): exits 4 when any verdict FAILED (the battery's MOD leg then reaches NONZERO LEGS); refuses a
# run folder that already holds mod-logs/SUMMARY.txt; takes the battery lock when run standalone (and refuses beside a
# running battery unless the battery launched it, IN_BATTERY=1); checks the GOROOT spelling before the export; honours
# DEADLINE (R5: no leg STARTS after it, exit 9); adds MR2 + JWT (module-cache legs, R6: run only when the exact module
# directories are present, GOPROXY=off, never a download), MR4v (vendor/), MR4c read by name, MR6e (GODEBUG env over the
# stamp) and MR6c (e9009f2945's converter, R7). Control converters build under $CTL (coord-scratch/tL), never a worktree.
set -u
W=/h/go2cs-tmp-coord/tL
WB='H:\go2cs-tmp-coord\tL'
EXPECT_HEAD=${EXPECT_HEAD:?union head, 10 chars}
CONTROLS=${CONTROLS:-0}
IN_BATTERY=${IN_BATTERY:-0}
DEADLINE=${DEADLINE:-17:30}
SD=$(cd "$(dirname "$0")" && pwd)
LOCK=/h/go2cs-tmp-coord/coord-scratch/tL/.battery.lock
CTL=/h/go2cs-tmp-coord/coord-scratch/tL/mod-ctl-build    # R7: control converters are built here from git archive
LOGDIR=$SD/mod-logs; FX=$SD/fx; OUT=$SD/out
SUM="$LOGDIR/SUMMARY.txt"
[ -e "$SUM" ] && { echo "ABORT: $SUM exists -- use a fresh per-run copy (critic 1 #4)"; exit 2; }
[[ "$DEADLINE" =~ ^([01][0-9]|2[0-3]):[0-5][0-9]$ ]] || { echo "ABORT: DEADLINE '$DEADLINE' is not HH:MM"; exit 2; }
[ -f "$SD/tL-helpers.py" ] || { echo "ABORT: $SD/tL-helpers.py missing (the by-name readers)"; exit 2; }
if [ "$IN_BATTERY" = 1 ]; then
  [ -d "$LOCK" ] || { echo "ABORT: IN_BATTERY=1 but no battery lock at $LOCK"; exit 2; }
else
  mkdir "$LOCK" 2>/dev/null || { echo "ABORT: $LOCK exists -- a TRAIN L battery is running (floors 1, 11)"; exit 2; }
  trap 'rmdir "$LOCK" 2>/dev/null' EXIT
fi
mkdir -p "$LOGDIR" "$FX" "$OUT"
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
verdict(){ # label, ok(0|1), detail
  if [ "$2" = 0 ]; then stamp "  $1: PASS -- $3"; else stamp "  $1: FAIL -- $3"; FAILS=$((FAILS + 1)); fi
}
notmeasured(){ stamp "  $1: NOT MEASURED -- $2"; }   # R6: stated, never a pass and never a fail
hp(){ python -B "$(cygpath -w "$SD/tL-helpers.py")" "$@"; }
FAILS=0
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }
# ---- env: tK-reread.sh verbatim
ORIGPATH="$PATH"
# critic 1 #5: the GOROOT spelling is checked on its VALUE before the export (derived from $HOME, never written out)
GR=$(cygpath -w "$HOME/sdk/go1.24.13")
case "$GR" in */*) stamp "ABORT: GOROOT spelling '$GR' has a forward slash (floor 6)"; exit 2 ;; esac
[[ "$GR" =~ ^[A-Za-z]:\\.*\\sdk\\go1\.24\.13$ ]] || { stamp "ABORT: GOROOT spelling '$GR'"; exit 2; }
head -n 1 "$HOME/sdk/go1.24.13/VERSION" | tr -d '\r' | grep -qx 'go1\.24\.13' || { stamp "ABORT: GOROOT VERSION is not go1.24.13"; exit 2; }
[ "$(env -u GOROOT "$HOME/sdk/go1.24.13/bin/go.exe" env GOROOT | tr -d '\r')" = "$GR" ] || { stamp "ABORT: the toolchain's own GOROOT spelling != '$GR'"; exit 2; }
export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
export GOROOT="$GR" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
go version | grep -q 'go1\.24\.13 ' || { stamp "ABORT: go on PATH is $(go version)"; exit 2; }
export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tmp /h/go-cache/go-build /h/nuget/packages
export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"
# ---- module legs: hermetic (replace-only fixtures), no proxy, no workspace. GO2CS_MODULE_ROOT must NOT be ambient:
#      the converter sets it per host child (M4); an inherited value would mask a converter that stopped setting it.
export GOPROXY=off GOWORK=off GOSUMDB=off
unset GO2CS_MODULE_ROOT
cd "$W" || exit 2
h=$(git rev-parse --short=10 HEAD); [ "$h" = "$EXPECT_HEAD" ] || { stamp "ABORT: HEAD $h != $EXPECT_HEAD"; exit 2; }
[ "$(git status --porcelain | grep -vc "^??")" = 0 ] || { stamp "ABORT: tracked changes at start"; exit 2; }
# R6: the module cache as go reports it under this environment (the battery does not override GOMODCACHE; `go env -w`
# puts it on H:). Presence is read by listing EXACT module directories only, never a walk, never a download.
GMC=$(go env GOMODCACHE | tr -d '\r'); GMCU=$(cygpath -u "$GMC")
for s in 27f2199b30 c5e934c464 e9009f2945 6a079a9675 bbbb3ff022; do
  git merge-base --is-ancestor "$s" HEAD || { stamp "ABORT: $s not an ancestor of the union"; exit 2; }
done
stamp "PRE head=$h $(go version) dotnet=$(dotnet --version | tr -d "\r") free=$(freegb)G GOMODCACHE=$GMC DEADLINE=$DEADLINE in-battery=$IN_BATTERY (R chain M2..M6 + K's saveblockevent fix contained)"
cleanup_tree(){ rm -rf "$W/src/go2cs/bin" "$CTL"; }
leg(){ # name, logfile-suffix, command...   (tK-reread.sh verbatim + the R5 deadline)
  local name=$1 suf=$2; shift 2
  local L="$LOGDIR/$suf.log" t0 f c now dl
  now=$(date +%H%M); dl=${DEADLINE/:/}
  if [ "$((10#$now))" -ge "$((10#$dl))" ]; then
    cleanup_tree
    stamp "DEADLINE STOP before $name (DEADLINE=$DEADLINE): converter bin and control builds removed; failures so far=$FAILS; exit 9"
    exit 9
  fi
  t0=$(date +%s); LEG_T0=$t0
  f=$(freegb); [ "${f:-0}" -ge 30 ] || { stamp "LEG $name ABORT: free disk ${f}G < 30G"; exit 3; }
  c=$(cfree); [ "${c:-0}" -ge 8 ] || { stamp "LEG $name ABORT: C: free ${c}G < 8G"; exit 3; }
  "$@" > "$L" 2>&1 < /dev/null; local rc=$?
  LEG_RC=$rc
  stamp "LEG $name rc=$rc wall=$(( $(date +%s) - t0 ))s free=$(freegb)G log=$suf.log"
  return $rc
}
wf(){ mkdir -p "$(dirname "$1")"; printf '%b' "$2" > "$1"; }   # write a fixture file (LF, printf escapes)
hashtree(){ ( cd "$1" && find . -type f | LC_ALL=C sort | xargs sha256sum ); }
validated(){ # log -> "pkg N" per phase-B package (the -tests header, then that package's Validated line)
  tr -d '\r' < "$1" | awk '/^-tests [^ ]+ -> /{pkg=$2} /^Validated [0-9]+ tests against go test/{print pkg, $2}'
}
stampval(){ grep -ao 'go\.GoDefaultGodebug("[^"]*")' "$1" 2>/dev/null | sed 's/^go\.GoDefaultGodebug("//; s/")$//' | head -n 1; }

# =========================================================================================== CM -- converter, BY NAME
# Leg C runs `go test ./...` WITHOUT -v: a module test that silently SKIPS (testing.Short) or is absent reads green.
# This leg reads every R-chain test by name: each must print '--- PASS', none '--- SKIP' / '--- FAIL'.
CM_TESTS='TestModuleCacheEndToEndLockAndClash TestModuleCachePreflightNamesMissingDependency TestModuleCacheOutsideGopathResolvesAsCacheDependency TestModulesLockRoundTripAndClash TestTestsRecurseConvertsAModuleAgainstItsTests TestTestsRecurseRefusesANewerGoLineAndAnInModuleRoot TestTestClosureDiscoveryAddsNoOrderEdges TestTestHostCarriesTheLogicalModulePathOnly TestTestHostModuleEnvironmentIsForModulePackagesOnly TestWriteTestHostUsesCSharpClassOverride TestFixtureDirectoriesStagePackageShape TestDefaultGODEBUGMatchesTheToolchain TestRecurseMainProgramCarriesItsDefaultGODEBUG TestModuleGodebugReadsTheGodebugBlockStrictly'
CM_SUBS='TestDefaultGODEBUGMatchesTheToolchain/go120 TestDefaultGODEBUGMatchesTheToolchain/go123 TestDefaultGODEBUGMatchesTheToolchain/go124 TestDefaultGODEBUGMatchesTheToolchain/block TestDefaultGODEBUGMatchesTheToolchain/directive'
CM_RE="^($(echo $CM_TESTS | tr ' ' '|'))\$"
leg CM conv-modules-named bash -c "cd src/go2cs && go test -count=1 -timeout 30m -v -run '$CM_RE' ."
cmrc=$LEG_RC; miss=''
for t in $CM_TESTS $CM_SUBS; do grep -aqE -- "--- PASS: $t \(" "$LOGDIR/conv-modules-named.log" || miss="$miss $t"; done
sk=$(grep -acE -- '--- (SKIP|FAIL):' "$LOGDIR/conv-modules-named.log")
[ "$cmrc" = 0 ] && [ -z "$miss" ] && [ "$sk" = 0 ]; verdict CM $? "rc=$cmrc not-PASS=[${miss# }] skip/fail-lines=$sk (19 names incl. 5 subtests)"

# =========================================================================================== converter binary
export MSYS_NO_PATHCONV=1
EXE="$W/src/go2cs/bin/go2cs.exe"
rm -f "$EXE"
( cd src/go2cs && go build -o bin/go2cs.exe . ) > "$LOGDIR/conv-build.log" 2>&1; brc=$?   # critic 1 #6: the rc is read
[ "$brc" = 0 ] && [ -x "$EXE" ] || { stamp "ABORT: converter build rc=$brc, exe present=$([ -x "$EXE" ] && echo yes || echo no)"; exit 3; }
ctlconv(){ # sha, name -- R7: a control converter from `git archive` into $CTL (coord-scratch/tL, never a worktree)
  local d="$CTL/$2"
  rm -rf "$d"; mkdir -p "$d"
  git archive "$1" src/go2cs | tar -x -C "$d" || return 1
  ( cd "$d/src/go2cs" && go build -o "go2cs-$2.exe" . ) > "$LOGDIR/conv-build-$2.log" 2>&1 || return 1
  [ -x "$d/src/go2cs/go2cs-$2.exe" ] && echo "$d/src/go2cs/go2cs-$2.exe"
}
stamp "BUILD converter sha=$(sha256sum "$EXE" | cut -c1-16)"
trec(){ # name, suffix, moduleDir(unix), outRoot(unix) -- the -tests -recurse driver, action all
  rm -rf "$4"
  leg "$1" "$2" "$EXE" -tests -test-action all -test-timeout 20m -recurse -go2cspath "$WB\\src" "$(cygpath -w "$3")" "$(cygpath -w "$4")"
}
present(){ # unix paths... -- R6: every exact path exists (no walk, no download)
  local p; for p in "$@"; do [ -e "$p" ] || { echo "absent: ${p#$GMCU/}"; return 1; }; done; return 0
}

# =========================================================================================== MR2 -- M2 real module cache
# Critic 0 #9: M2's claim 'real-cache reading runewidth->uniseg with exact go.sum hashes' has no other leg (MR3 is
# replace-only, CM hermetic). R6: runs ONLY when both modules (extracted dirs + the download cache's .mod/.zip that an
# offline resolve reads) are in the H: module cache; otherwise 'NOT MEASURED: module absent'. GOPROXY=off throughout.
# EXPECT: the oracle (go mod tidy offline + go run) prints 5; -recurse rc=0; go2cs.modules.lock holds EXACTLY
# runewidth v0.0.13 and uniseg v0.2.0, each hash == the fixture's go.sum h1 line; the fixture unchanged by the convert.
M2NEED="$GMCU/github.com/mattn/go-runewidth@v0.0.13/go.mod $GMCU/github.com/rivo/uniseg@v0.2.0/go.mod $GMCU/cache/download/github.com/mattn/go-runewidth/@v/v0.0.13.mod $GMCU/cache/download/github.com/mattn/go-runewidth/@v/v0.0.13.zip $GMCU/cache/download/github.com/rivo/uniseg/@v/v0.2.0.mod $GMCU/cache/download/github.com/rivo/uniseg/@v/v0.2.0.zip"
if m2why=$(present $M2NEED); then
  F2=$FX/m2/rw; rm -rf "$FX/m2"
  wf "$F2/go.mod" 'module example.test/rw\n\ngo 1.23\n\nrequire github.com/mattn/go-runewidth v0.0.13\n'
  wf "$F2/main.go" 'package main\n\nimport (\n\t"fmt"\n\n\t"github.com/mattn/go-runewidth"\n)\n\nfunc main() {\n\tfmt.Println(runewidth.StringWidth("go2cs"))\n}\n'
  leg MR2-oracle go-mod-m2 bash -c "cd '$F2' && go mod tidy && go run ."
  o2rc=$LEG_RC; o2=$(tr -d '\r' < "$LOGDIR/go-mod-m2.log" | tail -n 1)
  [ "$o2rc" = 0 ] && [ "$o2" = 5 ]; verdict MR2-oracle $? "rc=$o2rc output=[$o2] (go mod tidy offline + go run; EXPECT 5)"
  B2=$(hashtree "$F2")
  rm -rf "$OUT/m2"
  leg MR2 recurse-m2 "$EXE" -recurse -go2cspath "$WB\\src" "$(cygpath -w "$F2")" "$(cygpath -w "$OUT/m2")"
  r2=$LEG_RC
  hp lockcheck "$(cygpath -w "$OUT/m2/go2cs.modules.lock")" "$(cygpath -w "$F2/go.sum")" github.com/mattn/go-runewidth@v0.0.13 github.com/rivo/uniseg@v0.2.0 > "$LOGDIR/m2-lockcheck.txt" 2>&1; lk=$?
  [ "$r2" = 0 ] && [ "$lk" = 0 ] && [ "$(hashtree "$F2")" = "$B2" ]
  verdict MR2 $? "rc=$r2 $(grep -a '^LOCKCHECK \(OK\|FAILED\|lock absent\|module set\)' "$LOGDIR/m2-lockcheck.txt" | tr -d '\r' | tr '\n' ' ' | cut -c1-200) module-unchanged=$([ "$(hashtree "$F2")" = "$B2" ] && echo yes || echo NO)"
else
  notmeasured MR2 "module absent ($m2why) in $GMC (R6: never downloaded)"
fi

# =========================================================================================== JWT -- i9-crosspkg's jwt class
# Critic 0 #10: i9-crosspkg claims 'the jwt root now compiles and packs' (the jwt CS1929 class: a value method promoted
# through ANOTHER package's embedded struct, jwt's NumericDate embedding time.Time); MR3's fixture has no such call.
# R6: runs ONLY when github.com/golang-jwt/jwt/v5@v5.3.1 is in the H: module cache. A consumer module requiring it is
# converted with -recurse (the dependency lands under <out>/pkg/<importPath>, M2's layout: INFERRED path, a missing
# project is a FAIL), then the jwt ROOT project is built alone. EXPECT rc=0 and 0 CS1929 (GATED). The consumer's own run
# (which calls the promoted d.Unix()) is compared with `go run` as INFORMATION, not gated (the seat claims compile+pack).
JWNEED="$GMCU/github.com/golang-jwt/jwt/v5@v5.3.1/go.mod $GMCU/cache/download/github.com/golang-jwt/jwt/v5/@v/v5.3.1.mod $GMCU/cache/download/github.com/golang-jwt/jwt/v5/@v/v5.3.1.zip"
if jwwhy=$(present $JWNEED); then
  FJ=$FX/jwt/use; rm -rf "$FX/jwt"
  wf "$FJ/go.mod" 'module example.test/jwtuse\n\ngo 1.23\n\nrequire github.com/golang-jwt/jwt/v5 v5.3.1\n'
  wf "$FJ/main.go" 'package main\n\nimport (\n\t"fmt"\n\t"time"\n\n\t"github.com/golang-jwt/jwt/v5"\n)\n\nfunc main() {\n\td := jwt.NewNumericDate(time.Unix(1700000000, 0))\n\tfmt.Println(d.Unix())\n}\n'
  leg JWT-oracle go-mod-jwt bash -c "cd '$FJ' && go mod tidy && go run ."
  ojrc=$LEG_RC; oj=$(tr -d '\r' < "$LOGDIR/go-mod-jwt.log" | tail -n 1)
  [ "$ojrc" = 0 ] && [ "$oj" = 1700000000 ]; verdict JWT-oracle $? "rc=$ojrc output=[$oj] (EXPECT 1700000000)"
  rm -rf "$OUT/jwt"
  leg JWT-convert recurse-jwt "$EXE" -recurse -go2cspath "$WB\\src" "$(cygpath -w "$FJ")" "$(cygpath -w "$OUT/jwt")"
  rj=$LEG_RC
  JP=$(ls "$OUT"/jwt/pkg/github.com/golang-jwt/jwt/v5/*.csproj 2>/dev/null | grep -v '\.tests\.csproj$' | head -n 1)
  if [ -n "$JP" ]; then
    leg JWT-build build-jwt-root dotnet build "$(cygpath -w "$JP")" -c Release -p:UseSharedCompilation=false
    bj=$LEG_RC; c1929=$(grep -aoE 'error CS1929' "$LOGDIR/build-jwt-root.log" | wc -l); cerr=$(grep -aoE 'error CS[0-9]+' "$LOGDIR/build-jwt-root.log" | LC_ALL=C sort | uniq -c | sort -rn | head -n 4 | tr -s ' ' | tr '\n' ' ')
    [ "$rj" = 0 ] && [ "$bj" = 0 ] && [ "$c1929" = 0 ]; verdict JWT $? "convert rc=$rj root-project=${JP#$OUT/} build rc=$bj CS1929=$c1929 (EXPECT 0) top errors=[${cerr% }]"
    PJ=$(ls "$OUT"/jwt/src/example.test/jwtuse/*.csproj 2>/dev/null | head -n 1)
    if [ -n "$PJ" ]; then
      leg JWT-run run-jwtuse bash -c "dotnet build '$(cygpath -w "$PJ")' -c Release -p:UseSharedCompilation=false && dotnet run --project '$(cygpath -w "$PJ")' -c Release --no-build"
      cj=$(tr -d '\r' < "$LOGDIR/run-jwtuse.log" | tail -n 1)
      stamp "  JWTrun (INFORMATION, not gated): rc=$LEG_RC converted=[$cj] go-run=[$oj] $([ "$cj" = "$oj" ] && echo same || echo DIFFERENT)"
    fi
  else
    verdict JWT 1 "convert rc=$rj: no jwt root project under $OUT/jwt/pkg/github.com/golang-jwt/jwt/v5 (NOT MEASURED counts as a failure: the layout is INFERRED, read recurse-jwt.log)"
  fi
else
  notmeasured JWT "module absent ($jwwhy) in $GMC (R6: never downloaded)"
fi

# =========================================================================================== MR3 -- M3 jwt-shaped
# The fixture is moduleTestsDriver_integration_test.go's writeJwtLikeModule (go 1.23), byte for byte in content.
# R's reading at c5e934c464 (00:48Z): exit 0; 5 packages in phase A (NOT 7: no synthesized test main); jwtlike and
# jwtlike/request each 'Validated 1 tests against go test'; MODULE.md 'Total: 2 matched · 0 disclosed' across 2.
# AT THE UNION (new vs R's reading): M6 is in, so a go 1.23 module's hosts carry a non-empty DefaultGODEBUG stamp
# (the Changed:24 settings), which must equal the toolchain's own answer for the test main.
F3=$FX/m3; rm -rf "$F3"
wf "$F3/dep/go.mod" 'module example.test/dep\n\ngo 1.23\n'
wf "$F3/dep/prefix.go" 'package dep\n\nfunc Prefix() string {\n\treturn "signed:"\n}\n'
wf "$F3/dep/prefix_test.go" 'package dep\n\nimport "testing"\n\nfunc TestPrefix(t *testing.T) {\n\tif Prefix() == "" {\n\t\tt.Fatal("empty")\n\t}\n}\n'
wf "$F3/testkit/go.mod" 'module example.test/testkit\n\ngo 1.23\n'
wf "$F3/testkit/same.go" 'package testkit\n\nfunc Same(a, b string) bool {\n\treturn a == b\n}\n'
wf "$F3/jwtlike/go.mod" 'module example.test/jwtlike\n\ngo 1.23\n\nrequire (\n\texample.test/dep v0.0.0\n\texample.test/testkit v0.0.0\n)\n\nreplace example.test/dep => ../dep\n\nreplace example.test/testkit => ../testkit\n'
wf "$F3/jwtlike/token.go" 'package jwtlike\n\nimport "example.test/dep"\n\nfunc Sign(claim string) string {\n\treturn dep.Prefix() + claim\n}\n'
wf "$F3/jwtlike/token_test.go" 'package jwtlike\n\nimport (\n\t"testing"\n\n\t"example.test/jwtlike/testutil"\n)\n\nfunc TestSign(t *testing.T) {\n\ttestutil.Expect(t, Sign("a"), "signed:a")\n}\n'
wf "$F3/jwtlike/request/request.go" 'package request\n\nimport "example.test/jwtlike"\n\nfunc FromHeader(h string) string {\n\treturn jwtlike.Sign(h)\n}\n'
wf "$F3/jwtlike/request/request_test.go" 'package request_test\n\nimport (\n\t"testing"\n\n\t"example.test/jwtlike"\n\t"example.test/jwtlike/request"\n\t"example.test/jwtlike/testutil"\n\t"example.test/testkit"\n)\n\nfunc TestFromHeader(t *testing.T) {\n\ttestutil.Expect(t, request.FromHeader("h"), jwtlike.Sign("h"))\n\tif !testkit.Same("x", "x") {\n\t\tt.Fatal("testkit")\n\t}\n}\n'
wf "$F3/jwtlike/testutil/expect.go" 'package testutil\n\nimport "testing"\n\nfunc Expect(t *testing.T, got, want string) {\n\tt.Helper()\n\tif got != want {\n\t\tt.Fatalf("got %q, want %q", got, want)\n\t}\n}\n'
leg MR3-oracle go-test-m3 bash -c "cd '$F3/jwtlike' && go test -count=1 -timeout 10m ./..."
verdict MR3-oracle "$LEG_RC" "go test ./... over the fixture (the Go side must pass first)"
B3=$(hashtree "$F3")
trec MR3 tests-recurse-m3 "$F3/jwtlike" "$OUT/m3"; r3=$LEG_RC
L3=$LOGDIR/tests-recurse-m3.log; O3=$OUT/m3; S3=$O3/src/example.test/jwtlike
v3=$(validated "$L3" | LC_ALL=C sort | tr '\n' ';')
nconv=$(tr -d '\r' < "$L3" | grep -aoE 'Converting [0-9]+ packages in dependency order' | head -n 1)
nprod=$(find "$O3" -name '*.csproj' ! -name '*.tests.csproj' | wc -l)
abs=$(grep -rlE 'Include="([A-Za-z]:[\\/]|/)' --include='*.csproj' "$O3" | wc -l)
mains=$( { [ -e "$S3.test" ] && echo "$S3.test"; [ -e "$S3/request.test" ] && echo request.test; } | wc -l)
tk=$(ls "$O3"/pkg/example.test/testkit/*.cs 2>/dev/null | wc -l); dept=$(ls "$O3"/pkg/example.test/dep/*.tests.csproj 2>/dev/null | wc -l)
mod=$(tr -d '\r' < "$O3/validation/example.test/jwtlike/MODULE.md" 2>/dev/null | grep -a 'Total:')
lock=$([ -e "$O3/go2cs.modules.lock" ] && echo present || echo absent)
want3=$(cd "$F3/jwtlike" && go list -test -f '{{.ImportPath}}|{{.DefaultGODEBUG}}' . | tr -d '\r' | grep -a '^example.test/jwtlike.test|' | cut -d'|' -f2)
got3=$(stampval "$S3/go2cs_test_host.cs")
[ "$r3" = 0 ] && [ "$v3" = 'example.test/jwtlike 1;example.test/jwtlike/request 1;' ] && [ "$nconv" = 'Converting 5 packages in dependency order' ] \
  && [ "$abs" = 0 ] && [ "$mains" = 0 ] && [ "$tk" -ge 1 ] && [ "$dept" = 0 ] \
  && echo "$mod" | grep -q 'Total: 2 matched · 0 disclosed\*\* across 2 package(s)' && [ "$lock" = absent ] \
  && [ -n "$want3" ] && [ "$got3" = "$want3" ] && [ "$(hashtree "$F3")" = "$B3" ]
verdict MR3 $? "rc=$r3 validated=[$v3] [$nconv] prod-csproj=$nprod(info, R saw 5) abs-refs=$abs test-mains=$mains testkit-cs=$tk dep-tests-csproj=$dept MODULE=[$mod] lock=$lock stamp==go-list-test:[$([ "$got3" = "$want3" ] && echo yes || echo "NO got=${got3:0:60} want=${want3:0:60}")] module-unchanged=$([ "$(hashtree "$F3")" = "$B3" ] && echo yes || echo NO)"

# =========================================================================================== MR4 -- M4 module ancestry
# The shape of R's RED/GREEN fixture (e9009f2945's message; module path as ModuleAncestryTests' example.test/keys):
# a root test reading test/key.pem, a sub-package test reading ../test/key.pem and ../go.mod. SIX module files, as
# R hashed. EXPECT exit 0, keys 'Validated 1 tests', keys/parse 'Validated 2 tests', the module byte-identical.
F4=$FX/m4/keys; rm -rf "$FX/m4"
wf "$F4/go.mod" 'module example.test/keys\n\ngo 1.23\n'
wf "$F4/keys.go" 'package keys\n\nfunc Name() string {\n\treturn "keys"\n}\n'
wf "$F4/keys_test.go" 'package keys\n\nimport (\n\t"os"\n\t"testing"\n)\n\nfunc TestReadsTheModuleFixture(t *testing.T) {\n\tdata, err := os.ReadFile("test/key.pem")\n\tif err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif string(data) != "KEY-MATERIAL\\n" {\n\t\tt.Fatalf("got %q", data)\n\t}\n}\n'
wf "$F4/test/key.pem" 'KEY-MATERIAL\n'
wf "$F4/parse/parse.go" 'package parse\n\nfunc Name() string {\n\treturn "parse"\n}\n'
wf "$F4/parse/parse_test.go" 'package parse\n\nimport (\n\t"os"\n\t"strings"\n\t"testing"\n)\n\nfunc TestReadsTheParentFixture(t *testing.T) {\n\tdata, err := os.ReadFile("../test/key.pem")\n\tif err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif string(data) != "KEY-MATERIAL\\n" {\n\t\tt.Fatalf("got %q", data)\n\t}\n}\n\nfunc TestReadsTheModuleGoMod(t *testing.T) {\n\tdata, err := os.ReadFile("../go.mod")\n\tif err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif !strings.Contains(string(data), "module example.test/keys") {\n\t\tt.Fatalf("got %q", data)\n\t}\n}\n'
leg MR4-oracle go-test-m4 bash -c "cd '$F4' && go test -count=1 -timeout 10m ./..."
verdict MR4-oracle "$LEG_RC" "go test ./... over the fixture (3 tests)"
B4=$(hashtree "$F4"); n4=$(echo "$B4" | wc -l)
trec MR4 tests-recurse-m4 "$F4" "$OUT/m4"; r4=$LEG_RC
v4=$(validated "$LOGDIR/tests-recurse-m4.log" | LC_ALL=C sort | tr '\n' ';')
mp=$(grep -ac '}, "example.test/keys");' "$OUT/m4/src/example.test/keys/parse/go2cs_test_host.cs" 2>/dev/null)
mach=$(grep -rlE '"([A-Za-z]:[\\/]|/)[^"]*keys' --include='go2cs_test_host.cs' "$OUT/m4" | wc -l)
[ "$r4" = 0 ] && [ "$v4" = 'example.test/keys 1;example.test/keys/parse 2;' ] && [ "$n4" = 6 ] && [ "$(hashtree "$F4")" = "$B4" ] && [ "${mp:-0}" -ge 1 ] && [ "$mach" = 0 ]
verdict MR4 $? "rc=$r4 validated=[$v4] module-files=$n4 module-unchanged=$([ "$(hashtree "$F4")" = "$B4" ] && echo yes || echo NO) host-module-path-arg=${mp:-0} machine-path-hosts=$mach"
if [ "$CONTROLS" = 1 ]; then
  # MR4c, the discriminating control (floor 13): the SAME fixture through M3's converter (c5e934c464, before M4),
  # built from `git archive` into the run folder (no worktree, no tree write). The host still compiles against the
  # union's golib testing (TryStageModule present), but the M3 converter neither emits the module path nor sets
  # GO2CS_MODULE_ROOT, so staging declines. EXPECT a non-zero rc and C# failing on the three ENOENT reads
  # ('open test/key.pem', 'open ../test/key.pem', 'open ../go.mod') -- R's RED. A pass here means MR4 proves nothing.
  # DRAFT 2: built by ctlconv under $CTL (R7's rule applied to both controls); the red is read BY NAME from the control's
  # own comparison records (critic 0 #17: '>= 3 matching lines' could be met by one echoed error).
  if X3=$(ctlconv c5e934c464 m3); then
    rm -rf "$OUT/m4c"
    leg MR4c tests-recurse-m4-control "$X3" -tests -test-action all -test-timeout 20m -recurse -go2cspath "$WB\\src" "$(cygpath -w "$F4")" "$(cygpath -w "$OUT/m4c")"
    c4=$LEG_RC; e4=$(grep -acE 'open (\.\./)?(test/key\.pem|go\.mod)' "$LOGDIR/tests-recurse-m4-control.log")
    hp cmpnames "$(cygpath -w "$OUT/m4c/src/example.test/keys/go2cs_test_comparison.json")" pass fail TestReadsTheModuleFixture > "$LOGDIR/m4c-names.txt" 2>&1; n4a=$?
    hp cmpnames "$(cygpath -w "$OUT/m4c/src/example.test/keys/parse/go2cs_test_comparison.json")" pass fail TestReadsTheParentFixture TestReadsTheModuleGoMod >> "$LOGDIR/m4c-names.txt" 2>&1; n4b=$?
    [ "$c4" != 0 ] && [ "$n4a" = 0 ] && [ "$n4b" = 0 ]
    verdict "MR4c (EXPECT rc!=0 + the 3 tests go=pass cs=fail by name)" $? "rc=$c4 by-name: keys=$([ "$n4a" = 0 ] && echo ok || echo "MISS($n4a)") parse=$([ "$n4b" = 0 ] && echo ok || echo "MISS($n4b)") enoent-lines=$e4 (information) (m4c-names.txt)"
  else
    stamp "  MR4c: NOT MEASURED -- the M3 converter did not build (conv-build-m3.log)"; FAILS=$((FAILS + 1))
  fi
fi

# MR4v (critic 0 #15) -- M4's 'the copy includes vendor/': a SEPARATE module (so a vendoring surprise cannot touch MR4's
# reading) whose root test reads vendor/keep.txt. go >= 1.14 with a vendor/ directory defaults to -mod=vendor; with no
# requirements the (absent) modules.txt is consistent (INFERRED: the oracle leg is the proof). EXPECT the oracle PASS,
# exit 0, 'Validated 1 tests', the module byte-identical. The in-module LINK skip has no leg here: a GolibTests arm is
# requested from R (README open question).
F4v=$FX/m4v/vend; rm -rf "$FX/m4v"
wf "$F4v/go.mod" 'module example.test/vend\n\ngo 1.23\n'
wf "$F4v/vend.go" 'package vend\n\nfunc Name() string {\n\treturn "vend"\n}\n'
wf "$F4v/vend_test.go" 'package vend\n\nimport (\n\t"os"\n\t"testing"\n)\n\nfunc TestReadsTheVendoredFile(t *testing.T) {\n\tdata, err := os.ReadFile("vendor/keep.txt")\n\tif err != nil {\n\t\tt.Fatal(err)\n\t}\n\tif string(data) != "VENDORED\\n" {\n\t\tt.Fatalf("got %q", data)\n\t}\n}\n'
wf "$F4v/vendor/keep.txt" 'VENDORED\n'
leg MR4v-oracle go-test-m4v bash -c "cd '$F4v' && go test -count=1 -timeout 10m ./..."
verdict MR4v-oracle "$LEG_RC" "go test ./... over the vendor fixture (1 test)"
B4v=$(hashtree "$F4v")
trec MR4v tests-recurse-m4v "$F4v" "$OUT/m4v"; r4v=$LEG_RC
v4v=$(validated "$LOGDIR/tests-recurse-m4v.log" | LC_ALL=C sort | tr '\n' ';')
[ "$r4v" = 0 ] && [ "$v4v" = 'example.test/vend 1;' ] && [ "$(hashtree "$F4v")" = "$B4v" ]
verdict MR4v $? "rc=$r4v validated=[$v4v] module-unchanged=$([ "$(hashtree "$F4v")" = "$B4v" ] && echo yes || echo NO)"

# =========================================================================================== MR6 -- M6 DefaultGODEBUG
# R's RED/GREEN shape (6a079a9675's message), fixture INFERRED to that description: a go 1.20 module whose root
# package has TestPanicNilKeepsGo120Behavior (panicnil, a runtime dbgvar) and TestRandSeedIsHonored (math/rand's
# randseednop through internal/godebug -- the ONLY end-to-end proof of internal/godebug's new default layer and its
# sentinel cache key: no GolibTests class covers them), and an 'override' package whose test file carries
# //go:debug panicnil=0. EXPECT exit 0; root 'Validated 2 tests', override 'Validated 1 tests'; each host's stamp ==
# `go list -test` DefaultGODEBUG of its test main; root carries panicnil=1 and randseednop=0, override panicnil=0.
F6=$FX/m6/gd120; rm -rf "$FX/m6"
wf "$F6/go.mod" 'module example.test/gd120\n\ngo 1.20\n'
wf "$F6/gd.go" 'package gd\n\nfunc Name() string {\n\treturn "gd"\n}\n'
wf "$F6/gd_test.go" 'package gd\n\nimport (\n\t"math/rand"\n\t"testing"\n)\n\nfunc recoverPanicNil() (r any) {\n\tdefer func() { r = recover() }()\n\tpanic(nil)\n}\n\nfunc TestPanicNilKeepsGo120Behavior(t *testing.T) {\n\tif r := recoverPanicNil(); r != nil {\n\t\tt.Fatalf("recover() = %T %v, want nil under go 1.20 (panicnil=1)", r, r)\n\t}\n}\n\nfunc TestRandSeedIsHonored(t *testing.T) {\n\trand.Seed(42)\n\ta := rand.Int63()\n\trand.Seed(42)\n\tb := rand.Int63()\n\tif a != b {\n\t\tt.Fatalf("rand.Seed ignored (%d != %d), want honored under go 1.20 (randseednop=0)", a, b)\n\t}\n}\n'
wf "$F6/override/override.go" 'package override\n\nfunc Name() string {\n\treturn "override"\n}\n'
wf "$F6/override/override_test.go" '//go:debug panicnil=0\n\npackage override\n\nimport (\n\t"runtime"\n\t"testing"\n)\n\nfunc recoverPanicNil() (r any) {\n\tdefer func() { r = recover() }()\n\tpanic(nil)\n}\n\nfunc TestPanicNilDirectiveRestoresPanicNilError(t *testing.T) {\n\tif _, ok := recoverPanicNil().(*runtime.PanicNilError); !ok {\n\t\tt.Fatal("want *runtime.PanicNilError under //go:debug panicnil=0")\n\t}\n}\n'
leg MR6-oracle go-test-m6 bash -c "cd '$F6' && go test -count=1 -timeout 10m ./..."
verdict MR6-oracle "$LEG_RC" "go test ./... over the fixture (3 tests)"
B6=$(hashtree "$F6")
trec MR6 tests-recurse-m6 "$F6" "$OUT/m6"; r6=$LEG_RC
v6=$(validated "$LOGDIR/tests-recurse-m6.log" | LC_ALL=C sort | tr '\n' ';')
GL6=$(cd "$F6" && go list -test -f '{{.ImportPath}}|{{.DefaultGODEBUG}}' ./... | tr -d '\r')
w6r=$(echo "$GL6" | grep -a '^example.test/gd120.test|' | cut -d'|' -f2); w6o=$(echo "$GL6" | grep -a '^example.test/gd120/override.test|' | cut -d'|' -f2)
g6r=$(stampval "$OUT/m6/src/example.test/gd120/go2cs_test_host.cs"); g6o=$(stampval "$OUT/m6/src/example.test/gd120/override/go2cs_test_host.cs")
[ "$r6" = 0 ] && [ "$v6" = 'example.test/gd120 2;example.test/gd120/override 1;' ] && [ -n "$w6r" ] && [ "$g6r" = "$w6r" ] && [ "$g6o" = "$w6o" ] \
  && echo ",$g6r," | grep -q ',panicnil=1,' && echo ",$g6r," | grep -q ',randseednop=0,' && echo ",$g6o," | grep -q ',panicnil=0,' && [ "$(hashtree "$F6")" = "$B6" ]
verdict MR6 $? "rc=$r6 validated=[$v6] root-stamp==go-list:[$([ "$g6r" = "$w6r" ] && echo yes || echo NO)] override-stamp==go-list:[$([ "$g6o" = "$w6o" ] && echo yes || echo NO)] root panicnil/randseednop=[$(echo ",$g6r," | grep -o ',panicnil=[01],' | tr -d ,)/$(echo ",$g6r," | grep -o ',randseednop=[01],' | tr -d ,)] override=[$(echo ",$g6o," | grep -o ',panicnil=[01],' | tr -d ,)]"
if [ "$CONTROLS" = 1 ]; then
  # MR6c (critic 0 #8, R7), the discriminating control for the -tests-host route, mirroring MR4c: the SAME gd120 fixture
  # through M4's converter (e9009f2945, before M6), built from `git archive` under $CTL (never a worktree), into its OWN
  # output root. The host compiles against the union's golib, but the M4 converter emits no DefaultGODEBUG stamp, so the
  # C# defaults are go1.24's. EXPECT rc!=0 and BOTH root tests go=pass cs=fail BY NAME in that root's comparison record.
  if X4=$(ctlconv e9009f2945 m4); then
    rm -rf "$OUT/m6c"
    leg MR6c tests-recurse-m6-control "$X4" -tests -test-action all -test-timeout 20m -recurse -go2cspath "$WB\\src" "$(cygpath -w "$F6")" "$(cygpath -w "$OUT/m6c")"
    c6c=$LEG_RC
    hp cmpnames "$(cygpath -w "$OUT/m6c/src/example.test/gd120/go2cs_test_comparison.json")" pass fail TestPanicNilKeepsGo120Behavior TestRandSeedIsHonored > "$LOGDIR/m6c-names.txt" 2>&1; n6c=$?
    [ "$c6c" != 0 ] && [ "$n6c" = 0 ]
    verdict "MR6c (EXPECT rc!=0 + both root tests go=pass cs=fail by name)" $? "rc=$c6c $(grep -a '^CMPNAME' "$LOGDIR/m6c-names.txt" | tr -d '\r' | sed 's/ (want[^)]*)//' | tr '\n' ' ' | cut -c1-220) (m6c-names.txt)"
  else
    stamp "  MR6c: NOT MEASURED -- the M4 converter did not build (conv-build-m4.log)"; FAILS=$((FAILS + 1))
  fi
fi

# MR6p -- the -recurse PROGRAM arm (the csproj <AssemblyAttribute> route): a go 1.20 main recovering panic(nil).
# EXPECT `go run .` and the converted program BOTH print 'recovered nil: true'; the project stamps panicnil=1.
F6p=$FX/m6/prog120
wf "$F6p/go.mod" 'module example.test/prog120\n\ngo 1.20\n'
wf "$F6p/main.go" 'package main\n\nimport "fmt"\n\nfunc main() {\n\tvar r any = "unset"\n\tfunc() {\n\t\tdefer func() { r = recover() }()\n\t\tpanic(nil)\n\t}()\n\tfmt.Println("recovered nil:", r == nil)\n}\n'
leg MR6p-oracle go-run-prog120 bash -c "cd '$F6p' && go run ."
o6=$(tr -d '\r' < "$LOGDIR/go-run-prog120.log" | tail -n 1)
rm -rf "$OUT/prog120"
leg MR6p-convert recurse-prog120 "$EXE" -recurse -go2cspath "$WB\\src" "$(cygpath -w "$F6p")" "$(cygpath -w "$OUT/prog120")"
P6=$(ls "$OUT"/prog120/src/example.test/prog120/*.csproj 2>/dev/null | head -n 1)
st6=$(grep -ac 'AssemblyAttribute Include="go.GoDefaultGodebugAttribute"' "$P6" 2>/dev/null); pn6=$(grep -ac 'panicnil=1' "$P6" 2>/dev/null)
leg MR6p-run run-prog120 bash -c "dotnet build '$(cygpath -w "$P6")' -c Release -p:UseSharedCompilation=false && dotnet run --project '$(cygpath -w "$P6")' -c Release --no-build"
rr6=$LEG_RC; c6=$(tr -d '\r' < "$LOGDIR/run-prog120.log" | grep -a '^recovered nil:' | tail -n 1)
[ "$o6" = 'recovered nil: true' ] && [ "${st6:-0}" = 1 ] && [ "${pn6:-0}" -ge 1 ] && [ "$rr6" = 0 ] && [ "$c6" = 'recovered nil: true' ]
verdict MR6p $? "go-run=[$o6] project-stamp=${st6:-0} panicnil=1-in-project=${pn6:-0} run rc=$rr6 converted=[$c6]"
# MR6e (critic 0 #7) -- r-m6's claim that internal/godebug layers the default UNDER the environment: the same stamped
# program with GODEBUG=panicnil=0 in its environment. EXPECT `go run .` and the converted program BOTH print
# 'recovered nil: false' (the env beats the go 1.20 default). The optional randseednop=1 arm on gd120 is not added.
leg MR6e-oracle go-run-prog120-env bash -c "cd '$F6p' && GODEBUG=panicnil=0 go run ."
o6e=$(tr -d '\r' < "$LOGDIR/go-run-prog120-env.log" | tail -n 1)
leg MR6e-run run-prog120-env env GODEBUG=panicnil=0 dotnet run --project "$(cygpath -w "$P6")" -c Release --no-build
re6=$LEG_RC; c6e=$(tr -d '\r' < "$LOGDIR/run-prog120-env.log" | grep -a '^recovered nil:' | tail -n 1)
[ "$o6e" = 'recovered nil: false' ] && [ "$re6" = 0 ] && [ "$c6e" = 'recovered nil: false' ]
verdict MR6e $? "GODEBUG=panicnil=0 over the panicnil=1 stamp: go-run=[$o6e] run rc=$re6 converted=[$c6e] (EXPECT both 'recovered nil: false')"
if [ "$CONTROLS" = 1 ]; then
  # MR6pc, the scope control: the SAME main converted PLAINLY (no -recurse; second positional = its output dir,
  # floor 3) carries NO stamp (the ruled M6 scope) and so keeps the corpus release's behaviour. EXPECT no
  # GoDefaultGodebug in its project and 'recovered nil: false' -- proving the stamp is what flips MR6p.
  rm -rf "$OUT/prog120-single"
  leg MR6pc-convert single-prog120 "$EXE" -go2cspath "$WB\\src" "$(cygpath -w "$F6p")" "$(cygpath -w "$OUT/prog120-single")"
  P6s=$(ls "$OUT"/prog120-single/*.csproj 2>/dev/null | head -n 1); ss6=$(grep -ac 'GoDefaultGodebug' "$P6s" 2>/dev/null)
  leg MR6pc-run run-prog120-single bash -c "dotnet build '$(cygpath -w "$P6s")' -c Release -p:UseSharedCompilation=false -p:go2csPath='$WB\\src\\' && dotnet run --project '$(cygpath -w "$P6s")' -c Release --no-build -p:go2csPath='$WB\\src\\'"
  cs6=$(tr -d '\r' < "$LOGDIR/run-prog120-single.log" | grep -a '^recovered nil:' | tail -n 1)
  # The label carries no ':' (critic 0 #1: the battery's old grep stopped at the colon inside 'recovered nil: false').
  [ "${ss6:-1}" = 0 ] && [ "$cs6" = 'recovered nil: false' ]; verdict "MR6pc (EXPECT no stamp, recovered nil false)" $? "stamp-lines=${ss6:-?} converted=[$cs6]"
fi
unset MSYS_NO_PATHCONV

# =========================================================================================== cleanup + END
cleanup_tree
left=$(git status --porcelain | grep -vc '^??'); td=$(git status --porcelain | grep -c '^ D')
stamp "END failures=$FAILS"
stamp "END tracked-changes=$left tracked-deletions=$td head=$(git rev-parse --short=10 HEAD) (fixtures + outputs kept in $SD/fx and $SD/out for reading; delete after)"
stamp "MODULE LEGS DONE"
# critic 0 #1 / critic 1 #1: a failed verdict must reach the battery's NONZERO LEGS (exit 4); tracked changes or a
# deletion left behind are a failure of this script too.
[ "$FAILS" = 0 ] && [ "$left" = 0 ] && [ "$td" = 0 ] || exit 4
exit 0
