#!/bin/bash
# The census runs: std on five targets, and modernc.org/sqlite with its libc on two.
: "${TOOL:?the built avcensus} ${OUT:?an output directory}"
T="$TOOL"
O="$OUT"
cd "$(dirname "$TOOL")"
for t in linux/amd64 linux/arm64 windows/amd64 darwin/amd64 darwin/arm64; do
  os=${t%/*}; arch=${t#*/}
  GOOS=$os GOARCH=$arch $T std-$os-$arch std > $O/std-$os-$arch.tsv 2> $O/std-$os-$arch.summary.txt
  echo "std $t rc=$?"
done
cd "$1"
for t in linux/amd64 windows/amd64; do
  os=${t%/*}; arch=${t#*/}
  GOOS=$os GOARCH=$arch GOFLAGS=-mod=mod $T sq-$os-$arch modernc.org/sqlite/... modernc.org/libc > $O/sq-$os-$arch.tsv 2> $O/sq-$os-$arch.summary.txt
  echo "sq $t rc=$?"
done
echo done
