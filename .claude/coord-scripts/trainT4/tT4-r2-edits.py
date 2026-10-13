# tT4-r2-edits.py -- TRAIN T4 run2: the kit changes over run1's kit, every edit anchored and asserted (an anchor that
# does not occur exactly the stated number of times aborts with nothing written). Run ONCE on the fresh run2 copy.
#   python -B tT4-r2-edits.py <run2 folder>
# R2   names: the union worktree /h/go2cs-tmp-coord/tT4r2 on the LOCAL branch train-t4-union-r2; the map worktree
#      tT4r2-map; the assembly and map stamps in coord-scratch/tT4/run2/stamps (run1's stamps in coord-scratch/tT4 are
#      left as they are); the fixup's emission scratch tT4emitr2. Each worktree guard ALSO keeps tT4 (run1's) refused
#      as a launch folder and accepted nowhere new.
# T4-13 the assembler's R1 DELTA arm (r1delta, between markers): the last seat merge's tree must differ from run1's
#      union tree (R1_TREE) by EXACTLY the paths of the row range R1_DELTA (a..b); DIFFERENT or not comparable (when
#      either variable is given) is exit 4 at the end, as I9 TREE.
# T4-14 battery leg NGF-j0 RETIRED (its target src/tools/j0-uuid-rehearsal left with cfe57cd718), and j0-consume.ps1
#      out of NGb's parse list (the same removal: run1's NGb51 / NGb7 read errors=1 on exactly that file).
import os, sys

D = sys.argv[1]
edits = {}

def rep(f, old, new, count=1):
    edits.setdefault(f, []).append((old, new, count))

GUARD_OLD = '/h/go2cs-tmp-coord/tT4|/h/go2cs-tmp-coord/tT4/*|/h/go2cs-tmp-coord/hnd|'
GUARD_NEW = '/h/go2cs-tmp-coord/tT4|/h/go2cs-tmp-coord/tT4/*|/h/go2cs-tmp-coord/tT4r2|/h/go2cs-tmp-coord/tT4r2/*|/h/go2cs-tmp-coord/hnd|'
for f in ('tT4-assemble.sh', 'tT4-battery.sh', 'tT4-emitcheck.sh', 'tT4-fixup.sh', 'tT4-modules-legs.sh'):
    rep(f, GUARD_OLD, GUARD_NEW)
WPAT_OLD = '  /h/go2cs-tmp-coord:tT4|/h/go2cs-tmp-coord:tT4[-_.]?*) ;;'
WPAT_NEW = '  /h/go2cs-tmp-coord:tT4|/h/go2cs-tmp-coord:tT4r2|/h/go2cs-tmp-coord:tT4[-_.]?*) ;;   # R2 (run2): + tT4r2'
for f in ('tT4-fixup.sh', 'tT4-emitcheck.sh'):
    rep(f, WPAT_OLD, WPAT_NEW)
