#!/usr/bin/env bash
# Times one shim process from launch to exit and logs it; SHIM_TIER=off|on|inherit sets the CHILD's tiering only.
t0=$(date +%s%N)
case "${SHIM_TIER:-inherit}" in
  off) export DOTNET_TieredCompilation=0 ;;
  on)  unset DOTNET_TieredCompilation ;;
esac
"${SHIM_REAL:?}" "$@"
rc=$?
t1=$(date +%s%N)
echo "$t0 $t1 $(( (t1 - t0) / 1000000 )) $rc tier=${DOTNET_TieredCompilation:-default} $*" >> "${SHIM_LOG:?}"
exit $rc
