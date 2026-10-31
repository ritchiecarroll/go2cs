#!/bin/bash
# The main-return wait seat's R3 controls: two broken classifiers, patched into a CHECKOUT's golib (never committed),
# each of which must regress one arm. If an arm does not regress, it cannot see the defect it is meant to guard.
#
#   make-red.sh <checkout> blocked    every counted goroutine reads blocked: main is released after BlockedConfirm,
#                                     so R1 (the amplifier under load) must exit 0 again
#   make-red.sh <checkout> runnable   every counted goroutine reads runnable with no budget: main is held to the
#                                     hang guard, so R2 (GoroutineRecoveringWhenMainReturns) must print "recovered"
#   make-red.sh <checkout> restore    git checkout the file, and verify it is the committed blob again
set -euo pipefail
[ $# -eq 2 ] || { sed -n '2,10p' "$0"; exit 2; }
TREE=$1; ARM=$2
F=src/core/golib/runtime/MainExit.cs
cd "$TREE"

# Only ever on a committed file: a patch on uncommitted work, or a restore over it, would destroy that work.
if [ "$ARM" != restore ] && ! git diff --quiet -- "$F"; then
  echo "refusing: $F differs from HEAD in $TREE (commit it first, or use a clean checkout)"; exit 2
fi

case "$ARM" in
  blocked)
    # The decision's first statement sees every sample as the same block, at once.
    perl -0pi -e 's/(    internal static bool Holds\(ref PanickerWatch watch, PanickerSample sample, TimeSpan elapsed\)\r?\n    \{\r?\n)/$1        sample = sample with { Blocked = true, BlockSequence = 0 }; \/\/ RED: always blocked\n/' "$F"
    ;;
  runnable)
    # A condition the compiler cannot fold, so the rest of Holds is not unreachable code (CS0162).
    perl -0pi -e 's/(    internal static bool Holds\(ref PanickerWatch watch, PanickerSample sample, TimeSpan elapsed\)\r?\n    \{\r?\n)/$1        if (elapsed >= TimeSpan.Zero) return true; \/\/ RED: always runnable, no budget\n/' "$F"
    ;;
  restore)
    # Restores only a RED patch: anything else in the file is someone's work, and stays.
    if [ "$(git diff -- "$F" | grep -c '^[-+] ')" != 1 ] || ! git diff -- "$F" | grep -q '^+.*// RED: '; then
      echo "refusing: $F holds changes other than one RED line; restore it by hand"; exit 2
    fi
    git checkout -- "$F"
    git diff --quiet -- "$F" && echo "restored: $F matches HEAD"
    exit 0
    ;;
  *) echo "unknown arm: $ARM"; exit 2 ;;
esac

grep -c 'RED: ' "$F" | grep -qx 1 || { echo "the patch did not land in $F"; git checkout -- "$F"; exit 3; }
echo "patched $F: $ARM (rebuild golib and the behavioral project before running)"
