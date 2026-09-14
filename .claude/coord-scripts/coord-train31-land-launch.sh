#!/usr/bin/env bash
# coord-train31-land-launch.sh -- the landing launch line for train 26: SUBQ32, C1Q34 (two commits, 3c6f1616a), RINC2 (4.2; 2b re-points
# it if R's cut lands before assembly), GB (set GB_SHA=<tip> in the environment once G announces and it is seated; unset = not pruned).
# Run AFTER the assembly's CHAIN DONE stamp and every leg's verdict has been read from its own log. cwd must be the coordinator worktree.
set -u
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
[ "$(git status --porcelain | wc -l)" = "0" ] || { echo "worktree dirty -- not launching"; exit 1; }
echo "launching land at head=$(git rev-parse --short HEAD) (expect the train-25 assembled head or a later assembly commit on it)"
C1ERB_SHA="${C1ERB_SHA:-810b03087}" C1ERB_BRANCH="${C1ERB_BRANCH:-claude/c1-elemrefbox-native-slice}" \
C2INC10B_SHA="${C2INC10B_SHA:-51884af75}" C2INC10B_BRANCH="${C2INC10B_BRANCH:-claude/c2-darwin-inc10}" \
C1REAP_SHA="${C1REAP_SHA:-3af4c88ec}" C1REAP_BRANCH="${C1REAP_BRANCH:-claude/c1-exec-foreground-reap}" \
C1RT8_SHA="${C1RT8_SHA:-b7a58eda0}" C1RT8_BRANCH="${C1RT8_BRANCH:-claude/c1-runtime-inc8}" \
C1Q58A_SHA="${C1Q58A_SHA:-a3ee3945c}" C1Q58A_BRANCH="${C1Q58A_BRANCH:-claude/c1-q58-record-amended}" \
C2Q44A_SHA="${C2Q44A_SHA:-66a6bdb96}" C2Q44A_BRANCH="${C2Q44A_BRANCH:-claude/c2-q44-record-amend}" \
RE2C_SHA="${RE2C_SHA:-3226509d7}" RE2C_BRANCH="${RE2C_BRANCH:-claude/reflect-embedded-inc-e2b}" \
GUDP_SHA="${GUDP_SHA:-52c01fbb9}" GUDP_BRANCH="${GUDP_BRANCH:-claude/g-wsasendto-seat}" \
SUBDOC12_SHA="${SUBDOC12_SHA:-6779206fc}" SUBDOC12_BRANCH="${SUBDOC12_BRANCH:-claude/sub-doc12}" \
C1PPD_SHA="${C1PPD_SHA:-f6124065f}" C1PPD_BRANCH="${C1PPD_BRANCH:-claude/c1-pprof-push-design}" \
C1PPP_SHA="${C1PPP_SHA:-99c408704}" C1PPP_BRANCH="${C1PPP_BRANCH:-claude/c1-pprof-push}" \
C1SELF_SHA="${C1SELF_SHA:-cf2b9015e}" C1SELF_BRANCH="${C1SELF_BRANCH:-claude/c1-pprof-selfsymbol}" \
RUNIQ_SHA="${RUNIQ_SHA:-1bb544a18}" RUNIQ_BRANCH="${RUNIQ_BRANCH:-claude/laneR-unique-liveness}" \
RDENOM_SHA="${RDENOM_SHA:-cb04ece1c}" RDENOM_BRANCH="${RDENOM_BRANCH:-claude/laneR-roster-denominators}" \
bash /c/Projects/go2cs/.claude/coord-scripts/coord-train31-land.sh 2>&1 | tee -a /c/Projects/go2cs/.claude/coord-scripts/coord-train31-land-$(date +%Y%m%d-%H%M%S).log
echo "land exit=${PIPESTATUS[0]}"
