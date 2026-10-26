# Cuts the go1.26.10 re-bank's FINAL RE-READ PASS (PLAN-rebank-go126.md section 13) from the batch-43 seats'
# footprints and this hop's own measured walls, deterministically.
# Usage: py -3 -I reread.py <worktree> <out.tsv>
# Inputs (all read from the worktree):
#   docs/phase4/data/population-go1.26.10.txt          the population (242): every row named here must be in it
#   docs/phase4/data/rebank-go1.26.10/eligible-*.txt  per-GOOS eligibility: a row is read only where it is eligible
#   src/run-validated-sweep.ps1                         $longTimeouts, the DERIVED reserved set
#   docs/phase4/h10-evidence/go1.26.10/{windows,linux}  pass-1 wall_s per row and lane: the costs (a projection)
# Output: the shards.tsv columns (os, lane, shard, seq, row, projected_s), so a lane reads its list the same way.
# Rules:
#   - WINDOWS + LINUX read SET_WL (below), every row once per OS.
#   - windows: the reserved rows off the owner's off-i9 list go to i9, the reserved rows on it to RLAP, and the rest
#     are dealt between H and G longest-first onto the lane that finishes first.
#   - linux: runtime, crypto/tls and net/http stay on RWSL (COORD Q2: R-LAPTOP is the proven box for runtime and the TLS rows,
#     with no cloud OOM risk), and the rest are dealt between P1 and P2 the same way, on each lane's own cost.
#   - darwin: SET_DARWIN on one lane, C1, in COORD's filters (the brief); costs are not measured here, so 0.
#   - within a lane: smallest-first, and `testing` last (as shards.py).
import csv, os, re, sys

tree, out = sys.argv[1], sys.argv[2]
D = 'docs/phase4/data/rebank-go1.26.10'

def rows(path):
    return [l.strip() for l in open(os.path.join(tree, path), encoding='utf-8') if l.strip() and not l.startswith('#')]

# The windows + linux set: every row whose test or production emission moved in batches 42-43, read from each seat's
# committed src/core diff against its merge base with 996a609534 (the brief lists the command), plus `log`.
TAILCALL = ['bufio', 'crypto/cipher', 'crypto/internal/fips140/edwards25519', 'crypto/rsa', 'crypto/tls', 'database/sql',
            'go/build', 'go/token', 'internal/coverage/slicewriter', 'internal/trace', 'io/fs', 'iter', 'log/slog',
            'net/http', 'os', 'path/filepath', 'runtime', 'runtime/debug', 'testing', 'time']   # fad9217ba0: 20 packages
H_PROD = ['math/big', 'go/constant', 'encoding/asn1', 'runtime']                             # h-named-shift-count, h-range-nil-ptrarray
H_TEST = ['crypto/internal/fips140/edwards25519/field', 'time', 'image/jpeg', 'runtime']
LEVER_A = ['log']        # lever A changes every generated NoInlining overload; log's Lshortfile is runtime.Caller's
SET_WL = sorted(set(TAILCALL + H_PROD + H_TEST + LEVER_A))
assert len(SET_WL) == 26, len(SET_WL)

# The darwin set (the brief gives each row's reason, and the rows judged to need NO darwin re-read).
SET_DARWIN = sorted(['net', 'net/http/pprof', 'net/http/cgi', 'os', 'os/exec', 'math/big', 'sync/atomic', 'log/slog',
                     'testing', 'runtime', 'runtime/pprof', 'syscall'])
assert len(SET_DARWIN) == 12

pop = set(rows('docs/phase4/data/population-go1.26.10.txt'))
assert len(pop) == 242, len(pop)
assert set(SET_WL) <= pop and set(SET_DARWIN) <= pop, (set(SET_WL) | set(SET_DARWIN)) - pop
elig = {o: set(rows(f'{D}/eligible-{o}.txt')) for o in ('windows', 'linux', 'darwin')}
for o in ('windows', 'linux'):
    assert set(SET_WL) <= elig[o], (o, set(SET_WL) - elig[o])
assert set(SET_DARWIN) <= elig['darwin'], set(SET_DARWIN) - elig['darwin']

sweep = open(os.path.join(tree, 'src/run-validated-sweep.ps1'), encoding='utf-8').read()
m = re.search(r"^\$longTimeouts = @\{(.*)\}\s*$", sweep, re.M)
reserved = set(re.findall(r"'([^']+)' = '\d+m'", m.group(1)))
assert len(reserved) >= 5, reserved
off_i9 = {'crypto/tls', 'net/http', 'net/http/httptest', 'net/smtp', 'crypto/x509', 'net/http/cgi',
          'net/http/httptrace', 'os/exec'}              # the owner's list, by name (COORD 2026-10-24)

# Pass-1 walls by (os, lane-prefix, row). A lane prefix is the shard name before its '-' (H-2 -> H, P1-L2 -> P1).
walls = {}
ev = os.path.join(tree, 'docs/phase4/h10-evidence/go1.26.10')
for goos in ('windows', 'linux'):
    for f in sorted(os.listdir(os.path.join(ev, goos))):
        if not f.endswith('.tsv') or '.ledger.' in f or '.untracked.' in f:
            continue
        lane = f.split('-')[0]
        with open(os.path.join(ev, goos, f), encoding='utf-8') as fh:
            for r in csv.DictReader(fh, delimiter='\t'):
                if r.get('wall_s', '').isdigit():
                    walls.setdefault((goos, lane, r['row']), []).append(int(r['wall_s']))

def measured(goos, lane, row):
    w = walls.get((goos, lane, row))
    return min(w) if w else None                         # an L2 re-read pays a fresh build; the pass-1 reading is the row

