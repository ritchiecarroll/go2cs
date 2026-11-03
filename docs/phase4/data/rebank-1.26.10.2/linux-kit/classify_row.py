# classify_row.py -- the linux re-bank kit's per-row word (rebank-linux.sh calls it after each -tests invocation;
# PLAN-rebank-go126 §12 R block / §3.2 linux act). The rwsl-dry2.sh heredoc, moved to a file unchanged, plus one rule
# ruled in by COORD (2026-11-02 04:15Z, kit change 4): a PASS whose -tests invocation exited non-zero reads PASS?rc,
# which does not bank.
#   python3 -I classify_row.py <row> <rc> <started epoch> <evidence dir> <output dir> <seconds> <timeout> <deadline>
# Prints one TSV line: row, word, verdicts, matched, disclosed, diverged, sweep_s, wall_s, rc, timeout, deadline.
import json, os, re, sys
row, rc, started, ev, out, secs, tmo, dl = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), sys.argv[4], sys.argv[5], sys.argv[6], sys.argv[7], sys.argv[8]
log = open(os.path.join(ev, "run.log"), encoding="utf-8", errors="replace").read()
cmp_src, res_src = os.path.join(out, "go2cs_test_comparison.json"), os.path.join(out, "go2cs_test_results.json")
doc, stale = None, False
if os.path.exists(cmp_src):
    if os.path.getmtime(cmp_src) < started:
        stale = True
    else:
        try:
            doc = json.load(open(cmp_src, encoding="utf-8"))
        except Exception:
            doc = None
timed_out = False
if os.path.exists(res_src) and os.path.getsize(res_src) > 0 and os.path.getmtime(res_src) >= started:
    timed_out = re.search(r'"action"\s*:\s*"timeout"', open(res_src, encoding="utf-8", errors="replace").read()) is not None
m = re.findall(r"Validated (\d+) tests against go test", log)
summary = int(m[-1]) if len(m) == 1 else None
diverged, matched, disclosed = "n/a", "", ""
if rc != 0 and re.search(r"Conversion failed|unresolved dynamic", log):
    word = "CONVERT"
elif rc != 0 and re.search(r"error CS[0-9]+", log):
    word = "BUILD"
elif timed_out:
    word = "TIMEOUT"
elif stale or doc is None:
    word = "NOVERDICT"
else:
    go, cs = doc.get("go") or {}, doc.get("csharp") or {}
    names = set()
    for s in doc.get("disclosed") or []:
        t = re.match(r"(\S+)", s or "")
        if t:
            names.add(t.group(1))
    d = sum(1 for n in set(go) | set(cs) if n not in names and go.get(n, "") != cs.get(n, ""))
    diverged = d
    word = "DIVERGED" if d > 0 else "PASS"
    matched = str(doc.get("matched"))
    disclosed = len(doc.get("disclosed") or [])
    if summary is None:
        summary = len(go) - len(doc.get("disclosed") or [])
# Kit change 4: no undisclosed divergence is not enough; the invocation itself must have exited 0.
if word == "PASS" and rc != 0:
    word = "PASS?rc"
verdicts = "NOMATCH" if word == "NOVERDICT" or summary is None else summary
print("\t".join(str(x) for x in (row, word, verdicts, matched, disclosed, diverged, secs, secs, rc, tmo, dl)))
