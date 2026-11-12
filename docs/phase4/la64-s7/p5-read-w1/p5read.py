"""P5's independent wave reader. Usage: p5read.py SHARD RUN TIP OUTDIR
Downloads both legs' artifacts of RUN (headSha must equal TIP), reads the dump proof, the job steps, and every row's
comparison record, and prints one line per row per RID plus proof and anomaly lines. Writes OUTDIR/<SHARD>.tsv."""
import json, os, re, subprocess, sys, zipfile, io, glob

shard, run, tip, out = sys.argv[1:5]
R = "repos/ritchiecarroll/go2cs"
def api(p):
    return json.loads(subprocess.run(["gh", "api", p], check=True, capture_output=True, text=True).stdout)
def raw(p):
    return subprocess.run(["gh", "api", p], check=True, capture_output=True).stdout

rv = api(f"{R}/actions/runs/{run}")
print(f"RUN {shard} {run} head={rv['head_sha'][:10]} {'at-tip' if rv['head_sha'] == tip else 'NOT-TIP'} {rv['status']} {rv['conclusion']} ref={rv['head_branch']}")
if rv["head_sha"] != tip:
    sys.exit("not evidence: head is not the tip")

for j in api(f"{R}/actions/runs/{run}/jobs?per_page=50")["jobs"]:
    if "sweep-shard" not in j["name"]:
        continue
    steps = [f"{s['name']}={s['conclusion']}" for s in j["steps"] if re.search(r"dump|proof|recheck|sweep", s["name"], re.I)]
    print(f"JOB {j['name']} {j['conclusion']}", "|", "; ".join(steps))

base = os.path.join(out, shard)
os.makedirs(base, exist_ok=True)
legs = {}
for a in api(f"{R}/actions/runs/{run}/artifacts?per_page=100")["artifacts"]:
    m = re.search(r"(linux-x64|linux-arm64)$", a["name"])
    if not m:
        print("ARTIFACT-OTHER", a["name"]); continue
    d = os.path.join(base, m.group(1))
    if not os.path.isdir(d):
        zipfile.ZipFile(io.BytesIO(raw(f"{R}/actions/artifacts/{a['id']}/zip"))).extractall(d)
    legs[m.group(1)] = d

rows = {}
for rid, d in sorted(legs.items()):
    pd = glob.glob(f"{d}/dumpproof-*")
    if not pd:
        print(f"PROOF {rid} NO dumpproof directory");
    else:
        pd = pd[0]
        cen = {os.path.basename(f): sum(1 for l in open(f) if l.strip()) for f in glob.glob(f"{pd}/census-*.txt")}
        def free(n):
            p = f"{pd}/free-{n}.tsv"
            return {l.split("\t")[0]: int(l.split("\t")[1]) for l in open(p) if "\t" in l} if os.path.exists(p) else {}
        rec, ctl, prv = free("record"), free("control"), free("prove")
        drop = max((rec[k] - prv.get(k, rec[k]) for k in rec), default=None)
        cdrop = max((ctl[k] - prv.get(k, ctl[k]) for k in ctl), default=None)
        st = open(f"{pd}/settings.txt").read() if os.path.exists(f"{pd}/settings.txt") else ""
        ul = re.search(r"ulimit -c soft : (\S+)", st)
        print(f"PROOF {rid} census-lines {cen} free drop record->prove {drop} B, control->prove {cdrop} B (bar 8 MiB = 8388608) soft-ulimit {ul.group(1) if ul else '?'} files {sorted(os.listdir(pd))}")
    log = glob.glob(f"{d}/sweep-*.log")
    logt = open(log[0], errors="replace").read() if log else ""
    words = {}
    for m in re.finditer(r"^\s*(PASS|FAIL|CVAC|NOT MEASURED|[A-Z][A-Z -]+?)\s{2,}(\S+)\s+(.*?)\[(\d+)s\]\s*$", logt, re.M):
        words[m.group(2)] = (m.group(1).strip(), m.group(3).strip(), m.group(4))
    for m in re.finditer(r"(?i)not implemented|PlatformNotSupported|NotImplementedException", logt):
        ln = logt[max(0, logt.rfind("\n", 0, m.start())):logt.find("\n", m.end())].strip()
        print(f"NOTIMPL-LOG {rid} {ln[:200]}")
    for cj in glob.glob(f"{d}/rows/src/core/**/go2cs_test_comparison.json", recursive=True):
        c = json.load(open(cj))
        row = c.get("package") or os.path.relpath(os.path.dirname(cj), f"{d}/rows/src/core")
        go, cs = c.get("go") or {}, c.get("csharp") or {}
        matched = sum(1 for k, v in go.items() if cs.get(k) == v)
        divn = sorted(k for k, v in go.items() if cs.get(k) != v)
        und = [e for e in (c.get("errors") or []) if re.search(r"(?i)diverg|mismatch|Go=", str(e))]
        oth = [e for e in (c.get("errors") or []) if e not in und]
        rows.setdefault(row, {})[rid] = dict(go=go, verdicts=len(go), cs=len(cs), matched=matched, disclosed=len(c.get("disclosed") or []),
            new=len(und), other=len(oth), status=c.get("status"), divn=divn, word=words.get(row, ("?", "", ""))[0],
            wall=words.get(row, ("", "", "?"))[2], note=words.get(row, ("", "", ""))[1], oracle=(c.get("environment") or {}).get("oracleGoVersion", ""))
        rj = os.path.join(os.path.dirname(cj), "go2cs_test_results.json")
        if os.path.exists(rj):
            t = open(rj, errors="replace").read()
            n = len(re.findall(r"(?i)not implemented|NotImplementedException|PlatformNotSupportedException", t))
            if n:
                print(f"NOTIMPL-RESULTS {rid} {row} {n} hits")
    for row, w in words.items():
        if row not in rows or rid not in rows[row]:
            rows.setdefault(row, {})[rid] = dict(go={}, verdicts=0, cs=0, matched=0, disclosed=0, new=0, other=0, status="NO-RECORD",
                divn=[], word=w[0], wall=w[2], note=w[1], oracle="")

hdr = "shard\trow\tRID\tverdicts\tcs_verdicts\tmatched\tdisclosed\tnew\tother_errors\tstatus\tword\tnote\tagree\tabsent\tdiff_names\trun\twall_s"
lines = [hdr]
for row in sorted(rows):
    r = rows[row]
    both = len(r) == 2
    for rid in sorted(r):
        x = r[rid]
        o = r.get("linux-arm64" if rid == "linux-x64" else "linux-x64")
        absent = ";".join(sorted(set(o["go"]) - set(x["go"]))) if o else "?"
        agree = "yes" if o and (x["word"], x["verdicts"], x["matched"]) == (o["word"], o["verdicts"], o["matched"]) else "no"
        lines.append("\t".join(map(str, [shard, row, rid, x["verdicts"], x["cs"], x["matched"], x["disclosed"], x["new"], x["other"], x["status"],
            x["word"], x["note"], agree, absent or "-", ";".join(x["divn"][:12]) or "-", run, x["wall"]])))
open(os.path.join(out, f"{shard}.tsv"), "w").write("\n".join(lines) + "\n")
print("\n".join(lines))
print(f"ROWS {shard} {len(rows)} (legs {sorted(legs)})")
