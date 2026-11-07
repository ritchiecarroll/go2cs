#!/usr/bin/env bash
# Paths: $KIT is the lane box's kit root and $TREES its worktree root (both box-local; set them to rerun).
# BoGo probe launch at one tree: pipeline convert + build of the crypto/tls test host, then 1 warm-up + 24 direct
# probe launches ($KIT/p1bg/pb/probe.sh, tiering off). usage: bogo.sh <tree> <label>
set -u
. $KIT/p1bg/env.sh
W=$1; L=$2; OUT=$KIT/p1av/bogo-$L; mkdir -p $OUT
echo "== bogo $L at $(git -C $W rev-parse --short=10 HEAD) $(date -u +%T)"
cd $W/src/go2cs && go build -o go2cs . || { echo "converter build failed"; exit 2; }
cd $W
for A in convert build; do
  $W/src/go2cs/go2cs -tests -test-action $A -test-config Release -test-timeout 60m -go2cspath $W/src $GOROOT/src/crypto/tls $W/src/core/crypto/tls > $OUT/$A.out 2>&1
  rc=$?; echo "$A rc=$rc $(date -u +%T)"; [ $rc = 0 ] || { tail -20 $OUT/$A.out; exit 2; }
done
P=$W/src/core/crypto/tls/bin/tests/publish/crypto.tls.tests
bash $KIT/p1bg/pb/probe.sh av-$L $P none 24
python3 -c "
import statistics as st
r=[l.split() for l in open('$KIT/p1bg/pb/run-av-$L/wall.tsv')][1:]; w=[int(x[1])/1000 for x in r]; q=st.quantiles(w,n=4)
print('probe $L n=%d median %.0f ms p25 %.0f p75 %.0f rc %s'%(len(w),st.median(w),q[0],q[2],set(x[2] for x in r)))"
cp -r $KIT/p1bg/pb/run-av-$L $OUT/
cd $W && git status --porcelain | grep -v '^??' | head; echo "deletes: $(git status --porcelain | grep -c '^ D')"
echo "== bogo $L done $(date -u +%T)"
