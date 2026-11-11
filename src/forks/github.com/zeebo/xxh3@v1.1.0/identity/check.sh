#!/usr/bin/env bash
# Byte-identity check of the go2cs xxh3 fork against the original zeebo/xxh3 v1.1.0, both under real Go.
# Runs the same program (identity/original/main.go = identity/fork/main.go) against each and compares the outputs.
# Exit 0 only when they are byte-identical.
set -euo pipefail
here="$(cd "$(dirname "$0")" && pwd)"
cmp "$here/original/main.go" "$here/fork/main.go"
out="$(mktemp -d)"
(cd "$here/original" && go run . > "$out/original.out")
(cd "$here/fork" && go run . > "$out/fork.out")
echo "original: $(wc -l < "$out/original.out") lines, sha256 $(sha256sum < "$out/original.out" | cut -c1-16)"
echo "fork:     $(wc -l < "$out/fork.out") lines, sha256 $(sha256sum < "$out/fork.out" | cut -c1-16)"
cmp "$out/original.out" "$out/fork.out"
echo "IDENTICAL"
