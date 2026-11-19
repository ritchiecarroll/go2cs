#!/usr/bin/env bash
# jit-build.sh ARM : a JIT (framework-dependent) Release build of PerfStartup from the scout tree as it stands, into
# <scratch>/<ARM>-jit, then the IL type census of that folder.
set -u
ARM=${1:?arm}
T=<tree>
S=<scratch>
O=$S/$ARM-jit
export DOTNET_ROOT=<dotnet10> DOTNET_ROLL_FORWARD=Major DOTNET_CLI_TELEMETRY_OPTOUT=1
export PATH="<dotnet10>:$PATH"
rm -rf "$O"
echo "ARM $ARM tree $(git -C $T rev-parse --short HEAD) diff-lines=$(git -C $T diff | wc -l) start $(date +%H:%M:%S)"
dotnet build "$(cygpath -w $T/src/tests/Performance/PerfStartup/PerfStartup.csproj)" -c Release \
  -p:go2csPath=<tree>/src/ -p:OutDir=<scratch>/$ARM-jit/ -nologo -v q > "$S/$ARM-jit-build-log.txt" 2>&1
echo "build rc=$? end $(date +%H:%M:%S)"
grep -a -E 'error [A-Z]+[0-9]+' "$S/$ARM-jit-build-log.txt" | head -3 | cut -c1-200
[ -f "$O/PerfStartup.exe" ] && echo "run: $("$O/PerfStartup.exe" 2>&1 | tr '\n' '|')"
<dotnet-tools>/pwsh.exe -NoProfile -File "$(cygpath -w $S/typecount.ps1)" -Dir "$(cygpath -w $O)" > "$S/il-$ARM.txt" 2>&1
tail -1 "$S/il-$ARM.txt"
