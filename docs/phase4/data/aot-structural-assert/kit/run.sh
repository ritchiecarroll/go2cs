#!/usr/bin/env bash
# usage: run.sh <label> <exe> [N]  -> $KIT/run-<label>/{wall.tsv,probe.tsv}; 1 warm-up + N, warm-up dropped
L=$1; X=$2; N=${3:-24}; set -u
O=$KIT/run-$L; rm -rf $O; mkdir -p $O; echo -e "i\twall_us\trc" > $O/wall.tsv; : > $O/probe.tsv
for i in $(seq 0 $N); do
  t0=$(date +%s%N); "$X" > $O/out-$i.txt 2>&1; rc=$?; t1=$(date +%s%N)
  [ $i = 0 ] && continue
  echo -e "$i\t$(( (t1-t0)/1000 ))\t$rc" >> $O/wall.tsv
  grep P1PROBE $O/out-$i.txt | sed "s/^/$i\t/" >> $O/probe.tsv
done
python3 - "$O" "$L" <<'PY'
import sys,statistics as st
O,L=sys.argv[1],sys.argv[2]
r=[l.split() for l in open(O+'/wall.tsv')][1:]; w=[int(x[1])/1000 for x in r]; q=st.quantiles(w,n=4)
out=[L,'wall median %.1f ms (%.1f-%.1f) rc %s'%(st.median(w),q[0],q[2],sorted(set(x[2] for x in r)))]
P=[l.rstrip('\n').split('\t') for l in open(O+'/probe.tsv')]
if P:
    keys=P[0][2::2]
    for k_i,k in enumerate(keys):
        v=[float(p[3+2*k_i]) for p in P]; q=st.quantiles(v,n=4)
        out.append('%s %.1f (%.1f-%.1f)'%(k,st.median(v),q[0],q[2]))
print(' | '.join(out))
PY
