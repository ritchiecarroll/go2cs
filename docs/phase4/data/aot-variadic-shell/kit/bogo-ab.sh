#!/usr/bin/env bash
# Paths: $KIT is the lane box's kit root and $TREES its worktree root (both box-local; set them to rerun).
# BoGo launch re-measure, interleaved: build both hosts, keep copies, then alternate base/fix launches.
set -u
. $KIT/p1bg/env.sh
K=$KIT/p1av
build() { # <tree> <label>
  local W=$1 L=$2 OUT=$K/bogo-$2; mkdir -p $OUT
  echo "== host $L at $(git -C $W rev-parse --short=10 HEAD) $(date -u +%T)"
  (cd $W/src/go2cs && go build -o go2cs .) || return 2
  cd $W
  for A in convert build; do
    $W/src/go2cs/go2cs -tests -test-action $A -test-config Release -test-timeout 60m -go2cspath $W/src $GOROOT/src/crypto/tls $W/src/core/crypto/tls > $OUT/$A.out 2>&1
    rc=$?; echo "$A rc=$rc $(date -u +%T)"; [ $rc = 0 ] || return 2
  done
  rm -rf $K/host-$L; cp -r $W/src/core/crypto/tls/bin/tests/publish $K/host-$L
  du -sb $K/host-$L | awk '{print "host bytes", $1}'
  git checkout -q HEAD -- src/core/crypto/tls && git clean -qfd src/core/crypto/tls
  find src -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + 2>/dev/null
  echo "tree dirty (tracked): $(git status --porcelain | grep -vc '^??')"
}
build $TREES/p1bg base3 || exit 2
build $TREES/p1av fix3 || exit 2
R=$KIT/go/pkg/mod/boringssl.googlesource.com/boringssl.git@v0.0.0-20250620172916-f51d8b099832/ssl/test/runner
export DOTNET_TieredCompilation=0
O=$K/bogo-ab; rm -rf $O; mkdir -p $O; : > $O/wall.tsv
cd $R
for i in $(seq 0 32); do
  for L in base3 fix3; do
    t0=$(date +%s%N); $K/host-$L/crypto.tls.tests -bogo-mode -is-handshaker-supported > $O/out-$L-$i.txt 2>&1; rc=$?; t1=$(date +%s%N)
    echo -e "$L\t$i\t$(( (t1-t0)/1000 ))\t$rc" >> $O/wall.tsv
  done
done
python3 - <<PY
import statistics as st
rows=[l.split('\t') for l in open('$O/wall.tsv').read().split('\n') if l]
for L in ('base3','fix3'):
    w=[int(r[2])/1000 for r in rows if r[0]==L and r[1]!='0']; q=st.quantiles(w,n=4)
    print('%s n=%d median %.0f ms p25 %.0f p75 %.0f rc %s'%(L,len(w),st.median(w),q[0],q[2],set(r[3] for r in rows if r[0]==L)))
PY
cmp <(cat $O/out-base3-1.txt) <(cat $O/out-fix3-1.txt) && echo "outputs equal"
echo "== step3 done $(date -u +%T)"
