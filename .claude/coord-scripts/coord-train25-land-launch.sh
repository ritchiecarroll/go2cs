#!/usr/bin/env bash
# coord-train25-land-launch.sh -- the landing launch line for train 25: SUBQ32, C1Q34 (two commits, 3c6f1616a), RINC2 (4.2; 2b re-points
# it if R's cut lands before assembly), GB (set GB_SHA=<tip> in the environment once G announces and it is seated; unset = not pruned).
# Run AFTER the assembly's CHAIN DONE stamp and every leg's verdict has been read from its own log. cwd must be the coordinator worktree.
set -u
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
[ "$(git status --porcelain | wc -l)" = "0" ] || { echo "worktree dirty -- not launching"; exit 1; }
echo "launching land at head=$(git rev-parse --short HEAD) (expect the train-25 assembled head or a later assembly commit on it)"
SUBQ32_SHA=fccc2c59e SUBQ32_BRANCH=claude/sub-q32 \
C1Q34_SHA=3c6f1616a C1Q34_BRANCH=claude/c1-q34-census \
RINC2_SHA="${RINC2_SHA:-216cc5f5c}" RINC2_BRANCH=claude/reflect-cargo-inc-2 \
GB_SHA="${GB_SHA:-a238b1855}" GB_BRANCH=claude/g-b-defer-finally \
C2CENSUS_SHA="${C2CENSUS_SHA:-eaae0d998}" C2CENSUS_BRANCH="${C2CENSUS_BRANCH:-claude/c2-darwin-board-t24}" \
C1Q31_SHA="${C1Q31_SHA:-d6af08bf7}" C1Q31_BRANCH="${C1Q31_BRANCH:-claude/c1-q31-host-condition}" \
SUBQ36_SHA="${SUBQ36_SHA:-e4a286866}" SUBQ36_BRANCH=claude/sub-q36 \
SUBQ27_SHA="${SUBQ27_SHA:-d1e1300a4}" SUBQ27_BRANCH=claude/sub-q27 \
C1RT1_SHA="${C1RT1_SHA:-44b5089b2}" C1RT1_BRANCH=claude/c1-runtime-inc1-sigprocmask \
GQ35_SHA="${GQ35_SHA:-}" GQ35_BRANCH=claude/g-q35-i1-l3-footprint \
C2INC5_SHA="${C2INC5_SHA:-9074e18ce}" C2INC5_BRANCH=claude/c2-darwin-sigprocmask \
SUBQ39_SHA="${SUBQ39_SHA:-}" SUBQ39_BRANCH=claude/sub-q39 \
SUBQ29_SHA="${SUBQ29_SHA:-0a504b2f3}" SUBQ29_BRANCH=claude/sub-q29 \
C2CRASH_SHA="${C2CRASH_SHA:-}" C2CRASH_BRANCH=claude/c2-darwin-crashreport \
bash /c/Projects/go2cs/.claude/coord-scripts/coord-train25-land.sh 2>&1 | tee -a /c/Projects/go2cs/.claude/coord-scripts/coord-train25-land-$(date +%Y%m%d-%H%M%S).log
echo "land exit=${PIPESTATUS[0]}"