# assembler
rep('tT4-assemble.sh', '\nWT=/h/go2cs-tmp-coord/tT4\n', '\nWT=/h/go2cs-tmp-coord/tT4r2   # R2 (run2)\n')
rep('tT4-assemble.sh', '"$X/tT4/', '"$X/tT4/run2/stamps/', 25)
rep('tT4-assemble.sh', 'mkdir -p "$X/tT4"\n', 'mkdir -p "$X/tT4/run2/stamps"   # R2 (run2): run1\'s stamps in coord-scratch/tT4 are left as they are\n')
rep('tT4-assemble.sh', '= train-t4-union ]', '= train-t4-union-r2 ]')
rep('tT4-assemble.sh', 'git worktree add -q -b train-t4-union "$WT"', 'git worktree add -q -b train-t4-union-r2 "$WT"')
# map
rep('tT4-conflict-map.sh', '\nWT=/h/go2cs-tmp-coord/tT4-map\n', '\nWT=/h/go2cs-tmp-coord/tT4r2-map   # R2 (run2): run1\'s tT4-map is left as it is\n')
rep('tT4-conflict-map.sh', '\nX=/h/go2cs-tmp-coord/coord-scratch/tT4\n', '\nX=/h/go2cs-tmp-coord/coord-scratch/tT4/run2/stamps   # R2 (run2)\n')
# fixup
rep('tT4-fixup.sh', '\nW=${W:-/h/go2cs-tmp-coord/tT4}\n', '\nW=${W:-/h/go2cs-tmp-coord/tT4r2}   # R2 (run2)\n')
rep('tT4-fixup.sh', '\nBRANCH=train-t4-union\n', '\nBRANCH=train-t4-union-r2   # R2 (run2)\n')
rep('tT4-fixup.sh', '\nES=/h/go2cs-tmp-coord/tT4emitfix ', '\nES=/h/go2cs-tmp-coord/tT4emitr2  ')
rep('tT4-fixup.sh', '\nMTF=/h/go2cs-tmp-coord/coord-scratch/tT4/assemble-maptree.txt', '\nMTF=/h/go2cs-tmp-coord/coord-scratch/tT4/run2/stamps/assemble-maptree.txt')
# emitcheck
rep('tT4-emitcheck.sh', '\nW=${W:-/h/go2cs-tmp-coord/tT4}   #', '\nW=${W:-/h/go2cs-tmp-coord/tT4r2}   # R2 (run2);')
# module legs
rep('tT4-modules-legs.sh', "\nW=/h/go2cs-tmp-coord/tT4\nWB='H:\\go2cs-tmp-coord\\tT4'\n", "\nW=/h/go2cs-tmp-coord/tT4r2   # R2 (run2)\nWB='H:\\go2cs-tmp-coord\\tT4r2'\n")
# battery
rep('tT4-battery.sh', "\nW=/h/go2cs-tmp-coord/tT4\nWB='H:\\go2cs-tmp-coord\\tT4'\n", "\nW=/h/go2cs-tmp-coord/tT4r2   # R2 (run2)\nWB='H:\\go2cs-tmp-coord\\tT4r2'\n")
rep('tT4-battery.sh', '-p:go2csPath=H:/go2cs-tmp-coord/tT4/src/', '-p:go2csPath=H:/go2cs-tmp-coord/tT4r2/src/', 3)
rep('tT4-battery.sh', "[char]92 + 'tT4' + [char]92; \\$p2 = 'go2cs-tmp-coord' + '/' + 'tT4' + '/';", "[char]92 + 'tT4r2' + [char]92; \\$p2 = 'go2cs-tmp-coord' + '/' + 'tT4r2' + '/';")
# re-read and landing scripts
for f in ('tT4-reread.sh', 'tT4-land-prep.sh', 'tT4-land-final-reads.sh', 'tT4-land-rereads.sh'):
    rep(f, '\nW=${W:-/h/go2cs-tmp-coord/tT4}\n', '\nW=${W:-/h/go2cs-tmp-coord/tT4r2}   # R2 (run2)\n')
for f in ('tT4-reread.sh', 'tT4-land-prep.sh', 'tT4-land-final-reads.sh'):
    rep(f, 'case "$W" in /h/go2cs-tmp-coord/tT4|/h/go2cs-tmp-coord/tT4-[A-Za-z0-9]*) ;;', 'case "$W" in /h/go2cs-tmp-coord/tT4|/h/go2cs-tmp-coord/tT4r2|/h/go2cs-tmp-coord/tT4-[A-Za-z0-9]*) ;;')
# helpers: the LIVE census's worktree fragment
rep('tT4-helpers.py', "for p in (os.path.join(top, 'tT4') + os.sep, root + os.sep):", "for p in (os.path.join(top, 'tT4r2') + os.sep, root + os.sep):   # R2 (run2)")

