#!/bin/bash
# Train 17: UNSEAT the array-range copy seat (D4, merge f3ba6368e) at the tip after the battery caught its math/big regression,
# then re-run the battery on the reverted tree (SKIP_ASSEMBLE=1 through the assembly script's resume path).
set -u
SP="/c/Projects/go2cs/.claude/coord-scripts"
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
stamp(){ echo "[$(date '+%F %T')] $*"; }
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "dirty -- ABORT"; exit 1; }
alive=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.Name -eq 'go2cs.exe' -and \$_.ExecutablePath -like '*worktrees\musing-moser-d4552c\src\go2cs\bin\go2cs.exe' } | Measure-Object).Count" | tr -d '\r'); [ "$alive" = "0" ] || { stamp "a converter is alive in this worktree ($alive) -- ABORT"; exit 1; }
stamp "UNSEAT START head=$(git rev-parse --short HEAD)"
git revert --no-edit -m 1 f3ba6368e >/dev/null 2>&1 || { stamp "revert CONFLICTED -- ABORT for hand resolution"; git status --porcelain | head; exit 1; }
git commit --amend -S -q -F - <<'MSG'
Revert "Merge claude/sub-array-range-enumerator (D4, a82e8dce8)" -- UNSEATED from train 17

The train's own union sweep at Release caught a regression in the banked math/big row: 224 -> 222 matched + 2 infrastructure-error, all four rows one cause. TestFloatAdd and TestFloatMul die in array<T>.Clone() with an InvalidCastException (a cast defect in the new clone for that element shape, float_test.cs:1329); TestNewIntAllocs reads "wanted 0 allocations, got 1" because golib's AllocsPerRun mirror now counts the range copy, and TestMulUnbalanced reads "uses too much memory" because the bytes mirror counts it too -- where Go's array-range copy is a STACK copy that Mallocs and TotalAlloc never see. The copy is semantically right (seven of seven shapes matched go run at the seat's own gates) and its ACCOUNTING is wrong; the seat's first cut carried no banked-row sweeps, and a seat whose corpus footprint lands in banked packages owes those rows' sweeps as its own gate. Reverted here so the train lands with its other seats; the seat returns on train 18 with the clone cast fixed, the range copy an UNCOUNTED site by golib's instruments with the reason at the site, and math/big at 224 as its acceptance. The two behavioral goldens the union CNR reported CHANGED by this seat's line (ForeignIfaceFieldPointer, GoSyntaxIfaceFieldPointer) return to byte-identical with the revert.

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>
MSG
stamp "reverted -> $(git rev-parse --short HEAD) dirty=$(git status --porcelain | wc -l); D4 in HEAD: $(git merge-base --is-ancestor a82e8dce8 HEAD && echo STILL-ANCESTOR-(expected,-reverted-content) || echo no)"
git diff --stat HEAD~1 HEAD | tail -1
stamp "re-battery on the reverted tree"
SKIP_ASSEMBLE=1 CONTROL_SHA=6fa031d08 bash "$SP/coord-train17-assemble.sh"
