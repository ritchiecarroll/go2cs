# The 1.26.11.1 re-bank deal (PLAN-rebank-1-26-11-1.md): which lane reads which row on each OS, and when each lane is
# projected to finish. It reads every input at a worktree, so a re-cut at a new tip is a re-run, not an edit.
#
# Usage:
#   python3 -I deal.py cut   <worktree> <roster.tsv> <legs.tsv> [R|C]   write the deal, print the shards and totals
#   python3 -I deal.py check <worktree> <roster.tsv> <legs.tsv>         re-check a deal against the tree; rc 1 on a miss
#
# <roster.tsv> is roster_dump.ps1's output: the roster read through src/_roster.ps1's Get-ValidatedRosterRows.
# Mode R (default): R-LAPTOP is taken off its other work for the re-bank: crypto/tls on windows, then R-WSL on linux.
# Mode C: R-LAPTOP reads crypto/tls on windows only; the linux leg is the three cloud lanes plus one G-WSL leg.
#
# Inputs read at the worktree, each the latest measured wall per row (COORD 2026-11-18):
#   windows      docs/phase4/rebank-1.26.10.3/sweep/{i9-S,H-S,GH-S,R-W}.tsv          the .3 re-bank, per box
#   linux amd64  docs/phase4/rebank-1.26.10.3/linux/*.tsv                              the .3 re-bank, WSL and cloud
#   linux arm64  docs/phase4/rebank-1.26.10.4/linux-arm64/evidence/**/*.tsv            the arm64 bank (hosted, both RIDs)
#   darwin       docs/phase4/rebank-1.26.10.3/darwin/**/*.tsv                          the .3 re-bank and its re-reads
#   reserved set src/run-validated-sweep.ps1 $longTimeouts
# Where one row has several readings in one class, the file added last (git) wins.
import csv, os, re, subprocess, sys
from collections import Counter, defaultdict

csv.field_size_limit(sys.maxsize)

# --- the cost model (each constant is cited in the plan) -----------------------------------------------------------
WIN_FACTOR = {'G': 0.655, 'i9': 0.751, 'H': 1.072, 'R': 1.551}   # .2 discovery/pass-1 box factors (lower is faster)
# Linux lane factors are DERIVED at run time (linux_factors): a lane's .3 walls over the hosted linux-x64 walls of the
# same rows, read beside the arm64 bank. P4 has no reading and takes the slowest cloud factor; G-WSL takes R-WSL's.
LANE_SETUP_LINUX = 900        # s: clone and converter build, once per linux lane (.2: about 15 minutes)
HOSTED_SETUP = {'linux': 120, 'darwin': 300}   # s per hosted job: linux measured (job start to sweep, plus upload),
                                               # darwin .2's figure
HOSTED_SLOTS = {'linux-arm64': 7, 'darwin': 5} # concurrent jobs: 7 arm64 jobs ran at once in wave 1; 5 mac jobs
SOLO_S = 1200                 # s: a row this long on any RID dispatches alone, so a re-read of it re-reads nothing else
SPAN_SLACK = 300              # s: a deal with fewer dispatches is preferred while it finishes within this of the best
ROW_CAP = 20                  # rows per hosted dispatch (the darwin leg's unit, runbook H10 amendment 2026-10-24, 5)
OFF_I9 = lambda r: r.startswith('crypto/') or r in ('net/http', 'net/smtp', 'os/exec') or r.startswith('net/http/')

def git_added(tree, path):
    out = subprocess.run(['git', '-C', tree, 'log', '--diff-filter=A', '--format=%ct', '--', path],
                         capture_output=True, text=True).stdout.split()
    return int(out[-1]) if out else 0

def tsv(path):
    with open(path, encoding='utf-8', newline='') as f:
        lines = [l for l in f if not l.startswith('#')]
    return list(csv.DictReader(lines, delimiter='\t'))

def files(tree, rel, pred=lambda p: True):
    out = []
    for root, _, names in os.walk(os.path.join(tree, rel)):
        for n in names:
            p = os.path.join(root, n)
            if n.endswith('.tsv') and pred(n):
                out.append(p)
    return sorted(out, key=lambda p: (git_added(tree, os.path.relpath(p, tree)), p))

def rel(tree, p):
    return os.path.relpath(p, tree).replace(os.sep, '/')

