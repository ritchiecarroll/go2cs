#!/usr/bin/env bash
# run.sh : build n3bench for the JIT and publish it Native AOT, then run each 5 times (separate processes).
# Writes jit-N.txt and aot-N.txt and build logs beside it. Mints no verdict.
set -u
B=/c/go2cs/scratch/mn2/proto/b/n3bench
export DOTNET_ROOT=C:/Users/<user>/dotnet10 DOTNET_ROLL_FORWARD=Major DOTNET_CLI_TELEMETRY_OPTOUT=1
export PATH="/c/Users/<user>/dotnet10:/c/Program Files (x86)/Microsoft Visual Studio/Installer:$PATH"
cd "$B" || exit 2
dotnet build n3bench.csproj -c Release -nologo -v q -p:OutDir="$(cygpath -w "$B/jit")/" > build-jit.txt 2>&1
echo "jit build rc=$?"
dotnet publish n3bench.csproj -c Release -r win-x64 -nologo -v q -p:PublishDir="$(cygpath -w "$B/aot")/" > build-aot.txt 2>&1
echo "aot publish rc=$?"
for i in 1 2 3 4 5; do
    "$B/jit/n3bench.exe" > "$B/jit-$i.txt" 2>&1
    "$B/aot/n3bench.exe" > "$B/aot-$i.txt" 2>&1
done
echo "runs done"
