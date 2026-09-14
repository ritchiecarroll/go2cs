import re, io, sys

P = r"C:\Projects\go2cs\.claude\worktrees\musing-moser-d4552c\docs\ValidatedTestPackages.md"
with io.open(P, encoding='utf-8', newline='') as f:
    text = f.read()
lines = text.split('\n')

row_re = re.compile(r'^\|\s*\[`([^`]+)`\]\([^)]*\)\s*\|\s*(\d+)\s*\|\s*(\d*)\s*\|(.*)$')
rows = []
for i, ln in enumerate(lines, 1):
    m = row_re.match(ln.rstrip('\r'))
    if m:
        rows.append((i, m.group(1), int(m.group(2)), int(m.group(3)) if m.group(3).strip() else 0, m.group(4)))

print("roster rows:", len(rows))
print("sum tests:", sum(r[2] for r in rows))
print("sum disclosed:", sum(r[3] for r in rows))

lin_re = re.compile(r'·\s*linux:\s*(n/a|\d+(?:\s*\+\s*\d+)?)\s*(?:·|\||$)')
annot, na, none = [], [], []
lsum = 0
dsum = 0
for i, pkg, t, d, rest in rows:
    m = lin_re.search(rest)
    if not m:
        none.append((i, pkg, t, d))
    elif m.group(1) == 'n/a':
        na.append((i, pkg))
    else:
        parts = [int(x) for x in re.findall(r'\d+', m.group(1))]
        lsum += parts[0]
        if len(parts) > 1:
            dsum += parts[1]
        annot.append((i, pkg, parts))

print("linux annotated:", len(annot))
print("linux n/a:", len(na), [p for _, p in na])
print("no linux annotation:", len(none))
for i, pkg, t, d in none:
    print("   line %d  %-12s windows tests=%d disclosed=%d" % (i, pkg, t, d))
print("linux matched sum:", lsum, " linux disclosed sum:", dsum)
print("applicable denominator:", len(rows) - len(na))

for want in ['os/user', 'plugin', 'crypto/tls', 'math/big', 'net/netip', 'net/http', 'os/exec',
             'runtime/debug', 'syscall', 'net', 'internal/poll', 'sync/atomic', 'iter',
             'internal/godebug', 'internal/concurrent', 'net/http/httptrace']:
    for i, pkg, t, d, rest in rows:
        if pkg == want:
            m = lin_re.search(rest)
            print("  %-24s line %d  tests=%-5d disclosed=%-3d linux=%s" % (pkg, i, t, d, m.group(1) if m else 'NONE'))
for want in ['reflect', 'os', 'unique', 'runtime', 'runtime/pprof', 'runtime/trace', 'testing',
             'crypto/internal/boring/bcache']:
    print("  present in roster? %-30s %s" % (want, any(r[1] == want for r in rows)))
