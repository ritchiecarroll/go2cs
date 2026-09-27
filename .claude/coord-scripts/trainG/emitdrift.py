import os, sys
# usage: emitdrift.py <emitted root> <pristine seed root> <sentinel file> <out-written> <out-drift>
# Lists files under <emitted>/src/core newer than the sentinel (excluding *.cs.auto), and those whose CR-stripped
# bytes differ from the pristine seed (or are absent from it: NEW).
root, seed, sentinel, outw, outd = sys.argv[1:6]
t0 = os.stat(sentinel).st_mtime
written, drift = [], []
base = os.path.join(root, 'src', 'core')
for dp, dn, fn in os.walk(base):
    for n in fn:
        if n.endswith('.cs.auto'):
            continue
        p = os.path.join(dp, n)
        if os.stat(p).st_mtime <= t0:
            continue
        rel = os.path.relpath(p, root).replace(os.sep, '/')
        written.append(rel)
        q = os.path.join(seed, rel)
        if not os.path.exists(q):
            drift.append(rel + ' NEW'); continue
        a = open(p, 'rb').read().replace(b'\r', b'')
        b = open(q, 'rb').read().replace(b'\r', b'')
        if a != b:
            drift.append(rel)
written.sort(); drift.sort()
open(outw, 'w', encoding='utf-8', newline='\n').write(''.join(x + '\n' for x in written))
open(outd, 'w', encoding='utf-8', newline='\n').write(''.join(x + '\n' for x in drift))
print(len(written), len(drift))
