#!/usr/bin/env bash
# TRAIN Q leg TE-i9 (TRAIN L's critic 0 #5): the i9 shard's tracked-changes-U0.patch through the SAME TE reader the i7's
# TE leg uses (tQ-helpers.py teattr). DERIVED 2026-10-06 from trainP/tP-i9-te.sh (from trainO/, trainN/, trainM/),
# names and texts only. The reader CLASSIFIES. At Q the EXPECTED class in rewritten test sources is N-PARTIAL: the
# carrier (c2-noinline-partial) renders a no-inline method as a partial method and regenerated no committed test
# source, so every -tests run re-emits those methods (tQ-helpers.py, Q18; 738 lines in 173 committed test sources
# at TRAIN P's landed line, read with git). A G-FRAME hunk is a source the earlier refreshes missed; every OTHER hunk is
# compared with a baseline when one is given. Read-only: it reads patches, prints.
#   bash tQ-i9-te.sh <the i9's tQ-i9-logs/tracked-changes-U0.patch> [<the i9's TRAIN P shard patch, as the baseline>]
# The baseline is the i9's OWN TRAIN P patch at P's fixup-2 (COORD's copy: coord-scratch/tP/i9-patches-run2/
# tP2-tracked-changes-U0.patch, 997 lines): the same box and the same 132 rows one train earlier. Without it the OTHER
# hunks are a count only.
# The battery runs it as leg TE-i9 when I9_PATCH names the delivered file; otherwise COORD runs it from the battery's
# run folder when the patch arrives. Exit = teattr's rc (0 = read; 2 = no readable patch). A READING, not a gate.
# The MS13 refresh takes the i9's tracked-changes.patch (with context), not this reading: tQ-README.md MS13.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
P=${1:?usage: tQ-i9-te.sh <tracked-changes-U0.patch> [<baseline patch>]}
B=${2:-}
[ -f "$P" ] || { echo "TE-i9 ABORT: no patch at $P"; exit 2; }
[ -f "$SD/tQ-helpers.py" ] || { echo "TE-i9 ABORT: $SD/tQ-helpers.py missing"; exit 2; }
echo "TE-i9 patch=$P lines=$(wc -l < "$P") sha256=$(sha256sum "$P" | cut -c1-16) files=$(grep -c '^diff --git ' "$P") baseline=${B:-none}"
if [ -n "$B" ]; then
  python -B "$(cygpath -w "$SD/tQ-helpers.py")" teattr --baseline "$(cygpath -w "$B")" "$(cygpath -w "$P")"; rc=$?
else
  python -B "$(cygpath -w "$SD/tQ-helpers.py")" teattr "$(cygpath -w "$P")"; rc=$?
fi
echo "TE-i9 rc=$rc"
exit $rc
