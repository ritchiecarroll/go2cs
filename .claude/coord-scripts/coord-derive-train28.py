# coord-derive-train28.py -- derive the train-27 assemble/rehearse/land/land-launch scripts from train 26's with the seat block rewritten.
import io, re, os
NL = chr(10); BS = chr(92)
SP = 'C:/Projects/go2cs/.claude/coord-scripts/'
SEATS = [  # (var, branch, default sha, msgfile, label)
    ('C2Q44',  'claude/c2-q44-cut',                  '',          'coord-merge-c2-q44-cut.txt',  'C2 Q44: the managed pointer token for reference-bearing boxes + the syncTimer displacement + the registry-growth arm + SUB-Q27 labels re-entry'),
    ('C2Q49',  'claude/c2-q49-cut',                  '',          'coord-merge-c2-q49.txt',      'C2 Q49: the KeepAlive predicate widened -- the bridged-wrapper arm + the darwin funnel arm (third package) + the census guard darwin arm + C1 arm-7 twin'),
    ('C1RT4',  'claude/c1-runtime-inc4-getg',        '49ad67e32',          'coord-merge-c1-rt4.txt',      'C1 RT4: runtime increment 4, the managed getg (a g AND its m, ThreadStatic-cached) per DESIGN-managed-getg, stacked on RT3'),
    ('C1RT5', 'claude/c1-runtime-inc5-q54', '7f318ab29', 'coord-merge-c1-rt5.txt', 'C1 increment 5 = Q54 runtime lock abandoned on goroutine death'),
    ('SUBQ45C','claude/sub-q45',                     'a38e638fe',          'coord-merge-sub-q45-cut.txt', 'SUB-Q45 cut: runtime.Pinner over the CLR heap -- pinner_impl.cs, 3 registry additions with placeholders, manifest, Reference (second commit on the design branch)'),
    ('SUBQ50', 'claude/sub-q50',                     'e8fcb6703',          'coord-merge-sub-q50.txt',     'SUB-Q50: golib unsafe.String aliases its source bytes where Go aliases (the 4-line arm) + guard'),
    ('GFVC',  'claude/g-field-view-cut',            '',          'coord-merge-g-field-view-cut.txt', 'G: the field-view cache CUT (arm 3, SlottedStandardBox + per-T weak table + the seven-arm guard) -- branch name set at the announce'),
    ('RE2',    'claude/reflect-field-metadata-inc-e2', 'f5df84f49',          'coord-merge-r-e2.txt',        'R E2: the reflect field-metadata cluster (Anonymous, equal-depth ambiguity, flagRO through unexported embedded) -- 3 rows'),
    ('C2Q41F','claude/c2-q41-frames',              'eb2ffed3d', 'coord-merge-c2-q41-frames.txt', 'C2 Q41: the darwin crash-report stage grows a frames-and-registers block (workflow only) -- the arm64 SIGBUS at 0x4200000000 placed by frames'),
    ('GFVD',  'claude/g-field-view-design',         'c4bc47917', 'coord-merge-g-field-view-design.txt', 'G: DESIGN-field-view-cache.md -- the type-gated view slot (arm 3 of the seg-3 spike), the identity contract, the byte formula, the seven-arm guard; docs only'),
    ('C2INC6','claude/c2-darwin-inc6',              'cc16ab170',          'coord-merge-c2-inc6.txt',     'C2: darwin run-layer increment 6 -- sigaction over a blittable mirror (encode new / decode old), registry entry, linux contract guard; the arm64 mute death cleared if the stale-register write was the cause'),
    ('RE3',    'claude/reflect-value-singles-inc-e3', '10eecadb9', 'coord-merge-r-e3.txt',        'R E3: the reflect Value singles, one root per commit on one branch (root 1 SetCap hand-own; root 2 Bytes; ...) -- the branch tip at assembly'),
    ('SUBQ22', 'claude/sub-q22',                     '969cbaeae',          'coord-merge-sub-q22.txt',     'SUB-Q22: the elided & composite element with a NAMED array/slice/map pointee routed through the typed renderer (CS0144 at master) + guard; SHA at the announce'),
    ('C1Q46',  'claude/c1-q46-hostfatal',            'f99111123', 'coord-merge-c1-q46.txt',      'C1 Q46: TestPanicSystemstack as a host-fatal HANG member of the runtime manifest + the board line (the init-started goroutine vs the type-initializer lock); no code'),
    ('SUBQ55', 'claude/sub-q55',                     'b0c9d1d8c', 'coord-merge-sub-q55.txt',     'SUB-Q55: the -tests pipeline deadline kill takes the child PROCESS GROUP (unix Setpgid; Windows job object with kill-on-close), guard re-execs three deep with a heartbeat; converter-side only'),
    ('SUBQ57', 'claude/sub-q57', '', 'coord-merge-sub-q57.txt', 'SUB-Q57 named-array empty literal constructs its elements (layer A)'),
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
for src, dst in [('coord-train27-assemble.sh', 'coord-train28-assemble.sh'), ('coord-train27-rehearse.sh', 'coord-train28-rehearse.sh')]:
    t = rd(SP + src).replace('train27', 'train28').replace('TRAIN 27', 'TRAIN 28').replace('coord-t27-resolutions', 'coord-t28-resolutions')
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
t = rd(SP + 'coord-train27-land.sh').replace('train27', 'train28').replace('TRAIN 27', 'TRAIN 28')
t = re.sub(r'^for b in .*$', 'for b in ' + ' '.join('${%s_SHA:+${%s_BRANCH:-%s}}' % (v, v, b) for v, b, s, m, lab in SEATS) + '; do', t, count=1, flags=re.M)
wr(SP + 'coord-train28-land.sh', t)
# --- land-launch: replace the env assignment lines
t = rd(SP + 'coord-train27-land-launch.sh').replace('train27', 'train28').replace('TRAIN 27', 'TRAIN 28')
lines = t.split(NL); env_idx = [i for i, l in enumerate(lines) if re.match(r'^[A-Z0-9]+_SHA="\$\{[A-Z0-9]+_SHA:-', l)]
assert env_idx
first = env_idx[0]
newenv = ['%s_SHA="${%s_SHA:-%s}" %s_BRANCH="${%s_BRANCH:-%s}" %s' % (v, v, s, v, v, b, BS) for v, b, s, m, lab in SEATS]
lines = [l for i, l in enumerate(lines) if i not in env_idx]
lines[first:first] = newenv
wr(SP + 'coord-train28-land-launch.sh', NL.join(lines))
os.makedirs(SP + 'coord-t28-resolutions', exist_ok=True)
print('derived train-27 scripts:', [f for f in os.listdir(SP) if f.startswith('coord-train28-')])
add_worktree_guard('coord-train28-assemble.sh', 'musing-moser-d4552c')
add_worktree_guard('coord-train28-rehearse.sh', 'coord-t25-dryrun')
add_worktree_guard('coord-train28-land.sh', 'musing-moser-d4552c')
