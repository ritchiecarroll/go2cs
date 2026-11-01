#!/bin/bash
# r3-run.sh -- main-return-wait R3, the controls, with G's kit/make-red.sh (copied from the tip), on a FRESH tip
# checkout (i9-mrw-tip3) that never had a Native AOT publish: the first R3 run rebuilt on the tip tree after its
# win-x64 publish, and that plain build gave a C# exe that cannot start (r3-abort.log).
#  baseline: the runner on the unpatched tree (it must PASS); its golib.dll is the same-path reference.
#  blocked:  patch, rebuild through the runner (pad), copy the exe folder out, then pad at load n32 and loadhi n8,
#            JIT N=100 each. Must REGRESS: exit 0 at least at R0's rate.
#  runnable: patch, rebuild (rec, NEW shape), copy out, then rec idle and load n32, JIT N=20 each. Must REGRESS:
#            "recovered" in every run.
#  Each patch is checked to be exactly one RED line, and each restore to give back the committed blob (hash-object).
#  Each red golib.dll must differ from the baseline's (same tree, same path), and each red exe must start: one idle
#  run must exit 0 or 2.
# Writes r3.log and a final DONE-R3-RUN line. Stops (and restores) at the first failed check.
P=/c/go2cs-build/logs/pado
R=$P/r1
T=D:/go2cs-build/i9-mrw-tip3
TW='D:\go2cs-build\i9-mrw-tip3'
F=src/core/golib/runtime/MainExit.cs
log=$R/r3.log
whea() { powershell -NoProfile -File 'C:\go2cs-build\logs\pado\whea.ps1' -Since "$1" | tr -d '\r'; }
now() { powershell -NoProfile -Command "Get-Date -Format o" | tr -d '\r'; }
PAD=GoroutinePanicAfterDeferredDone; REC=GoroutineRecoveringWhenMainReturns
h0=$(git -C $T show HEAD:$F | git hash-object --stdin)
say() { echo "$(date '+%F %T') $*" >> $log; }
fail() { say "FAIL $*"; git -C $T diff --quiet -- $F || bash $R/make-red.sh $T restore >> $log 2>&1; touch $R/sampler.stop; say "DONE-R3-RUN"; exit 3; }
count() { awk -F'\t' -v l="$1" -v m="$2" '$1==l && $2==m {n++; if ($4==0) z++; if ($7==1) a++} END {printf "N=%d exit0=%d alive=%d", n, z, a}' $P/rows.tsv; }
recov() { grep -l recovered $P/work/$1-$2/out-*.txt 2>/dev/null | wc -l; }
arm() { # label mode spin_n runs exe
  local t=$(now) s=$(date +%s)
  say "ARM $1 $2 n$3 runs=$4 START"
  SPIN_N=$3 bash $P/race3.sh $2 $4 $1=$5 > $R/arm-$1-$2.txt 2>&1
  say "ARM $1 $2 n$3 END elapsed=$(( $(date +%s)-s ))s {$(count $1 $2)} recovered=$(recov $1 $2) [$(whea "$t")]"
}
runner() { # name test
  local t=$(now) s=$(date +%s)
  say "START $1"
  cmd //c 'C:\go2cs-build\logs\pado\behav.cmd' "$TW" --filter $2 > $R/build-$1.txt 2>&1
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
red() { # arm test short
  local a=$1 p=$2 sh=$3
  git -C $T diff --quiet -- $F || fail "$F dirty before $a"
  bash $R/make-red.sh $T $a >> $log 2>&1 || fail "make-red $a rc=$?"
  local lines=$(git -C $T diff -- $F | grep -c '^[-+] ')
  say "PATCH $a diff-lines=$lines: $(git -C $T diff -- $F | grep '^+ ' | tr -s ' ')"
  [ "$lines" = 1 ] || fail "patch $a is $lines lines"
  runner rebuild-red-$a $p
  rm -rf $R/jit-red-$a-$sh
  cp -r $T/src/tests/Behavioral/$p/bin/Release/net10.0 $R/jit-red-$a-$sh || fail "copy $a"
  local g1=$(sha256sum $R/jit-red-$a-$sh/golib.dll | cut -c1-16)
  say "COPY red $a golib=$g1 baseline-golib=$gb"
  [ "$g1" != "$gb" ] || fail "red $a golib equals the baseline's: the patch did not reach the binary"
  starts $R/jit-red-$a-$sh/$p.exe || fail "red $a exe does not start"
}
restore() {
  bash $R/make-red.sh $T restore >> $log 2>&1 || fail "restore rc=$?"
  local h=$(git -C $T hash-object $F)
  say "RESTORE hash=$h committed=$h0"
  [ "$h" = "$h0" ] && git -C $T diff --quiet -- $F || fail "restore did not give back the committed blob"
}
t0=$(now)
rm -f $R/sampler.stop
powershell -NoProfile -Command "\$p = Start-Process -WindowStyle Hidden -PassThru -FilePath powershell.exe -ArgumentList '-NoProfile','-File','C:\go2cs-build\logs\pado\r1\affinity-sampler.ps1','C:\go2cs-build\logs\pado\r1\affinity-r3.tsv','C:\go2cs-build\logs\pado\r1\sampler.stop'; \$p.Id" > $R/sampler-r3.pid
say "START r3 tree=$T tip=$(git -C $T rev-parse HEAD) committed MainExit.cs blob=$h0 tracked-changes=$(git -C $T status --porcelain | grep -vc '^??')"
runner baseline-pad $PAD || fail "baseline runner did not pass"
gb=$(sha256sum $T/src/tests/Behavioral/$PAD/bin/Release/net10.0/golib.dll | cut -c1-16)
say "BASELINE golib=$gb"
red blocked $PAD pad
arm r3-blocked-pad-n32 load 32 100 $R/jit-red-blocked-pad/$PAD.exe
arm r3-blocked-pad-n8 loadhi 8 100 $R/jit-red-blocked-pad/$PAD.exe
restore
red runnable $REC rec
arm r3-runnable-rec-n8 idle 8 20 $R/jit-red-runnable-rec/$REC.exe
arm r3-runnable-rec-n32 load 32 20 $R/jit-red-runnable-rec/$REC.exe
restore
touch $R/sampler.stop
say "END r3 tracked-changes=$(git -C $T status --porcelain | grep -vc '^??') [$(whea "$t0")]"
say "DONE-R3-RUN"
