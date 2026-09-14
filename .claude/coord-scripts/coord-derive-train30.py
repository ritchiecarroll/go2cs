# coord-derive-train30.py -- derive the train-27 assemble/rehearse/land/land-launch scripts from train 26's with the seat block rewritten.
import io, re, os
NL = chr(10); BS = chr(92)
SP = 'C:/Projects/go2cs/.claude/coord-scripts/'
SEATS = [  # (var, branch, default sha, msgfile, label)
    ('C2Q44', 'claude/c2-q44-cut', 'eed11b550', 'coord-merge-c2-q44-cut.txt', 'C2 Q44: the managed pointer token for reference-bearing boxes (take 2: the void* in-operator line; the F13 row demoted to compile-shape) -- SHA at the announce'),
    ('C2INC9', 'claude/c2-darwin-inc9', 'd185e28b8', 'coord-merge-c2-inc9.txt', 'C2 darwin increment 9: the darwin bridge sigignore installs the kernel SIG_IGN + the job-control trio mapped (acceptance OWED to increment 10)'),
    ('C2Q56L', 'claude/c2-q56-lift', '0ac8a607c', 'coord-merge-c2-q56-lift.txt', 'C2 Q56 LIFT: the //go:cgo_unsafe_args block lift (converter rule + fixture guard, golib forms, darwin table; 27 lifted bodies in sys_darwin.cs; darwin census predicted to move 0 rows)'),
    ('C2INC10', 'claude/c2-darwin-inc10', '4efd81cf5', 'coord-merge-c2-inc10.txt', 'C2 darwin INCREMENT 10 (a): the five keystone pull-linknames bodied in internal/syscall/unix darwin companion, two public doors in the keystone hand-own, the SyscallKeystonePulls guard row; on increment 9'),
    ('SUBQ63', 'claude/sub-q63', '66a73ab03', 'coord-merge-sub-q63.txt', 'SUB-Q63: the unique row Blocker A -- the companion type parameter at reflect.TypeFor[T] use sites (16 -> 19 predicted)'),
    ('SUBQ60', 'claude/sub-q60', '16d1943ac', 'coord-merge-sub-q60.txt', 'SUB-Q60: the named-array wrapper zero value with a needy element (layer B; carrier sized first)'),
    ('SUBQ59', 'claude/sub-q59', '1dd5bf492', 'coord-merge-sub-q59.txt', 'SUB-Q59: the -tests comparison record carries the host stderr tail (additive field)'),
    ('GBD', 'claude/g-design-b-outparam', '58e83c419', 'coord-merge-g-design-b.txt', 'G DESIGN B: the syscall out-parameter (ref-lowering + fixed over the ref through the funnel; population 56/25/34; os row 376.25/4 -> 184.25/2 predicted) -- docs only'),
    ('GED', 'claude/g-design-e-elemaddr', 'b4337813a', 'coord-merge-g-design-e.txt', 'G DESIGN E: the syscall buffer element address (fixed-scope from the caller side; population 41/26/28 + 4/20/14; os row 184.25/2 -> 64.25/1 predicted) -- docs only'),
    ('GCD', 'claude/g-design-c-strwindow', '6db8d95a2', 'coord-merge-g-design-c.txt', 'G DESIGN C: the string byte-window (the element box between two already-zero-copy halves; converter recognition emitting s.Slice(0,len(s)); population 2 production sites; os row 64.25/1 -> 0.25/0 predicted) -- docs only'),
    ('GFVCR', 'claude/g-fvc-record-measured', '2f43ef7b3', 'coord-merge-g-fvc-record.txt', 'G: the field-view-cache record section 9, the cut MEASURED at the train-29 landing (552.25/7 -> 488.25/6 -> 376.25/4 by A); docs only, off the landed master'),
    ('RE2B', 'claude/reflect-embedded-inc-e2b', 'ca74dd433', 'coord-merge-r-e2b.txt', 'R E2b + 7e-b + 7g: the embedded-BUILTIN [GoEmbedded] marker, the [GoChanDir] and widened [GoArrayDims] sibling attributes (TestConvert GREEN, 66 -> 65) + the reflectlite test-companion fix; stacked on RE3B'),
    ('C1Q61', 'claude/c1-runtime-q61-parkhook', 'e33e14ccf', 'coord-merge-c1-q61.txt', 'C1 Q61: the park hook -- golib ParkTransition slot at the outermost park boundary, runtime installs gopark/ready halves (casgstatus _Grunning <-> _Gwaiting); TestMutexWaitTimeMetric PASS; stacked on C1RT7'),
    ('C1Q64', 'claude/c1-runtime-q64-sigign', '7ab3d6fa6', 'coord-merge-c1-q64.txt', 'C1 Q64: Ignore becomes the KERNEL disposition in the linux signal bridge for the CLR-free class (USR1/USR2 + TSTP/TTIN/TTOU); TestForeground pair PASSES under a real tty where master hangs; on 9c44a6d6a'),
    ('C1Q58D', 'claude/c1-q58-design', '44fba8cf6', 'coord-merge-c1-q58-design.txt', 'C1 Q58 DESIGN: the native-backed array pointer (a virtual consultation inside arrayView answered by the native box with OverNativeMemory; NativeBox<array<T>> refuted as the crash itself; increment 8 = the W1 write + W2b read pair) -- docs only'),
    ('GDC', 'claude/g-deferred-class', '67eba534f', 'coord-merge-g-deferred-class.txt', 'G: the DEFERRED / STRUCTURAL disclosure classes in the schema and the roster guard (41 manifests, 190 entries), refused without their plan, both PowerShell editions; the owner-ratified ruling made mechanical'),
]


