#!/usr/bin/env bash
# p7b.sh -- item 7: is the i9's REPUBLISHED symbol-carrying TLS AOT image (7f2534b7..., size-equal to D26) representative of
# the timed D26 (7ac2b53c...)? D26 and the republish, each twice (A/A), 11 rounds, with the Fib/Sieve control.
S=/c/go2cs/scratch/p1262; H=$S/armab/bin/Release/net10.0/armab.exe; L=$S/p7b.log
export DOTNET_ROOT='C:\Users\Admin\dotnet10'
w() { cygpath -w "$1"; }
echo "start $(date '+%F %T')" > "$L"
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
for x in "arms-w2-control.tsv 11 p7b-warm-ctl.tsv" "arms-w2-control.tsv 11 p7b-ctl-start.tsv" "arms-p7b.tsv 11 p7b-tls.tsv" "arms-w2-control.tsv 11 p7b-ctl-end.tsv"; do
  set -- $x; echo "== $3 $(date '+%T')" >> "$L"; "$H" "$(w "$S/$1")" "$2" "$(w "$S/$3")" >> "$L" 2>&1 || { echo "FAILED $3" >> "$L"; echo 4 > "$S/p7b.rc"; exit 4; }
done
MSYS_NO_PATHCONV=1 powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(w "$S/xbox/census.ps1")" >> "$L" 2>&1
echo "end $(date '+%F %T') rc=0" >> "$L"; echo 0 > "$S/p7b.rc"
