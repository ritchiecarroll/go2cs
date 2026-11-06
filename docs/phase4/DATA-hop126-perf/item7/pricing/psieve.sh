#!/usr/bin/env bash
# psieve.sh -- item 7 pricing window, the SIEVE-ONLY RERUN (proposed 2026-11-06 after the window's Sieve B0 A/A read 3.86%).
# Sieve B0 and A from price/ (re-verified by sha256 here), each twice (A/A), 21 rounds, order alternating; the Fib/Sieve
# control at start and end after one discarded warm-up; the box census at both ends. Validity, fixed BEFORE the run: the
# ORIGINAL rule, every arm's A/A < 3%, no amendment, and the Fib control holding 0. Reading: A/B0 (v3sread.sh NUM=A DEN=B0).
S=/c/go2cs/scratch/p1262; P=$S/price; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/psieve.log
export DOTNET_ROOT='C:\Users\Admin\dotnet10'
w() { cygpath -w "$1"; }
fin() { echo "end $(date '+%F %T') rc=$1" >> "$L"; echo "$1" > "$S/psieve.rc"; exit "$1"; }
rm -f "$S/psieve.rc"; echo "start $(date '+%F %T')" > "$L"
n=0
while read -r sum arm prog rel; do
  [ "$prog" = PerfSieve ] || continue
  got=$(sha256sum "$P/$arm/PerfSieve.exe" | cut -d' ' -f1); [ "$got" = "$sum" ] || { echo "sha MISMATCH $arm" >> "$L"; fin 3; }; n=$((n + 1))
done < "$P/sums.txt"
[ "$n" = 2 ] || { echo "expected 2 Sieve images, got $n" >> "$L"; fin 3; }
grep -P '^PerfSieve\t' "$S/arms-price.tsv" > "$S/arms-psieve.tsv"
[ "$(wc -l < "$S/arms-psieve.tsv")" = 4 ] || { echo "arm count wrong" >> "$L"; fin 3; }
echo "verified 2 images; arms 4" >> "$L"
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
for x in "arms-w2-control.tsv 11 psieve-warm-ctl.tsv" "arms-w2-control.tsv 11 psieve-ctl-start.tsv" "arms-psieve.tsv 21 psieve-arms.tsv" "arms-w2-control.tsv 11 psieve-ctl-end.tsv"; do
  set -- $x; echo "== $3 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" "$2" "$(w "$S/$3")" >> "$L" 2>&1 || { echo "FAILED $3" >> "$L"; fin 4; }
done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
fin 0
