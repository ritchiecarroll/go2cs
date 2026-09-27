#!/usr/bin/env bash
# TRAIN G battery on the i7 -- the union of record (COORD). Launch from a PER-RUN COPY (floor 4): copy this directory
# (tG-seats.txt, tG-emitcheck.sh, emitdrift.py) to a run folder and run the copy; logs land beside it.
# Seats: see tG-seats.txt (18 lines), plus the fixup commit (ReadTrace block order; four StackTraceHidden forwarders).
# Legs run SEQUENTIALLY; the assembly worktree is FROZEN while this runs. Every leg's rc is captured before any pipe (floor 7).
W=/h/go2cs-tmp-coord/tG
WB='H:\go2cs-tmp-coord\tG'
EXPECT_HEAD=${EXPECT_HEAD:?}
MASTER=${MASTER:?}
SD=$(cd "$(dirname "$0")" && pwd)
LOGDIR=$SD/tG-logs
mkdir -p "$LOGDIR"
SUM="$LOGDIR/SUMMARY.txt"
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null | tr -d '\r'; }

ORIGPATH="$PATH"
export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
export GOROOT="$(cygpath -w "$HOME/sdk/go1.24.13")" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
PWSH=$HOME/.dotnet/tools/pwsh

cd "$W" || { stamp "ABORT: no worktree $W"; exit 2; }
h=$(git rev-parse --short=10 HEAD)
[ "$h" = "$EXPECT_HEAD" ] || { stamp "ABORT: HEAD $h != $EXPECT_HEAD"; exit 2; }
[ "$(go env GOROOT)" = "$(cygpath -w "$HOME/sdk/go1.24.13")" ] || { stamp "ABORT: GOROOT pin $(go env GOROOT)"; exit 2; }
while IFS='|' read -r b sha msg; do [ -z "$b" ] && continue; git merge-base --is-ancestor "$sha" HEAD || { stamp "ABORT: seat $b $sha not an ancestor"; exit 2; }; done < "$SD/tG-seats.txt"
stamp "PRE head=$h tree=$(git rev-parse --short=10 HEAD^{tree}) master=$MASTER $(go version) dotnet=$(dotnet --version | tr -d '\r') free=$(freegb)G tracked-changes=$(git status --porcelain | grep -vc '^??')"

