#!/usr/bin/env bash
# Append the train-30 by-address-class board entry INSIDE the raw guard (doctrine: one raw, one endraw, endraw FINAL),
# then assert the guard's structure and commit it on the assembly head as a docs-only assembly commit.
set -u; W=/c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c; S=/c/Projects/go2cs/.claude/coord-scripts
cd "$W" || exit 1
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "worktree dirty -- not appending"; git status --porcelain | head -3; exit 1; }
python - "$W" "$S" <<'PY'
import io,sys
W,S=sys.argv[1],sys.argv[2]; NL=chr(10)
b=W+"/docs/phase4/BOARD-next-validation-candidates.md"
s=io.open(b,encoding='utf-8',newline='').read()
entry=io.open(S+"/coord-t30-board-entry.md",encoding='utf-8',newline='').read()
lines=s.split(NL)
idx=[i for i,l in enumerate(lines) if '{% endraw %}' in l]
assert len(idx)==1, ("endraw count",len(idx))
i=idx[0]
assert not [l for l in lines[i+1:] if l.strip()], "content after endraw"
body=NL.join(lines[:i]).rstrip(NL)
out=body+NL+entry.rstrip(NL)+NL+NL+NL.join(lines[i:])
io.open(b,'w',encoding='utf-8',newline='').write(out)
chk=io.open(b,encoding='utf-8',newline='').read().split(NL)
assert sum('{% raw %}' in l for l in chk)==1 and sum('{% endraw %}' in l for l in chk)==1, "guard count"
j=[k for k,l in enumerate(chk) if '{% endraw %}' in l][0]
assert not [l for l in chk[j+1:] if l.strip()], "endraw not final"
print("appended; board lines", len(chk), "endraw at", j+1)
PY
[ $? = 0 ] || { echo "append FAILED -- restoring"; git checkout -- docs/phase4/BOARD-next-validation-candidates.md; exit 1; }
n=$(git status --porcelain | wc -l); [ "$n" = 1 ] || { echo "unexpected change set ($n) -- restoring"; git checkout -- docs/; exit 1; }
git diff --numstat | cut -c1-90
git add docs/phase4/BOARD-next-validation-candidates.md
git commit -S -q -F "$S/coord-commit-board-t30.txt" && echo "BOARD COMMITTED $(git rev-parse --short=9 HEAD)"
