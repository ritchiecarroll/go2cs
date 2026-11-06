#!/usr/bin/env bash
# Performance suite Startup row: Go / JIT / R2R, no AOT. usage: startup.sh <label> [runs]
. $KIT/env.sh
L=$1; N=${2:-24}
cd $W/src/tests/Performance
rm -rf PerfStartup/bin PerfStartup/obj
dotnet build PerformanceRunner/PerformanceRunner.csproj -c Debug -clp:ErrorsOnly --nologo > $KIT/runner-build-$L.log 2>&1 || { echo "runner build failed"; exit 2; }
EXE=$(ls PerformanceRunner/bin/Debug/net*/PerformanceRunner | head -1); unset GOFLAGS GOMEMLIMIT
$EXE --filter PerfStartup --no-aot --runs $N > $KIT/startup-$L.log 2>&1
rc=$?; echo "startup $L rc $rc"; tail -25 $KIT/startup-$L.log
