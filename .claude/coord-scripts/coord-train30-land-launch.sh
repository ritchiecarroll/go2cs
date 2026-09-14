#!/usr/bin/env bash
# coord-train30-land-launch.sh -- the landing launch line for train 26: SUBQ32, C1Q34 (two commits, 3c6f1616a), RINC2 (4.2; 2b re-points
# it if R's cut lands before assembly), GB (set GB_SHA=<tip> in the environment once G announces and it is seated; unset = not pruned).
# Run AFTER the assembly's CHAIN DONE stamp and every leg's verdict has been read from its own log. cwd must be the coordinator worktree.
set -u
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
[ "$(git status --porcelain | wc -l)" = "0" ] || { echo "worktree dirty -- not launching"; exit 1; }
echo "launching land at head=$(git rev-parse --short HEAD) (expect the train-25 assembled head or a later assembly commit on it)"
C2Q44_SHA="${C2Q44_SHA:-eed11b550}" C2Q44_BRANCH="${C2Q44_BRANCH:-claude/c2-q44-cut}" \
C2INC9_SHA="${C2INC9_SHA:-d185e28b8}" C2INC9_BRANCH="${C2INC9_BRANCH:-claude/c2-darwin-inc9}" \
C2Q56L_SHA="${C2Q56L_SHA:-0ac8a607c}" C2Q56L_BRANCH="${C2Q56L_BRANCH:-claude/c2-q56-lift}" \
C2INC10_SHA="${C2INC10_SHA:-4efd81cf5}" C2INC10_BRANCH="${C2INC10_BRANCH:-claude/c2-darwin-inc10}" \
SUBQ63_SHA="${SUBQ63_SHA:-66a73ab03}" SUBQ63_BRANCH="${SUBQ63_BRANCH:-claude/sub-q63}" \
SUBQ60_SHA="${SUBQ60_SHA:-16d1943ac}" SUBQ60_BRANCH="${SUBQ60_BRANCH:-claude/sub-q60}" \
SUBQ59_SHA="${SUBQ59_SHA:-1dd5bf492}" SUBQ59_BRANCH="${SUBQ59_BRANCH:-claude/sub-q59}" \
GBD_SHA="${GBD_SHA:-58e83c419}" GBD_BRANCH="${GBD_BRANCH:-claude/g-design-b-outparam}" \
GED_SHA="${GED_SHA:-b4337813a}" GED_BRANCH="${GED_BRANCH:-claude/g-design-e-elemaddr}" \
GCD_SHA="${GCD_SHA:-6db8d95a2}" GCD_BRANCH="${GCD_BRANCH:-claude/g-design-c-strwindow}" \
GFVCR_SHA="${GFVCR_SHA:-2f43ef7b3}" GFVCR_BRANCH="${GFVCR_BRANCH:-claude/g-fvc-record-measured}" \
RE2B_SHA="${RE2B_SHA:-ca74dd433}" RE2B_BRANCH="${RE2B_BRANCH:-claude/reflect-embedded-inc-e2b}" \
C1Q61_SHA="${C1Q61_SHA:-e33e14ccf}" C1Q61_BRANCH="${C1Q61_BRANCH:-claude/c1-runtime-q61-parkhook}" \
C1Q64_SHA="${C1Q64_SHA:-7ab3d6fa6}" C1Q64_BRANCH="${C1Q64_BRANCH:-claude/c1-runtime-q64-sigign}" \
C1Q58D_SHA="${C1Q58D_SHA:-44fba8cf6}" C1Q58D_BRANCH="${C1Q58D_BRANCH:-claude/c1-q58-design}" \
GDC_SHA="${GDC_SHA:-67eba534f}" GDC_BRANCH="${GDC_BRANCH:-claude/g-deferred-class}" \
bash /c/Projects/go2cs/.claude/coord-scripts/coord-train30-land.sh 2>&1 | tee -a /c/Projects/go2cs/.claude/coord-scripts/coord-train30-land-$(date +%Y%m%d-%H%M%S).log
echo "land exit=${PIPESTATUS[0]}"