leg(){ # name, logfile-suffix, command...
  local name=$1 suf=$2; shift 2
  local L="$LOGDIR/$suf.log" t0=$(date +%s) f
  f=$(freegb); [ "${f:-0}" -ge 30 ] || { stamp "LEG $name ABORT: free disk ${f}G < 30G"; exit 3; }
  "$@" > "$L" 2>&1 < /dev/null; local rc=$?
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

# LEG C -- the full converter suite (twin-gen, complex bridge/64, float32, generic alias, G's x/sys forward are converter seats)
leg C conv-suite bash -c 'cd src/go2cs && go test -count=1 -timeout 30m ./...'
stamp "  C: $(grep -aE '^(ok|FAIL|---)' "$LOGDIR/conv-suite.log" | tr '\n' ' ' | cut -c1-300)"

# LEG E -- whole-corpus emission check, two arms (master vs union), three targets (tG-emitcheck.sh)
leg E emitcheck env M="$MASTER" U="$EXPECT_HEAD" bash "$SD/tG-emitcheck.sh"
stamp "  E: $(grep -aE '^(EMIT|windows:|linux:|darwin:|ABORT)' "$LOGDIR/emitcheck.log" | tr '\r\n' '  ' | cut -c1-900)"

# LEG G1 -- the roster guard under pwsh 7 (DOTNET_ROOT unset, dotnet10 off PATH)
leg G1 roster env -u DOTNET_ROOT PATH="$ORIGPATH" "$PWSH" -NoProfile -File src/check-roster-format.ps1
stamp "  G1: $(tail -n 3 "$LOGDIR/roster.log" | tr '\r\n' '  ' | cut -c1-300)"

# LEG G2 -- the roster guard under Windows PowerShell 5.1
leg G2 roster51 env -u DOTNET_ROOT PATH="$ORIGPATH" powershell -NoProfile -ExecutionPolicy Bypass -File src/check-roster-format.ps1
stamp "  G2: $(tail -n 1 "$LOGDIR/roster51.log" | tr '\r\n' '  ' | cut -c1-300)"

# LEG SY -- symbol sync (twin-gen touches the symbol table's consumers)
leg SY symbol-sync env -u DOTNET_ROOT PATH="$HOME/sdk/go1.24.13/bin:$ORIGPATH" "$PWSH" -NoProfile -File src/check-symbol-sync.ps1
stamp "  SY: $(tail -n 3 "$LOGDIR/symbol-sync.log" | tr '\r\n' '  ' | cut -c1-300)"

# LEG 2b -- go2cs.slnx (golib API changes: marshal, TypeLayout, managed_impl; twins)
leg 2b go2cs-slnx dotnet build src/go2cs.slnx -c Debug -m -p:UseSharedCompilation=false --no-incremental
stamp "  2b: errors=$(grep -acE ' error (CS|MSB|NETSDK)[0-9]+' "$LOGDIR/go2cs-slnx.log") warnings-line=$(grep -aE 'Warning\(s\)|Error\(s\)' "$LOGDIR/go2cs-slnx.log" | tr '\n' ' ' | cut -c1-120)"

# LEG 2t -- dotnet test over go2cs.slnx: the configuration in which GolibTests aborted on the flag parse of a bogus -test.v
# value (2 of 2 suite runs) before the i9's flag-isolation seat; the abort must be GONE
leg 2t slnx-test timeout 14400 dotnet test src/go2cs.slnx -c Debug --no-build
stamp "  2t: bogus-lines=$(grep -ac 'for -test.v' "$LOGDIR/slnx-test.log") crashed=$(grep -aciE 'Test host process crashed|testhost.*abort' "$LOGDIR/slnx-test.log") :: $(grep -aE '(Passed|Failed)!' "$LOGDIR/slnx-test.log" | tr '\r\n' '  ' | cut -c1-700)"

# LEG GN -- GenTests (twin-gen's StrGenerator)
leg GN gentests bash -c "dotnet build src/tests/GenTests/GenTests.csproj -c Debug -p:UseSharedCompilation=false && dotnet test src/tests/GenTests/GenTests.csproj -c Debug --no-build"
stamp "  GN: $(grep -aE '(Passed|Failed)!' "$LOGDIR/gentests.log" | tail -n 1 | cut -c1-200) :: failed-names: $(grep -aE '^\s+Failed [A-Za-z]' "$LOGDIR/gentests.log" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-300)"

# LEG GT -- GolibTests, Debug and Release
for cfg in Debug Release; do
  leg GT-$cfg golibtests-$cfg bash -c "dotnet build src/tests/GolibTests/GolibTests.csproj -c $cfg -p:UseSharedCompilation=false && dotnet test src/tests/GolibTests/GolibTests.csproj -c $cfg --no-build"
  stamp "  GT-$cfg: $(grep -aE '(Passed|Failed)!' "$LOGDIR/golibtests-$cfg.log" | tail -n 1 | cut -c1-200) :: failed-names: $(grep -aE '^\s+Failed [A-Za-z]' "$LOGDIR/golibtests-$cfg.log" | sed 's/^ *Failed //' | cut -d' ' -f1 | tr '\n' ' ' | cut -c1-300)"
done
purge after-GT

# LEG 4 -- CNR
leg 4 cnr powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/check-no-regression.ps1
stamp "  4: $(grep -aiE 'NO REGRESSION|REGRESSION|byte-identical|CHANGED|verdict' "$LOGDIR/cnr.log" | tail -n 4 | tr '\r\n' '  ' | cut -c1-400)"
purge after-CNR

# LEG 5 -- the full behavioral suite
leg 5 behavioral powershell -NoProfile -ExecutionPolicy Bypass -File src/tests/Behavioral/run-behavioral.ps1 --build-timeout 10800 --build-one-timeout 900
stamp "  5: $(grep -aiE 'Transpile|Compile|Target|Output|compared|pass|fail|skip' "$LOGDIR/behavioral.log" | tail -n 8 | tr '\r\n' '  ' | cut -c1-600)"
stamp "  5 named: FuncLiteralCallerNames=$(grep -ac 'FuncLiteralCallerNames' "$LOGDIR/behavioral.log") :: FAIL lines: $(grep -aiE 'FAIL|MISMATCH|differ' "$LOGDIR/behavioral.log" | grep -aoE '[A-Z][A-Za-z0-9]+(Test|State|Names|Seam)?' | sort -u | tr '\n' ' ' | cut -c1-300)"
purge after-behavioral

# LEG H7 -- compile parity per flavour
export MSYS_NO_PATHCONV=1
for FL in windows linux darwin; do
  leg H7-$FL h7-$FL dotnet build src/go2cs-stdlib.slnx -c Debug -p:GoTargetOS=$FL --no-incremental -m -p:UseSharedCompilation=false
  L="$LOGDIR/h7-$FL.log"
  stamp "  H7-$FL: CS=$(grep -aoE 'error CS[0-9]+' "$L" | wc -l) MSB/NETSDK=$(grep -aoE 'error (MSB|NETSDK)[0-9]+' "$L" | wc -l) :: $(grep -aoE '[A-Za-z0-9_./\\-]+[.]cs[(][0-9]+,[0-9]+[)]: error CS[0-9]+' "$L" | sed 's#\\#/#g; s#.*/core/#core/#' | LC_ALL=C sort -u | head -n 5 | tr '\n' ' ')"
  purge after-H7-$FL
done
unset MSYS_NO_PATHCONV

# LEG S -- banked rows the union reaches (one row at a time, exact match)
for pkg in internal/trace internal/sync sync/atomic weak bufio bytes context crypto/ed25519 crypto/internal/fips140test crypto/md5 crypto/rand crypto/rsa crypto/sha1 crypto/sha256 crypto/sha3 crypto/sha512 database/sql encoding/binary fmt io log log/slog log/slog/internal/buffer mime net net/http/internal net/netip os slices strconv strings sync testing unicode/utf16 unicode/utf8 encoding/gob hash/maphash internal/reflectlite internal/zstd math/big net/http net/rpc net/textproto path path/filepath sort syscall time unique text/template text/template/parse math crypto/cipher crypto/tls go/types encoding/json crypto/x509 go/build flag testing/slogtest errors runtime/debug math/cmplx maps encoding/xml expvar archive/tar archive/zip go/parser go/printer go/ast unicode go/doc go/scanner go/format go/token text/tabwriter io/fs encoding/csv html/template net/url os/exec iter reflect internal/synctest os/signal; do
  n=$(echo "$pkg" | tr '/' '.')
  leg "S:$pkg" "sweep-$n" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact
  stamp "  S:$pkg: $(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT)" "$LOGDIR/sweep-$n.log" | tail -n 1 | cut -c1-200)"
