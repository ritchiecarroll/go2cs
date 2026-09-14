#!/usr/bin/env bash
# coord-train24-land-launch.sh -- the landing launch line for train 24: the nine seated tips (RINC2 deliberately UNSET, it rides train 25).
# Run AFTER the assembly's CHAIN DONE stamp and every leg's verdict has been read from its own log. cwd must be the coordinator worktree.
set -u
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
[ "$(git status --porcelain | wc -l)" = "0" ] || { echo "worktree dirty -- not launching"; exit 1; }
echo "launching land at head=$(git rev-parse --short HEAD) (expect 8f82b3f63 or a later assembly commit on it)"
GREC9_SHA=7a5725f79 GREC9_BRANCH=claude/g-record-i1-retired \
C2SIG_SHA=3137e4e80e C2SIG_BRANCH=claude/c2-darwin-signote \
SUBQ18_SHA=85301839f SUBQ18_BRANCH=claude/sub-q18 \
SUBQ23_SHA=c7de2e643 SUBQ23_BRANCH=claude/sub-q23 \
C1Q12_SHA=960e518f9 C1Q12_BRANCH=claude/c1-q12-main-identity \
C1Q15_SHA=eaa284ad5 C1Q15_BRANCH=claude/c1-q15-syscall-tty \
RTRACE_SHA=ae8e50459 RTRACE_BRANCH=claude/r-trace-recon \
GI1_SHA=de376e7a6 GI1_BRANCH=claude/g-i1-samepkg-primary \
RINC2_SHA= \
SUBDOC9_SHA=8508b0e3b SUBDOC9_BRANCH=claude/sub-doc9 \
bash /c/Projects/go2cs/.claude/coord-scripts/coord-train24-land.sh 2>&1 | tee -a /c/Projects/go2cs/.claude/coord-scripts/coord-train24-land-$(date +%Y%m%d-%H%M%S).log
echo "land exit=${PIPESTATUS[0]}"
