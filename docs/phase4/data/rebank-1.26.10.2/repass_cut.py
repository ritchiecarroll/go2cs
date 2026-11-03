# Cuts the 1.26.10.2 RE-PASS deal for the new freeze tip from the MEASURED walls of the discovery pass at f6666d98a2
# (PLAN-release-1-26-10-2.md "The re-pass at the new freeze tip"; COORD's constraints of 2026-11-03), deterministically.
# Usage: py -3 -I repass_cut.py <worktree> <out.tsv> [A|B|C] [--p2-to-rwsl N [--p2-recorded K]]
# Inputs (all read from the worktree):
#   docs/phase4/data/rebank-1.26.10.2/discovery-walls.tsv   the discovery walls, each with its source ref@sha
#   docs/phase4/data/rebank-go1.26.10/sweep-shards.tsv       the windows rows and their pass-1 walls (the cost shape)
#   docs/phase4/data/rebank-1.26.10.2/legs.tsv               the linux and darwin rows and their 1.26.10.1 walls
#   docs/phase4/data/rebank-1.26.10.2/build-weight.tsv       each linux row's build weight (build_weight.py), read
#                                                           only by --p2-to-rwsl
#   src/run-validated-sweep.ps1                             $longTimeouts, the reserved set
# The P2 REBALANCE (COORD, 2026-11-03: P2's purge-rate trigger, more than 1 purge in 4 over its first 12 rows, stops
# P2, and COORD moves its heavy-build rows to R-WSL). --p2-to-rwsl N moves P2's N heaviest unrecorded rows, by build
# weight (then predicted wall, then name), to a new shard RWSL-3 that R-WSL runs after its own legs (ruling 1 on the
# re-pass briefs), each predicted at its P2 wall x factor(R-WSL)/factor(P2). --p2-recorded K holds P2-1's first K rows
# in seq order (the rows P2 has already recorded) where they are. The windows deal is cut first and never moves.
# Without --p2-to-rwsl the output is the deal as ruled.
# The cost model, stated:
#   - A windows box's FACTOR is its discovery walls over its own rows' pass-1 walls. A row's measured wall on the box
#     that read it becomes a box-free cost (wall / that box's factor), and its prediction on lane L is cost x factor(L).
#     A row predicted on the box that measured it is exactly its measured wall.
#   - Linux has no free choice under COORD's rules, so it needs no cross-class estimate. P1 carries no leg, and its
#     rows move to P2, the other cloud container, as wall x factor(P2) / factor(P1). internal/runtime/gc/scan and the
#     reserved rows go to R-WSL. A row with no linux reading at f6666d98a2 (RWSL-2 had not posted) is predicted at
#     its 1.26.10.1 wall and labelled so: that is a reading at the OLD tip, not a new one.
# Placement rules:
#   - windows: net on G (NXDOMAIN); the owner's off-i9 rows on R or G (the boxes whose network banked them; H is not
#     proven for them); the other reserved rows on the i9, which runs its shard alone; everything else is dealt
#     longest-first to the lane that finishes earliest, where R-LAPTOP's finish is its windows leg PLUS its whole
#     linux work (one battery per box, windows first);
#   - linux: as above. darwin: C1's dispatches, unchanged;
#   - within a list: smallest-first, `testing` last.
import csv, os, re, sys
from collections import defaultdict

