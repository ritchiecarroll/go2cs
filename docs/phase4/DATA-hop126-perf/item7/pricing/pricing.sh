#!/usr/bin/env bash
# pricing.sh [--pull] -- item 7 pricing window (COORD-approved, arms amended 2026-11-06): source-reference AOT builds by the i9.
#   B0 at f0ed6c71f6 (base)   A at 2393238b8a (R's zero-size pre-gate)   AI at 32a0372d23 (A + bits.Mul inline; TLS only)
# Programs: TlsHandshake (B0, A, AI), Sieve, MatMul, Sort (B0, A). Readings A/B0 and AI/A (v3sread.sh with NUM/DEN).
# Input: price/sums.txt, "<sha256> <arm> <Program> <share-relative path>" per line, written from the i9's DONE (PDB lines too).
# --pull: copies every listed file from the share into price/<arm>/ and REFUSES on any sha mismatch; then stops.
# Without --pull: re-verifies the exes, REQUIRES pgates.sh to pass, then times: each arm twice (A/A), 11 rounds, order
# alternating; the Fib/Sieve control at start, middle and end after one discarded warm-up; the box census at both ends.
S=/c/go2cs/scratch/p1262; P=$S/price; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/pricing.log; SH=/s
export DOTNET_ROOT='C:\Users\Admin\dotnet10'
w() { cygpath -w "$1"; }
fin() { echo "end $(date '+%F %T') rc=$1" >> "$L"; echo "$1" > "$S/pricing.rc"; exit "$1"; }
rm -f "$S/pricing.rc"; echo "start $(date '+%F %T') $*" > "$L"
[ -s "$P/sums.txt" ] || { echo "no sums.txt" >> "$L"; fin 3; }
while read -r sum arm prog rel; do
  [ -n "$rel" ] || continue; mkdir -p "$P/$arm"; dst="$P/$arm/$(basename "$rel")"
  if [ "${1:-}" = --pull ] && [ ! -f "$dst" ]; then cp "$SH/$rel" "$dst" || { echo "pull FAILED $rel" >> "$L"; fin 3; }; fi
  case "$dst" in *.pdb) [ "${1:-}" = --pull ] || continue;; esac
  got=$(sha256sum "$dst" | cut -d' ' -f1); [ "$got" = "$sum" ] || { echo "sha MISMATCH $arm $prog $(basename "$dst"): $got" >> "$L"; fin 3; }
done < "$P/sums.txt"
echo "verified: $(grep -c . "$P/sums.txt") lines" >> "$L"
[ "${1:-}" = --pull ] && fin 0
bash "$S/pgates.sh" >> "$L" 2>&1 || { echo "GATES FAILED: no timing" >> "$L"; fin 5; }
A=$S/arms-price.tsv; : > "$A"
while read -r sum arm prog rel; do
  case "$rel" in *.exe) ;; *) continue;; esac
  for k in "" "-AA"; do printf '%s\t%s%s\t%s\t%s\n' "$prog" "$arm" "$k" "$(w "$P/$arm/$prog.exe")" "$(w "$P/$arm")" >> "$A"; done
done < "$P/sums.txt"
grep -P '^PerfTlsHandshake\t' "$A" > "$S/arms-price-1.tsv"; grep -vP '^PerfTlsHandshake\t' "$A" > "$S/arms-price-2.tsv"
echo "arms $(wc -l < "$S/arms-price-1.tsv") TLS + $(wc -l < "$S/arms-price-2.tsv") others" >> "$L"
[ "$(wc -l < "$S/arms-price-1.tsv")" = 6 ] && [ "$(wc -l < "$S/arms-price-2.tsv")" = 12 ] || { echo "arm count wrong" >> "$L"; fin 3; }
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
for x in "arms-w2-control.tsv price-warm-ctl.tsv" "arms-w2-control.tsv price-ctl-start.tsv" "arms-price-1.tsv price-arms-1.tsv" \
         "arms-w2-control.tsv price-ctl-mid.tsv" "arms-price-2.tsv price-arms-2.tsv" "arms-w2-control.tsv price-ctl-end.tsv"; do
  set -- $x; echo "== $2 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" 11 "$(w "$S/$2")" >> "$L" 2>&1 || { echo "FAILED $2" >> "$L"; fin 4; }
done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
fin 0
