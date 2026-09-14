#!/usr/bin/env bash
# coord-train23-land-launch.sh -- the landing launch line for train 23, every seat's SHA and branch from the assembly's own
# defaults (SUBQ18 deliberately UNSET: it rides train 24, so claude/sub-q18 is not pruned). Run AFTER coord-cle-rebaseline.sh
# has committed the union golden and its gates read green. cwd must be the coordinator worktree; the land script cds there itself.
set -u
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
[ "$(git status --porcelain | wc -l)" = "0" ] || { echo "worktree dirty -- not launching"; exit 1; }
echo "launching land at head=$(git rev-parse --short HEAD) (expect the re-baseline commit on top of c04ded546)"
GREC8_SHA=011abc8b4 \
C2PIN_SHA=f349b3499a C2PIN_BRANCH=claude/c2-syscall-pin \
RINCC_SHA=268a6d4b2 RINCC_BRANCH=claude/reflect-cargo-inc-c \
GI3_SHA=6a7688c88 GI3_BRANCH=claude/g-i3-callsite-rule \
GSEMA_SHA=ad0ed9a2a GSEMA_BRANCH=claude/g-bprime-inline-gates \
SUBQ14_SHA=4b8e19ee6 SUBQ10_SHA=5f1490009 SUBDOC8_SHA=86e16e9f5 SUBQ11_SHA=bc5acdaf8 \
SUBQ1_SHA=54c7ecb85 SUBQ2_SHA=46c13d703 SUBQ9_SHA=dc7667683 C1ZR_SHA=5fdd7ebeb SUBQ20_SHA=5d8d69744 \
SUBSEC_SHA=60ba23404 SUBQ17_SHA=667bf9c71 SUBQ18_SHA= SUBSECG_SHA=fda427e4e SUBQ26_SHA=40619d109 \
bash /c/Projects/go2cs/.claude/coord-scripts/coord-train23-land.sh 2>&1 | tee -a /c/Projects/go2cs/.claude/coord-scripts/coord-train23-land-$(date +%Y%m%d-%H%M%S).log
echo "land exit=${PIPESTATUS[0]}"
