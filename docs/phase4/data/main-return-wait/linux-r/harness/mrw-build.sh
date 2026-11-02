#!/usr/bin/env bash
# mrw-build.sh -- the main-return-wait linux leg's builds on R-WSL (COORD dispatch 2026-11-01 19:06Z; brief of record
# docs/phase4/data/main-return-wait/leg-briefs.md at bbf1725aa7). Two detached worktrees of R's clone: base c5a4637bba
# (R0) and tip bbf1725aa7 (R1). Per tree: the amplifier GoroutinePanicAfterDeferredDone (pad) and, at the tip only, the
# recovering test GoroutineRecoveringWhenMainReturns (rec), each as a Release JIT build (framework-dependent, the
# behavioral runner's own configuration) with the tree's go2csPath, plus the Go binary. Then GolibTests MainExit* and
# ThreadCpuTime* at the tip, all with -p:GoTargetOS=linux (the linux corpus flavor). AOT builds are a separate script (they need a native linker decision).
# Output: /root/h10-mrw/{build.log, bin/<arm>/..., golib-tests.log}. Nothing is committed or pushed.
set -u
BASE=c5a4637bba
TIP=bbf1725aa7
GOROOT=/root/sdk/go1.26.10
C=/root/go2cs
R=/root/h10-mrw
mkdir -p "$R/bin" || exit 2
LOG="$R/build.log"
stamp(){ echo "$(date -u '+%Y-%m-%d %H:%M:%SZ') $*" | tee -a "$LOG"; }
abort(){ stamp "ABORT: $*"; echo "MRW-BUILD-DONE rc=2" >> "$LOG"; exit 2; }
export GOROOT GOTOOLCHAIN=local CGO_ENABLED=0 GOFLAGS=
export PATH="$GOROOT/bin:/root/dotnet10:$PATH"
export DOTNET_ROOT=/root/dotnet10 DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 MSBUILDDISABLENODEREUSE=1
unset GoTargetOS GO2CS_MODULE_ROOT DOTNET_TieredCompilation
PAD=GoroutinePanicAfterDeferredDone; REC=GoroutineRecoveringWhenMainReturns

cd "$C" || abort "no clone"
[ "$(git rev-parse --show-toplevel)" = "$C" ] || abort "toplevel is not $C"
git fetch -q origin master || abort "fetch"
BASE=$(git rev-parse --verify "$BASE^{commit}") || abort "base"
TIP=$(git rev-parse --verify "$TIP^{commit}") || abort "tip"
[ "$(git rev-parse "$TIP^1")" = "$BASE" ] || abort "the tip's first parent is not the base"
for t in base tip; do
  W=/root/mrw-$t; ref=$BASE; [ $t = tip ] && ref=$TIP
  if [ -d "$W" ]; then git -C "$W" checkout -q --detach "$ref" || abort "checkout $t"; else git worktree add -q --detach "$W" "$ref" || abort "worktree $t"; fi
  [ "$(git -C "$W" rev-parse HEAD)" = "$ref" ] || abort "$t HEAD"
  [ -z "$(git -C "$W" status --porcelain)" ] || abort "$t worktree not clean"
done
stamp "PRE base=$BASE tip=$TIP $(go version | cut -d' ' -f3) dotnet=$(dotnet --version) runtime=$(dotnet --list-runtimes | awk '/NETCore.App/ {print $2}' | tr '\n' ' ')"
stamp "golib MainExit.cs differs base..tip: $(git diff --quiet "$BASE" "$TIP" -- src/core/golib/runtime/MainExit.cs && echo no || echo yes)"

smoke(){ # arm exe want_exit want_text: one idle run must exit as Go does, print the text, and carry no flavor warning
  local arm=$1 exe=$2 want=$3 text=$4 o e rc
  o=$(mktemp); e=$(mktemp)
  GOMAXPROCS=4 timeout 60 "$exe" > "$o" 2> "$e"; rc=$?
  if grep -q 'running the "windows" build' "$e" || [ "$rc" != "$want" ] || ! grep -qF "$text" "$o" "$e"; then
    stamp "SMOKE $arm FAILED rc=$rc (want $want) stderr: $(grep -v '^  ' "$e" | head -3 | tr '
' '|')"; rm -f "$o" "$e"; return 1
  fi
  stamp "SMOKE $arm ok rc=$rc"; rm -f "$o" "$e"
}
build(){ # tree test arm
  local W=/root/mrw-$1 T=$2 arm=$3 P=/root/mrw-$1/src/tests/Behavioral/$2 s=$SECONDS
  ( cd "$P" && dotnet build "$T.csproj" -c Release -nr:false -p:UseSharedCompilation=false -p:GoTargetOS=linux "-p:go2csPath=$W/src/" -nologo -v:m ) > "$R/build-$arm.txt" 2>&1
  local rc=$?
  local out="$P/bin/Release/net10.0"
  stamp "BUILD $arm rc=$rc $((SECONDS-s))s errors=$(grep -cE ' error [A-Z]+[0-9]+' "$R/build-$arm.txt")"
  [ $rc -eq 0 ] && [ -x "$out/$T" ] || abort "$arm: no apphost at $out/$T"
  rm -rf "$R/bin/$arm"; cp -r "$out" "$R/bin/$arm"
  ( cd "$P" && go build -o "$R/bin/go-$arm/$T" . ) > "$R/build-go-$arm.txt" 2>&1 || abort "go build $arm"
  stamp "COPY $arm jit=$(sha256sum "$R/bin/$arm/$T.dll" | cut -c1-16) golib=$(sha256sum "$R/bin/$arm/golib.dll" | cut -c1-16) go=$(sha256sum "$R/bin/go-$arm/$T" | cut -c1-16)"
}
build base $PAD jit-base-pad
build tip  $PAD jit-tip-pad
build tip  $REC jit-tip-rec
smoke jit-base-pad "$R/bin/jit-base-pad/$PAD" 2 'panic: panic after deferred Done' || abort "smoke"
smoke jit-tip-pad "$R/bin/jit-tip-pad/$PAD" 2 'panic: panic after deferred Done' || abort "smoke"
smoke jit-tip-rec "$R/bin/jit-tip-rec/$REC" 0 '' || abort "smoke"
smoke go-tip-pad "$R/bin/go-jit-tip-pad/$PAD" 2 'panic: panic after deferred Done' || abort "smoke"

# GolibTests at the tip: MainExit* and ThreadCpuTime*, Release.
s=$SECONDS
( cd /root/mrw-tip/src/tests/GolibTests && dotnet test -c Release -nr:false -p:UseSharedCompilation=false -p:GoTargetOS=linux --filter "FullyQualifiedName~MainExit|FullyQualifiedName~ThreadCpuTime" --logger "console;verbosity=normal" ) > "$R/golib-tests.log" 2>&1
rc=$?
stamp "GOLIBTESTS rc=$rc $((SECONDS-s))s; $(grep -E '^(Passed!|Failed!)|Total tests|Passed:|Failed:|Skipped:' "$R/golib-tests.log" | tr -s ' ' | tr '\n' ' ')"
echo "MRW-BUILD-DONE rc=0" >> "$LOG"
