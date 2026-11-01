#!/bin/bash
# r2-run.sh -- main-return-wait re-read at G's tip 5d017630ae (GoroutineRecoveringWhenMainReturns sets GOMAXPROCS(1)).
# One heavy step at a time; WHEA per step. Two fresh worktrees: i9-mrw2-a (runner JIT + Go, and the blocked red) and
# i9-mrw2-b (the Native AOT publish only, so no plain build ever follows a publish in one tree).
# Builds:
#  1. runner on a, rec: the JIT and Go exes (copied out); its golib.dll is the same-path unpatched reference.
#  2. blocked red on a: make-red.sh (exactly one RED line), runner on a, pad, copied out. Its golib.dll must differ
#     from the reference and its exe must start (one idle run exits 0 or 2). Then restore (the committed blob).
#  3. AOT publish of rec on b (harness r1-aot.cmd's recipe).
# Runs, through race3.sh (spinner 5400 s, per-run spinner_alive):
#  A+B, one session per load, each round in order Go, JIT, AOT: idle Go N=100; load n8 Go N=100; load n32 Go N=400,
#      JIT N=100, AOT N=50; loadhi n8 Go N=100, JIT N=100, AOT N=50.
#  C:  the blocked red pad, JIT, load n32, N=400.
# Writes r2.log and a final DONE-R2-RUN line. Stops (and restores) at the first failed check.
P=/c/go2cs-build/logs/pado
R=$P/r2
A=D:/go2cs-build/i9-mrw2-a; AW='D:\go2cs-build\i9-mrw2-a'
B=D:/go2cs-build/i9-mrw2-b
F=src/core/golib/runtime/MainExit.cs
log=$R/r2.log
whea() { powershell -NoProfile -File 'C:\go2cs-build\logs\pado\whea.ps1' -Since "$1" | tr -d '\r'; }
now() { powershell -NoProfile -Command "Get-Date -Format o" | tr -d '\r'; }
PAD=GoroutinePanicAfterDeferredDone; REC=GoroutineRecoveringWhenMainReturns
h0=$(git -C $A show HEAD:$F | git hash-object --stdin)
say() { echo "$(date '+%F %T') $*" >> $log; }
fail() { say "FAIL $*"; git -C $A diff --quiet -- $F || bash $R/make-red.sh $A restore >> $log 2>&1; touch $R/sampler.stop; say "DONE-R2-RUN"; exit 3; }
count() { awk -F'\t' -v l="$1" -v m="$2" '$1==l && $2==m {n++; if ($4==0) z++; if ($7==1) a++} END {printf "N=%d exit0=%d alive=%d", n, z, a}' $P/rows.tsv; }
recov() { grep -l recovered $P/work/$1-$2/out-*.txt 2>/dev/null | wc -l; }
arm() { # name mode spin_n runs label=exe[:max]...
  local name=$1 mode=$2 sn=$3 runs=$4; shift 4
  local t=$(now) s=$(date +%s)
  say "ARM $name $mode n$sn runs=$runs START"
  SPIN_N=$sn bash $P/race3.sh $mode $runs "$@" > $R/arm-$name-$mode-n$sn.txt 2>&1
  local c=""; for x in "$@"; do l=${x%%=*}; c="$c $l:{$(count $l $mode) recovered=$(recov $l $mode)}"; done
  say "ARM $name $mode n$sn END elapsed=$(( $(date +%s)-s ))s$c [$(whea "$t")]"
}
runner() { # name test
  local t=$(now) s=$(date +%s)
  say "START $1"
  cmd //c 'C:\go2cs-build\logs\pado\behav.cmd' "$AW" --filter $2 > $R/build-$1.txt 2>&1
  local rc=$?
  say "END $1 rc=$rc elapsed=$(( $(date +%s)-s ))s runner: $(tr -d '\r' < $R/build-$1.txt | grep -E '^(PASS|FAIL)' | tail -1) [$(whea "$t")]"
  return $rc
}
starts() { # exe: one idle run through aff.cmd, rc must be 0 or 2
  timeout 60 cmd //c 'C:\go2cs-build\logs\pado\aff.cmd' "$(cygpath -w "$1")" > /dev/null 2>&1
  local rc=$?
  say "STARTS $(basename "$(dirname "$1")") rc=$rc"
  [ $rc = 0 ] || [ $rc = 2 ]
}
t0=$(now)
rm -f $R/sampler.stop
powershell -NoProfile -Command "\$p = Start-Process -WindowStyle Hidden -PassThru -FilePath powershell.exe -ArgumentList '-NoProfile','-File','C:\go2cs-build\logs\pado\r1\affinity-sampler.ps1','C:\go2cs-build\logs\pado\r2\affinity.tsv','C:\go2cs-build\logs\pado\r2\sampler.stop'; \$p.Id" > $R/sampler.pid
say "START r2 a=$(git -C $A rev-parse HEAD) b=$(git -C $B rev-parse HEAD) tracked-changes a=$(git -C $A status --porcelain | grep -vc '^??') b=$(git -C $B status --porcelain | grep -vc '^??') MainExit.cs blob=$h0"
# 1
runner runner-rec $REC || fail "runner rec did not pass"
S=$A/src/tests/Behavioral/$REC/bin/Release
cp -r $S/net10.0 $R/jit-rec && mkdir -p $R/go-rec && cp $S/Go/$REC.exe $R/go-rec/ || fail "copy rec"
gb=$(sha256sum $R/jit-rec/golib.dll | cut -c1-16)
say "COPY rec jit=$(sha256sum $R/jit-rec/$REC.dll | cut -c1-16) golib=$gb go=$(sha256sum $R/go-rec/$REC.exe | cut -c1-16)"
# 2
git -C $A diff --quiet -- $F || fail "$F dirty before the red"
bash $R/make-red.sh $A blocked >> $log 2>&1 || fail "make-red blocked rc=$?"
lines=$(git -C $A diff -- $F | grep -c '^[-+] ')
say "PATCH blocked diff-lines=$lines: $(git -C $A diff -- $F | grep '^+ ' | tr -s ' ')"
[ "$lines" = 1 ] || fail "patch is $lines lines"
runner rebuild-red-blocked $PAD
cp -r $A/src/tests/Behavioral/$PAD/bin/Release/net10.0 $R/jit-red-blocked-pad || fail "copy red"
g1=$(sha256sum $R/jit-red-blocked-pad/golib.dll | cut -c1-16)
say "COPY red blocked golib=$g1 reference-golib=$gb"
[ "$g1" != "$gb" ] || fail "red golib equals the reference: the patch did not reach the binary"
starts $R/jit-red-blocked-pad/$PAD.exe || fail "red exe does not start"
bash $R/make-red.sh $A restore >> $log 2>&1 || fail "restore rc=$?"
h=$(git -C $A hash-object $F)
say "RESTORE hash=$h committed=$h0"
{ [ "$h" = "$h0" ] && git -C $A diff --quiet -- $F; } || fail "restore did not give back the committed blob"
# 3
t=$(now); s=$(date +%s)
say "START aot-rec"
cmd //c 'C:\go2cs-build\logs\pado\r1\r1-aot.cmd' 'D:\go2cs-build\i9-mrw2-b\src\tests\Behavioral\GoroutineRecoveringWhenMainReturns' $REC 'C:\go2cs-build\logs\pado\r2\aot-rec' 'D:/go2cs-build/i9-mrw2-b/src/' > $R/build-aot-rec.txt 2>&1
rc=$?
say "END aot-rec rc=$rc elapsed=$(( $(date +%s)-s ))s exe-sha=$(sha256sum $R/aot-rec/$REC.exe 2>/dev/null | cut -c1-16) [$(whea "$t")]"
[ $rc = 0 ] && [ -f $R/aot-rec/$REC.exe ] || fail "aot publish"
starts $R/aot-rec/$REC.exe || fail "aot exe does not start"
starts $R/jit-rec/$REC.exe || fail "jit exe does not start"
starts $R/go-rec/$REC.exe || fail "go exe does not start"
# A+B
arm m2-rec idle 8 100 m2-go-rec-n8=$R/go-rec/$REC.exe
arm m2-rec load 8 100 m2-go-rec-n8=$R/go-rec/$REC.exe
arm m2-rec load 32 400 m2-go-rec-n32=$R/go-rec/$REC.exe m2-jit-rec-n32=$R/jit-rec/$REC.exe:100 m2-aot-rec-n32=$R/aot-rec/$REC.exe:50
arm m2-rec loadhi 8 100 m2-go-rec-n8=$R/go-rec/$REC.exe m2-jit-rec-n8=$R/jit-rec/$REC.exe m2-aot-rec-n8=$R/aot-rec/$REC.exe:50
# C
arm m2-blocked load 32 400 m2-blocked-pad-n32=$R/jit-red-blocked-pad/$PAD.exe
touch $R/sampler.stop
say "END r2 tracked-changes a=$(git -C $A status --porcelain | grep -vc '^??') b=$(git -C $B status --porcelain | grep -vc '^??') [$(whea "$t0")]"
say "DONE-R2-RUN"
