# usage: time.py <dir holding the arm dirs> <program> <armA> <armB> <N> <out>
# Start-up A/B/Boff: wall time exec->exit, arms interleaved (order alternating), 3 warm-ups, N measured.
import subprocess, time, os, sys, statistics as st
S, prog, A, B, n, out = sys.argv[1], sys.argv[2], sys.argv[3], sys.argv[4], int(sys.argv[5]), sys.argv[6]
def arms(regime):
    if regime == 'r2r':
        return {'A': ([f"{S}/{A}/r2r/{prog}"], {}), 'B': ([f"{S}/{B}/r2r/{prog}"], {}), 'Boff': ([f"{S}/{B}/r2r/{prog}"], {'GO2CS_LITTABLE_OFF': '1'})}
    return {'A': (['dotnet', f"{S}/{A}/jit/{prog}.dll"], {}), 'B': (['dotnet', f"{S}/{B}/jit/{prog}.dll"], {}), 'Boff': (['dotnet', f"{S}/{B}/jit/{prog}.dll"], {'GO2CS_LITTABLE_OFF': '1'})}
with open(out, 'a') as o:
    for regime, renv in (('tiered', {}), ('tc0', {'DOTNET_TieredCompilation': '0'}), ('r2r', {})):
        a = arms(regime); ts = {k: [] for k in a}; order = list(a)
        for i in range(n + 3):
            for k in (order if i % 2 == 0 else order[::-1]):
                c, e = a[k]
                t0 = time.perf_counter(); r = subprocess.run(c, env={**os.environ, **renv, **e}, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL); t = (time.perf_counter() - t0) * 1000
                assert r.returncode == 0, (k, regime)
                if i >= 3: ts[k].append(t)
        m = {k: st.median(v) for k, v in ts.items()}; mn = {k: min(v) for k, v in ts.items()}
        line = (f"{prog}\t{regime}\tmedian A {m['A']:.1f}  Boff {m['Boff']:.1f}  B {m['B']:.1f}\tB-A {m['B']-m['A']:+.1f} (initializers Boff-A {m['Boff']-m['A']:+.1f}, table B-Boff {m['B']-m['Boff']:+.1f})"
                f"\tmin A {mn['A']:.1f} B {mn['B']:.1f} (B-A {mn['B']-mn['A']:+.1f})\tn={n}")
        print(line); o.write(line + "\n"); o.flush()
