# wall2.py BASE_EXE CUT_EXE ROUNDS out.json : interleaved wall arms (probe off), rotating order; paired against base.
import hashlib, json, os, statistics, subprocess, sys, time
base, cut, rounds, out = sys.argv[1], sys.argv[2], int(sys.argv[3]), sys.argv[4]
ARMS = {"base": base, "base-AA": base, "cut": cut}
def run(exe):
    env = dict(os.environ); env.pop("GO2CS_B3_PROBE", None)
    t0 = time.perf_counter(); p = subprocess.run([exe], stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, env=env, cwd=os.path.dirname(exe))
    return time.perf_counter() - t0, hashlib.sha256(p.stdout).hexdigest()[:12], p.returncode
for a in ARMS: run(ARMS[a])
wall = {a: [] for a in ARMS}; shas = set(); rcs = set(); names = list(ARMS)
for r in range(rounds):
    for a in names[r % len(names):] + names[:r % len(names)]:
        w, s, rc = run(ARMS[a]); wall[a].append(w); shas.add(s); rcs.add(rc)
def q(v): qs = statistics.quantiles(v, n=4); return qs[0], qs[2]
rep = {"rounds": rounds, "shas": sorted(shas), "rcs": sorted(rcs), "wall": {}, "paired": {}}
for a, v in wall.items(): rep["wall"][a] = {"median": statistics.median(v), "runs": v}
for a in names[1:]:
    d = [x - y for x, y in zip(wall[a], wall["base"])]; q1, q3 = q(d)
    rep["paired"][a] = {"median": statistics.median(d), "q1": q1, "q3": q3, "faster": sum(1 for x in d if x < 0)}
json.dump(rep, open(out, "w"), indent=1)
print("hashes", rep["shas"], "rcs", rep["rcs"])
for a in names: print(f"wall {a:8} median {rep['wall'][a]['median']*1000:7.1f} ms")
for a in names[1:]: p = rep["paired"][a]; print(f"paired {a:8} {p['median']*1000:+7.1f} ms (q1 {p['q1']*1000:+.1f}, q3 {p['q3']*1000:+.1f}), faster {p['faster']}/{rounds}")
