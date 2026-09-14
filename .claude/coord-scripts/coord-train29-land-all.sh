#!/bin/bash
# coord-train29-land-all.sh -- run AFTER the tail legs' 'TRAIN 29 RELEGS DONE' stamp and GO to SUB-Q60: gates -> TC0 runner -> land-launch -> bookkeeping -> entry fill.
# Posting the landing entry stays MANUAL (read coord-entry-train29-landed.md first).
set -u
S=/c/Projects/go2cs/.claude/coord-scripts
SC=$HOME/AppData/Local/Temp/claude/C--Projects-go2cs--claude-worktrees-go2cs-fleet-train-19-7d1cc7/388822cf-bade-49e8-96a9-7ab378101ad4/scratchpad
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 2
[ "$(git status --porcelain | wc -l)" = 0 ] || { echo "worktree dirty -- not landing"; exit 1; }
ML=$(ls -t $S/coord-train29-merge-*.log | head -1); echo "relegs merge log: $ML"
grep -aq 'TRAIN 29 RELEGS DONE' "$ML" || { echo "tail legs not done -- not landing"; exit 1; }
bad=$(grep -aE '\] LEG ' "$ML" | grep -av 'LEG reflectrun' | grep -acE 'UNMEASURED|exit=[1-9]|FAIL'); echo "tail-leg reds (excluding the reflect run, read by its set diff): $bad"
RR=$(ls -t $S/coord-train29-reflectrun-*.log | head -1); grep -aq 'BROKEN (agreed before, diverges now): \[\]' "$RR" || { echo "reflect run BROKEN set not empty or not read -- not landing"; grep -aE 'FIXED|BROKEN' "$RR" | tail -2; exit 1; }; echo "reflect run: $(grep -aE 'FIXED|BROKEN' "$RR" | tail -2 | tr '
' ' ' | cut -c1-200)"
[ "$bad" = 0 ] || { echo "a tail leg is RED or UNMEASURED -- read it before landing"; grep -aE '\] LEG ' "$ML" | grep -aE 'UNMEASURED|exit=[1-9]|FAIL' | cut -c1-200; exit 1; }
bash $S/coord-bank-legs.sh "$ML" $S/coord-t29-battery-lines-chain5.txt | cut -c1-100
echo "chain-5 battery lines: $(wc -l < $S/coord-t29-battery-lines-chain5.txt)"
echo "=== TC0 runner"; bash $S/coord-t29-golibtc0-run.sh > $SC/coord-t29-tc0.log 2>&1; t=$?; tail -2 $SC/coord-t29-tc0.log | cut -c1-200
[ "$t" = 0 ] || { echo "TC0 RED exit=$t -- not landing"; exit 1; }
grep -aq 'Test Run Aborted' $SC/coord-t29-tc0.log && { echo "TC0 ABORTED -- unmeasured, not landing"; exit 1; }
echo "=== land-launch"; bash $S/coord-train29-land-launch.sh > $SC/coord-t29-land-launch.log 2>&1
grep -aE 'LAND25 START|coarse security|ABORT|push exit|verify|pruned|control worktree|LAND DONE|land exit' $SC/coord-t29-land-launch.log | cut -c1-160
sha=$(grep -aoE 'LAND DONE master=[0-9a-f]+' $SC/coord-t29-land-launch.log | tail -1 | grep -oE '[0-9a-f]+$')
[ -n "$sha" ] || { echo "no LAND DONE stamp -- read the land log"; exit 1; }
sha9=$(git rev-parse --short=9 "$sha"); echo "LANDED master=$sha9"
echo "=== bookkeeping"; python $S/coord-train29-bookkeeping.py "$sha9" | tail -3
echo "=== entry fill"; python - "$sha9" "$S" <<'PY'
import io,sys
sha,S=sys.argv[1],sys.argv[2]; p=S+'/coord-entry-train29-landed.md'; s=io.open(p,encoding='utf-8',newline='').read()
lines=io.open(S+'/coord-t29-battery-lines-chain5.txt',encoding='utf-8',newline='').read().strip().split(chr(10))
tc0=[l for l in io.open('$HOME/AppData/Local/Temp/claude/C--Projects-go2cs--claude-worktrees-go2cs-fleet-train-19-7d1cc7/388822cf-bade-49e8-96a9-7ab378101ad4/scratchpad/coord-t29-tc0.log',encoding='utf-8',errors='replace').read().split(chr(10)) if 'TC0 exit=' in l]
body=chr(10).join('- '+l for l in lines)+(chr(10)+'- LEG golibtests-tc0 :: '+tc0[-1].strip() if tc0 else '')
s=s.replace('<LANDED_SHA>',sha).replace('<BATTERY_LINES>',body)
io.open(p,'w',encoding='utf-8',newline='').write(s)
import re; print("entry placeholders left:", re.findall(r'<[A-Z_]+>', s))
PY
echo "DONE -- read $S/coord-entry-train29-landed.md, then post it with coord-mailbox-post.ps1"
