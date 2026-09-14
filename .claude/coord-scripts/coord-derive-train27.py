# coord-derive-train27.py -- derive the train-27 assemble/rehearse/land/land-launch scripts from train 26's with the seat block rewritten.
import io, re, os
NL = chr(10); BS = chr(92)
SP = 'C:/Projects/go2cs/.claude/coord-scripts/'
SEATS = [  # (var, branch, default sha, msgfile, label)
    ('GB2',   'claude/g-b2-widenings',              '39624e080',          'coord-merge-g-b2.txt',        'G B2: the two defer-finally widenings (conditional defers + receiver-method) with the 35-file footprint, nss.cs pair applied after Q35; os row 744.25/8 -> 552.25/7'),
    ('GQ48',  'claude/g-q48-trace-header',          'c5e552949', 'coord-merge-g-q48.txt',       'G Q48: one header for both runtime trace_impl.cs copies -- the L3 merge refusal at master cleared, comment lines only'),
    ('C1RT3', 'claude/c1-runtime-inc3-sliceheader', 'c3279299b',          'coord-merge-c1-rt3.txt',      'C1 RT3: runtime increment 3, option A slice half -- SliceHeaderBox adapter in Reinterpret, golib only'),
    ('C2Q44', 'claude/c2-q44-cut',                  '',          'coord-merge-c2-q44-cut.txt',  'C2 Q44: the managed pointer token for reference-bearing boxes + the syncTimer displacement + the registry-growth arm'),
    ('C2Q49', 'claude/c2-q49-cut',                  '',          'coord-merge-c2-q49.txt',      'C2 Q49: the KeepAlive predicate widened -- the bridged-wrapper arm + the darwin funnel arm + the census guard darwin arm'),
    ('RD',    'claude/reflect-cargo-inc-d',         'a7b3e4a6a',          'coord-merge-r-inc-d.txt',     'R D: the channel-value cargo (direction chain + element dims) on the header, the value route through the bridge, TestChanOf/TestTypes'),
    ('SUBQ43','claude/sub-q43',                     '5b58d49ea',          'coord-merge-sub-q43.txt',     'SUB-Q43: runtime/pprof host-killer gated by capability disclosure + the 180-row census by door'),
    ('SUBDOC10','claude/sub-doc10',                 '674982db9', 'coord-merge-sub-doc10.txt',   'SUB-DOC10: DOCTRINE BATCH 10 (accumulator items 448-492) landed in CLAUDE.md, +345/-0 pure inserts, docs only'),
    ('SUBQ45','claude/sub-q45',                    '49ccad282', 'coord-merge-sub-q45.txt',     'SUB-Q45: DESIGN-runtime-pinner.md -- the Pinner over the CLR heap sized (pin count by referent, +0 B, no token write), 19 rows classified, prediction 17/1/1; docs only'),
    ('C2CEN25','claude/c2-darwin-board-t25',        '3752b7495', 'coord-merge-c2-census25.txt',  'C2: the train-25 darwin census scored on both legs (increment 5 moved x64 from sigprocmask to setsig FuncPCABI0(sigtramp)); board +64, docs only'),
]
def rd(p): return io.open(p, encoding='utf-8', newline='').read()
def wr(p, t): io.open(p, 'w', encoding='utf-8', newline='').write(t)
def strip_seat_vars(t):
    return NL.join(l for l in t.split(NL) if not re.match(r'^[A-Z0-9]+_SHA="\$\{[A-Z0-9]+_SHA:-', l))
# --- assemble + rehearse
for src, dst in [('coord-train26-assemble.sh', 'coord-train27-assemble.sh'), ('coord-train26-rehearse.sh', 'coord-train27-rehearse.sh')]:
    t = rd(SP + src).replace('train26', 'train27').replace('TRAIN 26', 'TRAIN 27').replace('coord-t26-resolutions', 'coord-t27-resolutions')
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
t = rd(SP + 'coord-train26-land.sh').replace('train26', 'train27').replace('TRAIN 26', 'TRAIN 27')
t = re.sub(r'^for b in .*$', 'for b in ' + ' '.join('${%s_SHA:+${%s_BRANCH:-%s}}' % (v, v, b) for v, b, s, m, lab in SEATS) + '; do', t, count=1, flags=re.M)
wr(SP + 'coord-train27-land.sh', t)
# --- land-launch: replace the env assignment lines
t = rd(SP + 'coord-train26-land-launch.sh').replace('train26', 'train27').replace('TRAIN 26', 'TRAIN 27')
lines = t.split(NL); env_idx = [i for i, l in enumerate(lines) if re.match(r'^[A-Z0-9]+_SHA="\$\{[A-Z0-9]+_SHA:-', l)]
assert env_idx
first = env_idx[0]
newenv = ['%s_SHA="${%s_SHA:-%s}" %s_BRANCH="${%s_BRANCH:-%s}" %s' % (v, v, s, v, v, b, BS) for v, b, s, m, lab in SEATS]
lines = [l for i, l in enumerate(lines) if i not in env_idx]
lines[first:first] = newenv
wr(SP + 'coord-train27-land-launch.sh', NL.join(lines))
os.makedirs(SP + 'coord-t27-resolutions', exist_ok=True)
print('derived train-27 scripts:', [f for f in os.listdir(SP) if f.startswith('coord-train27-')])
