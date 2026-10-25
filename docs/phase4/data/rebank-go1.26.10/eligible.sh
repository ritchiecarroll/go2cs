#!/bin/bash
# Per-GOOS eligibility of the go1.26.10 population at the corpus tags, written beside this script:
# eligible-<os>.txt holds every population row whose test files (internal + external) are non-empty for that GOOS.
# Usage: GOROOT=<go1.26.10 root, spelled as `go env GOROOT` prints it> bash eligible.sh
# -e keeps one unresolvable package from aborting the listing (an all-zero shape, measured at the 1.24 hop), and a
# fabricated package is the control: it must list with an error on every OS, never vanish.
set -u
: "${GOROOT:?set GOROOT to the go1.26.10 root}"
export GOTOOLCHAIN=local CGO_ENABLED=0 PATH="$GOROOT/bin:$PATH"
D=$(cd "$(dirname "$0")" && pwd)
POP="$D/../population-go1.26.10.txt"
ROWS=$(grep -v '^#' "$POP" | awk 'NF')
for os in windows linux darwin; do
  LIST=$(GOOS=$os GOARCH=amd64 go list -e -tags purego,math_big_pure_go \
    -f '{{.ImportPath}}|{{len .TestGoFiles}}|{{len .XTestGoFiles}}|{{if .Error}}ERR{{end}}' \
    $ROWS example.invalid/fabricated 2>/dev/null)
  control=$(printf '%s\n' "$LIST" | grep -c '^example.invalid/fabricated|.*|ERR$')
  [ "$control" = 1 ] || { echo "$os: the fabricated control did not list with an error -- refusing"; exit 1; }
  printf '%s\n' "$LIST" | awk -F'|' '$1 != "example.invalid/fabricated" && ($2 + $3) > 0 {print $1}' | LC_ALL=C sort > "$D/eligible-$os.txt"
  echo "$os: eligible=$(wc -l < "$D/eligible-$os.txt") control=$control"
done
