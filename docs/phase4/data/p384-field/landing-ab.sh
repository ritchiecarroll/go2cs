#!/usr/bin/env bash
# Landing A/B for the P-384 seat: base (master 7fbb8fabfa images) and cut (claude/r-p384-field images), interleaved in
# every round, 3 rounds, DOTNET_TieredCompilation=0, alone on the box. Then the cut's differential and run-time JIT list.
set -u
export DOTNET_ROOT='%USERPROFILE%\dotnet10'
export DOTNET_TieredCompilation=0
B=/c/go2cs-tmp/r-kit/p384bench-base
C=/c/go2cs-tmp/r-kit/p384bench
O=/c/go2cs-tmp/r-kit/p384bench/landing
mkdir -p "$O"
for f in "$B/out-jit/p384bench.exe" "$B/out-r2r/p384bench.exe" "$C/out-jit-land/p384bench.exe" "$C/out-r2r-land/p384bench.exe"; do
  test -f "$f" || { echo "ABORT: missing $f"; exit 2; }
done
: > "$O/bench-base-jit.txt"; : > "$O/bench-cut-jit.txt"; : > "$O/bench-base-r2r.txt"; : > "$O/bench-cut-r2r.txt"
for r in 1 2 3; do
  echo "== round $r $(date -u +%FT%TZ)"
  echo "== base jit run $r" >> "$O/bench-base-jit.txt"; "$B/out-jit/p384bench.exe" >> "$O/bench-base-jit.txt" 2>&1 || echo "base jit rc=$?"
  echo "== cut jit run $r"  >> "$O/bench-cut-jit.txt";  "$C/out-jit-land/p384bench.exe" >> "$O/bench-cut-jit.txt" 2>&1 || echo "cut jit rc=$?"
  echo "== base r2r run $r" >> "$O/bench-base-r2r.txt"; "$B/out-r2r/p384bench.exe" >> "$O/bench-base-r2r.txt" 2>&1 || echo "base r2r rc=$?"
  echo "== cut r2r run $r"  >> "$O/bench-cut-r2r.txt";  "$C/out-r2r-land/p384bench.exe" >> "$O/bench-cut-r2r.txt" 2>&1 || echo "cut r2r rc=$?"
done
echo "== differential $(date -u +%FT%TZ)"
"$C/out-jit-land/p384bench.exe" check > "$O/check-cut-jit.txt" 2>&1; echo "check jit rc=$?"; cat "$O/check-cut-jit.txt"
"$C/out-r2r-land/p384bench.exe" check > "$O/check-cut-r2r.txt" 2>&1; echo "check r2r rc=$?"; cat "$O/check-cut-r2r.txt"
echo "== run-time JIT list (R2R, TC=0)"
DOTNET_JitStdOutFile='C:\go2cs-tmp\r-kit\p384bench\landing\r2r-jitsum-cut.txt' DOTNET_JitDisasmSummary=1 "$C/out-r2r-land/p384bench.exe" > /dev/null 2>&1
DOTNET_JitStdOutFile='C:\go2cs-tmp\r-kit\p384bench\landing\r2r-jitsum-base.txt' DOTNET_JitDisasmSummary=1 "$B/out-r2r/p384bench.exe" > /dev/null 2>&1
echo "cut fiat methods JIT-compiled: $(grep -c 'fiat_package:p' "$O/r2r-jitsum-cut.txt")  base: $(grep -c 'fiat_package:p' "$O/r2r-jitsum-base.txt")"
echo "== JIT listing of the cut p384Mul"
DOTNET_JitDisasm='p384Mul' DOTNET_JitStdOutFile='C:\go2cs-tmp\r-kit\p384bench\landing\jit-final-p384Mul.txt' "$C/out-jit-land/p384bench.exe" check > /dev/null 2>&1
grep -E "Total bytes|inlinees" "$O/jit-final-p384Mul.txt"
echo "end $(date -u +%FT%TZ)"
