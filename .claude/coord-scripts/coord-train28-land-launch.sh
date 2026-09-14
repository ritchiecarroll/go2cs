#!/usr/bin/env bash
# coord-train28-land-launch.sh -- the landing launch line for train 26: SUBQ32, C1Q34 (two commits, 3c6f1616a), RINC2 (4.2; 2b re-points
# it if R's cut lands before assembly), GB (set GB_SHA=<tip> in the environment once G announces and it is seated; unset = not pruned).
# Run AFTER the assembly's CHAIN DONE stamp and every leg's verdict has been read from its own log. cwd must be the coordinator worktree.
set -u
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
[ "$(git status --porcelain | wc -l)" = "0" ] || { echo "worktree dirty -- not launching"; exit 1; }
echo "launching land at head=$(git rev-parse --short HEAD) (expect the train-25 assembled head or a later assembly commit on it)"
C2Q44_SHA="${C2Q44_SHA:-}" C2Q44_BRANCH="${C2Q44_BRANCH:-claude/c2-q44-cut}" \
C2Q49_SHA="${C2Q49_SHA:-}" C2Q49_BRANCH="${C2Q49_BRANCH:-claude/c2-q49-cut}" \
C1RT4_SHA="${C1RT4_SHA:-49ad67e32}" C1RT4_BRANCH="${C1RT4_BRANCH:-claude/c1-runtime-inc4-getg}" \
C1RT5_SHA="${C1RT5_SHA:-7f318ab29}" C1RT5_BRANCH="${C1RT5_BRANCH:-claude/c1-runtime-inc5-q54}" \
SUBQ45C_SHA="${SUBQ45C_SHA:-a38e638fe}" SUBQ45C_BRANCH="${SUBQ45C_BRANCH:-claude/sub-q45}" \
SUBQ50_SHA="${SUBQ50_SHA:-e8fcb6703}" SUBQ50_BRANCH="${SUBQ50_BRANCH:-claude/sub-q50}" \
GFVC_SHA="${GFVC_SHA:-}" GFVC_BRANCH="${GFVC_BRANCH:-claude/g-field-view-cut}" \
RE2_SHA="${RE2_SHA:-f5df84f49}" RE2_BRANCH="${RE2_BRANCH:-claude/reflect-field-metadata-inc-e2}" \
C2Q41F_SHA="${C2Q41F_SHA:-eb2ffed3d}" C2Q41F_BRANCH="${C2Q41F_BRANCH:-claude/c2-q41-frames}" \
GFVD_SHA="${GFVD_SHA:-c4bc47917}" GFVD_BRANCH="${GFVD_BRANCH:-claude/g-field-view-design}" \
C2INC6_SHA="${C2INC6_SHA:-cc16ab170}" C2INC6_BRANCH="${C2INC6_BRANCH:-claude/c2-darwin-inc6}" \
RE3_SHA="${RE3_SHA:-10eecadb9}" RE3_BRANCH="${RE3_BRANCH:-claude/reflect-value-singles-inc-e3}" \
SUBQ22_SHA="${SUBQ22_SHA:-969cbaeae}" SUBQ22_BRANCH="${SUBQ22_BRANCH:-claude/sub-q22}" \
C1Q46_SHA="${C1Q46_SHA:-f99111123}" C1Q46_BRANCH="${C1Q46_BRANCH:-claude/c1-q46-hostfatal}" \
SUBQ55_SHA="${SUBQ55_SHA:-b0c9d1d8c}" SUBQ55_BRANCH="${SUBQ55_BRANCH:-claude/sub-q55}" \
SUBQ57_SHA="${SUBQ57_SHA:-}" SUBQ57_BRANCH="${SUBQ57_BRANCH:-claude/sub-q57}" \
bash /c/Projects/go2cs/.claude/coord-scripts/coord-train28-land.sh 2>&1 | tee -a /c/Projects/go2cs/.claude/coord-scripts/coord-train28-land-$(date +%Y%m%d-%H%M%S).log
echo "land exit=${PIPESTATUS[0]}"
