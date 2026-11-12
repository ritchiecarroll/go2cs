"""Write wave 1's seven row filters from C1's macOS shard lists (C1-N is the bank plan's A-N).

Usage: filters.py DARWIN_DIR OUTDIR
  DARWIN_DIR  docs/phase4/rebank-1.26.10.3/darwin (the C1-N.tsv files; columns `shard` and `row`)
  OUTDIR      where the A-N.filter files go

A filter is the shard's roster package names, comma-separated, in the list's own order, with no line end.
"""
import csv
import io
import sys

darwin, out = sys.argv[1], sys.argv[2]
wave1 = {"A-13": 1, "A-9": 19, "A-12": 19, "A-7": 19, "A-8": 19, "A-6": 19, "A-2": 20}
named = {
    "A-13": ["runtime"], "A-9": ["math", "math/big"], "A-12": ["syscall", "time"],
    "A-7": ["internal/cpu", "internal/runtime/atomic", "internal/poll"],
    "A-8": ["internal/runtime/gc/scan", "internal/runtime/sys", "internal/syscall/unix"],
    "A-6": ["hash/crc32", "internal/abi", "internal/buildcfg", "internal/chacha8rand"],
    "A-2": ["crypto/internal/fips140/subtle"],
}
seen = set()

for a, n in wave1.items():
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
