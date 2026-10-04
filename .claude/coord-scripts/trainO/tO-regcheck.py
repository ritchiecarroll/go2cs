#!/usr/bin/env python3
# tO-regcheck.py <base> <head> <chain.tsv from tO-premap.sh MODE=chain> [files...] (DRAFT 2026-10-03, UNCOMMITTED)
# For each shared registration file (default: go2cs.slnx, go2cs-src.projitems, the four BehavioralTests lists), assert
#   COUNT: lines(head) == lines(base) + sum(each row's own net, own = merge-base(previous chain step, row)..row)
#   NOTE: two rows inserting the SAME lines at the SAME place collapse to one copy in a 3-way merge (correct), and the
#   COUNT then reads LOW by that many lines while KEYS stay clean: read the KEYS before calling a COUNT miss a loss.
# Controls 2026-10-03: ok on all six files at N's real merge 8fbc1b0a13 (rows = N's 30); FAIL (16 / 30 missing) on 8f46.
#   KEYS : every '+' line a row adds is present in head; every '-' line a row removes is absent (unless re-added by another row);
#          no line occurs in head more often than in base + the rows that add it (silent duplication).
import subprocess, sys, collections
GD = 'H:/Projects/go2cs/.git'
def git(*a):
    return subprocess.run(['git', '--git-dir=' + GD, '-c', 'core.quotepath=false'] + list(a), capture_output=True).stdout
base, head, rowsf = sys.argv[1], sys.argv[2], sys.argv[3]
files = sys.argv[4:] or ['src/go2cs.slnx', 'src/go2cs/go2cs-src.projitems',
    'src/tests/Behavioral/BehavioralTests/CompileTests.cs', 'src/tests/Behavioral/BehavioralTests/OutputComparisonTests.cs',
    'src/tests/Behavioral/BehavioralTests/TargetComparisonTests.cs', 'src/tests/Behavioral/BehavioralTests/TranspileTests.cs']
# rowsf = chain.tsv written by tO-premap.sh: k, name, sha10, chain-commit-after-step; a row's OWN change is mb(prev step, row)..row
rows = []; prev = base
for l in open(rowsf, encoding='utf-8'):
    k, n, s10, c = l.rstrip('\r\n').split('\t')
    s = git('rev-parse', s10 + '^{commit}').decode().strip()
    c = git('rev-parse', c + '^{commit}').decode().strip()
    if c != prev: rows.append((n, s, prev))
    prev = c
def lines(rev, f):
    b = git('show', f'{rev}:{f}')
    return b.decode('utf-8', 'replace').replace('\r\n', '\n').split('\n')
for f in files:
    bl = lines(base, f); hl = lines(head, f)
    bc = collections.Counter(bl); hc = collections.Counter(hl)
    exp = len(bl); adds = collections.Counter(); rems = collections.Counter(); writers = []
    for n, s, pv in rows:
        mb = git('merge-base', pv, s).decode().strip()
        # own change = mb..row, but a row whose base predates BASE carries BASE-side lines only through the merge;
        # its own patch is mb..row on this file
        d = git('diff', '-U0', mb, s, '--', f).decode('utf-8', 'replace').replace('\r\n', '\n').split('\n')
        a = [x[1:] for x in d if x.startswith('+') and not x.startswith('+++')]
        r = [x[1:] for x in d if x.startswith('-') and not x.startswith('---')]
        if a or r:
            writers.append(f'{n}(+{len(a)}/-{len(r)})')
            exp += len(a) - len(r); adds.update(a); rems.update(r)
    missing = [x for x in adds if hc[x] < 1]
    dup = [x for x in hc if x.strip() and hc[x] > bc[x] + adds[x]]
    back = [x for x in rems if hc[x] > max(0, bc[x] - rems[x]) + adds[x]]
    status = 'ok' if (len(hl) == exp and not missing and not dup and not back) else 'FAIL'
    print(f'{status} {f}: base {len(bl)} head {len(hl)} expected {exp} writers={len(writers)} missing={len(missing)} dup={len(dup)} removed-back={len(back)}')
    for x in missing[:5]: print('   missing:', x[:160])
    for x in dup[:5]: print('   dup:', x[:160])
    for x in back[:5]: print('   removed-back:', x[:160])
