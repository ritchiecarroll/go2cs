# Extracts the measured per-row walls of the 1.26.10.2 DISCOVERY pass at the freeze tip f6666d98a2 into one table,
# so the re-pass re-cut (repass_cut.py) is reproducible from master after the evidence refs are pruned.
# Usage: py -3 -I discovery_walls.py <repo> <out.tsv>
# Each source is read with `git show <sha>:<path>` at the SHA pinned below: the evidence ref's tip when the re-cut
# was drafted (2026-11-03). A missing object is an error, never a skipped source.
import csv, io, subprocess, sys

repo, out = sys.argv[1], sys.argv[2]

# (os, box, ref, sha, path): the discovery shards. RWSL-2 had not posted, so its rows have no reading here.
SOURCES = [
    ('windows', 'i9', 'claude/i9-sweep-1262b', '7f279ccf35', 'docs/phase4/rebank-1.26.10.2/sweep/i9-S.tsv'),
    ('windows', 'H',  'claude/h-sweep-1262b',  '0d9dbd348f', 'docs/phase4/rebank-1.26.10.2/sweep/H-S.tsv'),
    ('windows', 'G',  'claude/g-sweep-1262',   '13906ec1b4', 'docs/phase4/rebank-1.26.10.2/sweep/G-S.tsv'),
    ('windows', 'R',  'claude/r-sweep-1262b',  'ab383a53e7', 'docs/phase4/rebank-1.26.10.2/sweep/R-S.tsv'),
    ('linux',   'P1', 'claude/p1-linux-1262b', '0b9f48bdec', 'docs/phase4/rebank-1.26.10.2/linux/P1-1.tsv'),
    ('linux',   'P1', 'claude/p1-linux-1262b', '0b9f48bdec', 'docs/phase4/rebank-1.26.10.2/linux/P1-2.tsv'),
    ('linux',   'P2', 'claude/p2-linux-1262b', 'f173721327', 'docs/phase4/rebank-1.26.10.2/linux/P2-1.tsv'),
    ('linux',   'P2', 'claude/p2-linux-1262b', 'f173721327', 'docs/phase4/rebank-1.26.10.2/linux/P2-2.tsv'),
    ('linux',   'R-WSL', 'claude/r-linux-1262b', '9602352d71', 'docs/phase4/rebank-1.26.10.2/linux/RWSL-1.tsv'),
]

# (os, row) -> (the TSV's wall_s, the wall read instead, why): a TSV wall that is not the row's own time. The TSV is the
# kit's record and stays as written; the correction is applied here, named in the source column, and asserted against
# the exact wall it corrects, so a re-read of the row is never silently overridden.
OVERRIDES = {
    # R-LAPTOP slept on battery 11:50-12:37Z with crypto/ecdh in flight: its wall_s includes the suspension. The kit's
    # monotonic elapsed time (2506 s against 5347 s of wall clock) puts the row's own time at about 83 s, and no other
    # row spans the gap (R's post b9389661d6; COORD's re-cut input, 2026-11-03).
    ('linux', 'crypto/ecdh'): ('2924', '83', 'host suspension 11:50-12:37Z, R post b9389661d6'),
}
applied = set()

seen = set()
with open(out, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('os\trow\tbox\twall_s\tword\tsource\n')
    for goos, box, ref, sha, path in SOURCES:
        text = subprocess.run(['git', '-C', repo, 'show', f'{sha}:{path}'], capture_output=True, check=True).stdout.decode('utf-8')
        n = 0
        for r in csv.DictReader(io.StringIO(text), delimiter='\t'):
            key = (goos, r['row'])
            assert key not in seen, f'{key} read twice'
            seen.add(key)
            wall = r['wall_s'].strip()
            assert wall.isdigit(), f'{path}: {r["row"]} has no numeric wall_s ({wall!r})'
            note = ''
            if key in OVERRIDES:
                was, wall, why = OVERRIDES[key]
                assert r['wall_s'].strip() == was, f'{key}: the override corrects wall_s {was}, but {path} reads {r["wall_s"]!r}'
                note = f'; wall_s {was} read as {wall} ({why})'
                applied.add(key)
            fh.write(f'{goos}\t{r["row"]}\t{box}\t{wall}\t{r["word"]}\t{ref}@{sha}:{path.rsplit("/", 1)[1]}{note}\n')
            n += 1
        print(f'{ref}@{sha} {path.rsplit("/", 1)[1]}: {n} rows', file=sys.stderr)
print(f'{len(seen)} rows written', file=sys.stderr)
for key in sorted(set(OVERRIDES) - applied):
    print(f'override {key} not applied: no source reads that row yet', file=sys.stderr)
