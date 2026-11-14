# collect_darwin.py -- the darwin twin of docs/phase4/la64-s7/collect.py (R, the 1.26.11 hop's H10). One os-matrix
# sweep-shard dispatch, goos darwin: reads each mac leg's artifact (rows/src/core/<row>/go2cs_test_comparison.json and
# the sweep log) and prints the darwin evidence TSV that check-roster-format.ps1's darwin section reads, in the column
# set docs/phase4/rebank-1.26.10.3/darwin/README.md documents (the 1.26.10.2 header plus run, wall_s, cells, absent).
# Every cell is READ: verdicts/matched/disclosed/divergent/new from the comparison record, status and wall from the log,
# win_expected/win_disclosed and darwin_seg from the roster row at the tree, with the same cell logic as collect.py.
#   python -I collect_darwin.py <shard> <run id> <osx-x64 artifact dir> <osx-arm64 artifact dir> <roster md>
import json, os, re, sys

shard, run = sys.argv[1], sys.argv[2]
legs = {'osx-x64': sys.argv[3], 'osx-arm64': sys.argv[4]}
roster = open(sys.argv[5], encoding='utf-8').read()
LINE = re.compile(r'^\s+(PASS|FAIL|COUNT|CVAC|DISC|HOP|HOPNONE)\s+(\S+)\s+(?:(\d+)\b)?.*?(?:\[(\d+)s\])?\s*$')
SEG = re.compile(r'darwin: (\d+)(?: \+ (\d+))?(?: \(osx-x64; osx-arm64: (\d+)(?: \+ (\d+))?\))?')


def leg(rid, root):
    logs = [f for f in os.listdir(root) if f.startswith('sweep-') and f.endswith('.log')]
    if len(logs) != 1:
        sys.exit(f'{rid}: {len(logs)} sweep logs in {root}, not one')
    words = {}
    for raw in open(os.path.join(root, logs[0]), encoding='utf-8', errors='replace'):
        m = LINE.match(raw.rstrip('\n'))
        if m:
            words[m.group(2)] = (m.group(1), int(m.group(3) or -1), int(m.group(4) or -1))
    rows = {}
    for d, _, files in os.walk(os.path.join(root, 'rows', 'src', 'core')):
        if 'go2cs_test_comparison.json' in files:
            doc = json.load(open(os.path.join(d, 'go2cs_test_comparison.json'), encoding='utf-8'))
            rows[doc['package']] = doc
    return words, rows


def roster_row(row):
    # The table line of the row: | [`row`](...) | windows expected | windows disclosed | ... darwin: ... |
    m = re.search(r'^\| \[`' + re.escape(row) + r'`\]\([^)]*\) \| (\d+) \|\s*(\d*)\s*\|(.*)$', roster, re.M)
    if not m:
        sys.exit(f'{row}: no roster row')
    win_e, win_d, rest = m.group(1), m.group(2) or '0', m.group(3)
    # The darwin segment is the LAST "darwin:" in the row's visible text (comments carry old segments).
    visible = re.sub(r'<!--.*?-->', '', rest, flags=re.S)
    segs = list(SEG.finditer(visible))
    seg = {}
    if segs:
        s = segs[-1]
        x64 = (s.group(1), s.group(2) or '0')
        arm = (s.group(3), s.group(4) or '0') if s.group(3) else x64
        seg = {'osx-x64': x64, 'osx-arm64': arm}
    return win_e, win_d, seg


read = {rid: leg(rid, root) for rid, root in legs.items()}
names = sorted(set(read['osx-x64'][0]) | set(read['osx-arm64'][0]))
cols = 'shard row RID verdicts matched disclosed divergent new status agree win_expected win_disclosed darwin_seg win_differs run wall_s cells absent'.split()
print('\t'.join(cols))
failed = 0
for row in names:
    win_e, win_d, seg = roster_row(row)
    sets = {}
    for rid in legs:
        doc = read[rid][1].get(row)
        if doc is not None:
            sets[rid] = set(doc.get('go') or {}) | set(doc.get('csharp') or {})
    for rid in legs:
        words, docs = read[rid]
        other_rid = 'osx-arm64' if rid == 'osx-x64' else 'osx-x64'
        word, count, wall = words.get(row, ('ABSENT', -1, -1))
        other = read[other_rid][0].get(row, ('ABSENT', -1, -1))
        agree = 'yes' if (word, count) == other[:2] else 'no'
        doc = docs.get(row)
        if doc is None:
            sys.exit(f'{row} {rid}: no comparison record in the artifact')
        go, cs = doc.get('go') or {}, doc.get('csharp') or {}
        verdict_names = set(go) | set(cs)
        disclosed_names = {re.match(r'(\S+)', s).group(1) for s in (doc.get('disclosed') or []) if s and re.match(r'(\S+)', s)}
        div = sorted(n for n in verdict_names if go.get(n, '') != cs.get(n, ''))
        new = [n for n in div if n not in disclosed_names]
        matched = len(verdict_names) - len(div)
        if word == 'PASS' and matched != count:
            print(f'REFUSED {row} {rid}: the log prints {count}, the comparison record reads matched {matched}', file=sys.stderr)
            failed += 1
        osets = sets.get(other_rid)
        absent = sorted(osets - verdict_names) if osets is not None else []
        ds = seg.get(rid)
        darwin_seg = f'{ds[0]}/{ds[1]}' if ds else '-'
        win_differs = 'yes' if ds and (ds[0], ds[1]) != (win_e, win_d) else ('no' if ds else '-')
        print('\t'.join([shard, row, rid, str(len(verdict_names)), str(matched), str(len(doc.get('disclosed') or [])),
                         str(len(div)), str(len(new)), 'validated' if word == 'PASS' else word, agree, win_e, win_d,
                         darwin_seg, win_differs, run, str(wall), 'read', ';'.join(absent) or '-']))
sys.exit(failed)
