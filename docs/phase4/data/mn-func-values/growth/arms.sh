#!/usr/bin/env bash
# arms.sh : the re-review's twin-growth and residual-hole arms on net/http, one after another. Mints no verdict.
# Usage: arms.sh [prefix] [pattern]   (defaults: net/http net/http). Writes <tag>-<arm>.txt / .tsv beside it.
set -u
D=/c/go2cs/scratch/mn2/proto/b/n3census
O=/c/go2cs/scratch/mn2/proto/b/n3code
PREFIX=${1:-net/http}
PATTERN=${2:-net/http}
TAG=$(echo "$PREFIX" | tr '/' '-')
export GOROOT=C:/Users/<user>/sdk/go1.26.10
export PATH="/c/Users/<user>/sdk/go1.26.10/bin:$PATH"
REACH="-no-func-values -long-only -exclude-runtime"

run() {
    local arm=$1
    shift
    "$D/census2.exe" $REACH "$@" -creation "$PREFIX" -creation-out "$O/$TAG-$arm.tsv" $PATTERN > "$O/$TAG-$arm.txt" 2>&1
    echo "$arm rc=$?"
}

run base
run fix1 -seed-factories
run leaf -seed-func-calls -seed-leaf-only
run leaf-fix1 -seed-func-calls -seed-leaf-only -seed-factories
run prop -seed-func-calls
run prop-fix1 -seed-func-calls -seed-factories
echo "arms done"
run fix1a -seed-factories -factories-arm
run leaf-fix1a -seed-func-calls -seed-leaf-only -seed-factories -factories-arm
run prop-fix1a -seed-func-calls -seed-factories -factories-arm
echo "arms done (with fix1a)"
