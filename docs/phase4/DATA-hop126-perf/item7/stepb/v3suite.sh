#!/usr/bin/env bash
# v3suite.sh -- item 7 §11.6, the v3 SUITE window (COORD 2026-11-05; diagnostic, the shipping default untouched): every Perf
# program's x86-64-v3 AOT image (the i9, drop recipe + -p:IlcInstructionSet=x86-64-v3 only) against its timed D26 image
# (drop1262, default instruction set). Per program: D26 / D26-AA / V3 / V3-AA, 11 rounds, order alternating; the Fib/Sieve
# control at start, middle and end (one discarded warm-up first); the box census at both ends.
# Inputs: v3suite/sums.txt, "<sha256>  <Program>" per line, written from the i9's suite DONE (TlsHandshake from Step B's).
# Pull: copies each v3 image from the share into v3suite/<Program>/ and REFUSES on any sha mismatch, before timing anything.
# Writes v3suite.log, arms-v3s-{1,2}.tsv, v3s-*.tsv, v3suite.rc last. Reader: v3sread.sh.
S=/c/go2cs/scratch/p1262; V=$S/v3suite; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/v3suite.log
SH=/s/go2cs-perf/aot1262-v3/go1.26.10.2; D26=/c/go2cs-perf/drop1262/go1.26.10.2
export DOTNET_ROOT='C:\Users\Admin\dotnet10'
w() { cygpath -w "$1"; }
fin() { echo "end $(date '+%F %T') rc=$1" >> "$L"; echo "$1" > "$S/v3suite.rc"; exit "$1"; }
rm -f "$S/v3suite.rc"; echo "start $(date '+%F %T')" > "$L"
[ -s "$V/sums.txt" ] || { echo "no sums.txt" >> "$L"; fin 3; }
n=0
while read -r sum prog; do
  [ -n "$prog" ] || continue; mkdir -p "$V/$prog"
  if [ "$prog" = PerfTlsHandshake ]; then src="$SH/PerfTlsHandshake.exe"; else src="$SH/$prog/$prog.exe"; fi
  [ -f "$V/$prog/$prog.exe" ] || cp "$src" "$V/$prog/$prog.exe" || { echo "pull FAILED $prog" >> "$L"; fin 3; }
  got=$(sha256sum "$V/$prog/$prog.exe" | cut -d' ' -f1)
  [ "$got" = "$sum" ] || { echo "v3 sha MISMATCH $prog: $got" >> "$L"; fin 3; }
  [ -f "$D26/$prog.exe" ] || { echo "no D26 image for $prog" >> "$L"; fin 3; }
  n=$((n + 1))
done < "$V/sums.txt"
echo "pulled+verified $n v3 images" >> "$L"
[ "$n" = 17 ] || { echo "expected 17 programs, got $n" >> "$L"; fin 3; }
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
A=$S/arms-v3s.tsv; : > "$A"
while read -r sum prog; do
  [ -n "$prog" ] || continue
  for k in "" "-AA"; do
    printf '%s\tD26%s\t%s\t%s\n' "$prog" "$k" "$(w "$D26/$prog.exe")" "$(w "$V/$prog")" >> "$A"
    printf '%s\tV3%s\t%s\t%s\n' "$prog" "$k" "$(w "$V/$prog/$prog.exe")" "$(w "$V/$prog")" >> "$A"
  done
done < "$V/sums.txt"
progs=$(cut -f1 "$A" | awk '!s[$0]++'); h=$(( ($(echo "$progs" | wc -l) + 1) / 2 ))
first=$(echo "$progs" | head -n $h | tr '\n' '|' | sed 's/|$//')
awk -F'\t' -v re="^($first)\$" '$1 ~ re' "$A" > "$S/arms-v3s-1.tsv"; awk -F'\t' -v re="^($first)\$" '$1 !~ re' "$A" > "$S/arms-v3s-2.tsv"
echo "arms $(wc -l < "$A"); halves $(cut -f1 "$S/arms-v3s-1.tsv" | sort -u | wc -l) + $(cut -f1 "$S/arms-v3s-2.tsv" | sort -u | wc -l)" >> "$L"
for x in "arms-w2-control.tsv v3s-warm-ctl.tsv" "arms-w2-control.tsv v3s-ctl-start.tsv" "arms-v3s-1.tsv v3s-arms-1.tsv" \
         "arms-w2-control.tsv v3s-ctl-mid.tsv" "arms-v3s-2.tsv v3s-arms-2.tsv" "arms-w2-control.tsv v3s-ctl-end.tsv"; do
  set -- $x; echo "== $2 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" 11 "$(w "$S/$2")" >> "$L" 2>&1 || { echo "FAILED $2" >> "$L"; fin 4; }
done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
fin 0
