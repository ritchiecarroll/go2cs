# Checks the 1.26.10.2 re-bank deal against a tree, so a re-cut at a new tip is proved, not asserted
# (PLAN-release-1-26-10-2.md "The re-bank legs"). It re-reads every input the deal depends on at that tree and
# fails on the first disagreement.
# Usage: py -3 -I legs_check.py <worktree> [<legs.tsv>] [<sweep-shards.tsv>]
# Inputs (read from the worktree):
#   docs/ValidatedTestPackages.md                   the banked rows and their linux:/darwin: segments (comments stripped)
#   docs/phase4/data/rebank-go1.26.10/eligible-darwin.txt   the darwin-eligible population
#   src/run-validated-sweep.ps1                     $longTimeouts, the reserved set
# Checks:
#   - linux: the legs' linux rows equal the rows whose linux: segment is a number, each exactly once;
#   - darwin: the legs' darwin rows equal the banked rows in eligible-darwin.txt, each exactly once;
#   - RWSL-1 equals the $longTimeouts keys that are linux-eligible;
#   - windows: sweep-shards.tsv holds every banked row exactly once; R holds the off-i9 list by name; net is on G;
#   - every shard's seq runs 1..n, and `testing` is the last row of the list it is in.
# It prints each shard's rows and projected wall, and each lane's total.
import csv, os, re, sys
from collections import Counter, defaultdict

tree = sys.argv[1]
legs_path = sys.argv[2] if len(sys.argv) > 2 else os.path.join(tree, 'docs/phase4/data/rebank-1.26.10.2/legs.tsv')
sweep_path = sys.argv[3] if len(sys.argv) > 3 else os.path.join(tree, 'docs/phase4/data/rebank-go1.26.10/sweep-shards.tsv')
read = lambda p: open(os.path.join(tree, p), encoding='utf-8').read()
fails = []
def check(ok, what):
    print(('  ok    ' if ok else '  FAIL  ') + what)
    if not ok:
        fails.append(what)

table = read('docs/ValidatedTestPackages.md')
start = table.find('| Package |')
end = table.find('\n## ', start)
banked, seg = [], {}
for line in table[start:end if end > 0 else len(table)].splitlines():
    m = re.match(r'^\| \[`([^`]+)`\]\(', line)
    if not m:
        continue
    line = re.sub(r'<!--.*?-->', '', line)              # a comment can quote a retired segment
    banked.append(m.group(1))
    lx = re.search(r'linux: ([^·|]+?)\s*(?:·|\|)', line)
    seg[m.group(1)] = lx.group(1).strip() if lx else ''
numeric = lambda s: re.fullmatch(r'\d+( \+ \d+)?', s) is not None
linux_el = {r for r in banked if numeric(seg[r])}
darwin_el = set(banked) & set(read('docs/phase4/data/rebank-go1.26.10/eligible-darwin.txt').split())
m = re.search(r"^\$longTimeouts = @\{(.*)\}\s*$", read('src/run-validated-sweep.ps1'), re.M)
reserved = set(re.findall(r"'([^']+)' = '\d+m'", m.group(1)))
print(f'tree: banked={len(banked)} linux-eligible={len(linux_el)} darwin-eligible={len(darwin_el)} reserved={len(reserved)}')
check(len(banked) == len(set(banked)), 'the roster lists no row twice')

def shards(path):
    lists = defaultdict(list)
    for r in csv.DictReader(open(path, encoding='utf-8', newline=''), delimiter='\t'):
        lists[(r['os'], r['lane'], r['shard'])].append((int(r['seq']), r['row'], int(r['projected_s'])))
    return lists

def check_lists(lists, label):
    for key, items in sorted(lists.items()):
        seqs = [s for s, _, _ in items]
        check(seqs == list(range(1, len(items) + 1)), f'{label} {key[2]}: seq runs 1..{len(items)} in file order')
        names = [r for _, r, _ in items]
        if 'testing' in names:
            check(names[-1] == 'testing', f'{label} {key[2]}: testing is last of its {len(names)} rows')

legs = shards(legs_path)
for goos, want in (('linux', linux_el), ('darwin', darwin_el)):
    got = Counter(r for (o, _, _), items in legs.items() if o == goos for _, r, _ in items)
    dup = sorted(r for r, n in got.items() if n > 1)
    check(not dup, f'{goos}: no row dealt twice {dup or ""}')
    check(set(got) == want, f'{goos}: dealt rows == eligible set ({len(got)} vs {len(want)}); '
                            f'missing {sorted(want - set(got))}, extra {sorted(set(got) - want)}')
rwsl1 = {r for (o, _, s), items in legs.items() if s == 'RWSL-1' for _, r, _ in items}
check(rwsl1 == reserved & linux_el, f'RWSL-1 == the linux-eligible $longTimeouts keys ({len(rwsl1)} vs {len(reserved & linux_el)})')
check_lists(legs, 'legs')

sweep = shards(sweep_path)
got = Counter(r for items in sweep.values() for _, r, _ in items)
check(not [r for r, n in got.items() if n > 1] and set(got) == set(banked),
      f'windows: every banked row exactly once ({sum(got.values())} lines, {len(banked)} banked)')
OFF_I9 = {'crypto/tls', 'net/http', 'net/http/httptest', 'net/smtp', 'crypto/x509', 'net/http/cgi',
          'net/http/httptrace', 'os/exec'}
lane_rows = lambda lane: {r for (_, l, _), items in sweep.items() if l == lane for _, r, _ in items}
check(lane_rows('R') == OFF_I9, 'windows: R holds exactly the off-i9 list')
check('net' in lane_rows('G'), 'windows: net is on G')
check_lists(sweep, 'windows')

print()
for label, lists in (('windows', sweep), ('legs', legs)):
    lanes = defaultdict(lambda: [0, 0])
    for (o, lane, shard), items in sorted(lists.items()):
        s = sum(p for _, _, p in items)
        lanes[(o, lane)][0] += len(items); lanes[(o, lane)][1] += s
        print(f'{o:8} {lane:6} {shard:7} rows={len(items):3}  projected={s:6d} s ({s / 3600:.2f} h)')
    for (o, lane), (n, s) in lanes.items():
        print(f'{o:8} {lane:6} TOTAL   rows={n:3}  projected={s:6d} s ({s / 3600:.2f} h)')
print()
print(f'{len(fails)} check(s) FAILED' if fails else 'ALL CHECKS PASS')
sys.exit(1 if fails else 0)
