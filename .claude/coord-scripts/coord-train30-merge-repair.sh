#!/usr/bin/env bash
# coord-train30-merge-repair.sh <branch> <sha9> <message-file> -- merge a REPAIR branch (C2's setsockopt fix, or any other
# repair cut against the train-30 assembly head) into the assembly head. Gates: no live legs, clean tree, the branch is a
# descendant of the CURRENT head, markers and identifier census clean, and its commit count is printed rather than assumed.
set -u; W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; S=/c/Projects/go2cs/.claude/coord-scripts
B="${1:?usage: coord-train30-merge-repair.sh <branch> <sha9> <message-file>}"; SHA="${2:?}"; MSG="${3:?}"
cd "$W" || exit 1
live=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*musing-moser*' -and \$_.Name -match 'go2cs|dotnet|BehavioralRunner' }).Count" 2>/dev/null | tr -d '\r ')
[ "${live:-0}" = "0" ] || { echo "legs live in the train worktree ($live) -- not merging"; exit 1; }
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "worktree dirty -- not merging"; git status --porcelain | head -3; exit 1; }
[ -f "$S/$MSG" ] || { echo "message file $MSG not found"; exit 1; }
grep -qE '<[A-Z0-9_]{3,}>' "$S/$MSG" && { echo "message carries an unfilled placeholder -- not merging"; exit 1; }
H=$(git rev-parse --short=9 HEAD); echo "assembly head: $H"
git fetch -q origin "$B" || { echo "fetch of $B failed"; exit 1; }
# Compare FULL object ids, resolving the announced value as a commit-ish: an announced SHA may be any length
# (a 10-character announce against a 9-character rev-parse is a FALSE refusal, met 2026-09-06).
t=$(git rev-parse FETCH_HEAD); ann=$(git rev-parse "$SHA^{commit}" 2>/dev/null)
echo "$B -> $(echo $t | cut -c1-10) (announced $SHA)"
[ -n "$ann" ] || { echo "the announced SHA $SHA does not resolve to a commit here -- stop and ask the lane"; exit 1; }
[ "$t" = "$ann" ] || { echo "the remote tip $(echo $t | cut -c1-10) is not the announced $SHA -- stop and ask the lane"; exit 1; }
# A repair based on the commit it REPAIRS is an ordinary merge, not a fast-forward: assert that its base is
# already IN this assembly and that there is something to merge, rather than that it descends from the head
# (ruled 2026-09-06 -- the assembly is local, so a lane bases on the introducing seat).
mb=$(git merge-base HEAD "$t"); echo "merge base: $(echo $mb | cut -c1-10) (the repair is based there; that commit must already be in this assembly)"
git merge-base --is-ancestor "$mb" HEAD || { echo "the repair base is NOT in this assembly -- stop"; exit 1; }
! git merge-base --is-ancestor "$t" HEAD || { echo "the repair is already merged -- nothing to do"; exit 1; }
echo "commits on its base: $(git rev-list --count $mb..$t)"; git log --format='  %h %s' $mb..$t | cut -c1-140
git diff --stat $mb..$t | tail -3
mk=$(git diff $mb..$t | grep -cE '^\+(<<<<<<<|=======|>>>>>>>)'); cs=$(git diff $mb..$t | grep '^+' | grep -ciE 'users[\/][a-z0-9._-]+|/home/[a-z0-9._-]+')
echo "markers $mk census $cs"; [ "$mk" = 0 ] && [ "$cs" = 0 ] || { echo "markers or census hits -- not merging"; exit 1; }
git merge --no-ff -S -F "$S/$MSG" "$t" || { echo "MERGE FAILED -- resolve by hand"; git status --porcelain | head; exit 1; }
echo "MERGED -> $(git rev-parse --short=9 HEAD)"; git log -1 --format='%h %s' | cut -c1-120
