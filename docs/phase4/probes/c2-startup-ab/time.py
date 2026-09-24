# usage: time.py <scratch dir holding <arm>-<program>/jit> <program> <N> <out>
# Start-up: wall time exec->exit under two JIT regimes, arms interleaved (order alternating), 3 warm-ups,
# N measured. Arms (revision 4):
#   A    today (golib carries the probe table unused; the operator is not wired; nothing registers)
#   B    eager: the operator consults the table, one [ModuleInitializer] per module registers its literals
#   Boff B with GO2CS_LITTABLE_OFF=1 (the initializers run, Register returns at once)
#   L    hybrid lazy: each module registers only its range and a registrar; a miss inside it runs it once
#   Lh   L with GO2CS_LITTABLE_HELPER=1 (the registrar runs on a pre-created worker thread)
# Reported per arm: median, p25-p75, min; and each arm's delta against A at the median and at the min.
import subprocess, time, os, sys, statistics as st

W, prog, n, out = sys.argv[1], sys.argv[2], int(sys.argv[3]), sys.argv[4]
arms = {
    'A': ('A', {}), 'B': ('B', {}), 'Boff': ('B', {'GO2CS_LITTABLE_OFF': '1'}),
    'L': ('L', {}), 'Lh': ('L', {'GO2CS_LITTABLE_HELPER': '1'}),
}

def q(v, p):
    v = sorted(v)
    k = (len(v) - 1) * p
    f = int(k)
    return v[f] + (v[min(f + 1, len(v) - 1)] - v[f]) * (k - f)

with open(out, 'a') as o:
    for regime, renv in (('tiered', {}), ('tc0', {'DOTNET_TieredCompilation': '0'})):
        ts = {k: [] for k in arms}
        order = list(arms)
        for i in range(n + 3):
            for k in (order if i % 2 == 0 else order[::-1]):
                arm, e = arms[k]
                cmd = ['dotnet', f"{W}/{arm}-{prog}/jit/{prog}.dll"]
                t0 = time.perf_counter()
                r = subprocess.run(cmd, env={**os.environ, **renv, **e}, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
                t = (time.perf_counter() - t0) * 1000
                assert r.returncode == 0, (k, regime)
                if i >= 3:
                    ts[k].append(t)
        a_med, a_min = st.median(ts['A']), min(ts['A'])
        for k, v in ts.items():
            line = (f"{prog}\t{regime}\t{k:4}\tmedian {st.median(v):7.1f}\tp25-p75 {q(v, .25):7.1f}-{q(v, .75):7.1f}\tmin {min(v):7.1f}"
                    f"\tvs A: median {st.median(v) - a_med:+6.1f}  min {min(v) - a_min:+6.1f}\tn={n}")
            print(line)
            o.write(line + "\n")
        o.flush()
