#!/usr/bin/env bash
# cmp.sh <mask>... -- StringMatch pair: per-symbol size, default -> v3, for every symbol a dbghelp mask matches.
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/dotnet10:$PATH"
S=/c/go2cs/scratch/p1262/smpair; F=/c/go2cs/scratch/p1262/symfind/out/symfind.exe
for m in "$@"; do
  for side in default v3; do "$F" "$(cygpath -w $S/$side/PerfStringMatch.exe)" "$(cygpath -w $S/$side)" --enum "$m" | tr -d '\r' | grep -vE "^unwind|vftable|^_ehinfo|STATICS|^_GenericDict|^_InterfaceDispatchCell|^_readonlydata" > $S/e-$side.tsv; done
  join -t$'\t' -a1 -a2 -e MISSING -o 0,1.2,2.2,1.3,2.3 <(cut -f1,2,3 $S/e-default.tsv | sort -t$'\t' -k1,1 -u) <(cut -f1,2,3 $S/e-v3.tsv | sort -t$'\t' -k1,1 -u)
done
