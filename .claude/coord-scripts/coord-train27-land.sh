#!/bin/bash
# Train 25 landing: marker scan over the merge range, push HEAD:master fast-forward, verify, prune seated branches, reset the nistec control.
set -u
SP="C:/Projects/go2cs/.claude/coord-scripts"
export GOROOT='$HOME\sdk\go1.23.12'
export DOTNET_ROOT='$HOME\dotnet10'
export PATH="$HOME/sdk/go1.23.12/bin:$HOME/dotnet10:$PATH"
# GC0_BRANCH / GLINUX_BRANCH / C2DARWIN3_BRANCH: set at launch to the lanes' branch names once seated; unset = not pruned.
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
stamp(){ echo "[$(date '+%F %T')] $*"; }
head=$(git rev-parse --short HEAD); stamp "LAND25 START head=$head dirty=$(git status --porcelain | wc -l)"
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master; base=$(git rev-parse --short origin/master); stamp "origin/master=$base"

git merge-base --is-ancestor origin/master HEAD || { stamp "origin/master moved past this train -- ABORT"; exit 1; }
# conflict-marker scan over every blob the train touches
n=0; for f in $(git diff --name-only origin/master HEAD); do if git show "HEAD:$f" 2>/dev/null | grep -qaE '^(<<<<<<<|>>>>>>>) '; then stamp "MARKER in $f"; n=$((n+1)); fi; done; stamp "marker scan: $n files with markers over $(git diff --name-only origin/master HEAD | wc -l) changed"
[ "$n" = "0" ] || { stamp "markers present -- ABORT"; exit 1; }
# board structural invariant: one raw, one endraw (final), zero bare openers
b=docs/phase4/BOARD-next-validation-candidates.md; stamp "board: raw=$(grep -c '{% raw %}' $b) endraw=$(grep -c '{% endraw %}' $b) last-nonblank=$(grep -v '^\s*$' $b | tail -1 | cut -c1-30)"
# security census over the train's diff (nicknames only)
# security census over the train's diff (nicknames only): the RULED instrument is SUB-SEC's converter-suite guard (path-anchored + denied-token passes with clearances);
# the coarse three-arm grep stays as a second derivation but EXCLUDES the guard's own source, whose regex text and synthetic fixtures ARE the patterns by construction.
(cd src/go2cs && go test -count=1 -run 'FleetIdentifier|Clearance|Denied' ./... > "$SP/coord-land-census-guard-$(date +%Y%m%d-%H%M%S).log" 2>&1); gr=$?; stamp "identifier-census GUARD exit=$gr (must be 0)"
[ "$gr" = "0" ] || { stamp "identifier-census guard RED -- ABORT"; exit 1; }
sec=$(git diff origin/master HEAD -- . ':(exclude)src/go2cs/fleetIdentifierCensus_test.go' | grep -a '^+' | grep -aicE 'users\\[a-z]|/home/[a-z]|(^|[^\\a-z0-9])\\\\[a-z0-9-]{2,}\\[a-z0-9$_.-]+' ); stamp "coarse security census over the diff (guard file excluded): $sec hits (must be 0)"
[ "$sec" = "0" ] || { stamp "security census non-zero -- ABORT"; exit 1; }
git push origin HEAD:master 2>&1 | tail -2; rc=${PIPESTATUS[0]}; stamp "push exit=$rc"
[ "$rc" = "0" ] || { stamp "push FAILED -- nothing pruned"; exit 1; }
git fetch -q origin master; stamp "verify: origin/master=$(git rev-parse --short origin/master) HEAD=$(git rev-parse --short HEAD)"
[ "$(git rev-parse origin/master)" = "$(git rev-parse HEAD)" ] || { stamp "verify FAILED"; exit 1; }
for b in ${GB2_SHA:+${GB2_BRANCH:-claude/g-b2-widenings}} ${GQ48_SHA:+${GQ48_BRANCH:-claude/g-q48-trace-header}} ${C1RT3_SHA:+${C1RT3_BRANCH:-claude/c1-runtime-inc3-sliceheader}} ${C2Q44_SHA:+${C2Q44_BRANCH:-claude/c2-q44-cut}} ${C2Q49_SHA:+${C2Q49_BRANCH:-claude/c2-q49-cut}} ${RD_SHA:+${RD_BRANCH:-claude/reflect-cargo-inc-d}} ${SUBQ43_SHA:+${SUBQ43_BRANCH:-claude/sub-q43}} ${SUBDOC10_SHA:+${SUBDOC10_BRANCH:-claude/sub-doc10}} ${C2CEN25_SHA:+${C2CEN25_BRANCH:-claude/c2-darwin-board-t25}}; do
  if git merge-base --is-ancestor "origin/$b" HEAD 2>/dev/null; then git push -q origin --delete "$b" && stamp "pruned $b"; else stamp "NOT pruned $b (tip not in master)"; fi
done
# nistec control worktree -> the new master, its exe removed so the next pair rebuilds it there
( cd /c/Projects/go2cs/.claude/worktrees/coord-nistec-ctrl && git fetch -q origin master && git checkout -q --detach origin/master && rm -f src/go2cs/bin/go2cs.exe && stamp "control worktree at $(git rev-parse --short HEAD), exe removed" )
stamp "LAND DONE master=$(git rev-parse --short HEAD)"
