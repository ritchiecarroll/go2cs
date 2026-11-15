#!/bin/bash
# run.sh <workdir> <outdir> [<module dir> <pattern...>] -- build reassigncensus from this record and run it.
#   With no module: the standard library on windows/amd64, linux/amd64, darwin/amd64 and linux/arm64, production
#   (std-<target>.tsv) and with tests (stdtests-<target>.tsv), under the corpus's tags (purego, math_big_pure_go).
#   With a module dir and patterns (a demo closure, e.g. `. ./...` or `. all`): that load, for the host's GOOS and
#   GOARCH unless set, into closure.tsv.
# Needs Go 1.26.11 and golang.org/x/tools v0.52.0 in the module cache (src/go2cs's own pin).
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
W="$1"; O="$2"; shift 2
mkdir -p "$W" "$O"
cp "$HERE/reassigncensus-main.go" "$W/main.go"
cp "$HERE/reassigncensus-go.mod" "$W/go.mod"
cp "$HERE/reassigncensus-go.sum" "$W/go.sum"
(cd "$W" && GOFLAGS=-mod=mod go build -o reassigncensus .)
T="$W/reassigncensus"
[ -x "$T" ] || T="$W/reassigncensus.exe"
if [ $# -ge 2 ]; then
  M="$1"; shift
  "$T" -dir "$M" "${GOOS:-host}-${GOARCH:-host}" "$@" > "$O/closure.tsv"
  exit 0
fi
for t in windows/amd64 linux/amd64 darwin/amd64 linux/arm64; do
  os=${t%/*}; arch=${t#*/}; lab="$os-$arch"
  GOOS=$os GOARCH=$arch CGO_ENABLED=0 GOFLAGS=-tags=purego,math_big_pure_go "$T" "$lab" std > "$O/std-$lab.tsv"
  GOOS=$os GOARCH=$arch CGO_ENABLED=0 GOFLAGS=-tags=purego,math_big_pure_go "$T" -tests "$lab" std > "$O/stdtests-$lab.tsv"
done