tree, out = sys.argv[1], sys.argv[2]
# R-LAPTOP's windows share (COORD rules between them): A = all eight off-i9 rows on R, as at 1.26.10.1;
# B = crypto/tls on R (its count depends on the host's BoGo runner, proven on R only), the other seven dealt
# between R and G; C = all eight dealt between R and G.
# D (COORD, 2026-11-03: R-LAPTOP intermittent for about two days) = R-LAPTOP carries NOTHING. crypto/tls goes to G
# windows, with net (G's NXDOMAIN placement); R-WSL's linux rows (gc/scan included: G-LAPTOP has no AVX-512) go to
# G-LAPTOP's WSL, which runs them AFTER G's windows leg (one battery per box; the host-disk rule); every other windows
# row is dealt onto the i9 and H, the off-i9 rows to H only (net/http never to the i9). G-WSL has never been read at 1.26, so its walls are an
# ESTIMATE, said in every basis: R-WSL's wall x the windows G/R box ratio.
args = sys.argv[3:]
MODE = args.pop(0) if args and not args[0].startswith('--') else 'B'
assert MODE in ('A', 'B', 'C', 'D'), MODE
HOST, WSL, PFX = ('G', 'G-WSL', 'GWSL') if MODE == 'D' else ('R', 'R-WSL', 'RWSL')
opts = dict(zip(args[::2], args[1::2]))
assert len(args) % 2 == 0 and set(opts) <= {'--p2-to-rwsl', '--p2-recorded'}, f'unknown arguments {args}'
MOVE_N, RECORDED = int(opts.get('--p2-to-rwsl', 0)), int(opts.get('--p2-recorded', 0))
assert MOVE_N >= 0 and RECORDED >= 0 and (MOVE_N or not RECORDED), '--p2-recorded needs --p2-to-rwsl'
rd = lambda p: open(os.path.join(tree, p), encoding='utf-8', newline='')

disc = {}
for r in csv.DictReader(rd('docs/phase4/data/rebank-1.26.10.2/discovery-walls.tsv'), delimiter='\t'):
    disc[(r['os'], r['row'])] = (r['box'], int(r['wall_s']), r['source'])

win_rows, proj = [], {}
for r in csv.DictReader(rd('docs/phase4/data/rebank-go1.26.10/sweep-shards.tsv'), delimiter='\t'):
    win_rows.append(r['row']); proj[r['row']] = int(r['projected_s'])
legs = list(csv.DictReader(rd('docs/phase4/data/rebank-1.26.10.2/legs.tsv'), delimiter='\t'))
m = re.search(r"^\$longTimeouts = @\{(.*)\}\s*$", open(os.path.join(tree, 'src/run-validated-sweep.ps1'), encoding='utf-8').read(), re.M)
reserved = set(re.findall(r"'([^']+)' = '\d+m'", m.group(1)))
OFF_I9 = {'crypto/tls', 'net/http', 'net/http/httptest', 'net/smtp', 'crypto/x509', 'net/http/cgi',
          'net/http/httptrace', 'os/exec'}

# ---- windows
assert all(('windows', r) in disc for r in win_rows), 'a windows row has no discovery wall'
wf_num, wf_den = defaultdict(int), defaultdict(int)
for r in win_rows:
    box, wall, _ = disc[('windows', r)]
    wf_num[box] += wall; wf_den[box] += proj[r]
wfactor = {b: wf_num[b] / wf_den[b] for b in wf_num}
# A windows row G has read itself since the discovery pass, as a qualifying read for mode D. It is test-bound, so the
# box ratio mispredicts it (it put crypto/tls at 673 s on G); the measured wall stands instead.
QUALIFIED_ON_G = {'crypto/tls': (1322, "G's qualifying read at 25e3b6ede6, PASS 1393/1393, the runner's own 1322 s (COORD accepted, 2026-11-03)")}
def wpred(r, lane):
    box, wall, src = disc[('windows', r)]
    if MODE == 'D' and lane == 'G' and r in QUALIFIED_ON_G:
        q, why = QUALIFIED_ON_G[r]
        return q, f'{q} s measured on G ({why})'
    if lane == box:
        return wall, f'{wall} s measured on {box} ({src})'
    p = round(wall / wfactor[box] * wfactor[lane])
    return p, f'{wall} s measured on {box} ({src}) x {wfactor[lane]:.3f}/{wfactor[box]:.3f}'

# ---- linux (no free choice)
lrows = [r for r in legs if r['os'] == 'linux']
lproj = {r['row']: int(r['projected_s']) for r in lrows}
lf_num, lf_den = defaultdict(int), defaultdict(int)
for r in lrows:
    if ('linux', r['row']) in disc:
        box, wall, _ = disc[('linux', r['row'])]
        lf_num[box] += wall; lf_den[box] += lproj[r['row']]
