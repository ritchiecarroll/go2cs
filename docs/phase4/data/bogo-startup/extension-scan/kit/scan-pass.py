# Pass-0 timing of golib's extension scan from the probe hook's SCAN rows (P1HOOK_SCAN=1; the hook is
# ../probe-breakdown/instrument/StartupHook.cs). Pass 0 = the scan rows, up to the first assembly scanned a
# second time, for the assemblies loaded before the first row. An assembly's time is the gap to the next row.
# usage: python3 scan-pass.py <run dir with hook-N.tsv> ...   (hook-0.tsv is the warm-up and is dropped)
import sys, glob, statistics as st, collections
for d in sys.argv[1:]:
    per = collections.defaultdict(list); spans = []
    for f in sorted(glob.glob(d + '/hook-*.tsv')):
        if f.endswith('hook-0.tsv'): continue
        L = [l.rstrip('\n').split('\t') for l in open(f)]
        scans = sorted((float(x[1]), x[3]) for x in L if x[0] == 'SCAN')
        t0 = scans[0][0]; pre = {x[3] for x in L if x[0] == 'ASM' and float(x[1]) < t0}
        seen = set(); p0 = []
        for t, a in scans:
            if a in seen: break
            seen.add(a)
            if a in pre: p0.append((t, a))
        for (t, a), (t2, _) in zip(p0, p0[1:]): per[a].append((t2 - t) / 1000)
        spans.append((p0[-1][0] - p0[0][0]) / 1000)
    print(d, 'pass 0 first-to-last row median %.1f ms over %d launches' % (st.median(spans), len(spans)))
