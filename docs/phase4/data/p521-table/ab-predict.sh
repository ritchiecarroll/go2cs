#!/bin/bash
O=/c/go2cs-h/scratch/p521
JIT=/c/go2cs-h/wt-p521/src/tests/Performance/PerfP521Probe/bin/Release/net10.0/PerfP521Probe.exe
R2R=$O/r2r/PerfP521Probe.exe
GO=$O/go-probe.exe
: > $O/ab.txt
for r in $(seq 0 15); do
  echo "round $r go $($GO 50 2>&1 | tr '\n' ' ')" >> $O/ab.txt
  echo "round $r jit $(DOTNET_TieredCompilation=0 $JIT 50 2>&1 | tr '\n' ' ')" >> $O/ab.txt
  echo "round $r r2r $(DOTNET_TieredCompilation=0 $R2R 50 2>&1 | tr '\n' ' ')" >> $O/ab.txt
done
echo done >> $O/ab.txt
