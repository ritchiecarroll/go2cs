# typecheck_slice.py --native <checkdemo-go.exe> --converted <checkdemo.exe> --corpus <dir> --out <dir> [--every 37] [--batch 20]
# The TS type-check slice: every Nth single-file .ts unit from tests/cases/compiler in the parse corpus's manifest, run
# through checkdemo (each file is type-checked as /main.ts in its own program, with the bundled libs) on both sides, in
# batches of --batch files per process. Each side's output is split into per-file blocks at the "file <name>" lines and
# compared block by block. A batch whose converted process dies (nonzero rc or missing blocks) has its missing units
# re-run one at a time, so a crash is attributed to the unit that crashes. Writes <out>/slice.json and the mismatches.
import argparse, json, os, re, subprocess, sys, time

sys.stdout.reconfigure(encoding="utf-8")
ap = argparse.ArgumentParser()
for a in ("native", "converted", "corpus", "out"):
    ap.add_argument("--" + a, required=True)
ap.add_argument("--every", type=int, default=37)
ap.add_argument("--batch", type=int, default=20)
ap.add_argument("--timeout", type=int, default=3600)
A = ap.parse_args()
os.makedirs(os.path.join(A.out, "mismatch"), exist_ok=True)

rows = [r.split("\t") for r in open(os.path.join(A.corpus, "manifest.tsv"), encoding="utf-8").read().rstrip("\n").split("\n")]
pool = [r for r in rows if r[1] == ".ts" and r[5].startswith("tests/cases/compiler/")]
units = pool[::A.every]
units_dir = os.path.join(A.corpus, "units")


def run(exe, idxs):
    files = [os.path.join(units_dir, i + ".txt") for i in idxs]
    t = time.time()
    try:
        p = subprocess.run([exe] + files, capture_output=True, timeout=A.timeout)
        rc, out, err = p.returncode, p.stdout, p.stderr
    except subprocess.TimeoutExpired as e:
        rc, out, err = "timeout", e.stdout or b"", e.stderr or b""
    return rc, out.decode("utf-8", "replace"), err.decode("utf-8", "replace"), time.time() - t


def blocks(text):
    got, cur, buf = {}, None, []
    for line in text.splitlines(keepends=True):
        m = re.match(r"file (\d{5})\.txt \(", line)
        if m:
            if cur:
                got[cur] = "".join(buf)
            cur, buf = m[1], []
        buf.append(line)
    if cur:
        got[cur] = "".join(buf)
    return got


res, secs = {}, {"go": 0.0, "cs": 0.0}
# Resume and pause (2026-11-11, for the full 7,404-unit pass): every finished batch is appended to <out>/units.jsonl and
# reloaded on a restart; while <out>/PAUSE exists, the run waits between batches (another battery may need the box).
done_path, pause_path = os.path.join(A.out, "units.jsonl"), os.path.join(A.out, "PAUSE")
if os.path.exists(done_path):
    for line in open(done_path, encoding="utf-8"):
        u = json.loads(line)
        res[u["index"]] = (u["go"], u["cs"])
# 2026-11-12: seconds are carried across resumes in <out>/seconds.json, so a resumed pass reports its whole run time
# (before this, a resumed pass reported only the last session's seconds).
secs_path = os.path.join(A.out, "seconds.json")
if os.path.exists(secs_path):
    secs.update(json.load(open(secs_path, encoding="utf-8")))
for k in range(0, len(units), A.batch):
    idxs = [u[0] for u in units[k:k + A.batch]]
    if all(i in res for i in idxs):
        continue
    while os.path.exists(pause_path):
        time.sleep(30)
    grc, gout, _, gs = run(A.native, idxs); secs["go"] += gs
    crc, cout, cerr, cs = run(A.converted, idxs); secs["cs"] += cs
    g, c = blocks(gout), blocks(cout)
    # the last converted block of a dead process may be cut off: re-run it, and every missing unit, alone
    redo = [i for i in idxs if i not in c] + ([max(c)] if crc != 0 and c else [])
    for i in dict.fromkeys(redo):
        urc, uout, uerr, us = run(A.converted, [i]); secs["cs"] += us
        ub = blocks(uout)
        c[i] = ub.get(i, "") if urc == 0 else f"CRASH rc={urc}\n{ub.get(i, '')}\n--- stderr\n{uerr[-3000:]}"
    # 2026-11-11: the NATIVE side gets the same attribution. A native process that dies (Go's own panic on a unit) loses the
    # units after it, which the first full pass misread as 11 "native missing" units; all 11 match when run alone.
    gredo = [i for i in idxs if i not in g] + ([max(g)] if grc != 0 and g else [])
    for i in dict.fromkeys(gredo):
        urc, uout, uerr, us = run(A.native, [i]); secs["go"] += us
        ub = blocks(uout)
        g[i] = ub.get(i, "") if urc == 0 else f"CRASH rc={urc}\n{ub.get(i, '')}"
        if urc != 0 and i in c and c[i].startswith("CRASH") and c[i].split("\n", 1)[0] == f"CRASH rc={urc}" and \
                c[i].split("\n")[1:2] == g[i].split("\n")[1:2]:
            c[i] = g[i]                         # both sides die the same way on this unit: Go's own behaviour, reproduced
    with open(done_path, "a", encoding="utf-8") as jf:
        for i in idxs:
            res[i] = (g.get(i), c.get(i))
            jf.write(json.dumps({"index": i, "go": res[i][0], "cs": res[i][1]}) + "\n")
    json.dump(secs, open(secs_path, "w", encoding="utf-8"))
    print(f"batch {k // A.batch + 1}: {sum(1 for i in idxs if res[i][0] is not None and res[i][0] == res[i][1])} of {len(idxs)} match", flush=True)

meta = {r[0]: {"source": r[5], "unit": r[6] if len(r) > 6 else ""} for r in rows}
mism = []
for i, (g, c) in res.items():
    if g is not None and g == c:
        continue
    kind = "converted crashed" if (c or "").startswith("CRASH") else ("native missing" if g is None else "output differs")
    first = next((n for n, (a, b) in enumerate(zip((g or "").splitlines(), (c or "").splitlines())) if a != b), None)
    gl, cl = (g or "").splitlines(), (c or "").splitlines()
    mism.append({"index": i, "class": kind, **meta[i], "first_diff_line": first,
                 "go": gl[first] if first is not None and first < len(gl) else "", "cs": cl[first] if first is not None and first < len(cl) else (c or "")[:300]})
    open(os.path.join(A.out, "mismatch", f"{i}.go.txt"), "w", encoding="utf-8").write(g or "")
    open(os.path.join(A.out, "mismatch", f"{i}.cs.txt"), "w", encoding="utf-8").write(c or "")
matched = len(res) - len(mism)
out = {"pool": len(pool), "every": A.every, "units": len(res), "matched": matched, "classes": {},
       "seconds": {k: round(v, 1) for k, v in secs.items()}, "mismatches": mism}
for m in mism:
    out["classes"][m["class"]] = out["classes"].get(m["class"], 0) + 1
json.dump(out, open(os.path.join(A.out, "slice.json"), "w", encoding="utf-8"), indent=1)
print(f"type-check slice: {matched} of {len(res)} match (every {A.every}th of {len(pool)} units); classes {out['classes']}; seconds {out['seconds']}")
sys.exit(0 if matched == len(res) else 1)
