#!/usr/bin/env bash
# run.sh : build sortbench for the JIT and publish it Native AOT, then run each 5 times (separate processes).
# Writes jit-N.txt, aot-N.txt and build logs beside it. Mints no verdict.
set -u
B=/c/go2cs/scratch/mn2/proto/b/n3code/sortbench
export DOTNET_ROOT=C:/Users/<user>/dotnet10 DOTNET_CLI_TELEMETRY_OPTOUT=1
export PATH="/c/Users/<user>/dotnet10:/c/Program Files (x86)/Microsoft Visual Studio/Installer:$PATH"
cd "$B" || exit 2
dotnet build sortbench.csproj -c Release -nologo -v q -p:OutDir="$(cygpath -w "$B/jit")/" > build-jit-log.txt 2>&1
echo "jit build rc=$?"
dotnet publish sortbench.csproj -c Release -r win-x64 -nologo -v q -p:PublishDir="$(cygpath -w "$B/aot")/" > build-aot-log.txt 2>&1
echo "aot publish rc=$?"
for i in 1 2 3 4 5; do
    "$B/jit/sortbench.exe" > "$B/jit-$i.txt" 2>&1
    DOTNET_TieredCompilation=0 "$B/jit/sortbench.exe" > "$B/jit-tc0-$i.txt" 2>&1
    "$B/aot/sortbench.exe" > "$B/aot-$i.txt" 2>&1
done
echo "runs done"