# ---- T4-13: the R1 DELTA arm
R1FUNC = r'''# >>> r1delta (T4-13, run2; tT4-controls-r2.sh arm R1D extracts the lines between the markers and runs them on planted heads)
r1delta(){ # head, run1's union TREE (10 to 40 hex), the row range a..b -> verdict line(s); rc 0 EXACT; 1 DIFFERENT; 2 not comparable
  local h=$1 pt=${2:-} rg=${3:-} ht a b got want extra miss nw nb p
  printf '%s' "$pt" | grep -qE '^[0-9a-f]{10,40}$' && git cat-file -e "$pt^{tree}" 2>/dev/null || { echo "R1 DELTA: NOT COMPARED (R1_TREE='$pt' is not a tree in this object store)"; return 2; }
  case "$rg" in ?*..?*) a=${rg%%..*}; b=${rg##*..} ;; *) echo "R1 DELTA: NOT COMPARED (R1_DELTA='$rg' is not a range a..b)"; return 2 ;; esac
  git rev-parse -q --verify "$a^{commit}" > /dev/null && git rev-parse -q --verify "$b^{commit}" > /dev/null || { echo "R1 DELTA: NOT COMPARED (R1_DELTA=$rg does not resolve)"; return 2; }
  ht=$(git rev-parse -q --verify "$h^{tree}") || { echo "R1 DELTA: NOT COMPARED (${h:0:10} does not resolve to a commit)"; return 2; }
  got=$(git diff --name-only "$pt" "$ht" | LC_ALL=C sort); want=$(git diff --name-only "$a" "$b" | LC_ALL=C sort)
  extra=$(LC_ALL=C comm -13 <(printf '%s\n' "$want" | grep .) <(printf '%s\n' "$got" | grep .))
  miss=$(LC_ALL=C comm -23 <(printf '%s\n' "$want" | grep .) <(printf '%s\n' "$got" | grep .))
  nw=$(printf '%s\n' "$want" | grep -c .); nb=0
  while IFS= read -r p; do [ -n "$p" ] || continue; [ "$(git rev-parse -q --verify "$ht:$p")" = "$(git rev-parse -q --verify "$b:$p")" ] && nb=$((nb + 1)); done <<< "$want"
  if [ -z "$extra$miss" ]; then
    echo "R1 DELTA: EXACT -- $(git rev-parse --short=10 "$h") tree ${ht:0:10} differs from run1's union tree ${pt:0:10} by exactly the $nw path(s) of ${a:0:10}..${b:0:10} [$(printf '%s\n' "$want" | grep . | tr '\n' ' ')]; $nb of $nw hold ${b:0:10}'s blob"
    return 0
  fi
  echo "R1 DELTA: DIFFERENT -- $(git rev-parse --short=10 "$h") tree ${ht:0:10} vs run1's union tree ${pt:0:10}: $(printf '%s\n' "$got" | grep -c .) path(s) differ, the range ${a:0:10}..${b:0:10} names $nw; EXTRA (not in the range) $(printf '%s\n' "$extra" | grep -c .), MISSING (in the range, not different) $(printf '%s\n' "$miss" | grep -c .)"
  printf '%s\n' "$extra" | grep . | head -n 20 | sed 's/^/  EXTRA /'
  printf '%s\n' "$miss" | grep . | head -n 20 | sed 's/^/  MISSING /'
  return 1
}
# <<< r1delta
'''
rep('tT4-assemble.sh', "MTBAD=1   # verify round 3:", R1FUNC + "MTBAD=1   # verify round 3:")
rep('tT4-assemble.sh', '''case "$i9rc" in 0) ;; 1) I9BAD=1 ;; *) [ -z "${I9_TREE:-}" ] || I9BAD=1 ;; esac   # not comparable: a NOTE, a STOP when COORD posted a tree
''', '''case "$i9rc" in 0) ;; 1) I9BAD=1 ;; *) [ -z "${I9_TREE:-}" ] || I9BAD=1 ;; esac   # not comparable: a NOTE, a STOP when COORD posted a tree
# T4-13 (run2): no i9 rehearsal of this union; run1's union tree is the reference. EXACT is the only pass when either
# variable is given (not comparable is then a STOP too); neither given is a NOTE.
R1V=$(r1delta "$SEATTOP" "${R1_TREE:-}" "${R1_DELTA:-}"); r1rc=$?
printf '%s\\n' "$R1V"
echo "head=$(git rev-parse --short=10 HEAD) :: $(printf '%s' "$R1V" | head -n 1) (R1_TREE=${R1_TREE:-unset} R1_DELTA=${R1_DELTA:-unset})" >> "$X/tT4/run2/stamps/assemble-maptree.txt"
R1BAD=0
case "$r1rc" in 0) ;; 1) R1BAD=1 ;; *) [ -z "${R1_TREE:-}${R1_DELTA:-}" ] || R1BAD=1 ;; esac
''')
rep('tT4-assemble.sh', '''[ "$MTBAD" = 0 ] || { echo "STOP: $MT''', '''[ "$R1BAD" = 0 ] || { echo "STOP: $(printf '%s' "$R1V" | head -n 1) -- the assembled union stands at $(git rev-parse --short=10 HEAD) and is NOT accepted (exit 4): T4-13, COORD reads the listed paths"; exit 4; }
[ "$MTBAD" = 0 ] || { echo "STOP: $MT''')

