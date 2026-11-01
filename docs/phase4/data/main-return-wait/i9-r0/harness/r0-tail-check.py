"""r0-tail-check.py -- for each loaded R0 arm: how many of its runs ended after its spinner had stopped.

The spinner (spin.exe N 600) runs 600 s; an arm that outlasts it runs its tail unloaded. The spinner's last live
moment is its last affinity sample (2 s cadence) inside the arm's window; a run is LOADED if its stdout file was last
written at or before that sample + 2 s. Prints arm, mode, N, loaded, unloaded, the spinner's last sample, and the
marked runs with each one's state: the panic test's (`pad`) exit-0 runs, or the recovering test's (`rec`) runs whose
stdout holds "recovered".
"""
import datetime, os, re, sys

P = r"C:\go2cs-build\logs\pado"
day = "2026-10-31"
arms = []
for line in open(os.path.join(P, "r0", "run.log"), encoding="utf-8"):
    m = re.match(r"(\S+ \S+) ARM (\S+) (\S+) N=(\d+) (START|END)", line)
    if m:
        t = datetime.datetime.strptime(m.group(1), "%Y-%m-%d %H:%M:%S")
        if m.group(5) == "START":
            arms.append({"label": m.group(2), "mode": m.group(3), "n": int(m.group(4)), "start": t})
        else:
            arms[-1]["end"] = t
spin = []
for line in open(os.path.join(P, "r0", "affinity.tsv"), encoding="utf-8"):
    c = line.rstrip("\n").split("\t")
    if len(c) >= 2 and c[1].lower().startswith("spin"):
        spin.append(datetime.datetime.strptime(day + " " + c[0], "%Y-%m-%d %H:%M:%S"))
rows = {}
for line in open(os.path.join(P, "rows.tsv"), encoding="utf-8"):
    c = line.rstrip("\n").split("\t")
    if len(c) >= 4:
        rows[(c[0], c[1], c[2])] = c[3]
print("arm\tmode\tN\tloaded\tunloaded\tspinner_last\tmarked_runs")
for a in arms:
    if a["mode"] == "idle":
        continue
    s = [t for t in spin if a["start"] <= t <= a["end"]]
    last = max(s) if s else None
    w = os.path.join(P, "work", a["label"] + "-" + a["mode"])
    loaded = unloaded = 0
    e0 = []
    for i in range(1, a["n"] + 1):
        mt = datetime.datetime.fromtimestamp(os.path.getmtime(os.path.join(w, f"out-{i}.txt")))
        ok = last is not None and mt <= last + datetime.timedelta(seconds=2)
        loaded += ok
        unloaded += not ok
        rec = "-rec-" in a["label"]
        if (not rec and rows.get((a["label"], a["mode"], str(i))) == "0") or \
           (rec and "recovered" in open(os.path.join(w, f"out-{i}.txt"), encoding="utf-8", errors="replace").read()):
            e0.append(f"{i}:{'loaded' if ok else 'UNLOADED'}")
    print(f"{a['label']}\t{a['mode']}\t{a['n']}\t{loaded}\t{unloaded}\t{last.time() if last else None}\t{','.join(e0)}")
