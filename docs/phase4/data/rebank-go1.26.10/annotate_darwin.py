# The go1.26.10 darwin ANNOTATION LEG (PLAN-rebank-go126.md 13.3 step 3): rewrite each roster row's `darwin:`
# segment from the hop's darwin evidence, by the same rule src/check-roster-format.ps1 §2h checks.
# Usage: py -3 -I annotate_darwin.py <worktree> [--write]
#   Without --write it prints the change list and changes nothing.
# The rule, §2h's:
#   - the evidence is docs/phase4/h10-evidence/go<pin>/darwin/, read in DECLARED WAVES ('' = D-W1 at the top, dw2,
#     e1, reread), and a row's reading of record is its LATEST wave;
#   - banked = both RIDs `validated`, `agree` yes on both, and equal matched and disclosed. The segment is then
#     "matched[ + disclosed]";
#   - read and not banked: the row carries NO darwin: segment;
#   - a row the evidence does not name is left as it is.
# The waves are the guard's own list, read from the script so the two cannot drift.
import csv, os, re, sys

tree = sys.argv[1]
write = '--write' in sys.argv[2:]

props = open(os.path.join(tree, 'src/version.props'), encoding='utf-8').read()
pin = re.search(r'<GoStdLibVersion>([^<]+)</GoStdLibVersion>', props).group(1)
guard = open(os.path.join(tree, 'src/check-roster-format.ps1'), encoding='utf-8').read()
waves = re.findall(r"'([^']*)'", re.search(r'^\$DarwinEvidenceWaves = @\(([^)]*)\)', guard, re.M).group(1))
assert waves and waves[0] == '', waves

evdir = os.path.join(tree, f'docs/phase4/h10-evidence/go{pin}/darwin')
by_row = {}
for d, _, files in os.walk(evdir):
    rel = os.path.relpath(d, evdir).replace(os.sep, '/')
    rel = '' if rel == '.' else rel
    if rel not in waves:
        sys.exit(f'evidence subdirectory {rel!r} is not a declared wave {waves}')
    for f in sorted(files):
        if f.endswith('.tsv'):
            for r in csv.DictReader(open(os.path.join(d, f), encoding='utf-8'), delimiter='\t'):
                by_row.setdefault(r['row'], []).append((waves.index(rel), r))

def reading(records):
    latest = max(w for w, _ in records)
    rs = [r for w, r in records if w == latest]
    arm = [r for r in rs if r['RID'] == 'osx-arm64']
    x64 = [r for r in rs if r['RID'] == 'osx-x64']
    if len(arm) != 1 or len(x64) != 1:
        sys.exit(f"{rs[0]['row']}: {len(arm)} osx-arm64 and {len(x64)} osx-x64 lines in its latest wave")
    a, x = arm[0], x64[0]
    banked = a['status'] == x['status'] == 'validated' and a['agree'] == x['agree'] == 'yes' \
        and a['matched'] == x['matched'] and a['disclosed'] == x['disclosed']
    if not banked:
        return None
    return a['matched'] + (f" + {a['disclosed']}" if int(a['disclosed']) else '')

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
print(f'{len(changes)} row(s) change; {len(by_row)} rows named by the evidence', file=sys.stderr)
if write:
    open(table, 'w', encoding='utf-8', newline='').write('\n'.join(lines))
