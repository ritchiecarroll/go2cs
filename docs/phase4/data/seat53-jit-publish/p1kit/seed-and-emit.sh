#!/bin/bash
# Seat #53 P1 kit, step 1: build the two converters, seed one root from the checkout's committed standard library, and
# emit -stdlib into an H root (the base converter) and an A root (the seat converter).
#
# usage: seed-and-emit.sh <checkout> <goroot> <work> <base-commit> <seat-commit>
#   <checkout>     a clean checkout of the repository; its committed src/core must equal <base-commit>'s
#   <goroot>       GOROOT of Go 1.26.10, spelled exactly as `go env GOROOT` prints it with that toolchain
#   <work>         a directory that must not exist yet; everything this kit makes goes under it
#   <base-commit>  the converter before seat #53 (1dacbdf4b6)
#   <seat-commit>  the converter with seat #53 (5bf0fcb694, or later)
set -euo pipefail
[ $# -eq 5 ] || { sed -n '5,10p' "$0"; exit 2; }
CHECKOUT=$1; GR=$2; W=$3; BASE=$4; SEAT=$5
[ -e "$W" ] && { echo "refusing: $W exists"; exit 2; }
native() { if command -v cygpath > /dev/null; then cygpath -w "$1"; else echo "$1"; fi; }
posix() { if command -v cygpath > /dev/null; then cygpath -u "$1"; else echo "$1"; fi; }
export GOROOT="$GR" GOTOOLCHAIN=local CGO_ENABLED=0
export PATH="$(posix "$GR")/bin:$PATH"
go version | grep -q 'go1.26.10' || { echo "refusing: $(go version) is not go1.26.10"; exit 2; }
git -C "$CHECKOUT" diff --quiet "$BASE" HEAD -- src/core || { echo "refusing: the checkout's src/core differs from $BASE's"; exit 2; }
mkdir -p "$W"

# The two converters, by the ladder's reproducible recipe.
for arm in H:$BASE A:$SEAT; do
  name=${arm%%:*}; commit=${arm#*:}
  mkdir -p "$W/$name/build"
  git -C "$CHECKOUT" archive "$commit" src/go2cs | tar -x -C "$W/$name/build"
  (cd "$W/$name/build/src/go2cs" && go build -trimpath -buildvcs=false -o "$(native "$W/$name/go2cs.exe")" .)
  echo "$name $commit converter sha256 $(sha256sum "$W/$name/go2cs.exe" | cut -c1-16)"
done

# The seed: the checkout's TRACKED files only, so bin/, obj/ and Generated/ never enter it.
mkdir -p "$W/seed"
(cd "$CHECKOUT" && git ls-files -z src/core src/gen src/go2cs src/version.props src/Directory.Build.props \
  src/Directory.Build.targets | xargs -0 cp --parents -t "$W/seed")
want=$(git -C "$CHECKOUT" ls-files 'src/core/*.cs' | wc -l)
got=$(find "$W/seed/src/core" -name '*.cs' | wc -l)
echo "seed: $got .cs files under src/core, want $want (git ls-files)"
[ "$got" -eq "$want" ] || { echo "refusing: the seed is not the committed src/core"; exit 3; }

# One emission per arm, each into its own copy of the seed. -stdlib writes into -go2cspath's core/; it takes no
# output positional (the second-positional rule is for a single-package conversion).
for name in H A; do
  cp -r "$W/seed/." "$W/$name/root/"
  start=$(date +%s)
  "$W/$name/go2cs.exe" -stdlib -comments -go2cspath "$(native "$W/$name/root/src")" \
    -platforms windows/amd64,linux/amd64,darwin/amd64 -platform-stage "$(native "$W/$name/stage")" \
    -convert-timeout 90m > "$W/emit-$name.log" 2>&1
  echo "$name emit rc=0 wall=$(( $(date +%s) - start ))s $(grep -a 'Failed:' "$W/emit-$name.log" | tr -s ' ' | tr '\n' '|')"
  rm -rf "$W/$name/stage"
done
echo "roots: $W/H/root/src (H) and $W/A/root/src (A)"
