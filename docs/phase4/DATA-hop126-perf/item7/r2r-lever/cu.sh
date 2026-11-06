#!/usr/bin/env bash
# cu.sh -- the composite lever's discriminating arm (COORD 2026-11-06): CU = composite + EnableCompressionInSingleFile=false,
# PerfStartup only, timed beside C (composite, compressed: the SDK default under the converted template) and N (per-assembly,
# compressed). CU ~ N means compression carries the ~3 s start-up; CU ~ C means composite itself does.
# Usage: CUSHA=<exe sha256> CUMSHA=<manifest sha256> bash cu.sh. Pulls the CU publish folder whole from the share, checks every
# file against its own manifest and the manifest against CUMSHA, re-verifies the C and N exes, then times each arm twice (A/A),
# 11 rounds, order alternating, with the Fib/Sieve control at start and end (one discarded warm-up) and the census at both
# ends. Validity: every arm's A/A < 3% (original rule) and the Fib control holding 0. Readings: CU/N and CU/C (process wall).
S=/c/go2cs/scratch/p1262; P=$S/comp; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/cu.log
CUSHA="${CUSHA:?set CUSHA}"; CUMSHA="${CUMSHA:?set CUMSHA}"
export DOTNET_ROOT='C:\Users\Admin\dotnet10'
w() { cygpath -w "$1"; }
fin() { echo "end $(date '+%F %T') rc=$1" >> "$L"; echo "$1" > "$S/cu.rc"; exit "$1"; }
rm -f "$S/cu.rc"; echo "start $(date '+%F %T')" > "$L"
d=$P/CU/PerfStartup; mkdir -p "$d"
[ -f "$d/PerfStartup.exe" ] || cp -r /s/go2cs-perf/r2r-lever/CU/PerfStartup/. "$d/" || { echo "pull FAILED" >> "$L"; fin 3; }
[ "$(sha256sum "$d/manifest.sha256" | cut -d' ' -f1)" = "$CUMSHA" ] || { echo "CU manifest sha MISMATCH" >> "$L"; fin 3; }
( cd "$d" && sha256sum -c --strict --quiet manifest.sha256 ) >> "$L" 2>&1 || { echo "CU content MISMATCH" >> "$L"; fin 3; }
[ "$(sha256sum "$d/PerfStartup.exe" | cut -d' ' -f1)" = "$CUSHA" ] || { echo "CU exe sha MISMATCH" >> "$L"; fin 3; }
while read -r sum arm prog rel; do
  [ "$prog" = PerfStartup ] || continue
  [ "$(sha256sum "$P/$arm/$prog/$prog.exe" | cut -d' ' -f1)" = "$sum" ] || { echo "sha MISMATCH $arm" >> "$L"; fin 3; }
done < "$P/sums.txt"
echo "verified: CU folder ($(grep -vc '^#' "$d/manifest.sha256") entries) + C, N exes; CU exe $(stat -c %s "$d/PerfStartup.exe") bytes" >> "$L"
A=$S/arms-cu.tsv; : > "$A"
for arm in C N CU; do for k in "" "-AA"; do
  printf 'PerfStartup\t%s%s\t%s\t%s\n' "$arm" "$k" "$(w "$P/$arm/PerfStartup/PerfStartup.exe")" "$(w "$P/$arm/PerfStartup")" >> "$A"; done; done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
for x in "arms-w2-control.tsv cu-warm-ctl.tsv" "arms-w2-control.tsv cu-ctl-start.tsv" "arms-cu.tsv cu-arms.tsv" "arms-w2-control.tsv cu-ctl-end.tsv"; do
  set -- $x; echo "== $2 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" 11 "$(w "$S/$2")" >> "$L" 2>&1 || { echo "FAILED $2" >> "$L"; fin 4; }
done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
fin 0
