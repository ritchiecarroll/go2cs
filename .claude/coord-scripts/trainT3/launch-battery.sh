cd /h/go2cs-tmp-coord/coord-scratch/tT3/run2 || exit 1
# EXPECT_HEAD is OWED: the 10-char sha of 'fixup: TRAIN T3' that tT3-fixup.sh commits on train-t3-union (498de280bf at the
# derive is the 24-merge union BEFORE the fixup; the battery refuses a head without its fixup). DEADLINE = launch + 16 h,
# FL's rule (FL: launched 09:42 with DEADLINE 01:42 the next day; its run took 10 h 43 m), computed at launch.
EXPECT_HEAD=${EXPECT_HEAD:?set EXPECT_HEAD to the T3 fixup commit, 10 chars}
DEADLINE=$(date -d '+16 hours' '+%Y-%m-%d %H:%M')
MASTER=f43e0a2f4e4a3d6b5891ad9430cb7bffe811a5e9 EXPECT_HEAD=$EXPECT_HEAD SEATS_EXPECTED=24 DEADLINE="$DEADLINE" TP_PIN=04239ec85e bash ./tT3-battery.sh > battery.console.log 2>&1
rc=$?; echo "battery rc=$rc (DEADLINE was $DEADLINE)" >> battery.console.log; exit $rc
