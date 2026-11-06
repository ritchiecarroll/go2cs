#!/bin/bash
# Interleaved A/B, the start-up role: Go, base JIT, cut JIT, base ReadyToRun, cut ReadyToRun; 1 warm-up + 15 rounds.
O=/c/go2cs-h/scratch/p521
GO=$O/go-probe.exe
BJ=/c/go2cs-h/wt-p521/src/tests/Performance/PerfP521Probe/bin/Release/net10.0/PerfP521Probe.exe
CJ=/c/go2cs-h/wt-p521/src/tests/Performance/PerfP521ProbeCut/bin/Release/net10.0/PerfP521Probe.exe
BR=$O/r2r/PerfP521Probe.exe
CR=$O/r2r-cut/PerfP521Probe.exe
: > $O/ab2.txt
for r in $(seq 0 15); do
  echo "round $r go $($GO 50 2>&1 | tr '\n' ' ')" >> $O/ab2.txt
  echo "round $r jitbase $(DOTNET_TieredCompilation=0 $BJ 50 2>&1 | tr '\n' ' ')" >> $O/ab2.txt
  echo "round $r jitcut $(DOTNET_TieredCompilation=0 $CJ 50 2>&1 | tr '\n' ' ')" >> $O/ab2.txt
  echo "round $r r2rbase $(DOTNET_TieredCompilation=0 $BR 50 2>&1 | tr '\n' ' ')" >> $O/ab2.txt
  echo "round $r r2rcut $(DOTNET_TieredCompilation=0 $CR 50 2>&1 | tr '\n' ' ')" >> $O/ab2.txt
done
echo done >> $O/ab2.txt
