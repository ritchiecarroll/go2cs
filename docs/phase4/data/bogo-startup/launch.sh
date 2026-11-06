#!/usr/bin/env bash
# One-launch timing: N+1 runner invocations of ONE handshake test at one worker; the first is discarded (warm-up).
# usage: launch.sh <arm-label> <host exe> <off|on> [N]
set -u
. "$KIT/env.sh"
LABEL=$1; HOST=$2; TIER=$3; N=${4:-12}
TEST=${TEST:-CurveTest-Client-MLKEM-TLS13}
R=$GOMODCACHE/boringssl.googlesource.com/boringssl.git@v0.0.0-20250620172916-f51d8b099832/ssl/test/runner
OUT=$KIT/launch-$LABEL; mkdir -p $OUT; : > $OUT/shim.log
unset DOTNET_TieredCompilation
for i in $(seq 0 $N); do
  SHIM_REAL=$HOST SHIM_TIER=$TIER SHIM_LOG=$OUT/shim.log \
  bash -c "cd $R && go test -count=1 . -shim-path=$KIT/shimwrap.sh -shim-config=$GOROOT/src/crypto/tls/bogo_config.json -shim-extra-flags=-bogo-mode -allow-unimplemented -loose-errors -test=$TEST -num-workers=1" > $OUT/run-$i.out 2>&1
  echo "run $i rc $? $(grep -c . $OUT/shim.log) shim lines; $(tail -n 2 $OUT/run-$i.out | tr '\n' ' ')"
done
