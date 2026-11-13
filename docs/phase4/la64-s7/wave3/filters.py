"""Write wave 3's two row filters (a copy of wave2/filters.py; only the shard table differs: wave 3 names no row) from C1's macOS shard lists (C1-N is the bank plan's A-N).

Usage: filters.py DARWIN_DIR OUTDIR
  DARWIN_DIR  docs/phase4/rebank-1.26.10.3/darwin (the C1-N.tsv files; columns `shard` and `row`)
  OUTDIR      where the A-N.filter files go

A filter is the shard's roster package names, comma-separated, in the list's own order, with no line end.
"""
import csv
import io
import sys

darwin, out = sys.argv[1], sys.argv[2]
wave = {"A-4": 20, "A-1": 20}
named = {"A-4": [], "A-1": []}
seen = set()

for a, n in wave.items():
    shard = "C1-" + a.split("-")[1]
    rows = []

    with io.open(f"{darwin}/{shard}.tsv", encoding="utf-8", newline="") as f:
        for rec in csv.DictReader(f, delimiter="\t"):
            if rec["shard"] != shard:
                sys.exit(f"{shard}.tsv: a row of shard {rec['shard']}")

            if rec["row"] not in rows:
                rows.append(rec["row"])

    missing = [r for r in named[a] if r not in rows]

    if len(rows) != n or missing or seen & set(rows):
        sys.exit(f"{a}: {len(rows)} rows (plan {n}), named rows missing {missing}, shared {sorted(seen & set(rows))}")

    seen |= set(rows)

    with io.open(f"{out}/{a}.filter", "w", encoding="utf-8", newline="") as f:
        f.write(",".join(rows))

    print(a, n, "rows")

print("total", len(seen))
