#!/usr/bin/env bash
# v3rr.sh -- item 7 §11.6, the v3 suite RERUN (COORD ruling 2026-11-06): Channel, Map, StringView (the window's disturbed rows)
# and StringMatch, IfaceShellChurn (replication of the two unexpected slower rows). The same D26/V3 images and arms as
# v3suite.sh (re-verified by sha256 here), 21 rounds per arm, order alternating; the Fib/Sieve control at start, middle and end
# after one discarded warm-up; the box census at both ends. Validity, fixed BEFORE the run: StringMatch and IfaceShellChurn keep
# A/A < 3%; Channel, Map, StringView use A/A < 3% OR < the larger IQR of the pair (noise-bound, no claim unless MOVED).
S=/c/go2cs/scratch/p1262; V=$S/v3suite; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/v3rr.log
export DOTNET_ROOT='C:\Users\Admin\dotnet10'
w() { cygpath -w "$1"; }
fin() { echo "end $(date '+%F %T') rc=$1" >> "$L"; echo "$1" > "$S/v3rr.rc"; exit "$1"; }
rm -f "$S/v3rr.rc"; echo "start $(date '+%F %T')" > "$L"
P1='PerfChannel|PerfMap|PerfStringView'; P2='PerfStringMatch|PerfIfaceShellChurn'
n=0
while read -r sum prog; do
  echo "$prog" | grep -qE "^($P1|$P2)\$" || continue
  got=$(sha256sum "$V/$prog/$prog.exe" | cut -d' ' -f1); [ "$got" = "$sum" ] || { echo "v3 sha MISMATCH $prog" >> "$L"; fin 3; }
  n=$((n + 1))
done < "$V/sums.txt"
[ "$n" = 5 ] || { echo "expected 5 programs, got $n" >> "$L"; fin 3; }
echo "verified $n v3 images" >> "$L"
awk -F'\t' -v re="^($P1)\$" '$1 ~ re' "$S/arms-v3s.tsv" > "$S/arms-v3rr-1.tsv"
awk -F'\t' -v re="^($P2)\$" '$1 ~ re' "$S/arms-v3s.tsv" > "$S/arms-v3rr-2.tsv"
echo "arms $(wc -l < "$S/arms-v3rr-1.tsv") + $(wc -l < "$S/arms-v3rr-2.tsv")" >> "$L"
[ "$(wc -l < "$S/arms-v3rr-1.tsv")" = 12 ] && [ "$(wc -l < "$S/arms-v3rr-2.tsv")" = 8 ] || { echo "arm count wrong" >> "$L"; fin 3; }
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
for x in "arms-w2-control.tsv 11 v3rr-warm-ctl.tsv" "arms-w2-control.tsv 11 v3rr-ctl-start.tsv" "arms-v3rr-1.tsv 21 v3rr-arms-1.tsv" \
         "arms-w2-control.tsv 11 v3rr-ctl-mid.tsv" "arms-v3rr-2.tsv 21 v3rr-arms-2.tsv" "arms-w2-control.tsv 11 v3rr-ctl-end.tsv"; do
  set -- $x; echo "== $3 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" "$2" "$(w "$S/$3")" >> "$L" 2>&1 || { echo "FAILED $3" >> "$L"; fin 4; }
done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
fin 0
