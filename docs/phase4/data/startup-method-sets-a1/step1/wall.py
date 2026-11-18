# wall.py EXE ROUNDS out.json : interleaved wall arms (probe off), rotating order; paired medians against base.
import hashlib, json, os, statistics, subprocess, sys, time
exe, rounds, out = sys.argv[1], int(sys.argv[2]), sys.argv[3]
ARMS = {"base": {}, "base-AA": {}, "keys": {"GO2CS_B3_KEYS": "1"}, "keys+box": {"GO2CS_B3_KEYS": "1", "GO2CS_B3_BOXKEYS": "1"}}
def run(extra):
    env = dict(os.environ); env.update(extra); env.pop("GO2CS_B3_PROBE", None)
    t0 = time.perf_counter(); p = subprocess.run([exe], stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, env=env)
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
    rep["paired"][a] = {"median": statistics.median(d), "q1": q1, "q3": q3}
json.dump(rep, open(out, "w"), indent=1)
print("hashes", rep["shas"], "rcs", rep["rcs"])
for a in names: print(f"wall {a:9} median {rep['wall'][a]['median']*1000:7.1f} ms")
for a in names[1:]: p = rep["paired"][a]; print(f"paired {a:9} {p['median']*1000:+7.1f} ms (q1 {p['q1']*1000:+.1f}, q3 {p['q3']*1000:+.1f})")