# --- inputs ---------------------------------------------------------------------------------------------------------
def read_inputs(tree, roster_path):
    roster = {}
    for line in open(roster_path, encoding='utf-8'):
        p = line.rstrip('\n').split('\t')
        if len(p) == 7:
            roster[p[0]] = p
    num = lambda c: re.match(r'^\d+\+\d+', c) is not None
    el = {
        'windows': set(roster),
        'linux': {r for r, p in roster.items() if num(p[4])},
        'linux-arm64': {r for r, p in roster.items() if num(p[4]) and re.search(r'\|linux-arm64=\d+\+\d+', p[4])},
        'darwin': {r for r, p in roster.items() if num(p[6])},
    }
    m = re.search(r"^\$longTimeouts = @\{(.*)\}\s*$", open(os.path.join(tree, 'src/run-validated-sweep.ps1'),
                  encoding='utf-8').read(), re.M)
    reserved = set(re.findall(r"'([^']+)' = '\d+m'", m.group(1)))

    walls = {'windows': {}, 'linux': {}, 'linux-arm64': {}, 'darwin': {}}
    sweep = 'docs/phase4/rebank-1.26.10.3/sweep'
    for shard, box in (('i9-S', 'i9'), ('H-S', 'H'), ('GH-S', 'hosted'), ('R-W', 'R')):
        p = os.path.join(tree, sweep, shard + '.tsv')
        for r in tsv(p):
            walls['windows'][(r['row'], box)] = (int(r['wall_s']), f'{rel(tree, p)}')
    for p in files(tree, 'docs/phase4/rebank-1.26.10.3/linux', lambda n: '.ledger.' not in n):
        b = os.path.basename(p)
        lane = 'R-WSL' if b.startswith('RWSL') else ('hosted' if b.startswith('C1') else b[:2])
        for r in tsv(p):
            walls['linux'][(r['row'], lane)] = (int(r['wall_s']), rel(tree, p))
    for p in files(tree, 'docs/phase4/rebank-1.26.10.4/linux-arm64/evidence'):
        for r in tsv(p):
            key = 'linux-arm64' if r['RID'] == 'linux-arm64' else 'linux'
            if key == 'linux-arm64':
                walls[key][r['row']] = (int(r['wall_s']), rel(tree, p))
            else:
                walls['linux'][(r['row'], 'hosted')] = (int(r['wall_s']), rel(tree, p))
    for p in files(tree, 'docs/phase4/rebank-1.26.10.3/darwin'):
        for r in tsv(p):
            if r.get('wall_s', '').isdigit():
                walls['darwin'][(r['row'], r['RID'])] = (int(r['wall_s']), rel(tree, p))
    return roster, el, reserved, walls

# --- per-OS cost ----------------------------------------------------------------------------------------------------
def win_cost(walls, row, box):
    if (row, box) in walls:
        w, src = walls[(row, box)]
        return w, f'{w} s measured on {box} ({src})'
    for src_box in ('i9', 'H', 'R'):
        if (row, src_box) in walls and box in WIN_FACTOR:
            w, src = walls[(row, src_box)]
            est = round(w * WIN_FACTOR[box] / WIN_FACTOR[src_box])
            return est, f'est: {w} s on {src_box} x {WIN_FACTOR[box]}/{WIN_FACTOR[src_box]} ({src})'
    raise SystemExit(f'windows: no wall for {row}')

def linux_factors(walls):
    f = {}
    for lane in ('R-WSL', 'P1', 'P2'):
        rows = [r for (r, l) in walls if l == lane and (r, 'hosted') in walls]
        f[lane] = round(sum(walls[(r, lane)][0] for r in rows) / sum(walls[(r, 'hosted')][0] for r in rows), 2)
    f['P4'] = max(f['P1'], f['P2'])
    f['G-WSL'] = f['R-WSL']
    return f

def linux_cost(walls, row, lane, factors):
    if (row, lane) in walls:
        w, src = walls[(row, lane)]
        return w, f'{w} s measured on {lane} ({src})'
    if (row, 'hosted') in walls:
        w, src = walls[(row, 'hosted')]
        e = round(w * factors[lane])
        return e, f'est: {w} s on the hosted linux-x64 runner x {factors[lane]}, the {lane} factor ({src})'
    raise SystemExit(f'linux: no wall for {row} on {lane}')

def darwin_cost(walls, row):
    got = {rid: walls.get((row, rid)) for rid in ('osx-x64', 'osx-arm64')}
    if all(got.values()):
        per = {rid: v[0] for rid, v in got.items()}
        src = sorted({v[1] for v in got.values()})
        return per, 'x64 {} s, arm64 {} s ({})'.format(per['osx-x64'], per['osx-arm64'], ', '.join(src))
    one = [(rid, v) for rid, v in got.items() if v]
    if one:
        rid, (w, src) = one[0]
        return {'osx-x64': w, 'osx-arm64': w}, f'est: both RIDs at {rid}\'s {w} s ({src})'
    raise SystemExit(f'darwin: no wall for {row}')

