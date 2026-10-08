#!/usr/bin/env bash
# TRAIN FL leg TE-i9 (TRAIN L's critic 0 #5): the i9 shard's tracked-changes-U0.patch through the SAME TE reader the
# i7's TE leg uses (tFL-helpers.py teattr). DERIVED 2026-10-08 from trainQ/tQ-i9-te.sh (names and texts only). The
# reader CLASSIFIES. At FL the EXPECTED classes in rewritten test sources are the FACE LIFT's (tFL-helpers.py FL1: one
# class per face-lift step, from trainFL/facelift-tests-hunk-classes.md and C2's three posts): row 10 regenerated the
# -stdlib corpus and the behavioral goldens, NOT the committed test sources, so every -tests run of the union re-emits
# them in the new form until the landing refresh commits it. N-PARTIAL (Q's carrier class) and G-FRAME stay classes.
# Every OTHER hunk is compared with a baseline when one is given. Read-only: it reads patches, prints.
#   bash tFL-i9-te.sh <the i9's tracked-changes-U0.patch> [<the i9's TRAIN Q shard patch, as the baseline>]
# The baseline is the i9's OWN TRAIN Q patch (COORD's copy: coord-scratch/tQ/i9-patches/tQ-tracked-changes-U0.patch,
# 1211 lines): the same box and the same rows one train earlier. Without it the OTHER hunks are a count only.
# The battery runs it as leg TE-i9 when I9_PATCH names the delivered file; otherwise COORD runs it from the battery's
# run folder when the patch arrives. Exit = teattr's rc (0 = read; 2 = no readable patch). A READING, not a gate.
# The MS13 refresh takes the i9's tracked-changes.patch (with context), not this reading: tFL-README.md MS13.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
P=${1:?usage: tFL-i9-te.sh <tracked-changes-U0.patch> [<baseline patch>]}
B=${2:-}
[ -f "$P" ] || { echo "TE-i9 ABORT: no patch at $P"; exit 2; }
[ -f "$SD/tFL-helpers.py" ] || { echo "TE-i9 ABORT: $SD/tFL-helpers.py missing"; exit 2; }
echo "TE-i9 patch=$P lines=$(wc -l < "$P") sha256=$(sha256sum "$P" | cut -c1-16) files=$(grep -c '^diff --git ' "$P") baseline=${B:-none}"
if [ -n "$B" ]; then
  python -B "$(cygpath -w "$SD/tFL-helpers.py")" teattr --baseline "$(cygpath -w "$B")" "$(cygpath -w "$P")"; rc=$?
else
  python -B "$(cygpath -w "$SD/tFL-helpers.py")" teattr "$(cygpath -w "$P")"; rc=$?
fi
echo "TE-i9 rc=$rc"
exit $rc
