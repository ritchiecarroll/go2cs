"""Seat #53 interleaved A/B driver (G-LAPTOP).

Usage: python -I -X utf8 drive.py <arms.tsv> <rounds> <out.tsv>
  arms.tsv rows: program<TAB>arm<TAB>exe-path   (the arm named 'base' is the reference; 'aa' is its A/A twin)

Per program: one discarded warm-up round, then <rounds> rounds; within a round every arm runs once, in an order that
reverses every other round. Each run records the process wall time (ms, measured here around the whole process) and
every `key: number` line the program prints. Every arm of a program must print the same checksum and exit 0.
Readout per (program, metric): each arm's median [min-max]; the A/A band = per-round aa/base over the rounds; each arm's
per-round ratio to base as median [min-max], its median delta against base in the metric's unit, and that delta as a
percentage of base's median wall time (wall metric only). A ratio whose range is disjoint from the A/A range is OUTSIDE.
"""
import os, re, statistics, subprocess, sys, time

arms_path, rounds, out = sys.argv[1], int(sys.argv[2]), sys.argv[3]
programs = {}
for line in open(arms_path, encoding="utf-8"):
    if line.strip() and not line.startswith("#"):
        cols = line.rstrip("\n").split("\t")
        prog, arm, exe = cols[:3]
        # optional 4th column: K=V;K=V environment for this arm, applied after the tiering variables are stripped
        extra = dict(kv.split("=", 1) for kv in cols[3].split(";")) if len(cols) > 3 and cols[3] else {}
        programs.setdefault(prog, []).append((arm, exe, extra))

env = {k: v for k, v in os.environ.items() if not (k.startswith("DOTNET_Tiered") or k.startswith("DOTNET_TC_") or k.startswith("DOTNET_Jit"))}

def run(exe, extra):
    t0 = time.perf_counter()
    p = subprocess.run([exe], capture_output=True, text=True, env={**env, **extra}, timeout=600, cwd=os.path.dirname(exe))
    wall = (time.perf_counter() - t0) * 1000.0
    vals = {"wall_ms": wall}
    for m in re.finditer(r"^([a-z_]+):\s*(-?[0-9.]+)\s*$", p.stdout, re.M):
        vals[m.group(1)] = float(m.group(2))
    ck = re.search(r"^checksum:\s*(.+)$", p.stdout, re.M)
    return p.returncode, (ck.group(1).strip() if ck else None), vals

def span(v, fmt="{:.3f}"):
    return (fmt + " [" + fmt + "-" + fmt + "]").format(statistics.median(v), min(v), max(v))

with open(out, "w", encoding="utf-8") as f:
    f.write("program\tround\tarm\tmetric\tvalue\trc\n")
    for prog, arms in programs.items():
        data = {a: {} for a, _, _ in arms}
        ref, bad = None, False
        for r in range(rounds + 1):
            for arm, exe, extra in (arms if r % 2 == 0 else list(reversed(arms))):
                rc, ck, vals = run(exe, extra)
                if ref is None:
                    ref = ck
                if rc != 0 or ck is None or ck != ref:
                    bad = True
                    print(f"  BAD {prog} {arm} round {r}: rc={rc} checksum={ck} ref={ref}", flush=True)
                    continue
                if r == 0:
                    continue
                for k, v in vals.items():
                    data[arm].setdefault(k, []).append(v)
                    f.write(f"{prog}\t{r}\t{arm}\t{k}\t{v}\t{rc}\n")
            f.flush()
        if bad:
            print(f"{prog} | MISMATCH", flush=True)
            continue
        base_wall = statistics.median(data["base"]["wall_ms"])
        for metric in data["base"]:
            if metric == "checksum" or 0.0 in data["base"][metric] or 0.0 in data["aa"][metric]:
                continue  # a program that prints a constant 0 (PerfStartup's elapsed_ns) has no ratio to read
            base, aa = data["base"][metric], data["aa"][metric]
            band = [x / y for x, y in zip(aa, base)]
            lo, hi = min(band), max(band)
            print(f"{prog} {metric}: A/A {span(band)}", flush=True)
            for arm, _, _ in arms:
                v = data[arm][metric]
                ratio = [x / y for x, y in zip(v, base)]
                delta = statistics.median(v) - statistics.median(base)
                verdict = "OUTSIDE" if (min(ratio) > hi or max(ratio) < lo) else "inside"
                pct = f" ({100.0 * delta / base_wall:+.1f}% of wall)" if metric == "wall_ms" else ""
                print(f"    {arm:6} {span(v)}  ratio {span(ratio)} {verdict}  delta {delta:+.3f}{pct}", flush=True)
