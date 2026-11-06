#!/usr/bin/env bash
# comp.sh [--pull] -- the R2R COMPOSITE lever window (COORD 2026-11-06): self-contained single-file ReadyToRun builds by the i9,
# default instruction set, arm C (crossgen2 --composite, the SDK default for self-contained single file) against arm N
# (-p:PublishReadyToRunComposite=false). Programs: PerfStartup (process wall) and PerfStringMatch (in-program time).
# Input: comp/sums.txt, "<sha256> <arm> <Program> <share-relative path>" per line, from the i9's DONE; each image runs from
# its own folder (a single-file bundle may carry side files; every file of a folder listed in sums.txt is pulled).
# --pull copies and sha-verifies everything listed, then stops. Without it: re-verifies the exes, then each arm twice (A/A),
# 11 rounds, order alternating; the Fib/Sieve control at start, middle and end after one discarded warm-up; census at both
# ends. Validity: every arm's A/A < 3% and the Fib control holding 0. Reading: N/C per program (v3sread.sh NUM=N DEN=C).
S=/c/go2cs/scratch/p1262; P=$S/comp; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/comp.log
export DOTNET_ROOT='C:\Users\Admin\dotnet10'
w() { cygpath -w "$1"; }
fin() { echo "end $(date '+%F %T') rc=$1" >> "$L"; echo "$1" > "$S/comp.rc"; exit "$1"; }
rm -f "$S/comp.rc"; echo "start $(date '+%F %T') $*" > "$L"
[ -s "$P/sums.txt" ] || { echo "no sums.txt" >> "$L"; fin 3; }
while read -r sum arm prog rel; do
  [ -n "$rel" ] || continue; d="$P/$arm/$prog"; mkdir -p "$d"; dst="$d/$(basename "$rel")"
  if [ "${1:-}" = --pull ] && [ ! -f "$dst" ]; then cp "/s/$rel" "$dst" || { echo "pull FAILED $rel" >> "$L"; fin 3; }; fi
  got=$(sha256sum "$dst" | cut -d' ' -f1); [ "$got" = "$sum" ] || { echo "sha MISMATCH $arm $prog $(basename "$dst"): $got" >> "$L"; fin 3; }
done < "$P/sums.txt"
echo "verified: $(grep -c . "$P/sums.txt") files" >> "$L"
[ "${1:-}" = --pull ] && fin 0
A=$S/arms-comp.tsv; : > "$A"
for prog in PerfStartup PerfStringMatch; do for arm in C N; do
  [ -f "$P/$arm/$prog/$prog.exe" ] || { echo "missing $arm/$prog.exe" >> "$L"; fin 3; }
  for k in "" "-AA"; do printf '%s\t%s%s\t%s\t%s\n' "$prog" "$arm" "$k" "$(w "$P/$arm/$prog/$prog.exe")" "$(w "$P/$arm/$prog")" >> "$A"; done
done; done
[ "$(wc -l < "$A")" = 8 ] || { echo "arm count wrong" >> "$L"; fin 3; }
grep -P '^PerfStartup\t' "$A" > "$S/arms-comp-1.tsv"; grep -P '^PerfStringMatch\t' "$A" > "$S/arms-comp-2.tsv"
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
for x in "arms-w2-control.tsv comp-warm-ctl.tsv" "arms-w2-control.tsv comp-ctl-start.tsv" "arms-comp-1.tsv comp-arms-1.tsv" \
         "arms-w2-control.tsv comp-ctl-mid.tsv" "arms-comp-2.tsv comp-arms-2.tsv" "arms-w2-control.tsv comp-ctl-end.tsv"; do
  set -- $x; echo "== $2 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" 11 "$(w "$S/$2")" >> "$L" 2>&1 || { echo "FAILED $2" >> "$L"; fin 4; }
done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
fin 0
