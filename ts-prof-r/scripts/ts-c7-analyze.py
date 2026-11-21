import statistics as st, re, sys
# ts-c7-analyze.py: per arm, the median over 3 rounds of: first-10-file check ms (median per file), last-50 check, the
# phase totals and work, from the phase-timed stderr files.
D = 'C:/go2cs-tmp/ts-prof/phase/'
def per(tag):
    return [(float(p[1]), float(p[2]), float(p[3])) for p in (l.split() for l in open(D + tag + '.stderr', encoding='utf-8')) if len(p) == 4 and p[0].endswith('.txt')]
def tot(tag):
    t = [l for l in open(D + tag + '.stderr', encoding='utf-8') if l.startswith('TOTAL')][0]
    return dict((k, float(v)) for k, v in re.findall(r'(\w+)_ms=([\d.]+)', t))
for arm in 'ABC':
    rows = []
    for r in (1, 2, 3):
        p = per(f'c7-{arm}-r{r}'); t = tot(f'c7-{arm}-r{r}')
        rows.append((st.median(x[2] for x in p[:10]), st.median(x[2] for x in p[51:]), st.median(x[0] for x in p[:10]), st.median(x[1] for x in p[:10]), t['parse'], t['bind'], t['check'], t['work']))
    med = [st.median(c) for c in zip(*rows)]
    print(f"{arm}: first10 check {med[0]:.1f} ms/file, last50 check {med[1]:.1f} ms/file, first10 parse {med[2]:.1f}, first10 bind {med[3]:.1f}; totals parse {med[4]:.0f} bind {med[5]:.0f} check {med[6]:.0f} work {med[7]:.0f} ms;  rounds: " + '; '.join(f"{x[0]:.1f}/{x[1]:.1f}/{x[7]:.0f}" for x in rows))
