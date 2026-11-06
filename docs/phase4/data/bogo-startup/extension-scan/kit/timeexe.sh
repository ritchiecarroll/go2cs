#!/usr/bin/env bash
# 1 warm-up + N timed runs of one executable (wall, ns clock). usage: timeexe.sh <label> <exe> [N]
L=$1; EXE=$2; N=${3:-24}; O=$KIT/time-$L; rm -rf $O; mkdir -p $O
for i in $(seq 0 $N); do t0=$(date +%s%N); $EXE > $O/out.txt 2>&1; rc=$?; t1=$(date +%s%N); echo -e "$i\t$(( (t1-t0)/1000 ))\t$rc" >> $O/wall.tsv; done
python3 -c "
import statistics as st
r=[l.split() for l in open('$O/wall.tsv')][1:]; w=[int(x[1])/1000 for x in r]; q=st.quantiles(w,n=4)
print('$L n=%d median %.1f ms p25 %.1f p75 %.1f rc %s'%(len(w),st.median(w),q[0],q[2],set(x[2] for x in r)))"
