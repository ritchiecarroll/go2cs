#!/usr/bin/env bash
# TRAIN F battery on the i7 -- the union of record (COORD). Launch from a PER-RUN COPY (floor 4).
# Seats: see tF-seats.txt (18 seats), plus the union commit b95278ff3a (census entries + the composed pooled-then-lent arm) and the runtime map fixup.
# Legs run SEQUENTIALLY; the assembly worktree is FROZEN while this runs. Every leg's rc is captured before any pipe (floor 7).
W=/h/go2cs-tmp-coord/tF
WB='H:\go2cs-tmp-coord\tF'
EXPECT_HEAD=${EXPECT_HEAD:?}
MASTER=${MASTER:?}
SD=${COORD_SCRATCH:?set COORD_SCRATCH to the COORD scratch directory}
LOGDIR=$SD/tF-logs
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
while IFS='|' read -r b sha msg; do [ -z "$b" ] && continue; git merge-base --is-ancestor "$sha" HEAD || { stamp "ABORT: seat $b $sha not an ancestor"; exit 2; }; done < "$SD/tF-seats.txt"
git merge-base --is-ancestor b95278ff3a HEAD || { stamp "ABORT: union commit not an ancestor"; exit 2; }
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
  for try in 1 2 3; do
    find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -exec rm -rf {} + 2>/dev/null
    left=$(find src -type d \( -name bin -o -name obj -o -name Generated \) -prune -print | wc -l)
    [ "$left" = 0 ] && break
    sleep 5   # a just-exited build can hold an empty directory for a moment (TRAIN F's first battery aborted on 2 such)
  done
  td=$(git status --porcelain | grep -c '^ D')
  stamp "PURGE($1) purged=$P remaining=$left tracked-deletions=$td free=$(freegb)G"
  [ "$left" = 0 ] && [ "$td" = 0 ] || { stamp "ABORT: purge incomplete or tracked deletions"; exit 3; }
}

stamp "PART2 resume after the purge abort (H7 windows+linux were green): H7 darwin, sweeps, -tests"
# LEG H7 -- compile parity per flavour
export MSYS_NO_PATHCONV=1
for FL in darwin; do
  leg H7-$FL h7-$FL dotnet build src/go2cs-stdlib.slnx -c Debug -p:GoTargetOS=$FL --no-incremental -m -p:UseSharedCompilation=false
  L="$LOGDIR/h7-$FL.log"
  stamp "  H7-$FL: CS=$(grep -aoE 'error CS[0-9]+' "$L" | wc -l) MSB/NETSDK=$(grep -aoE 'error (MSB|NETSDK)[0-9]+' "$L" | wc -l) :: $(grep -aoE '[A-Za-z0-9_./\\-]+[.]cs[(][0-9]+,[0-9]+[)]: error CS[0-9]+' "$L" | sed 's#\\#/#g; s#.*/core/#core/#' | LC_ALL=C sort -u | head -n 5 | tr '\n' ' ')"
  purge after-H7-$FL
done
unset MSYS_NO_PATHCONV

# LEG S -- banked rows the union reaches (one row at a time, exact match)
for pkg in internal/trace internal/sync sync/atomic weak bufio bytes context crypto/ed25519 crypto/internal/fips140test crypto/md5 crypto/rand crypto/rsa crypto/sha1 crypto/sha256 crypto/sha3 crypto/sha512 database/sql encoding/binary fmt io log log/slog log/slog/internal/buffer mime net net/http/internal net/netip os slices strconv strings sync testing unicode/utf16 unicode/utf8 encoding/gob hash/maphash internal/reflectlite internal/zstd math/big net/http net/rpc net/textproto path path/filepath sort syscall time unique text/template text/template/parse math crypto/cipher crypto/tls go/types encoding/json crypto/x509 go/build flag testing/slogtest errors runtime/debug math/cmplx maps encoding/xml expvar archive/tar archive/zip go/parser go/printer go/ast unicode go/doc go/scanner go/format go/token text/tabwriter io/fs encoding/csv html/template net/url os/exec iter; do
  n=$(echo "$pkg" | tr '/' '.')
  leg "S:$pkg" "sweep-$n" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact
  stamp "  S:$pkg: $(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT)" "$LOGDIR/sweep-$n.log" | tail -n 1 | cut -c1-200)"
done
purge after-sweep

# LEG T -- candidate rows, direct -tests (reflect; internal/synctest; runtime/pprof and net/http/pprof, the Windows readings owed; runtime convert then build)
export MSYS_NO_PATHCONV=1
( cd src/go2cs && go build -o bin/go2cs.exe . ) > "$LOGDIR/conv-build.log" 2>&1
EXE="$W/src/go2cs/bin/go2cs.exe"
[ -x "$EXE" ] || { stamp "LEG T ABORT: no converter at $EXE"; exit 3; }
stamp "BUILD converter sha=$(sha256sum "$EXE" | cut -c1-16)"
for spec in "reflect|30m|all|-test-config Release" "internal/synctest|30m|all|-test-config Release" "runtime/pprof|30m|all|-test-config Release" "net/http/pprof|30m|all|-test-config Release" "runtime|90m|convert|-test-config Release" "runtime|90m|build|-test-config Release"; do
  pkg=${spec%%|*}; rest=${spec#*|}; to=${rest%%|*}; rest=${rest#*|}; act=${rest%%|*}; xa=${rest#*|}; n=$(echo "$pkg" | tr '/' '.'); pb=$(echo "$pkg" | tr '/' '\\')
  leg "T:$pkg:$act" "tests-$n-$act" "$EXE" -tests -test-action "$act" -test-timeout "$to" $xa -go2cspath "$WB\\src" "$GOROOT\\src\\$pb" "$WB\\src\\core\\$pb"
  stamp "  T:$pkg:$act: $(grep -aiE 'match|diverg|agree|verdict|total|results|error CS' "$LOGDIR/tests-$n-$act.log" | tail -n 3 | tr '\r\n' '  ' | cut -c1-500)"
  for f in go2cs_test_comparison.json go2cs_test_results.json go2cs_test_results.xml; do [ -f "src/core/$pkg/$f" ] && cp "src/core/$pkg/$f" "$LOGDIR/tests-$n-$act.$f"; done
done
unset MSYS_NO_PATHCONV

stamp "END tracked-changes=$(git status --porcelain | grep -vc '^??') tracked-deletions=$(git status --porcelain | grep -c '^ D') head=$(git rev-parse --short=10 HEAD)"
stamp "BATTERY DONE"
