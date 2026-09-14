# coord-derive-train29.py -- derive the train-27 assemble/rehearse/land/land-launch scripts from train 26's with the seat block rewritten.
import io, re, os
NL = chr(10); BS = chr(92)
SP = 'C:/Projects/go2cs/.claude/coord-scripts/'
SEATS = [  # (var, branch, default sha, msgfile, label)
    ('C2Q44',  'claude/c2-q44-cut',                  '',          'coord-merge-c2-q44-cut.txt',  'C2 Q44: the managed pointer token for reference-bearing boxes + the syncTimer displacement + the registry-growth arm + SUB-Q27 labels re-entry'),
    ('C2Q49',  'claude/c2-q49-cut',                  'd5645ab97',          'coord-merge-c2-q49.txt',      'C2 Q49: the KeepAlive predicate widened -- the bridged-wrapper arm + the darwin funnel arm (third package) + the census guard darwin arm + C1 arm-7 twin'),
    ('GFVC',  'claude/g-field-view-cut',            'a5d40fdfc',          'coord-merge-g-field-view-cut.txt', 'G: the field-view cache CUT (arm 3, SlottedStandardBox + per-T weak table + the seven-arm guard) -- branch name set at the announce'),
    ('GA', 'claude/g-elem-take-concrete', '955e271c0', 'coord-merge-g-elem-take.txt', 'G candidate A: the concrete-header element-take overloads (golib; -1 object per &s[i] on 520 sites; the os row 488.25/6 -> 376.25/4 MET)'),
    ('SUBQ57', 'claude/sub-q57', '588a01aaa', 'coord-merge-sub-q57.txt', 'SUB-Q57 named-array empty literal constructs its elements (layer A)'),
    ('C1RT6', 'claude/c1-runtime-inc6-mem', 'd8cecc3ce', 'coord-merge-c1-rt6.txt', 'C1 increment 6 = the memory family: sysMmap/sysMunmap/madvise/usleep over libc (linux), persistentalloc1/inPersistentAlloc displaced (W1 retired on every row)'),
    ('C1RT7', 'claude/c1-runtime-inc7-w2a', '846c36e1e', 'coord-merge-c1-rt7.txt', 'C1 increment 7: W2a (addrRanges writers displaced over OverNativeMemory(base,len,cap) + HeaderSliceBox) + linux bootstrap constants + the promoted-selection arm gate; SHA at the announce'),
    ('RE3B', 'claude/reflect-value-singles-inc-e3', '6a7ea30be', 'coord-merge-r-e3b.txt', 'R E3 roots 4-5 + the order-token amendment (ccf4776b8) + Convert 7b/7c (3eff1e1ca) -- the branch tip behind the train-28 seat 10eecadb9, filled at the announce'),
    ('C2Q56D', 'claude/c2-q56-design', '8c1d2d506', 'coord-merge-c2-q56-design.txt', 'C2 Q56 DESIGN: the whole cgo_unsafe_args parameter block lifted for darwin libcCall (docs only; findings: shape (d) result discarded, the darwin sockaddr door)'),
    ('C2Q52D', 'claude/c2-q52-design', '15968370d', 'coord-merge-c2-q52-design.txt', 'C2 Q52 DESIGN: the os/signal posix bridge darwin flavour under its distinct basename (docs only)'),
    ('C2INC7', 'claude/c2-darwin-inc7', '48291283b', 'coord-merge-c2-inc7.txt', 'C2 darwin increment 7: libcCall returns the C result (20 readers) and a null box is the zero-argument trampoline; getpid guards both arms'),
    ('C2INC8', 'claude/c2-darwin-inc8', '261f3e1e4', 'coord-merge-c2-inc8.txt', 'C2 darwin increment 8: the sockaddr TWIN over the BSD layout + the datagram companion; twelve names widened; the five net rows predicted to move to runtime: kevent failed'),
    ('C2Q52B', 'claude/c2-q52-bridge', '554620235', 'coord-merge-c2-q52-bridge.txt', 'C2 Q52 BRIDGE: the os/signal posix bridge darwin flavour under its distinct basename; SignalPrimitives predicted exit 0 / stderr 0 / stdout 6 on both mac legs'),
    ('SUBQ62', 'claude/sub-q62', 'd97193e1f', 'coord-merge-sub-q62.txt', 'SUB-Q62: the registration ledger guard gains the FLAVOUR axis (census 290 x 3 = 0 fires; guard-only)'),
    ('SUBDOC11', 'claude/sub-doc11', '5b407a952', 'coord-merge-sub-doc11.txt', 'SUB-DOC11: CLAUDE.md doctrine batch 11 (accumulator items 493-516), docs only'),
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
# --- assemble + rehearse
for src, dst in [('coord-train28-assemble.sh', 'coord-train29-assemble.sh'), ('coord-train28-rehearse.sh', 'coord-train29-rehearse.sh')]:
    t = rd(SP + src).replace('train28', 'train29').replace('TRAIN 28', 'TRAIN 29').replace('coord-t28-resolutions', 'coord-t29-resolutions')
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
t = rd(SP + 'coord-train28-land.sh').replace('train28', 'train29').replace('TRAIN 28', 'TRAIN 29')
t = re.sub(r'^for b in .*$', 'for b in ' + ' '.join('${%s_SHA:+${%s_BRANCH:-%s}}' % (v, v, b) for v, b, s, m, lab in SEATS) + '; do', t, count=1, flags=re.M)
wr(SP + 'coord-train29-land.sh', t)
# --- land-launch: replace the env assignment lines
t = rd(SP + 'coord-train28-land-launch.sh').replace('train28', 'train29').replace('TRAIN 28', 'TRAIN 29')
lines = t.split(NL); env_idx = [i for i, l in enumerate(lines) if re.match(r'^[A-Z0-9]+_SHA="\$\{[A-Z0-9]+_SHA:-', l)]
assert env_idx
first = env_idx[0]
newenv = ['%s_SHA="${%s_SHA:-%s}" %s_BRANCH="${%s_BRANCH:-%s}" %s' % (v, v, s, v, v, b, BS) for v, b, s, m, lab in SEATS]
lines = [l for i, l in enumerate(lines) if i not in env_idx]
lines[first:first] = newenv
wr(SP + 'coord-train29-land-launch.sh', NL.join(lines))
os.makedirs(SP + 'coord-t29-resolutions', exist_ok=True)
print('derived train-27 scripts:', [f for f in os.listdir(SP) if f.startswith('coord-train29-')])
add_worktree_guard('coord-train29-assemble.sh', 'musing-moser-d4552c')
add_worktree_guard('coord-train29-rehearse.sh', 'coord-t25-dryrun')
add_worktree_guard('coord-train29-land.sh', 'musing-moser-d4552c')
