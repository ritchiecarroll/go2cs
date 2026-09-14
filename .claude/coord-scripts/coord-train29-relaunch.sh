#!/bin/bash
# coord-train29-relaunch.sh <C1RT7_FIX_SHA> -- re-point C1RT7 at C1's csproj follow-up, regenerate, verify at the remote, rehearse from the dry-run.
set -u
S=/c/Projects/go2cs/.claude/coord-scripts; NEW=${1:?fix sha}
cd $S || exit 2
python - "$NEW" <<'PY'
import io,sys
p='coord-derive-train29.py'; s=io.open(p,encoding='utf-8',newline='').read()
old="'20c11d05a'"; assert s.count(old)==1, ("C1RT7 tuple count", s.count(old))
io.open(p,'w',encoding='utf-8',newline='').write(s.replace(old,"'"+sys.argv[1]+"'")); print("re-pointed C1RT7 ->", sys.argv[1])
PY
[ $? -eq 0 ] || exit 3
python coord-derive-train29.py >/dev/null || exit 3
for f in coord-train29-assemble.sh coord-train29-rehearse.sh coord-train29-land.sh coord-train29-land-launch.sh; do bash -n $f || exit 4; done
grep -c "$NEW" coord-train29-assemble.sh | sed 's/^/assemble carries fix sha x/'
W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c
git -C $W fetch -q origin claude/c1-runtime-inc7-w2a || exit 5
T=$(git -C $W rev-parse --short=9 origin/claude/c1-runtime-inc7-w2a); [ "$T" = "$NEW" ] || { echo "remote tip $T != $NEW"; exit 6; }
git -C $W merge-base --is-ancestor 20c11d05a $NEW || { echo "$NEW not on 20c11d05a"; exit 7; }
echo "delta vs 20c11d05a: $(git -C $W diff --stat 20c11d05a..$NEW | tail -1)"; git -C $W diff --name-only 20c11d05a..$NEW
echo "markers: $(git -C $W diff 20c11d05a..$NEW | grep -cE '^[+](<<<<<<<|=======|>>>>>>>)')"
D=/c/Projects/go2cs/.claude/worktrees/coord-t25-dryrun
git -C $D reset -q --hard 9c44a6d6a && git -C $D clean -fdq && echo "dry-run at $(git -C $D rev-parse --short=9 HEAD) dirty $(git -C $D status --porcelain | wc -l)"
cd $D && REHEARSAL=1 CONTROL_SHA=9c44a6d6a bash $S/coord-train29-rehearse.sh; r=$?; echo "REHEARSAL exit=$r"; exit $r
