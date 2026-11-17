# bench.py: wall and CPU time of the native Go demos against the converted C# demos, on the same inputs.
#
#   python -I bench.py --bins <dir with {demo}-go, {demo}-cs[, {demo}-aot]> --corpus <corpus dir> --samples <dir>
#                      --units <file of unit indices> --out <dir> [--runs 5] [--warmup 1] [--cases a,b,...]
#
# Each case runs one process per run. Before timing, each side runs --warmup times (file cache, JIT disk caches);
# then --runs timed runs. Per run: wall seconds, user+sys CPU seconds and peak RSS of that process (os.wait4).
# The report is the median of the timed runs, per case and side, and the ratio converted/native of the medians.
# Every run's stdout is hashed: a side whose output differs from native's is reported, because a timing of a wrong
# answer is not a timing.
import argparse, hashlib, json, os, statistics, subprocess, sys, time

ap = argparse.ArgumentParser()
ap.add_argument("--bins", required=True)
ap.add_argument("--corpus", required=True)
ap.add_argument("--samples", required=True)
ap.add_argument("--units", required=True)
ap.add_argument("--out", required=True)
ap.add_argument("--runs", type=int, default=5)
ap.add_argument("--warmup", type=int, default=1)
ap.add_argument("--cases", default="hello,startup,parse,check,emit,mf")
ap.add_argument("--sides", default="go,cs,aot")
A = ap.parse_args()
os.makedirs(A.out, exist_ok=True)
units = [u.strip() for u in open(A.units, encoding="utf-8") if u.strip()]
unit_files = [os.path.join(A.corpus, "units", u + ".txt") for u in units]
CASES = {
    "hello": ("parsedemo", ["good.ts"], A.samples),        # process start plus parsing one small file
    "startup": ("checkdemo", ["good.ts"], A.samples),      # one small program type-checked, lib.d.ts included
    "parse": ("parsedemo", ["-corpus", A.corpus], None),
    "check": ("checkdemo", unit_files, None),
    "emit": ("emitdemo", unit_files, None),
    "mf": ("mfdemo", ["shop", "legacy"], os.path.join(A.samples, "projects")),
}


def run(exe, args, cwd):
    t0 = time.perf_counter()
    p = subprocess.Popen([exe] + args, cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
    out = p.stdout.read()
    _, status, ru = os.wait4(p.pid, 0)
    wall = time.perf_counter() - t0
    return {"wall": round(wall, 3), "cpu": round(ru.ru_utime + ru.ru_stime, 3), "rss_mb": round(ru.ru_maxrss / 1024, 1),
            "rc": os.waitstatus_to_exitcode(status), "sha": hashlib.sha256(out).hexdigest()[:12]}


report = {"units": len(units), "runs": A.runs, "warmup": A.warmup, "cases": {}}
for case in A.cases.split(","):
    demo, args, cwd = CASES[case]
    report["cases"][case] = {}
    for side in A.sides.split(","):
        exe = os.path.join(A.bins, f"{demo}-{side}")
        if not os.path.exists(exe):
            continue
        for _ in range(A.warmup):
            run(exe, args, cwd)
        rs = [run(exe, args, cwd) for _ in range(A.runs)]
        med = {k: statistics.median(r[k] for r in rs) for k in ("wall", "cpu", "rss_mb")}
        report["cases"][case][side] = {"median": med, "runs": rs, "shas": sorted({r["sha"] for r in rs}),
                                       "rcs": sorted({r["rc"] for r in rs})}
        print(f"{case:8} {side:4} wall {med['wall']:9.3f} s  cpu {med['cpu']:9.3f} s  rss {med['rss_mb']:8.1f} MB  "
              f"sha {','.join(report['cases'][case][side]['shas'])}", flush=True)
    c = report["cases"][case]
    if "go" in c:
        for side in c:
            if side != "go":
                c[side]["ratio_wall"] = round(c[side]["median"]["wall"] / max(c["go"]["median"]["wall"], 1e-6), 1)
                c[side]["ratio_cpu"] = round(c[side]["median"]["cpu"] / max(c["go"]["median"]["cpu"], 1e-6), 1)
                c[side]["same_output"] = c[side]["shas"] == c["go"]["shas"]
                print(f"{case:8} {side:4} / go: wall x{c[side]['ratio_wall']}  cpu x{c[side]['ratio_cpu']}  "
                      f"same output {c[side]['same_output']}", flush=True)
    json.dump(report, open(os.path.join(A.out, "bench.json"), "w", encoding="utf-8"), indent=1)
