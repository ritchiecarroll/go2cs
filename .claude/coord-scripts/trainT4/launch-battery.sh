cd /h/go2cs-tmp-coord/coord-scratch/tT4/run2/go || exit 1
# RUN2 (2026-10-13). EXPECT_HEAD is the 10-char sha of 'fixup: TRAIN T4' that tT4-fixup.sh committed on train-t4-union-r2
# in /h/go2cs-tmp-coord/tT4r2 (b258fe81db is the 11-merge run2 union BEFORE the fixup; the battery refuses a head without its
# fixup). The battery runs in run2/go, the folder the run2 map, assembly and fixup ran in (the same two lists: rows-sha256
# a2f727a60342, follow e3b0c44298fc). MASTER stays the base 226afb810a (origin/master has moved by docs/workflow merges
# only: the landing reads them through MASTER_MOVED_OK). Leg NGF-j0 is RETIRED in this copy (T4-14).
# DEADLINE = launch + 16 h, T3's rule. The probe worktree tT4-probes (5f363aeae4) is run1's, read as it stands.
EXPECT_HEAD=${EXPECT_HEAD:?set EXPECT_HEAD to the run2 T4 fixup commit, 10 chars}
DEADLINE=$(date -d '+16 hours' '+%Y-%m-%d %H:%M')
MASTER=226afb810a21408ebda2412dd74d526345c46f65 EXPECT_HEAD=$EXPECT_HEAD SEATS_EXPECTED=11 DEADLINE="$DEADLINE" TP_PIN=5f363aeae4 bash ./tT4-battery.sh > battery.console.log 2>&1
rc=$?; echo "battery rc=$rc (DEADLINE was $DEADLINE)" >> battery.console.log; exit $rc
