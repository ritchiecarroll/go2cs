#!/bin/bash
# r3-run.sh -- main-return-wait HOLD ARM: base 5d017630ae (no hold) against tip 438c43dbab (the exit hold).
# One heavy step at a time; WHEA per step. Four fresh worktrees, so no plain build follows a publish in one tree:
#   i9-mrw3-bj (base, runner JIT), i9-mrw3-tj (tip, runner JIT + Go), i9-mrw3-ba (base AOT), i9-mrw3-ta (tip AOT).
# Builds: runner rec on bj; runner rec and pad on tj; AOT publish of rec on ba, of rec and pad on ta. Each exe must
#   start (one idle run exits 0 or 2). Go's rec exe comes from tj (rec's Go source is the same at base and tip).
# Runs, through race3.sh (SPIN_S=14400, per-run spinner_alive), one session per load, rounds in the order given:
#   H3 first, as the pilot: idle, tip rec JIT, tip rec AOT, tip pad JIT, tip pad AOT, N=50 each.
#   H1: n32 and loadhi n8: base AOT, tip AOT, base JIT, tip JIT, Go, N=100 each.
#   H2: n32 and loadhi n8: tip pad JIT, tip pad AOT, N=100 each.
# Writes r3.log and a final DONE-R3-RUN line. Stops at the first failed check.
P=/c/go2cs-build/logs/pado
R=$P/r3
log=$R/r3.log
export SPIN_S=14400
whea() { powershell -NoProfile -File 'C:\go2cs-build\logs\pado\whea.ps1' -Since "$1" | tr -d '\r'; }
now() { powershell -NoProfile -Command "Get-Date -Format o" | tr -d '\r'; }
PAD=GoroutinePanicAfterDeferredDone; REC=GoroutineRecoveringWhenMainReturns
say() { echo "$(date '+%F %T') $*" >> $log; }
fail() { say "FAIL $*"; touch $R/sampler.stop; say "DONE-R3-RUN"; exit 3; }
count() { awk -F'\t' -v l="$1" -v m="$2" '$1==l && $2==m {n++; if ($4==0) z++; if ($4==2) t++; if ($7==1) a++} END {printf "N=%d exit0=%d exit2=%d alive=%d", n, z, t, a}' $P/rows.tsv; }
recov() { grep -l recovered $P/work/$1-$2/out-*.txt 2>/dev/null | wc -l; }
arm() { # name mode spin_n runs label=exe[:max]...
  local name=$1 mode=$2 sn=$3 runs=$4; shift 4
  local t=$(now) s=$(date +%s)
  say "ARM $name $mode n$sn runs=$runs START"
  SPIN_N=$sn bash $P/race3.sh $mode $runs "$@" > $R/arm-$name-$mode-n$sn.txt 2>&1
  local c=""; for x in "$@"; do l=${x%%=*}; c="$c $l:{$(count $l $mode) recovered=$(recov $l $mode)}"; done
  say "ARM $name $mode n$sn END elapsed=$(( $(date +%s)-s ))s$c [$(whea "$t")]"
}
runner() { # name tree test
  local t=$(now) s=$(date +%s)
  say "START $1"
  cmd //c 'C:\go2cs-build\logs\pado\behav.cmd' "D:\\go2cs-build\\i9-mrw3-$2" --filter $3 > $R/build-$1.txt 2>&1
  local rc=$?
  say "END $1 rc=$rc elapsed=$(( $(date +%s)-s ))s runner: $(tr -d '\r' < $R/build-$1.txt | grep -E '^(PASS|FAIL)' | tail -1) [$(whea "$t")]"
  return $rc
}
aot() { # name tree test
  local t=$(now) s=$(date +%s)
  say "START $1"
  cmd //c 'C:\go2cs-build\logs\pado\r1\r1-aot.cmd' "D:\\go2cs-build\\i9-mrw3-$2\\src\\tests\\Behavioral\\$3" $3 "C:\\go2cs-build\\logs\\pado\\r3\\$1" "D:/go2cs-build/i9-mrw3-$2/src/" > $R/build-$1.txt 2>&1
  local rc=$?
  say "END $1 rc=$rc elapsed=$(( $(date +%s)-s ))s exe-sha=$(sha256sum $R/$1/$3.exe 2>/dev/null | cut -c1-16) [$(whea "$t")]"
  [ $rc = 0 ] && [ -f $R/$1/$3.exe ]
}
copyjit() { # name tree test
  local S=D:/go2cs-build/i9-mrw3-$2/src/tests/Behavioral/$3/bin/Release
  cp -r $S/net10.0 $R/$1 || return 1
  say "COPY $1 dll=$(sha256sum $R/$1/$3.dll | cut -c1-16) golib=$(sha256sum $R/$1/golib.dll | cut -c1-16)"
}
starts() { # exe: one idle run through aff.cmd, rc must be 0 or 2
  timeout 60 cmd //c 'C:\go2cs-build\logs\pado\aff.cmd' "$(cygpath -w "$1")" > /dev/null 2>&1
  local rc=$?
  say "STARTS $(basename "$(dirname "$1")") rc=$rc"
  [ $rc = 0 ] || [ $rc = 2 ]
}
t0=$(now)
rm -f $R/sampler.stop
powershell -NoProfile -Command "\$p = Start-Process -WindowStyle Hidden -PassThru -FilePath powershell.exe -ArgumentList '-NoProfile','-File','C:\go2cs-build\logs\pado\r1\affinity-sampler.ps1','C:\go2cs-build\logs\pado\r3\affinity.tsv','C:\go2cs-build\logs\pado\r3\sampler.stop'; \$p.Id" > $R/sampler.pid
st=""; for w in bj tj ba ta; do st="$st $w=$(git -C D:/go2cs-build/i9-mrw3-$w rev-parse --short=10 HEAD)/$(git -C D:/go2cs-build/i9-mrw3-$w status --porcelain | wc -l)"; done
say "START r3 (tree=head/changes)$st"
runner runner-base-rec bj $REC || fail "runner base rec"
copyjit jit-base-rec bj $REC || fail "copy base rec"
runner runner-tip-rec tj $REC || fail "runner tip rec"
copyjit jit-tip-rec tj $REC || fail "copy tip rec"
mkdir -p $R/go-rec && cp D:/go2cs-build/i9-mrw3-tj/src/tests/Behavioral/$REC/bin/Release/Go/$REC.exe $R/go-rec/ || fail "copy go"
say "COPY go-rec exe=$(sha256sum $R/go-rec/$REC.exe | cut -c1-16)"
runner runner-tip-pad tj $PAD || fail "runner tip pad"
copyjit jit-tip-pad tj $PAD || fail "copy tip pad"
aot aot-base-rec ba $REC || fail "aot base rec"
aot aot-tip-rec ta $REC || fail "aot tip rec"
aot aot-tip-pad ta $PAD || fail "aot tip pad"
for e in jit-base-rec/$REC jit-tip-rec/$REC go-rec/$REC jit-tip-pad/$PAD aot-base-rec/$REC aot-tip-rec/$REC aot-tip-pad/$PAD; do
  starts $R/$e.exe || fail "$e does not start"
