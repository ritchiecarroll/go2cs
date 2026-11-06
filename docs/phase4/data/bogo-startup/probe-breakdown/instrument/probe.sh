#!/usr/bin/env bash
# Probe launches, direct: <host> -bogo-mode -is-handshaker-supported, tiering off as the pipeline runs it.
# usage: probe.sh <label> <host exe> <mode: none|hook|events|init|initevents> [N]   (1 warm-up + N; warm-up dropped)
set -u
LABEL=$1; HOST=$2; MODE=$3; N=${4:-12}
R=$GOMODCACHE/boringssl.googlesource.com/boringssl.git@v0.0.0-20250620172916-f51d8b099832/ssl/test/runner
OUT=$KIT/run-$LABEL; rm -rf $OUT; mkdir -p $OUT; : > $OUT/wall.tsv
HOOK=$KIT/hook/bin/Release/net10.0/P1Hook.dll
export DOTNET_TieredCompilation=0
cd $R
for i in $(seq 0 $N); do
  unset DOTNET_STARTUP_HOOKS P1HOOK_OUT P1HOOK_EVENTS P1INIT_OUT
  case $MODE in
    hook)   export DOTNET_STARTUP_HOOKS=$HOOK P1HOOK_OUT=$OUT/hook-$i.tsv P1HOOK_EVENTS=0 ;;
    events) export DOTNET_STARTUP_HOOKS=$HOOK P1HOOK_OUT=$OUT/hook-$i.tsv ;;
    init)   export P1INIT_OUT=$OUT/init-$i.tsv ;;
    initevents) export DOTNET_STARTUP_HOOKS=$HOOK P1HOOK_OUT=$OUT/hook-$i.tsv P1INIT_OUT=$OUT/init-$i.tsv ;;
  esac
  t0=$(date +%s%N)
  P1_T0=$t0 "$HOST" -bogo-mode -is-handshaker-supported > $OUT/out-$i.txt 2>&1
  rc=$?
  t1=$(date +%s%N)
  echo -e "$i\t$(( (t1-t0)/1000 ))\t$rc" >> $OUT/wall.tsv
done