lfactor = {b: lf_num[b] / lf_den[b] for b in lf_num}
# The WSL lane's scale: 1 on R-WSL, which measured these rows; in mode D, G-WSL's ESTIMATE.
wsl_scale = wfactor['G'] / wfactor['R'] if MODE == 'D' else 1
def wsl(item):
    n, p, b = item
    if MODE != 'D':
        return item
    return (n, round(p * wsl_scale), f'{b} x {wsl_scale:.3f} ESTIMATE (G-WSL unread at 1.26: the windows G/R box ratio)')
plan = defaultdict(list)          # (os, lane, shard) -> [(row, predicted_s, basis)]
for r in lrows:
    name, old = r['row'], r['shard']
    if ('linux', name) in disc:
        box, wall, src = disc[('linux', name)]
        if name == 'internal/runtime/gc/scan':
            plan[('linux', WSL, f'{PFX}-1')].append(wsl((name, wall, f'{wall} s measured on {box} ({src}); no WSL reading at f6666d98a2 (no AVX-512 on {WSL}: placed there by rule)')))
        elif box == 'R-WSL':
            assert old in ('RWSL-1', 'RWSL-2'), f'{name}: read on R-WSL but dealt to {old}'
            plan[('linux', WSL, PFX + old[4:])].append(wsl((name, wall, f'{wall} s measured on R-WSL ({src})')))
        elif box == 'P2':
            plan[('linux', 'P2', old)].append((name, wall, f'{wall} s measured on P2 ({src})'))
        else:
            p = round(wall / lfactor['P1'] * lfactor['P2'])
            plan[('linux', 'P2', None)].append((name, p, f'{wall} s measured on P1 ({src}) x {lfactor["P2"]:.3f}/{lfactor["P1"]:.3f}'))
    else:
        plan[('linux', WSL, f'{PFX}-2')].append(wsl((name, lproj[name], f'NO linux reading at f6666d98a2 (RWSL-2 not posted); 1.26.10.1 wall {lproj[name]} s ({r["wall_source"]})')))
# P1's moved rows join P2's two shards, longest-first onto the lighter one.
moved = sorted(plan.pop(('linux', 'P2', None)), key=lambda t: (-t[1], t[0]))
for item in moved:
    k = min(('P2-1', 'P2-2'), key=lambda s: (sum(p for _, p, _ in plan[('linux', 'P2', s)]), s))
    plan[('linux', 'P2', k)].append(item)
host_linux = sum(p for (o, l, _), items in plan.items() if l == WSL for _, p, _ in items)

# ---- windows deal
lanes = ('i9', 'H', 'G') if MODE == 'D' else ('i9', 'H', 'G', 'R')
load = {l: 0 for l in lanes}
offset = {HOST: host_linux}             # the WSL box runs its linux work after its windows shard
wins = defaultdict(list)
def put(r, lane):
    p, basis = wpred(r, lane)
    wins[lane].append((r, p, basis)); load[lane] += p
for r in win_rows:
    if r == 'net':
        put(r, 'G')
    elif r in reserved and r not in OFF_I9:
        put(r, 'i9')
    elif r in OFF_I9 and (MODE == 'A' or (MODE == 'B' and r == 'crypto/tls')):
        put(r, 'R')
    elif MODE == 'D' and r == 'crypto/tls':
        put(r, 'G')
movable = [r for r in win_rows if not any(r == n for n, _, _ in wins['G'] + wins['i9'] + wins['R'])]
cost = lambda r: disc[('windows', r)][1] / wfactor[disc[('windows', r)][0]]
for r in sorted(movable, key=lambda r: (-cost(r), r)):
    # D: G keeps only net and crypto/tls, so the G-WSL estimate never decides a windows row's lane.
    allowed = (('H',) if MODE == 'D' else ('R', 'G')) if r in OFF_I9 else (('i9', 'H') if MODE == 'D' else lanes)
    best = min(allowed, key=lambda l: (load[l] + wpred(r, l)[0] + offset.get(l, 0), l))
    put(r, best)
for l in lanes:
    plan[('windows', l, f'{l}-S')] = wins[l]

