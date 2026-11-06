#!/usr/bin/env bash
# PerfStartup ReadyToRun arm: the converted project's own publish, then 1 warm-up + N timed runs (wall, ns clock).
# usage: r2r.sh <label> [N]
. $KIT/env.sh; unset GOFLAGS GOMEMLIMIT
L=$1; N=${2:-24}; P=$W/src/tests/Performance/PerfStartup; O=$KIT/r2r-$L
rm -rf $O; mkdir -p $O
# a private copy of the project: its publish restores per-RID and must not touch the in-tree bin/obj the runner uses
C=$W/src/tests/Performance/_r2r_PerfStartup; rm -rf $C; mkdir -p $C; (cd $P && tar --exclude=./bin --exclude=./obj -cf - .) | (cd $C && tar xf -)
P=$C
dotnet publish $P/PerfStartup.csproj -c Release -p:go2csPath=$W/src/ -o $O/pub -nologo -v q > $O/publish.log 2>&1; rc=$?; echo "publish rc $rc"; [ $rc = 0 ] || exit 2
rm -rf $C
EXE=$O/pub/PerfStartup; ls -la $EXE | awk '{print "image", $5}'
grep -c . <(ls $O/pub) | awk '{print "files", $1}'
for i in $(seq 0 $N); do t0=$(date +%s%N); $EXE > $O/out.txt 2>&1; rc=$?; t1=$(date +%s%N); echo -e "$i\t$(( (t1-t0)/1000 ))\t$rc" >> $O/wall.tsv; done
python3 -c "
import statistics as st
r=[l.split() for l in open('$O/wall.tsv')][1:]; w=[int(x[1])/1000 for x in r]; q=st.quantiles(w,n=4)
print('r2r $L n=%d median %.1f ms p25 %.1f p75 %.1f rc %s'%(len(w),st.median(w),q[0],q[2],set(x[2] for x in r)))"
cat $O/out.txt
