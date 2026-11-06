#!/usr/bin/env bash
# p7.sh -- PERF-REVIEW item 7, STEP A (COORD approved 2026-11-05): TlsHandshake .2, the SAME JIT binary under runtime ISA
# knobs, against the admitted D26 AOT image, 11 rounds, in one window. Two verify groups, because AES-off makes the converted
# Go TLS stack pick ChaCha20-Poly1305 (suite 4867) instead of AES-128-GCM (4865) -- and the D26 AOT image negotiates 4865:
#   TLS        J, J-AA, J-noBMI2, J-noAVX2, J-v2a (AVX/AVX2/BMI1/BMI2/LZCNT/MOVBE off; AES/PCLMUL kept, so the AOT
#              image's own suite), D26   -- all output 4865
#   TLS-chacha J-noAES (AES/PCLMUL off), J-v2 (the plan's full v2 set)   -- both output 4867; a diagnostic group
# Control: the 1.24.13 Go Fib/Sieve at start and end.
set -u
S=/c/go2cs/scratch/p1262; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/p7.log; J=$S/i7/jit/PerfTlsHandshake.exe
D=/c/go2cs-perf/drop1262/go1.26.10.2/PerfTlsHandshake.exe
rm -f "$S/p7.rc" "$S/p7.finished"
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/dotnet10:$PATH"
fin() { echo "$1" > "$S/p7.rc"; echo "end $(date '+%F %T') rc=$1" >> "$L"; echo done > "$S/p7.finished"; exit "$1"; }
w() { cygpath -w "$1"; }
harness() { echo "== $4 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" "$2" "$(w "$S/$3")" >> "$L" 2>&1 || { echo "harness $4 FAILED" >> "$L"; fin 4; }; }
echo "start $(date '+%F %T') J sha256 $(sha256sum "$J" | cut -c1-16) jit dll $(sha256sum "$S/i7/jit/PerfTlsHandshake.dll" | cut -c1-16) D26 $(sha256sum "$D" | cut -c1-16)" > "$L"
[ "$(sha256sum "$D" | cut -d' ' -f1)" = 7ac2b53c0aa89c9399f02bb14011e3bc7f5a9a689d46ac116391b714268d32be ] || { echo "D26 is not the admitted image" >> "$L"; fin 3; }
export MSYS_NO_PATHCONV=1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
WD=$(w "$S/i7/jit"); V2="DOTNET_EnableAVX=0;DOTNET_EnableAVX2=0;DOTNET_EnableBMI1=0;DOTNET_EnableBMI2=0;DOTNET_EnableLZCNT=0;DOTNET_EnableMOVBE=0"
{
  printf '%s\t%s\t%s\t%s\t%s\n' TLS J "$(w "$J")" "$WD" ""
  printf '%s\t%s\t%s\t%s\t%s\n' TLS J-AA "$(w "$J")" "$WD" ""
  printf '%s\t%s\t%s\t%s\t%s\n' TLS J-noBMI2 "$(w "$J")" "$WD" "DOTNET_EnableBMI2=0"
  printf '%s\t%s\t%s\t%s\t%s\n' TLS J-noAVX2 "$(w "$J")" "$WD" "DOTNET_EnableAVX2=0"
  printf '%s\t%s\t%s\t%s\t%s\n' TLS J-v2a "$(w "$J")" "$WD" "$V2"
  printf '%s\t%s\t%s\t%s\t%s\n' TLS D26 "$(w "$D")" "$WD" ""
  printf '%s\t%s\t%s\t%s\t%s\n' TLS-chacha J-noAES "$(w "$J")" "$WD" "DOTNET_EnableAES=0;DOTNET_EnablePCLMULQDQ=0"
  printf '%s\t%s\t%s\t%s\t%s\n' TLS-chacha J-v2 "$(w "$J")" "$WD" "$V2;DOTNET_EnableAES=0;DOTNET_EnablePCLMULQDQ=0"
} > "$S/arms-p7.tsv"
harness arms-w2-control.tsv 11 p7-warm-ctl.tsv warm-control-DISCARDED
harness arms-w2-control.tsv 11 p7-ctl-start.tsv control-start
harness arms-p7.tsv 11 p7-tls.tsv tls
harness arms-w2-control.tsv 11 p7-ctl-end.tsv control-end
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
fin 0
