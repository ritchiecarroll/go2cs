#!/usr/bin/env bash
# D4 CENSUS, the i7's 10 TLS/net-family rows at claude/i9-d4-go-test-order 6820ab1799 (host test order = Go's).
# Controls: the same rows at the TRAIN J union (run 1, 603f51490d; crypto/tls also run 2), read at their banked counts.
# Launch from a per-run copy. One row at a time, exact match.
W=/h/go2cs-tmp-coord/tD4
WB='H:\go2cs-tmp-coord\tD4'
EXPECT_HEAD=${EXPECT_HEAD:?}
SD=$(cd "$(dirname "$0")" && pwd)
LOGDIR=$SD/tD4-logs
mkdir -p "$LOGDIR"
SUM="$LOGDIR/SUMMARY.txt"
stamp(){ echo "$(date '+%H:%M:%S') $*" | tee -a "$SUM"; }
freegb(){ powershell -NoProfile -Command "[int]((Get-PSDrive H).Free/1GB)" 2>/dev/null | tr -d '\r'; }
cfree(){ powershell -NoProfile -Command "[int]((Get-PSDrive C).Free/1GB)" 2>/dev/null | tr -d '\r'; }

export PATH="$HOME/sdk/go1.24.13/bin:$HOME/dotnet10:$PATH"
export GOROOT="$(cygpath -w "$HOME/sdk/go1.24.13")" GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export DOTNET_ROOT="$(cygpath -w "$HOME/dotnet10")" MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_TELEMETRY_OPTOUT=1
mkdir -p /h/go2cs-tmp-coord/coord-scratch/tmp /h/go-cache/go-build /h/nuget/packages
export TEMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMP="H:\go2cs-tmp-coord\coord-scratch\tmp" TMPDIR=/h/go2cs-tmp-coord/coord-scratch/tmp
export GOCACHE="H:\go-cache\go-build" GOTMPDIR="H:\go2cs-tmp-coord\coord-scratch\tmp" NUGET_PACKAGES="H:\nuget\packages"

cd "$W" || { stamp "ABORT: no worktree $W"; exit 2; }
h=$(git rev-parse --short=10 HEAD)
[ "$h" = "$EXPECT_HEAD" ] || { stamp "ABORT: HEAD $h != $EXPECT_HEAD"; exit 2; }
[ "$(go env GOROOT)" = "$(cygpath -w "$HOME/sdk/go1.24.13")" ] || { stamp "ABORT: GOROOT pin $(go env GOROOT)"; exit 2; }
git merge-base --is-ancestor d616cc686c HEAD || { stamp "ABORT: the TRAIN J union d616cc686c is not an ancestor"; exit 2; }
[ "$(git status --porcelain | grep -vc '^??')" = 0 ] || { stamp "ABORT: tracked changes before run 2"; exit 2; }
stamp "PRE D4-census (the i7 10 rows at the i9 D4 cut) head=$h tree=$(git rev-parse --short=10 HEAD^{tree}) $(go version) dotnet=$(dotnet --version | tr -d '\r') free=$(freegb)G"

leg(){ # name, logfile-suffix, command...
  local name=$1 suf=$2; shift 2
  local L="$LOGDIR/$suf.log" t0=$(date +%s) f c
  f=$(freegb); [ "${f:-0}" -ge 30 ] || { stamp "LEG $name ABORT: free disk ${f}G < 30G"; exit 3; }
  c=$(cfree); [ "${c:-0}" -ge 8 ] || { stamp "LEG $name ABORT: C: free ${c}G < 8G"; exit 3; }
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


for pkg in crypto/tls net net/http net/http/httptest net/http/httputil net/http/cgi net/http/fcgi net/rpc net/smtp crypto/x509; do
  n=$(echo "$pkg" | tr '/' '.')
  leg "S:$pkg" "sweep-$n" powershell -NoProfile -ExecutionPolicy Bypass -File src/run-validated-sweep.ps1 -Filter "$pkg" -Exact
  stamp "  S:$pkg: $(grep -aE "^\s*(PASS|FAIL|COUNT|DRIFT)" "$LOGDIR/sweep-$n.log" | tail -n 1 | cut -c1-200)"
done
purge after-sweep
stamp "END tracked-changes=$(git status --porcelain | grep -vc '^??') tracked-deletions=$(git status --porcelain | grep -c '^ D') head=$(git rev-parse --short=10 HEAD)"
stamp "D4 CENSUS DONE"
