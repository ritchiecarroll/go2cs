#!/bin/bash
# run.sh <GOROOT> <program directory>: the planted positive, then the standard library (production and tests) on
# four targets. Writes planted.out and std-<target>.out beside this script.
C=$(cd "$(dirname "$0")" && pwd)
export GOROOT=$1 PATH=$1/bin:$PATH GOTOOLCHAIN=local CGO_ENABLED=0 GOWORK=off GOFLAGS=-mod=mod
cd "$C" && go build -o "${TMPDIR:-/tmp}/w17census" . || exit 1
CENSUS_GOOS=linux CENSUS_GOARCH=amd64 "${TMPDIR:-/tmp}/w17census" "$2" . | sed "s#$2/##" > planted.out
for t in linux/amd64 linux/arm64 darwin/amd64 windows/amd64; do
  CENSUS_STD=1 CENSUS_TESTS=1 CENSUS_GOOS=${t%/*} CENSUS_GOARCH=${t#*/} "${TMPDIR:-/tmp}/w17census" "$1/src" std > "std-${t/\//-}.out"
done
