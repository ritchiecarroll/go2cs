# p5-b3s-run.py <exe> <rounds> <probe-rounds> <out.json>: interleaved arms of the B3 sizing on one binary.
# Wall rounds run with the probe off; probe rounds read the probe's inclusive ms per site. Arm order rotates.
import hashlib, json, os, re, statistics, subprocess, sys, time
exe, rounds, prounds, out = sys.argv[1], int(sys.argv[2]), int(sys.argv[3]), sys.argv[4]
ARMS = {"base": {}, "base-AA": {}, "memo": {"GO2CS_B3_MEMO": "1"}, "keys": {"GO2CS_B3_KEYS": "1"}}
PARMS = dict(ARMS); PARMS["base-gen0"] = {"DOTNET_GCgen0size": "10000000"}
def run(extra, probe):
    env = dict(os.environ); env.update(extra)
    if probe: env["GO2CS_B3_PROBE"] = "1"
    t0 = time.perf_counter()
    p = subprocess.Popen([exe], stdout=subprocess.PIPE, stderr=subprocess.PIPE if probe else subprocess.DEVNULL, env=env)
    o, e = p.communicate()
    return time.perf_counter() - t0, hashlib.sha256(o).hexdigest()[:12], (e or b"").decode("utf-8", "replace"), p.returncode
for a in ARMS: run(ARMS[a], False)
wall = {a: [] for a in ARMS}; shas = set(); rcs = set()
names = list(ARMS)
for r in range(rounds):
    order = names[r % len(names):] + names[:r % len(names)]
    for a in order:
        w, s, _, rc = run(ARMS[a], False); wall[a].append(w); shas.add(s); rcs.add(rc)
sites = {a: {} for a in PARMS}; items = {a: {} for a in PARMS}
pn = list(PARMS)
for r in range(prounds):
    order = pn[r % len(pn):] + pn[:r % len(pn)]
    for a in order:
        _, s, e, rc = run(PARMS[a], True); shas.add(s); rcs.add(rc)
        for line in e.splitlines():
            m = re.match(r"B3PROBE ([^\t]+)\t(\d+)\t([\d.]+)\t(\d+)$", line)
            if m:
                sites[a].setdefault(m.group(1), []).append(float(m.group(3)))
                items[a].setdefault(m.group(1), set()).add((int(m.group(2)), int(m.group(4))))
rep = {"rounds": rounds, "probe_rounds": prounds, "shas": sorted(shas), "rcs": sorted(rcs),
       "wall": {a: {"median": statistics.median(v), "q1": statistics.quantiles(v, n=4)[0], "q3": statistics.quantiles(v, n=4)[2], "runs": v} for a, v in wall.items()},
       "paired": {a: {"median": statistics.median(x - y for x, y in zip(wall[a], wall["base"])),
                      "q1": statistics.quantiles([x - y for x, y in zip(wall[a], wall["base"])], n=4)[0],
                      "q3": statistics.quantiles([x - y for x, y in zip(wall[a], wall["base"])], n=4)[2]} for a in ARMS if a != "base"},
       "sites": {a: {k: {"median": statistics.median(v), "min": min(v), "max": max(v), "calls_items": sorted(items[a][k])} for k, v in d.items()} for a, d in sites.items()}}
json.dump(rep, open(out, "w"), indent=1)
print("output hashes", rep["shas"], "rcs", rep["rcs"])
for a in ARMS: print(f"wall {a:8} median {rep['wall'][a]['median']*1000:7.1f} ms  q1 {rep['wall'][a]['q1']*1000:7.1f}  q3 {rep['wall'][a]['q3']*1000:7.1f}")
for a, d in rep["paired"].items(): print(f"paired {a:8} - base: median {d['median']*1000:+7.1f} ms  q1 {d['q1']*1000:+7.1f}  q3 {d['q3']*1000:+7.1f}")
keys = ["CollectGoMethodSetCandidates", "GetExtensionMethodsHomedIn", "HomedIn.call#1 (read)", "HomedIn.call#2 (read)", "HomedIn.call#2 (memo)", "HomedIn.pass call#1", "HomedIn.pass call#2", "HomedIn.walk", "LoadAssemblyExtensionHosts", "ExtensionHost.Methods(read)", "ExtensionHost.Type(resolve)", "Collect Kind", "Collect *Kind", "AdapterBinder.BuildFactory", "StructurallyImplements"]
print("site medians (min..max) ms; calls,items")
for k in keys:
    row = []
    for a in PARMS:
        d = rep["sites"][a].get(k)
        row.append(f"{a}: {d['median']:.1f} ({d['min']:.1f}..{d['max']:.1f}) {d['calls_items']}" if d else f"{a}: -")
    print(f"  {k}\n     " + "\n     ".join(row))