done
# H3, the pilot
arm m3-h3 idle 8 50 m3-tip-jit-rec=$R/jit-tip-rec/$REC.exe m3-tip-aot-rec=$R/aot-tip-rec/$REC.exe \
  m3-tip-jit-pad=$R/jit-tip-pad/$PAD.exe m3-tip-aot-pad=$R/aot-tip-pad/$PAD.exe
# H1
for row in "load 32" "loadhi 8"; do
  set -- $row; n=n$2
  arm m3-h1 $1 $2 100 m3-base-aot-rec-$n=$R/aot-base-rec/$REC.exe m3-tip-aot-rec-$n=$R/aot-tip-rec/$REC.exe \
    m3-base-jit-rec-$n=$R/jit-base-rec/$REC.exe m3-tip-jit-rec-$n=$R/jit-tip-rec/$REC.exe m3-go-rec-$n=$R/go-rec/$REC.exe
done
# H2
for row in "load 32" "loadhi 8"; do
  set -- $row; n=n$2
  arm m3-h2 $1 $2 100 m3-tip-jit-pad-$n=$R/jit-tip-pad/$PAD.exe m3-tip-aot-pad-$n=$R/aot-tip-pad/$PAD.exe
done
touch $R/sampler.stop
st=""; for w in bj tj ba ta; do st="$st $w=$(git -C D:/go2cs-build/i9-mrw3-$w status --porcelain | grep -vc '^??')"; done
say "END r3 tracked-changes$st [$(whea "$t0")]"
say "DONE-R3-RUN"
