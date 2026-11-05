#!/usr/bin/env bash
# run.sh -- the CROSS-BOX literal-policy check (owner: "Run i9 check", 2026-11-05), the SAME protocol G ran in its 04:28 window
# (DATA-hop126-perf section 8.4.4), on the four SAME Native AOT images of PerfStringMatch:
#   P  plain (released golib)   H  -hoist-literals   C0  P + golib at master 9d5d67c735   C  P + golib at b061a01052 (H's seat)
# Usage (Git Bash on the box under test):  bash /c/go2cs-perf/xbox-literal/run.sh
# Needs: Git Bash, and a .NET 10 runtime for armab (set DOTNET_ROOT when .NET 10 is not the machine-wide install).
# Sequence: box census -> warm control (DISCARDED) -> control start -> 8 arms (each image twice: A/A) x 11 rounds,
# interleaved, order alternating forward/reverse -> control end -> box census -> medians + the control's bootstrap.
# Every arm is verified (exit 0, output equal to the first arm) before timing; any failure stops the run.
# Writes out/run.log, out/*.tsv (raw samples: program arm round wall_ms inner_ms peak_bytes) and out/finished.
set -u
X=$(cd "$(dirname "$0")" && pwd); O=$X/out; H=$X/armab/armab.exe; L=$O/run.log
mkdir -p "$O"; rm -f "$O/finished"
w() { cygpath -w "$1"; }
fin() { echo "end $(date '+%F %T') rc=$1" >> "$L"; echo "$1" > "$O/finished"; exit "$1"; }
harness() { echo "== $4 $(date '+%T')" >> "$L"; "$H" "$(w "$1")" "$2" "$(w "$O/$3")" >> "$L" 2>&1 || { echo "harness $4 FAILED" >> "$L"; fin 4; }; }
echo "start $(date '+%F %T') host=$(hostname) dotnet-runtime-for-armab: DOTNET_ROOT=${DOTNET_ROOT:-unset}" > "$L"
( cd "$X" && sha256sum -c manifest.sha256 ) >> "$L" 2>&1 || { echo "manifest check FAILED" >> "$L"; fin 3; }
box() {
  {
    echo "== box $1 $(date '+%F %T')"
    MSYS_NO_PATHCONV=1 powercfg /getactivescheme
    powershell.exe -NoProfile -Command "\$n = @('dotnet.exe','go.exe','MSBuild.exe','link.exe','VBCSCompiler.exe','go2cs.exe','cl.exe','ilc.exe','crossgen2.exe'); 'CPU=' + (Get-CimInstance Win32_Processor | Select-Object -First 1).Name; \$h = @(Get-Process | Where-Object { \$_.Path -and (\$n -contains [System.IO.Path]::GetFileName(\$_.Path)) }); 'build/toolchain processes: ' + \$h.Count; \$h | ForEach-Object { 'proc ' + \$_.Id + ' ' + \$_.Path }"
  } >> "$L" 2>&1
}
WD=$(w "$X")
printf '%s\t%s\t%s\t%s\n' PerfFib C "$(w "$X/control/go124-PerfFib.exe")" "$WD" PerfSieve C "$(w "$X/control/go124-PerfSieve.exe")" "$WD" > "$O/arms-control.tsv"
: > "$O/arms-sm.tsv"
for a in P H C0 C; do
  printf '%s\t%s\t%s\t%s\n' SM-aot "$a" "$(w "$X/aot/$a/PerfStringMatch.exe")" "$WD" SM-aot "$a-AA" "$(w "$X/aot/$a/PerfStringMatch.exe")" "$WD" >> "$O/arms-sm.tsv"
done
box start
harness "$O/arms-control.tsv" 11 warm-ctl.tsv warm-control-DISCARDED
harness "$O/arms-control.tsv" 11 ctl-start.tsv control-start
harness "$O/arms-sm.tsv" 11 sm-aot.tsv arms
harness "$O/arms-control.tsv" 11 ctl-end.tsv control-end
box end
{
  echo "== medians (inner_ms), 11 rounds"
  awk -F'\t' 'NR>1 {x[$2]=x[$2]" "$5} END {for (k in x) {m=split(substr(x[k],2),a," "); asort(a); md=(m%2)?a[(m+1)/2]:(a[m/2]+a[m/2+1])/2; q1=a[int(m*0.25)+1]; q3=a[int(m*0.75)+1]; printf "%s\t%.1f\tiqr %.1f%%\n", k, md, (q3-q1)/md*100}}' "$O/sm-aot.tsv" | sort
  echo "== control (Fib carries the void test)"
  bash "$X/boot.sh" "$O/ctl-start.tsv" "$O/ctl-end.tsv" PerfFib
  bash "$X/boot.sh" "$O/ctl-start.tsv" "$O/ctl-end.tsv" PerfSieve
} >> "$L" 2>&1
fin 0
