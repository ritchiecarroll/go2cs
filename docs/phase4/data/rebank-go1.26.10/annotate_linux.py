# The go1.26.10 LINUX annotation leg (PLAN-rebank-go126.md 5.1 and 13.3 step 2): rewrite each roster row's `linux:`
# segment from the hop's linux evidence TSVs.
# Usage: py -3 -I annotate_linux.py <worktree> [--write]
#   Without --write it prints the change list and changes nothing.
# The rule, the batch-41 leg's (61fe31d90a), extended to the later legs:
#   - the evidence is docs/phase4/h10-evidence/go<pin>/linux/*.tsv (ledgers and .untracked excluded), read in DECLARED
#     WAVES by shard name: pass 1 (`P1-3`), L1 (`P2-L1`), L2 (`P1-L2`, `P1-L2-poll`), the final re-read (`P1-R`).
#     A row's reading of record is its LATEST wave, and within it its LAST line (crypto/x509's SSL_CERT_FILE-unset
#     re-run is the precedent);
#   - that reading banks only on `PASS`, and the segment is "<matched>[ + <disclosed-divergent>]", matched being the count
#     column where the TSV has one and `verdicts` where `matched` is pass 1's boolean;
#   - the TSVs' `disclosed` column is len(comparison.disclosed), the disclosure LIST, which the converter builds as the
#     disclosed-divergent set PLUS every host-fatal name in scope (src/go2cs/testConversion.go, hostFatalNames, appended
#     after the comparison: those tests ran on neither side and have no verdict). The roster's disclosed count is the
#     summary's "K disclosed-divergent" (Get-DisclosedCount), so the row's host-fatal entries in scope on linux are
#     subtracted (COORD 2026-10-26, R's finding: runtime 11091 + 76 is 11091 + 70);
#   - a row whose reading of record is NOT PASS keeps its segment and is listed, for its re-read or a ruling.
import csv, json, os, re, sys

tree = sys.argv[1]
write = '--write' in sys.argv[2:]
props = open(os.path.join(tree, 'src/version.props'), encoding='utf-8').read()
pin = re.search(r'<GoStdLibVersion>([^<]+)</GoStdLibVersion>', props).group(1)
evdir = os.path.join(tree, f'docs/phase4/h10-evidence/go{pin}/linux')

WAVES = [('pass1', re.compile(r'^[A-Za-z0-9]+-\d+$')), ('L1', re.compile(r'^[A-Za-z0-9]+-L1$')),
         ('L2', re.compile(r'^[A-Za-z0-9]+-L2(-[a-z0-9]+)?$')), ('reread', re.compile(r'^[A-Za-z0-9]+-R$'))]

by_row = {}
for f in sorted(os.listdir(evdir)):
    if not f.endswith('.tsv') or '.ledger.' in f or '.untracked.' in f:
        continue
    stem = f[:-4]
    wave = [i for i, (_, rx) in enumerate(WAVES) if rx.match(stem)]
    if len(wave) != 1:
        sys.exit(f'{f}: matches {len(wave)} declared waves, not one')
    for n, r in enumerate(csv.DictReader(open(os.path.join(evdir, f), encoding='utf-8'), delimiter='\t')):
        by_row.setdefault(r['row'], []).append(((wave[0], f, n), r))

def host_fatal_in_scope(pkg, goos='linux'):
    path = os.path.join(tree, 'src/core', pkg, 'go2cs_test_disclosures.json')
    if not os.path.exists(path):
        return 0
    entries = json.load(open(path, encoding='utf-8')).get('disclosures', [])
    return sum(1 for e in entries if e.get('class') == 'host-fatal' and (not e.get('platforms') or goos in e['platforms']))

def reading(records):
    (_, f, _), r = max(records, key=lambda t: t[0])
    if r['word'] != 'PASS':
        return None, f'{r["word"]} in {f}'
    matched = r['matched'] if r['matched'].isdigit() else r['verdicts']
    divergent = int(r['disclosed']) - host_fatal_in_scope(r['row'])
    assert divergent >= 0, f"{r['row']}: disclosed {r['disclosed']} is less than its host-fatal entries in scope"
    return matched + (f' + {divergent}' if divergent else ''), f

table = os.path.join(tree, 'docs/ValidatedTestPackages.md')
lines = open(table, encoding='utf-8', newline='').read().split('\n')
SEG = re.compile(r' · linux: (?:\d+(?: \+ \d+)?|n/a)(?= · | \|$)')
row_re = re.compile(r'^\| \[`([^`]+)`\]\(')
changes, held = [], []
for i, line in enumerate(lines):
    m = row_re.match(line)
    if not m or m.group(1) not in by_row:
        continue
    pkg = m.group(1)
    want, src = reading(by_row[pkg])
    have = SEG.search(line)
    if want is None:
        held.append((pkg, have.group(0).strip(' ·') if have else '-', src))
        continue
    seg = f' · linux: {want}'
    if have:
        new = line[:have.start()] + seg + line[have.end():]
    else:
        proof = line.rfind(' · [proof](')
        assert proof > 0, f'{pkg}: no proof link to insert before'
        new = line[:proof] + seg + line[proof:]
    if new != line:
        changes.append((pkg, have.group(0).strip(' ·') if have else '-', f'linux: {want}', src))
        lines[i] = new

for pkg, old, new, src in changes:
    print(f'{pkg:40} {old:22} -> {new:22} ({src})')
for pkg, old, src in held:
    print(f'{pkg:40} {old:22} HELD: reading of record is {src}')
print(f'{len(changes)} row(s) change, {len(held)} held; {len(by_row)} rows named by the evidence', file=sys.stderr)
if write:
    open(table, 'w', encoding='utf-8', newline='').write('\n'.join(lines))