def cost(goos, lane, row):
    if goos == 'windows':                                # G has no pass-1 lane: it is costed as H (measured 2026-10-25:
        for l in (lane, 'H', 'i9', 'RLAP'):              # log 47 s here vs 68, testing 62 vs 86, math/big 80 vs 120)
            c = measured('windows', 'H' if l == 'G' else l, row)
            if c is not None:
                return c
    else:
        p1, p2, rw = (measured('linux', l, row) for l in ('P1', 'P2', 'RWSL'))
        base = p1 if p1 is not None else (max(25, p2 - 240) if p2 is not None else rw)
        if lane == 'P2':                                 # P2 pays a ~260 s floor per row (pass-1 min 260-269 s on all 3 shards)
            return p2 if p2 is not None else base + 240
        if lane == 'RWSL' and rw is not None:
            return rw
        return base
    raise SystemExit(f'no measured wall for {goos} {row}')

def deal(goos, lanes, todo):
    load = {l: 0 for l in lanes}
    got = {l: [] for l in lanes}
    for row in sorted(todo, key=lambda r: (-max(cost(goos, l, r) for l in lanes), r)):
        best = min(lanes, key=lambda l: (load[l] + cost(goos, l, row), l))
        load[best] += cost(goos, best, row)
        got[best].append(row)
    return got

RWSL_ROWS = ('runtime', 'crypto/tls', 'net/http')  # COORD Q2: R-LAPTOP, the proven box for runtime and the TLS-and-HTTP rows
plan = {}
win = list(SET_WL)
plan[('windows', 'i9')] = [r for r in win if r in reserved and r not in off_i9]
plan[('windows', 'RLAP')] = [r for r in win if r in reserved and r in off_i9]
rest = [r for r in win if r not in reserved]
for l, rs in deal('windows', ['H', 'G'], rest).items():
    plan[('windows', l)] = rs
plan[('linux', 'RWSL')] = [r for r in SET_WL if r in RWSL_ROWS]
for l, rs in deal('linux', ['P1', 'P2'], [r for r in SET_WL if r not in RWSL_ROWS]).items():
    plan[('linux', l)] = rs
plan[('darwin', 'C1')] = list(SET_DARWIN)

for goos, want in (('windows', SET_WL), ('linux', SET_WL), ('darwin', SET_DARWIN)):
    got = [r for (o, _), rs in plan.items() if o == goos for r in rs]
    assert sorted(got) == sorted(want), (goos, set(want) ^ set(got))   # every row exactly once per OS

with open(out, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('os\tlane\tshard\tseq\trow\tprojected_s\n')
    for (goos, lane) in sorted(plan, key=lambda k: (['windows', 'linux', 'darwin'].index(k[0]), k[1])):
        rs = plan[(goos, lane)]
        c = {r: (cost(goos, lane, r) if goos != 'darwin' else 0) for r in rs}
        order = sorted(rs, key=lambda r: (r == 'testing', c[r], r))
        for i, r in enumerate(order, 1):
            fh.write(f'{goos}\t{lane}\t{lane}-R\t{i}\t{r}\t{c[r]}\n')
        print(f'{goos:8} {lane:5} rows={len(rs):2} projected={sum(c.values()):5d} s ({sum(c.values()) / 60:.0f} min)', file=sys.stderr)

# The WINDOWS lanes bank through run-h10-dispatch.ps1 -Mode rebank, as pass 2 did, so the same rows are also emitted in
# the driver's version-1 plan format (`reread.py <worktree> <out.tsv> <dispatch.tsv>`): W = 4 (pass 2's windows fleet),
# the worker names pass 2's W1 refs used (RLAP is R's windows), one slice per worker under the 2400 s cap, the digest
# over the LF-joined data lines exactly as the driver recomputes it.
if len(sys.argv) > 3:
    import hashlib
    WORKER = {'i9': 'i9', 'H': 'H', 'G': 'G', 'RLAP': 'R'}
    CAP, COOLDOWN = 2400, 600
    data = []
    for lane in ('G', 'H', 'RLAP', 'i9'):
        rs = plan[('windows', lane)]
        c = {r: cost('windows', lane, r) for r in rs}
        assert sum(c.values()) <= CAP, (lane, sum(c.values()))     # one slice per worker; split if this ever fires
        for i, r in enumerate(sorted(rs, key=lambda r: (r == 'testing', c[r], r)), 1):
            data.append(f'4\t{WORKER[lane]}\t1\t{i}\t{r}\t{c[r]}\t{int(r in reserved)}')
    digest = hashlib.sha256(''.join(l + '\n' for l in data).encode('utf-8')).hexdigest()
    with open(sys.argv[3], 'w', encoding='utf-8', newline='\n') as fh:
        fh.write('# go2cs H10 dispatch plan -- MACHINE-READABLE. Generated by reread.py (the final re-read pass, windows).\n')
        fh.write('# Do not hand-edit: the driver recomputes #digest over the rows and refuses a mismatch.\n')
        fh.write('#version\t1\n')
        fh.write('#basis\th10-evidence/go1.26.10 pass-1 wall_s (min per row), G costed as H\n')
        fh.write(f'#slice_cap_seconds\t{CAP}\n#cooldown_seconds\t{COOLDOWN}\n#rows\t{len(data)}\n#digest\t{digest}\n')
        fh.write('W\tworker\tslice\tseq\tpackage\tcost_seconds\treserved\n')
        for l in data:
            fh.write(l + '\n')
    print(f'dispatch plan: {len(data)} rows, digest {digest[:12]}', file=sys.stderr)
