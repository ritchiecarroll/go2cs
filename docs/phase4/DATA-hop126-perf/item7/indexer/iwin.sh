#!/usr/bin/env bash
# iwin.sh [--pull] -- the AOT indexer-inline seat's timing window (COORD 2026-11-06). Every arm SOURCE-built at P (53b3eb88d7,
# base) and D (da3f68520d, R's AggressiveInlining on slice<T>'s int/nint/ulong getters):
#   AOT  AP/AD  TlsHandshake, Sieve, MatMul  the i9's images, inl/<P|D>/<prog>/ (pulled from the share, sha-verified)
#   JIT  JP/JD  TlsHandshake, Sieve, MatMul  G's builds, inl-src/<P|D>/<prog>/jit/ (ibuild.sh)
#   R2R  RP/RD  Startup (process wall)      G's builds, inl-src/<P|D>/PerfStartup/r2r/
# Input inl/sums.txt: "<sha256> <P|D> <Program> <share-relative path>" for every AOT file (exe + pdb), from the i9's post.
# --pull copies and verifies the AOT files, then stops. Without it: re-verifies the AOT exes against sums.txt and the
# JIT/R2R binaries against inl-src/manifest-{P,D}.sha256, REQUIRES igates.sh to pass, then each arm twice (A/A), 11 rounds,
# order alternating, in two halves; the Fib/Sieve control at start, middle and end after one discarded warm-up; census at
# both ends. Validity: every arm's A/A < 3% (original rule) and the Fib control holding 0. Readings AD/AP, JD/JP, RD/RP.
S=/c/go2cs/scratch/p1262; I=$S/inl; B=$S/inl-src; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/iwin.log
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/dotnet10:$PATH"
w() { cygpath -w "$1"; }
fin() { echo "end $(date '+%F %T') rc=$1" >> "$L"; echo "$1" > "$S/iwin.rc"; exit "$1"; }
rm -f "$S/iwin.rc"; echo "start $(date '+%F %T') $*" > "$L"
[ -s "$I/sums.txt" ] || { echo "no sums.txt" >> "$L"; fin 3; }
while read -r sum arm prog rel; do
  [ -n "$rel" ] || continue; d=$I/$arm/$prog; mkdir -p "$d"; dst=$d/$(basename "$rel")
  if [ "${1:-}" = --pull ] && [ ! -f "$dst" ]; then cp "/s/$rel" "$dst" || { echo "pull FAILED $rel" >> "$L"; fin 3; }; fi
  case "$dst" in *.pdb) [ "${1:-}" = --pull ] || continue;; esac
  [ "$(sha256sum "$dst" | cut -d' ' -f1)" = "$sum" ] || { echo "sha MISMATCH $arm $prog $(basename "$dst")" >> "$L"; fin 3; }
done < "$I/sums.txt"
echo "AOT verified: $(grep -c . "$I/sums.txt") lines" >> "$L"
[ "${1:-}" = --pull ] && fin 0
for arm in P D; do ( cd "$B/$arm" && grep -v '^#' "../manifest-$arm.sha256" | sha256sum -c --quiet ) >> "$L" 2>&1 || { echo "JIT/R2R $arm manifest MISMATCH" >> "$L"; fin 3; }; done
echo "JIT/R2R verified against manifest-P/D" >> "$L"
bash "$S/igates.sh" >> "$L" 2>&1 || { echo "GATES FAILED: no timing" >> "$L"; fin 5; }
A1=$S/arms-iwin-1.tsv; A2=$S/arms-iwin-2.tsv; : > "$A1"; : > "$A2"
row() { [ -f "$4" ] || { echo "missing $4" >> "$L"; fin 3; }; for k in "" "-AA"; do printf '%s\t%s%s\t%s\t%s\n' "$2" "$3" "$k" "$(w "$4")" "$(w "$(dirname "$4")")" >> "$1"; done; }
for arm in P D; do
  row "$A1" PerfTlsHandshake A$arm "$I/$arm/PerfTlsHandshake/PerfTlsHandshake.exe"
  row "$A1" PerfTlsHandshake J$arm "$B/$arm/PerfTlsHandshake/jit/PerfTlsHandshake.exe"
  for p in PerfSieve PerfMatMul; do row "$A2" $p A$arm "$I/$arm/$p/$p.exe"; row "$A2" $p J$arm "$B/$arm/$p/jit/$p.exe"; done
  row "$A2" PerfStartup R$arm "$B/$arm/PerfStartup/r2r/PerfStartup.exe"
done
echo "arms $(wc -l < "$A1") + $(wc -l < "$A2")" >> "$L"
[ "$(wc -l < "$A1")" = 8 ] && [ "$(wc -l < "$A2")" = 20 ] || { echo "arm count wrong" >> "$L"; fin 3; }
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
for x in "arms-w2-control.tsv iwin-warm-ctl.tsv" "arms-w2-control.tsv iwin-ctl-start.tsv" "arms-iwin-1.tsv iwin-arms-1.tsv" \
         "arms-w2-control.tsv iwin-ctl-mid.tsv" "arms-iwin-2.tsv iwin-arms-2.tsv" "arms-w2-control.tsv iwin-ctl-end.tsv"; do
  set -- $x; echo "== $2 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" 11 "$(w "$S/$2")" >> "$L" 2>&1 || { echo "FAILED $2" >> "$L"; fin 4; }
done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
fin 0