# --- dealing --------------------------------------------------------------------------------------------------------
def order(rows, cost):
    """Smallest-first, testing last (the deal's list rule since 1.26.10.2)."""
    rows = sorted(rows, key=lambda r: (r == 'testing', cost(r), r))
    return rows

def lpt(rows, lanes, cost, allowed, start):
    load = dict(start)
    got = defaultdict(list)
    for r in sorted(rows, key=lambda r: (-max(cost(r, l) for l in allowed(r)), r)):
        lane = min(allowed(r), key=lambda l: (load[l] + cost(r, l), lanes.index(l)))
        got[lane].append(r)
        load[lane] += cost(r, lane)
    return got, load

def pack_hosted(rows, jobcost, rids, slots, setup):
    """Bin-pack rows into N dispatches of at most ROW_CAP rows, longest-first, and pick the N whose simulated
    schedule (slots concurrent jobs, one job per RID per dispatch, longest dispatch first) finishes earliest."""
    cands = []
    solo = sorted((r for r in rows if max(jobcost(r, x) for x in rids) >= SOLO_S),
                  key=lambda r: (-max(jobcost(r, x) for x in rids), r))
    rest = [r for r in rows if r not in solo]
    lo = -(-len(rest) // ROW_CAP)
    for m in range(lo, lo + 25):
        n = len(solo) + m
        bins = [[r] for r in solo] + [[] for _ in range(m)]
        load = [max(jobcost(r, x) for x in rids) for r in solo] + [0] * m
        for r in sorted(rest, key=lambda r: (-max(jobcost(r, x) for x in rids), r)):
            i = min((i for i in range(len(solo), n) if len(bins[i]) < ROW_CAP), key=lambda i: (load[i], i))
            bins[i].append(r)
            load[i] += max(jobcost(r, x) for x in rids)
        jobs = sorted(((setup + sum(jobcost(r, x) for r in b), k, x) for k, b in enumerate(bins) for x in rids),
                      reverse=True)
        free = [0] * slots
        finish = defaultdict(int)
        for d, k, x in jobs:
            s = free.index(min(free))
            free[s] += d
            finish[k] = max(finish[k], free[s])
        cands.append((max(finish.values()), n, bins, finish))
    # Fewest dispatches within SPAN_SLACK of the earliest finish: each dispatch is a run someone reads.
    floor = min(c[0] for c in cands)
    span, n, bins, finish = min((c for c in cands if c[0] <= floor + SPAN_SLACK), key=lambda c: c[1])
    return span, bins, finish

def cut(tree, roster_path, out, mode):
    roster, el, reserved, walls = read_inputs(tree, roster_path)
    legs = []                          # (os, lane, shard, seq, row, projected_s, source)
    summary = []                       # (os, lane, shard, rows, start, projected incl. setup, finish)

    # windows -------------------------------------------------------------------------------------------------------
    W = walls['windows']
    pin = {'net': 'GH', 'crypto/tls': 'R', 'runtime': 'H', 'os': 'i9', 'archive/tar': 'i9'}
    def wcost(r, lane):
        if lane == 'GH':
            return W[(r, 'hosted')][0]
        return win_cost(W, r, lane)[0]
    def wallowed(r):
        if r in pin:
            return [pin[r]]
        # Lean on account B (owner's order): the i9 takes every row it may read, since even all of them finish far
        # inside the linux leg; H and G (account A, sweep shards only) share the off-list rows.
        return ['H', 'G'] if OFF_I9(r) else ['i9']
    lanes = ['i9', 'H', 'G', 'R', 'GH']
    got, load = lpt(sorted(el['windows']), lanes, wcost, wallowed, {l: 0 for l in lanes})
    finish_win = {}
    for lane in lanes:
        rows = order(got[lane], lambda r: wcost(r, lane))
        shard = {'GH': 'GH-S'}.get(lane, f'{lane}-S')
        for i, r in enumerate(rows, 1):
            src = (f'{W[(r, "hosted")][0]} s measured on the hosted windows runner ({W[(r, "hosted")][1]})'
                   if lane == 'GH' else win_cost(W, r, lane)[1])
            legs.append(('windows', lane, shard, i, r, wcost(r, lane), src))
        setup = HOSTED_SETUP['darwin'] if lane == 'GH' else 0
        finish_win[lane] = load[lane] + setup
        summary.append(('windows', lane, shard, len(rows), 0, load[lane] + setup, load[lane] + setup))

    # linux amd64 -----------------------------------------------------------------------------------------------------
    L = walls['linux']
    host_x64 = {'os'}                                  # COORD ruling 2026-11-10: os's linux count is the hosted runner's
    no_avx512 = {'internal/runtime/gc/scan'}           # an AVX-512 host reads 9261, the bank is the non-AVX-512 3095
    full_tls = {'crypto/tls'}                          # the full BoGo count needs a 16-core box (.3: R-WSL, PASS 4921)
    if mode == 'R':
        lanes = ['R-WSL', 'P1', 'P2', 'P4']
        start = {'R-WSL': finish_win['R'] + LANE_SETUP_LINUX, 'P1': LANE_SETUP_LINUX, 'P2': LANE_SETUP_LINUX,
                 'P4': LANE_SETUP_LINUX}
        fixed = {r: 'R-WSL' for r in (reserved & el['linux']) | no_avx512 | full_tls}
    else:
        lanes = ['G-WSL', 'P1', 'P2', 'P4']
        start = {'G-WSL': finish_win['G'] + LANE_SETUP_LINUX, 'P1': LANE_SETUP_LINUX, 'P2': LANE_SETUP_LINUX,
                 'P4': LANE_SETUP_LINUX}
        fixed = {r: 'G-WSL' for r in no_avx512 | full_tls}
    LF = linux_factors(L)
    print('linux lane factors (lane walls / hosted linux-x64 walls, same rows):', LF)
    lcost = lambda r, lane: linux_cost(L, r, lane, LF)[0]
    lallowed = lambda r: [fixed[r]] if r in fixed else [l for l in lanes if not l.endswith('WSL') or mode == 'R']
    rows = sorted(el['linux'] - host_x64)
    got, load = lpt(rows, lanes, lcost, lallowed, start)
    for lane in lanes:
        rows = order(got[lane], lambda r: lcost(r, lane))
        shard = 'RWSL-1' if lane == 'R-WSL' else ('GWSL-1' if lane == 'G-WSL' else f'{lane}-1')
        for i, r in enumerate(rows, 1):
            legs.append(('linux', lane, shard, i, r, lcost(r, lane), linux_cost(L, r, lane, LF)[1]))
        st = start[lane] - LANE_SETUP_LINUX
        summary.append(('linux', lane, shard, len(rows), st, load[lane] - st, load[lane]))
    for r in sorted(host_x64 & el['linux']):
        w, src = L[(r, 'hosted')]
        legs.append(('linux', 'hosted', 'HX-1', 1, r, w, f'{w} s measured on the hosted linux-x64 runner ({src})'))
        summary.append(('linux', 'hosted', 'HX-1', 1, 0, w + HOSTED_SETUP['linux'], w + HOSTED_SETUP['linux']))

    # linux arm64 and darwin: hosted dispatches ----------------------------------------------------------------------
    A = walls['linux-arm64']
    span, bins, finish = pack_hosted(sorted(el['linux-arm64']), lambda r, x: A[r][0], ['linux-arm64'],
                                     HOSTED_SLOTS['linux-arm64'], HOSTED_SETUP['linux'])
    for k, b in enumerate(bins, 1):
        rows = order(b, lambda r: A[r][0])
        for i, r in enumerate(rows, 1):
            legs.append(('linux-arm64', 'hosted', f'AR-{k}', i, r, A[r][0],
                         f'{A[r][0]} s measured on the hosted linux-arm64 runner ({A[r][1]})'))
        summary.append(('linux-arm64', 'hosted', f'AR-{k}', len(rows), 0,
                        HOSTED_SETUP['linux'] + sum(A[r][0] for r in rows), finish[k - 1]))

    D = walls['darwin']
    dc = lambda r, x: darwin_cost(D, r)[0][x]
    span, bins, finish = pack_hosted(sorted(el['darwin']), dc, ['osx-x64', 'osx-arm64'], HOSTED_SLOTS['darwin'],
                                     HOSTED_SETUP['darwin'])
    for k, b in enumerate(bins, 1):
        slow = lambda r: max(dc(r, 'osx-x64'), dc(r, 'osx-arm64'))
        rows = order(b, slow)
        for i, r in enumerate(rows, 1):
            legs.append(('darwin', 'hosted', f'DW-{k}', i, r, slow(r), darwin_cost(D, r)[1]))
        job = max(HOSTED_SETUP['darwin'] + sum(dc(r, x) for r in rows) for x in ('osx-x64', 'osx-arm64'))
        summary.append(('darwin', 'hosted', f'DW-{k}', len(rows), 0, job, finish[k - 1]))

    with open(out, 'w', encoding='utf-8', newline='\n') as f:
        f.write('os\tlane\tshard\tseq\trow\tprojected_s\twall_source\n')
        for leg in legs:
            f.write('\t'.join(str(x) for x in leg) + '\n')
    print(f'mode {mode}: windows {len(el["windows"])}, linux {len(el["linux"])}, linux-arm64 '
          f'{len(el["linux-arm64"])}, darwin {len(el["darwin"])}; reserved {len(reserved)}')
    print(f'{"os":12} {"lane":7} {"shard":7} {"rows":>4} {"start_h":>7} {"run_h":>6} {"finish_h":>8}')
    for o, lane, shard, n, st, run, fin in summary:
        print(f'{o:12} {lane:7} {shard:7} {n:4d} {st / 3600:7.2f} {run / 3600:6.2f} {fin / 3600:8.2f}')
    latest = defaultdict(int)
    for o, lane, shard, n, st, run, fin in summary:
        latest[o] = max(latest[o], fin)
    for o, fin in latest.items():
        print(f'LATEST {o:12} {fin / 3600:.2f} h')
    print(f'LATEST overall {max(latest.values()) / 3600:.2f} h')
    est = Counter(o for o, *_, src in legs if src.startswith('est:'))
    print('estimated lines per OS:', dict(est))

def check(tree, roster_path, legs_path):
    roster, el, reserved, walls = read_inputs(tree, roster_path)
    fails = []
    def ok(cond, what):
        print(('  ok    ' if cond else '  FAIL  ') + what)
        if not cond:
            fails.append(what)
    lists = defaultdict(list)
    for r in tsv(legs_path):
        lists[(r['os'], r['lane'], r['shard'])].append((int(r['seq']), r['row']))
    for o in ('windows', 'linux', 'linux-arm64', 'darwin'):
        got = Counter(row for (oo, _, _), items in lists.items() if oo == o for _, row in items)
        dup = sorted(r for r, n in got.items() if n > 1)
        ok(not dup, f'{o}: no row dealt twice {dup or ""}')
        ok(set(got) == el[o], f'{o}: dealt == eligible ({len(got)} vs {len(el[o])}); missing '
                              f'{sorted(el[o] - set(got))}, extra {sorted(set(got) - el[o])}')
    for key, items in sorted(lists.items()):
        ok([s for s, _ in items] == list(range(1, len(items) + 1)), f'{key[0]} {key[2]}: seq runs 1..{len(items)}')
        names = [r for _, r in sorted(items)]          # in seq order, so a renumbered list is read as dispatched
        if 'testing' in names:
            ok(names[-1] == 'testing', f'{key[0]} {key[2]}: testing is last')
        if key[1] == 'hosted' or key[2] == 'GH-S':
            ok(len(items) <= ROW_CAP, f'{key[0]} {key[2]}: at most {ROW_CAP} rows ({len(items)})')
    lane_rows = lambda o, lane: {r for (oo, l, _), items in lists.items() if oo == o and l == lane for _, r in items}
    i9 = lane_rows('windows', 'i9')
    ok(not {r for r in i9 if OFF_I9(r)}, f'windows: no off-list row on the i9 {sorted(r for r in i9 if OFF_I9(r))}')
    ok(not ({'runtime', 'net'} & i9), 'windows: runtime and net are off the i9')
    for row, lane in (('net', 'GH'), ('crypto/tls', 'R'), ('runtime', 'H'), ('os', 'i9'), ('archive/tar', 'i9')):
        ok(row in lane_rows('windows', lane), f'windows: {row} on {lane}')
    ok('os' in lane_rows('linux', 'hosted'), 'linux: os on the hosted linux-x64 runner')
    wsl = lane_rows('linux', 'R-WSL') | lane_rows('linux', 'G-WSL')
    ok({'internal/runtime/gc/scan', 'crypto/tls'} <= wsl, 'linux: gc/scan and crypto/tls on a WSL lane')
    if lane_rows('linux', 'R-WSL'):
        ok(reserved & el['linux'] <= lane_rows('linux', 'R-WSL'), 'linux: the reserved set is on R-WSL (mode R)')
    print(f'{len(fails)} check(s) FAILED' if fails else 'ALL CHECKS PASS')
    return 1 if fails else 0

if __name__ == '__main__':
    if len(sys.argv) < 5 or sys.argv[1] not in ('cut', 'check'):
        raise SystemExit(__doc__ or 'usage: deal.py cut|check <worktree> <roster.tsv> <legs.tsv> [R|C]')
    if sys.argv[1] == 'cut':
        cut(sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5] if len(sys.argv) > 5 else 'R')
    else:
        sys.exit(check(sys.argv[2], sys.argv[3], sys.argv[4]))
