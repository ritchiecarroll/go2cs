#!/usr/bin/env bash
# P-256 landing A/B: base (claude/r-p384-field 8a4aaba56f, the P-384 tip) and the seat (cut, 492f3344d4), interleaved per round, 3 rounds, TC=0, alone on the box.
# Arms: the field/point microbenchmark (p256bench) and the TLS handshake program (default RSA/X25519MLKEM and P-256 arms).
set -u
export DOTNET_ROOT='%USERPROFILE%\dotnet10'
export DOTNET_TieredCompilation=0
PB=/c/go2cs-tmp/r-kit/p256bench-base
PC=/c/go2cs-tmp/r-kit/p256bench
TB=/c/go2cs-tmp/r-kit/tlsp256/cs-base
TC=/c/go2cs-tmp/r-kit/tlsp256/cs-cut
O=/c/go2cs-tmp/r-kit/p256bench/union-ab
mkdir -p "$O"
for f in "$PB/out-jit/p256bench.exe" "$PB/out-r2r/p256bench.exe" "$PC/out-jit-land/p256bench.exe" "$PC/out-r2r-land/p256bench.exe" \
         "$TB/bin/Release/net10.0/TlsP256Scout.exe" "$TB/out-r2r/TlsP256Scout.exe" "$TC/bin/Release/net10.0/TlsP256Scout.exe" "$TC/out-r2r/TlsP256Scout.exe"; do
  test -f "$f" || { echo "ABORT: missing $f"; exit 2; }
done
for a in bench-base-jit bench-cut-jit bench-base-r2r bench-cut-r2r tls-base-jit tls-cut-jit tls-base-r2r tls-cut-r2r; do : > "$O/$a.txt"; done
for r in 1 2 3; do
  echo "== round $r $(date -u +%FT%TZ)"
  "$PB/out-jit/p256bench.exe" >> "$O/bench-base-jit.txt" 2>&1 || echo "rc=$? bench base jit"
  "$PC/out-jit-land/p256bench.exe" >> "$O/bench-cut-jit.txt" 2>&1 || echo "rc=$? bench cut jit"
  "$PB/out-r2r/p256bench.exe" >> "$O/bench-base-r2r.txt" 2>&1 || echo "rc=$? bench base r2r"
  "$PC/out-r2r-land/p256bench.exe" >> "$O/bench-cut-r2r.txt" 2>&1 || echo "rc=$? bench cut r2r"
  "$TB/bin/Release/net10.0/TlsP256Scout.exe" >> "$O/tls-base-jit.txt" 2>&1 || echo "rc=$? tls base jit"
  "$TC/bin/Release/net10.0/TlsP256Scout.exe" >> "$O/tls-cut-jit.txt" 2>&1 || echo "rc=$? tls cut jit"
  "$TB/out-r2r/TlsP256Scout.exe" >> "$O/tls-base-r2r.txt" 2>&1 || echo "rc=$? tls base r2r"
  "$TC/out-r2r/TlsP256Scout.exe" >> "$O/tls-cut-r2r.txt" 2>&1 || echo "rc=$? tls cut r2r"
done
echo "== base differential (oracle)"; "$PB/out-jit/p256bench.exe" check256
echo "== cut differential"; "$PC/out-jit-land/p256bench.exe" check256; "$PC/out-r2r-land/p256bench.exe" check256
DOTNET_JitDisasm=p256Mul DOTNET_JitStdOutFile=C:/go2cs-tmp/r-kit/p256bench/union-ab/jit-final-p256Mul.txt "$PC/out-jit-land/p256bench.exe" check256 > /dev/null 2>&1
echo "== R2R run-time JIT list"
DOTNET_JitStdOutFile=C:/go2cs-tmp/r-kit/p256bench/union-ab/r2r-jitsum-cut.txt DOTNET_JitDisasmSummary=1 "$PC/out-r2r-land/p256bench.exe" > /dev/null 2>&1
DOTNET_JitStdOutFile=C:/go2cs-tmp/r-kit/p256bench/union-ab/r2r-jitsum-base.txt DOTNET_JitDisasmSummary=1 "$PB/out-r2r/p256bench.exe" > /dev/null 2>&1
echo "fiat methods JIT-compiled at run time: cut $(grep -c 'fiat_package:p' "$O/r2r-jitsum-cut.txt") base $(grep -c 'fiat_package:p' "$O/r2r-jitsum-base.txt")"
echo "end $(date -u +%FT%TZ)"
