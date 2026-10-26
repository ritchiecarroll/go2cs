# Cuts the go1.26.10 re-bank's per-lane shard lists from the population of record, deterministically.
# Usage: py -3 -I shards.py <worktree> <out.tsv>
# Inputs (all read from the worktree at the hop tip):
#   docs/phase4/data/population-go1.26.10.txt      the population (242)
#   src/run-validated-sweep.ps1                    $longTimeouts, the DERIVED reserved set
#   docs/phase4/hopA-inputs/recon-basis.tsv        windows sweep_s at go1.24.13 (ordering only: a projection)
#   docs/phase4/DATA-sweep-row-walltimes.md        the linux section (R-LAPTOP WSL, go1.23-era; ordering only)
#   eligible-linux.txt / eligible-darwin.txt       per-GOOS test-file eligibility (go list, go1.26.10, corpus tags)
# Rules: every eligible row exactly once per OS; smallest-first within a lane; `testing` last in its list; the
# reserved set from the table, never typed; the owner's off-i9 list taken by name from COORD's message.
import re, sys, os, hashlib

tree, out = sys.argv[1], sys.argv[2]
here = os.path.dirname(os.path.abspath(__file__))

def rows(path):
    return [l.strip() for l in open(path, encoding='utf-8') if l.strip() and not l.startswith('#')]

pop = rows(os.path.join(tree, 'docs/phase4/data/population-go1.26.10.txt'))
assert len(pop) == len(set(pop)) == 242, len(pop)

sweep = open(os.path.join(tree, 'src/run-validated-sweep.ps1'), encoding='utf-8').read()
m = re.search(r"^\$longTimeouts = @\{(.*)\}\s*$", sweep, re.M)
reserved = set(re.findall(r"'([^']+)' = '\d+m'", m.group(1)))
assert len(reserved) >= 5, reserved                       # a derivation reading fewer than five refuses
assert reserved <= set(pop), reserved - set(pop)

off_i9 = {'crypto/tls', 'net/http', 'net/http/httptest', 'net/smtp', 'crypto/x509', 'net/http/cgi',
          'net/http/httptrace', 'os/exec'}                 # the owner's list, by name (COORD 2026-10-24)
assert off_i9 <= set(pop), off_i9 - set(pop)
# COORD's ruling Q5 (2026-10-24): the list is about the i9's BOX, not the rows. So R-LAPTOP's windows shard keeps only
# the off-i9 rows that are RESERVED, and the other off-i9 rows deal into H like any non-reserved row.
rlap_rows = reserved & off_i9

# Windows costs at go1.24.13 (recon basis) and linux costs (go1.23-era WSL section). Ordering only.
wcost = {}
for i, l in enumerate(open(os.path.join(tree, 'docs/phase4/hopA-inputs/recon-basis.tsv'), encoding='utf-8')):
    f = l.rstrip('\n').split('\t')
    if i == 0:
        col = {n: k for k, n in enumerate(f)}
        continue
    try:
        wcost[f[col['row']]] = int(f[col['sweep_s']])
    except ValueError:
        pass
lcost = {}
md = open(os.path.join(tree, 'docs/phase4/DATA-sweep-row-walltimes.md'), encoding='utf-8').read()
sect = md.split('## linux ·', 1)[1].split('## digests', 1)[0]
for l in sect.splitlines():
    mm = re.match(r'^(\S+)\s+\S+\s+(?:\d+\s+)?(\d+)s\s*$', l.strip())
    if mm:
        lcost[mm.group(1)] = int(mm.group(2))

UNKNOWN = 10**6                                            # no measured cost: sorted after every costed row

def order(names, cost):
    # smallest-first by the projection; a row with no cost goes after every costed row, alphabetically; testing last
    k = lambda r: (r == 'testing', cost.get(r, UNKNOWN), r)
    return sorted(names, key=k)

out_rows = []   # (os, lane, shard, seq, row, projected_s)

def emit(osname, lane, shard_lists, cost):
    for s, names in enumerate(shard_lists, 1):
        for q, r in enumerate(names, 1):
            out_rows.append((osname, lane, f'{lane}-{s}', q, r, cost.get(r, '')))

def split(names, n):
    # n contiguous shards of near-equal ROW count over a smallest-first list (the measured H rate is per row)
    size = -(-len(names) // n)
    return [names[i:i + size] for i in range(0, len(names), size)]

# ---- windows: every population row (the recon leg is the membership authority)
win = set(pop)
i9 = order(sorted((reserved - off_i9) & win), wcost)
rlap = order(sorted(rlap_rows & win), wcost)
h = order(sorted(win - reserved), wcost)
emit('windows', 'i9', [i9], wcost)
emit('windows', 'RLAP', [rlap], wcost)
emit('windows', 'H', split(h, 3), wcost)

# ---- linux: every linux-eligible row; the RESERVED set to R's WSL (Q2: no cloud OOM risk on it), the rest to P1/P2
# dealt round-robin. The off-i9 list says nothing about linux (Q5), so it plays no part here.
lin = set(rows(os.path.join(here, 'eligible-linux.txt')))
assert lin <= set(pop)
heavy = order(sorted(reserved & lin), lcost if lcost else wcost)
rest = order(sorted(lin - reserved), {r: lcost.get(r, wcost.get(r, UNKNOWN)) for r in lin})
p1 = [r for i, r in enumerate(rest) if i % 2 == 0]
p2 = [r for i, r in enumerate(rest) if i % 2 == 1]
emit('linux', 'RWSL', [heavy], lcost)
emit('linux', 'P1', split(p1, 3), lcost)
emit('linux', 'P2', split(p2, 3), lcost)

# ---- darwin: every darwin-eligible row, in hosted waves of at most 20, alphabetical
dar = sorted(set(rows(os.path.join(here, 'eligible-darwin.txt'))))
assert set(dar) <= set(pop)
waves = [dar[i:i + 20] for i in range(0, len(dar), 20)]
waves = [sorted(w, key=lambda r: (r == 'testing', r)) for w in waves]   # testing last in its list here too
emit('darwin', 'C1', waves, {})

# ---- the identities: every eligible row exactly once per OS
for osname, want in (('windows', win), ('linux', lin), ('darwin', set(dar))):
    got = [r[4] for r in out_rows if r[0] == osname]
    assert len(got) == len(set(got)) == len(want) and set(got) == want, osname
for osname in ('windows', 'linux', 'darwin'):
    shard = [r for r in out_rows if r[0] == osname and r[4] == 'testing'][0][2]
    assert [r for r in out_rows if r[2] == shard][-1][4] == 'testing', osname   # testing is LAST in its list

body = 'os\tlane\tshard\tseq\trow\tprojected_s\n' + ''.join('\t'.join(map(str, r)) + '\n' for r in out_rows)
with open(out, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write(body)

print('reserved (derived):', ' '.join(sorted(reserved)))
for osname in ('windows', 'linux', 'darwin'):
    lanes = {}
    for r in out_rows:
        if r[0] == osname:
            lanes.setdefault(r[2], []).append(r)
    tot = sum(len(v) for v in lanes.values())
    print(f'{osname}: {tot} rows')
    for sh, v in lanes.items():
        proj = sum(int(x[5]) for x in v if x[5] != '')
        unk = sum(1 for x in v if x[5] == '')
        print(f'  {sh:7} {len(v):4} rows  projected {proj:6} s over the costed rows, {unk} uncosted  first={v[0][4]} last={v[-1][4]}')
print('sha256', hashlib.sha256(body.encode('utf-8')).hexdigest())
