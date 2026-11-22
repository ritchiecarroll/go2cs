import csv, os, sys
D = os.path.dirname(os.path.abspath(__file__))
# family order from COORD's D1 brief: sync, runtime-adjacent, time, context, net/http, io, fmt, then the rest
RUNTIME_ADJ = {'internal/poll','internal/synctest','testing/synctest','os/signal','unique','weak','iter','testing'}
def family(p):
    if p == 'sync' or p.startswith('sync/') or p == 'internal/sync': return 1, 'sync'
    if p == 'runtime' or p.startswith('runtime/') or p.startswith('internal/runtime/') or p in RUNTIME_ADJ: return 2, 'runtime-adj'
    if p == 'time': return 3, 'time'
    if p == 'context': return 4, 'context'
    if p == 'net/http' or p.startswith('net/http/'): return 5, 'net/http'
    if p == 'net' or p.startswith('net/'): return 8, 'net'
    if p in ('io','bufio','os','os/exec') or p.startswith('io/'): return 6, 'io'
    if p == 'fmt': return 7, 'fmt'
    return 9, 'other'
rows = list(csv.reader(open(os.path.join(D, 'census.tsv')), delimiter='\t'))
hdr, rows = rows[0], rows[1:]
out = []
for r in rows:
    p = r[0]; v = list(map(int, r[1:]))
    pk, ts = v[:8], v[8:]
    strong = sum(pk[i] for i in (0,1,2,5,6,7)) + sum(ts[i] for i in range(8))
    if strong == 0: continue
    score = sum(ts) + pk[0] + pk[1]          # test-side features + goroutines/selects the package itself starts
    f, name = family(p)
    out.append((f, -score, p, name, score))
out.sort()
for i, (f, ns, p, name, score) in enumerate(out, 1):
    print(f"{i}\t{p}\t{name}\t{score}")
