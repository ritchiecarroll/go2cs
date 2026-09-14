#!/bin/bash
# Train 17 landing: marker scan over the merge range, push HEAD:master fast-forward, verify, prune seated branches, reset the nistec control.
set -u
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
stamp(){ echo "[$(date '+%F %T')] $*"; }
head=$(git rev-parse --short HEAD); stamp "LAND17 START head=$head dirty=$(git status --porcelain | wc -l)"
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "worktree dirty -- ABORT"; exit 1; }
git fetch -q origin master; base=$(git rev-parse --short origin/master); stamp "origin/master=$base"
# Docs seat before the push: R's cargo record branch advanced (docs-only) after this train's assembly fetched it -- merge its tip now,
# asserting the delta is docs-only (no path outside docs/), so the landing carries the record's current form.
git fetch -q origin claude/reflect-tail-r-cargo-record
if ! git merge-base --is-ancestor origin/claude/reflect-tail-r-cargo-record HEAD; then
  nondocs=$(git diff --name-only HEAD...origin/claude/reflect-tail-r-cargo-record | grep -v '^docs/' | wc -l)
  [ "$nondocs" = "0" ] || { stamp "record branch carries NON-docs paths ($nondocs) -- ABORT, not a docs seat"; exit 1; }
  git merge --no-ff -S -q -m "Merge claude/reflect-tail-r-cargo-record at $(git rev-parse --short origin/claude/reflect-tail-r-cargo-record) (R, docs only) -- the descriptor-cargo record's later sections, landed as a docs seat after this train's assembly had fetched the branch at f732617a2: R2 answered (the array arm is test-only reachable), the canonType interning-key census (four kinds, four local choices, identity failing in both directions), gob measured (keys on reflect.Type identity, banked green WITH the collapse -- a canary against damage, not a detector), the tree model as the rule (a container references its element's canonical descriptor and interns on it; unknown differs from zero by construction), and section 8.4 closed (DeepEqual compares canonical Types, so it is fixed by the tree and unbreakable by descriptor internals).

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>" origin/claude/reflect-tail-r-cargo-record && stamp "docs seat merged: record at $(git rev-parse --short origin/claude/reflect-tail-r-cargo-record) -> $(git rev-parse --short HEAD)"
else stamp "record branch already in HEAD"; fi

git merge-base --is-ancestor origin/master HEAD || { stamp "origin/master moved past this train -- ABORT"; exit 1; }
# conflict-marker scan over every blob the train touches
n=0; for f in $(git diff --name-only origin/master HEAD); do if git show "HEAD:$f" 2>/dev/null | grep -qaE '^(<<<<<<<|>>>>>>>) '; then stamp "MARKER in $f"; n=$((n+1)); fi; done; stamp "marker scan: $n files with markers over $(git diff --name-only origin/master HEAD | wc -l) changed"
[ "$n" = "0" ] || { stamp "markers present -- ABORT"; exit 1; }
# board structural invariant: one raw, one endraw (final), zero bare openers
b=docs/phase4/BOARD-next-validation-candidates.md; stamp "board: raw=$(grep -c '{% raw %}' $b) endraw=$(grep -c '{% endraw %}' $b) last-nonblank=$(grep -v '^\s*$' $b | tail -1 | cut -c1-30)"
# security census over the train's diff (nicknames only)
sec=$(git diff origin/master HEAD | grep -a '^+' | grep -aicE 'users\\[a-z]|/home/[a-z]|\\\\[a-z0-9-]+\\' ); stamp "security census over the diff: $sec hits (must be 0)"
[ "$sec" = "0" ] || { stamp "security census non-zero -- ABORT"; exit 1; }
git push origin HEAD:master 2>&1 | tail -2; rc=${PIPESTATUS[0]}; stamp "push exit=$rc"
[ "$rc" = "0" ] || { stamp "push FAILED -- nothing pruned"; exit 1; }
git fetch -q origin master; stamp "verify: origin/master=$(git rev-parse --short origin/master) HEAD=$(git rev-parse --short HEAD)"
[ "$(git rev-parse origin/master)" = "$(git rev-parse HEAD)" ] || { stamp "verify FAILED"; exit 1; }
for b in claude/reflect-tail-r-vmn claude/reflect-tail-r-cargo-record claude/sub-bytes-identity-cast claude/sub-rawsyscall-keepalive claude/sub-pprof-builder-av claude/sub-convert-timeout-flag claude/c1-pprof-labels claude/c1-pc-registry-design claude/c1-pc-registry claude/c1-pc-readback; do
  if git merge-base --is-ancestor "origin/$b" HEAD 2>/dev/null; then git push -q origin --delete "$b" && stamp "pruned $b"; else stamp "NOT pruned $b (tip not in master)"; fi
done
# nistec control worktree -> the new master, its exe removed so the next pair rebuilds it there
( cd /c/Projects/go2cs/.claude/worktrees/coord-nistec-ctrl && git fetch -q origin master && git checkout -q --detach origin/master && rm -f src/go2cs/bin/go2cs.exe && stamp "control worktree at $(git rev-parse --short HEAD), exe removed" )
stamp "LAND DONE master=$(git rev-parse --short HEAD)"
