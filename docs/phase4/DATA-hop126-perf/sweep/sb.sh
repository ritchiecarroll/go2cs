#!/usr/bin/env bash
# sb.sh -- the -hoist-literals SWEEP's builds (COORD approved 2026-11-05; the owner: "Both, yes, always"), NOT timed.
# All 17 Perf* programs, Go sources equal to the shipping commit 33589333b2 (checked), each converted twice by the .2 converter
# (7871e564) with -recurse=nuget against go.* 1.26.10.2 as a consumer gets it:
#   P  plain (the default)      H  -hoist-literals
# Each conversion gets the template JIT build (dotnet build -c Release) and the template ReadyToRun publish
# (dotnet publish -c Release -r win-x64). Then two runtime-only copies of P for the thin tables: P's JIT and R2R folders
# with ONLY runtimeconfig CallCountingDelayMs 0 -> 100. Writes sw/<arm>/<prog>/{jit,r2r} and sw/build.log; the converted
# trees are purged after each program to keep disk bounded. sw/finished is written last.
set -u
S=/c/go2cs/scratch/p1262; SW=$S/sw; L=$SW/build.log; SRC=/c/go2cs/abw/g-perf-data3/src/tests/Performance
CONV=$S/go2cs-go1.26.10.2-sha.exe
rm -rf "$SW"; mkdir -p "$SW"
export DOTNET_ROOT='C:\Users\Admin\dotnet10' PATH="/c/Users/Admin/sdk/go1.26.10/bin:/c/Users/Admin/dotnet10:$PATH"
export GOTOOLCHAIN=local CGO_ENABLED=0 NUGET_PACKAGES='C:\go2cs-perf\nuget-126102' MSBUILDDISABLENODEREUSE=1 DOTNET_CLI_USE_MSBUILD_SERVER=0 UseSharedCompilation=false
unset GOROOT GOFLAGS
echo "start $(date '+%F %T') conv=$(sha256sum "$CONV" | cut -c1-16) dotnet=$(dotnet --version) go=$(go version | cut -d' ' -f3)" > "$L"
fail=0
for d in "$SRC"/Perf*/; do
  p=$(basename "$d"); ls "$d"*.go > /dev/null 2>&1 || continue
  for arm in P H; do
    flag=""; [ $arm = H ] && flag="-hoist-literals"
    w="$SW/tmp/$arm/$p"; rm -rf "$w"; mkdir -p "$w/src" "$w/out"
    cp "$d"*.go "$d"go.mod "$w/src/"
    ( cd "$w/src" && "$CONV" -recurse=nuget $flag "$(cygpath -w "$w/src")" "$(cygpath -w "$w/out")" ) >> "$L" 2>&1; rc=$?
    pd=$(dirname "$(find "$w/out" -name "$p.csproj" | head -1)")
    [ $rc -eq 0 ] && [ -d "$pd" ] || { echo "$arm/$p CONVERT FAILED rc=$rc" >> "$L"; fail=1; continue; }
    hf=$(grep -c 'static readonly @string' "$pd/$p.cs" 2>/dev/null)
    ( cd "$pd" && dotnet build -nologo -clp:ErrorsOnly -c Release "-p:OutDir=$(cygpath -w "$SW/$arm/$p/jit")\\" ) >> "$L" 2>&1; rj=$?
    ( cd "$pd" && dotnet publish -nologo -clp:ErrorsOnly -c Release -r win-x64 -o "$(cygpath -w "$SW/$arm/$p/r2r")" ) >> "$L" 2>&1; rr=$?
    echo "$arm/$p hoisted-fields=$hf jit rc=$rj r2r rc=$rr jit-dll $(sha256sum "$SW/$arm/$p/jit/$p.dll" 2>/dev/null | cut -c1-12) r2r-dll $(sha256sum "$SW/$arm/$p/r2r/$p.dll" 2>/dev/null | cut -c1-12)" >> "$L"
    [ $rj -eq 0 ] && [ $rr -eq 0 ] && [ -f "$SW/$arm/$p/jit/$p.exe" ] && [ -f "$SW/$arm/$p/r2r/$p.exe" ] || fail=1
    rm -rf "$w"
  done
  for k in jit r2r; do
    src="$SW/P/$p/$k"; dst="$SW/D/$p/$k"; [ -d "$src" ] || continue
    rm -rf "$dst"; mkdir -p "$dst"; cp -r "$src/." "$dst/"
    sed -i 's/"System.Runtime.TieredCompilation.CallCountingDelayMs": 0,/"System.Runtime.TieredCompilation.CallCountingDelayMs": 100,/' "$dst/$p.runtimeconfig.json"
    grep -q '"System.Runtime.TieredCompilation.CallCountingDelayMs": 100,' "$dst/$p.runtimeconfig.json" || { echo "D/$p/$k: delay edit FAILED" >> "$L"; fail=1; }
  done
done
rm -rf "$SW/tmp"
echo "end $(date '+%F %T') fail=$fail" >> "$L"; echo "$fail" > "$SW/finished"
