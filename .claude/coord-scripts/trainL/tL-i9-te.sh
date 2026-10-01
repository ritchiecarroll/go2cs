#!/usr/bin/env bash
# TRAIN L leg TE-i9 (critic 0 #5): the i9 shard's tracked-changes-U0.patch through the SAME TE reader the i7's TE leg
# uses (tL-helpers.py teattr: the D6 and cross-package embed-hop signatures on ADDED lines; INFERRED patterns,
# positive- and negative-controlled, see controls/). DRAFT 2 2026-10-01. Read-only: it reads the patch and prints.
#   bash tL-i9-te.sh <path to the i9's tL-i9-logs/tracked-changes-U0.patch>
# The battery runs it as leg TE-i9 when I9_PATCH names the delivered file; otherwise COORD runs it from the battery's
# run folder when the patch arrives. Exit = teattr's rc (0 = no signature line; 1 = signature line(s): READ EACH, a
# match is a candidate, not a finding; 2 = no readable patch). Gated like TE: a nonzero rc is listed, never absorbed.
set -u
SD=$(cd "$(dirname "$0")" && pwd)
P=${1:?usage: tL-i9-te.sh <tracked-changes-U0.patch>}
[ -f "$P" ] || { echo "TE-i9 ABORT: no patch at $P"; exit 2; }
[ -f "$SD/tL-helpers.py" ] || { echo "TE-i9 ABORT: $SD/tL-helpers.py missing"; exit 2; }
echo "TE-i9 patch=$P lines=$(wc -l < "$P") sha256=$(sha256sum "$P" | cut -c1-16) files=$(grep -c '^diff --git ' "$P")"
python -B "$(cygpath -w "$SD/tL-helpers.py")" teattr "$(cygpath -w "$P")"; rc=$?
echo "TE-i9 rc=$rc"
exit $rc
