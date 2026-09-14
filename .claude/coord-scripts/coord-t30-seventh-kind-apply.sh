#!/usr/bin/env bash
# coord-t30-seventh-kind-apply.sh -- land the SEVENTH box kind's storage answer as an ASSEMBLY commit, if and only if the
# bounded window given to the lane has closed with no follow-up. One member, placed exactly where its sibling places it,
# with the sibling's own reason restated for this kind. No other change: no tidying, nothing touched in the repaired operators.
set -u; W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; S=/c/Projects/go2cs/.claude/coord-scripts
cd "$W" || exit 1
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "worktree dirty -- not landing"; exit 1; }
t=$(git ls-remote origin refs/heads/claude/c2-token-storage-repair 2>/dev/null | cut -c1-10)
[ "$t" = "d6e181fe1a" ] || { echo "the lane's branch has MOVED to $t -- merge its follow-up instead of landing this"; exit 1; }
F=src/core/golib/ж.HeaderSliceBox.cs
grep -q 'StorageKind' "$F" && { echo "the seventh kind already states an answer -- nothing to land"; exit 1; }
python - "$W/$F" <<'PY'
import io,sys
NL=chr(10); p=sys.argv[1]; s=io.open(p,encoding='utf-8',newline='').read()
anchor='    public override object? PinnableStorage => null;'+chr(13)+NL+chr(13)+NL+'    /// <summary>The slice is the header variable'
assert s.count(anchor)==1, ('anchor count', s.count(anchor))
CR=chr(13)
add=('    public override object? PinnableStorage => null;'+CR+NL+CR+NL+
'    /// <inheritdoc/>'+CR+NL+
'    // NEVER an address, for the same reason its sibling states in its own file: the value handed out'+CR+NL+
'    // through Value is MATERIALIZED on demand, so `fixed` would give the address of a temporary that the'+CR+NL+
'    // next materialization replaces, and the address route over a header box has a recorded native crash'+CR+NL+
'    // behind it. This kind must stay tokenised, and it says so here rather than inheriting the answer from'+CR+NL+
'    // a pinnability question -- which is the distinction the storage kind exists to draw.'+CR+NL+
'    public override PointerStorage StorageKind => PointerStorage.None;'+CR+NL+CR+NL+
'    /// <summary>The slice is the header variable')
io.open(p,'w',encoding='utf-8',newline='').write(s.replace(anchor,add))
print("member inserted")
PY
[ $? = 0 ] || { echo "insertion FAILED"; git checkout -- "$F"; exit 1; }
n=$(git status --porcelain | wc -l); [ "$n" = 1 ] || { echo "unexpected change set ($n) -- restoring"; git checkout -- .; exit 1; }
git diff --numstat | cut -c1-80
git add "$F" && git commit -S -q -F "$S/coord-commit-seventh-kind.txt" && echo "COMMITTED $(git rev-parse --short=9 HEAD)"
