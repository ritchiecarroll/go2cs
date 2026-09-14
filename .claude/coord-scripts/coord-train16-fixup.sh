#!/bin/bash
# Train 16 post-battery fixups, run AFTER the battery closes and BEFORE landing:
#  (1) NamedStringConsts re-transpiled IN PLACE (the CNR's NOT MEASURED package) and git-statused;
#  (2) ONE GoPositionMap line (syscall/syscall_linux.go) in src/core/syscall/linux/package_info.cs taken from a fresh seeded
#      -stdlib syscall emission at THIS tip with THIS tip's converter -- the value neither seat's converter emitted (C1's finding);
#      the seven pre-existing stale map lines in that file stay with the regen.
set -u
SP="/c/Projects/go2cs/.claude/coord-scripts"
export DOTNET_ROOT="$HOME\dotnet10"; export GOROOT="$HOME\sdk\go1.23.12"; export PATH="$HOME/dotnet10:$HOME/sdk/go1.23.12/bin:$PATH"
cd /c/Projects/go2cs/.claude/worktrees/musing-moser-d4552c || exit 1
stamp(){ echo "[$(date '+%F %T')] $*"; }
go version | grep -q go1.23.12 || { stamp "WRONG GO -- ABORT"; exit 1; }
[ "$(git status --porcelain | wc -l)" = "0" ] || { stamp "dirty -- ABORT"; exit 1; }
alive=$(powershell -NoProfile -Command "(Get-CimInstance Win32_Process | Where-Object { \$_.Name -eq 'go2cs.exe' -and \$_.ExecutablePath -like '*worktrees\musing-moser-d4552c\src\go2cs\bin\go2cs.exe' } | Measure-Object).Count" | tr -d '\r'); [ "$alive" = "0" ] || { stamp "a converter is alive in this worktree ($alive) -- ABORT"; exit 1; }
stamp "FIXUP START head=$(git rev-parse --short HEAD)"
(cd src/go2cs && go build -o bin/go2cs.exe . ) && stamp "converter rebuilt at $(git rev-parse --short HEAD): $(stat -c %s src/go2cs/bin/go2cs.exe) B"
# (1) NamedStringConsts
./src/go2cs/bin/go2cs.exe -go2cspath "$(pwd -W)\src" "$(pwd -W)\src\tests\Behavioral\NamedStringConsts" > "$SP/coord-t16-nsc-inplace.log" 2>&1; stamp "NamedStringConsts in-place transpile exit=$? dirty-in-pkg=$(git status --porcelain src/tests/Behavioral/NamedStringConsts | wc -l)"
git status --porcelain src/tests/Behavioral/NamedStringConsts | head -3
[ "$(git status --porcelain src/tests/Behavioral/NamedStringConsts | wc -l)" = "0" ] || { stamp "NamedStringConsts DRIFTS -- stop, this is a finding"; exit 2; }
# (2) the map line from a fresh seeded emission at this tip
V="$SP/coord-t16-fixup-seed"; rm -rf "$V"; mkdir -p "$V/src" "$V/docs"
powershell -NoProfile -Command "robocopy 'src\core' '$(cygpath -w "$V")\src\core' /E /XD bin obj Generated /NFL /NDL /NJH /NJS /NP | Out-Null; Copy-Item src\version.props '$(cygpath -w "$V")\src\'; robocopy 'docs\validation' '$(cygpath -w "$V")\docs\validation' /E /NFL /NDL /NJH /NJS /NP | Out-Null; 'seeded cs: ' + (Get-ChildItem '$(cygpath -w "$V")\src\core' -Recurse -Filter *.cs | Measure-Object).Count"
./src/go2cs/bin/go2cs.exe -stdlib syscall -comments -platforms linux/amd64 -go2cspath "$(cygpath -w "$V")\src" > "$SP/coord-t16-fixup-emit.log" 2>&1; stamp "seeded -stdlib syscall (linux) exit=$? emitted-mtime=$(stat -c %y "$V/src/core/syscall/linux/package_info.cs" | cut -c1-19)"
python - "$V/src/core/syscall/linux/package_info.cs" src/core/syscall/linux/package_info.cs <<'PY'
import io,re,sys
emit=io.open(sys.argv[1],encoding='utf-8-sig',newline='').read(); cur=io.open(sys.argv[2],encoding='utf-8-sig',newline='').read()
pat=re.compile(r'^(\[assembly: [^\r\n]*GoPositionMap\("syscall/syscall_linux\.go"[^\r\n]*)$', re.M)
e=pat.findall(emit); c=pat.findall(cur)
assert len(e)==1 and len(c)==1, (len(e),len(c))
if e[0]==c[0]: print("map line already identical -- nothing to apply"); sys.exit(0)
new=cur.replace(c[0], e[0], 1); assert new.count(e[0])==1
io.open(sys.argv[2],'w',encoding='utf-8',newline='').write(new)
# line-kind census of the applied delta
import difflib
d=[l for l in difflib.unified_diff(cur.splitlines(), new.splitlines(), lineterm='', n=0) if l.startswith(('+','-')) and not l.startswith(('+++','---'))]
print("applied delta lines:", len(d), "| all GoPositionMap(syscall/syscall_linux.go):", all('GoPositionMap("syscall/syscall_linux.go"' in l for l in d))
# how many map lines in the file still differ from the emission (the pre-existing staleness, untouched)
em=set(re.findall(r'^\[assembly: [^\r\n]*GoPositionMap\([^\r\n]*$', emit, re.M)); cm=set(re.findall(r'^\[assembly: [^\r\n]*GoPositionMap\([^\r\n]*$', new, re.M))
print("map lines still differing from the fresh emission (left to the regen):", len(cm-em))
PY
rc=$?; stamp "map-line apply exit=$rc numstat=$(git diff --numstat -- src/core/syscall/linux/package_info.cs | cut -f1,2 | tr '\t' '/')"
[ "$rc" = "0" ] || exit 3
if [ "$(git status --porcelain | wc -l)" = "1" ]; then
  git add src/core/syscall/linux/package_info.cs && git commit -S -q -F - <<'MSG'
syscall/linux: ONE GoPositionMap line (syscall/syscall_linux.go) re-derived at the train-16 tip -- the value neither seat's converter emitted

C1 found at the merge result that the keystone seat's map line for syscall/syscall_linux.go was derived at 8c15217c8, before train 15's comment drain (610aef4ae) shifted the file's positions -- three hashes (drain-only, displacement-only, both) and the merge took a value NO converter emits; C2 then showed the union has a FOURTH value because the multicast seat moves the same file by -28 lines without writing any map line (a co-seat following the hunk rule correctly is structurally invisible to a per-seat check). So the line is computed ONCE, here, at the assembled tip, from a seeded -stdlib syscall emission with this tip's own converter, and only that line is applied; the seven pre-existing stale map lines in this file stay with the deliberate regen. Line-kind census of the delta: one GoPositionMap line, nothing else.

Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>
MSG
  stamp "map fixup committed -> $(git rev-parse --short HEAD)"
else stamp "nothing to commit (dirty=$(git status --porcelain | wc -l))"; fi
rm -rf "$V"; stamp "FIXUP DONE head=$(git rev-parse --short HEAD) dirty=$(git status --porcelain | wc -l)"