# ---- the P2 rebalance (only with --p2-to-rwsl)
order = lambda items: sorted(items, key=lambda t: (t[0] == 'testing', t[1], t[0]))
moved_rows = []
if MOVE_N:
    bw = {r['row']: int(r['build_weight']) for r in csv.DictReader(rd('docs/phase4/data/rebank-1.26.10.2/build-weight.tsv'), delimiter='\t')}
    recorded = {n for n, _, _ in order(plan[('linux', 'P2', 'P2-1')])[:RECORDED]}
    assert len(recorded) == RECORDED, f'--p2-recorded {RECORDED} exceeds P2-1'
    cands = [(s, item) for s in ('P2-1', 'P2-2') for item in plan[('linux', 'P2', s)] if item[0] not in recorded]
    cands.sort(key=lambda c: (-bw[c[1][0]], -c[1][1], c[1][0]))
    assert MOVE_N <= len(cands), f'--p2-to-rwsl {MOVE_N} exceeds the {len(cands)} unrecorded P2 rows'
    for s, (n, p, b) in cands[:MOVE_N]:
        plan[('linux', 'P2', s)].remove((n, p, b))
        q = round(p / lfactor['P2'] * lfactor['R-WSL'])
        plan[('linux', WSL, f'{PFX}-3')].append(wsl((n, q, f'{b}; moved from {s} by --p2-to-rwsl (build weight {bw[n]}) x {lfactor["R-WSL"]:.3f}/{lfactor["P2"]:.3f}')))
        moved_rows.append(n)
    host_linux += sum(p for _, p, _ in plan[('linux', WSL, f'{PFX}-3')])

# ---- darwin, unchanged
for r in legs:
    if r['os'] == 'darwin':
        plan[('darwin', r['lane'], r['shard'])].append((r['row'], int(r['projected_s']), f'unchanged; 1.26.10.1 wall ({r["wall_source"]}); C1\'s discovery TSVs carry no wall'))

# ---- checks and output
for goos, want in (('windows', set(win_rows)), ('linux', {r['row'] for r in lrows})):
    got = [n for (o, _, _), items in plan.items() if o == goos for n, _, _ in items]
    assert len(got) == len(set(got)) and set(got) == want, f'{goos}: {len(got)} dealt vs {len(want)}'
with open(out, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('os\tlane\tshard\tseq\trow\tpredicted_s\tbasis\n')
    for key in sorted(plan, key=lambda k: (['windows', 'linux', 'darwin'].index(k[0]), k[1], int(re.sub(r'\D', '', k[2]) or 0), k[2])):
        for i, (n, p, b) in enumerate(order(plan[key]), 1):
            fh.write(f'{key[0]}\t{key[1]}\t{key[2]}\t{i}\t{n}\t{p}\t{b}\n')
tot = defaultdict(int); rows = defaultdict(int)
for (o, l, s), items in plan.items():
    tot[(o, l, s)] = sum(p for _, p, _ in items); rows[(o, l, s)] = len(items)
print('windows factors: ' + ', '.join(f'{b} {f:.3f}' for b, f in sorted(wfactor.items())), file=sys.stderr)
print('linux factors:   ' + ', '.join(f'{b} {f:.3f}' for b, f in sorted(lfactor.items())), file=sys.stderr)
for k in sorted(tot):
    print(f'{k[0]:8} {k[1]:6} {k[2]:7} rows={rows[k]:3} predicted={tot[k]:6d} s ({tot[k] / 3600:.2f} h)', file=sys.stderr)
print(f'{HOST}-LAPTOP: windows {load[HOST]} s + linux {host_linux} s = {load[HOST] + host_linux} s', file=sys.stderr)
if MODE == 'D':
    print(f'G-WSL scale {wsl_scale:.3f} (ESTIMATE); unscaled, G-LAPTOP would be {load[HOST] + round(host_linux / wsl_scale)} s', file=sys.stderr)
if MOVE_N:
    print(f'--p2-to-rwsl {MOVE_N} --p2-recorded {RECORDED}: moved to RWSL-3: {", ".join(moved_rows)}', file=sys.stderr)
