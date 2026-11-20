#!/bin/bash
# run.sh <patched tree's src directory> : build the control against the census tree and run each arm with its
# own report directory. Expected: copy and threads report ALTERNATING; grow reports SECOND-ADDRESS only;
# gcmove and plain report neither; off (the variable unset) writes no file.
set -u
SRC=${1:?the patched tree's src directory}
D=$(cd "$(dirname "$0")" && pwd)
cd "$D" && dotnet build control.csproj -c Release -nologo -v q -p:GoTargetOS=linux -p:go2csPath="$SRC/" > build.log 2>&1 || { echo "build FAILED"; grep -E ' error ' build.log | sort -u | head -8; exit 1; }
exe=$(find "$D/bin" -name control -type f | head -1)
for arm in copy threads grow gcmove plain; do
  out=$D/out-$arm; rm -rf "${out:?}"
  GO2CS_MUTEX_COPY_CENSUS=$out $exe $arm; rc=$?
  echo "== $arm rc=$rc files=$(ls "$out" 2>/dev/null | wc -l)"
  grep -hE "^(SUMMARY|ALTERNATING|SECOND-ADDRESS)" "$out"/* 2>/dev/null
done
rm -rf "${D:?}/out-off"; $exe copy; echo "== off rc=$? files=$(ls "$D/out-off" 2>/dev/null | wc -l)"
echo CONTROL-DONE
