# Cuts the go1.26.10 hop's §6 FULL VALIDATED-ROSTER SWEEP into per-box shards (PLAN-rebank-go126.md §15, COORD's
# ruling of 2026-10-26: one complete pass at ONE tree, sharded across boxes), deterministically.
# Usage: py -3 -I sweep_shards.py <worktree> <out.tsv>
# Inputs (all read from the worktree):
#   docs/ValidatedTestPackages.md                   the BANKED rows: the table rows before the exclusion ledger
#   src/run-validated-sweep.ps1                     $longTimeouts, the DERIVED reserved set
#   docs/phase4/h10-evidence/go1.26.10/windows      pass-1 wall_s per row (min per row): the costs, a projection
# Rules:
#   - every banked row exactly once;
#   - the owner's off-i9 list goes to R (R-LAPTOP's windows), by name;
#   - `net` goes to G: windows net banks only on a host whose resolver answers NXDOMAIN for invalid.invalid
#     (PLAN §4 step 4), and the i9's network is SERVFAIL; G-LAPTOP banked it at pass 2;
#   - the other reserved rows go to the i9;
#   - everything else is dealt longest-first onto whichever of H, G and the i9 finishes first;
#   - within a box: smallest-first, `testing` last.
import csv, os, re, sys

tree, out = sys.argv[1], sys.argv[2]

table = open(os.path.join(tree, 'docs/ValidatedTestPackages.md'), encoding='utf-8').read()
ledger_at = table.find('\n## ', table.find('| Package |'))      # the table ends at the next heading
rows = re.findall(r'^\| \[`([^`]+)`\]\(', table[:ledger_at if ledger_at > 0 else len(table)], re.M)
assert len(rows) == len(set(rows)), 'a row appears twice'
assert len(rows) == 238, len(rows)                                # the roster guard's banked count at the spine

sweep = open(os.path.join(tree, 'src/run-validated-sweep.ps1'), encoding='utf-8').read()
m = re.search(r"^\$longTimeouts = @\{(.*)\}\s*$", sweep, re.M)
reserved = set(re.findall(r"'([^']+)' = '\d+m'", m.group(1)))
assert len(reserved) >= 5, reserved
OFF_I9 = {'crypto/tls', 'net/http', 'net/http/httptest', 'net/smtp', 'crypto/x509', 'net/http/cgi',
          'net/http/httptrace', 'os/exec'}                      # the owner's list, by name (COORD 2026-10-24)
NXDOMAIN = {'net'}
assert OFF_I9 <= set(rows) and NXDOMAIN <= set(rows)

walls = {}
ev = os.path.join(tree, 'docs/phase4/h10-evidence/go1.26.10/windows')
for f in sorted(os.listdir(ev)):
    if f.endswith('.tsv') and '.ledger.' not in f:
        for r in csv.DictReader(open(os.path.join(ev, f), encoding='utf-8'), delimiter='\t'):
            w = r.get('wall_s', '')
            if w.isdigit():
                walls[r['row']] = min(int(w), walls.get(r['row'], 10 ** 9))
missing = [r for r in rows if r not in walls]
assert not missing, f'no pass-1 wall for {missing}'

plan = {'R': [r for r in rows if r in OFF_I9], 'G': [r for r in rows if r in NXDOMAIN],
        'i9': [r for r in rows if r in reserved and r not in OFF_I9 and r not in NXDOMAIN], 'H': []}
load = {k: sum(walls[r] for r in v) for k, v in plan.items()}
rest = [r for r in rows if r not in OFF_I9 and r not in NXDOMAIN and r not in reserved]
for r in sorted(rest, key=lambda r: (-walls[r], r)):
    best = min(('i9', 'H', 'G'), key=lambda k: (load[k], k))
    plan[best].append(r)
    load[best] += walls[r]

got = sorted(r for v in plan.values() for r in v)
assert got == sorted(rows), set(got) ^ set(rows)

with open(out, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('os\tlane\tshard\tseq\trow\tprojected_s\n')
    for lane in ('i9', 'H', 'G', 'R'):
        order = sorted(plan[lane], key=lambda r: (r == 'testing', walls[r], r))
        for i, r in enumerate(order, 1):
            fh.write(f'windows\t{lane}\t{lane}-S\t{i}\t{r}\t{walls[r]}\n')
        print(f'{lane:3} rows={len(order):3} projected={load[lane]:6d} s ({load[lane] / 3600:.2f} h)', file=sys.stderr)