done
purge after-sweep

# LEG T -- direct -tests: runtime/trace (banks at landing, out of E4), runtime/pprof and net/http/pprof (P2 seats move them), and the FULL
# windows runtime row at the union (P2 F's owed windows reading; the sink guard; TestScavenger re-read)
export MSYS_NO_PATHCONV=1
( cd src/go2cs && go build -o bin/go2cs.exe . ) > "$LOGDIR/conv-build.log" 2>&1
EXE="$W/src/go2cs/bin/go2cs.exe"
[ -x "$EXE" ] || { stamp "LEG T ABORT: no converter at $EXE"; exit 3; }
stamp "BUILD converter sha=$(sha256sum "$EXE" | cut -c1-16)"
for spec in "runtime/trace|30m|all|-test-config Release" "runtime/pprof|30m|all|-test-config Release" "net/http/pprof|30m|all|-test-config Release" "runtime|150m|all|-test-config Release"; do
  pkg=${spec%%|*}; rest=${spec#*|}; to=${rest%%|*}; rest=${rest#*|}; act=${rest%%|*}; xa=${rest#*|}; n=$(echo "$pkg" | tr '/' '.'); pb=$(echo "$pkg" | tr '/' '\\')
  leg "T:$pkg:$act" "tests-$n-$act" "$EXE" -tests -test-action "$act" -test-timeout "$to" $xa -go2cspath "$WB\\src" "$GOROOT\\src\\$pb" "$WB\\src\\core\\$pb"
  stamp "  T:$pkg:$act: $(grep -aiE 'match|diverg|agree|verdict|total|results|error CS' "$LOGDIR/tests-$n-$act.log" | tail -n 3 | tr '\r\n' '  ' | cut -c1-500)"
  for f in go2cs_test_comparison.json go2cs_test_results.json go2cs_test_results.xml; do [ -f "src/core/$pkg/$f" ] && cp "src/core/$pkg/$f" "$LOGDIR/tests-$n-$act.$f"; done
done
unset MSYS_NO_PATHCONV

stamp "END tracked-changes=$(git status --porcelain | grep -vc '^??') tracked-deletions=$(git status --porcelain | grep -c '^ D') head=$(git rev-parse --short=10 HEAD)"
stamp "BATTERY DONE"
