# collect.py -- S7 linux evidence collector (P2). One os-matrix sweep-shard dispatch, arch both: reads each leg's
# artifact (rows/src/core/<row>/go2cs_test_comparison.json and the sweep log) and prints the mac-form TSV that
# annotate_linux.py --rids and check-roster-format.ps1 section 2i read, one line per row per RID, every cell READ.
#   python3 -I collect.py <shard> <run id> <x64 artifact dir> <arm64 artifact dir>
# Columns: shard row RID verdicts matched disclosed divergent new status agree absent divergent_names run wall_s cells
#   verdicts   the verdict names compared: go's names united with C#'s
#   matched    names whose two verdicts are equal; divergent: names whose verdicts differ (matched + divergent = verdicts)
#   disclosed  len(comparison.disclosed), the disclosure LIST (it carries host-fatal names too; the guard subtracts)
#   new        divergent names the disclosure list does not name; divergent_names: the divergent names it does name
#   status     the sweep's per-row word: PASS -> validated; anything else verbatim
#   agree      yes when both legs print the same word and the same count for the row
#   absent     the verdict names this RID lacks against the other RID's set
# A row whose log count disagrees with `matched` is refused by name: the collector and the sweep must read one record.
import json, os, re, sys

shard, run, legs = sys.argv[1], sys.argv[2], {'linux-x64': sys.argv[3], 'linux-arm64': sys.argv[4]}
LINE = re.compile(r'^\s+(PASS|FAIL|COUNT|CVAC|DISC|HOP|HOPNONE)\s+(\S+)\s+(?:(\d+)\b)?.*?(?:\[(\d+)s\])?\s*$')


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
    base = os.path.join(root, 'rows', 'src', 'core')
    for d, _, files in os.walk(base):
        if 'go2cs_test_comparison.json' in files:
            doc = json.load(open(os.path.join(d, 'go2cs_test_comparison.json'), encoding='utf-8'))
            rows[doc['package']] = doc
    return words, rows


read = {rid: leg(rid, root) for rid, root in legs.items()}
names = sorted(set(read['linux-x64'][0]) | set(read['linux-arm64'][0]))
cols = 'shard row RID verdicts matched disclosed divergent new status agree absent divergent_names run wall_s cells'.split()
print('\t'.join(cols))
failed = 0
for row in names:
    sets, out = {}, {}
    for rid in legs:
        words, docs = read[rid]
        doc = docs.get(row)
        if doc is not None:
            go, cs = doc.get('go') or {}, doc.get('csharp') or {}
            sets[rid] = set(go) | set(cs)
    for rid in legs:
        words, docs = read[rid]
        word, count, wall = words.get(row, ('ABSENT', -1, -1))
        other = read['linux-arm64' if rid == 'linux-x64' else 'linux-x64'][0].get(row, ('ABSENT', -1, -1))
        agree = 'yes' if (word, count) == other[:2] else 'no'
        doc = docs.get(row)
        if doc is None:
            out[rid] = [shard, row, rid, '-', str(count), '-', '-', '-', word if word != 'PASS' else 'validated', agree, '-', '-', run, str(wall), 'read']
            continue
        go, cs = doc.get('go') or {}, doc.get('csharp') or {}
        verdict_names = set(go) | set(cs)
        disclosed_names = {re.match(r'(\S+)', s).group(1) for s in (doc.get('disclosed') or []) if s and re.match(r'(\S+)', s)}
        div = sorted(n for n in verdict_names if go.get(n, '') != cs.get(n, ''))
        new = [n for n in div if n not in disclosed_names]
        named = [n for n in div if n in disclosed_names]
        matched = len(verdict_names) - len(div)
        if word == 'PASS' and matched != count:
            print(f'REFUSED {row} {rid}: the log prints {count}, the comparison record reads matched {matched}', file=sys.stderr)
            failed += 1
        osets = sets.get('linux-arm64' if rid == 'linux-x64' else 'linux-x64')
        absent = sorted(osets - verdict_names) if osets is not None else []
        out[rid] = [shard, row, rid, str(len(verdict_names)), str(matched), str(len(doc.get('disclosed') or [])), str(len(div)),
                    str(len(new)), 'validated' if word == 'PASS' else word, agree, ';'.join(absent) or '-', ';'.join(named) or '-',
                    run, str(wall), 'read']
    for rid in legs:
        print('\t'.join(out[rid]))
sys.exit(failed)
