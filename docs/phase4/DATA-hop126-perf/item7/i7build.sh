#!/usr/bin/env bash
# i7build.sh -- item 7 Step A build (not timed): PerfTlsHandshake converted by the .2 converter (7871e564) with -recurse=nuget
# against go.* 1.26.10.2 as a consumer gets it, then the template-default JIT build (dotnet build -c Release).
set -u
S=/c/go2cs/scratch/p1262; I=$S/i7; L=$I/build.log; SRC=/c/go2cs/abw/g-perf-data3/src/tests/Performance/PerfTlsHandshake
rm -rf "$I"; mkdir -p "$I/src/PerfTlsHandshake" "$I/out"
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/sdk/go1.26.10/bin:/c/Users/Admin/dotnet10:$PATH"
export GOTOOLCHAIN=local CGO_ENABLED=0 NUGET_PACKAGES='C:\go2cs-perf\nuget-126102' MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_USE_MSBUILD_SERVER=0
unset GOROOT GOFLAGS
echo "start $(date '+%F %T') conv=$(sha256sum "$S/go2cs-go1.26.10.2-sha.exe" | cut -c1-16)" > "$L"
cp "$SRC"/*.go "$SRC/go.mod" "$I/src/PerfTlsHandshake/"
( cd "$I/src/PerfTlsHandshake" && "$S/go2cs-go1.26.10.2-sha.exe" -recurse=nuget "$(cygpath -w "$I/src/PerfTlsHandshake")" "$(cygpath -w "$I/out")" ) >> "$L" 2>&1
echo "convert rc=$?" >> "$L"
pd=$(dirname "$(find "$I/out" -name PerfTlsHandshake.csproj | head -1)")
( cd "$pd" && dotnet build -nologo -clp:ErrorsOnly -c Release "-p:OutDir=$(cygpath -w "$I/jit")\\" ) >> "$L" 2>&1
echo "jit rc=$? exe=$(ls "$I/jit/PerfTlsHandshake.exe" 2>/dev/null)" >> "$L"
grep -h '"go.lib/' $(find "$I/out" -name project.assets.json | head -1) | head -1 >> "$L"
