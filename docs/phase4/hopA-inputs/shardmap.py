"""Compute the hop-era fleet shard map from JOB-007 per-row sweep wall times.

Method: PLAN-hop-campaign.md section 4.3 -- reserved set pinned to the i9, remaining
rows LPT-greedy across W bins weighted by provisional speed factors s_w (i9 = 1.00).
"""
import re
import statistics
from pathlib import Path

# Repo-relative off this file (docs/phase4/hopA-inputs/ -> docs/phase4/), never a clone path --
# a hardcoded clone root ran on exactly one machine (found by the i9 on first cross-box use).
DATA = Path(__file__).resolve().parent.parent / "DATA-sweep-row-walltimes.md"

# ---------------------------------------------------------------- parse
text = DATA.read_text(encoding="utf-8")
# Take the first fenced block (the JOB-007 windows table)
block = re.search(r"```\n(.*?)```", text, re.S).group(1)
rows = []
for line in block.strip().splitlines():
    m = re.match(r"^(\S+)\s+(\d+)\s+(\d+)s\s*$", line)
    if not m:
        raise SystemExit(f"unparsed line: {line!r}")
    rows.append((m.group(1), int(m.group(2)), int(m.group(3))))

assert len(rows) == 162, f"expected 162 rows, parsed {len(rows)}"
total = sum(t for _, _, t in rows)
verdicts = sum(v for _, v, _ in rows)
times = sorted(t for _, _, t in rows)

print(f"rows parsed:      {len(rows)}")
print(f"total verdicts:   {verdicts}")
print(f"total i9-seconds: {total}  ({total/60:.1f} min)")
print(f"median row:       {statistics.median(times)} s")
print(f"mean row:         {total/len(rows):.1f} s")
p = lambda q: times[min(len(times)-1, int(q*len(times)))]
print(f"p75: {p(0.75)} s   p90: {p(0.90)} s   p95: {p(0.95)} s")

# distribution buckets
buckets = [(0,10),(11,30),(31,60),(61,120),(121,300),(301,10**9)]
print("\ndistribution:")
for lo,hi in buckets:
    n = sum(1 for t in times if lo <= t <= hi)
    s = sum(t for t in times if lo <= t <= hi)
    label = f"{lo}-{hi}s" if hi < 10**9 else f">{lo-1}s"
    print(f"  {label:>10}: {n:3d} rows, {s:5d} s  ({100*s/total:.1f}% of wall)")

top10 = sorted(rows, key=lambda r: -r[2])[:10]
print("\ntop-10 heaviest rows:")
t10sum = 0
for name, v, t in top10:
    t10sum += t
    print(f"  {name:40s} {v:6d} verdicts  {t:5d} s")
print(f"  top-10 sum: {t10sum} s = {100*t10sum/total:.1f}% of total wall")

# ---------------------------------------------------------------- fleet
# Provisional speed factors (i9 = 1.00) -- PLACEHOLDERS pending hop-recon
# calibration; LANES.md marks historical cross-machine ratios SUSPECT.
MACHINES = {
    "i9-13900K (sweeper)":       1.00,
    "6850U R (R-LAPTOP)":  0.45,
    "i7-5820K (coordinator)":    0.35,
    "6650U G (G-LAPTOP)": 0.35,
    "X (5th engaged machine)":   0.35,   # placeholder silicon, placeholder factor
}
FLEETS = {
    3: ["i9-13900K (sweeper)", "6850U R (R-LAPTOP)", "i7-5820K (coordinator)"],
    4: ["i9-13900K (sweeper)", "6850U R (R-LAPTOP)", "i7-5820K (coordinator)",
        "6650U G (G-LAPTOP)"],
    5: ["i9-13900K (sweeper)", "6850U R (R-LAPTOP)", "i7-5820K (coordinator)",
        "6650U G (G-LAPTOP)", "X (5th engaged machine)"],
}

# The reserved set is TWO ideas, and only one of them is this script's to decide:
#   1. The $longTimeouts floor packages -- DERIVED from run-validated-sweep.ps1 AT GENERATION
#      TIME, never copied. A copied list drifted twice in the map's short life (crypto/tls
#      joined the table, two floors moved) before this derivation replaced it; whatever the
#      sweep script floors when the map is emitted is what gets pinned.
#   2. BIG_ROWS -- rows pinned for raw wall time rather than a timeout floor. An explicit,
#      visible editorial choice, kept separate so nobody mistakes it for the derived half.
import re as _re, os as _os
_sweep = _os.path.join(_os.path.dirname(_os.path.abspath(__file__)),
                       "..", "..", "..", "src", "run-validated-sweep.ps1")
_m = _re.search(r"\$longTimeouts\s*=\s*@\{(.*?)\}",
                open(_sweep, encoding="utf-8").read(), _re.S)
assert _m, f"cannot derive the reserved set: no $longTimeouts table in {_sweep}"
_floors = _re.findall(r"'([^']+)'\s*=\s*'[^']+'", _m.group(1))
assert _floors, f"$longTimeouts parsed empty from {_sweep} -- the pattern is stale, fix it here"
BIG_ROWS = ["go/doc/comment", "go/types"]
RESERVED = _floors + [b for b in BIG_ROWS if b not in _floors]
print(f"reserved set derived at generation time: {len(_floors)} floor rows "
      f"({', '.join(_floors)}) + {len(BIG_ROWS)} big rows")
