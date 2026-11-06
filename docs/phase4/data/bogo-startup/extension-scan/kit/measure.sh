#!/usr/bin/env bash
# One measurement round at the tree's HEAD: probe host (pipeline convert+build), 24 probe launches; Startup JIT
# (runner) + Go/JIT spread + own R2R publish. usage: measure.sh <label>
L=$1; set -u
echo "== $L at $(git -C $TREE rev-parse --short=10 HEAD) $(date -u +%T)"
cd $KIT0 && (ARM=conv-$L ACTION=convert bash runA.sh && ARM=build-$L ACTION=build bash runA.sh) > $KIT/host-$L.log 2>&1 || { echo "host build FAILED"; cat $KIT/host-$L.log; exit 2; }
P=$TREE/src/core/crypto/tls/bin/tests/publish/crypto.tls.tests
bash $PB/probe.sh es-$L $P none 24
python3 -c "
import statistics as st
r=[l.split() for l in open('$PB/run-es-$L/wall.tsv')][1:]; w=[int(x[1])/1000 for x in r]; q=st.quantiles(w,n=4)
print('probe $L n=%d median %.0f ms p25 %.0f p75 %.0f rc %s'%(len(w),st.median(w),q[0],q[2],set(x[2] for x in r)))"
bash $KIT/startup.sh $L 24 | grep -E "rc|Startup \|"
S=$TREE/src/tests/Performance/PerfStartup/bin/Release
bash $KIT/timeexe.sh go-$L $S/Go/PerfStartup
bash $KIT/timeexe.sh jit-$L $S/net10.0/PerfStartup
bash $KIT/r2r.sh $L 24 | grep -E "r2r|publish"
echo "== $L done $(date -u +%T)"
