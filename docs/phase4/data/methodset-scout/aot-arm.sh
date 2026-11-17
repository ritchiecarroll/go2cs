#!/usr/bin/env bash
# aot-arm.sh ARM : Native AOT publish of PerfStartup from the scout tree (g-ms) as it stands, full trim, one job,
# with ILC memory sampled; then read the .obj's string table, the image size, and the program's output.
set -u
ARM=${1:?arm}
T=<tree>
S=<scratch>
P=$T/src/tests/Performance/PerfStartup
O=$S/$ARM; mkdir -p "$O"
export DOTNET_ROOT=<dotnet10> DOTNET_ROLL_FORWARD=Major DOTNET_CLI_TELEMETRY_OPTOUT=1
export PATH="<dotnet10>:/c/Program Files (x86)/Microsoft Visual Studio/Installer:$PATH"
echo "ARM $ARM tree $(git -C $T rev-parse --short HEAD) dirty=$(git -C $T status --porcelain | grep -v '^??' | wc -l) start $(date +%H:%M:%S)"
git -C $T diff --stat | tail -2
rm -rf "$P/obj/aot" "$P/bin/aot-build" "$P/bin/Release"
rm -f "$O/stop"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$(cygpath -w $S/sample-ilc.ps1)" -StopFile "$(cygpath -w $O/stop)" -Out "$(cygpath -w $O/ilc-peak.txt)" &
SP=$!
s=$(date +%s)
dotnet publish "$(cygpath -w $P/PerfStartup.csproj)" -c Release -p:PerfAot=true -p:TrimMode=full \
  -p:go2csPath=<tree>/src/ -o "$(cygpath -w $O/pub)" -nologo -v q > "$O/publish-log.txt" 2>&1
rc=$?
wall=$(( $(date +%s) - s ))
touch "$O/stop"; wait $SP
echo "publish rc=$rc wall=${wall}s ilc-peak(private_gb ws_gb samples)=$(cat $O/ilc-peak.txt 2>/dev/null)"
grep -a -E ' error |error [A-Z]+[0-9]+' "$O/publish-log.txt" | head -5 | cut -c1-200
obj=$(find "$P/obj/aot" -name 'PerfStartup.obj' | head -1)
echo "obj=$obj size=$(stat -c %s "$obj" 2>/dev/null)"
[ -n "$obj" ] && py -3 -I "$(cygpath -w $S/coffstr.py)" "$(cygpath -w "$obj")"
exe=$O/pub/PerfStartup.exe
echo "image=$(stat -c %s "$exe" 2>/dev/null)"
[ -f "$exe" ] && echo "run: $("$exe" 2>&1 | tr '\n' '|')"
echo "end $(date +%H:%M:%S)"
