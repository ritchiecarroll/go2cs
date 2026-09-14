#!/usr/bin/env bash
# coord-train30-assembly-merge-rtlgv.sh -- merge the rtlGetVersion hand-own (cut on claude/coord-t30-rtlgetversion in the
# dry-run worktree, off the assembly head 75758cf06) into the train-30 assembly head as an ASSEMBLY commit. No seat owns it:
# it remedies a corpus-wide defect the union made loud, and it is not on any lane's branch. Gates first, merge second.
set -u; S=/c/Projects/go2cs/.claude/coord-scripts; W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c
D=/c/Projects/go2cs/.claude/worktrees/coord-t25-dryrun; B=claude/coord-t30-rtlgetversion
live=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.CommandLine -like '*musing-moser*' -and \$_.Name -match 'go2cs|dotnet|BehavioralRunner' }).Count" 2>/dev/null | tr -d '\r ')
[ "${live:-0}" = "0" ] || { echo "legs still live in the train worktree ($live) -- not merging"; exit 1; }
cd "$W" || exit 1
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "train worktree dirty -- not merging"; git status --porcelain | head -5; exit 1; }
[ "$(git rev-parse --short=9 HEAD)" = "75758cf06" ] || { echo "train head is $(git rev-parse --short=9 HEAD), expected the assembly head 75758cf06"; exit 1; }
F=$(git -C "$D" rev-parse --short=9 "$B" 2>/dev/null) || { echo "fix branch $B not found in the dry-run worktree"; exit 1; }
git -C "$D" merge-base --is-ancestor 75758cf06 "$F" || { echo "fix $F does not descend from the assembly head"; exit 1; }
n=$(git -C "$D" rev-list --count 75758cf06.."$F"); echo "fix branch $B at $F: $n commit(s) on the assembly head"
[ "$n" = "1" ] || { echo "expected exactly one commit, got $n -- read them before merging"; git -C "$D" log --oneline 75758cf06.."$F"; exit 1; }
[ "$(git -C "$D" status --porcelain | wc -l)" = 0 ] || { echo "the dry-run worktree is dirty -- the cut is not finished"; exit 1; }
git -C "$D" diff --stat 75758cf06.."$F" | tail -3
mk=$(git -C "$D" diff 75758cf06.."$F" | grep -cE '^\+(<<<<<<<|=======|>>>>>>>)'); cs=$(git -C "$D" diff 75758cf06.."$F" | grep '^+' | grep -ciE 'users[\/][a-z0-9._-]+|/home/[a-z0-9._-]+')
echo "markers $mk census $cs"; [ "$mk" = 0 ] && [ "$cs" = 0 ] || { echo "markers or census hits in the cut -- not merging"; exit 1; }
git fetch -q "$D" "$B" || { echo "fetch from the dry-run worktree failed"; exit 1; }
git merge --no-ff -S -F "$S/coord-merge-assembly-rtlgv.txt" FETCH_HEAD || { echo "MERGE FAILED (conflict) -- resolve by hand"; git status --porcelain | head; exit 1; }
echo "MERGED -> $(git rev-parse --short=9 HEAD)"; git log -1 --stat --format='%h %s' | head -12
