#!/usr/bin/env bash
# sww.sh -- the -hoist-literals SWEEP's measurement window (COORD approved 2026-11-05), one window, 11 rounds, every arm twice
# (A/A), interleaved per program with the order alternating. Per program (17), from sb.sh's builds at the shipping commit:
#   JP / JP-AA     JIT, plain (the default)              JH / JH-AA   JIT, -hoist-literals
#   RP / RP-AA     ReadyToRun, plain                     RH / RH-AA   ReadyToRun, -hoist-literals
#   JD / JD-AA     JIT, plain, CallCountingDelayMs 100   RD / RD-AA   ReadyToRun, plain, delay 100
#   JG / JG-AA     JIT, plain, DOTNET_TieredPGO=0
# Reading (section 8): every arm's value is the mean of its A and AA medians (Startup: process wall, else in-program time);
# a ratio's floor is the largest of its arms' A/A gaps and IQRs; a cell MOVES only beyond its floor, either direction.
# Control: the 1.24.13 Go Fib/Sieve at start, middle and end. Box census at both ends (census.ps1). Writes sww.log,
# sww-arms.tsv (raw), sww.rc and sww.finished last.
set -u
S=/c/go2cs/scratch/p1262; SW=$S/sw; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/sww.log
rm -f "$S/sww.rc" "$S/sww.finished"
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/dotnet10:$PATH"
fin() { echo "$1" > "$S/sww.rc"; echo "end $(date '+%F %T') rc=$1" >> "$L"; echo done > "$S/sww.finished"; exit "$1"; }
w() { cygpath -w "$1"; }
harness() { echo "== $4 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" "$2" "$(w "$S/$3")" >> "$L" 2>&1 || { echo "harness $4 FAILED" >> "$L"; fin 4; }; }
echo "start $(date '+%F %T')" > "$L"
[ "$(cat "$SW/finished" 2>/dev/null)" = 0 ] || { echo "the sweep builds are not complete" >> "$L"; fin 3; }
export MSYS_NO_PATHCONV=1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
A=$S/arms-sww.tsv; : > "$A"
row() { [ -f "$3" ] || { echo "missing $1/$2: $3" >> "$L"; fin 3; }; printf '%s\t%s\t%s\t%s\t%s\n' "$1" "$2" "$(w "$3")" "$(w "$(dirname "$3")")" "${4:-}" >> "$A"; }
for d in "$SW"/P/Perf*/; do
  p=$(basename "$d")
  for k in A AA; do s=""; [ $k = AA ] && s="-AA"
    row "$p" "JP$s" "$SW/P/$p/jit/$p.exe"; row "$p" "JH$s" "$SW/H/$p/jit/$p.exe"
    row "$p" "RP$s" "$SW/P/$p/r2r/$p.exe"; row "$p" "RH$s" "$SW/H/$p/r2r/$p.exe"
    row "$p" "JD$s" "$SW/D/$p/jit/$p.exe"; row "$p" "RD$s" "$SW/D/$p/r2r/$p.exe"
    row "$p" "JG$s" "$SW/P/$p/jit/$p.exe" "DOTNET_TieredPGO=0"
  done
done
echo "arms $(wc -l < "$A") ($(cut -f1 "$A" | sort -u | wc -l) programs)" >> "$L"
harness arms-w2-control.tsv 11 sww-warm-ctl.tsv warm-control-DISCARDED
harness arms-w2-control.tsv 11 sww-ctl-start.tsv control-start
progs=$(cut -f1 "$A" | awk '!s[$0]++'); n=$(echo "$progs" | wc -l); h=$(( (n + 1) / 2 ))
first=$(echo "$progs" | head -n $h | tr '\n' '|' | sed 's/|$//')
awk -F'\t' -v re="^($first)\$" '$1 ~ re' "$A" > "$S/arms-sww-1.tsv"; awk -F'\t' -v re="^($first)\$" '$1 !~ re' "$A" > "$S/arms-sww-2.tsv"
echo "halves: $(cut -f1 "$S/arms-sww-1.tsv" | sort -u | wc -l) + $(cut -f1 "$S/arms-sww-2.tsv" | sort -u | wc -l) programs" >> "$L"
harness arms-sww-1.tsv 11 sww-arms-1.tsv arms-first-half
harness arms-w2-control.tsv 11 sww-ctl-mid.tsv control-mid
harness arms-sww-2.tsv 11 sww-arms-2.tsv arms-second-half
harness arms-w2-control.tsv 11 sww-ctl-end.tsv control-end
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
fin 0
