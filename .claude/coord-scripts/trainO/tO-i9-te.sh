#!/usr/bin/env bash
# TRAIN O leg TE-i9 (TRAIN L's critic 0 #5): the i9 shard's tracked-changes-U0.patch through the SAME TE reader the i7's
# TE leg uses (tO-helpers.py teattr). DERIVED 2026-10-03 from trainN/tN-i9-te.sh (from trainM/), names only. The reader
# CLASSIFIES: G's frame class (the NoInlining prefix, its using line, a position-map re-encode) was EXPECTED in the
# rewritten test sources until N's MS13 refresh, which is in O's base: at O a G-FRAME hunk is a source the refresh missed,
# and every OTHER hunk is compared with a baseline when one is given. Read-only: it reads patches, prints.
#   bash tO-i9-te.sh <the i9's tO-i9-logs/tracked-changes-U0.patch> [<the i9's TRAIN N shard patch, as the baseline>]
# The baseline is the i9's OWN TRAIN N patch (tN-i9-logs/tracked-changes-U0.patch from its N run folder; COORD's copy is
# coord-scratch/tN/i9-patches): the same box and the same 132 rows one train earlier. Without it the OTHER hunks are a
# count only.
# The battery runs it as leg TE-i9 when I9_PATCH names the delivered file; otherwise COORD runs it from the battery's
# run folder when the patch arrives. Exit = teattr's rc (0 = read; 2 = no readable patch). A READING, not a gate.
# The MS13 refresh takes the i9's tracked-changes.patch (with context), not this reading: tO-README.md MS13.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
P=${1:?usage: tO-i9-te.sh <tracked-changes-U0.patch> [<baseline patch>]}
B=${2:-}
[ -f "$P" ] || { echo "TE-i9 ABORT: no patch at $P"; exit 2; }
[ -f "$SD/tO-helpers.py" ] || { echo "TE-i9 ABORT: $SD/tO-helpers.py missing"; exit 2; }
echo "TE-i9 patch=$P lines=$(wc -l < "$P") sha256=$(sha256sum "$P" | cut -c1-16) files=$(grep -c '^diff --git ' "$P") baseline=${B:-none}"
if [ -n "$B" ]; then
  python -B "$(cygpath -w "$SD/tO-helpers.py")" teattr --baseline "$(cygpath -w "$B")" "$(cygpath -w "$P")"; rc=$?
else
  python -B "$(cygpath -w "$SD/tO-helpers.py")" teattr "$(cygpath -w "$P")"; rc=$?
fi
echo "TE-i9 rc=$rc"
exit $rc