byname = {n: (v, t) for n, v, t in rows}
for r in RESERVED:
    assert r in byname, f"reserved row {r} not in dataset"
reserved_rows = [(n, byname[n][0], byname[n][1]) for n in RESERVED]
reserved_total = sum(t for _, _, t in reserved_rows)
bulk = [r for r in rows if r[0] not in RESERVED]
bulk.sort(key=lambda r: (-r[2], r[0]))          # DESC by t, name tiebreak -> deterministic
assert len(rows) == len(RESERVED) + len(bulk)   # checksum |rows| == |R| + |B|
print(f"\nreserved set: {len(RESERVED)} rows, {reserved_total} s "
      f"({reserved_total/60:.1f} min) pinned to the i9")
print(f"bulk set:     {len(bulk)} rows, {total-reserved_total} s")

C_TARGET = 90 * 60  # ~90-minute shard target, local wall seconds

def lpt(W):
    names = FLEETS[W]
    s = {m: MACHINES[m] for m in names}
    load = {m: 0.0 for m in names}          # i9-seconds
    pkgs = {m: [] for m in names}
    # step 2: reserved pinned to the i9
    i9 = names[0]
    for n, v, t in reserved_rows:
        load[i9] += t
        pkgs[i9].append((n, t, True))
    # step 4: LPT-greedy -- largest row to the bin with smallest projected LOCAL time
    for n, v, t in bulk:
        target = min(names, key=lambda m: (load[m] / s[m], m))
        load[target] += t
        pkgs[target].append((n, t, False))
    return names, s, load, pkgs

def fmt_hm(sec):
    return f"{sec/60:.1f} min"

for W in (3, 4, 5):
    names, s, load, pkgs = lpt(W)
    makespan = max(load[m] / s[m] for m in names)
    print(f"\n{'='*100}\nW = {W}   makespan = {makespan:.0f} s local = {fmt_hm(makespan)}")
    for m in names:
        local = load[m] / s[m]
        shards = max(1, -(-load[m] // (s[m] * C_TARGET)))  # ceil
        print(f"\n  {m}  (s_w={s[m]:.2f})  rows={len(pkgs[m])}  "
              f"load={load[m]:.0f} i9-s  local={local:.0f} s ({fmt_hm(local)})  "
              f"shards@90min={int(shards)}")
        # compact package list, reserved marked *
        items = [f"{n}{'*' if r else ''}[{t}]" for n, t, r in pkgs[m]]
        line = "    "
        for it in items:
            if len(line) + len(it) > 118:
                print(line.rstrip(", "))
                line = "    "
            line += it + ", "
        print(line.rstrip(", "))
    # checksum
    n_assigned = sum(len(pkgs[m]) for m in names)
    assert n_assigned == 162, n_assigned
    print(f"\n  checksum: {n_assigned} rows assigned == 7 reserved + {len(bulk)} bulk")

# ---------------------------------------------------------------- sensitivity
print(f"\n{'='*100}\nSENSITIVITY (W=4): makespan vs. speed-factor perturbations")
import itertools
def makespan_with(factors):
    saved = dict(MACHINES)
    MACHINES.update(factors)
    try:
        names, s, load, pkgs = lpt(4)
        return max(load[m] / s[m] for m in names)
    finally:
        MACHINES.update(saved)

base = makespan_with({})
print(f"  base (i9=1.00, R=0.45, i7=0.35, G=0.35): {base:.0f} s = {fmt_hm(base)}")
scenarios = {
    "slow laptops (R=0.35, G=0.25)": {"6850U R (R-LAPTOP)": 0.35, "6650U G (G-LAPTOP)": 0.25},
    "slow coordinator (i7=0.25)":    {"i7-5820K (coordinator)": 0.25},
    "fast laptops (R=0.55, G=0.45)": {"6850U R (R-LAPTOP)": 0.55, "6650U G (G-LAPTOP)": 0.45},
    "everything slow (R=0.35, i7=0.25, G=0.25)": {"6850U R (R-LAPTOP)": 0.35,
        "i7-5820K (coordinator)": 0.25, "6650U G (G-LAPTOP)": 0.25},
    "i9 degraded 20% (i9=0.80)":     {"i9-13900K (sweeper)": 0.80},
}
for label, f in scenarios.items():
    ms = makespan_with(f)
    print(f"  {label:45s}: {ms:.0f} s = {fmt_hm(ms)}  ({100*(ms-base)/base:+.0f}%)")

# lower bounds
print("\nlower bounds:")
print(f"  i9 reserved-set floor (serial on i9): {reserved_total} s = {fmt_hm(reserved_total)}")
for W in (3,4,5):
    cap = sum(MACHINES[m] for m in FLEETS[W])
    ideal = total / cap
    print(f"  W={W} perfect-balance bound (total/sum s_w = {total}/{cap:.2f}): "
          f"{ideal:.0f} s = {fmt_hm(ideal)}")
print(f"  single-row floor on a 0.35 box: crypto/dsa 1317/0.35 = {1317/0.35:.0f} s "
      f"= {fmt_hm(1317/0.35)} (why the reserved pin matters)")