# ---- T4-14: leg NGF-j0 retired
rep('tT4-battery.sh', '''NGF="$(git ls-files 'src/tools/nugetgo/*.ps1' 'src/tools/nugetgo/*.psm1' | tr '\\n' ' ')src/tools/j0-uuid-rehearsal/j0-consume.ps1"''',
    '''NGF="$(git ls-files 'src/tools/nugetgo/*.ps1' 'src/tools/nugetgo/*.psm1' | tr '\\n' ' ')"   # T4-14 (run2): j0-consume.ps1 left with src/tools/j0-uuid-rehearsal (cfe57cd718)''')
J0_START = 'J0C=src/tools/j0-uuid-rehearsal/consumer/J0UuidConsumer.csproj\n'
J0_END_MARK = '''|| finding "NGF-j0 consumer ID (default rc=$j0a n=$j0an go.=$j0ago; nugetgo rc=$j0b n=$j0bn; property rc=$j0c '${j0cv:-unread}'; script default '${j0ps:-unread}')"\n'''
J0_NEW = '''# T4-14 (run2, COORD 2026-10-13): leg NGF-j0 is RETIRED. Its target, src/tools/j0-uuid-rehearsal (the J0 consumer and
# j0-consume.ps1), was removed by master's cleanup cfe57cd718, which the base holds: run1's battery read rc=1 on its three
# msbuild reads (and NGb's parse of the missing j0-consume.ps1, now out of NGF). Every other NG leg stands. Guard: the
# folder back at HEAD is a finding (the leg is then owed again).
if git cat-file -e "HEAD:src/tools/j0-uuid-rehearsal" 2>/dev/null; then
  finding "NGF-j0 is RETIRED (T4-14) but src/tools/j0-uuid-rehearsal is back at HEAD: the leg is owed again"
else
  stamp "  NGF-j0: RETIRED (T4-14): src/tools/j0-uuid-rehearsal is absent at HEAD (removed by cfe57cd718); its three msbuild reads and the script-default read are not run"
fi
'''

def apply_j0(s):
    i = s.find(J0_START); j = s.find(J0_END_MARK)
    assert s.count(J0_START) == 1 and s.count(J0_END_MARK) == 1 and 0 < i < j, 'T4-14 block anchors'
    j = s.rfind('\n', 0, j) + 1   # the start of the finding line ([ ... ] || finding ...)
    j = s.index(J0_END_MARK, j) + len(J0_END_MARK)
    blk = s[i:j]
    assert blk.count('leg NGF-j0') == 3 and blk.count('\nleg ') == 3, f'T4-14 block holds {blk.count("leg NGF-j0")} NGF-j0 legs'
    return s[:i] + J0_NEW + s[j:]

out = {}
for f, es in edits.items():
    p = os.path.join(D, f)
    s = open(p, encoding='utf-8', newline='').read()
    for old, new, n in es:
        c = s.count(old)
        assert c == n, f'{f}: anchor found {c} times, want {n}: {old[:90]!r}'
        s = s.replace(old, new)
    out[f] = s
out['tT4-battery.sh'] = apply_j0(out['tT4-battery.sh'])
for f, s in out.items():
    assert '\r' not in s
    with open(os.path.join(D, f), 'w', encoding='utf-8', newline='') as fh: fh.write(s)
    print(f'EDITED {f}: {len(edits.get(f, []))} anchored edit(s)' + (' + the T4-14 block' if f == 'tT4-battery.sh' else ''))
print('R2-EDITS DONE')