def rd(p): return io.open(p, encoding='utf-8', newline='').read()
def wr(p, t): io.open(p, 'w', encoding='utf-8', newline='').write(t)
def strip_seat_vars(t):
    return NL.join(l for l in t.split(NL) if not re.match(r'^[A-Z0-9]+_SHA="\$\{[A-Z0-9]+_SHA:-', l))
def add_worktree_guard(path, wt):
    # WORKTREE GUARD (doctrine 515): a train script without its own cd runs in the caller's cwd.
    t = io.open(path, encoding='utf-8', newline='').read()
    if 'show-toplevel' in t: return
    g = 'cd /c/Projects/go2cs/.claude/worktrees/' + wt + ' || exit 2' + chr(10) + '[ "$(git rev-parse --show-toplevel)" = "C:/Projects/go2cs/.claude/worktrees/' + wt + '" ] || { echo "WRONG WORKTREE: $(pwd)"; exit 2; }' + chr(10)
    i = t.find('set -u'); assert i >= 0, path
    j = t.find(chr(10), i) + 1
    io.open(path, 'w', encoding='utf-8', newline='').write(t[:j] + g + t[j:])
def add_impl_test_class_leg(path):
    # doctrine 542: every banked row with a *_impl_test.cs companion gets its test host BUILT at assembly, and the sweep list carries it.
    t = io.open(path, encoding='utf-8', newline='').read()
    if 'coord-train30-reflectlite-' in t: return
    t = t.replace('crypto/internal/alias,slices ', 'crypto/internal/alias,slices,internal/reflectlite ', 1)
    lines = t.split(chr(10)); out = []
    for l in lines:
        out.append(l)
        if l.startswith('stamp "LEG reflect '):
            out.append('stamp "BATTERY: internal/reflectlite -tests build (doctrine 542: a banked row with a *_impl_test.cs companion)"')
            out.append('powershell -NoProfile -ExecutionPolicy Bypass -File "$SP/coord-reflect-tests-build.ps1" -Package internal/reflectlite > "$SP/coord-train30-reflectlite-$ts.log" 2>&1; echo "reflectlite exit=$?" >> "$SP/coord-train30-reflectlite-$ts.log"')
            out.append('stamp "LEG reflectlite $(tail -1 "$SP/coord-train30-reflectlite-$ts.log") :: $(grep -aE \'exit=\' "$SP/coord-train30-reflectlite-$ts.log" | tr \'\\n\' \' \')"')
    io.open(path, 'w', encoding='utf-8', newline='').write(chr(10).join(out))
# --- assemble + rehearse
for src, dst in [('coord-train29-assemble.sh', 'coord-train30-assemble.sh'), ('coord-train29-rehearse.sh', 'coord-train30-rehearse.sh')]:
    t = rd(SP + src).replace('train29', 'train30').replace('TRAIN 29', 'TRAIN 30').replace('coord-t29-resolutions', 'coord-t30-resolutions')
    t = strip_seat_vars(t)
    varblock = NL.join('%s_SHA="${%s_SHA:-%s}"; %s_BRANCH="${%s_BRANCH:-%s}"   # %s' % (v, v, s, v, v, b, lab[:80]) for v, b, s, m, lab in SEATS)
    t = t.replace('export MSBUILDDISABLENODEREUSE=1', 'export MSBUILDDISABLENODEREUSE=1' + NL + varblock, 1)
    lines = t.split(NL); seat_idx = [i for i, l in enumerate(lines) if l.startswith('seat ')]
    assert seat_idx, src
    first = seat_idx[0]
    newseats = ['seat "$%s_BRANCH" "$%s_SHA" "$SP/%s" "%s"' % (v, v, m, lab) for v, b, s, m, lab in SEATS]
    lines = [l for i, l in enumerate(lines) if i not in seat_idx]
    lines[first:first] = newseats
    wr(SP + dst, NL.join(lines))
# --- land
t = rd(SP + 'coord-train29-land.sh').replace('train29', 'train30').replace('TRAIN 29', 'TRAIN 30')
t = re.sub(r'^for b in .*$', 'for b in ' + ' '.join('${%s_SHA:+${%s_BRANCH:-%s}}' % (v, v, b) for v, b, s, m, lab in SEATS) + '; do', t, count=1, flags=re.M)
wr(SP + 'coord-train30-land.sh', t)
# --- land-launch: replace the env assignment lines
t = rd(SP + 'coord-train29-land-launch.sh').replace('train29', 'train30').replace('TRAIN 29', 'TRAIN 30')
lines = t.split(NL); env_idx = [i for i, l in enumerate(lines) if re.match(r'^[A-Z0-9]+_SHA="\$\{[A-Z0-9]+_SHA:-', l)]
assert env_idx
first = env_idx[0]
newenv = ['%s_SHA="${%s_SHA:-%s}" %s_BRANCH="${%s_BRANCH:-%s}" %s' % (v, v, s, v, v, b, BS) for v, b, s, m, lab in SEATS]
lines = [l for i, l in enumerate(lines) if i not in env_idx]
lines[first:first] = newenv
wr(SP + 'coord-train30-land-launch.sh', NL.join(lines))
os.makedirs(SP + 'coord-t30-resolutions', exist_ok=True)
print('derived train-27 scripts:', [f for f in os.listdir(SP) if f.startswith('coord-train30-')])
add_worktree_guard('coord-train30-assemble.sh', 'musing-moser-d4552c')
add_worktree_guard('coord-train30-rehearse.sh', 'coord-t25-dryrun')
add_worktree_guard('coord-train30-land.sh', 'musing-moser-d4552c')
add_impl_test_class_leg('coord-train30-assemble.sh')
