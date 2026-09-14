#!/usr/bin/env bash
# coord-train29-land-launch.sh -- the landing launch line for train 26: SUBQ32, C1Q34 (two commits, 3c6f1616a), RINC2 (4.2; 2b re-points
# it if R's cut lands before assembly), GB (set GB_SHA=<tip> in the environment once G announces and it is seated; unset = not pruned).
# Run AFTER the assembly's CHAIN DONE stamp and every leg's verdict has been read from its own log. cwd must be the coordinator worktree.
set -u
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
[ "$(git status --porcelain | wc -l)" = "0" ] || { echo "worktree dirty -- not launching"; exit 1; }
echo "launching land at head=$(git rev-parse --short HEAD) (expect the train-25 assembled head or a later assembly commit on it)"
C2Q44_SHA="${C2Q44_SHA:-}" C2Q44_BRANCH="${C2Q44_BRANCH:-claude/c2-q44-cut}" \
C2Q49_SHA="${C2Q49_SHA:-d5645ab97}" C2Q49_BRANCH="${C2Q49_BRANCH:-claude/c2-q49-cut}" \
GFVC_SHA="${GFVC_SHA:-a5d40fdfc}" GFVC_BRANCH="${GFVC_BRANCH:-claude/g-field-view-cut}" \
GA_SHA="${GA_SHA:-955e271c0}" GA_BRANCH="${GA_BRANCH:-claude/g-elem-take-concrete}" \
SUBQ57_SHA="${SUBQ57_SHA:-588a01aaa}" SUBQ57_BRANCH="${SUBQ57_BRANCH:-claude/sub-q57}" \
C1RT6_SHA="${C1RT6_SHA:-d8cecc3ce}" C1RT6_BRANCH="${C1RT6_BRANCH:-claude/c1-runtime-inc6-mem}" \
C1RT7_SHA="${C1RT7_SHA:-846c36e1e}" C1RT7_BRANCH="${C1RT7_BRANCH:-claude/c1-runtime-inc7-w2a}" \
RE3B_SHA="${RE3B_SHA:-6a7ea30be}" RE3B_BRANCH="${RE3B_BRANCH:-claude/reflect-value-singles-inc-e3}" \
C2Q56D_SHA="${C2Q56D_SHA:-8c1d2d506}" C2Q56D_BRANCH="${C2Q56D_BRANCH:-claude/c2-q56-design}" \
C2Q52D_SHA="${C2Q52D_SHA:-15968370d}" C2Q52D_BRANCH="${C2Q52D_BRANCH:-claude/c2-q52-design}" \
C2INC7_SHA="${C2INC7_SHA:-48291283b}" C2INC7_BRANCH="${C2INC7_BRANCH:-claude/c2-darwin-inc7}" \
C2INC8_SHA="${C2INC8_SHA:-261f3e1e4}" C2INC8_BRANCH="${C2INC8_BRANCH:-claude/c2-darwin-inc8}" \
C2Q52B_SHA="${C2Q52B_SHA:-554620235}" C2Q52B_BRANCH="${C2Q52B_BRANCH:-claude/c2-q52-bridge}" \
SUBQ62_SHA="${SUBQ62_SHA:-d97193e1f}" SUBQ62_BRANCH="${SUBQ62_BRANCH:-claude/sub-q62}" \
SUBDOC11_SHA="${SUBDOC11_SHA:-5b407a952}" SUBDOC11_BRANCH="${SUBDOC11_BRANCH:-claude/sub-doc11}" \
bash /c/Projects/go2cs/.claude/coord-scripts/coord-train29-land.sh 2>&1 | tee -a /c/Projects/go2cs/.claude/coord-scripts/coord-train29-land-$(date +%Y%m%d-%H%M%S).log
echo "land exit=${PIPESTATUS[0]}"
