#!/bin/bash
# Train 17 post-battery fixup: the union CNR's two CHANGED behavioral files are D4's intended array-range `.Clone()` on two guards
# born on master after D4's base (8c49f4fc5, 2026-09-02 21:52 > 01a7fdefe). Re-transpile both in place with the train's converter,
# re-baseline their goldens from the emission, run the runner's four phases on both, and commit as a stated fixup.
set -u
SP="/c/Projects/go2cs/.claude/coord-scripts"
export DOTNET_ROOT="$HOME\dotnet10"; export GOROOT="$HOME\sdk\go1.23.12"; export PATH="$HOME/dotnet10:$HOME/sdk/go1.23.12/bin:$PATH"; export MSBUILDDISABLENODEREUSE=1
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
stamp(){ echo "[$(date '+%F %T')] $*"; }
go version | grep -q go1.23.12 || { stamp "WRONG GO -- ABORT"; exit 1; }
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "dirty -- ABORT"; exit 1; }
alive=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.Name -eq 'go2cs.exe' -and \$_.ExecutablePath -like '*worktrees\musing-moser-d4552c\src\go2cs\bin\go2cs.exe' } | Measure-Object).Count" | tr -d '\r'); [ "$alive" = "0" ] || { stamp "a converter is alive in this worktree ($alive) -- ABORT"; exit 1; }
stamp "FIXUP17 START head=$(git rev-parse --short HEAD)"
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B"
for p in ForeignIfaceFieldPointer GoSyntaxIfaceFieldPointer; do
  ./src/go2cs/bin/go2cs.exe -go2cspath "$(pwd -W)\src" "$(pwd -W)\src\tests\Behavioral\$p" > "$SP/coord-t17-fixup-$p.log" 2>&1; stamp "$p transpile exit=$? numstat=$(git diff --numstat -- src/tests/Behavioral/$p/main.cs | cut -f1,2 | tr '\t' '/')"
  git diff -- src/tests/Behavioral/$p/main.cs | grep -aE '^[-+] ' | cut -c1-100
  cp src/tests/Behavioral/$p/main.cs src/tests/Behavioral/$p/main.cs.target
done
stamp "dirty after re-baseline: $(git status --porcelain | wc -l) (expect 4: two main.cs + two main.cs.target)"; git status --porcelain
(cd src/tests/Behavioral && for p in ForeignIfaceFieldPointer GoSyntaxIfaceFieldPointer; do powershell -NoProfile -ExecutionPolicy Bypass -File ./run-behavioral.ps1 --filter $p > "$SP/coord-t17-fixup-runner-$p.log" 2>&1; echo "runner $p exit=$? $(grep -aoE 'PASS|FAIL \[[A-Za-z,]+\]' "$SP/coord-t17-fixup-runner-$p.log" | tail -1)"; done)
git checkout HEAD -- src/tests/Behavioral/BehavioralTests 2>/dev/null; git status --porcelain | grep -vE 'main\.cs(\.target)?$' | head -3
if [ "$(git status --porcelain | wc -l)" = "4" ]; then
  git add src/tests/Behavioral/ForeignIfaceFieldPointer src/tests/Behavioral/GoSyntaxIfaceFieldPointer && git commit -S -q -F - <<'MSG'
tests/Behavioral: two goldens re-baselined at the train-17 tip -- D4's intended array-range copy on two guards born after its base

The union CNR at 303d74382 reported exactly two CHANGED behavioral files, ForeignIfaceFieldPointer/main.cs and GoSyntaxIfaceFieldPointer/main.cs, each differing by ONE line: `foreach (var (i, ca) in connAddrs)` -> `foreach (var (i, ca) in connAddrs.Clone())` -- the array-range copy the D4 seat (a82e8dce8) emits at the range expression, exactly as it re-baselined fourteen other guards. Both projects were added to master at 8c49f4fc5 (2026-09-02 21:52), after D4's base 01a7fdefe (16:52), so D4's own CNR at 699 packages could not see them; the union is where later-born guards meet a seat. Re-transpiled in place with the train's converter, goldens copied from the emission, and the runner's four phases run on both -- Output compared against `go run`, which the copy moves toward Go's own semantics. Classified by the diff's CONTENT (the seat's own intended line, nothing else), stated here rather than folded into the merge.

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>
MSG
  stamp "fixup committed -> $(git rev-parse --short HEAD) dirty=$(git status --porcelain | wc -l)"
else stamp "unexpected dirt count -- NOT committed"; git status --porcelain | head; exit 2; fi
stamp "FIXUP17 DONE"
