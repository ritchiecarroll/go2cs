#!/bin/bash
# Seat #53 P1 kit, step 3: the NoInlining control root. A copy of the A root in which EVERY struct-static accessor is
# marked NoInlining. Convert and build the program against it with the A converter (convert-and-build.sh) for the JIT
# control and the Native AOT control alike. If the control reads like H, the A-arm gain is the inlining.
#
# usage: make-ni-root.sh <A-root> <NI-root>
#   <A-root>   W/A/root from seed-and-emit.sh (the folder holding src/)
#   <NI-root>  a directory that must not exist yet
set -euo pipefail
[ $# -eq 2 ] || { sed -n '6,8p' "$0"; exit 2; }
A=$1; NI=$2
[ -e "$NI" ] && { echo "refusing: $NI exists"; exit 2; }
mkdir -p "$NI"
# Source only: never another arm's bin/, obj/ or Generated/.
(cd "$A" && find . \( -name bin -o -name obj -o -name Generated \) -prune -o -type f -print0 | xargs -0 cp --parents -t "$NI")
# `static ref readonly T X => ref Xᵇ;` becomes `static ref readonly T X { [NoInlining] get => ref Xᵇ; }`, in every file
# that declares one. Byte mode, never a UTF-8 decode: the pattern is ASCII, and every other byte must stay as emitted.
grep -r -l -Z 'static ref readonly ' "$NI/src/core" --include='*.cs' | xargs -0 perl -pi -e 's/^(\s*(?:public|internal|private) static ref readonly \S+ \S+) => ref (\S+);/$1 { [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)] get => ref $2; }/'
marked=$(grep -r -h -c 'MethodImplOptions.NoInlining)] get => ref' "$NI/src/core" --include='*.cs' | awk '{s+=$1} END{print s+0}')
# grep exits 1 when it finds nothing, which is the outcome wanted here; under pipefail that must not end the script.
left=$({ grep -r -h -E 'static ref readonly \S+ \S+ => ref \S+;\s*$' "$NI/src/core" --include='*.cs' || true; } | wc -l)
echo "NoInlining accessors: $marked; left inlinable: $left (want 0)"
[ "$left" -eq 0 ] && [ "$marked" -gt 0 ] || exit 3
echo "NI root: $NI/src"
