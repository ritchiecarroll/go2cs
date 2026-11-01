#!/bin/bash
# p6-run.sh -- main-return-wait P6, the SyscallN(GetCurrentProcessId) microbench (the tip's kit/syscallbench).
# The tip's converter (built from a never-built tip checkout) converts the bench against three roots, through the
# seat #53 P1 kit's convert-and-build.sh: base = a 2e619b9966 checkout's src, aa = the same root built again, tip = the
# never-built tip checkout's src. JIT and Native AOT (win-x64). Then drive.py, 15 rounds per runtime.
# One heavy step at a time; WHEA per step. Writes p6.log and a final DONE-P6-RUN line.
Q=/c/go2cs-build/logs/pado/r1/p6
W=D:/go2cs-build/i9-mrw-p6
# The pushed copy writes the profile folder as %USERPROFILE%; the run spelled it out.
GR='%USERPROFILE%\sdk\go1.26.10'
log=$Q/p6.log
whea() { powershell -NoProfile -File 'C:\go2cs-build\logs\pado\whea.ps1' -Since "$1" | tr -d '\r'; }
now() { powershell -NoProfile -Command "Get-Date -Format o" | tr -d '\r'; }
say() { echo "$(date '+%F %T') $*" >> $log; }
step() { # name, command...
  local name=$1; shift; local t=$(now) s=$(date +%s)
  say "START $name"
  "$@" > $Q/step-$name.txt 2>&1; local rc=$?
  say "END $name rc=$rc elapsed=$(( $(date +%s)-s ))s [$(whea "$t")]"
  return $rc
}
[ -e $W ] && { say "refusing: $W exists"; say "DONE-P6-RUN"; exit 2; }
mkdir -p $W
t0=$(now)
say "START p6 base=$(git -C D:/go2cs-build/i9-mrw-base rev-parse HEAD) tip=$(git -C D:/go2cs-build/i9-mrw-tip6 rev-parse HEAD)"
say "roots clean: base=$(git -C D:/go2cs-build/i9-mrw-base status --porcelain | wc -l) tip6=$(git -C D:/go2cs-build/i9-mrw-tip6 status --porcelain | wc -l)"
step converter cmd //c 'C:\go2cs-build\logs\pado\r1\p6\p6-conv.cmd' 'D:\go2cs-build\i9-mrw-tip6\src\go2cs' 'D:\go2cs-build\i9-mrw-p6\go2cs.exe' || { say "DONE-P6-RUN"; exit 3; }
say "converter sha=$(sha256sum $W/go2cs.exe | cut -c1-16)"
for x in "base base jit" "aa base jit" "tip tip6 jit" "baseaot base aot" "aaaot base aot" "tipaot tip6 aot"; do
  set -- $x
  step arm-$1 cmd //c 'C:\go2cs-build\logs\pado\r1\p6\p6-arm.cmd' D:/go2cs-build/i9-mrw-p6/go2cs.exe "$GR" D:/go2cs-build/i9-mrw-$2/src C:/go2cs-build/logs/pado/r1/p6/syscallbench D:/go2cs-build/i9-mrw-p6/arms $1 $3 win-x64
  exe=$(tr -d '\r' < $Q/step-arm-$1.txt | tail -1)
  say "EXE $1 $exe exists=$([ -f "$(cygpath -u "$exe")" ] && echo yes || echo NO) sha=$(sha256sum "$(cygpath -u "$exe")" 2>/dev/null | cut -c1-16)"
  echo "$1	$exe" >> $Q/exes.tsv
done
: > $Q/arms-jit.tsv; : > $Q/arms-aot.tsv
while IFS=$'\t' read -r name exe; do
  case $name in
    *aot) printf 'syscallbench\t%s\t%s\n' "${name%aot}" "$exe" >> $Q/arms-aot.tsv ;;
    *) printf 'syscallbench\t%s\t%s\tDOTNET_TC_CallCountingDelayMs=0\n' "$name" "$exe" >> $Q/arms-jit.tsv ;;
  esac
done < $Q/exes.tsv
step drive-jit cmd //c 'C:\go2cs-build\logs\pado\r1\p6\p6-drive.cmd' arms-jit.tsv 15 p6-jit.tsv
step drive-aot cmd //c 'C:\go2cs-build\logs\pado\r1\p6\p6-drive.cmd' arms-aot.tsv 15 p6-aot.tsv
say "END p6 [$(whea "$t0")]"
say "DONE-P6-RUN"
