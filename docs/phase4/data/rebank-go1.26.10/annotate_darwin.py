# The go1.26.10 darwin ANNOTATION LEG (PLAN-rebank-go126.md 13.3 step 3): rewrite each roster row's `darwin:`
# segment from the hop's darwin evidence, by the same rule src/check-roster-format.ps1 §2h checks.
# Usage: py -3 -I annotate_darwin.py <worktree> [--evidence <dir>] [--write]
#   Without --write it prints the change list and changes nothing.
#   --evidence names the evidence directory, relative to <worktree> (or absolute). It defaults to the guard's
#   $DarwinEvidenceDirectory, the one place the darwin evidence is declared, so that the annotation and the guard
#   read the same directory; the annotation seat of a re-bank at an unchanged pin points that line at its own
#   directory (1.26.10.2: docs/phase4/rebank-1.26.10.2/darwin) in the same commit. The directory read is printed
#   with the summary.
# The rule, §2h's:
#   - the evidence is <evidence>/, read in DECLARED WAVES ('' = D-W1 at the top, dw2,
#     e1, reread), and a row's reading of record is its LATEST wave;
#   - banked = both RIDs `validated`, `agree` yes on both, and equal matched and disclosed-divergent. The segment is
#     then "matched[ + disclosed-divergent]". Disclosed-divergent is `divergent - new` (the disclosed share of the
#     non-matching verdicts), NOT the `disclosed` column: that is len(comparison.disclosed), the disclosure LIST, which
#     the converter builds as the divergent set PLUS every host-fatal name in scope, tests with no verdict on either side
#     (COORD 2026-10-26, R's finding; runtime/debug read 8 + 2 by the list and is 8 + 1, as windows banks it);
#   - read and not banked: the row carries NO darwin: segment;
#   - a row the evidence does not name is left as it is.
# The waves are the guard's own list, read from the script so the two cannot drift.
import argparse, csv, os, re, sys

ap = argparse.ArgumentParser(description='Rewrite the roster rows\' darwin: segments from the darwin evidence TSVs.')
ap.add_argument('tree')
ap.add_argument('--evidence', help='the evidence directory, relative to the worktree or absolute (default: the guard\'s $DarwinEvidenceDirectory)')
ap.add_argument('--write', action='store_true')
args = ap.parse_args()
tree, write = args.tree, args.write

props = open(os.path.join(tree, 'src/version.props'), encoding='utf-8').read()
pin = re.search(r'<GoStdLibVersion>([^<]+)</GoStdLibVersion>', props).group(1)
guard = open(os.path.join(tree, 'src/check-roster-format.ps1'), encoding='utf-8').read()
waves = re.findall(r"'([^']*)'", re.search(r'^\$DarwinEvidenceWaves = @\(([^)]*)\)', guard, re.M).group(1))
assert waves and waves[0] == '', waves
# The guard's $DarwinAgreementRulings keys (COORD 2026-10-26): rows whose RIDs agree RELATIVE TO GO though the TSV's
# raw-verdict agree column reads NO. Read from the guard so the two cannot drift.
rulings_block = re.search(r'^\$DarwinAgreementRulings = @\{(.*?)^\}', guard, re.M | re.S).group(1)
rulings = set(re.findall(r"^\s*'([^']+)'\s*=", rulings_block, re.M))
assert rulings, 'no agreement rulings parsed'

declared = re.search(r"^\$DarwinEvidenceDirectory = '([^']+)'", guard, re.M)
assert declared, 'no $DarwinEvidenceDirectory parsed from the guard'
evrel = args.evidence or declared.group(1).replace('{pin}', pin)
evdir = os.path.join(tree, evrel)
if not os.path.isdir(evdir):
    sys.exit(f'evidence directory {evrel} does not exist')
by_row = {}
for d, _, files in os.walk(evdir):
    rel = os.path.relpath(d, evdir).replace(os.sep, '/')
    rel = '' if rel == '.' else rel
    if rel not in waves:
        sys.exit(f'evidence subdirectory {rel!r} is not a declared wave {waves}')
    for f in sorted(files):
        if f.endswith('.tsv'):
            for r in csv.DictReader(open(os.path.join(d, f), encoding='utf-8'), delimiter='\t'):
                by_row.setdefault(r['row'], []).append((waves.index(rel), f[:-4], r))

def reading(records):
    latest = max(w for w, _, _ in records)
    in_wave = [(s, r) for w, s, r in records if w == latest]
    stems = sorted({s for s, _ in in_wave})
    # A re-dispatch (its stem extends the original's, r3 -> r3b) supersedes the original within the wave; anything else
    # keeps every line, and the one-line-per-RID check below refuses it. The guard's Select-LatestDarwinWave rule.
    if len(stems) > 1 and all(stems[k].startswith(stems[k - 1]) for k in range(1, len(stems))):
        in_wave = [(s, r) for s, r in in_wave if s == stems[-1]]
    rs = [r for _, r in in_wave]
    arm = [r for r in rs if r['RID'] == 'osx-arm64']
    x64 = [r for r in rs if r['RID'] == 'osx-x64']
    if len(arm) != 1 or len(x64) != 1:
        sys.exit(f"{rs[0]['row']}: {len(arm)} osx-arm64 and {len(x64)} osx-x64 lines in its latest wave")
    a, x = arm[0], x64[0]
    agreed = a['agree'] == x['agree'] == 'yes' or a['row'] in rulings
    if not (a['status'] == x['status'] == 'validated' and agreed):
        return None
    div_a = int(a['divergent']) - int(a['new'])
    div_x = int(x['divergent']) - int(x['new'])
    banked = a['matched'] == x['matched'] and div_a == div_x
    if not banked:
        return None
    return a['matched'] + (f' + {div_a}' if div_a else '')

table = os.path.join(tree, 'docs/ValidatedTestPackages.md')
lines = open(table, encoding='utf-8', newline='').read().split('\n')
SEG = re.compile(r' · darwin: (?:\d+(?: \+ \d+)?|n/a)(?= · | \|$)')
row_re = re.compile(r'^\| \[`([^`]+)`\]\(')
changes = []
for i, line in enumerate(lines):
    m = row_re.match(line)
    if not m or m.group(1) not in by_row:
        continue
    pkg = m.group(1)
    want = reading(by_row[pkg])
    have = SEG.search(line)
    if want is None:
        new = SEG.sub('', line) if have else line
    else:
        seg = f' · darwin: {want}'
        if have:
            new = line[:have.start()] + seg + line[have.end():]
        else:
            proof = line.rfind(' · [proof](')
            assert proof > 0, f'{pkg}: no proof link to insert before'
            new = line[:proof] + seg + line[proof:]
    if new != line:
        changes.append((pkg, have.group(0).strip(' ·') if have else '-', f'darwin: {want}' if want else '-'))
        lines[i] = new

for pkg, old, new in changes:
    print(f'{pkg:45} {old:25} -> {new}')
print(f'{len(changes)} row(s) change; {len(by_row)} rows named by the evidence in {evrel}', file=sys.stderr)
if write:
    open(table, 'w', encoding='utf-8', newline='').write('\n'.join(lines))
