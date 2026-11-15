#!/bin/bash
# sqlite-closure.sh <moddir> <workdir> <outdir> -- the v2 census over modernc.org/sqlite v1.62.0's non-standard closure
# (the owner's OK to fetch it, 2026-11-15), linux/amd64 and windows/amd64, through this record's run.sh (closure mode).
# <moddir> is created with a one-import main module if it has no go.mod. Needs Go 1.26.11.
set -u
HERE="$(cd "$(dirname "$0")" && pwd)"
M="$1"; W="$2"; O="$3"
mkdir -p "$M" "$O"
cd "$M"
if [ ! -f go.mod ]; then
  printf 'module sqlitecensus\n\ngo 1.26.11\n' > go.mod
  printf 'package main\n\nimport _ "modernc.org/sqlite"\n\nfunc main() {}\n' > main.go
  GOFLAGS=-mod=mod go get modernc.org/sqlite@v1.62.0 || exit 1
  GOFLAGS=-mod=mod go mod tidy || exit 1
fi
grep -n 'modernc.org/sqlite \|modernc.org/libc ' go.mod
for t in linux/amd64 windows/amd64; do
  os=${t%/*}; arch=${t#*/}; lab="sqlite-$os-$arch"
  pk=$(GOOS=$os GOARCH=$arch CGO_ENABLED=0 go list -deps -f '{{if not .Standard}}{{.ImportPath}}{{end}}' . | grep -v '^sqlitecensus$')
  echo "$lab: $(echo "$pk" | wc -l) non-std packages"
  GOOS=$os GOARCH=$arch CGO_ENABLED=0 bash "$HERE/run.sh" "$W" "$O/$lab" "$M" $pk
  rc=$?
  echo "$lab rc=$rc"
  [ $rc -eq 0 ] || exit $rc
  mv "$O/$lab/closure.tsv" "$O/$lab.tsv" && rmdir "$O/$lab"
done
