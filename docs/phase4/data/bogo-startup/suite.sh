#!/usr/bin/env bash
# The whole BoGo run exactly as TestBogoSuite spells it (bogo_shim_test.go:576-590: same runner args, go test's
# default 10-minute wall), driven directly so the runner's progress line and every shim launch are on record.
# The shim path is the timing wrapper around the converted host. usage: suite.sh <label> <host exe> <off|on>
set -u
. "$KIT/env.sh"
LABEL=$1; HOST=$2; TIER=$3
R=$GOMODCACHE/boringssl.googlesource.com/boringssl.git@v0.0.0-20250620172916-f51d8b099832/ssl/test/runner
OUT=$KIT/suite-$LABEL; rm -rf $OUT; mkdir -p $OUT; : > $OUT/shim.log
unset DOTNET_TieredCompilation; [ "$TIER" = off ] && export DOTNET_TieredCompilation=0
cd $R
t0=$(date +%s)
SHIM_REAL=$HOST SHIM_TIER=inherit SHIM_LOG=$OUT/shim.log go test . -shim-config=$GOROOT/src/crypto/tls/bogo_config.json -shim-path=$KIT/shimwrap.sh -shim-extra-flags=-bogo-mode -allow-unimplemented -loose-errors -json-output=$OUT/results.json > $OUT/runner.out 2>&1
rc=$?
echo "rc $rc wall $(( $(date +%s) - t0 ))s tier=${DOTNET_TieredCompilation:-default} launches $(grep -c . $OUT/shim.log) last $(grep -ao '[0-9]*/[0-9]*/[0-9]*/[0-9]*/[0-9]*' $OUT/runner.out | tail -1) results $( [ -s $OUT/results.json ] && echo yes || echo no)" | tee $OUT/wall.txt
