#!/usr/bin/env bash
# stepb.sh -- item 7 Step B (COORD 2026-11-05, diagnostic only; the shipping default is untouched): the i9's x86-64-v3 TLS AOT
# image against the timed D26 (7ac2b53c..., baseline instruction set). Each twice (A/A), 11 rounds, with the Fib/Sieve control
# at start and end (one discarded warm-up first) and the box census at both ends. Refuses to start unless the v3 image's
# sha256 equals the i9's DONE (V3SHA, set from that post) and D26's equals its recorded hash.
S=/c/go2cs/scratch/p1262; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/stepb.log
V3SHA="${V3SHA:?set V3SHA to the i9 DONE sha256}"; D26SHA=7ac2b53c0aa89c93
export DOTNET_ROOT='C:\Users\Admin\dotnet10'
w() { cygpath -w "$1"; }
rm -f "$S/stepb.rc"
echo "start $(date '+%F %T')" > "$L"
got=$(sha256sum "$S/v3/PerfTlsHandshake.exe" | cut -d' ' -f1); [ "$got" = "$V3SHA" ] || { echo "v3 sha MISMATCH $got" >> "$L"; echo 3 > "$S/stepb.rc"; exit 3; }
got=$(sha256sum /c/go2cs-perf/drop1262/go1.26.10.2/PerfTlsHandshake.exe | cut -c1-16); [ "$got" = "$D26SHA" ] || { echo "D26 sha MISMATCH $got" >> "$L"; echo 3 > "$S/stepb.rc"; exit 3; }
echo "hashes ok: v3 $V3SHA d26 $D26SHA" >> "$L"
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
for x in "arms-w2-control.tsv 11 stepb-warm-ctl.tsv" "arms-w2-control.tsv 11 stepb-ctl-start.tsv" "arms-stepb.tsv 11 stepb-tls.tsv" "arms-w2-control.tsv 11 stepb-ctl-end.tsv"; do
  set -- $x; echo "== $3 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" "$2" "$(w "$S/$3")" >> "$L" 2>&1 || { echo "FAILED $3" >> "$L"; echo 4 > "$S/stepb.rc"; exit 4; }
done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
echo "end $(date '+%F %T') rc=0" >> "$L"; echo 0 > "$S/stepb.rc"
