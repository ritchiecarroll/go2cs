# score.py <shard> <plain-dir|-> <twinned-dir> [<plain-joblog|-> <twinned-joblog>]  ('-' = no plain half; scored against the bank)
# D2/T6 wave scorer. Each dir is a `gh run download` of one sweep-shard run; each joblog is `gh run view --log` of it.
# Per row: the sweep's word and count (PASS = EQUAL to the roster's banked linux reading), the comparison record's matching
# count and disclosed set, and the t6-proof line (model, twin names). Twinned is scored against the bank (its word) and
# against the plain half (same matching count AND the same disclosed set).
import json, os, re, sys

LINE = re.compile(r'^\s+(PASS|FAIL|COUNT|CVAC|DISC|HOP|HOPNONE)\s+(\S+)\s+(?:(\d+)\b)?.*?(?:\[(\d+)s\])?\s*$')
PROOF = re.compile(r'READING t6-proof row=(\S+) twins-option=\'(\d?)\'(?: model=(\S+))? twin-names=(\d+) test-assembly=(\d+)')
SHARD = re.compile(r'READING t6-proof shard twin-names=(\d+)')
ENV = re.compile(r"READING t6-env GO2CS_MN='([^']*)' GoStdlibTwins='([^']*)'")

def half(root, joblog):
    words, rows = {}, {}
    if root == '-':
        return words, rows, {}, None, None
    for d, _, files in os.walk(root):
        for f in files:
            p = os.path.join(d, f)
            if f.startswith('sweep-') and f.endswith('.log'):
                for raw in open(p, encoding='utf-8', errors='replace'):
                    m = LINE.match(raw.rstrip('\n'))
                    if m:
                        words[m.group(2)] = (m.group(1), int(m.group(3) or -1), int(m.group(4) or -1))
            elif f == 'go2cs_test_comparison.json':
                doc = json.load(open(p, encoding='utf-8'))
                rows[doc['package']] = doc
    proof, shard, env = {}, None, None
    if joblog:
        for raw in open(joblog, encoding='utf-8', errors='replace'):
            if 'Write-Host' in raw:
                continue
            m = PROOF.search(raw)
            if m:
                proof[m.group(1)] = (m.group(3) or '-', int(m.group(4)), int(m.group(5)))
            m = SHARD.search(raw)
            if m:
                shard = int(m.group(1))
            m = ENV.search(raw)
            if m:
                env = (m.group(1), m.group(2))
    return words, rows, proof, shard, env

def matched_and_disclosed(doc):
    if doc is None:
        return None, None
    go, cs = doc.get('go') or {}, doc.get('csharp') or {}
    names = set(go) | set(cs)
    matched = sum(1 for n in names if go.get(n) == cs.get(n))
    disclosed = tuple(sorted(doc.get('disclosed') or []))
    return matched, disclosed

shard = sys.argv[1]
P = half(sys.argv[2], sys.argv[4] if len(sys.argv) > 4 and sys.argv[4] != '-' else None)
T = half(sys.argv[3], sys.argv[5] if len(sys.argv) > 5 else None)
print(f'# {shard} env plain={P[4]} twinned={T[4]} shard-twin-names plain={P[3]} twinned={T[3]}')
cols = 'shard row p_word p_count t_word t_count p_matched t_matched disclosed_equal t_eq_bank t_eq_plain model p_twins t_twins t_test p_wall t_wall'.split()
print('\t'.join(cols))
rows = sorted(set(P[0]) | set(T[0]))
summary = {'rows': 0, 'eq_bank': 0, 'eq_plain': 0, 'p_pass': 0}
for r in rows:
    pw, pc, pwall = P[0].get(r, ('ABSENT', -1, -1))
    tw, tc, twall = T[0].get(r, ('ABSENT', -1, -1))
    pm, pd = matched_and_disclosed(P[1].get(r))
    tm, td = matched_and_disclosed(T[1].get(r))
    eq_bank = 'yes' if tw == 'PASS' else 'NO'
    if sys.argv[2] == '-':
        eq_plain = 'bank'  # no plain half in this wave: the bank is the plain reading
    else:
        eq_plain = 'yes' if (pm is not None and pm == tm and pd == td and pw == tw) else 'NO'
    model, ptw, _ = P[2].get(r, ('-', -1, -1))
    tmodel, ttw, ttest = T[2].get(r, ('-', -1, -1))
    summary['rows'] += 1
    summary['eq_bank'] += eq_bank == 'yes'
    summary['eq_plain'] += eq_plain == 'yes'
    summary['p_pass'] += pw == 'PASS'
    print('\t'.join(map(str, [shard, r, pw, pc, tw, tc, pm, tm, 'yes' if pd == td else 'NO', eq_bank, eq_plain,
                              tmodel if tmodel != '-' else model, ptw, ttw, ttest, pwall, twall])))
print(f"# summary rows={summary['rows']} plain PASS={summary['p_pass']} twinned EQUAL-bank={summary['eq_bank']} twinned EQUAL-plain={summary['eq_plain']}")
