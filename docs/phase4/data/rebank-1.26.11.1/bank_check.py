# The 1.26.11.1 bank's per-cell reading (P5, 2026-11-19): every (row, RID) the roster banks is matched to its .4 reading
# of record, and that reading must equal the roster's expectation at the tree being banked.
#
# Why a script and not the annotators: annotate_linux.py's single-RID mode refuses every row once the row carries a
# linux-arm64 note (all of them since S7), and its --rids mode needs a linux-x64 and a linux-arm64 line from ONE dispatch,
# which this deal did not read (linux-x64 on the lanes, linux-arm64 hosted). annotate_darwin.py does run over the hosted
# lines; this script reads the same lines plus the lanes', so one check covers every OS.
#
# Usage: python3 -I bank_check.py <roster.tsv> <tree>
#   <roster.tsv>: roster_dump.ps1's output at <tree> (pwsh -NoProfile -File roster_dump.ps1 -Tree <tree>).
#   <tree>: the worktree; the evidence is read from it at the paths below.
# Exit 0 when every banked cell reads AT (or is a ruled exception below) and every cell is read exactly once.
import csv, os, sys

if len(sys.argv) != 3:
    sys.exit('usage: bank_check.py <roster.tsv> <tree>')
roster_path, tree = sys.argv[1], sys.argv[2]
EV = 'docs/phase4/rebank-1.26.11.1'
HOSTED = 'docs/phase4/data/rebank-1.26.11.1/hosted-280a01952c/final.tsv'
WINDOWS = ['sweep/i9-S.tsv', 'sweep/G-S.tsv', 'sweep/H-S-F.tsv', 'sweep/R-S.tsv']
LINUX = ['linux/P1-1.tsv', 'linux/P2-1.tsv', 'linux/P4-1.tsv', 'linux/RWSL-1.tsv']

# Ruled exceptions, by (row, RID): the cell is read and is NOT banked at .4. Its roster reading stays as it was.
RULED = {
    ('runtime', 'osx-x64'): 'BELOW: not banked at .4, no third read (COORD 085746Z; BOARD 2026-11-19, P5)',
}
# Superseded lane lines, by (row, RID): a later read of record replaces the lane's line (COORD rulings).
SUPERSEDED = {
    ('net', 'linux-x64'): 'RWSL-1 DIVERGED, superseded by HX-NET (COORD 043546Z: the R-WSL read is host network)',
}


def parse_cell(cell):
    """'E+D', 'E+D@RID|OTHER=E+D', 'n/a' or '-' -> {rid: 'E+D'} for linux or darwin RIDs ('*' when unsplit)."""
    if cell in ('-', 'n/a'):
        return None
    head, _, note = cell.partition('|')
    reading, _, rid = head.partition('@')
    out = {rid or '*': reading}
    if note:
        orid, _, oread = note.partition('=')
        out[orid] = oread
    return out


expect = {}
for r in csv.reader(open(roster_path, encoding='utf-8'), delimiter='\t'):
    pkg, win_e, win_d, _, lin, _, dar = r
    expect[(pkg, 'win-x64')] = f'{win_e}+{win_d or 0}'
    for cell, rids in ((lin, ('linux-x64', 'linux-arm64')), (dar, ('osx-x64', 'osx-arm64'))):
        parsed = parse_cell(cell)
        if parsed is None:
            continue
        for rid in rids:
            expect[(pkg, rid)] = parsed.get(rid, parsed.get('*'))
            if expect[(pkg, rid)] is None:
                sys.exit(f'{pkg}: roster cell {cell!r} gives no reading for {rid}')

reads = {}  # (row, RID) -> (source, verdict, detail)
bad = []


def take(key, source, verdict, detail):
    if key in reads:
        bad.append(f'{key[0]} {key[1]}: read twice ({reads[key][0]} and {source})')
    reads[key] = (source, verdict, detail)


def rows(path):
    return list(csv.DictReader(open(os.path.join(tree, path), encoding='utf-8'), delimiter='\t'))


for f in WINDOWS:
    for r in rows(f'{EV}/{f}'):
        key = (r['row'], 'win-x64')
        want = expect.get(key, '?+?').split('+')[0]
        ok = r['word'] == 'PASS' and r['count'] == r['banked'] == want
        take(key, f, 'AT' if ok else 'MISS', f"{r['word']} count {r['count']} banked {r['banked']} roster {want}")

for f in LINUX:
    for r in rows(f'{EV}/{f}'):
        key = (r['row'], 'linux-x64')
        if key in SUPERSEDED:
            continue
        e, d = (int(x) for x in expect.get(key, '-1+0').split('+'))
        # Two kit forms. In both, `verdicts` is the matched count and PASS is the sweep's own comparison against the
        # roster at the tip; the P2 kit's `matched` is a count, checked too. Neither kit's `disclosed` is compared: it
        # is the disclosure LIST's length, host-fatal names included (runtime/debug reads 2 where the roster banks
        # + 1, COORD 2026-10-26; P2-1.md says so), and the sweep's PASS already compared the disclosed-divergent count.
        if r['matched'].isdigit():
            matched_ok = int(r['matched']) == e
        else:
            matched_ok = r['matched'] == 'True'
        ok = r['word'] == 'PASS' and int(r['verdicts']) == e and matched_ok
        take(key, f, 'AT' if ok else 'MISS', f"{r['word']} verdicts {r['verdicts']} matched {r['matched']} roster {e}+{d}")

for r in rows(HOSTED):
    key = (r['row'], r['RID'])
    roster = expect.get(key)
    ok = r['verdict'] == 'AT' and r['expected'] == roster
    verdict = 'AT' if ok else ('RULED' if key in RULED and r['expected'] == roster else 'MISS')
    take(key, f"hosted {r['source']}", verdict, f"{r['word']} {r['matched']} expected {r['expected']} roster {roster}")

for key in sorted(set(expect) - set(reads)):
    bad.append(f'{key[0]} {key[1]}: banked at {expect[key]} and not read')
for key in sorted(set(reads) - set(expect)):
    bad.append(f'{key[0]} {key[1]}: read ({reads[key][0]}) but the roster banks no reading there')
for key, (source, verdict, detail) in sorted(reads.items()):
    if verdict == 'MISS':
        bad.append(f'{key[0]} {key[1]}: {source}: {detail}')
for key in sorted(RULED):
    if reads.get(key, ('', ''))[1] != 'RULED':
        bad.append(f'{key[0]} {key[1]}: ruled exception not read as ruled ({reads.get(key)})')

by_rid = {}
for (row, rid), (_, verdict, _) in reads.items():
    by_rid.setdefault(rid, {}).setdefault(verdict, 0)
    by_rid[rid][verdict] += 1
for rid in sorted(by_rid):
    print(f'{rid:12} ' + ', '.join(f'{v} {n}' for v, n in sorted(by_rid[rid].items())))
for key, why in sorted(RULED.items()):
    print(f'ruled  {key[0]} {key[1]}: {why}')
for key, why in sorted(SUPERSEDED.items()):
    print(f'superseded  {key[0]} {key[1]}: {why}')
print(f'{len(reads)} cells read, {len(expect)} banked, {len(bad)} refused')
for b in bad:
    print('REFUSED ' + b)
sys.exit(1 if bad else 0)
