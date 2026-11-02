#!/usr/bin/env bash
# mrw-aot.sh -- the main-return-wait linux leg's Native AOT publishes (linux-x64, Release), one at a time, with the
# i9's recipe: the seat #53 P1 kit's aot.targets (PublishAot for the program only, never go2cs-gen) and the tree's
# go2csPath. clang 14 (Ubuntu 22.04) is the platform linker, installed 2026-11-01 with the owner's approval.
set -u
GOROOT=/root/sdk/go1.26.10
R=/root/h10-mrw
LOG="$R/build.log"
stamp(){ echo "$(date -u '+%Y-%m-%d %H:%M:%SZ') $*" | tee -a "$LOG"; }
abort(){ stamp "ABORT: $*"; echo "MRW-AOT-DONE rc=2" >> "$LOG"; exit 2; }
export GOROOT GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export PATH="$GOROOT/bin:/root/dotnet10:$PATH"
export DOTNET_ROOT=/root/dotnet10 DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 MSBUILDDISABLENODEREUSE=1
unset GoTargetOS GO2CS_MODULE_ROOT DOTNET_TieredCompilation
grep -q 'MRW-BUILD-DONE rc=0' "$LOG" || abort "the JIT builds have not finished clean"
command -v clang > /dev/null || abort "no clang"
mkdir -p /root/mrw
cat > /root/mrw/aot.targets <<'X'
<Project>
  <!-- Seat #53 P1 kit: Native AOT for the program only. A global -p:PublishAot=true also reaches go2cs-gen, a
       netstandard analyzer, and the SDK refuses it (NETSDK1207). -->
  <PropertyGroup Condition="'$(GoAot)'=='true' AND '$(OutputType)'=='Exe'">
    <PublishAot>true</PublishAot>
    <PublishReadyToRun>false</PublishReadyToRun>
  </PropertyGroup>
</Project>
X
stamp "AOT start: $(clang --version | head -1)"
publish(){ # tree test arm
  local W=/root/mrw-$1 T=$2 arm=$3 P=/root/mrw-$1/src/tests/Behavioral/$2 s=$SECONDS
  rm -rf "$R/bin/$arm"
  ( cd "$P" && dotnet publish "$T.csproj" -c Release -r linux-x64 -nr:false -p:UseSharedCompilation=false -p:GoAot=true \
      -p:GoTargetOS=linux -p:CustomAfterMicrosoftCommonTargets=/root/mrw/aot.targets "-p:go2csPath=$W/src/" -o "$R/bin/$arm" -nologo -v:m ) > "$R/publish-$arm.txt" 2>&1
  local rc=$?
  stamp "PUBLISH $arm rc=$rc $((SECONDS-s))s errors=$(grep -cE ' error [A-Z]+[0-9]+' "$R/publish-$arm.txt")"
  [ $rc -eq 0 ] && [ -x "$R/bin/$arm/$T" ] || abort "$arm: no native exe"
  file "$R/bin/$arm/$T" | grep -q 'ELF 64-bit' || abort "$arm is not an ELF executable"
  [ -e "$R/bin/$arm/$T.dll" ] && abort "$arm carries a managed $T.dll: not a Native AOT publish"
  stamp "AOT $arm exe sha=$(sha256sum "$R/bin/$arm/$T" | cut -c1-16) size=$(stat -c %s "$R/bin/$arm/$T")"
}
smoke(){ # arm exe want_exit want_text: one idle run must exit as Go does, print the text, and carry no flavor warning
  local arm=$1 exe=$2 want=$3 text=$4 o e rc
  o=$(mktemp); e=$(mktemp)
  GOMAXPROCS=4 timeout 60 "$exe" > "$o" 2> "$e"; rc=$?
  if grep -q 'running the "windows" build' "$e" || [ "$rc" != "$want" ] || ! grep -qF "$text" "$o" "$e"; then
    stamp "SMOKE $arm FAILED rc=$rc (want $want) stderr: $(grep -v '^  ' "$e" | head -3 | tr '\n' '|')"; rm -f "$o" "$e"; return 1
  fi
  stamp "SMOKE $arm ok rc=$rc"; rm -f "$o" "$e"
}
publish base GoroutinePanicAfterDeferredDone aot-base-pad
smoke aot-base-pad "$R/bin/aot-base-pad/GoroutinePanicAfterDeferredDone" 2 'panic: panic after deferred Done' || abort "smoke"
publish tip  GoroutinePanicAfterDeferredDone aot-tip-pad
smoke aot-tip-pad "$R/bin/aot-tip-pad/GoroutinePanicAfterDeferredDone" 2 'panic: panic after deferred Done' || abort "smoke"
publish tip  GoroutineRecoveringWhenMainReturns aot-tip-rec
smoke aot-tip-rec "$R/bin/aot-tip-rec/GoroutineRecoveringWhenMainReturns" 0 '' || abort "smoke"
echo "MRW-AOT-DONE rc=0" >> "$LOG"
