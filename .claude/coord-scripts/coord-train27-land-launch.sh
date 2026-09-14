#!/usr/bin/env bash
# coord-train27-land-launch.sh -- the landing launch line for train 26: SUBQ32, C1Q34 (two commits, 3c6f1616a), RINC2 (4.2; 2b re-points
# it if R's cut lands before assembly), GB (set GB_SHA=<tip> in the environment once G announces and it is seated; unset = not pruned).
# Run AFTER the assembly's CHAIN DONE stamp and every leg's verdict has been read from its own log. cwd must be the coordinator worktree.
set -u
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
[ "$(git status --porcelain | wc -l)" = "0" ] || { echo "worktree dirty -- not launching"; exit 1; }
echo "launching land at head=$(git rev-parse --short HEAD) (expect the train-25 assembled head or a later assembly commit on it)"
GB2_SHA="${GB2_SHA:-39624e080}" GB2_BRANCH="${GB2_BRANCH:-claude/g-b2-widenings}" \
GQ48_SHA="${GQ48_SHA:-c5e552949}" GQ48_BRANCH="${GQ48_BRANCH:-claude/g-q48-trace-header}" \
C1RT3_SHA="${C1RT3_SHA:-c3279299b}" C1RT3_BRANCH="${C1RT3_BRANCH:-claude/c1-runtime-inc3-sliceheader}" \
C2Q44_SHA="${C2Q44_SHA:-}" C2Q44_BRANCH="${C2Q44_BRANCH:-claude/c2-q44-cut}" \
C2Q49_SHA="${C2Q49_SHA:-}" C2Q49_BRANCH="${C2Q49_BRANCH:-claude/c2-q49-cut}" \
RD_SHA="${RD_SHA:-a7b3e4a6a}" RD_BRANCH="${RD_BRANCH:-claude/reflect-cargo-inc-d}" \
SUBQ43_SHA="${SUBQ43_SHA:-5b58d49ea}" SUBQ43_BRANCH="${SUBQ43_BRANCH:-claude/sub-q43}" \
SUBDOC10_SHA="${SUBDOC10_SHA:-674982db9}" SUBDOC10_BRANCH="${SUBDOC10_BRANCH:-claude/sub-doc10}" \
SUBQ45_SHA="${SUBQ45_SHA:-49ccad282}" SUBQ45_BRANCH="${SUBQ45_BRANCH:-claude/sub-q45}" \
C2CEN25_SHA="${C2CEN25_SHA:-3752b7495}" C2CEN25_BRANCH="${C2CEN25_BRANCH:-claude/c2-darwin-board-t25}" \
bash /c/Projects/go2cs/.claude/coord-scripts/coord-train27-land.sh 2>&1 | tee -a /c/Projects/go2cs/.claude/coord-scripts/coord-train27-land-$(date +%Y%m%d-%H%M%S).log
echo "land exit=${PIPESTATUS[0]}"
