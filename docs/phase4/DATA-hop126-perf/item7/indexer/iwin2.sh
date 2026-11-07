#!/usr/bin/env bash
# iwin2.sh -- the indexer-inline JIT/R2R RE-WINDOW (COORD 2026-11-06), after the Debug-stdlib root cause: the JIT P/D arms
# (TlsHandshake, Sieve, MatMul), R2R Startup P/D in the fd1 shape (RP/RD) and in the TEMPLATE shape (TP/TD, self-contained) rebuilt by naming the csproj, from inl-src/. REFUSES unless
# inl-src/asmgate.txt reads ASMGATE PASS (0 Debug assemblies in every closure) and every binary matches manifest-{P,D}.sha256.
# Each arm twice (A/A), 11 rounds, order alternating, in two halves; the Fib/Sieve control at start, middle and end after one
# discarded warm-up; census at both ends. Validity: every arm's A/A < 3% (original rule), Fib control holding 0.
S=/c/go2cs/scratch/p1262; B=$S/inl-src; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/iwin2.log
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/dotnet10:$PATH"
w() { cygpath -w "$1"; }
fin() { echo "end $(date '+%F %T') rc=$1" >> "$L"; echo "$1" > "$S/iwin2.rc"; exit "$1"; }
rm -f "$S/iwin2.rc"; echo "start $(date '+%F %T')" > "$L"
grep -qx "ASMGATE PASS" "$B/asmgate.txt" || { echo "asmflags gate not PASS" >> "$L"; fin 5; }
cat "$B/asmgate.txt" >> "$L"
for arm in P D; do ( cd "$B/$arm" && grep -v '^#' "../manifest-$arm.sha256" | sha256sum -c --quiet ) >> "$L" 2>&1 || { echo "manifest $arm MISMATCH" >> "$L"; fin 3; }; done
echo "JIT/R2R verified against manifest-P/D" >> "$L"
A1=$S/arms-iwin2-1.tsv; A2=$S/arms-iwin2-2.tsv; : > "$A1"; : > "$A2"
row() { [ -f "$4" ] || { echo "missing $4" >> "$L"; fin 3; }; for k in "" "-AA"; do printf '%s\t%s%s\t%s\t%s\n' "$2" "$3" "$k" "$(w "$4")" "$(w "$(dirname "$4")")" >> "$1"; done; }
for arm in P D; do
  row "$A1" PerfTlsHandshake J$arm "$B/$arm/PerfTlsHandshake/jit/PerfTlsHandshake.exe"
  for p in PerfSieve PerfMatMul; do row "$A2" $p J$arm "$B/$arm/$p/jit/$p.exe"; done
  row "$A2" PerfStartup R$arm "$B/$arm/PerfStartup/r2r/PerfStartup.exe"
  row "$A2" PerfStartup T$arm "$B/$arm/PerfStartup/r2r-tmpl/PerfStartup.exe"
done
[ "$(wc -l < "$A1")" = 4 ] && [ "$(wc -l < "$A2")" = 16 ] || { echo "arm count wrong" >> "$L"; fin 3; }
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
for x in "arms-w2-control.tsv iwin2-warm-ctl.tsv" "arms-w2-control.tsv iwin2-ctl-start.tsv" "arms-iwin2-1.tsv iwin2-arms-1.tsv" \
         "arms-w2-control.tsv iwin2-ctl-mid.tsv" "arms-iwin2-2.tsv iwin2-arms-2.tsv" "arms-w2-control.tsv iwin2-ctl-end.tsv"; do
  set -- $x; echo "== $2 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" 11 "$(w "$S/$2")" >> "$L" 2>&1 || { echo "FAILED $2" >> "$L"; fin 4; }
done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
fin 0
